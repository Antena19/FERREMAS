import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';                // 📥 Capturar ID de la URL
import { ApiService } from 'src/app/services/api.service';      // 📡 Servicio para obtener el producto
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-detalle-producto',
  standalone: true,
  templateUrl: './detalle-producto.page.html',
  styleUrls: ['./detalle-producto.page.scss'],
  imports: [CommonModule, IonicModule, RouterModule]
})
export class DetalleProductoPage implements OnInit {
  producto: any = null;       // 🧩 Producto cargado
  cargando: boolean = true;   // ⏳ Spinner mientras carga

  constructor(
    private route: ActivatedRoute,
    private api: ApiService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.api.getProductoPorId(+id).subscribe({
        next: (res) => {
          this.producto = res;
          this.cargando = false;
        },
        error: () => {
          console.error('❌ Error al obtener producto');
          this.cargando = false;
        }
      });
    }
  }

  agregarAlCarrito(): void {
    console.log('🛒 Agregado al carrito:', this.producto?.nombre);
  }
}
  