import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-bodegueros',
  templateUrl: './bodegueros.page.html',
  styleUrls: ['./bodegueros.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class BodeguerosPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
