import { Component, OnInit } from '@angular/core';
import { Order } from 'src/app/shared/models/order.model';
import { OrderService } from 'src/app/core/services/order.service';

@Component({
  selector: 'app-my-orders',
  templateUrl: './my-orders.component.html',
  styleUrls: ['./my-orders.component.scss']
})
export class MyOrdersComponent implements OnInit {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  isLoading = true;
  errorMessage = '';

  // Filters
  searchTerm = '';
  selectedStatus = 'All';
  expandedOrderId: number | null = null;

  statuses = ['All', 'Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe({
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

  // Toggle expand/collapse order card
  toggleOrder(orderId: number): void {
    this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
  }

  // Get product names as comma separated string
  getProductNames(order: Order): string {
    return order.items.map(i => i.productName).join(', ');
  }

  // Filter orders by search and status
  applyFilters(): void {
    this.filteredOrders = this.orders.filter(order => {

      // Status filter
      const statusMatch = this.selectedStatus === 'All' ||
        order.status === this.selectedStatus;

      // Search filter — search in product names and category names
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

  // Count orders by status
  getStatusCount(status: string): number {
    if (status === 'All') return this.orders.length;
    return this.orders.filter(o => o.status === status).length;
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