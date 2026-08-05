import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable
({
    providedIn: 'root'
})

export class AuthService
{
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7038/api/auth';

    login(credentials: { username: string; password: string }): Observable<{ token: string; expiresAt: string }>
    {
        return this.http.post<{ token: string; expiresAt: string }>(`${this.apiUrl}/login`, credentials).pipe
        (
            tap(response =>
            {
                if(response && response.token)
                {
                    localStorage.setItem('jwt_token', response.token);
                    localStorage.setItem('jwt_expires_at', response.expiresAt);
                }
            })
        );
    }

    changePassword(payload: { currentPassword: string; newPassword: string }): Observable<any>
    {
        return this.http.post(`${this.apiUrl}/change-password`, payload);
    }

    getToken(): string | null
    {
        return localStorage.getItem('jwt_token');
    }

    logout(): void
    {
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('jwt_expires_at');
    }

    isTokenExpired(): boolean
    {
        const token = this.getToken();
        const expiresAt = localStorage.getItem('jwt_expires_at');
        if (!token || !expiresAt) return true;

        return Date.now() >= new Date(expiresAt).getTime();
    }
}