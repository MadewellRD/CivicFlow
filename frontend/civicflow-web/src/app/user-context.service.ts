import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

const STORAGE_KEY = 'civicflow:active-user-id';

/**
 * Holds the active demo user id. The interceptor reads this on every request
 * and forwards it as X-CivicFlow-User. In production the value would come from
 * an OIDC token exchange, not a header switch, but the rest of the app does
 * not care which source provided the identity.
 */
@Injectable({ providedIn: 'root' })
export class UserContextService {
  private readonly activeUserSubject: BehaviorSubject<string | null>;

  constructor() {
    const persisted = typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null;
    this.activeUserSubject = new BehaviorSubject<string | null>(persisted);
  }

  get activeUserId$(): Observable<string | null> {
    return this.activeUserSubject.asObservable();
  }

  get currentUserId(): string | null {
    return this.activeUserSubject.value;
  }

  setActiveUser(userId: string | null): void {
    this.activeUserSubject.next(userId);
    if (typeof localStorage !== 'undefined') {
      if (userId) {
        localStorage.setItem(STORAGE_KEY, userId);
      } else {
        localStorage.removeItem(STORAGE_KEY);
      }
    }
  }
}
