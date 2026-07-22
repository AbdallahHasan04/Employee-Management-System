# Employee Management System
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

A full-stack web application designed to manage employee records seamlessly.

Features:
- Employees — full CRUD, assigned to a Department, auto-generated linked user account with a system-generated password on creation
- Departments — full CRUD, cannot be deleted while employees are still assigned to it (enforced server-side)
- Authentication — JWT-based, PBKDF2 password hashing, configurable token expiration
- Sessions stay active while in use, only a page refresh/navigation after expiry redirects to login
- Localization — English/Arabic toggle across the entire app, including full RTL layout mirroring (via ngx-translate + Angular CDK        Directionality)

Tech stack details:

Backend:
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core + Pomelo.EntityFrameworkCore.MySql
- JWT Bearer authentication
- PBKDF2 password hashing (built-in System.Security.Cryptography, no external packages)

Frontend:
-Angular (standalone components)
-Angular Material
-@ngx-translate/core + @ngx-translate/http-loader for i18n
-Angular CDK Directionality for reactive RTL support
