import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventosService {
  private loginSubject = new Subject<void>();
  public login$ = this.loginSubject.asObservable();

  notificarLogin(): void {
    this.loginSubject.next();
  }
} 