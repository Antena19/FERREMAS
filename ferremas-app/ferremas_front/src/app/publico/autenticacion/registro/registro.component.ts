import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../servicios/auth.service';
import { NotificacionService } from '../../../servicios/notificacion.service';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './registro.component.html',
  styleUrls: ['./registro.component.css']
})
export class RegistroComponent {
  nombre = '';
  apellido = '';
  email = '';
  password = '';
  confirmarPassword = '';
  rut = '';
  telefono = '';
  cargando = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificacionService: NotificacionService
  ) {}

  registrar(): void {
    if (!this.validarFormulario()) {
      return;
    }

    this.cargando = true;
    const usuario = {
      nombre: this.nombre,
      apellido: this.apellido,
      email: this.email,
      password: this.password,
      confirmarPassword: this.confirmarPassword,
      rut: this.rut,
      telefono: this.telefono,
      rol: 'cliente' as const
    };

    this.authService.registro(usuario).subscribe({
      next: () => {
        this.notificacionService.exito('¡Registro exitoso! Por favor, inicie sesión.');
        this.cargando = false;
        setTimeout(() => {
          this.router.navigate(['/autenticacion/login']);
        }, 1500);
      },
      error: (err: any) => {
        console.error('Error detallado de registro:', err);
        const mensajeError = err.error?.message || 'Error al registrar. Por favor, intente nuevamente.';
        this.notificacionService.error(mensajeError);
        this.cargando = false;
      }
    });
  }

  private validarFormulario(): boolean {
    if (!this.nombre || !this.apellido || !this.email || !this.password || !this.confirmarPassword) {
      this.notificacionService.error('Por favor, complete todos los campos obligatorios');
      return false;
    }

    if (this.password !== this.confirmarPassword) {
      this.notificacionService.error('Las contraseñas no coinciden');
      return false;
    }

    if (this.password.length < 6) {
      this.notificacionService.error('La contraseña debe tener al menos 6 caracteres');
      return false;
    }

    return true;
  }
} 