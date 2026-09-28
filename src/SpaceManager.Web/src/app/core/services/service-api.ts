import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceDto } from '../models/api.models';

/** GET /api/services */
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
}
