import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { OverviewComponent } from './app/overview.component';
import { RequestsPageComponent } from './app/requests-page.component';
import { ImportRepairCenterComponent } from './app/import-repair-center.component';
import { PlatformInspectorComponent } from './app/platform-inspector.component';
import { civicFlowAuthInterceptor } from './app/auth.interceptor';

const routes: Routes = [
  { path: '', component: OverviewComponent },
  { path: 'requests', component: RequestsPageComponent },
  { path: 'imports', component: ImportRepairCenterComponent },
  { path: 'platform', component: PlatformInspectorComponent },
  { path: '**', redirectTo: '' }
];

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(withInterceptors([civicFlowAuthInterceptor])),
    provideRouter(routes)
  ]
}).catch(error => console.error(error));
