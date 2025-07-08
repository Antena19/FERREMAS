import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-usuarios',
  templateUrl: './usuarios.page.html',
  styleUrls: ['./usuarios.page.scss'],
   imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class UsuariosPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
