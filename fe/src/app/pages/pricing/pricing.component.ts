import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionService, PlanDto } from '../../core/services/subscription.service';
import { PaymentModalComponent } from '../../components/payment-modal/payment-modal.component';
import { Router, ActivatedRoute } from '@angular/router';

import { CurrencyUtils } from '../../utils/currency.utils';

@Component({
  selector: 'app-pricing',
  standalone: true,
  imports: [CommonModule, PaymentModalComponent],
  templateUrl: './pricing.component.html',
  styleUrls: ['./pricing.component.scss']
})
export class PricingComponent implements OnInit {
  plans: PlanDto[] = [];
  selectedPlan: PlanDto | null = null;
  billingCycle: 'monthly' | 'yearly' = 'monthly';

  constructor(
    private subscriptionService: SubscriptionService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.loadPlans();
    this.checkPaymentStatus();
  }

  checkPaymentStatus() {
    this.route.queryParams.subscribe(params => {
      const transactionId = params['transactionId'];
      const sessionId = params['sessionId']; // For Stripe
      const resultCode = params['resultCode']; // For Momo (0 = success)

      if (transactionId) {
        // Handle Momo or Generic
        if (resultCode && resultCode !== '0') {
          alert('Thanh toán thất bại hoặc đã bị hủy.');
          return;
        }

        this.subscriptionService.activate(transactionId, sessionId || '').subscribe({
          next: (res) => {
            if (res.isSuccess) {
              alert('Thanh toán thành công! Gói cước của bạn đã được kích hoạt.');
              this.router.navigate(['/']);
            } else {
              alert('Kích hoạt thất bại: ' + res.message);
            }
          },
          error: (err) => {
            console.error(err);
            alert('Lỗi khi kích hoạt gói cước.');
          }
        });
      }
    });
  }

  loadPlans() {
    this.subscriptionService.getPlans().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.plans = res.data;
        }
      }
    });
  }

  openPayment(plan: PlanDto) {
    this.selectedPlan = plan;
  }

  onPay(provider: string) {
    if (this.selectedPlan) {
      const returnUrl = `${window.location.origin}/pricing`;
      const cancelUrl = `${window.location.origin}/pricing`;

      this.subscriptionService.subscribe(this.selectedPlan.id, provider, returnUrl, cancelUrl, this.billingCycle).subscribe({
        next: (res) => {
          if (res.isSuccess && res.data) {
            if (res.data.isActivated) {
              alert('Đăng ký thành công! Bạn đã được kích hoạt gói cước.');
              this.selectedPlan = null;
              this.router.navigate(['/']);
            } else if (res.data.paymentUrl) {
              // Redirect to payment provider
              window.location.href = res.data.paymentUrl;
            } else {
              alert('Đăng ký thất bại: Phản hồi không hợp lệ');
            }
          } else {
            alert('Đăng ký thất bại: ' + res.message);
          }
        },
        error: () => alert('Đăng ký thất bại')
      });
    }
  }

  getPlanType(type: number): string {
    switch (type) {
      case 0: return 'Miễn phí';
      case 1: return 'VIP';
      case 2: return 'SVIP';
      default: return 'Không xác định';
    }
  }

  formatPrice(price: number): string {
    return CurrencyUtils.formatVND(price);
  }
}
