import { Component, OnInit } from '@angular/core';
import { UserOrder, Order } from 'src/app/shared/models/order.model';
import { OrderService } from 'src/app/core/services/order.service';
import { ToastrService } from 'ngx-toastr';
import { Inject } from '@angular/core';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'app-user-orders',
  templateUrl: './user-orders.component.html',
  styleUrls: ['./user-orders.component.scss']
})

export class UserOrdersComponent implements OnInit {
  userOrders: UserOrder[] = [];
  filteredUsers: UserOrder[] = [];
  isLoading = true;
  errorMessage = '';

  // Filters
  searchTerm = '';
  expandedUserId: number | null = null;
  expandedOrderId: number | null = null;

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    @Inject(ToastrService) private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.orderService.getOrdersByUser().subscribe({
      next: (data) => {
        this.userOrders    = data;
        this.filteredUsers = data;
        this.isLoading     = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load user orders.';
        this.isLoading    = false;
      }
    });
  }

  // Toggle expand/collapse user card
  toggleUser(userId: number): void {
    this.expandedUserId  = this.expandedUserId === userId ? null : userId;
    this.expandedOrderId = null; // collapse any open order
  }

  // Toggle expand/collapse order inside user
  toggleOrder(orderId: number): void {
    this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
  }

  // Get product names as comma separated string
  getProductNames(order: Order): string {
    return order.items.map(i => i.productName).join(', ');
  }

  // Search by name or email
  onSearch(value: string): void {
    this.searchTerm   = value.toLowerCase();
    this.filteredUsers = this.userOrders.filter(user =>
      user.firstName.toLowerCase().includes(this.searchTerm) ||
      user.lastName.toLowerCase().includes(this.searchTerm)  ||
      user.email.toLowerCase().includes(this.searchTerm)
    );
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending':    return 'bg-warning text-dark';
      case 'Processing': return 'bg-info text-dark';
      case 'Shipped':    return 'bg-primary';
      case 'Delivered':  return 'bg-success';
      case 'Cancelled':  return 'bg-danger';
      default:           return 'bg-secondary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Pending':    return '🟡';
      case 'Processing': return '🔵';
      case 'Shipped':    return '🚚';
      case 'Delivered':  return '✅';
      case 'Cancelled':  return '❌';
      default:           return '⚪';
    }
  }

  deleteUser(userId: number, userName: string): void {
  if (confirm(`Are you sure you want to delete ${userName}?`)) {
    this.authService.deleteUser(userId).subscribe({
      next: () => {
        this.toastr.success(`${userName} deleted successfully!`);
        this.userOrders = this.userOrders.filter(u => u.userId !== userId);
        this.filteredUsers = this.filteredUsers.filter(u => u.userId !== userId);
      },
      error: () => this.toastr.error('Failed to delete user.')
    });
  }
}
}