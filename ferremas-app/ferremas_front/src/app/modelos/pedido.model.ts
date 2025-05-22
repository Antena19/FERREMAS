// Importamos la interfaz de Producto
import { Producto } from './producto.model';

// Tipos para pedidos
export type EstadoPedido = 'pendiente' | 'aprobado' | 'preparando' | 'enviado' | 'entregado' | 'cancelado';
export type TipoEntrega = 'retiro_tienda' | 'despacho_domicilio';

// Define la interfaz del pedido basada en la tabla 'pedidos'
export interface Pedido {
  id: number;
  usuarioId: number;
  fechaPedido: Date;
  estado: EstadoPedido;
  tipoEntrega: TipoEntrega;
  sucursalId?: number;
  direccionId?: number;
  subtotal: number;
  costoEnvio: number;
  impuestos: number;
  total: number;
  notas?: string;
  vendedorId?: number;
  bodegueroId?: number;
  
  // Relaciones opcionales
  detalles?: DetallePedido[];
}

// Detalle del pedido
export interface DetallePedido {
  id: number;
  pedidoId: number;
  productoId: number;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
  
  // Para UI
  producto?: Producto;
}

// Para crear un nuevo pedido
export interface PedidoCreate {
  usuarioId: number;
  tipoEntrega: TipoEntrega;
  sucursalId?: number;
  direccionId?: number;
  subtotal: number;
  costoEnvio: number;
  impuestos: number;
  total: number;
  notas?: string;
  detalles: {
    productoId: number;
    cantidad: number;
    precioUnitario: number;
    subtotal: number;
  }[];
}