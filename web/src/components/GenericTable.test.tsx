import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { GenericTable } from './GenericTable';

type Fruit = { id: string; name: string };

const fruits: Fruit[] = [
  { id: '1', name: 'Apple' },
  { id: '2', name: 'Pear' },
];

const header = (
  <tr>
    <th>Name</th>
  </tr>
);

function renderTable(props: Partial<Parameters<typeof GenericTable<Fruit>>[0]> = {}) {
  return render(
    <GenericTable<Fruit>
      items={fruits}
      header={header}
      renderRow={(fruit) => <td>{fruit.name}</td>}
      getRowKey={(fruit) => fruit.id}
      {...props}
    />,
  );
}

describe('GenericTable', () => {
  it('renders the header and one row per item', () => {
    renderTable();

    expect(screen.getByRole('columnheader', { name: 'Name' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Apple' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Pear' })).toBeInTheDocument();
  });

  it('shows the default empty text when there are no items', () => {
    renderTable({ items: [] });

    expect(screen.getByText('Sin datos')).toBeInTheDocument();
  });

  it('shows a custom empty text when provided', () => {
    renderTable({ items: [], emptyText: 'No bookings' });

    expect(screen.getByText('No bookings')).toBeInTheDocument();
  });

  it('emits the clicked item', async () => {
    const onRowClick = vi.fn();
    renderTable({ onRowClick });

    await userEvent.click(screen.getByRole('cell', { name: 'Pear' }));

    expect(onRowClick).toHaveBeenCalledTimes(1);
    expect(onRowClick).toHaveBeenCalledWith(fruits[1]);
  });

  it('emits the focused item when Enter is pressed', async () => {
    const onRowClick = vi.fn();
    renderTable({ onRowClick });

    await userEvent.tab();
    await userEvent.keyboard('{Enter}');

    expect(onRowClick).toHaveBeenCalledTimes(1);
    expect(onRowClick).toHaveBeenCalledWith(fruits[0]);
  });

  it('shows a loading message instead of rows while loading', () => {
    renderTable({ loading: true });

    expect(screen.getByText('Cargando…')).toBeInTheDocument();
    expect(screen.queryByRole('cell', { name: 'Apple' })).not.toBeInTheDocument();
  });
});
