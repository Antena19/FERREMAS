# Documentación de Endpoints - API Ferremas

## Stack Tecnológico

### Backend
- .NET Core
- C#
- MySQL 8.0.41
- Entity Framework Core
- JWT para autenticación

### Frontend
- Angular 19.2.0
- TypeScript
- Bootstrap Icons
- RxJS
- Angular Material (UI Components)

## Configuración del Entorno

### 1. Requisitos Previos
- Node.js (versión recomendada: 18.x o superior)
- .NET Core SDK (versión recomendada: 7.0 o superior)
- MySQL Server 8.0.41 o superior
- Angular CLI: `npm install -g @angular/cli`
- Git

### 2. Estructura del Proyecto
```
ferremas/
├── ferremas-app/           # Frontend (Angular)
│   └── ferremas_front/
├── Ferremas.Api/          # Backend (.NET Core)
│   └── api_ferremas/
└── BASE DE DATOS/         # Scripts y esquemas de base de datos
    └── ferremas_ecommerce.sql
```

### 3. Configuración del Entorno

#### 3.1 Base de Datos (MySQL)
1. Crear la base de datos:
   ```sql
   CREATE DATABASE ferremas_ecommerce;
   ```
2. Importar el esquema:
   ```bash
   mysql -u root -p ferremas_ecommerce < BASE\ DE\ DATOS/ferremas_ecommerce.sql
   ```

#### 3.2 Backend (.NET Core)
1. Navegar al directorio del backend:
   ```bash
   cd Ferremas.Api/api_ferremas
   ```
2. Restaurar dependencias:
   ```bash
   dotnet restore
   ```
3. Configurar la base de datos:
   - Configurar la cadena de conexión en `appsettings.json`

#### 3.3 Frontend (Angular)
1. Navegar al directorio del frontend:
   ```bash
   cd ferremas-app/ferremas_front
   ```
2. Instalar dependencias:
   ```bash
   npm install
   ```
3. Configurar variables de entorno:
   - Crear archivo `environment.ts` con las variables necesarias

### 4. Variables de Entorno

#### Backend
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ferremas_ecommerce;User=root;Password=tu_password;"
  },
  "JwtSettings": {
    "SecretKey": "tu_clave_secreta",
    "Issuer": "ferremas-api",
    "Audience": "ferremas-client"
  }
}
```

#### Frontend (environment.ts)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  version: '1.0.0'
};
```

### 5. Pasos para la Integración

1. **Clonar el Repositorio**
   ```bash
   git clone [URL_DEL_REPOSITORIO]
   ```

2. **Configurar Base de Datos**
   - Crear base de datos MySQL: `ferremas_ecommerce`
   - Importar el esquema desde `BASE DE DATOS/ferremas_ecommerce.sql`
   - Verificar permisos y roles

3. **Iniciar Backend**
   ```bash
   cd Ferremas.Api/api_ferremas
   dotnet run
   ```

4. **Iniciar Frontend**
   ```bash
   cd ferremas-app/ferremas_front
   ng serve
   ```

## Autenticación (AuthController)

### Login
- **Endpoint**: `POST /api/auth/login`
- **Descripción**: Inicia sesión de usuario
- **Body**:
```json
{
    "email": "usuario@ejemplo.com",
    "password": "contraseña123"
}
```
- **Respuesta Exitosa**: 
```json
{
    "token": "jwt_token",
    "usuario": {
        "id": 1,
        "email": "usuario@ejemplo.com",
        "nombre": "Nombre Usuario"
    },
    "message": "Login exitoso"
}
```

### Registro
- **Endpoint**: `POST /api/auth/registro`
- **Descripción**: Registra un nuevo usuario
- **Body**:
```json
{
    "email": "nuevo@ejemplo.com",
    "password": "contraseña123",
    "nombre": "Nombre Usuario",
    "apellido": "Apellido Usuario"
}
```

### Recuperar Contraseña
- **Endpoint**: `POST /api/auth/recuperar-contrasena`
- **Descripción**: Inicia proceso de recuperación de contraseña
- **Body**:
```json
{
    "email": "usuario@ejemplo.com"
}
```

### Cambiar Contraseña
- **Endpoint**: `POST /api/auth/cambiar-contrasena`
- **Descripción**: Cambia la contraseña del usuario
- **Body**:
```json
{
    "email": "usuario@ejemplo.com",
    "token": "token_recuperacion",
    "nuevaPassword": "nueva_contraseña"
}
```

## Productos (ProductosController)

### Obtener Todos los Productos
- **Endpoint**: `GET /api/productos`
- **Descripción**: Lista todos los productos
- **Respuesta**: Array de ProductoDTO

### Obtener Producto por ID
- **Endpoint**: `GET /api/productos/{id}`
- **Descripción**: Obtiene un producto específico
- **Parámetros**:
  - `id`: ID del producto (int)
- **Respuesta**: ProductoDTO

### Buscar Productos
- **Endpoint**: `GET /api/productos/buscar`
- **Descripción**: Busca productos con filtros
- **Parámetros Query**:
  - `termino`: Término de búsqueda (string, opcional)
  - `categoriaId`: ID de categoría (int, opcional)
  - `precioMin`: Precio mínimo (decimal, opcional)
  - `precioMax`: Precio máximo (decimal, opcional)
- **Ejemplo**: `/api/productos/buscar?termino=taladro&categoriaId=2&precioMin=1000&precioMax=5000`

### Crear Producto
- **Endpoint**: `POST /api/productos`
- **Descripción**: Crea un nuevo producto (Requiere rol admin)
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Body**:
```json
{
    "nombre": "Nombre Producto",
    "descripcion": "Descripción del producto",
    "precio": 1000.00,
    "stock": 10,
    "categoriaId": 1,
    "marcaId": 1
}
```

### Actualizar Producto
- **Endpoint**: `PUT /api/productos/{id}`
- **Descripción**: Actualiza un producto existente (Requiere rol admin)
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del producto (int)
- **Body**:
```json
{
    "nombre": "Nuevo Nombre",
    "descripcion": "Nueva descripción",
    "precio": 1500.00,
    "stock": 15,
    "categoriaId": 2,
    "marcaId": 2
}
```

### Eliminar Producto
- **Endpoint**: `DELETE /api/productos/{id}`
- **Descripción**: Elimina un producto (Requiere rol admin)
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del producto (int)

### Actualizar Stock
- **Endpoint**: `PATCH /api/productos/{id}/stock/{cantidad}`
- **Descripción**: Actualiza el stock de un producto
- **Parámetros**:
  - `id`: ID del producto (int)
  - `cantidad`: Nueva cantidad de stock (int)

## Carrito (CarritoController)

### Agregar al Carrito
- **Endpoint**: `POST /api/carrito/agregar`
- **Descripción**: Agrega un producto al carrito de compras
- **Body**:
```json
{
    "usuarioId": 1,
    "productoId": 1,
    "cantidad": 2
}
```

### Obtener Carrito
- **Endpoint**: `GET /api/carrito/{usuarioId}`
- **Descripción**: Obtiene el carrito de compras de un usuario
- **Parámetros**:
  - `usuarioId`: ID del usuario (int)

### Actualizar Cantidad
- **Endpoint**: `PUT /api/carrito/actualizar-cantidad`
- **Descripción**: Actualiza la cantidad de un item en el carrito
- **Body**:
```json
{
    "usuarioId": 1,
    "itemId": 1,
    "cantidad": 3
}
```

### Eliminar Item
- **Endpoint**: `DELETE /api/carrito/eliminar-item`
- **Descripción**: Elimina un item del carrito
- **Body**:
```json
{
    "usuarioId": 1,
    "itemId": 1
}
```

### Vaciar Carrito
- **Endpoint**: `DELETE /api/carrito/vaciar/{usuarioId}`
- **Descripción**: Vacía todo el carrito de un usuario
- **Parámetros**:
  - `usuarioId`: ID del usuario (int)

## Pedidos (PedidosController)

### Obtener Todos los Pedidos
- **Endpoint**: `GET /api/pedidos`
- **Descripción**: Lista todos los pedidos
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Respuesta**: Array de PedidoDTO

### Obtener Pedido por ID
- **Endpoint**: `GET /api/pedidos/{id}`
- **Descripción**: Obtiene un pedido específico
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del pedido (int)
- **Respuesta**: PedidoDTO

### Obtener Pedidos por Cliente
- **Endpoint**: `GET /api/pedidos/cliente/{clienteId}`
- **Descripción**: Obtiene todos los pedidos de un cliente específico
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `clienteId`: ID del cliente (int)
- **Respuesta**: Array de PedidoDTO

### Crear Pedido
- **Endpoint**: `POST /api/pedidos`
- **Descripción**: Crea un nuevo pedido desde el carrito
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Body**:
```json
{
    "usuarioId": 1,
    "tipoEntrega": "DOMICILIO",
    "sucursalId": 1,
    "direccionId": 1,
    "notas": "Entregar en horario de la tarde"
}
```

### Actualizar Pedido
- **Endpoint**: `PUT /api/pedidos/{id}`
- **Descripción**: Actualiza un pedido existente
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del pedido (int)
- **Body**: PedidoUpdateDTO

### Actualizar Estado del Pedido
- **Endpoint**: `PATCH /api/pedidos/{id}/estado`
- **Descripción**: Actualiza el estado de un pedido
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del pedido (int)
- **Body**:
```json
"EN_PROCESO"
```
- **Estados Válidos**:
  - "PENDIENTE"
  - "EN_PROCESO"
  - "COMPLETADO"
  - "CANCELADO"

### Eliminar Pedido
- **Endpoint**: `DELETE /api/pedidos/{id}`
- **Descripción**: Elimina un pedido
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del pedido (int)

## Pagos (PagosController)

### Obtener Todos los Pagos
- **Endpoint**: `GET /api/pagos`
- **Descripción**: Lista todos los pagos
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Respuesta**: Array de PagoResponseDTO

### Obtener Pago por ID
- **Endpoint**: `GET /api/pagos/{id}`
- **Descripción**: Obtiene un pago específico
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `id`: ID del pago (int)
- **Respuesta**: PagoResponseDTO

### Obtener Pagos por Pedido
- **Endpoint**: `GET /api/pagos/pedido/{pedidoId}`
- **Descripción**: Obtiene todos los pagos asociados a un pedido
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Parámetros**:
  - `pedidoId`: ID del pedido (int)
- **Respuesta**: Array de PagoResponseDTO

### Crear Pago
- **Endpoint**: `POST /api/pagos`
- **Descripción**: Crea un nuevo pago
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Body**: PagoCreateDTO
- **Respuesta**: PagoResponseDTO

### Confirmar Pago
- **Endpoint**: `POST /api/pagos/confirmar`
- **Descripción**: Confirma un pago existente
- **Headers**: 
  - `Authorization: Bearer {token}`
- **Body**: PagoConfirmacionDTO
- **Respuesta**: PagoResponseDTO

## Notas Importantes

1. **Autenticación**:
   - La mayoría de los endpoints requieren autenticación mediante JWT
   - Incluir el token en el header: `Authorization: Bearer {token}`
   - El frontend maneja el token mediante un servicio de autenticación Angular

2. **Roles**:
   - Algunos endpoints requieren rol específico (ej: admin)
   - Verificar permisos antes de realizar operaciones
   - El frontend implementa guards de Angular para protección de rutas

3. **Respuestas**:
   - 200: Operación exitosa
   - 201: Recurso creado
   - 204: Operación exitosa sin contenido
   - 400: Error en la solicitud
   - 401: No autorizado
   - 403: Prohibido
   - 404: Recurso no encontrado
   - 500: Error interno del servidor

4. **Formato de Datos**:
   - Todas las solicitudes y respuestas están en formato JSON
   - Las fechas deben estar en formato ISO 8601
   - Los números decimales deben usar punto como separador

5. **Interoperabilidad Frontend-Backend**:
   - El frontend utiliza servicios Angular para comunicarse con el backend
   - Se implementan interceptores para manejar tokens y errores
   - Se utilizan observables RxJS para manejar las respuestas asíncronas
   - Los DTOs del backend se mapean a interfaces TypeScript en el frontend
   - Se implementan guards de Angular para proteger rutas

6. **Base de Datos**:
   - Sistema de gestión de base de datos: MySQL 8.0.41
   - Nombre de la base de datos: `ferremas_ecommerce`
   - Características:
     - Codificación: utf8mb4
     - Collation: utf8mb4_0900_ai_ci
     - Encriptación: Desactivada por defecto
   - Tablas principales:
     - usuarios
     - productos
     - categorias
     - pedidos
     - detalles_pedido
     - carritos
     - items_carrito
     - pagos
     - clientes
     - sucursales
     - inventario
   - Procedimientos almacenados para operaciones comunes
   - Triggers para mantenimiento de datos
   - Índices optimizados para consultas frecuentes 