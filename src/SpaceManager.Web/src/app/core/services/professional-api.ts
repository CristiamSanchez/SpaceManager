import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AvailabilityDto,
  CreateAvailabilityRequest,
  CreateProfessionalRequest,
  ProfessionalDto,
  ServiceDto,
  UpdateProfessionalRequest,
} from '../models/api.models';

/**
 * GET /api/professionals and its sub-resources (availability, services),
 * plus Admin mutations: POST/PUT/DELETE and availability/assignment management.
 */
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

  /** POST /api/professionals (Admin only). */
  create(request: CreateProfessionalRequest): Observable<ProfessionalDto> {
    return this.http.post<ProfessionalDto>(this.baseUrl, request);
  }

  /** PUT /api/professionals/{id} (Admin only). */
  update(id: string, request: UpdateProfessionalRequest): Observable<ProfessionalDto> {
    return this.http.put<ProfessionalDto>(`${this.baseUrl}/${id}`, request);
  }

  /** DELETE /api/professionals/{id} — soft deactivation (Admin only). */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /** GET /api/professionals/{professionalId}/availability (anonymous). */
  getAvailability(professionalId: string): Observable<AvailabilityDto[]> {
    return this.http.get<AvailabilityDto[]>(`${this.baseUrl}/${professionalId}/availability`);
  }

  /** POST /api/professionals/{professionalId}/availability (Admin only). */
  createAvailability(professionalId: string, request: CreateAvailabilityRequest): Observable<AvailabilityDto> {
    return this.http.post<AvailabilityDto>(`${this.baseUrl}/${professionalId}/availability`, request);
  }

  /** DELETE /api/professionals/{professionalId}/availability/{id} — soft deactivation (Admin only). */
  removeAvailability(professionalId: string, availabilityId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${professionalId}/availability/${availabilityId}`);
  }

  /** GET /api/professionals/{professionalId}/services (Admin only). */
  getServices(professionalId: string): Observable<ServiceDto[]> {
    return this.http.get<ServiceDto[]>(`${this.baseUrl}/${professionalId}/services`);
  }

  /** POST /api/professionals/{professionalId}/services (Admin only). */
  assignService(professionalId: string, serviceId: string): Observable<ServiceDto> {
    return this.http.post<ServiceDto>(`${this.baseUrl}/${professionalId}/services`, { serviceId });
  }

  /** DELETE /api/professionals/{professionalId}/services/{serviceId} (Admin only). */
  removeService(professionalId: string, serviceId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${professionalId}/services/${serviceId}`);
  }
}
