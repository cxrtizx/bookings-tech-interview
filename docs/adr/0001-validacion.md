# ADR 0001 — Validación: FluentValidation y dónde se valida cada cosa

## Contexto

Las reglas aparecen en tres sitios: el formulario de la SPA, la entrada de la API y el dominio. Hay que decidir quién es la fuente de verdad, cómo evitar contradicciones y qué contrato de error recibe el cliente (el formulario debe pintar los errores junto a cada campo, CA6).

## Decisión

- **Dominio (fuente de verdad)**: los Value Objects y el agregado se autovalidan y lanzan `DomainException` con código estable (`booking.start_in_past`, `booking.invalid_date_range`…). Cualquier camino que llegue al dominio (API, tests, un futuro consumidor) respeta las invariantes.
- **Aplicación (forma de la entrada)**: FluentValidation sobre commands **y queries**, ejecutado por `ValidationBehavior` en el pipeline del mediator. Responde 400 con `errors` por campo en camelCase y mensajes en inglés. Los validadores reutilizan las constantes del dominio (`ResourceId.MaxLength`, `Booking.CancellationReasonMaxLength`) y miden lo mismo (p. ej. el motivo recortado).
- **No** se validan en FluentValidation reglas que dependen del estado (existencia, solape): las decide el handler/agregado con datos de la base de datos.
- **SPA**: Zod con reglas equivalentes para dar feedback inmediato; los errores del servidor se mapean a los mismos campos.

## Alternativas

- *Solo validación de dominio*: un único lugar, pero el cliente recibiría un error por petición en vez de todos los campos a la vez.
- *Solo FluentValidation*: el agregado podría quedar inválido entrando por otro camino.
- *Data Annotations*: menos expresivas y acopladas a los DTOs HTTP.
- *Validadores asíncronos que consulten la BD*: mezclan forma con negocio, duplican consultas y abren una carrera entre validar y escribir.

## Consecuencias

- Reglas duplicadas en tres capas, mitigado reutilizando constantes y con tests de los límites.
- Contrato de error homogéneo en toda la API (`validation_failed`, `errors`, `code`, `correlationId`).
- Ampliar `ValidationBehavior` a queries rechaza `pageSize=5000` o `from > to` con el mismo formato que un POST.
