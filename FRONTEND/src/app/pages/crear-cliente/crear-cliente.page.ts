import { Component } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastController } from '@ionic/angular';
import { MensajeService } from 'src/app/services/mensaje.service';
import { IonIcon } from '@ionic/angular/standalone';

import {
  IonContent,
  IonInput
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-crear-cliente',
  templateUrl: './crear-cliente.page.html',
  styleUrls: ['./crear-cliente.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    IonContent,
    IonInput
    
  ],
})
export class CrearClientePage {

  // 🧾 Datos del cliente (estructura esperada por el backend)
  cliente = {
    nombre: '',
    apellido: '',               // ✅ Campo nuevo
    email: '',                  // ✅ antes era 'correo'
    rut: '',
    telefono: '',
    password: '',               // ✅ antes era 'clave'
    confirmarPassword: ''      // ✅ Campo nuevo
  };

  // 🆔 Diccionario para mostrar nombres más legibles en errores
  nombresCampos: any = {
    nombre: 'Nombre',
    apellido: 'Apellido',
    email: 'Correo electrónico',
    rut: 'RUT',
    telefono: 'Teléfono',
    password: 'Contraseña',
    confirmarPassword: 'Confirmar contraseña'
  };

  mensajeError: string = ''; // ⚠️ Texto de error en pantalla

  nombreTouched = false;
  apellidoTouched = false;
  emailTouched = false;
  rutTouched = false;
  telefonoTouched = false;
  passwordTouched = false;
  confirmarPasswordTouched = false;
  submitted = false;

  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private api: ApiService,
    private router: Router,
    private toastController: ToastController,
    private mensajeService: MensajeService
  ) {}

  async mostrarToast(mensaje: string, color: 'success' | 'danger' = 'success') {
    const toast = await this.toastController.create({
      message: mensaje,
      duration: 2500,
      position: 'bottom',
      animated: true,
      cssClass: color === 'success' ? 'toast-exito' : 'toast-error'
    });
    toast.present();
  }

  /**
   * ✅ Intenta registrar al cliente si los datos son válidos
   */
  registrarCliente() {
    this.submitted = true;
    if (this.isRegistroDisabled()) return;

    this.api.crearCliente(this.cliente).subscribe({
      next: async () => {
        this.mensajeError = '';
        this.cliente = {
          nombre: '',
          apellido: '',
          email: '',
          rut: '',
          telefono: '',
          password: '',
          confirmarPassword: ''
        };
        this.nombreTouched = false;
        this.apellidoTouched = false;
        this.emailTouched = false;
        this.rutTouched = false;
        this.telefonoTouched = false;
        this.passwordTouched = false;
        this.confirmarPasswordTouched = false;
        this.submitted = false;
        this.mensajeService.setMensaje('¡Registro exitoso! Ahora puedes iniciar sesión.', 'success');
        this.router.navigate(['/login']);
      },
      error: async (err) => {
        let msg = '❌ Error al registrar cliente. Intenta más tarde.';
        if (err && err.error && err.error.message) {
          msg = err.error.message;
        }
        this.mensajeError = msg;
        await this.mostrarToast(msg, 'danger');
      }
    });
  }

  /**
   * 🔍 Verifica que todos los campos estén completos y válidos
   */
  validarFormulario(): boolean {
    const campos = Object.entries(this.cliente);
    for (const [key, valor] of campos) {
      if (!valor.trim()) {
        const nombreCampo = this.nombresCampos[key] || key;
        this.mensajeError = `⚠️ El campo "${nombreCampo}" es obligatorio`;
        return false;
      }
    }

    // ⚠️ Validar que las contraseñas coincidan
    if (this.cliente.password !== this.cliente.confirmarPassword) {
      this.mensajeError = '⚠️ Las contraseñas no coinciden';
      return false;
    }

    this.mensajeError = '';
    return true;
  }

  /**
   * 🧭 Redirige al home al hacer clic en el logo FERREMAS
   */
  irAlHome() {
    this.router.navigate(['/home']);
  }

  // Mensajes de error individuales
  get nombreError(): string {
    const v = this.cliente.nombre.trim();
    if (!v) return 'El nombre es obligatorio';
    if (!/^[A-Za-zÁÉÍÓÚáéíóúÑñ ]{2,}$/.test(v)) return 'Solo letras y mínimo 2 caracteres';
    return '';
  }
  get apellidoError(): string {
    const v = this.cliente.apellido.trim();
    if (!v) return 'El apellido es obligatorio';
    if (!/^[A-Za-zÁÉÍÓÚáéíóúÑñ ]{2,}$/.test(v)) return 'Solo letras y mínimo 2 caracteres';
    return '';
  }
  get emailError(): string {
    const v = this.cliente.email.trim();
    if (!v) return 'El correo es obligatorio';
    if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(v)) return 'Correo no válido';
    return '';
  }
  get rutError(): string {
    const v = this.cliente.rut.trim();
    if (!v) return 'El RUT es obligatorio';
    if (!/^\d{7,8}-[\dkK]$/.test(v)) return 'Formato: 12345678-9 (sin puntos, solo guion)';
    return '';
  }
  get telefonoError(): string {
    const v = this.cliente.telefono.trim();
    if (!v) return 'El teléfono es obligatorio';
    if (!/^\+?\d[\d ]{8,}$/.test(v)) return 'Debe tener al menos 9 dígitos';
    return '';
  }
  get passwordError(): string {
    const v = this.cliente.password;
    if (!v) return 'La contraseña es obligatoria';
    if (v.length < 6) return 'Mínimo 6 caracteres';
    return '';
  }
  get confirmarPasswordError(): string {
    const v = this.cliente.confirmarPassword;
    if (!v) return 'Debes confirmar la contraseña';
    if (v !== this.cliente.password) return 'Las contraseñas no coinciden';
    return '';
  }

  isRegistroDisabled(): boolean {
    return !!(
      this.nombreError ||
      this.apellidoError ||
      this.emailError ||
      this.rutError ||
      this.telefonoError ||
      this.passwordError ||
      this.confirmarPasswordError
    );
  }

  marcarTouched(campo: string) {
    switch (campo) {
      case 'nombre': this.nombreTouched = true; break;
      case 'apellido': this.apellidoTouched = true; break;
      case 'email': this.emailTouched = true; break;
      case 'rut': this.rutTouched = true; break;
      case 'telefono': this.telefonoTouched = true; break;
      case 'password': this.passwordTouched = true; break;
      case 'confirmarPassword': this.confirmarPasswordTouched = true; break;
    }
  }

  // Limpia puntos del RUT automáticamente al escribir
  onRutChange() {
    this.cliente.rut = this.cliente.rut.replace(/\./g, '');
    this.rutTouched = true;
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }
  toggleConfirmPassword() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }
}
