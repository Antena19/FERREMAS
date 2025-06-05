# Ferremas.Api - Backend de Sistema de Ferretería

## Descripción
Ferremas.Api es un backend robusto desarrollado en ASP.NET Core 8.0 que proporciona una API RESTful para un sistema de gestión de ferretería. El sistema maneja productos, clientes, pedidos, pagos y autenticación de usuarios.

## Roles y Permisos

### Administrador (`/api/Admin`)
- **Usuarios:**
  - `GET /api/Admin/usuarios`
  - `GET /api/Admin/usuarios/{id}`
  - `POST /api/Admin/usuarios`
  - `PUT /api/Admin/usuarios`
  - `DELETE /api/Admin/usuarios/{id}`
  - `PUT /api/Admin/usuarios/{id}/activar`
  - `PUT /api/Admin/usuarios/{id}/desactivar`
- **Clientes:**
  - `GET /api/Admin/clientes`
  - `GET /api/Admin/clientes/{id}`
  - `POST /api/Admin/clientes`
  - `PUT /api/Admin/clientes`
  - `DELETE /api/Admin/clientes/{id}`
- **Vendedores, Bodegueros, Contadores:**
  - CRUD similar a clientes, usando `/api/Admin/vendedores`, `/api/Admin/bodegueros`, `/api/Admin/contadores`

### Vendedor (`/api/Vendedor`)
- `GET /api/Vendedor/clientes`
- `GET /api/Vendedor/cliente/{clienteId}`
- `GET /api/Vendedor/pedidos-asignados/{vendedorId}`
- `GET /api/Vendedor/pedido/{pedidoId}`
- `POST /api/Vendedor/pedido-bodega`
- `GET /api/Vendedor/todos-pedidos`
- `PUT /api/Vendedor/pedido/{pedidoId}/estado`

### Bodeguero (`/api/Bodeguero`)
- `GET /api/Bodeguero/inventario/{sucursalId}`
- `GET /api/Bodeguero/pedidos-asignados/{bodegueroId}`
- `GET /api/Bodeguero/pedido-bodega/{pedidoBodegaId}`
- `POST /api/Bodeguero/entrega-bodega/{pedidoBodegaId}`
- `PUT /api/Bodeguero/producto`
- `PUT /api/Bodeguero/inventario`
- `GET /api/Bodeguero/productos/{sucursalId}`

### Contador (`/api/Contador`)
- `POST /api/Contador/aprobar-transferencia`
- `GET /api/Contador/historial-pagos?fechaInicio=...&fechaFin=...`
- `GET /api/Contador/pagos-pendientes`
- `GET /api/Contador/pago/{pagoId}`
- `GET /api/Contador/pedidos-pendientes-pago`

### Cliente (`/api/Clientes`)
- `GET /api/Clientes`
- `GET /api/Clientes/{id}`
- `GET /api/Clientes/rut/{rut}`
- `GET /api/Clientes/correo/{correo}`
- `POST /api/Clientes`
- `PUT /api/Clientes/{id}`
- `DELETE /api/Clientes/{id}`
- **Direcciones:**
  - `GET /api/Clientes/{clienteId}/direcciones`
  - `GET /api/Clientes/direcciones/{id}`
  - `POST /api/Clientes/{clienteId}/direcciones`
  - `PUT /api/Clientes/direcciones/{id}`
  - `DELETE /api/Clientes/direcciones/{id}`

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

### 6. Gestión de Empleados
- CRUD completo de usuarios por rol (vendedores, bodegueros, contadores)
- Activación/desactivación de usuarios
- Asignación de sucursales

### 7. Gestión de Inventario
- Control de stock por sucursal
- Actualización de productos y stock
- Gestión de entregas a bodega

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
- `POST /api/auth/login` - Iniciar sesión **(Público)**
- `POST /api/auth/registro` - Registrar nuevo usuario **(Público)**
- `POST /api/auth/recuperar-contrasena` - Solicitar recuperación de contraseña **(Público)**
- `POST /api/auth/cambiar-contrasena` - Cambiar contraseña **(Público)**
- `POST /api/auth/refresh-token` - Refrescar token **(Público)**

### Productos (`/api/productos`)
- `GET /api/productos` - Obtener todos los productos **(Público)**
- `GET /api/productos/{id}` - Obtener producto por ID **(Público)**
- `GET /api/productos/buscar` - Buscar productos con filtros **(Público)**
- `GET /api/productos/categoria/{id}` - Productos por categoría **(Público)**
- `POST /api/productos` - Crear nuevo producto **(Solo Administrador)**
- `PUT /api/productos/{id}` - Actualizar producto **(Solo Administrador)**
- `DELETE /api/productos/{id}` - Eliminar producto **(Solo Administrador)**
- `PATCH /api/productos/{id}/stock/{cantidad}` - Actualizar stock **(Solo Administrador)**

### Categorías (`/api/categorias`)
- `GET /api/categorias` - Obtener todas las categorías **(Público)**

### Marcas (`/api/marcas`)
- `GET /api/marcas` - Obtener todas las marcas **(Público)**

### Clientes (`/api/clientes`)
- `GET /api/clientes` - Obtener todos los clientes **(Solo Administrador)**
- `GET /api/clientes/{id}` - Obtener cliente por ID **(Requiere login)**
- `GET /api/clientes/rut/{rut}` - Obtener cliente por RUT **(Requiere login)**
- `GET /api/clientes/correo/{correo}` - Obtener cliente por correo **(Requiere login)**
- `POST /api/clientes` - Crear cliente **(Público)**
- `PUT /api/clientes/{id}` - Actualizar datos del cliente **(Requiere login)**
- `DELETE /api/clientes/{id}` - Eliminar cliente **(Solo Administrador)**
- `GET /api/clientes/{clienteId}/direcciones` - Listar direcciones de cliente **(Requiere login)**
- `GET /api/clientes/direcciones/{id}` - Obtener dirección por ID **(Requiere login)**
- `POST /api/clientes/{clienteId}/direcciones` - Crear dirección **(Requiere login)**
- `PUT /api/clientes/direcciones/{id}` - Actualizar dirección **(Requiere login)**
- `DELETE /api/clientes/direcciones/{id}` - Eliminar dirección **(Requiere login)**

### Pedidos (`/api/pedidos`)
- `GET /api/pedidos` - Obtener todos los pedidos **(Solo Administrador)**
- `GET /api/pedidos/{id}` - Obtener pedido por ID **(Requiere login)**
- `POST /api/pedidos` - Crear nuevo pedido **(Requiere login)**
- `PUT /api/pedidos/{id}/estado` - Actualizar estado del pedido **(Solo Vendedor/Admin)**
- `GET /api/pedidos/cliente/{clienteId}` - Obtener pedidos por cliente **(Requiere login)**

### Carrito (`/api/carrito`)
- `GET /api/carrito` - Obtener carrito actual **(Requiere login)**
- `POST /api/carrito/agregar` - Agregar producto al carrito **(Requiere login)**
- `PUT /api/carrito/actualizar` - Actualizar cantidad en carrito **(Requiere login)**
- `DELETE /api/carrito/eliminar/{productoId}` - Eliminar producto del carrito **(Requiere login)**
- `POST /api/carrito/checkout` - Procesar checkout del carrito **(Requiere login)**

### Pagos (`/api/pagos`)
- `POST /api/pagos/crear-preferencia` - Crear preferencia de pago **(Requiere login)**
- `POST /api/pagos/webhook` - Webhook para notificaciones de pago **(Público)**
- `GET /api/pagos/historial` - Obtener historial de pagos **(Solo Contador/Admin)**
- `GET /api/pagos/{id}` - Obtener detalle de pago **(Solo Contador/Admin)**

### Admin (`/api/Admin`)
- `GET /api/Admin/usuarios` - Listar todos los usuarios **(Solo Administrador)**
- `GET /api/Admin/usuarios/{id}` - Obtener usuario específico **(Solo Administrador)**
- `POST /api/Admin/usuarios` - Crear nuevo usuario **(Solo Administrador)**
- `PUT /api/Admin/usuarios` - Actualizar usuario **(Solo Administrador)**
- `DELETE /api/Admin/usuarios/{id}` - Eliminar usuario **(Solo Administrador)**
- `PUT /api/Admin/usuarios/{id}/activar` - Activar usuario **(Solo Administrador)**
- `PUT /api/Admin/usuarios/{id}/desactivar` - Desactivar usuario **(Solo Administrador)**
- `/api/Admin/clientes` (GET, POST, PUT, DELETE) **(Solo Administrador)**
- `/api/Admin/vendedores` (GET, POST, PUT, DELETE) **(Solo Administrador)**
- `/api/Admin/bodegueros` (GET, POST, PUT, DELETE) **(Solo Administrador)**
- `/api/Admin/contadores` (GET, POST, PUT, DELETE) **(Solo Administrador)**

### Vendedor (`/api/Vendedor`)
- `GET /api/Vendedor/clientes` **(Solo Vendedor)**
- `GET /api/Vendedor/cliente/{clienteId}` **(Solo Vendedor)**
- `GET /api/Vendedor/pedidos-asignados/{vendedorId}` **(Solo Vendedor)**
- `GET /api/Vendedor/pedido/{pedidoId}` **(Solo Vendedor)**
- `POST /api/Vendedor/pedido-bodega` **(Solo Vendedor)**
- `GET /api/Vendedor/todos-pedidos` **(Solo Vendedor)**
- `PUT /api/Vendedor/pedido/{pedidoId}/estado` **(Solo Vendedor)**

### Bodeguero (`/api/Bodeguero`)
- `GET /api/Bodeguero/inventario/{sucursalId}` - Ver inventario de sucursal **(Solo Bodeguero)**
- `GET /api/Bodeguero/productos/{sucursalId}` - Ver productos de sucursal **(Solo Bodeguero)**
- `PUT /api/Bodeguero/producto` - Actualizar producto **(Solo Bodeguero)**
- `PUT /api/Bodeguero/inventario` - Actualizar inventario **(Solo Bodeguero)**
- `GET /api/Bodeguero/pedidos-asignados/{bodegueroId}` - Ver pedidos asignados **(Solo Bodeguero)**
- `GET /api/Bodeguero/pedido-bodega/{pedidoBodegaId}` - Ver detalle de pedido **(Solo Bodeguero)**
- `POST /api/Bodeguero/entrega-bodega/{pedidoBodegaId}` - Crear entrega a bodega **(Solo Bodeguero)**

### Contador (`/api/Contador`)
- `POST /api/Contador/aprobar-transferencia` **(Solo Contador)**
- `GET /api/Contador/historial-pagos?fechaInicio=...&fechaFin=...` **(Solo Contador)**
- `GET /api/Contador/pagos-pendientes` **(Solo Contador)**
- `GET /api/Contador/pago/{pagoId}` **(Solo Contador)**
- `GET /api/Contador/pedidos-pendientes-pago` **(Solo Contador)**

### Notas Importantes
- Todos los endpoints que requieren autenticación necesitan el token JWT en el header `Authorization: Bearer {token}`
- Los endpoints marcados con "Solo Administrador" solo son accesibles para usuarios con rol de administrador
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