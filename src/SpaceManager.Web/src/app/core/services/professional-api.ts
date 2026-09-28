import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AvailabilityDto, ProfessionalDto, ServiceDto } from '../models/api.models';

/** GET /api/professionals (and its availability/services sub-resources). */
@Injectable({ providedIn: 'root' })
export class ProfessionalApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/professionals`;

  getAll(): Observable<ProfessionalDto[]> {
    return this.http.get<ProfessionalDto[]>(this.baseUrl);
  }

  getById(id: string): Observable<ProfessionalDto> {
    return this.http.get<ProfessionalDto>(`${this.baseUrl}/${id}`);
  }

  /** GET /api/professionals/{professionalId}/availability (anonymous). */
  getAvailability(professionalId: string): Observable<AvailabilityDto[]> {
    return this.http.get<AvailabilityDto[]>(`${this.baseUrl}/${professionalId}/availability`);
  }

  /** GET /api/professionals/{professionalId}/services (Admin only). */
  getServices(professionalId: string): Observable<ServiceDto[]> {
    return this.http.get<ServiceDto[]>(`${this.baseUrl}/${professionalId}/services`);
  }
}
