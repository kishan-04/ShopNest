import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Cart } from '../../shared/models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = 'https://localhost:7271/api/cart';

  // BehaviorSubject to track cart state across all components
  private cartSubject = new BehaviorSubject<Cart | null>(null);
  cart$ = this.cartSubject.asObservable();

  constructor(private http: HttpClient) {}

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  addToCart(productId: number, quantity: number = 1): Observable<any> {
    return this.http.post(this.apiUrl, { productId, quantity }).pipe(
      tap(() => this.getCart().subscribe())
    );
  }

  updateItem(productId: number, quantity: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${productId}?quantity=${quantity}`, {}).pipe(
      tap(() => this.getCart().subscribe())
    );
  }

  removeItem(productId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${productId}`).pipe(
      tap(() => this.getCart().subscribe())
    );
  }

  clearCart(): Observable<any> {
    return this.http.delete(this.apiUrl).pipe(
      tap(() => this.cartSubject.next(null))
    );
  }

  // Call this when user logs in
  loadCart(): void {
    this.getCart().subscribe();
  }

  // Call this when user logs out
  resetCart(): void {
    this.cartSubject.next(null);
  }

  getTotalItems(): number {
    return this.cartSubject.value?.totalItems || 0;
  }
}