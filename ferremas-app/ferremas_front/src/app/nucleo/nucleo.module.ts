import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { TokenInterceptor } from './interceptores/token.interceptor';
import { ErrorInterceptor } from './interceptores/error.interceptor';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
  ]
})
export class NucleoModule {
  constructor(@Optional() @SkipSelf() parentModule: NucleoModule) {
    if (parentModule) {
      throw new Error('NucleoModule ya está cargado. Importarlo solo en AppModule');
    }
  }
}