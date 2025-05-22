import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Marca } from '../modelos/marca.model';

@Injectable({
  providedIn: 'root'
})
export class MarcaService {
  private apiUrl = `${environment.apiUrl}/marcas`;

  constructor(private http: HttpClient) { }

  obtenerTodas(): Observable<Marca[]> {
    return this.http.get<Marca[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<Marca> {
    return this.http.get<Marca>(`${this.apiUrl}/${id}`);
  }

  obtenerPorCategoria(categoriaId: number): Observable<Marca[]> {
    return this.http.get<Marca[]>(`${this.apiUrl}/categoria/${categoriaId}`);
  }

  crearMarca(marca: Omit<Marca, 'id'>): Observable<number> {
    return this.http.post<number>(this.apiUrl, marca);
  }

  actualizarMarca(id: number, marca: Partial<Marca>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, marca);
  }

  eliminarMarca(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 