// Importamos la interfaz de Producto
import { Producto } from './producto.model';

// Define la interfaz del carrito basada en la tabla 'carrito'
export interface ItemCarrito {
  id?: number;
  carritoId?: number;
  productoId: number;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
  producto?: Producto;
}

// Carrito completo
export interface Carrito {
  id?: number;
  usuarioId?: number;
  fechaCreacion?: Date;
  fechaActualizacion?: Date;
  items: ItemCarrito[];
  subtotal: number;
  impuestos: number;
  descuentos: number;
  total: number;
  activo?: boolean;
}