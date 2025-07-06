import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-actualizar-producto',
  templateUrl: './actualizar-producto.page.html',
  styleUrls: ['./actualizar-producto.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class ActualizarProductoPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
