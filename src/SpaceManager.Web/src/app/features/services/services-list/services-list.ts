import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ServiceApi } from '../../../core/services/service-api';
import { HttpErrorService } from '../../../core/services/http-error.service';
import { ServiceDto } from '../../../core/models/api.models';
import { StateMessage } from '../../../shared/state-message/state-message';

/** Public list of services — GET /api/services. */
@Component({
  selector: 'app-services-list',
  imports: [DecimalPipe, StateMessage],
  styleUrl: './services-list.css',
  templateUrl: './services-list.html',
})
export class ServicesList {
  private readonly api = inject(ServiceApi);
  private readonly httpError = inject(HttpErrorService);

  readonly services = signal<ServiceDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

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
}
