import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-clientes',
  templateUrl: './clientes.page.html',
  styleUrls: ['./clientes.page.scss'],
  imports: [IonicModule, CommonModule, FormsModule],
  standalone: true,
})
export class ClientesPage implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
