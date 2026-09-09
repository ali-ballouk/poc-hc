import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { AuthService } from './auth.service';
import { modules } from './module-definitions';
export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService),
    router = inject(Router);
  return auth.load().pipe(
    map(() => {
      if (!auth.user()) return false;
      const roles = route.params['module']
        ? modules[route.params['module']]?.roles
        : route.data['roles'];
      return roles && auth.canRoles(roles)
        ? true
        : router.parseUrl(
            auth.can('Doctor') ? '/manage/appointments' : '/billing',
          );
    }),
    catchError(() => of(false)),
  );
};
