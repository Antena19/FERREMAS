import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-inventario',
  templateUrl: './inventario.page.html',
  styleUrls: ['./inventario.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class InventarioPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
