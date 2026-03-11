import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, PagedResult } from '../../shared/models/product.model';

export interface ProductFilter {
  search?: string;
  categoryId?: number;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = 'https://localhost:7271/api/products';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getPaged(filter: ProductFilter): Observable<PagedResult<Product>> {
    // Build query string parameters
    let params = new HttpParams();

    if (filter.search)
      params = params.set('search', filter.search);

    if (filter.categoryId)
      params = params.set('categoryId', filter.categoryId.toString());

    params = params.set('page',     (filter.page     || 1).toString());
    params = params.set('pageSize', (filter.pageSize || 6).toString());

    return this.http.get<PagedResult<Product>>(`${this.apiUrl}/paged`, { params });
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  create(product: Partial<Product>): Observable<any> {
    return this.http.post(this.apiUrl, product);
  }

  update(id: number, product: Partial<Product>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, product);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}