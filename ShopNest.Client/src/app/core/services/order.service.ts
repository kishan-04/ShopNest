import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, UserOrder } from '../../shared/models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = 'https://localhost:7271/api/orders';

  constructor(private http: HttpClient) {}

  placeOrder(shippingAddress: string): Observable<any> {
    return this.http.post(this.apiUrl, { shippingAddress });
  }

  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/my`);
  }

  getAllOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  updateStatus(orderId: number, status: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${orderId}/status`, { status });
  }

  getOrdersByUser(): Observable<UserOrder[]> {
  return this.http.get<UserOrder[]>(`${this.apiUrl}/by-user`);
}
}