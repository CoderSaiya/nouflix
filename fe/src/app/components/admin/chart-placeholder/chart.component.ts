import { Component, inject, Input } from "@angular/core"
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: "app-chart",
  templateUrl: "./chart.component.html",
  styleUrls: ["./chart.component.scss"],
})
export class ChartComponent {
  @Input() type: "line" | "pie" | "bar" = "line"
  @Input() labels: string[] = []
  @Input() values: number[] = []
  @Input() title: string = ''

  private sanitizer = inject(DomSanitizer)

  // Palette màu đẹp cho biểu đồ
  private colors = [
    '#3b82f6', // blue
    '#8b5cf6', // purple
    '#ec4899', // pink
    '#f59e0b', // amber
    '#10b981', // green
    '#6366f1', // indigo
  ];

  get svgHtml(): SafeHtml {
    const content = this.svgContent || '<text x="200" y="100" fill="#6b7280" font-size="14" text-anchor="middle">Không có dữ liệu</text>';
    const svg = `<svg viewBox="0 0 400 ${this.type === 'pie' ? '250' : '280'}" xmlns="http://www.w3.org/2000/svg" role="img" aria-label="Chart">
      ${content}
    </svg>`;
    return this.sanitizer.bypassSecurityTrustHtml(svg);
  }

  get svgContent(): string {
    if (!this.values || this.values.length === 0) return '';
    if (this.type === 'pie') return this.renderPie();
    if (this.type === 'bar') return this.renderBar();
    return this.renderLine();
  }

  private normalize(values: number[]) {
    const max = Math.max(...values, 1);
    return values.map(v => v / max);
  }

  private renderBar(): string {
    const w = 360;
    const h = 180;
    const padding = 40;
    const chartW = w - padding * 2;
    const chartH = h - 20;

    const barW = Math.max(20, Math.floor(chartW / (this.values.length * 1.8)));
    const gap = Math.max(8, barW * 0.4);
    const norm = this.normalize(this.values);

    let parts = [`<defs>
      <linearGradient id="barGrad" x1="0%" y1="0%" x2="0%" y2="100%">
        <stop offset="0%" style="stop-color:#3b82f6;stop-opacity:1" />
        <stop offset="100%" style="stop-color:#1e40af;stop-opacity:0.8" />
      </linearGradient>
    </defs>`];

    // Grid lines
    for (let i = 0; i <= 4; i++) {
      const y = padding + (chartH / 4) * i;
      parts.push(`<line x1="${padding}" y1="${y}" x2="${w - padding}" y2="${y}" stroke="#374151" stroke-width="0.5" opacity="0.3"/>`);
      parts.push(`<text x="${padding - 8}" y="${y + 4}" font-size="10" fill="#9ca3af" text-anchor="end">${Math.round((4 - i) * 25)}%</text>`);
    }

    // Bars
    norm.forEach((n, i) => {
      const x = padding + i * (barW + gap) + gap;
      const barH = Math.round(n * chartH);
      const y = padding + chartH - barH;
      const color = this.colors[i % this.colors.length];

      parts.push(`
        <rect x="${x}" y="${y}" width="${barW}" height="${barH}" fill="${color}" rx="4" opacity="0.85">
          <animate attributeName="height" from="0" to="${barH}" dur="0.6s" fill="freeze"/>
          <animate attributeName="y" from="${padding + chartH}" to="${y}" dur="0.6s" fill="freeze"/>
        </rect>
        <text x="${x + barW/2}" y="${padding + chartH + 18}" font-size="11" text-anchor="middle" fill="#d1d5db">${this.labels[i] ?? ''}</text>
        <text x="${x + barW/2}" y="${y - 6}" font-size="10" text-anchor="middle" fill="#f3f4f6" font-weight="600">${this.values[i]}</text>
      `);
    });

    return parts.join('\n');
  }

  private renderLine(): string {
    const w = 360;
    const h = 180;
    const padding = 40;
    const chartW = w - padding * 2;
    const chartH = h - 20;

    const step = chartW / Math.max(1, this.values.length - 1);
    const norm = this.normalize(this.values);

    let parts = [`<defs>
      <linearGradient id="lineGrad" x1="0%" y1="0%" x2="100%" y2="0%">
        <stop offset="0%" style="stop-color:#3b82f6;stop-opacity:1" />
        <stop offset="100%" style="stop-color:#8b5cf6;stop-opacity:1" />
      </linearGradient>
      <filter id="glow">
        <feGaussianBlur stdDeviation="2" result="coloredBlur"/>
        <feMerge>
          <feMergeNode in="coloredBlur"/>
          <feMergeNode in="SourceGraphic"/>
        </feMerge>
      </filter>
    </defs>`];

    // Grid
    for (let i = 0; i <= 4; i++) {
      const y = padding + (chartH / 4) * i;
      parts.push(`<line x1="${padding}" y1="${y}" x2="${w - padding}" y2="${y}" stroke="#374151" stroke-width="0.5" opacity="0.3"/>`);
      parts.push(`<text x="${padding - 8}" y="${y + 4}" font-size="10" fill="#9ca3af" text-anchor="end">${Math.round((4 - i) * 25)}%</text>`);
    }

    // Area fill
    const areaPoints = norm.map((n, i) =>
      `${padding + Math.round(i * step)},${padding + Math.round(chartH - n * chartH)}`
    ).join(' ');
    const areaPath = `M ${padding},${padding + chartH} L ${areaPoints} L ${padding + chartW},${padding + chartH} Z`;
    parts.push(`<path d="${areaPath}" fill="url(#lineGrad)" opacity="0.2"/>`);

    // Line
    const linePoints = norm.map((n, i) =>
      `${padding + Math.round(i * step)},${padding + Math.round(chartH - n * chartH)}`
    ).join(' ');
    parts.push(`<polyline points="${linePoints}" fill="none" stroke="url(#lineGrad)" stroke-width="3" filter="url(#glow)"/>`);

    // Points
    norm.forEach((n, i) => {
      const x = padding + Math.round(i * step);
      const y = padding + Math.round(chartH - n * chartH);
      parts.push(`
        <circle cx="${x}" cy="${y}" r="5" fill="#ffffff" stroke="${this.colors[i % this.colors.length]}" stroke-width="2">
          <animate attributeName="r" from="0" to="5" dur="0.4s" begin="${i * 0.1}s" fill="freeze"/>
        </circle>
        <text x="${x}" y="${padding + chartH + 18}" font-size="11" text-anchor="middle" fill="#d1d5db">${this.labels[i] ?? ''}</text>
      `);
    });

    return parts.join('\n');
  }

  private renderPie(): string {
    const cx = 200, cy = 120, r = 80;
    const total = this.values.reduce((s, v) => s + (v || 0), 0) || 1;
    let angle = -Math.PI / 2;
    const parts: string[] = [];

    parts.push(`<defs>
      <filter id="shadow">
        <feDropShadow dx="0" dy="2" stdDeviation="3" flood-opacity="0.3"/>
      </filter>
    </defs>`);

    this.values.forEach((v, i) => {
      const value = v || 0;
      const frac = value / total;
      const theta = frac * Math.PI * 2;
      const x1 = cx + r * Math.cos(angle);
      const y1 = cy + r * Math.sin(angle);
      angle += theta;
      const x2 = cx + r * Math.cos(angle);
      const y2 = cy + r * Math.sin(angle);
      const large = theta > Math.PI ? 1 : 0;
      const color = this.colors[i % this.colors.length];

      // Label position
      const labelAngle = angle - theta / 2;
      const labelR = r + 30;
      const labelX = cx + labelR * Math.cos(labelAngle);
      const labelY = cy + labelR * Math.sin(labelAngle);

      parts.push(`
        <path d="M ${cx} ${cy} L ${x1} ${y1} A ${r} ${r} 0 ${large} 1 ${x2} ${y2} z"
              fill="${color}"
              opacity="0.9"
              filter="url(#shadow)"
              class="pie-slice">
          <animate attributeName="opacity" from="0" to="0.9" dur="0.5s" begin="${i * 0.1}s" fill="freeze"/>
        </path>
        <text x="${labelX}" y="${labelY}" font-size="11" fill="#f3f4f6" text-anchor="middle" font-weight="500">
          ${this.labels[i] ?? ''}
        </text>
        <text x="${labelX}" y="${labelY + 14}" font-size="10" fill="#9ca3af" text-anchor="middle">
          ${Math.round(frac * 100)}%
        </text>
      `);
    });

    // Center circle for donut effect
    parts.push(`<circle cx="${cx}" cy="${cy}" r="${r * 0.5}" fill="#1f2937"/>`);
    parts.push(`<text x="${cx}" y="${cy + 5}" font-size="14" fill="#f3f4f6" text-anchor="middle" font-weight="600">${total}</text>`);
    parts.push(`<text x="${cx}" y="${cy + 20}" font-size="10" fill="#9ca3af" text-anchor="middle">Total</text>`);

    return parts.join('\n');
  }
}
