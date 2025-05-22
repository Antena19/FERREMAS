// src/app/publico/catalogo/filtros-producto/filtros-producto.component.ts
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Categoria } from '../../../modelos/categoria.model';

@Component({
  selector: 'app-filtros-producto',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './filtros-producto.component.html',
  styleUrls: ['./filtros-producto.component.css']
})
export class FiltrosProductoComponent implements OnInit {
  @Input() categorias: Categoria[] = []; // Asegúrate de que esté correctamente definido como Input
  @Output() filtrosAplicados = new EventEmitter<any>();

  termino = '';
  categoriaId?: number;
  precioMin?: number;
  precioMax?: number;

  constructor() {}

  ngOnInit(): void {}

  aplicarFiltros(): void {
    this.filtrosAplicados.emit({
      termino: this.termino,
      categoriaId: this.categoriaId,
      precioMin: this.precioMin,
      precioMax: this.precioMax
    });
  }

  limpiarFiltros(): void {
    this.termino = '';
    this.categoriaId = undefined;
    this.precioMin = undefined;
    this.precioMax = undefined;
    this.aplicarFiltros();
  }
}