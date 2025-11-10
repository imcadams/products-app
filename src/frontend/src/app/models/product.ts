export interface Product {
  id: number;
  name: string;
  price: number;
  categoryName: string;
  stockQuantity: number;
  description?: string;
  isActive: boolean;
  createdDate?: string;
  categoryId?: number;
}
