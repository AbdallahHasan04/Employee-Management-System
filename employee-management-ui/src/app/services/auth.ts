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

    login(credentials: { username: string; password: string }): Observable<{ token: string }>
    {
        return this.http.post<{ token: string }>(`${this.apiUrl}/login`, credentials).pipe
        (
            tap(response =>
            {
                if(response && response.token)
                {
                    localStorage.setItem('jwt_token', response.token);
                }
            })
        );
    }

    getToken(): string | null
    {
        return localStorage.getItem('jwt_token');
    }

    logout(): void
    {
        localStorage.removeItem('jwt_token');
    }
}