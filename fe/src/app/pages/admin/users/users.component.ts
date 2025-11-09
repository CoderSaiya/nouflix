import {Component, inject, type OnInit} from "@angular/core"
import { Router } from "@angular/router"
import {User} from '../../../models/user.model';
import {UserService} from '../../../core/services/user.service';
import { UserTableComponent } from "../../../components/admin/user-table/user-table.component";

@Component({
  selector: "app-users-list",
  templateUrl: "./users.component.html",
  styleUrls: ["./users.component.scss"],
  imports: [UserTableComponent],
})
export class UsersListComponent implements OnInit {
  private userSvc = inject(UserService)
  private router = inject(Router)

  users: User[] = []
  isLoading = true
  selectedUsers: Set<string> = new Set()
  isDeleteConfirmOpen = false
  userToDelete: string | null = null
  currentPage = 1
  itemsPerPage = 10

  ngOnInit(): void {
    this.loadUsers()
  }

  loadUsers(): void {
    this.isLoading = true
    this.userSvc.getUsers().subscribe({
      next: (data) => {
        this.users = data
        this.isLoading = false
      },
      error: () => {
        this.isLoading = false
      },
    })
  }

  toggleSelectUser(userId: string): void {
    if (this.selectedUsers.has(userId)) {
      this.selectedUsers.delete(userId)
    } else {
      this.selectedUsers.add(userId)
    }
  }

  toggleSelectAll(): void {
    if (this.selectedUsers.size === this.users.length) {
      this.selectedUsers.clear()
    } else {
      this.users.forEach((u) => this.selectedUsers.add(u.userId))
    }
  }

  isAllSelected(): boolean {
    return this.selectedUsers.size === this.users.length && this.users.length > 0
  }

  editUser(id: string): void {
    this.router.navigate(["/admin/users/edit", id])
  }

  openDeleteConfirm(id: string): void {
    this.userToDelete = id
    this.isDeleteConfirmOpen = true
  }

  confirmDelete(): void {
    if (this.userToDelete) {
      this.users = this.users.filter((u) => u.userId !== this.userToDelete)
      this.selectedUsers.delete(this.userToDelete)
      this.isDeleteConfirmOpen = false
      this.userToDelete = null
    }
  }

  cancelDelete(): void {
    this.isDeleteConfirmOpen = false
    this.userToDelete = null
  }

  deleteSelected(): void {
    this.users = this.users.filter((u) => !this.selectedUsers.has(u.userId))
    this.selectedUsers.clear()
  }

  createUser(): void {
    this.router.navigate(["/admin/users/create"])
  }

  toggleUserStatus(userId: string): void {
    const user = this.users.find((u) => u.userId === userId)
    if (user) {
      user.isBanned = !user.isBanned
    }
  }

  get paginatedUsers(): User[] {
    const start = (this.currentPage - 1) * this.itemsPerPage
    const end = start + this.itemsPerPage
    return this.users.slice(start, end)
  }

  get totalPages(): number {
    return Math.ceil(this.users.length / this.itemsPerPage)
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++
    }
  }
}
