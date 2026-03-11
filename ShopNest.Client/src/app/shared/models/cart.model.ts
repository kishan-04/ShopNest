export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  imageUrl: string | null;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  stock: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  totalPrice: number;
  totalItems: number;
}