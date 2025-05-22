import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoriaService } from '../../../../app/servicios/categoria.service';
import { MarcaService } from '../../../../app/servicios/marca.service';
import { Categoria } from '../../../../app/modelos/categoria.model';
import { Marca } from '../../../../app/modelos/marca.model';

@Component({
  selector: 'app-filtros-productos',
  templateUrl: './filtros-productos.component.html',
  styleUrls: ['./filtros-productos.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class FiltrosProductosComponent implements OnInit {
  @Output() categoriaSeleccionada = new EventEmitter<number | null>();
  @Output() marcaSeleccionada = new EventEmitter<number | null>();
  @Output() busqueda = new EventEmitter<string>();

  categorias: Categoria[] = [];
  marcas: Marca[] = [];
  terminoBusqueda: string = '';
  categoriaId: number | null = null;
  marcaId: number | null = null;
  cargando: boolean = true;
  error: string = '';

  constructor(
    private categoriaService: CategoriaService,
    private marcaService: MarcaService
  ) { }

  ngOnInit(): void {
    this.cargarCategorias();
    this.cargarMarcas();
  }

  cargarCategorias(): void {
    this.categoriaService.obtenerTodas().subscribe({
      next: (data: Categoria[]) => {
        this.categorias = data;
        this.cargando = false;
      },
      error: (error: any) => {
        this.error = 'Error al cargar las categorías';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  cargarMarcas(): void {
    this.marcaService.obtenerTodas().subscribe({
      next: (data: Marca[]) => {
        this.marcas = data;
        this.cargando = false;
      },
      error: (error: any) => {
        this.error = 'Error al cargar las marcas';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  onCategoriaChange(): void {
    this.categoriaSeleccionada.emit(this.categoriaId);
  }

  onMarcaChange(): void {
    this.marcaSeleccionada.emit(this.marcaId);
  }

  onBuscar(): void {
    this.busqueda.emit(this.terminoBusqueda);
  }

  limpiarFiltros(): void {
    this.categoriaId = null;
    this.marcaId = null;
    this.terminoBusqueda = '';
    this.categoriaSeleccionada.emit(null);
    this.marcaSeleccionada.emit(null);
    this.busqueda.emit('');
  }
} 