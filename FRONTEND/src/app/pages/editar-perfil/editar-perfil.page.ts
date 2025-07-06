import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { ApiService } from 'src/app/services/api.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { 
  IonInput, 
  IonLabel, 
  IonItem, 
  IonButton, 
  IonCard, 
  IonCardHeader, 
  IonCardTitle, 
  IonCardContent,
  IonSelect,
  IonSelectOption,
  IonIcon,
  IonList,
  IonThumbnail,
  IonNote,
  IonBadge
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-editar-perfil',
  templateUrl: './editar-perfil.page.html',
  styleUrls: ['./editar-perfil.page.scss'],
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    IonInput, 
    IonLabel, 
    IonItem, 
    IonButton, 
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonSelect,
    IonSelectOption,
    IonIcon,
    IonList,
    IonThumbnail,
    IonNote,
    IonBadge
  ]
})
export class EditarPerfilPage implements OnInit {
  
  // 📦 Datos del cliente
  cliente: any = {
    nombre: '',
    apellido: '',
    correoElectronico: '',
    telefono: '',
    rut: '',
    tipoCliente: 'particular',
    numeroCompras: 0,
    totalCompras: 0
  };

  // 📊 Historial de pedidos
  pedidos: any[] = [];

  // 🏠 Direcciones de envío
  direcciones: any[] = [];

  // 📨 Mensajes de confirmación
  mensaje: string = '';
  tipoMensaje: 'exito' | 'error' = 'exito';

  mostrarModalDireccion: boolean = false;
  direccionEditando: any = null;
  nuevaDireccion: any = {
    calle: '',
    numero: '',
    departamento: '',
    comuna: '',
    region: '',
    codigoPostal: ''
  };

  constructor(
    private api: ApiService, 
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarDatosPerfil();
  }

  /**
   * 📥 Carga todos los datos del perfil del usuario
   */
  cargarDatosPerfil(): void {
    // 🔄 Cargar perfil completo del usuario autenticado
    this.api.getMiPerfil().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const perfil = response.data;
          
          // 📦 Datos personales y contacto
          this.cliente = {
            nombre: perfil.nombre,
            apellido: perfil.apellido,
            correoElectronico: perfil.email,
            telefono: perfil.telefono,
            rut: perfil.rut,
            tipoCliente: perfil.tipoCliente || 'particular',
            numeroCompras: perfil.numeroCompras || 0,
            totalCompras: perfil.totalCompras || 0,
            ultimaCompra: perfil.ultimaCompra,
            newsletter: perfil.newsletter || false
          };

          // 🏠 Direcciones de envío (primero)
          this.direcciones = perfil.direcciones || [];

          // 📊 Historial de compras (segundo)
          this.pedidos = perfil.historialCompras || [];

          console.log('✅ Perfil cargado correctamente:', perfil);
        } else {
          this.mostrarMensaje(response.message || 'Error al cargar perfil', 'error');
        }
      },
      error: (err) => {
        console.error('❌ Error al cargar perfil:', err);
        this.mostrarMensaje('Error al cargar datos del perfil', 'error');
      }
    });
  }

  /**
   * 💾 Guarda los datos personales del cliente
   */
  guardarDatosPersonales(): void {
    const datosPersonales = {
      nombre: this.cliente.nombre,
      apellido: this.cliente.apellido,
      telefono: this.cliente.telefono,
      tipoCliente: 'particular'
    };

    this.api.actualizarDatosPersonales(datosPersonales).subscribe({
      next: (response) => {
        if (response.success) {
          this.mostrarMensaje('✅ Datos personales actualizados correctamente', 'exito');
          this.cargarDatosPerfil(); // Recargar datos
        } else {
          this.mostrarMensaje(response.message || 'Error al actualizar datos personales', 'error');
        }
      },
      error: (err) => {
        console.error('❌ Error al actualizar datos personales:', err);
        this.mostrarMensaje('Error al actualizar datos personales', 'error');
      }
    });
  }

  /**
   * 👁️ Ver detalle de un pedido específico
   */
  verDetallePedido(idPedido: number): void {
    this.router.navigate(['/detalle-pedido', idPedido]);
  }

  /**
   * ➕ Muestra modal para agregar nueva dirección
   */
  abrirModalDireccion(): void {
    this.direccionEditando = null;
    this.nuevaDireccion = {
      calle: '',
      numero: '',
      departamento: '',
      comuna: '',
      region: '',
      codigoPostal: ''
    };
    this.mostrarModalDireccion = true;
  }

  /**
   * ✏️ Editar dirección existente
   */
  editarDireccion(direccion: any): void {
    this.direccionEditando = direccion;
    this.nuevaDireccion = {
      calle: direccion.calle,
      numero: direccion.numero,
      departamento: direccion.departamento || '',
      comuna: direccion.comuna,
      region: direccion.region,
      codigoPostal: direccion.codigoPostal || ''
    };
    this.mostrarModalDireccion = true;
  }

  /**
   * ❌ Cerrar modal de direcciones
   */
  cerrarModalDireccion(): void {
    this.mostrarModalDireccion = false;
    this.direccionEditando = null;
    this.nuevaDireccion = {
      calle: '',
      numero: '',
      departamento: '',
      comuna: '',
      region: '',
      codigoPostal: ''
    };
  }

  /**
   * 💾 Guardar dirección (crear o actualizar)
   */
  guardarDireccion(): void {
    if (!this.nuevaDireccion.calle || !this.nuevaDireccion.numero || !this.nuevaDireccion.comuna || !this.nuevaDireccion.region) {
      this.mostrarMensaje('Por favor completa todos los campos obligatorios', 'error');
      return;
    }

    // Siempre marcar como principal
    this.nuevaDireccion.esPrincipal = true;

    if (this.direccionEditando) {
      // Actualizar dirección existente
      this.api.actualizarDireccionPerfil(this.direccionEditando.id, this.nuevaDireccion).subscribe({
        next: (response) => {
          if (response.success) {
            this.mostrarMensaje('✅ Dirección actualizada correctamente', 'exito');
            this.cerrarModalDireccion();
            this.cargarDatosPerfil();
          } else {
            this.mostrarMensaje(response.message || 'Error al actualizar dirección', 'error');
          }
        },
        error: (err) => {
          console.error('❌ Error al actualizar dirección:', err);
          this.mostrarMensaje('Error al actualizar dirección', 'error');
        }
      });
    } else {
      // Crear nueva dirección
      this.api.agregarDireccionPerfil(this.nuevaDireccion).subscribe({
        next: (response) => {
          if (response.success) {
            this.mostrarMensaje('✅ Dirección agregada correctamente', 'exito');
            this.cerrarModalDireccion();
            this.cargarDatosPerfil();
          } else {
            this.mostrarMensaje(response.message || 'Error al agregar dirección', 'error');
          }
        },
        error: (err) => {
          console.error('❌ Error al agregar dirección:', err);
          this.mostrarMensaje('Error al agregar dirección', 'error');
        }
      });
    }
  }

  /**
   * 🗑️ Eliminar dirección
   */
  eliminarDireccion(id: number): void {
    if (confirm('¿Estás seguro de que quieres eliminar esta dirección?')) {
      this.api.eliminarDireccionPerfil(id).subscribe({
        next: (response) => {
          if (response.success) {
            this.mostrarMensaje('✅ Dirección eliminada correctamente', 'exito');
            this.cerrarModalDireccion();
            this.cargarDatosPerfil();
            this.mostrarModalDireccion = false; // Mostrar formulario si no hay dirección
          } else {
            this.mostrarMensaje(response.message || 'Error al eliminar dirección', 'error');
          }
        },
        error: (err) => {
          console.error('❌ Error al eliminar dirección:', err);
          this.mostrarMensaje('Error al eliminar dirección', 'error');
        }
      });
    }
  }

  /**
   * 📨 Muestra mensajes de confirmación o error
   */
  mostrarMensaje(texto: string, tipo: 'exito' | 'error'): void {
    this.mensaje = texto;
    this.tipoMensaje = tipo;
    
    // 🔄 Auto-ocultar mensaje después de 3 segundos
    setTimeout(() => {
      this.mensaje = '';
    }, 3000);
  }

  irAProductos(): void {
    this.router.navigate(['/home']);
  }

  cancelarEdicionDireccion(): void {
    this.direccionEditando = null;
    this.nuevaDireccion = {
      calle: '',
      numero: '',
      departamento: '',
      comuna: '',
      region: '',
      codigoPostal: ''
    };
    this.mostrarModalDireccion = false;
  }
}
