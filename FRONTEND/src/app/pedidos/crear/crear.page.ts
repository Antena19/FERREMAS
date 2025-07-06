import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-crear',
  templateUrl: './crear.page.html',
  styleUrls: ['./crear.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,

})
export class CrearPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
