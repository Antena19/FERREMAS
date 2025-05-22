// Tipos para pagos
export type MetodoPago = 'tarjeta_debito' | 'tarjeta_credito' | 'transferencia' | 'mercadopago';
export type EstadoPago = 'pendiente' | 'completado' | 'fallido' | 'reembolsado';

// Define la interfaz del pago basada en la tabla 'pagos'
export interface Pago {
  id: number;
  pedidoId: number;
  metodo: MetodoPago;
  monto: number;
  estado: EstadoPago;
  fechaPago?: Date;
  
  // Campos específicos de Mercado Pago
  mercadoPagoPaymentId?: string;
  mercadoPagoPreferenceId?: string;
  mercadoPagoStatus?: string;
  mercadoPagoStatusDetail?: string;
  mercadoPagoPaymentMethodId?: string;
  mercadoPagoPaymentTypeId?: string;
  mercadoPagoInstallments?: number;
  mercadoPagoCardNumber?: string;
  
  referenciaTransaccion?: string;
  notas?: string;
  contadorId?: number;
}

// Para crear un nuevo pago
export interface PagoCreate {
  pedidoId: number;
  metodo: MetodoPago;
  urlRetorno?: string;
}

// Respuesta de Mercado Pago
export interface RespuestaMercadoPago {
  id: string;
  status: string;
  status_detail: string;
  external_reference: string;
  preference_id: string;
  payment_method_id: string;
  payment_type_id: string;
  transaction_amount: number;
  installments: number;
  card_number: string;
  init_point: string;
  sandbox_init_point: string;
}