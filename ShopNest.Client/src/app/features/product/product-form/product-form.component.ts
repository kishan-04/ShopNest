import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from 'src/app/core/services/product.service';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss'],
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  isEditMode = false;
  productId: number | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      price: [0, [Validators.required, Validators.min(0.01)]],
      categoryId: [0, [Validators.required, Validators.min(1)]],
      stock: [0, [Validators.required, Validators.min(0)]],
      imageUrl: [''],
      description: ['', [Validators.required, Validators.minLength(5)]],
    });
  }

  ngOnInit(): void {
    // Check if there is an id in the URL
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.productId = +id; // + converts string to number
      this.loadProduct(this.productId);
    }
  }

  loadProduct(id: number): void {
    this.productService.getById(id).subscribe({
      next: (product) => {
        // Fill the form with existing product data
        this.productForm.patchValue({
          name: product.name,
          price: product.price,
          categoryId: product.categoryId,
          stock: product.stock,
          imageUrl: product.imageUrl,
          description: product.description,
        });
      },
      error: () => {
        this.errorMessage = 'Product not found.';
      },
    });
  }

  onImageError(event: any): void {
    event.target.src = 'https://placehold.co/400x300?text=Invalid+URL';
  }

  onSubmit(): void {
    if (this.productForm.invalid) return;

    this.isLoading = true;
    const formData = this.productForm.value;

    if (this.isEditMode && this.productId) {
      // Update existing product
      this.productService.update(this.productId, formData).subscribe({
        next: () => this.router.navigate(['/products']),
        error: () => {
          this.errorMessage = 'Failed to update product.';
          this.isLoading = false;
        },
      });
    } else {
      // Create new product
      this.productService.create(formData).subscribe({
        next: () => this.router.navigate(['/products']),
        error: () => {
          this.errorMessage = 'Failed to create product.';
          this.isLoading = false;
        },
      });
    }
  }

  // Helper to easily check field errors in HTML
  isFieldInvalid(field: string): boolean {
    const control = this.productForm.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
