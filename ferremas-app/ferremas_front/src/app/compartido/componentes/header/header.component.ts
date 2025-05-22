import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MiniCarritoComponent } from '../mini-carrito/mini-carrito.component';
import { AuthService } from '../../../servicios/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, MiniCarritoComponent],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  usuarioAutenticado = false;
  nombreUsuario = '';
  rol = '';
  mostrarMenu = false;

  constructor(private authService: AuthService) { }

  ngOnInit(): void {
    this.authService.usuario$.subscribe(usuario => {
      this.usuarioAutenticado = !!usuario;
      if (usuario) {
        this.nombreUsuario = `${usuario.nombre} ${usuario.apellido}`;
        this.rol = usuario.rol;
      } else {
        this.nombreUsuario = '';
        this.rol = '';
      }
    });
  }

  toggleMenu(): void {
    this.mostrarMenu = !this.mostrarMenu;
  }

  cerrarSesion(): void {
    this.authService.logout();
  }
}