import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceApi } from '../../../core/services/service-api';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorService } from '../../../core/services/http-error.service';
import { ServiceDto } from '../../../core/models/api.models';
import { StateMessage } from '../../../shared/state-message/state-message';

/**
 * Services list — public read; Admin can create, edit and deactivate
 * (POST/PUT /api/services, DELETE = soft deactivation).
 */
@Component({
  selector: 'app-services-list',
  imports: [DecimalPipe, ReactiveFormsModule, StateMessage],
  styleUrl: './services-list.css',
  templateUrl: './services-list.html',
})
export class ServicesList {
  private readonly api = inject(ServiceApi);
  private readonly httpError = inject(HttpErrorService);
  private readonly fb = inject(FormBuilder);

  readonly auth = inject(AuthService);

  readonly services = signal<ServiceDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  /** Action errors (create/update) shown next to the form, not the whole list. */
  readonly actionError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly creating = signal(false);
  readonly editingId = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    durationInMinutes: [30, [Validators.required, Validators.min(1)]],
    price: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() {
    this.api.getAll().subscribe({
      next: (services) => {
        this.services.set(services);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(this.httpError.message(err));
        this.loading.set(false);
      },
    });
  }

  startCreate(): void {
    this.creating.set(true);
    this.editingId.set(null);
    this.actionError.set(null);
    this.form.reset({ name: '', description: '', durationInMinutes: 30, price: 0 });
  }

  startEdit(service: ServiceDto): void {
    this.editingId.set(service.id);
    this.creating.set(false);
    this.actionError.set(null);
    this.form.setValue({
      name: service.name,
      description: service.description ?? '',
      durationInMinutes: service.durationInMinutes,
      price: service.price,
    });
  }

  cancelForm(): void {
    this.creating.set(false);
    this.editingId.set(null);
    this.actionError.set(null);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = {
      name: value.name.trim(),
      description: value.description.trim() || null,
      durationInMinutes: value.durationInMinutes,
      price: value.price,
    };

    this.saving.set(true);
    this.actionError.set(null);

    const editingId = this.editingId();
    if (editingId !== null) {
      const current = this.services().find((s) => s.id === editingId);
      this.api.update(editingId, { ...request, isActive: current?.isActive ?? true }).subscribe({
        next: (updated) => {
          this.services.update((list) => list.map((s) => (s.id === updated.id ? updated : s)));
          this.afterSave();
        },
        error: (err) => this.failSave(err),
      });
    } else {
      this.api.create(request).subscribe({
        next: (created) => {
          this.services.update((list) =>
            [...list, created].sort((a, b) => a.name.localeCompare(b.name)),
          );
          this.afterSave();
        },
        error: (err) => this.failSave(err),
      });
    }
  }

  /** Soft deactivate/activate toggle (PUT keeps every other field). */
  toggleActive(service: ServiceDto): void {
    this.actionError.set(null);
    this.api
      .update(service.id, {
        name: service.name,
        description: service.description,
        durationInMinutes: service.durationInMinutes,
        price: service.price,
        isActive: !service.isActive,
      })
      .subscribe({
        next: (updated) =>
          this.services.update((list) => list.map((s) => (s.id === updated.id ? updated : s))),
        error: (err) => this.actionError.set(this.httpError.message(err)),
      });
  }

  private afterSave(): void {
    this.saving.set(false);
    this.cancelForm();
  }

  private failSave(err: unknown): void {
    this.saving.set(false);
    this.actionError.set(this.httpError.message(err));
  }
}
