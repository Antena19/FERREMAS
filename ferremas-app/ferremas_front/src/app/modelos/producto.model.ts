// Define la interfaz del producto basada en la tabla 'productos'
export interface Producto {
    id: number;
    codigo: string;
    nombre: string;
    descripcion: string;
    precio: number;
    categoriaId: number;
    marcaId: number;
    imagenUrl: string;
    especificaciones: string;
    fechaCreacion: Date;
    fechaModificacion: Date;
    activo: boolean;
}

export interface Categoria {
    id: number;
    nombre: string;
    descripcion: string;
    categoriaPadreId?: number;
    activo: boolean;
}

export interface Marca {
    id: number;
    nombre: string;
    descripcion: string;
    logoUrl: string;
    activo: boolean;
}

export interface ProductoDetalle extends Producto {
    categoria: Categoria;
    marca: Marca;
    stock: number;
}

// Para crear un nuevo producto (sin ID)
export interface ProductoCreate {
    codigo: string;
    nombre: string;
    descripcion: string;
    precio: number;
    categoriaId?: number;
    marcaId?: number;
    imagenUrl?: string;
    especificaciones?: string;
    activo?: boolean;
}

// Para actualizar un producto existente (todos los campos opcionales)
export interface ProductoUpdate {
    codigo?: string;
    nombre?: string;
    descripcion?: string;
    precio?: number;
    categoriaId?: number;
    marcaId?: number;
    imagenUrl?: string;
    especificaciones?: string;
    activo?: boolean;
}