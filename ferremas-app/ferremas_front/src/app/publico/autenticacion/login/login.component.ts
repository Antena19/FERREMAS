import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../servicios/auth.service';
import { NotificacionService } from '../../../servicios/notificacion.service';
import { Observable } from 'rxjs';
import { RouterModule } from '@angular/router';
import { CarritoService } from '../../../servicios/carrito.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  cargando = false;

  constructor(
    private authService: AuthService,
    private carritoService: CarritoService,
    private router: Router,
    private notificacionService: NotificacionService,
    private route: ActivatedRoute
  ) {}

  login(): void {
    if (!this.email || !this.password) {
      this.notificacionService.error('Por favor, complete todos los campos');
      return;
    }

    this.cargando = true;
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.carritoService.sincronizarCarritoConBackend().subscribe({
          next: () => {
            this.notificacionService.exito('¡Bienvenido!');
            this.router.navigate(['/catalogo']);
          },
          error: (err) => {
            this.notificacionService.error('No se pudo sincronizar el carrito, pero tu sesión fue iniciada.');
            this.router.navigate(['/catalogo']);
          }
        });
      },
      error: (err) => {
        console.error('Error detallado de login:', err);
        const mensajeError = err.error?.message || 'Error al iniciar sesión. Por favor, verifique sus credenciales.';
        this.notificacionService.error(mensajeError);
        this.cargando = false;
      }
    });
  }
} 