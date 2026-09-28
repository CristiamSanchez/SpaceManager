import { Component, inject, signal } from '@angular/core';
import { ProfessionalApi } from '../../../core/services/professional-api';
import { HttpErrorService } from '../../../core/services/http-error.service';
import { ProfessionalDto } from '../../../core/models/api.models';
import { StateMessage } from '../../../shared/state-message/state-message';

/** Public list of professionals — GET /api/professionals. */
@Component({
  selector: 'app-professionals-list',
  imports: [StateMessage],
  styleUrl: './professionals-list.css',
  templateUrl: './professionals-list.html',
})
export class ProfessionalsList {
  private readonly api = inject(ProfessionalApi);
  private readonly httpError = inject(HttpErrorService);

  readonly professionals = signal<ProfessionalDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

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
}
