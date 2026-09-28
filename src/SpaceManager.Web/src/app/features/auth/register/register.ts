import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorService } from '../../../core/services/http-error.service';
import { StateMessage } from '../../../shared/state-message/state-message';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, StateMessage],
  styleUrl: './register.css',
  templateUrl: './register.html',
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly httpError = inject(HttpErrorService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  // Client-side rules mirror the backend (name/email/password required).
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.httpError.message(err));
      },
    });
  }
}
