import { HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { UserContextService } from './user-context.service';

/**
 * Stamps every outbound API call with X-CivicFlow-User so the backend
 * DemoAuthenticationHandler can resolve the seeded user and attach their role
 * claim. Production deployments would swap this interceptor for one that
 * forwards a bearer token from an OIDC provider.
 */
export const civicFlowAuthInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const userContext = inject(UserContextService);
  const userId = userContext.currentUserId;
  if (!userId) {
    return next(req);
  }
  const cloned = req.clone({ setHeaders: { 'X-CivicFlow-User': userId } });
  return next(cloned);
};
