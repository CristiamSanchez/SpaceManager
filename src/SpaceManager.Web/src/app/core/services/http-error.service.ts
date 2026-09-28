import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

/**
 * Turns HTTP failures into a short, user-friendly message.
 * Backend error bodies are a plain string ("Email is already registered."),
 * an object with { error: "..." }, or empty (401/403) — details never leak
 * stack traces or SQL, so it is safe to show them.
 */
@Injectable({ providedIn: 'root' })
export class HttpErrorService {
  /**
   * @param unauthorizedMessage shown for HTTP 401 (empty body); callers can
   *   override it contextually (e.g. the login form says "invalid credentials").
   */
  message(err: unknown, unauthorizedMessage = 'You need to sign in to do that.'): string {
    if (err instanceof HttpErrorResponse) {
      const body = err.error;
      if (typeof body === 'string' && body.trim().length > 0) {
        return body;
      }
      if (body && typeof body === 'object' && typeof (body as { error?: unknown }).error === 'string') {
        return (body as { error: string }).error;
      }
      if (err.status === 0) {
        return 'Cannot reach the server. Is the API running?';
      }
      if (err.status === 401) {
        return unauthorizedMessage;
      }
      if (err.status === 403) {
        return 'You do not have permission to do that.';
      }
      return `Request failed (HTTP ${err.status}).`;
    }
    return 'Unexpected error.';
  }
}
