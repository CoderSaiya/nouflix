import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlanDto } from '../../core/services/subscription.service';
import { CurrencyUtils } from '../../utils/currency.utils';

@Component({
  selector: 'app-payment-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-modal.component.html',
  styleUrls: ['./payment-modal.component.scss']
})
export class PaymentModalComponent {
  @Input() plan!: PlanDto;
  @Input() billingCycle: 'monthly' | 'yearly' = 'monthly';
  @Output() cancel = new EventEmitter<void>();
  @Output() pay = new EventEmitter<string>();

  selectedProvider: string | null = null;

  selectProvider(provider: string) {
    this.selectedProvider = provider;
  }

  confirm() {
    if (this.selectedProvider) {
      this.pay.emit(this.selectedProvider);
    }
  }

  formatPrice(price: number): string {
    return CurrencyUtils.formatVND(price);
  }
}
