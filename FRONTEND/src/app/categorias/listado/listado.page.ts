import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-listado',
  templateUrl: './listado.page.html',
  styleUrls: ['./listado.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class ListadoPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
