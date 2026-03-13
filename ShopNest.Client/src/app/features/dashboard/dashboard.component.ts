import { Component, OnInit, Inject } from '@angular/core';
import { Dashboard } from '../../shared/models/dashboard.model';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  dashboard: Dashboard | null = null;
  isLoading = true;
  errorMessage = '';

  // Chart data
  salesChartData: any;
  salesChartOptions: any;
  statusChartData: any;
  statusChartOptions: any;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.buildSalesChart(data);
        this.buildStatusChart(data);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load dashboard.';
        this.isLoading = false;
      }
    });
  }

  buildSalesChart(data: Dashboard): void {
    this.salesChartData = {
      labels: data.salesLast7Days.map(d => d.day),
      datasets: [{
        label: 'Sales ($)',
        data: data.salesLast7Days.map(d => d.total),
        backgroundColor: 'rgba(52, 152, 219, 0.2)',
        borderColor: 'rgba(52, 152, 219, 1)',
        borderWidth: 2,
        fill: true,
        tension: 0.4
      }]
    };

    this.salesChartOptions = {
      responsive: true,
      plugins: {
        legend: { display: false }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: (value: number) => '$' + value
          }
        }
      }
    };
  }

  buildStatusChart(data: Dashboard): void {
    const colors = [
      '#f39c12', '#3498db', '#2980b9',
      '#27ae60', '#e74c3c', '#95a5a6'
    ];

    this.statusChartData = {
      labels: data.ordersByStatus.map(s => s.status),
      datasets: [{
        data: data.ordersByStatus.map(s => s.count),
        backgroundColor: colors.slice(0, data.ordersByStatus.length),
        borderWidth: 2
      }]
    };

    this.statusChartOptions = {
      responsive: true,
      plugins: {
        legend: {
          position: 'bottom'
        }
      }
    };
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