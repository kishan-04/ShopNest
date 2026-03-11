import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Cart } from '../../shared/models/cart.model';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss'],
})
export class CartComponent implements OnInit {
  cart: Cart | null = null;
  isLoading = true;
  errorMessage = '';

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router,
    @Inject(ToastrService) private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadCart();
  }

  loadCart(): void {
    this.isLoading = true;
    this.cartService.getCart().subscribe({
      next: (cart) => {
        this.cart = cart;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load cart.';
        this.isLoading = false;
      },
    });
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity < 1) return;
    this.cartService.updateItem(productId, quantity).subscribe({
      next: () => this.loadCart(),
      error: (err) => alert(err.error?.message || 'Failed to update.'),
    });
  }

  removeItem(productId: number): void {
    this.cartService.removeItem(productId).subscribe({
      next: () => {
        this.toastr.success('Item removed from cart.');
        this.loadCart();
      },
      error: () => this.toastr.error('Failed to remove item.'),
    });
  }

  clearCart(): void {
    if (confirm('Are you sure you want to clear the cart?')) {
      this.cartService.clearCart().subscribe({
        next: () => {
          this.toastr.success('Cart cleared.');
          this.loadCart();
        },
        error: () => this.toastr.error('Failed to clear cart.'),
      });
    }
  }
}
