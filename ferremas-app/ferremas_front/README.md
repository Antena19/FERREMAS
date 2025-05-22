# Ferremas Frontend - Sistema de Ferretería

## Descripción
Ferremas Frontend es una aplicación web moderna desarrollada en Angular 19 que proporciona una interfaz de usuario intuitiva y responsiva para el sistema de gestión de ferretería. La aplicación está diseñada para ofrecer una experiencia de usuario óptima tanto para clientes como para administradores del sistema.

## Tecnologías Principales
- Angular 19.2
- TypeScript
- Bootstrap Icons
- RxJS para programación reactiva
- Angular Router para navegación
- Angular Forms para manejo de formularios

## Estructura del Proyecto
```
ferremas_front/
├── src/
│   ├── app/
│   │   ├── admin/         # Módulos y componentes para administradores
│   │   ├── bodeguero/     # Módulos y componentes para bodegueros
│   │   ├── cliente/       # Módulos y componentes para clientes
│   │   ├── compartido/    # Componentes y servicios compartidos
│   │   ├── modulos/       # Módulos principales de la aplicación
│   │   ├── nucleo/        # Componentes core de la aplicación
│   │   ├── publico/       # Componentes públicos (login, registro, etc.)
│   │   ├── servicios/     # Servicios de la aplicación
│   │   └── vendedor/      # Módulos y componentes para vendedores
│   ├── assets/           # Recursos estáticos
│   └── environments/     # Configuraciones de entorno
```

## Características Principales

### 1. Sistema de Autenticación
- Login y registro de usuarios
- Recuperación de contraseña
- Gestión de sesiones
- Protección de rutas

### 2. Interfaz de Administrador
- Gestión de productos
- Gestión de categorías
- Gestión de usuarios
- Reportes y estadísticas

### 3. Interfaz de Cliente
- Catálogo de productos
- Carrito de compras
- Historial de pedidos
- Gestión de perfil

### 4. Interfaz de Vendedor
- Gestión de ventas
- Atención al cliente
- Procesamiento de pedidos

### 5. Interfaz de Bodeguero
- Control de inventario
- Gestión de stock
- Registro de entradas/salidas

## Funcionalidades Implementadas

### 1. Autenticación y Usuarios ✅
- [x] Sistema de login
- [x] Formulario de registro de usuarios
- [x] Gestión de perfiles de usuario

### 2. Gestión de Productos ✅
- [x] Listado completo de productos
- [x] Vista detallada de producto
- [x] Búsqueda de productos

### 3. Carrito de Compras ✅
- [x] Agregar productos al carrito
- [x] Actualizar cantidades
- [x] Eliminar productos del carrito
- [x] Cálculo de totales

### 4. Sistema de Pagos 🚧
- [x] Integración con MercadoPago
- [x] Creación de preferencias de pago
- [x] Procesamiento de pagos
- [x] Pago por transferencia bancaria
- [ ] Corrección de errores en la creación del carrito
- [ ] Pruebas completas del flujo de pago

## Funcionalidades Pendientes de Implementar

### 1. Gestión de Productos (Admin)
- [ ] Gestión de stock (para administradores)
- [ ] CRUD completo de productos (para administradores)
- [ ] Búsqueda avanzada con filtros (término, categoría, precio)

### 2. Categorías y Marcas
- [ ] Listado de categorías
- [ ] Listado de marcas
- [ ] Gestión de categorías (CRUD para administradores)
- [ ] Gestión de marcas (CRUD para administradores)
- [ ] Filtrado por categorías y marcas

### 3. Sistema de Pedidos
- [ ] Creación de pedidos
- [ ] Seguimiento de estado de pedidos
- [ ] Historial de pedidos por cliente
- [ ] Detalles de pedido
- [ ] Gestión de estados (para administradores)

### 4. Gestión de Clientes
- [ ] Historial de compras
- [ ] Gestión de datos personales
- [ ] Listado de clientes (para administradores)
- [ ] Gestión de clientes (CRUD para administradores)

### 5. Interfaz de Administración
- [ ] Dashboard administrativo
- [ ] Gestión de usuarios
- [ ] Reportes de ventas
- [ ] Gestión de inventario
- [ ] Configuración del sistema

### 6. Mejoras de UX/UI
- [ ] Diseño responsivo para todos los componentes
- [ ] Mensajes de error y éxito
- [ ] Confirmaciones de acciones
- [ ] Loading states
- [ ] Validaciones de formularios

### 7. Optimizaciones
- [ ] Implementación de lazy loading
- [ ] Caché de datos
- [ ] Optimización de imágenes
- [ ] Manejo de errores global
- [ ] Interceptores HTTP

## Configuración

### Requisitos Previos
- Node.js (versión LTS recomendada)
- npm o yarn
- Angular CLI 19.2.5

### Instalación
1. Clonar el repositorio
2. Instalar dependencias:
   ```bash
   npm install
   ```
3. Configurar variables de entorno en `src/environments/`
4. Iniciar el servidor de desarrollo:
   ```bash
   npm start
   ```

### Scripts Disponibles
- `npm start`: Inicia el servidor de desarrollo
- `npm run build`: Compila la aplicación para producción
- `npm test`: Ejecuta las pruebas unitarias
- `npm run watch`: Compila la aplicación en modo watch

## Arquitectura

### Módulos Principales
- **AdminModule**: Gestión administrativa
- **ClienteModule**: Funcionalidades para clientes
- **VendedorModule**: Funcionalidades para vendedores
- **BodegueroModule**: Gestión de inventario
- **PublicoModule**: Componentes públicos

### Servicios
- Servicios de autenticación
- Servicios de productos
- Servicios de pedidos
- Servicios de usuarios
- Servicios de pagos

### Componentes Compartidos
- Header
- Footer
- Sidebar
- Modales
- Formularios comunes

## Características Técnicas

### Seguridad
- Interceptores HTTP para manejo de tokens
- Guards de ruta para protección de acceso
- Manejo seguro de sesiones
- Validación de formularios

### Rendimiento
- Lazy loading de módulos
- Optimización de imágenes
- Caché de datos
- Compresión de assets

### UX/UI
- Diseño responsivo
- Temas claros y oscuros
- Animaciones suaves
- Feedback visual para acciones

## Desarrollo

### Guías de Estilo
- Seguir las convenciones de Angular
- Utilizar TypeScript strict mode
- Implementar interfaces para modelos
- Documentar código con comentarios

### Pruebas
- Pruebas unitarias con Jasmine
- Pruebas e2e con Protractor
- Cobertura de código

## Despliegue
1. Construir la aplicación:
   ```bash
   npm run build
   ```
2. Los archivos generados estarán en `dist/ferremas-app`
3. Desplegar en el servidor web de preferencia

## Contribución
1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## Licencia
Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE.md para más detalles.
