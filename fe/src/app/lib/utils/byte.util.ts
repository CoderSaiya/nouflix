export function formatBytes(bytes: number) {
  if (!bytes || bytes <= 0) return '0 B';
  const units = ['B','KB','MB','GB','TB'];
  let i = 0;
  let v = bytes;
  while (v >= 1024 && i < units.length-1) { v = v/1024; i++; }
  return `${Math.round(v*10)/10} ${units[i]}`;
}
