import { z } from 'zod';

export const customerSchema = z.object({
  name: z.string().min(2, 'Informe o nome.'),
  phone: z.string().min(8, 'Informe o telefone.'),
  address: z.object({
    street: z.string().min(2, 'Informe a rua.'),
    number: z.string().min(1, 'Informe o número.'),
    neighborhood: z.string().min(2, 'Informe o bairro.'),
    city: z.string().min(2, 'Informe a cidade.'),
    state: z.string().min(2, 'UF').max(2, 'UF'),
    zipCode: z.string().min(8, 'Informe o CEP.'),
    complement: z.string(),
  }),
});

export type CustomerFormInput = z.input<typeof customerSchema>;
export type CustomerFormData = z.output<typeof customerSchema>;
