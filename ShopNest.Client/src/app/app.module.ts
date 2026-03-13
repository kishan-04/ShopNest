import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { ToastrModule } from 'ngx-toastr';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ProductsComponent } from './features/product/products/products.component';
import { ProductFormComponent } from './features/product/product-form/product-form.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { CartComponent } from './features/cart/cart.component';
import { CheckoutComponent } from './features/order/checkout/checkout.component';
import { MyOrdersComponent } from './features/order/my-orders/my-orders.component';
import { AdminOrdersComponent } from './features/order/admin-orders/admin-orders.component';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { UserOrdersComponent } from './features/order/user-orders/user-orders.component';
import { ContactComponent } from './features/contact/contact.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { NgChartsModule } from 'ng2-charts';

@NgModule({
  declarations: [
    AppComponent,
    ProductsComponent,
    ProductFormComponent,
    LoginComponent,
    RegisterComponent,
    CartComponent,
    CheckoutComponent,
    MyOrdersComponent,
    AdminOrdersComponent,
    UserOrdersComponent,
    ContactComponent,
    DashboardComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    AppRoutingModule,
    ToastrModule.forRoot({
      positionClass: 'toast-top-right',
      timeOut: 3000,
      closeButton: true,
      progressBar: true
    }),
    NgChartsModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }