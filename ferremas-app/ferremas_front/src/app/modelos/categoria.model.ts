// Define la interfaz de la categoría basada en la tabla 'categorias'
export interface Categoria {
    id: number;
    nombre: string;
    descripcion?: string;
    categoriaPadreId?: number;
    activo: boolean;
  }