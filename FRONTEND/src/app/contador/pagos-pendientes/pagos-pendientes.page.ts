import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-pagos-pendientes',
  templateUrl: './pagos-pendientes.page.html',
  styleUrls: ['./pagos-pendientes.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class PagosPendientesPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
