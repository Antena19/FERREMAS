import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './compartido/componentes/header/header.component';
import { FooterComponent } from './compartido/componentes/footer/footer.component';
import { NotificacionComponent } from './compartido/componentes/notificacion/notificacion.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, FooterComponent, NotificacionComponent],
  template: `
    <app-header></app-header>
    <main class="main-content">
      <router-outlet></router-outlet>
    </main>
    <app-footer></app-footer>
    <app-notificacion></app-notificacion>
  `,
  styles: [`
    .main-content {
      min-height: calc(100vh - 300px);
      padding-bottom: 2rem;
    }
  `]
})
export class AppComponent {
  title = 'ferremas-app';
}