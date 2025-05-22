import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificacionService, Notificacion } from '../../../servicios/notificacion.service';

@Component({
  selector: 'app-notificacion',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notificacion.component.html',
  styleUrls: ['./notificacion.component.css']
})
export class NotificacionComponent implements OnInit {
  notificacion: Notificacion | null = null;
  visible = false;

  constructor(private notificacionService: NotificacionService) { }

  ngOnInit(): void {
    this.notificacionService.notificacion$.subscribe(notificacion => {
      if (notificacion) {
        this.notificacion = notificacion;
        this.visible = true;
      } else {
        this.ocultar();
      }
    });
  }

  ocultar(): void {
    this.visible = false;
    setTimeout(() => {
      this.notificacion = null;
    }, 300); // Esperar a que termine la animación de salida
  }

  get claseNotificacion(): string {
    if (!this.notificacion) return '';
    
    switch (this.notificacion.tipo) {
      case 'exito':
        return 'notificacion-exito';
      case 'error':
        return 'notificacion-error';
      case 'advertencia':
        return 'notificacion-advertencia';
      case 'info':
        return 'notificacion-info';
      default:
        return '';
    }
  }

  get iconoNotificacion(): string {
    if (!this.notificacion) return '';
    
    switch (this.notificacion.tipo) {
      case 'exito':
        return 'bi-check-circle-fill';
      case 'error':
        return 'bi-exclamation-circle-fill';
      case 'advertencia':
        return 'bi-exclamation-triangle-fill';
      case 'info':
        return 'bi-info-circle-fill';
      default:
        return '';
    }
  }
}