import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-detalle-pago',
  templateUrl: './detalle-pago.page.html',
  styleUrls: ['./detalle-pago.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class DetallePagoPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
