import type { KeyboardEvent, ReactNode } from 'react';

// Generic and domain-agnostic: knows nothing about bookings. The caller decides how a row looks.
export type GenericTableProps<T> = {
  items: T[];
  header: ReactNode;
  renderRow: (item: T) => ReactNode;
  onRowClick?: (item: T) => void;
  getRowKey: (item: T) => string;
  emptyText?: string;
  loading?: boolean;
};

export function GenericTable<T>({
  items,
  header,
  renderRow,
  onRowClick,
  getRowKey,
  emptyText = 'Sin datos',
  loading = false,
}: GenericTableProps<T>) {
  const interactive = onRowClick !== undefined;

  const handleKeyDown = (event: KeyboardEvent<HTMLTableRowElement>, item: T) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      onRowClick?.(item);
    }
  };

  return (
    <table className="table" aria-busy={loading}>
      <thead>{header}</thead>
      <tbody>
        {loading ? (
          <tr>
            <td className="table__message" colSpan={100}>
              Cargando…
            </td>
          </tr>
        ) : items.length === 0 ? (
          <tr>
            <td className="table__message" colSpan={100}>
              {emptyText}
            </td>
          </tr>
        ) : (
          items.map((item) => (
            <tr
              key={getRowKey(item)}
              className={interactive ? 'table__row--interactive' : undefined}
              tabIndex={interactive ? 0 : undefined}
              onClick={interactive ? () => onRowClick(item) : undefined}
              onKeyDown={interactive ? (event) => handleKeyDown(event, item) : undefined}
            >
              {renderRow(item)}
            </tr>
          ))
        )}
      </tbody>
    </table>
  );
}
