import type { LucideIcon } from 'lucide-react';

export function Metric({ icon: Icon, label, value, tone }: { icon: LucideIcon; label: string; value: string | number; tone?: 'red' | 'yellow' }) {
  return (
    <article className={`metric metric--${tone ?? 'neutral'}`}>
      <Icon size={22} />
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}
