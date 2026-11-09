# ADR 0005 — Diseño de la tabla genérica en React

## Contexto

El listado necesita una tabla reutilizable, que no conozca el dominio de reservas, con filas activables por teclado (Enter) y tests propios (CA7). El enunciado propone una interfaz de props concreta.

## Decisión

`GenericTable<T>` es un **componente funcional genérico** con las props del enunciado: `items`, `header`, `renderRow`, `getRowKey`, `onRowClick?`, `emptyText = 'Sin datos'`, `loading?`.

- **Inversión de control del renderizado**: la tabla solo gestiona la estructura (`thead`/`tbody`), los estados (cargando, vacío) y la interacción; **cómo se pinta una fila lo decide quien la usa** con `renderRow`. Por eso no sabe nada de reservas.
- **Accesibilidad e interacción**: si hay `onRowClick`, cada fila es enfocable (`tabIndex=0`) y responde a click y Enter emitiendo el item; sin `onRowClick` las filas no son interactivas.
- `getRowKey` da a React una clave estable por fila (el id), no el índice.
- Tests con Vitest + Testing Library desde el punto de vista del usuario (roles y teclado): cabecera y filas, texto vacío por defecto y personalizado, click y Enter emiten el item, estado de carga.

## Alternativas

- *Tabla configurada por columnas* (`columns: [{ key, header, render }]`): muy cómoda para tablas homogéneas, pero añade una API propia y limita filas con estructura libre. `renderRow` es más simple y flexible para este alcance.
- *Librería (TanStack Table, AG Grid)*: ordenación, filtrado y virtualización listos, pero la ordenación y la paginación son de servidor aquí y el ejercicio evalúa el diseño del componente.
- *Navegación con `<a>` en cada celda*: accesible sin JavaScript, pero repetiría el enlace en cada celda y acoplaría la tabla a la navegación.

## Consecuencias

- El llamador escribe las celdas (`<td>`) en `renderRow`; la tabla no garantiza que coincidan con las columnas de `header`.
- Para tablas grandes habría que añadir virtualización.
- El comportamiento de teclado se limita a Enter sobre la fila enfocada; la navegación con flechas quedaría como mejora.
