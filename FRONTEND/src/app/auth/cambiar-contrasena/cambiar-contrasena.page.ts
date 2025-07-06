import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-cambiar-contrasena',
  templateUrl: './cambiar-contrasena.page.html',
  styleUrls: ['./cambiar-contrasena.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class CambiarContrasenaPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
