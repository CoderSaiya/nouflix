import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { SubscriptionService, PlanDto } from '../../../core/services/subscription.service';

@Component({
  selector: 'app-subscription-plans',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './subscription-plans.component.html',
  styleUrls: ['./subscription-plans.component.scss']
})
export class SubscriptionPlansComponent implements OnInit {
  plans: PlanDto[] = [];
  showCreateModal = false;
  planForm: FormGroup;
  isLoading = false;
  error: string | null = null;

  selectedPlanId: string | null = null;
  isEditing = false;

  constructor(
    private subscriptionService: SubscriptionService,
    private fb: FormBuilder
  ) {
    this.planForm = this.fb.group({
      name: ['', Validators.required],
      type: [1, Validators.required],
      priceMonthly: [0, [Validators.required, Validators.min(0)]],
      priceYearly: [0, [Validators.required, Validators.min(0)]],
      description: [''],
      features: this.fb.array([])
    });
  }

  get features() {
    return this.planForm.get('features') as FormArray;
  }

  addFeature() {
    this.features.push(this.fb.control('', Validators.required));
  }

  removeFeature(index: number) {
    this.features.removeAt(index);
  }

  ngOnInit() {
    this.loadPlans();
    this.planForm.get('type')?.valueChanges.subscribe(val => {
      const type = Number(val);
      if (type === 0) {
        this.planForm.patchValue({ priceMonthly: 0, priceYearly: 0 });
        this.planForm.get('priceMonthly')?.disable();
        this.planForm.get('priceYearly')?.disable();
      } else {
        this.planForm.get('priceMonthly')?.enable();
        this.planForm.get('priceYearly')?.enable();
      }
    });
  }

  loadPlans() {
    this.isLoading = true;
    this.error = null;
    this.subscriptionService.getPlans().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.plans = res.data || [];
        } else {
          this.error = res.message || 'Không thể tải danh sách gói';
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Đã xảy ra lỗi khi tải danh sách gói';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  openCreateModal() {
    this.isEditing = false;
    this.selectedPlanId = null;
    this.planForm.reset({ type: 1, priceMonthly: 0, priceYearly: 0 });
    this.features.clear();
    this.addFeature(); // Add one empty feature by default
    this.showCreateModal = true;
  }

  openEditModal(plan: PlanDto) {
    this.isEditing = true;
    this.selectedPlanId = plan.id;
    this.planForm.patchValue({
      name: plan.name,
      type: plan.type,
      priceMonthly: plan.priceMonthly,
      priceYearly: plan.priceYearly,
      description: plan.description
    });

    this.features.clear();
    if (plan.features && plan.features.length > 0) {
      plan.features.forEach(feature => {
        this.features.push(this.fb.control(feature, Validators.required));
      });
    } else {
      this.addFeature();
    }

    this.showCreateModal = true;
  }

  submitForm() {
    if (this.planForm.valid) {
      this.isLoading = true;
      const formValue = this.planForm.getRawValue();
      formValue.type = Number(formValue.type);

      if (this.isEditing && this.selectedPlanId) {
        this.subscriptionService.updatePlan(this.selectedPlanId, formValue).subscribe({
          next: (res) => {
            if (res.isSuccess) {
              this.loadPlans();
              this.showCreateModal = false;
              alert('Cập nhật gói thành công');
            } else {
              alert(res.message || 'Cập nhật thất bại');
            }
            this.isLoading = false;
          },
          error: (err) => {
            alert('Đã xảy ra lỗi khi cập nhật');
            this.isLoading = false;
            console.error(err);
          }
        });
      } else {
        this.subscriptionService.createPlan(formValue).subscribe({
          next: (res) => {
            if (res.isSuccess) {
              this.loadPlans();
              this.showCreateModal = false;
              alert('Tạo gói thành công');
            } else {
              alert(res.message || 'Tạo gói thất bại');
            }
            this.isLoading = false;
          },
          error: (err) => {
            alert('Đã xảy ra lỗi khi tạo gói');
            this.isLoading = false;
            console.error(err);
          }
        });
      }
    }
  }

  deletePlan(plan: PlanDto) {
    if (confirm(`Bạn có chắc chắn muốn xóa gói "${plan.name}" không?`)) {
      this.isLoading = true;
      this.subscriptionService.deletePlan(plan.id).subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this.loadPlans();
            alert('Xóa gói thành công');
          } else {
            alert(res.message || 'Xóa thất bại');
          }
          this.isLoading = false;
        },
        error: (err) => {
          alert('Đã xảy ra lỗi khi xóa gói');
          this.isLoading = false;
          console.error(err);
        }
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
}
