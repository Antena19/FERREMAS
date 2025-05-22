import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../servicios/auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const errorInterceptorFn: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error) => {
      let errorMessage = 'Ocurrió un error desconocido';

      if (error.error instanceof ErrorEvent) {
        errorMessage = `Error: ${error.error.message}`;
      } else {
        switch (error.status) {
          case 401:
            errorMessage = 'No autorizado. Por favor, inicie sesión nuevamente.';
            authService.logout();
            router.navigate(['/publico/autenticacion/login']);
            break;
          case 403:
            errorMessage = 'No tiene permisos para acceder a este recurso.';
            router.navigate(['/publico/inicio']);
            break;
          case 404:
            errorMessage = 'Recurso no encontrado.';
            break;
          case 500:
            errorMessage = 'Error interno del servidor. Por favor, intente más tarde.';
            break;
          default:
            errorMessage = `Error ${error.status}: ${error.error?.message || error.statusText}`;
        }
      }

      console.error(errorMessage);
      return throwError(() => new Error(errorMessage));
    })
  );
};
