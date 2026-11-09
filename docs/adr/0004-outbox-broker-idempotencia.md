# ADR 0004 — Outbox, broker e idempotencia del consumidor

## Contexto

Al confirmar o cancelar se publica un evento de integración en RabbitMQ que procesa un Worker independiente. Publicar dentro del handler sería una doble escritura (base de datos + broker): podría confirmarse la reserva sin evento o publicarse el evento de algo que no se guardó. El broker puede estar caído (CA10) y la entrega es *al menos una vez* (CA11).

## Decisión

- **Eventos de dominio → integración**: el agregado registra `BookingConfirmed`/`BookingCancelled`. `UnitOfWork` los despacha **antes** de `SaveChanges` a handlers de Application, que los traducen a `BookingConfirmedV1`/`BookingCancelledV1` (Contracts: tipos primitivos, versionados, con `EventId`, `OccurredAtUtc`, `CorrelationId`) mediante el puerto `IIntegrationEventPublisher`.
- **Transactional Outbox de MassTransit con EF Core** (`UseBusOutbox`, `UsePostgres`): `Publish` escribe en `OutboxMessage` dentro del mismo `DbContext`, así la reserva y el mensaje se confirman en **una transacción**. Un servicio en segundo plano entrega lo pendiente y reintenta.
- **MassTransit 8.4.1**: la v9 es comercial y la 8.5 depende de EF Core 9 (incompatible con el proveedor Npgsql 8).
- **Worker**: consumidores con reintentos (1 s, 5 s, 15 s) y cola `_error` como DLQ. **Idempotencia por `EventId`** (campo del contrato): comprobación previa + índice único en `notifications.Notifications`; un `23505` concurrente se trata como duplicado y el mensaje se confirma. Logs estructurados en JSON con `CorrelationId` y `EventId`.

## Alternativas

- *Publicar directamente desde el handler*: doble escritura.
- *Outbox propio*: tabla + `BackgroundService`; más control, pero hay que implementar bloqueo entre instancias, reintentos y limpieza.
- *Inbox de MassTransit*: deduplica por `MessageId`, que es interno del transporte; un reintento de publicación con otro `MessageId` pasaría como mensaje nuevo. `EventId` pertenece al contrato.
- *RabbitMQ.Client directo*: topología, reintentos y DLQ a mano.

## Consecuencias

- Con el broker caído la API sigue respondiendo 200 y los eventos se entregan al volver (verificado parando RabbitMQ). `/health` informa RabbitMQ como `Degraded`, no `Unhealthy`.
- Entrega eventual (latencia de sondeo del Outbox) y posibles duplicados, que absorbe la idempotencia del Worker.
- El despacho de eventos de dominio usa `dynamic` para resolver el handler concreto; es el único punto con resolución en tiempo de ejecución.
- Application referencia a Contracts (el handler de evento de dominio construye el evento de integración, como pide el enunciado); Contracts no depende de nada y el Worker solo de Contracts.
