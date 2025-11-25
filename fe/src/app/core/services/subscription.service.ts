import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PlanDto {
    id: string;
    name: string;
    type: number;
    priceMonthly: number;
    priceYearly: number;
    description?: string;
    features?: string[];
}

export interface SubscriptionRes {
    id: string;
    planName: string;
    planType: number;
    startDate: string;
    endDate: string;
    status: number;
}

import { GlobalResponse } from '../../models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class SubscriptionService {
    private apiUrl = `${environment.apiUrl}/api/subscription`;

    constructor(private http: HttpClient) { }

    getPlans(): Observable<GlobalResponse<PlanDto[]>> {
        return this.http.get<GlobalResponse<PlanDto[]>>(`${this.apiUrl}/plans`);
    }

    getMySubscription(): Observable<GlobalResponse<SubscriptionRes>> {
        return this.http.get<GlobalResponse<SubscriptionRes>>(`${this.apiUrl}/me`);
    }

    subscribe(planId: string, paymentProvider: string, returnUrl: string, cancelUrl: string, durationType: string): Observable<GlobalResponse<any>> {
        return this.http.post<GlobalResponse<any>>(`${this.apiUrl}/subscribe`, { planId, paymentProvider, returnUrl, cancelUrl, durationType });
    }

    activate(transactionId: string, sessionId: string): Observable<GlobalResponse<any>> {
        return this.http.post<GlobalResponse<any>>(`${this.apiUrl}/activate`, { transactionId, sessionId });
    }

    createPlan(plan: any): Observable<GlobalResponse<PlanDto>> {
        return this.http.post<GlobalResponse<PlanDto>>(`${this.apiUrl}/plans`, plan);
    }

    updatePlan(id: string, plan: any): Observable<GlobalResponse<any>> {
        return this.http.put<GlobalResponse<any>>(`${this.apiUrl}/plans/${id}`, plan);
    }

    deletePlan(id: string): Observable<GlobalResponse<any>> {
        return this.http.delete<GlobalResponse<any>>(`${this.apiUrl}/plans/${id}`);
    }
}
