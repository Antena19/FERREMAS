// Define la interfaz de la marca basada en la tabla 'marcas'
export interface Marca {
    id: number;
    nombre: string;
    descripcion?: string;
    imagenUrl?: string;
    activa: boolean;
  }