import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';

// ✅ Registro manual de los íconos que usas
import { addIcons } from 'ionicons';
import {
  cartOutline,
  logoFacebook,
  logoInstagram,
  logoWhatsapp,
  logInOutline,
  logOutOutline
} from 'ionicons/icons';

// 🛠️ Agregar los íconos que realmente estás usando
addIcons({
  'cart-outline': cartOutline,
  'logo-facebook': logoFacebook,
  'logo-instagram': logoInstagram,
  'logo-whatsapp': logoWhatsapp,
  'log-in-outline': logInOutline,
  'log-out-outline': logOutOutline
});

// 🚀 Inicializa el módulo principal de la app
platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));
