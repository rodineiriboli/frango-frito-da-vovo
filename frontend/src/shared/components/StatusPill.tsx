export function StatusPill({ active, activeText, inactiveText }: { active: boolean; activeText: string; inactiveText: string }) {
  return <span className={`pill ${active ? 'pill--on' : 'pill--off'}`}>{active ? activeText : inactiveText}</span>;
}
