import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const authGuard: CanActivateFn = () =>
{
    const authService = inject(AuthService);
    const router = inject(Router);

    const token = authService.getToken();

    if (!token)
    {
        return router.parseUrl('/login');
    }

    if (authService.isTokenExpired())
    {
        authService.logout();
        return router.parseUrl('/not-found?reason=expired');
    }

    return true;
};