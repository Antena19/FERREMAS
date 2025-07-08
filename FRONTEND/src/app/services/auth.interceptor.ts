import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.obtenerToken();
    console.log('🔐 Token obtenido:', token ? 'SÍ' : 'NO');
    console.log('🌐 URL de la petición:', req.url);
    
    if (token) {
      const authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      console.log('✅ Token agregado a la petición');
      return next.handle(authReq);
    }
    console.log('❌ No hay token disponible');
    return next.handle(req);
  }
} 