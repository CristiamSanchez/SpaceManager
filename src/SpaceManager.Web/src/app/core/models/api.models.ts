/**
 * TypeScript models of the API contract (Reservation.Api DTOs).
 * These mirror what the backend actually returns/accepts — they are NOT the
 * .NET Domain entities. JSON is camelCase and enums arrive as strings.
 */

export type UserRole = 'Admin' | 'Client';

export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';

export type DayOfWeek =
  | 'Monday'
  | 'Tuesday'
  | 'Wednesday'
  | 'Thursday'
  | 'Friday'
  | 'Saturday'
  | 'Sunday';

/** GET /api/users/{id} and POST /api/auth/register result (UserModel). */
export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

/** POST /api/auth/login result (LoginResult). */
export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: User;
}

/** GET /api/services and /api/services/{id} (ServiceResponse). */
export interface ServiceDto {
  id: string;
  name: string;
  description: string | null;
  durationInMinutes: number;
  price: number;
  isActive: boolean;
}

/** GET /api/professionals and /api/professionals/{id} (ProfessionalResponse). */
export interface ProfessionalDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
}

/** GET /api/professionals/{professionalId}/availability (AvailabilityResponse). */
export interface AvailabilityDto {
  id: string;
  professionalId: string;
  dayOfWeek: DayOfWeek;
  /** "HH:mm:ss" */
  startTime: string;
  /** "HH:mm:ss" */
  endTime: string;
  isActive: boolean;
}

/** POST /api/professionals/{professionalId}/services body (AssignServiceRequest). */
export interface ProfessionalServiceRequest {
  serviceId: string | null;
}

/** POST /api/services body (CreateServiceRequest). */
export interface CreateServiceRequest {
  name: string;
  description: string | null;
  durationInMinutes: number;
  price: number;
}

/** PUT /api/services/{id} body (UpdateServiceRequest). */
export interface UpdateServiceRequest extends CreateServiceRequest {
  isActive: boolean;
}

/** POST /api/professionals body (CreateProfessionalRequest). */
export interface CreateProfessionalRequest {
  name: string;
  description: string | null;
}

/** PUT /api/professionals/{id} body (UpdateProfessionalRequest). */
export interface UpdateProfessionalRequest extends CreateProfessionalRequest {
  isActive: boolean;
}

/** POST /api/professionals/{professionalId}/availability body (CreateAvailabilityRequest). */
export interface CreateAvailabilityRequest {
  dayOfWeek: DayOfWeek;
  /** "HH:mm" or "HH:mm:ss" */
  startTime: string;
  /** "HH:mm" or "HH:mm:ss" */
  endTime: string;
}

/** GET /api/reservations and /api/reservations/{id} (ReservationResponse). */
export interface ReservationDto {
  id: string;
  userId: string;
  professionalId: string;
  serviceId: string;
  startAt: string;
  endAt: string;
  status: ReservationStatus;
  notes: string | null;
  createdAt: string;
}
