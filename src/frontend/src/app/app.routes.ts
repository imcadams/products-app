import { Routes } from '@angular/router';
import { ProductList } from './components/product-list/product-list';

export const routes: Routes = [
  { path: 'products', component: ProductList },
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  { path: '**', redirectTo: '/products' }
];
