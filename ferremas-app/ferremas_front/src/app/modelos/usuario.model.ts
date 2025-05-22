// Define los tipos de roles basados en la tabla 'usuarios'
export type RolUsuario = 'cliente' | 'administrador' | 'vendedor' | 'bodeguero' | 'contador';

// Interfaz para el usuario
export interface Usuario {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  rut: string;
  telefono: string;
  rol: string;
  fechaRegistro: Date;
  ultimoAcceso?: Date;
  activo: boolean;
}

// Para login
export interface LoginRequest {
  email: string;
  password: string;
}

// Respuesta del login
export interface LoginResponse {
  token: string;
  usuario: Usuario;
}

export interface RegistroRequest {
  nombre: string;
  apellido: string;
  email: string;
  rut: string;
  telefono: string;
  password: string;
  confirmarPassword: string;
}

export interface RecuperacionContrasenaRequest {
  email: string;
}

export interface CambioContrasenaRequest {
  token: string;
  nuevaPassword: string;
  confirmarPassword: string;
}