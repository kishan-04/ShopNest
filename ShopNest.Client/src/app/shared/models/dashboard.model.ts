export interface OrderStatusCount {
  status: string;
  count: number;
}

export interface DailySales {
  day: string;
  total: number;
}

export interface RecentOrder {
  id: number;
  customerName: string;
  totalAmount: number;
  status: string;
  createdAt: string;
}

export interface Dashboard {
  totalUsers: number;
  totalOrders: number;
  totalSales: number;
  totalProducts: number;
  ordersByStatus: OrderStatusCount[];
  salesLast7Days: DailySales[];
  recentOrders: RecentOrder[];
}