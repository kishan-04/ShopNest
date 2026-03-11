import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { OrderService } from 'src/app/core/services/order.service';
import { CartService } from 'src/app/core/services/cart.service';
import { Cart } from 'src/app/shared/models/cart.model';
import { ToastrService } from 'ngx-toastr';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss'],
})
export class CheckoutComponent implements OnInit {
  checkoutForm: FormGroup;
  cart: Cart | null = null;
  isLoading = false;
  isCartLoading = true;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private cartService: CartService,
    private router: Router,
    @Inject(ToastrService) private toastr: ToastrService
  ) {
    this.checkoutForm = this.fb.group({
      shippingAddress: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  ngOnInit(): void {
    this.cartService.getCart().subscribe({
      next: (cart) => {
        this.cart         = cart;
        this.isCartLoading = false;

        // If cart is empty go back to products
        if (!cart || cart.items.length === 0) {
          this.router.navigate(['/products']);
        }
      },
      error: () => {
        this.isCartLoading = false;
        this.router.navigate(['/cart']);
      }
    });
  }

  onSubmit(): void {
  if (this.checkoutForm.invalid) return;

  this.isLoading    = true;
  this.errorMessage = '';

  this.orderService.placeOrder(this.checkoutForm.value.shippingAddress).subscribe({
    next: () => {
      this.toastr.success('Order placed successfully! 🎉');
      this.cartService.resetCart();
      this.router.navigate(['/my-orders']);
    },
    error: (err) => {
      this.errorMessage = err.error?.message || 'Failed to place order.';
      this.toastr.error(this.errorMessage);
      this.isLoading    = false;
    }
  });
}

  isFieldInvalid(field: string): boolean {
    const control = this.checkoutForm.get(field);
    return !!(control && control.invalid && control.touched);
  }
}