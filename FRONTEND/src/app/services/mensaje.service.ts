import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class MensajeService {
  private mensajeSubject = new BehaviorSubject<{ mensaje: string | null, tipo: 'success' | 'error' }>({ mensaje: null, tipo: 'success' });

  setMensaje(mensaje: string, tipo: 'success' | 'error' = 'success') {
    this.mensajeSubject.next({ mensaje, tipo });
  }

  clearMensaje() {
    this.mensajeSubject.next({ mensaje: null, tipo: 'success' });
  }

  getMensajeObservable() {
    return this.mensajeSubject.asObservable();
  }
} 