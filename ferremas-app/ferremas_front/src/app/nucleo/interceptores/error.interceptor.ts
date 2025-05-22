import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../../servicios/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Ocurrió un error desconocido';
        
        if (error.error instanceof ErrorEvent) {
          // Error del lado del cliente
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Error devuelto por el servidor
          switch (error.status) {
            case 401: // No autorizado
              errorMessage = 'No autorizado. Por favor, inicie sesión nuevamente.';
              this.authService.logout();
              this.router.navigate(['/publico/autenticacion/login']);
              break;
            case 403: // Prohibido
              errorMessage = 'No tiene permisos para acceder a este recurso.';
              this.router.navigate(['/publico/inicio']);
              break;
            case 404: // No encontrado
              errorMessage = 'Recurso no encontrado.';
              break;
            case 500: // Error del servidor
              errorMessage = 'Error interno del servidor. Por favor, intente más tarde.';
              break;
            default:
              errorMessage = `Error ${error.status}: ${error.error?.message || error.statusText}`;
          }
        }
        
        // Aquí podrías mostrar el error en un componente de notificaciones
        console.error(errorMessage);
        
        return throwError(() => new Error(errorMessage));
      })
    );
  }
}