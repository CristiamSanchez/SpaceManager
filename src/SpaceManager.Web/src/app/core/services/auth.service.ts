import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/api.models';

const TOKEN_KEY = 'sm.token';
const USER_KEY = 'sm.user';

/** Current JWT or null (used by the auth interceptor). */
export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function readStoredUser(): User | null {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as User;
  } catch {
    return null;
  }
}

/**
 * Session state: login/register against the API, and the JWT + minimal user
 * info in localStorage. The password is never stored.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  /** Restored from localStorage on startup. */
  readonly user = signal<User | null>(readStoredUser());
  readonly isLoggedIn = computed(() => this.user() !== null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, request)
      .pipe(
        tap((response) => {
          localStorage.setItem(TOKEN_KEY, response.accessToken);
          localStorage.setItem(USER_KEY, JSON.stringify(response.user));
          this.user.set(response.user);
        }),
      );
  }

  register(request: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${environment.apiUrl}/api/auth/register`, request);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.user.set(null);
    this.router.navigate(['/login']);
  }
}
