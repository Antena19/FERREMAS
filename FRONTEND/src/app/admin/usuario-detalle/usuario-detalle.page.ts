import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-usuario-detalle',
  templateUrl: './usuario-detalle.page.html',
  styleUrls: ['./usuario-detalle.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class UsuarioDetallePage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
