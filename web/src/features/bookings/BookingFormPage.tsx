import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { toApiError } from '../../api/problemDetails';
import { ErrorAlert } from '../../components/ErrorAlert';
import { localInputToIso } from '../../utils/dates';
import { bookingFormFields, bookingFormSchema, type BookingFormValues } from './bookingSchema';
import { useCreateBooking } from './hooks';

export function BookingFormPage() {
  const navigate = useNavigate();
  const createBooking = useCreateBooking();
  const [formError, setFormError] = useState<unknown>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<BookingFormValues>({
    resolver: zodResolver(bookingFormSchema),
    defaultValues: { resourceId: '', customerName: '', customerEmail: '', start: '', end: '' },
  });

  const onSubmit = async (values: BookingFormValues) => {
    setFormError(null);
    try {
      const { id } = await createBooking.mutateAsync({
        resourceId: values.resourceId,
        customer: { name: values.customerName, email: values.customerEmail },
        start: localInputToIso(values.start),
        end: localInputToIso(values.end),
      });
      navigate(`/bookings/${id}`);
    } catch (error) {
      // Server errors are painted next to each input (CA6); anything not tied to a field goes on top.
      const apiError = toApiError(error);
      let mappedToField = false;
      for (const field of bookingFormFields) {
        const message = apiError.fieldErrors[field]?.[0];
        if (message) {
          setError(field, { type: 'server', message });
          mappedToField = true;
        }
      }
      if (!mappedToField || apiError.code !== 'validation_failed') {
        setFormError(apiError);
      }
    }
  };

  return (
    <section>
      <h1>Nueva reserva</h1>

      {formError !== null && <ErrorAlert error={formError} />}

      <form className="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Field label="Recurso" error={errors.resourceId?.message}>
          <input {...register('resourceId')} placeholder="room-A" aria-invalid={!!errors.resourceId} />
        </Field>
        <Field label="Nombre del cliente" error={errors.customerName?.message}>
          <input {...register('customerName')} aria-invalid={!!errors.customerName} />
        </Field>
        <Field label="Email del cliente" error={errors.customerEmail?.message}>
          <input type="email" {...register('customerEmail')} aria-invalid={!!errors.customerEmail} />
        </Field>
        <Field label="Inicio" error={errors.start?.message}>
          <input type="datetime-local" {...register('start')} aria-invalid={!!errors.start} />
        </Field>
        <Field label="Fin" error={errors.end?.message}>
          <input type="datetime-local" {...register('end')} aria-invalid={!!errors.end} />
        </Field>

        <div className="actions">
          <button type="submit" className="button" disabled={isSubmitting}>
            {isSubmitting ? 'Creando…' : 'Crear reserva'}
          </button>
          <Link to="/bookings">Cancelar</Link>
        </div>
      </form>
    </section>
  );
}

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <label className="field">
      <span>{label}</span>
      {children}
      {error && (
        <span className="field__error" role="alert">
          {error}
        </span>
      )}
    </label>
  );
}
