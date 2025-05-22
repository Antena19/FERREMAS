import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppComponent } from './app/app.component';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { tokenInterceptorFn } from './app/nucleo/interceptores/token.interceptor.fn';
import { errorInterceptorFn } from './app/nucleo/interceptores/error.interceptor.fn';

// Configuración para aplicaciones standalone
bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        tokenInterceptorFn,
        errorInterceptorFn
      ])
    ),
  ]
}).catch(err => console.error(err));