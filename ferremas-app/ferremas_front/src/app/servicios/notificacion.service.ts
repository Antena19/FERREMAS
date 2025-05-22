import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notificacion {
  tipo: 'exito' | 'error' | 'info' | 'advertencia';
  mensaje: string;
  duracion?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificacionService {
  private notificacionSubject = new BehaviorSubject<Notificacion | null>(null);
  public notificacion$ = this.notificacionSubject.asObservable();
  
  constructor() { }

  mostrar(notificacion: Notificacion): void {
    this.notificacionSubject.next(notificacion);
    
    // Ocultar la notificación después de la duración especificada (por defecto 5 segundos)
    const duracion = notificacion.duracion || 5000;
    setTimeout(() => {
      // Solo limpiar si es la misma notificación (para evitar limpiar una nueva)
      if (this.notificacionSubject.value === notificacion) {
        this.ocultar();
      }
    }, duracion);
  }

  ocultar(): void {
    this.notificacionSubject.next(null);
  }

  exito(mensaje: string, duracion?: number): void {
    this.mostrar({
      tipo: 'exito',
      mensaje,
      duracion
    });
  }

  error(mensaje: string, duracion?: number): void {
    this.mostrar({
      tipo: 'error',
      mensaje,
      duracion
    });
  }

  info(mensaje: string, duracion?: number): void {
    this.mostrar({
      tipo: 'info',
      mensaje,
      duracion
    });
  }

  advertencia(mensaje: string, duracion?: number): void {
    this.mostrar({
      tipo: 'advertencia',
      mensaje,
      duracion
    });
  }
}