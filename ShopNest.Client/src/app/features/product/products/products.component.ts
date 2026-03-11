import { Component, OnInit, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Product, PagedResult } from 'src/app/shared/models/product.model';
import {
  ProductService,
  ProductFilter,
} from 'src/app/core/services/product.service';
import { AuthService } from 'src/app/core/services/auth.service';
import { CategoryService } from 'src/app/core/services/category.service';
import { Category } from 'src/app/shared/models/category.model';
import { CartService } from 'src/app/core/services/cart.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss'],
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  isLoading = true;
  errorMessage = '';

  currentPage = 1;
  pageSize = 6;
  totalPages = 0;
  totalCount = 0;

  // Bundle properties
  currentBundle = 0; // 0 = first bundle (1-10)
  bundleSize = 5; // pages per bundle

  searchTerm = '';
  selectedCategoryId: number | undefined = undefined;

  private searchSubject = new Subject<string>();

  quantities: { [productId: number]: number } = {};

  constructor(
    private productService: ProductService,
    @Inject(CategoryService) private categoryService: CategoryService,
    private router: Router,
    public authService: AuthService,
    private cartService: CartService,
    @Inject(ToastrService) private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();

    this.searchSubject
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe((search) => {
        this.searchTerm = search;
        this.currentPage = 1;
        this.loadProducts();
      });
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => (this.categories = data),
      error: () => console.error('Failed to load categories'),
    });
  }

  loadProducts(): void {
    this.isLoading = true;

    const filter: ProductFilter = {
      search: this.searchTerm || undefined,
      categoryId: this.selectedCategoryId,
      page: this.currentPage,
      pageSize: this.pageSize,
    };

    this.productService.getPaged(filter).subscribe({
      next: (result: PagedResult<Product>) => {
        this.products = result.data;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load products. Is the API running?';
        this.isLoading = false;
      },
    });
  }

  // If image URL is broken → show placeholder
  onImageError(event: any): void {
    event.target.src = 'https://placehold.co/400x300?text=No+Image';
  }

  onSearch(value: string): void {
    this.searchSubject.next(value);
  }

  onCategoryChange(categoryId: string): void {
    this.selectedCategoryId = categoryId ? +categoryId : undefined;
    this.currentPage = 1;
    this.loadProducts();
  }

  onPageChange(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.syncBundle();
    this.loadProducts();
  }

  onEdit(id: number): void {
    this.router.navigate(['/products/edit', id]);
  }
  onDelete(id: number): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.delete(id).subscribe({
        next: () => {
          this.toastr.success('Product deleted successfully!');
          this.loadProducts();
        },
        error: () => this.toastr.error('Failed to delete product.'),
      });
    }
  }

  getPages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  addToCart(productId: number): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    const quantity = this.quantities[productId] || 1;
    this.cartService.addToCart(productId, quantity).subscribe({
      next: () => {
        this.toastr.success(`${quantity} item(s) added to cart! 🛒`);
        this.quantities[productId] = 1;
      },
      error: (err) =>
        this.toastr.error(err.error?.message || 'Failed to add to cart.'),
    });
  }

  increaseQuantity(productId: number, stock: number): void {
    const current = this.quantities[productId] || 1;
    if (current < stock) this.quantities[productId] = current + 1;
    else alert(`Only ${stock} items available in stock!`);
  }

  decreaseQuantity(productId: number): void {
    const current = this.quantities[productId] || 1;
    if (current > 1) this.quantities[productId] = current - 1;
  }

  getCategoryName(categoryId: number): string {
    return this.categories.find((c) => c.id === categoryId)?.name || 'Unknown';
  }

  // Math min helper for template
  min(a: number, b: number): number {
    return Math.min(a, b);
  }

  get bundlePages(): number[] {
    const start = this.currentBundle * this.bundleSize + 1;
    const end = Math.min(start + this.bundleSize - 1, this.totalPages);
    const pages = [];
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  get totalBundles(): number {
    return Math.ceil(this.totalPages / this.bundleSize);
  }

  nextBundle(): void {
    if (this.currentBundle < this.totalBundles - 1) {
      this.currentBundle++;
      // Go to first page of new bundle
      this.onPageChange(this.currentBundle * this.bundleSize + 1);
    }
  }

  prevBundle(): void {
    if (this.currentBundle > 0) {
      this.currentBundle--;
      // Go to first page of new bundle
      this.onPageChange(this.currentBundle * this.bundleSize + 1);
    }
  }

  // Make sure current bundle matches current page
  syncBundle(): void {
    this.currentBundle = Math.floor((this.currentPage - 1) / this.bundleSize);
  }
}
