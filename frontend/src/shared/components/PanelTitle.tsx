import type { LucideIcon } from 'lucide-react';

export function PanelTitle({ icon: Icon, title }: { icon: LucideIcon; title: string }) {
  return (
    <div className="panel-title">
      <Icon size={19} />
      <h2>{title}</h2>
    </div>
  );
}
