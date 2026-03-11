export interface OrderItem {
  productId: number;
  productName: string;
  categoryName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface Order {
  id: number;
  totalAmount: number;
  status: string;
  shippingAddress: string;
  createdAt: string;
  items: OrderItem[];
}

export interface UserOrder {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  totalOrders: number;
  totalSpent: number;
  orders: Order[];
}