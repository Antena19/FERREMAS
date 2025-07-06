import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-transferencia',
  templateUrl: './transferencia.page.html',
  styleUrls: ['./transferencia.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})

export class TransferenciaPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
