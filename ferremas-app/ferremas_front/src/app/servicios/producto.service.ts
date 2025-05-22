import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Producto, ProductoDetalle } from '../modelos/producto.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductoService {
  private apiUrl = `${environment.apiUrl}/productos`;

  constructor(private http: HttpClient) { }

  obtenerTodos(): Observable<Producto[]> {
    return this.http.get<Producto[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<ProductoDetalle> {
    return this.http.get<ProductoDetalle>(`${this.apiUrl}/${id}`);
  }

  obtenerPorCategoria(categoriaId: number): Observable<Producto[]> {
    return this.http.get<Producto[]>(`${this.apiUrl}/categoria/${categoriaId}`);
  }

  obtenerPorMarca(marcaId: number): Observable<Producto[]> {
    return this.http.get<Producto[]>(`${this.apiUrl}/marca/${marcaId}`);
  }

  buscar(termino: string): Observable<Producto[]> {
    return this.http.get<Producto[]>(`${this.apiUrl}/buscar`, {
      params: { termino }
    });
  }
}