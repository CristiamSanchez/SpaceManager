import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReservationDto } from '../models/api.models';

/** GET /api/reservations (authenticated: Client sees own, Admin sees all). */
@Injectable({ providedIn: 'root' })
export class ReservationApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/reservations`;

  getAll(): Observable<ReservationDto[]> {
    return this.http.get<ReservationDto[]>(this.baseUrl);
  }

  getById(id: string): Observable<ReservationDto> {
    return this.http.get<ReservationDto>(`${this.baseUrl}/${id}`);
  }
}
