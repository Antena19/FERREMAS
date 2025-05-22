import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Categoria } from '../modelos/categoria.model';

@Injectable({
  providedIn: 'root'
})
export class CategoriaService {
  private apiUrl = `${environment.apiUrl}/categorias`;

  constructor(private http: HttpClient) { }

  obtenerTodas(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<Categoria> {
    return this.http.get<Categoria>(`${this.apiUrl}/${id}`);
  }

  obtenerPorMarca(marcaId: number): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${this.apiUrl}/marca/${marcaId}`);
  }

  getSubcategorias(categoriaId: number): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${this.apiUrl}/${categoriaId}/subcategorias`);
  }

  crearCategoria(categoria: Omit<Categoria, 'id'>): Observable<number> {
    return this.http.post<number>(this.apiUrl, categoria);
  }

  actualizarCategoria(id: number, categoria: Partial<Categoria>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, categoria);
  }

  eliminarCategoria(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}