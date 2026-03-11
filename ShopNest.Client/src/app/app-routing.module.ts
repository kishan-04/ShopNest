import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
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

const routes: Routes = [
  { path: '',                  redirectTo: 'products', pathMatch: 'full' },
  { path: 'products',          component: ProductsComponent },
  { path: 'login',             component: LoginComponent },
  { path: 'register',          component: RegisterComponent },

  // Protected routes — must be logged in
  { path: 'products/new',      component: ProductFormComponent,  canActivate: [AuthGuard] },
  { path: 'products/edit/:id', component: ProductFormComponent,  canActivate: [AuthGuard] },
  { path: 'cart',              component: CartComponent,         canActivate: [AuthGuard] },
  { path: 'checkout',          component: CheckoutComponent,     canActivate: [AuthGuard] },
  { path: 'my-orders',         component: MyOrdersComponent,     canActivate: [AuthGuard] },
  { path: 'contact', component: ContactComponent,                canActivate: [AuthGuard] },

  // Admin only routes
  { path: 'admin-orders',      component: AdminOrdersComponent,  canActivate: [AdminGuard] },
  { path: 'user-orders', component: UserOrdersComponent,canActivate: [AdminGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }