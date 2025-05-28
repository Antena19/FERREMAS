import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of, switchMap, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
    Usuario, 
    LoginRequest, 
    LoginResponse, 
    RegistroRequest, 
    RecuperacionContrasenaRequest, 
    CambioContrasenaRequest,
    RolUsuario 
} from '../modelos/usuario.model';
import { Producto } from '../modelos/producto.model';
import { Carrito } from '../modelos/carrito.model';
import { EventosService } from './eventos.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenKey = 'auth_token';
  private usuarioSubject = new BehaviorSubject<Usuario | null>(this.cargarUsuarioLocal());
  public usuario$ = this.usuarioSubject.asObservable();

  constructor(
    private http: HttpClient,
    private eventosService: EventosService
  ) {
    this.cargarUsuarioActual();
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<any>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(response => {
          console.log('Login exitoso, usuario:', response.usuario);
          localStorage.setItem('auth_token', response.token);
          localStorage.setItem('usuario', JSON.stringify(response.usuario));
          this.usuarioSubject.next(response.usuario);
          // Notificar que el usuario ha iniciado sesión
          console.log('Notificando evento de login...');
          this.eventosService.notificarLogin();
        })
      );
  }

  registro(usuario: RegistroRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/registro`, usuario);
  }

  recuperarContrasena(email: string): Observable<void> {
    const request: RecuperacionContrasenaRequest = { email };
    return this.http.post<void>(`${this.apiUrl}/recuperar-contrasena`, request);
  }

  cambiarContrasena(token: string, nuevaPassword: string): Observable<void> {
    const request: CambioContrasenaRequest = {
      token,
      nuevaPassword,
      confirmarPassword: nuevaPassword
    };
    return this.http.post<void>(`${this.apiUrl}/cambiar-contrasena`, request);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem('usuario');
    this.usuarioSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  esAdmin(): boolean {
    const usuario = this.usuarioSubject.value;
    return usuario?.rol === 'administrador';
  }

  hasAnyRole(roles: RolUsuario[]): boolean {
    const usuario = this.usuarioSubject.value;
    return usuario ? roles.includes(usuario.rol as RolUsuario) : false;
  }

  private guardarToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  private cargarUsuarioActual(): void {
    const token = this.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const usuario: Usuario = {
          id: payload.sub,
          nombre: payload.nombre,
          apellido: payload.apellido,
          email: payload.email,
          rol: payload.rol,
          rut: payload.rut,
          telefono: payload.telefono,
          fechaRegistro: new Date(payload.fechaRegistro),
          activo: payload.activo
        };
        this.usuarioSubject.next(usuario);
      } catch (error) {
        console.error('Error al decodificar el token:', error);
        this.logout();
      }
    }
  }

  private cargarUsuarioLocal(): Usuario | null {
    const usuario = localStorage.getItem('usuario');
    return usuario ? JSON.parse(usuario) : null;
  }

  getUsuarioId(): number | null {
    const usuario = this.usuarioSubject.value;
    return usuario?.id ?? null;
  }
}