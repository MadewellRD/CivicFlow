import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { RequestDashboardComponent } from './app/request-dashboard.component';
import { ImportRepairCenterComponent } from './app/import-repair-center.component';

const routes: Routes = [
  { path: '', component: RequestDashboardComponent },
  { path: 'imports', component: ImportRepairCenterComponent }
];

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(), provideRouter(routes)]
}).catch(error => console.error(error));
