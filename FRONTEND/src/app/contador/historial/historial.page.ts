import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-historial',
  templateUrl: './historial.page.html',
  styleUrls: ['./historial.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class HistorialPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
