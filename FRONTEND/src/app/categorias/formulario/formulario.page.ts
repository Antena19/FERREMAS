import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-formulario',
  templateUrl: './formulario.page.html',
  styleUrls: ['./formulario.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class FormularioPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
