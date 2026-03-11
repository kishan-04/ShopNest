import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(
    public authService: AuthService,
    public cartService: CartService,
    public themeService: ThemeService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Connect CartService to AuthService
    this.authService.setCartService(this.cartService);

    // If user is already logged in load their cart
    if (this.authService.isLoggedIn()) {
      this.cartService.loadCart();
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}