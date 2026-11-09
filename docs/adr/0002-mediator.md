# ADR 0002 — Elección del mediator

## Contexto

CQRS requiere despachar commands y queries a un handler por caso de uso, con behaviors transversales (logging y validación). El enunciado permite MediatR, Mediator (source generator) o una implementación propia, pidiendo justificar licencia, rendimiento y simplicidad.

## Decisión

**Mediator (martinothamar) 3.0.2**, con `ServiceLifetime.Scoped` y los behaviors `LoggingBehavior` → `ValidationBehavior`.

- **Licencia**: MIT. MediatR pasó en 2025 a licencia comercial para su versión actual.
- **Rendimiento**: el source generator crea en compilación el código de despacho; no hay reflexión ni búsqueda de handlers en tiempo de ejecución.
- **Errores en compilación**: un comando sin handler, o con un tipo de respuesta distinto al del handler, no compila (lo vimos con `ICommand<Guid>` frente a `ICommandHandler<TCommand>`).
- La API es casi idéntica a MediatR (`ICommand`, `IQuery`, `IPipelineBehavior`), así que el coste de aprendizaje es bajo.

## Alternativas

- *MediatR*: el estándar de facto, pero con coste de licencia para versiones actuales y despacho por reflexión.
- *Implementación propia*: sin dependencias, pero hay que mantener el registro de handlers y la tubería de behaviors; no aporta valor de negocio.
- *Sin mediator* (servicios de aplicación llamados desde los endpoints): menos piezas, pero los aspectos transversales se repetirían en cada caso de uso.

## Consecuencias

- Los handlers deben ser `public` para que el generador (que se ejecuta en el proyecto Api) los vea.
- `Unit` como respuesta de los comandos sin datos permite una única tubería genérica para todos los mensajes.
- Los behaviors se aplican por restricción genérica: `ValidationBehavior` usa `IMessage` para cubrir también las queries.
