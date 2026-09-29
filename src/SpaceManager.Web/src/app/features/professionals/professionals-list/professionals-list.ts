import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfessionalApi } from '../../../core/services/professional-api';
import { ServiceApi } from '../../../core/services/service-api';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorService } from '../../../core/services/http-error.service';
import {
  AvailabilityDto,
  DayOfWeek,
  ProfessionalDto,
  ServiceDto,
} from '../../../core/models/api.models';
import { StateMessage } from '../../../shared/state-message/state-message';

const DAYS: DayOfWeek[] = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
];

/**
 * Professionals list — public read; Admin can create, edit and deactivate,
 * and manage each professional's assigned services and availability
 * (POST/PUT/DELETE on the professionals, services and availability endpoints).
 */
@Component({
  selector: 'app-professionals-list',
  imports: [DecimalPipe, ReactiveFormsModule, StateMessage],
  styleUrl: './professionals-list.css',
  templateUrl: './professionals-list.html',
})
export class ProfessionalsList {
  private readonly api = inject(ProfessionalApi);
  private readonly serviceApi = inject(ServiceApi);
  private readonly httpError = inject(HttpErrorService);
  private readonly fb = inject(FormBuilder);

  readonly auth = inject(AuthService);
  readonly days = DAYS;

  readonly professionals = signal<ProfessionalDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly creating = signal(false);
  readonly editingId = signal<string | null>(null);

  /** Open management panel (admin) or null. */
  readonly managed = signal<ProfessionalDto | null>(null);
  readonly manageError = signal<string | null>(null);
  readonly servicesLoading = signal(false);
  readonly availabilityLoading = signal(false);
  readonly assignedServices = signal<ServiceDto[]>([]);
  readonly availability = signal<AvailabilityDto[]>([]);
  readonly allServices = signal<ServiceDto[]>([]);
  readonly addingAvailability = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
  });

  readonly availabilityForm = this.fb.nonNullable.group({
    dayOfWeek: ['Monday', Validators.required],
    startTime: ['09:00', Validators.required],
    endTime: ['17:00', Validators.required],
  });

  constructor() {
    this.api.getAll().subscribe({
      next: (professionals) => {
        this.professionals.set(professionals);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(this.httpError.message(err));
        this.loading.set(false);
      },
    });
  }

  // --- CRUD -----------------------------------------------------------------

  startCreate(): void {
    this.creating.set(true);
    this.editingId.set(null);
    this.actionError.set(null);
    this.form.reset({ name: '', description: '' });
  }

  startEdit(professional: ProfessionalDto): void {
    this.editingId.set(professional.id);
    this.creating.set(false);
    this.actionError.set(null);
    this.form.setValue({ name: professional.name, description: professional.description ?? '' });
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
    };

    this.saving.set(true);
    this.actionError.set(null);

    const editingId = this.editingId();
    if (editingId !== null) {
      const current = this.professionals().find((p) => p.id === editingId);
      this.api.update(editingId, { ...request, isActive: current?.isActive ?? true }).subscribe({
        next: (updated) => {
          this.professionals.update((list) => list.map((p) => (p.id === updated.id ? updated : p)));
          this.managed.update((m) => (m && m.id === updated.id ? updated : m));
          this.afterSave();
        },
        error: (err) => this.failSave(err),
      });
    } else {
      this.api.create(request).subscribe({
        next: (created) => {
          this.professionals.update((list) =>
            [...list, created].sort((a, b) => a.name.localeCompare(b.name)),
          );
          this.afterSave();
        },
        error: (err) => this.failSave(err),
      });
    }
  }

  toggleActive(professional: ProfessionalDto): void {
    this.actionError.set(null);
    this.api
      .update(professional.id, {
        name: professional.name,
        description: professional.description,
        isActive: !professional.isActive,
      })
      .subscribe({
        next: (updated) =>
          this.professionals.update((list) => list.map((p) => (p.id === updated.id ? updated : p))),
        error: (err) => this.actionError.set(this.httpError.message(err)),
      });
  }

  // --- Management panel -----------------------------------------------------

  toggleManage(professional: ProfessionalDto): void {
    if (this.managed()?.id === professional.id) {
      this.managed.set(null);
      return;
    }

    this.managed.set(professional);
    this.manageError.set(null);
    this.assignedServices.set([]);
    this.availability.set([]);
    this.servicesLoading.set(true);
    this.availabilityLoading.set(true);

    this.api.getServices(professional.id).subscribe({
      next: (assigned) => {
        this.assignedServices.set(assigned);
        this.servicesLoading.set(false);
      },
      error: (err) => {
        this.manageError.set(this.httpError.message(err));
        this.servicesLoading.set(false);
      },
    });

    this.api.getAvailability(professional.id).subscribe({
      next: (periods) => {
        this.availability.set(periods);
        this.availabilityLoading.set(false);
      },
      error: (err) => {
        this.manageError.set(this.httpError.message(err));
        this.availabilityLoading.set(false);
      },
    });

    if (this.allServices().length === 0) {
      this.serviceApi.getAll().subscribe({
        next: (services) => this.allServices.set(services),
        error: (err) => this.manageError.set(this.httpError.message(err)),
      });
    }
  }

  isAssigned(serviceId: string): boolean {
    return this.assignedServices().some((s) => s.id === serviceId);
  }

  toggleService(service: ServiceDto): void {
    const professional = this.managed();
    if (!professional) return;

    this.manageError.set(null);
    if (this.isAssigned(service.id)) {
      this.api.removeService(professional.id, service.id).subscribe({
        next: () =>
          this.assignedServices.update((list) => list.filter((s) => s.id !== service.id)),
        error: (err) => this.manageError.set(this.httpError.message(err)),
      });
    } else {
      this.api.assignService(professional.id, service.id).subscribe({
        next: (created) =>
          this.assignedServices.update((list) =>
            [...list, created].sort((a, b) => a.name.localeCompare(b.name)),
          ),
        error: (err) => this.manageError.set(this.httpError.message(err)),
      });
    }
  }

  addAvailability(): void {
    const professional = this.managed();
    if (!professional) return;

    const value = this.availabilityForm.getRawValue();
    // <input type=time> yields "HH:mm"; the API accepts "HH:mm:ss" too.
    const normalize = (time: string) => (time.length === 5 ? `${time}:00` : time);

    this.addingAvailability.set(true);
    this.manageError.set(null);

    this.api
      .createAvailability(professional.id, {
        dayOfWeek: value.dayOfWeek as DayOfWeek,
        startTime: normalize(value.startTime),
        endTime: normalize(value.endTime),
      })
      .subscribe({
        next: (created) => {
          this.availability.update((list) =>
            [...list, created].sort(
              (a, b) => DAYS.indexOf(a.dayOfWeek) - DAYS.indexOf(b.dayOfWeek) ||
                a.startTime.localeCompare(b.startTime),
            ),
          );
          this.addingAvailability.set(false);
        },
        error: (err) => {
          this.addingAvailability.set(false);
          this.manageError.set(this.httpError.message(err));
        },
      });
  }

  removeAvailability(period: AvailabilityDto): void {
    const professional = this.managed();
    if (!professional) return;

    this.manageError.set(null);
    this.api.removeAvailability(professional.id, period.id).subscribe({
      next: () =>
        this.availability.update((list) => list.filter((a) => a.id !== period.id)),
      error: (err) => this.manageError.set(this.httpError.message(err)),
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
