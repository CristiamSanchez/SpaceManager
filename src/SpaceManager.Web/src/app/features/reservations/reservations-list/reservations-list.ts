import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ReservationApi } from '../../../core/services/reservation-api';
import { HttpErrorService } from '../../../core/services/http-error.service';
import { ReservationDto } from '../../../core/models/api.models';
import { StateMessage } from '../../../shared/state-message/state-message';

/** Protected list of the current user's reservations — GET /api/reservations. */
@Component({
  selector: 'app-reservations-list',
  imports: [DatePipe, StateMessage],
  styleUrl: './reservations-list.css',
  templateUrl: './reservations-list.html',
})
export class ReservationsList {
  private readonly api = inject(ReservationApi);
  private readonly httpError = inject(HttpErrorService);

  readonly reservations = signal<ReservationDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

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
}
