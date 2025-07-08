import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pedidos',
  templateUrl: './pedidos.page.html',
  styleUrls: ['./pedidos.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,

})
export class PedidosPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
