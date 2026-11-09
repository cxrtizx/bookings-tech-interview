import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Link, Navigate, Route, Routes } from 'react-router-dom';
import { BookingDetailsPage } from './features/bookings/BookingDetailsPage';
import { BookingFormPage } from './features/bookings/BookingFormPage';
import { BookingsListPage } from './features/bookings/BookingsListPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
    mutations: { retry: false },
  },
});

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <header className="app-header">
          <Link to="/bookings" className="app-header__brand">
            Reservas
          </Link>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<Navigate to="/bookings" replace />} />
            <Route path="/bookings" element={<BookingsListPage />} />
            <Route path="/bookings/new" element={<BookingFormPage />} />
            <Route path="/bookings/:id" element={<BookingDetailsPage />} />
            <Route path="*" element={<p className="state">Página no encontrada.</p>} />
          </Routes>
        </main>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
