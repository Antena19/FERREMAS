import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private usuarioActual: any = null;                 // 🧠 Usuario actual en memoria
  private readonly STORAGE_KEY = 'usuario';          // 📦 Clave de localStorage
  private authStateSubject = new BehaviorSubject<any>(null); // 🔄 Observable para cambios de estado

  constructor() {
    // 🔄 Inicializar el estado de autenticación con el usuario guardado
    const usuarioGuardado = this.obtenerUsuario();
    this.authStateSubject.next(usuarioGuardado);
  }

  /**
   * 💾 Guarda el usuario en memoria y en localStorage
   * Asegura que tenga `id` correcto basado en el backend
   */
  guardarUsuario(usuario: any): void {
    const id = usuario.id; // ✅ Confirmado en tu base de datos
    if (!id) {
      console.warn('⚠️ Usuario sin ID válido:', usuario);
    }

    this.usuarioActual = { ...usuario, id };
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.usuarioActual));
    this.authStateSubject.next(this.usuarioActual); // 🔄 Emitir cambio de estado
  }

  /**
   * 📦 Recupera el usuario desde memoria o localStorage
   */
  obtenerUsuario(): any {
    if (!this.usuarioActual) {
      const guardado = localStorage.getItem(this.STORAGE_KEY);
      if (guardado) {
        try {
          this.usuarioActual = JSON.parse(guardado);
        } catch (error) {
          console.error('⚠️ Error al parsear usuario guardado:', error);
          this.usuarioActual = null;
        }
      }
    }
    return this.usuarioActual;
  }

  /**
   * 🔐 Devuelve el ID del usuario logueado
   */
  obtenerIdUsuario(): number | null {
    const usuario = this.obtenerUsuario();
    return usuario && usuario.id ? usuario.id : null;
  }

  /**
   * 🔑 Devuelve el token JWT del usuario logueado (si existe)
   */
  obtenerToken(): string | null {
    const usuario = this.obtenerUsuario();
    return usuario && usuario.token ? usuario.token : null;
  }

  /**
   * 🚪 Cierra la sesión y limpia localStorage
   */
  cerrarSesion(): void {
    this.usuarioActual = null;
    localStorage.removeItem(this.STORAGE_KEY);
    this.authStateSubject.next(null); // 🔄 Emitir cambio de estado
  }

  /**
   * ✅ Verifica si hay sesión activa
   */
  estaAutenticado(): boolean {
    return this.obtenerIdUsuario() !== null;
  }

  /**
   * 🔄 Observable para escuchar cambios en el estado de autenticación
   */
  getAuthState(): Observable<any> {
    return this.authStateSubject.asObservable();
  }

  /**
   * 🧪 Función de prueba para forzar un usuario (solo presentación)
   */
  forzarUsuarioDemo(): void {
    const demo = {
      id: 33,                                   // 👈 Asegúrate de que exista en tu tabla `usuarios`
      nombre: 'Usuario Demo',
      apellido: 'Demo',
      email: 'demo@ferremas.cl',
      rol: 'cliente'
    };
    this.guardarUsuario(demo);
  }
}
