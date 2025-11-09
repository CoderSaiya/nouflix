import { Component, EventEmitter, Input, Output } from "@angular/core"
import { User } from "../../../models/user.model"

@Component({
  selector: "app-user-table",
  templateUrl: "./user-table.component.html",
  styleUrls: ["./user-table.component.scss"],
})
export class UserTableComponent {
  @Input() users: User[] = []
  @Input() selectedUsers: Set<string> = new Set()
  @Input() isLoading = false
  @Output() toggleSelect = new EventEmitter<string>()
  @Output() toggleSelectAll = new EventEmitter<void>()
  @Output() edit = new EventEmitter<string>()
  @Output() delete = new EventEmitter<string>()
  @Output() toggleStatus = new EventEmitter<string>()

  isAllSelectedValue(): boolean {
    return this.selectedUsers.size === this.users.length && this.users.length > 0
  }

  onToggleSelectAll(): void {
    this.toggleSelectAll.emit()
  }

  onToggleSelect(userId: string): void {
    this.toggleSelect.emit(userId)
  }

  onEdit(id: string): void {
    this.edit.emit(id)
  }

  onDelete(id: string): void {
    this.delete.emit(id)
  }

  onToggleStatus(id: string): void {
    this.toggleStatus.emit(id)
  }

  getStatusClass(isBanned: boolean): string {
    return isBanned ? "status-banned" : "status-active"
  }

  getRoleClass(role: string): string {
    return `role-${role.toLowerCase()}`
  }

  getFullName(user: User): string {
    return `${user.firstName} ${user.lastName}`
  }
}
