import { HttpInterceptorFn } from '@angular/common/http';
import { getToken } from '../services/auth.service';

/**
 * Adds `Authorization: Bearer <jwt>` to outgoing API requests when a session
 * token exists. Registered once in app.config.ts — components never set the
 * header themselves.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = getToken();
  if (!token) {
    return next(req);
  }
  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};
