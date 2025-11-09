export function toISODateString(d: Date | string | number | undefined | null): string | null {
  if (!d) return null;
  const date = d instanceof Date ? d : new Date(d);
  if (isNaN(date.getTime())) return null;
  return date.toISOString();
}

export function formatDateLocal(d: Date | string | undefined | null): string {
  if (!d) return '';
  const date = d instanceof Date ? d : new Date(d);
  return date.toLocaleDateString();
}


export function toYearFormat(d: Date | string | undefined | null): string {
  if (!d) return '';
  const date = d instanceof Date ? d : new Date(d);
  if (isNaN(date.getTime())) return '';
  return String(date.getFullYear());
}
