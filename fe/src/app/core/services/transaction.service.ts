import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TransactionDto {
    id: string;
    userId: string;
    username: string;
    planId?: string;
    planName: string;
    amount: number;
    status: number;
    createdAt: string;
    note?: string;
}

@Injectable({
    providedIn: 'root'
})
export class TransactionService {
    private apiUrl = `${environment.apiUrl}/transaction`;

    constructor(private http: HttpClient) { }

    getTransactions(skip: number = 0, take: number = 10): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}`, { params: { skip: skip.toString(), take: take.toString() } });
    }

    refund(id: string): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/${id}/refund`, {});
    }
}
