import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TransactionService, TransactionDto } from '../../../core/services/transaction.service';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transactions.component.html',
  styleUrls: ['./transactions.component.scss']
})
export class TransactionsComponent implements OnInit {
  transactions: TransactionDto[] = [];
  skip = 0;
  take = 10;

  constructor(private transactionService: TransactionService) { }

  ngOnInit() {
    this.loadTransactions();
  }

  loadTransactions() {
    this.transactionService.getTransactions(this.skip, this.take).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.transactions = res.data.items;
        }
      }
    });
  }

  refund(tx: TransactionDto) {
    if (confirm('Are you sure you want to refund this transaction?')) {
      this.transactionService.refund(tx.id).subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this.loadTransactions();
          } else {
            alert('Refund failed: ' + res.message);
          }
        },
        error: () => alert('Refund failed')
      });
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Completed';
      case 2: return 'Failed';
      case 3: return 'Refunded';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 0: return 'bg-yellow-500/20 text-yellow-500';
      case 1: return 'bg-green-500/20 text-green-500';
      case 2: return 'bg-red-500/20 text-red-500';
      case 3: return 'bg-gray-500/20 text-gray-500';
      default: return 'bg-gray-500/20 text-gray-500';
    }
  }

  prevPage() {
    if (this.skip >= this.take) {
      this.skip -= this.take;
      this.loadTransactions();
    }
  }

  nextPage() {
    this.skip += this.take;
    this.loadTransactions();
  }
}
