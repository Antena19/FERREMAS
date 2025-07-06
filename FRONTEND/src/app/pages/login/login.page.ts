import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { MensajeService } from 'src/app/services/mensaje.service';
import { Subscription } from 'rxjs';

import {
  IonContent,
  IonInput,
  IonIcon
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    IonContent,
    IonInput
  ],
})
export class LoginPage implements OnInit, OnDestroy {

  correo: string = '';        // 📧 Email ingresado
  clave: string = '';         // 🔒 Contraseña ingresada
  mensajeError: string = '';  // ⚠️ Texto de error mostrado
  showRegistroExito = false;
  registroExitoMsg = '';
  registroExitoTipo: 'success' | 'error' = 'success';
  mensajeSub: Subscription | undefined;
  showPassword = false;

  constructor(
    private api: ApiService,       // 📡 Servicio API para login
    private auth: AuthService,     // 🔐 Servicio para guardar sesión
    private router: Router,         // 🧭 Navegación entre rutas
    private mensajeService: MensajeService
  ) {}

  ngOnInit() {
    // Limpiar campos al entrar al login
    this.correo = '';
    this.clave = '';
    this.mensajeError = '';

    this.mensajeSub = this.mensajeService.getMensajeObservable().subscribe(({ mensaje, tipo }) => {
      if (mensaje) {
        this.showRegistroExito = true;
        this.registroExitoMsg = mensaje;
        this.registroExitoTipo = tipo;
        setTimeout(() => {
          this.showRegistroExito = false;
          this.mensajeService.clearMensaje();
        }, 3500);
      }
    });
  }

  ngOnDestroy() {
    this.mensajeSub?.unsubscribe();
  }

  /**
   * 🔐 Intenta iniciar sesión
   */
  login() {
    this.mensajeError = '';

    // ⚠️ Validación simple de campos vacíos
    if (!this.correo.trim() || !this.clave.trim()) {
      this.mensajeError = '⚠️ Debes ingresar correo y clave';
      return;
    }

    const credenciales = {
      email: this.correo,
      password: this.clave
    };

    // 🚀 Enviar credenciales al backend
    this.api.login(credenciales).subscribe({
      next: (respuesta) => {
        console.log('🎯 Usuario recibido:', respuesta);

        // ✅ Validación del objeto recibido
        if (respuesta && respuesta.usuario && respuesta.usuario.id) {
          // 💾 Guardar usuario y token (opcional)
          this.auth.guardarUsuario({
            ...respuesta.usuario,
            token: respuesta.token
          });

          this.router.navigate(['/home']); // 🏠 Redirige al home
        } else {
          this.mensajeError = '⚠️ Datos de usuario inválidos';
        }
      },
      error: (err) => {
        console.error('❌ Error en login:', err);
        this.mensajeError = '❌ Credenciales incorrectas o servidor no disponible';
      }
    });
  }

  /**
   * 🧭 Redirige al home al hacer clic en el logo FERREMAS
   */
  irAlHome() {
    this.router.navigate(['/home']);
  }

  isLoginDisabled(): boolean {
    return (
      !this.correo ||
      !this.clave ||
      !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(this.correo)
    );
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}
