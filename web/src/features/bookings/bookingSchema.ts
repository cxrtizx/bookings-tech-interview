import { z } from 'zod';

// Client-side rules equivalent to the server ones (CreateBookingCommandValidator + domain).
// The server stays the source of truth: its field errors are also mapped onto these fields.
export const bookingFormSchema = z
  .object({
    resourceId: z.string().trim().min(1, 'El recurso es obligatorio.').max(200, 'Máximo 200 caracteres.'),
    customerName: z.string().trim().min(1, 'El nombre es obligatorio.').max(100, 'Máximo 100 caracteres.'),
    customerEmail: z
      .string()
      .trim()
      .min(1, 'El email es obligatorio.')
      .max(100, 'Máximo 100 caracteres.')
      .email('El email no tiene un formato válido.'),
    start: z.string().min(1, 'El inicio es obligatorio.'),
    end: z.string().min(1, 'El fin es obligatorio.'),
  })
  .superRefine((values, context) => {
    const start = new Date(values.start);
    const end = new Date(values.end);

    if (values.start && start.getTime() <= Date.now()) {
      context.addIssue({ code: z.ZodIssueCode.custom, path: ['start'], message: 'El inicio debe ser una fecha futura.' });
    }

    if (values.start && values.end && end.getTime() <= start.getTime()) {
      context.addIssue({ code: z.ZodIssueCode.custom, path: ['end'], message: 'El fin debe ser posterior al inicio.' });
    }
  });

export type BookingFormValues = z.infer<typeof bookingFormSchema>;

export const bookingFormFields = ['resourceId', 'customerName', 'customerEmail', 'start', 'end'] as const;
