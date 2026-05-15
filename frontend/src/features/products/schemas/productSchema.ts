import { z } from 'zod';

export const productSchema = z.object({
  name: z.string().min(2, 'Informe o nome.'),
  description: z.string(),
  price: z.coerce.number().positive('Preço deve ser maior que zero.'),
  categoryId: z.string().min(1, 'Selecione a categoria.'),
  isActive: z.boolean(),
  imageUrl: z.string(),
});

export type ProductFormInput = z.input<typeof productSchema>;
export type ProductFormData = z.output<typeof productSchema>;
