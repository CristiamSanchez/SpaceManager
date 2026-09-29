import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateServiceRequest, ServiceDto, UpdateServiceRequest } from '../models/api.models';

/** GET /api/services (+ Admin mutations: POST/PUT/DELETE). */
@Injectable({ providedIn: 'root' })
export class ServiceApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/services`;

  getAll(): Observable<ServiceDto[]> {
    return this.http.get<ServiceDto[]>(this.baseUrl);
  }

  getById(id: string): Observable<ServiceDto> {
    return this.http.get<ServiceDto>(`${this.baseUrl}/${id}`);
  }

  /** POST /api/services (Admin only). */
  create(request: CreateServiceRequest): Observable<ServiceDto> {
    return this.http.post<ServiceDto>(this.baseUrl, request);
  }

  /** PUT /api/services/{id} (Admin only). */
  update(id: string, request: UpdateServiceRequest): Observable<ServiceDto> {
    return this.http.put<ServiceDto>(`${this.baseUrl}/${id}`, request);
  }

  /** DELETE /api/services/{id} — soft deactivation (Admin only). */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
