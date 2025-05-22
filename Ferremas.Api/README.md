# Ferremas.Api - Backend de Sistema de Ferretería

## Descripción
Ferremas.Api es un backend robusto desarrollado en ASP.NET Core 8.0 que proporciona una API RESTful para un sistema de gestión de ferretería. El sistema maneja productos, clientes, pedidos, pagos y autenticación de usuarios.

## Tecnologías Principales
- ASP.NET Core 8.0
- MySQL como base de datos
- Dapper como micro-ORM
- JWT para autenticación
- Swagger/OpenAPI para documentación
- MercadoPago SDK para procesamiento de pagos

## Estructura del Proyecto
```
api_ferremas/
├── Controllers/     # Controladores de la API
├── DTOs/           # Objetos de transferencia de datos
├── Modelos/        # Modelos de dominio
├── Repositories/   # Capa de acceso a datos
├── Services/       # Lógica de negocio
├── Data/           # Configuración de base de datos
└── Program.cs      # Punto de entrada y configuración
```

## Características Principales

### 1. Gestión de Productos
- CRUD completo de productos
- Gestión de inventario
- Categorización de productos

### 2. Gestión de Clientes
- Registro y autenticación de usuarios
- Perfiles de cliente
- Historial de compras

### 3. Sistema de Pedidos
- Creación y seguimiento de pedidos
- Gestión del estado de pedidos
- Historial de pedidos

### 4. Integración con MercadoPago
- Procesamiento de pagos
- Gestión de transacciones
- Integración con el carrito de compras

### 5. Carrito de Compras
- Gestión de sesiones de carrito
- Cálculo de totales
- Proceso de checkout

## Configuración

### Requisitos Previos
- .NET 8.0 SDK
- MySQL Server
- Visual Studio 2022 o VS Code

### Variables de Entorno
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "tu_cadena_de_conexion"
  },
  "Jwt": {
    "Secret": "tu_clave_secreta_jwt"
  }
}
```

### Instalación
1. Clonar el repositorio
2. Restaurar los paquetes NuGet
3. Configurar la cadena de conexión en `appsettings.json`
4. Ejecutar las migraciones de base de datos
5. Iniciar el proyecto

## Endpoints Completos

### Autenticación (`/api/auth`)
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/registro` - Registrar nuevo usuario
- `POST /api/auth/recuperar-contrasena` - Solicitar recuperación de contraseña
- `POST /api/auth/cambiar-contrasena` - Cambiar contraseña

### Productos (`/api/productos`)
- `GET /api/productos` - Obtener todos los productos
- `GET /api/productos/{id}` - Obtener producto por ID
- `GET /api/productos/buscar` - Buscar productos con filtros
  - Parámetros: termino, categoriaId, precioMin, precioMax
- `POST /api/productos` - Crear nuevo producto (Requiere rol admin)
- `PUT /api/productos/{id}` - Actualizar producto (Requiere rol admin)
- `DELETE /api/productos/{id}` - Eliminar producto (Requiere rol admin)
- `PATCH /api/productos/{id}/stock/{cantidad}` - Actualizar stock

### Categorías (`/api/categorias`)
- `GET /api/categorias` - Obtener todas las categorías
- `GET /api/categorias/{id}` - Obtener categoría por ID
- `POST /api/categorias` - Crear nueva categoría (Requiere rol admin)
- `PUT /api/categorias/{id}` - Actualizar categoría (Requiere rol admin)
- `DELETE /api/categorias/{id}` - Eliminar categoría (Requiere rol admin)

### Marcas (`/api/marcas`)
- `GET /api/marcas` - Obtener todas las marcas
- `GET /api/marcas/{id}` - Obtener marca por ID
- `POST /api/marcas` - Crear nueva marca (Requiere rol admin)
- `PUT /api/marcas/{id}` - Actualizar marca (Requiere rol admin)
- `DELETE /api/marcas/{id}` - Eliminar marca (Requiere rol admin)

### Clientes (`/api/clientes`)
- `GET /api/clientes` - Obtener todos los clientes (Requiere rol admin)
- `GET /api/clientes/{id}` - Obtener cliente por ID
- `GET /api/clientes/perfil` - Obtener perfil del cliente actual
- `PUT /api/clientes/{id}` - Actualizar datos del cliente
- `DELETE /api/clientes/{id}` - Eliminar cliente (Requiere rol admin)

### Pedidos (`/api/pedidos`)
- `GET /api/pedidos` - Obtener todos los pedidos
- `GET /api/pedidos/{id}` - Obtener pedido por ID
- `POST /api/pedidos` - Crear nuevo pedido
- `PUT /api/pedidos/{id}/estado` - Actualizar estado del pedido
- `GET /api/pedidos/cliente/{clienteId}` - Obtener pedidos por cliente

### Carrito (`/api/carrito`)
- `GET /api/carrito` - Obtener carrito actual
- `POST /api/carrito/agregar` - Agregar producto al carrito
- `PUT /api/carrito/actualizar` - Actualizar cantidad en carrito
- `DELETE /api/carrito/eliminar/{productoId}` - Eliminar producto del carrito
- `POST /api/carrito/checkout` - Procesar checkout del carrito

### Pagos (`/api/pagos`)
- `POST /api/pagos/crear-preferencia` - Crear preferencia de pago
- `POST /api/pagos/webhook` - Webhook para notificaciones de pago
- `GET /api/pagos/historial` - Obtener historial de pagos
- `GET /api/pagos/{id}` - Obtener detalle de pago

### Notas Importantes
- Todos los endpoints que requieren autenticación necesitan el token JWT en el header `Authorization: Bearer {token}`
- Los endpoints marcados con "Requiere rol admin" solo son accesibles para usuarios con rol de administrador
- Los endpoints de búsqueda soportan filtros opcionales
- Las respuestas de error incluyen mensajes descriptivos
- Todos los endpoints devuelven respuestas en formato JSON

## Seguridad
- Autenticación JWT
- CORS configurado para desarrollo local
- Validación de datos
- Manejo seguro de contraseñas

## Desarrollo
El proyecto utiliza una arquitectura en capas:
- Controllers: Manejo de peticiones HTTP
- Services: Lógica de negocio
- Repositories: Acceso a datos
- DTOs: Transferencia de datos
- Modelos: Entidades de dominio

## Documentación API
La documentación de la API está disponible a través de Swagger UI cuando el proyecto se ejecuta en modo desarrollo:
```
https://localhost:{puerto}/swagger
```

## Contribución
1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## Licencia
Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE.md para más detalles. 