import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ReservationApi } from '../../../core/services/reservation-api';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorService } from '../../../core/services/http-error.service';
import { ReservationDto } from '../../../core/models/api.models';
import { StateMessage } from '../../../shared/state-message/state-message';

/**
 * Reservations list — GET /api/reservations (Client sees own, Admin sees all,
 * with owner/service/professional columns). Cancel: DELETE /api/reservations/{id}.
 */
@Component({
  selector: 'app-reservations-list',
  imports: [DatePipe, StateMessage],
  styleUrl: './reservations-list.css',
  templateUrl: './reservations-list.html',
})
export class ReservationsList {
  private readonly api = inject(ReservationApi);
  private readonly httpError = inject(HttpErrorService);

  readonly auth = inject(AuthService);

  readonly reservations = signal<ReservationDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);

  constructor() {
    this.api.getAll().subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(this.httpError.message(err));
        this.loading.set(false);
      },
    });
  }

  canCancel(reservation: ReservationDto): boolean {
    return reservation.status === 'Pending' || reservation.status === 'Confirmed';
  }

  cancel(reservation: ReservationDto): void {
    this.actionError.set(null);
    this.api.cancel(reservation.id).subscribe({
      next: () =>
        this.reservations.update((list) =>
          list.map((r) => (r.id === reservation.id ? { ...r, status: 'Cancelled' } : r)),
        ),
      error: (err) => this.actionError.set(this.httpError.message(err)),
    });
  }
}
