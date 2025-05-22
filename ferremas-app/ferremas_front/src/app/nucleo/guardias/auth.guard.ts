import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../../servicios/auth.service';
import { RolUsuario } from '../../modelos/usuario.model';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}
  
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    const usuario = this.authService['usuarioSubject'].value;
    const token = this.authService.getToken && this.authService.getToken();
    console.log('Guard: usuario:', usuario, 'token:', token);

    if (this.authService.isAuthenticated()) {
      // Verificar roles si están especificados
      const roles = route.data['roles'] as RolUsuario[];
      if (roles && roles.length > 0) {
        if (this.authService.hasAnyRole(roles)) {
          return true;
        } else {
          // Si está autenticado pero no tiene el rol adecuado
          this.router.navigate(['/inicio']);
          return false;
        }
      }
      return true;
    }
    
    // Si no está autenticado, redirigir al login
    this.router.navigate(['/autenticacion/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }
}