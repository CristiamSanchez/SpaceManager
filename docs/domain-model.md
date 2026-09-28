# Domain Model — Sistema de Reservas

## 1. Objetivo

Este documento define las entidades principales del dominio y sus relaciones.

El objetivo es establecer el modelo de negocio antes de implementar código.

El modelo debe mantenerse simple y evitar entidades o relaciones que no sean necesarias para las funcionalidades actuales del sistema.

---

# 2. Entidades principales

El sistema estará compuesto inicialmente por las siguientes entidades:

1. `User`
2. `Service`
3. `Professional`
4. `ProfessionalService`
5. `Availability`
6. `Reservation`
7. `ReservationStatus`

---

# 3. User

Representa a una persona que utiliza el sistema.

Un usuario puede ser:

* Cliente
* Administrador

## Propiedades

```text
User
├── Id
├── Name
├── Email
├── PasswordHash
├── Role
├── IsActive
├── CreatedAt
└── UpdatedAt
```

## Reglas

* `Email` debe ser único.
* `PasswordHash` nunca debe almacenar la contraseña en texto plano.
* Un usuario debe tener un rol.
* Un usuario puede estar activo o inactivo.
* Un usuario puede realizar múltiples reservas.
* Un usuario inactivo no debe poder realizar nuevas reservas.

---

# 4. Service

Representa un servicio que puede ser reservado.

Ejemplos:

* Corte de cabello
* Consulta médica
* Limpieza
* Mantenimiento
* Masaje

## Propiedades

```text
Service
├── Id
├── Name
├── Description
├── DurationInMinutes
├── Price
├── IsActive
├── CreatedAt
└── UpdatedAt
```

## Reglas

* `Name` es obligatorio.
* `DurationInMinutes` debe ser mayor que cero.
* `Price` no puede ser negativo.
* Un servicio puede estar asociado a múltiples profesionales.
* Un servicio inactivo no debe poder utilizarse para nuevas reservas.

---

# 5. Professional

Representa a la persona que proporciona uno o más servicios.

Ejemplos:

* Barbero
* Médico
* Técnico
* Estilista

## Propiedades

```text
Professional
├── Id
├── Name
├── Description
├── IsActive
├── CreatedAt
└── UpdatedAt
```

## Reglas

* Un profesional puede ofrecer uno o varios servicios.
* Un profesional puede tener uno o varios horarios de disponibilidad.
* Un profesional puede tener múltiples reservas.
* Un profesional inactivo no debe aceptar nuevas reservas.

---

# 6. ProfessionalService

Representa la relación entre un profesional y un servicio.

Esta entidad existe porque la relación es de muchos a muchos.

Un profesional puede ofrecer muchos servicios.

Un servicio puede ser ofrecido por muchos profesionales.

## Relación

```text
Professional 1 ───────< ProfessionalService >─────── 1 Service
```

## Propiedades

```text
ProfessionalService
├── ProfessionalId
└── ServiceId
```

## Reglas

* La combinación `ProfessionalId + ServiceId` debe ser única.
* No debe existir la misma asociación dos veces.
* El profesional debe existir.
* El servicio debe existir.

---

# 7. Availability

Representa los horarios en los que un profesional está disponible para recibir reservas.

## Propiedades

```text
Availability
├── Id
├── ProfessionalId
├── DayOfWeek
├── StartTime
├── EndTime
└── IsActive
```

## Ejemplo

Un profesional puede tener:

```text
Monday
09:00 - 13:00

Monday
14:00 - 18:00

Tuesday
09:00 - 13:00
```

## Reglas

* `StartTime` debe ser menor que `EndTime`.
* El profesional debe existir.
* Un profesional puede tener múltiples períodos de disponibilidad.
* No deben existir períodos de disponibilidad que se solapen para el mismo profesional.
* Una disponibilidad inactiva no debe utilizarse para generar nuevas reservas.

---

# 8. Reservation

Representa una reserva realizada por un usuario.

Es la entidad central del sistema.

## Propiedades

```text
Reservation
├── Id
├── UserId
├── ProfessionalId
├── ServiceId
├── StartAt
├── EndAt
├── Status
├── Notes
├── CreatedAt
└── UpdatedAt
```

## Relaciones

Una reserva pertenece a:

* Un usuario.
* Un profesional.
* Un servicio.

Por lo tanto:

```text
User 1 ─────────< Reservation

Professional 1 ─────────< Reservation

Service 1 ─────────< Reservation
```

## Reglas

### Regla 1 — Usuario

El usuario debe existir y estar activo.

### Regla 2 — Profesional

El profesional debe existir y estar activo.

### Regla 3 — Servicio

El servicio debe existir y estar activo.

### Regla 4 — Asociación

El profesional debe ofrecer el servicio seleccionado.

Es decir:

```text
ProfessionalService
```

debe existir para la combinación seleccionada.

### Regla 5 — Duración

`EndAt` debe calcularse utilizando la duración del servicio.

Ejemplo:

```text
Service duration = 30 minutos

StartAt = 10:00
EndAt   = 10:30
```

### Regla 6 — Disponibilidad

La reserva debe encontrarse dentro del horario disponible del profesional.

### Regla 7 — Conflictos

No se permite crear una reserva que se solape con otra reserva activa del mismo profesional.

Ejemplo:

```text
Reserva existente:
10:00 - 10:30

Nueva reserva:
10:15 - 10:45

Resultado:
RECHAZADA
```

Pero:

```text
Reserva existente:
10:00 - 10:30

Nueva reserva:
10:30 - 11:00

Resultado:
PERMITIDA
```

### Regla 8 — Estado

Una reserva debe tener un estado.

Estados iniciales:

```text
Pending
Confirmed
Cancelled
Completed
```

---

# 9. ReservationStatus

Representa el estado actual de una reserva.

Inicialmente se manejará como enum del dominio.

```text
ReservationStatus
├── Pending
├── Confirmed
├── Cancelled
└── Completed
```

## Reglas

### Pending

La reserva fue creada pero todavía no está confirmada.

### Confirmed

La reserva fue confirmada.

### Cancelled

La reserva fue cancelada.

### Completed

El servicio ya fue realizado.

---

# 10. Relaciones completas

La estructura general del dominio será:

```text
                         ┌──────────────┐
                         │     User     │
                         └──────┬───────┘
                                │
                                │ 1:N
                                │
                         ┌──────▼───────┐
                         │ Reservation  │
                         └───┬─────┬────┘
                             │     │
                         N:1 │     │ N:1
                             │     │
                 ┌───────────▼┐   ┌▼─────────────┐
                 │ Professional│   │   Service    │
                 └──────┬─────┘   └──────┬───────┘
                        │                 │
                        │                 │
                        │    N:M          │
                        └───────┬─────────┘
                                │
                       ┌────────▼─────────┐
                       │ProfessionalService│
                       └──────────────────┘

                 ┌─────────────────┐
                 │  Professional   │
                 └────────┬────────┘
                          │ 1:N
                          │
                   ┌──────▼───────┐
                   │ Availability │
                   └──────────────┘
```

---

# 11. Resumen de relaciones

| Entidad             | Relación | Entidad      | Descripción                                      |
| ------------------- | -------- | ------------ | ------------------------------------------------ |
| User                | 1:N      | Reservation  | Un usuario puede tener múltiples reservas        |
| Professional        | 1:N      | Reservation  | Un profesional puede tener múltiples reservas    |
| Service             | 1:N      | Reservation  | Un servicio puede aparecer en múltiples reservas |
| Professional        | N:M      | Service      | Un profesional puede ofrecer múltiples servicios |
| Professional        | 1:N      | Availability | Un profesional puede tener múltiples horarios    |
| ProfessionalService | N:1      | Professional | Cada asociación pertenece a un profesional       |
| ProfessionalService | N:1      | Service      | Cada asociación pertenece a un servicio          |

---

# 12. Reglas de negocio principales

Las reglas más importantes del sistema son:

```text
1. Un usuario debe estar autenticado para crear una reserva.

2. Un usuario debe estar activo.

3. El profesional debe estar activo.

4. El servicio debe estar activo.

5. El profesional debe ofrecer el servicio seleccionado.

6. La fecha de la reserva debe ser válida.

7. La hora de inicio debe estar dentro de la disponibilidad del profesional.

8. La hora de finalización se determina mediante la duración del servicio.

9. No pueden existir reservas activas solapadas para el mismo profesional.

10. Una reserva cancelada no debe bloquear el horario.

11. Una reserva completada no puede modificarse como una reserva pendiente.

12. Un profesional puede ofrecer múltiples servicios.

13. Un servicio puede ser ofrecido por múltiples profesionales.

14. Un profesional puede tener múltiples períodos de disponibilidad.

15. Los períodos de disponibilidad de un mismo profesional no deben solaparse.
```

---

# 13. Reglas que NO pertenecen a las entidades

No debemos colocar toda la lógica del sistema dentro de las entidades.

Algunas reglas pertenecen a casos de uso o servicios de dominio.

Por ejemplo:

```text
"¿Existe una reserva que se solape con esta nueva reserva?"
```

requiere consultar otras reservas.

Por lo tanto, no debe resolverse únicamente dentro de `Reservation`.

De igual manera:

```text
"¿El profesional ofrece este servicio?"
```

requiere verificar la relación `ProfessionalService`.

Estas reglas deberán implementarse posteriormente en `Application` o mediante servicios de dominio cuando sea necesario.

---

# 14. Decisiones iniciales

## Identificadores

Las entidades utilizarán identificadores propios.

La implementación concreta del tipo de `Id` se decidirá durante la implementación de `Domain`.

No se debe asumir todavía si serán:

* `Guid`
* `int`
* otro tipo

hasta definir la estrategia general de persistencia.

---

## Fechas

Las fechas y horas de las reservas deben manejarse de forma consistente.

La implementación concreta de zona horaria se definirá antes de implementar `Reservation`.

No se deben introducir conversiones de zona horaria arbitrarias dentro de las entidades.

---

## Dinero

`Service.Price` representa el precio del servicio.

La implementación debe utilizar un tipo apropiado para valores monetarios y evitar `float` o `double`.

---

# 15. Fuera del alcance inicial

No implementar inicialmente:

* Pagos en línea
* Facturación
* Notificaciones por SMS
* Integración con WhatsApp
* Google Calendar
* Múltiples sucursales
* Cupones
* Descuentos
* Historial avanzado
* Sistema de reseñas
* Multi-tenancy
* Suscripciones
* Reportes avanzados

Estas funcionalidades podrán considerarse en versiones posteriores.

---

# 16. Objetivo del modelo

El modelo debe ser suficientemente completo para practicar:

* Relaciones 1:N
* Relaciones N:M
* Reglas de negocio
* CRUD
* Autenticación
* Autorización
* Validaciones
* Consultas con EF Core
* Transacciones
* Manejo de concurrencia
* Tests

Pero debe mantenerse suficientemente pequeño para que el proyecto pueda ser entendido completamente por un desarrollador que está aprendiendo Clean Architecture.

