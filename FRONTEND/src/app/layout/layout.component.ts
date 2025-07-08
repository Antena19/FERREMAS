import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet, Router } from '@angular/router';
import { Subscription } from 'rxjs';

// ✅ Importamos solo los componentes de Ionic que usamos en el template
import {
  IonBadge,
  IonIcon
} from '@ionic/angular/standalone';

import { ApiService } from 'src/app/services/api.service';     // 📡 Servicio que se comunica con el backend
import { AuthService } from 'src/app/services/auth.service';   // 🔐 Servicio de autenticación para manejar sesiones

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  standalone: true, // ✅ Componente autónomo
  imports: [
    CommonModule,               // 🔁 *ngIf, *ngFor
    RouterOutlet,               // 📍 Para renderizar las rutas hijas
    RouterLink,                 // 🔗 Para navegación con routerLink
    IonBadge,                   // 🧩 Componentes Ionic usados en la navbar
    IonIcon                     // 🧩 Para los iconos en navbar y footer
  ]
})
export class LayoutComponent implements OnInit, OnDestroy {

  carritoCantidad: number = 0;     // 🛒 Número total de productos en el carrito
  usuarioNombre: string = '';      // 👤 Nombre del usuario logueado (si aplica)
  esAdmin: boolean = false;        // 🛡️ Indica si el usuario es administrador
  private authSubscription?: Subscription; // 🔄 Suscripción para cambios de autenticación

  constructor(
    private api: ApiService,       // 📡 Servicio que accede al backend
    private auth: AuthService,     // 🔐 Manejo de sesión del usuario
    private router: Router         // 🧭 Para redirecciones
  ) {}

  /**
   * 🔁 Al iniciar el componente, consultamos si el usuario está logueado
   */
  ngOnInit(): void {
    // 🔄 Suscribirse a cambios en el estado de autenticación
    this.authSubscription = this.auth.getAuthState().subscribe(usuario => {
      this.actualizarEstadoUsuario(usuario);
    });

    // 🔄 Estado inicial
    const usuarioInicial = this.auth.obtenerUsuario();
    this.actualizarEstadoUsuario(usuarioInicial);
  }

  /**
   * 🔄 Actualiza el estado del usuario y carrito
   */
  private actualizarEstadoUsuario(usuario: any): void {
    if (usuario && usuario.id) {
      // ✅ Usuario logueado
      this.usuarioNombre = usuario.nombre || 'Usuario';
      this.esAdmin = usuario.rol && usuario.rol.toLowerCase().includes('admin');
      
      // 📊 Consultar carrito del usuario
      this.api.getCarritoPorUsuario(usuario.id).subscribe({
        next: (productos) => this.carritoCantidad = productos.length,
        error: (err) => {
          console.error('❌ Error al obtener carrito:', err);
          this.carritoCantidad = 0;
        }
      });
    } else {
      // 👤 Usuario NO logueado
      this.usuarioNombre = '';
      this.esAdmin = false;
      this.carritoCantidad = 0;
    }
  }

  /**
   * 🧹 Limpia las suscripciones al destruir el componente
   */
  ngOnDestroy(): void {
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  /**
   * 🔓 Cierra la sesión actual del usuario
   */
  logout(): void {
    this.auth.cerrarSesion();           // 💣 Borra token y usuario
    this.usuarioNombre = '';            // 🔄 Limpiar nombre de usuario
    this.carritoCantidad = 0;          // 🛒 Resetear carrito
    this.router.navigate(['/login']);  // 🔄 Redirige al login
  }
}
