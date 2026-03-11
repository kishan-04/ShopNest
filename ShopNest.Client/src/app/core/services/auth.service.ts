import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl    = 'https://localhost:7271/api/auth';
  private tokenKey  = 'shopnest_token';
  private userKey   = 'shopnest_user';

  // BehaviorSubject keeps track of login state
  // Any component can subscribe to know if user is logged in
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  isLoggedIn$ = this.isLoggedInSubject.asObservable();

  // Inject CartService lazily to avoid circular dependency
  private cartServiceRef: any = null;

  constructor(private http: HttpClient) {}

  setCartService(cartService: any): void {
    this.cartServiceRef = cartService;
  }

  register(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(response => {
        this.saveSession(response);
        this.cartServiceRef?.loadCart();
      })
    );
  }

  login(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => {
        this.saveSession(response);
        this.cartServiceRef?.loadCart();
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.isLoggedInSubject.next(false);
    this.cartServiceRef?.resetCart();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUser(): AuthResponse | null {
    const user = localStorage.getItem(this.userKey);
    return user ? JSON.parse(user) : null;
  }

  getRole(): string {
    return this.getUser()?.role || '';
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  isLoggedIn(): boolean {
    return this.hasToken();
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }

  private saveSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response));
    this.isLoggedInSubject.next(true);
  }

  deleteUser(id: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/${id}`);
}
}