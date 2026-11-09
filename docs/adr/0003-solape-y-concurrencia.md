# ADR 0003 — Modelado del solape y estrategia de concurrencia

## Contexto

No puede haber dos reservas `Confirmed` solapadas del mismo recurso. Los intervalos son semiabiertos `[Start, End)`: 10:00–11:00 y 11:00–12:00 no se solapan. Las `Pending` sí pueden solaparse. La comprobación debe evaluarse en base de datos (Specification) y dos confirmaciones simultáneas no pueden acabar ambas confirmadas.

## Decisión

**Dos niveles: la regla en el dominio y la garantía en la base de datos.**

1. **Specification** `ConfirmedOverlappingBookingSpec(resourceId, range, excludeId)`, expuesta como `Expression<Func<Booking,bool>>`: `Status == Confirmed && ResourceId == r && Id != excludeId && Start < range.End && End > range.Start`. EF la traduce a `SELECT EXISTS (... WHERE ...)`; nunca se cargan reservas en memoria. El handler obtiene el dato con `AnyAsync(spec)` y el **agregado decide**: `booking.Confirm(hasConfirmedOverlap, clock)` comprueba primero la transición (422) y después el solape (409).
2. **Restricción de exclusión en PostgreSQL** (extensión `btree_gist`):
   `EXCLUDE USING gist ("ResourceId" WITH =, tstzrange("StartUtc","EndUtc",'[)') WITH &&) WHERE ("Status" = 'Confirmed')`.
   Cierra la ventana *check-then-act*: si dos confirmaciones pasan la spec a la vez, la segunda falla al guardar con `23P01`. `UnitOfWork` traduce esa excepción (filtrando por SqlState y nombre de la restricción) al mismo `booking.overlap`, así el cliente recibe el mismo 409.

## Alternativas

- *Concurrencia optimista (`xmin`/RowVersion)*: **no resuelve el caso**; las dos confirmaciones modifican filas distintas (*write skew*). Sirve para otra cosa: dos cambios sobre la misma reserva.
- *Bloqueo por recurso* (`pg_advisory_xact_lock`): funciona, pero exige transacción explícita y serializa las confirmaciones del recurso.
- *Aislamiento Serializable*: funciona, pero obliga a reintentar ante errores `40001`.
- *Solo la restricción*: la regla dejaría de estar en el dominio y el error llegaría siempre tras intentar escribir.
- *Ardalis.Specification*: buena librería (MIT), pero modela *query objects* completos (orden, paginación, includes); aquí basta un predicado de negocio de pocas líneas y el dominio queda sin dependencias externas.

## Consecuencias

- Garantía válida con varias instancias de la API y frente a escrituras directas en la tabla.
- La expresión de la spec repite la condición de `DateRange.Overlaps` (no se puede invocar un método del dominio dentro de una expresión traducible); los tests de dominio verifican que ambas coinciden en los casos límite, incluida la adyacencia.
- Dependencia de PostgreSQL (`EXCLUDE`, `tstzrange`); otro motor necesitaría otro mecanismo, aislado en Infrastructure.
- Cancelar una reserva confirmada libera el hueco automáticamente (la restricción es parcial).
