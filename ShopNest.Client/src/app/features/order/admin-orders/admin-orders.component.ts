import { Component, Inject, OnInit } from '@angular/core';
import { Order } from 'src/app/shared/models/order.model';
import { OrderService } from 'src/app/core/services/order.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-orders',
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.scss']
})
export class AdminOrdersComponent implements OnInit {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  isLoading = true;
  errorMessage = '';

  // Filters
  searchTerm = '';
  selectedStatus = 'All';
  expandedOrderId: number | null = null;

  statuses = ['All', 'Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

  constructor(
    private orderService: OrderService,
    @Inject(ToastrService) private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.getAllOrders().subscribe({
      next: (data) => {
        this.orders         = data;
        this.filteredOrders = data;
        this.isLoading      = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load orders.';
        this.isLoading    = false;
      }
    });
  }

  toggleOrder(orderId: number): void {
    this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
  }

  getProductNames(order: Order): string {
    return order.items.map(i => i.productName).join(', ');
  }

  applyFilters(): void {
    this.filteredOrders = this.orders.filter(order => {
      const statusMatch = this.selectedStatus === 'All' ||
        order.status === this.selectedStatus;

      const search = this.searchTerm.toLowerCase();
      const searchMatch = search === '' ||
        order.items.some(item =>
          item.productName.toLowerCase().includes(search) ||
          item.categoryName.toLowerCase().includes(search)
        );

      return statusMatch && searchMatch;
    });
  }

  onSearch(value: string): void {
    this.searchTerm = value;
    this.applyFilters();
  }

  onStatusChange(status: string): void {
    this.selectedStatus = status;
    this.applyFilters();
  }

  getStatusCount(status: string): number {
    if (status === 'All') return this.orders.length;
    return this.orders.filter(o => o.status === status).length;
  }

  updateStatus(orderId: number, status: string): void {
    this.orderService.updateStatus(orderId, status).subscribe({
      next: () => {
        this.toastr.success(`Order #${orderId} updated to ${status}!`);
        this.loadOrders();
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to update status.')
    });
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
}