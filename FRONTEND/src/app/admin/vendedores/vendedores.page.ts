import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-vendedores',
  templateUrl: './vendedores.page.html',
  styleUrls: ['./vendedores.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class VendedoresPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
