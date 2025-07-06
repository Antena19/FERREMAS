import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contadores',
  templateUrl: './contadores.page.html',
  styleUrls: ['./contadores.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class ContadoresPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
