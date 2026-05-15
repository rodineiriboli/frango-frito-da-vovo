import { z } from 'zod';

export const categorySchema = z.object({
  name: z.string().min(2, 'Informe o nome.'),
  description: z.string(),
  isActive: z.boolean(),
});

export type CategoryFormInput = z.input<typeof categorySchema>;
export type CategoryFormData = z.output<typeof categorySchema>;
