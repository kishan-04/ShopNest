import { Injectable, NgModule } from '@angular/core';
import { CanActivate, Router, RouterModule, Routes } from '@angular/router';
import { ProductsComponent } from './features/product/products/products.component';
import { ProductFormComponent } from './features/product/product-form/product-form.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { CartComponent } from './features/cart/cart.component';
import { CheckoutComponent } from './features/order/checkout/checkout.component';
import { MyOrdersComponent } from './features/order/my-orders/my-orders.component';
import { AdminOrdersComponent } from './features/order/admin-orders/admin-orders.component';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { UserOrdersComponent } from './features/order/user-orders/user-orders.component';
import { ContactComponent } from './features/contact/contact.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AuthService } from './core/services/auth.service';


@Injectable({ providedIn: 'root' })
export class HomeGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.authService.isAdmin()) {
      this.router.navigate(['/dashboard']);
    } else {
      this.router.navigate(['/products']);
    }
    return false;
  }
}

const routes: Routes = [
  { path: '',                  component: ProductsComponent },
  { path: 'products',          component: ProductsComponent },
  { path: 'login',             component: LoginComponent },
  { path: 'register',          component: RegisterComponent },

  // Protected routes — must be logged in
  { path: 'cart',              component: CartComponent,         canActivate: [AuthGuard] },
  { path: 'checkout',          component: CheckoutComponent,     canActivate: [AuthGuard] },
  { path: 'my-orders',         component: MyOrdersComponent,     canActivate: [AuthGuard] },
  { path: 'contact', component: ContactComponent,                canActivate: [AuthGuard] },

  // Admin only routes
  { path: 'products/new',      component: ProductFormComponent,  canActivate: [AdminGuard] },
  { path: 'products/edit/:id', component: ProductFormComponent,  canActivate: [AdminGuard] },
  { path: 'admin-orders',      component: AdminOrdersComponent,  canActivate: [AdminGuard] },
  { path: 'user-orders',       component: UserOrdersComponent,   canActivate: [AdminGuard] },
  { path: 'dashboard',         component: DashboardComponent,    canActivate: [AdminGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }5