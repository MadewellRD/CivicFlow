import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { RequestDashboardComponent } from './app/request-dashboard.component';
import { ImportRepairCenterComponent } from './app/import-repair-center.component';
import { civicFlowAuthInterceptor } from './app/auth.interceptor';

const routes: Routes = [
  { path: '', component: RequestDashboardComponent },
  { path: 'imports', component: ImportRepairCenterComponent }
];

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(withInterceptors([civicFlowAuthInterceptor])),
    provideRouter(routes)
  ]
}).catch(error => console.error(error));
