import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonInput,
  IonButton
} from '@ionic/angular/standalone';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-recuperar-contrasena',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    IonContent,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonInput,
    IonButton
  ],
  templateUrl: './recuperar-contrasena.page.html',
  styleUrls: ['./recuperar-contrasena.page.scss']
})
export class RecuperarContrasenaPage {
  correo: string = '';
  mensaje: string = '';
  error: string = '';
  cargando: boolean = false;

  constructor(private api: ApiService) {}

  recuperar(): void {
    this.mensaje = '';
    this.error = '';
    if (!this.correo) {
      this.error = 'Por favor, ingresa tu correo.';
      return;
    }
    if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(this.correo)) {
      this.error = 'Ingresa un correo válido.';
      return;
    }
    this.cargando = true;
    this.api.recuperarContrasena({ Email: this.correo }).subscribe({
      next: (resp) => {
        this.mensaje = resp?.message || 'Si el correo existe, recibirás un enlace de recuperación.';
        this.cargando = false;
      },
      error: (err) => {
        this.error = err?.error?.message || 'No se pudo procesar la solicitud.';
        this.cargando = false;
      }
    });
  }

  isDisabled(): boolean {
    return !this.correo || !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(this.correo);
  }
}
