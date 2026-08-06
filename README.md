# Employee Management System
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)

A full-stack web application for managing employee records, departments, positions, and position history, with bilingual English/Arabic support and full RTL layout mirroring.

## Features

**Employees**
- Full CRUD, assigned to a Department and a Position
- Auto-generated linked user account with a system-generated password shown once on creation
- Profile photo upload (JPEG/PNG/WEBP, 2MB max)
- Server-side search, sort, and pagination
- Status toggle (Active/Inactive) directly from the table
- Cannot be deleted without first checking for currently-open position assignments; the open position record is automatically closed out on deletion rather than lost

**Departments**
- Full CRUD, cannot be deleted while employees are still assigned to it (enforced server-side)
- Live employee count per department

**Positions**
- Full CRUD, cannot be deleted while currently assigned to one or more employees (enforced server-side)
- Live count of employees currently holding each position

**Position History**
- Every position change is tracked as a dated record (start date, end date) rather than overwritten
- Assigning a new position automatically closes out the employee's currently-open position record
- Full searchable, sortable, paginated history view

**Authentication & Security**
- JWT-based authentication with configurable token expiration
- PBKDF2 password hashing (built-in `System.Security.Cryptography`, no external packages)
- Login rate limiting: lockout after repeated failed attempts, with a persistent countdown that survives a page refresh
- Self-service change password (current password verification required, new password confirmed and validated)
- Sessions stay active while in use; only a page refresh or navigation after expiry redirects to login

**Validation**
- Required fields are marked with a red asterisk and show inline error text once touched
- Future dates are blocked (with both a visual `mat-error` and true server-facing enforcement) on birthdate and position assignment start date
- Phone numbers restricted to digits and standard phone symbols only
- Discard-changes confirmation when cancelling an in-progress edit with unsaved changes

**Localization**
- Full English/Arabic toggle across the entire app via `@ngx-translate/core`
- Complete RTL layout mirroring via Angular CDK Directionality, including dialogs (which otherwise default to LTR direction and stale-cache the direction from the first dialog opened in a session — explicitly worked around here)

**UX details**
- Reusable confirmation dialog (used for both delete confirmation and discard-unsaved-changes, with configurable label/color)
- Snackbar success/error notifications throughout
- Password visibility toggle on login and change-password forms
- Debounced live search on all list pages

## Architecture

**Backend** — ASP.NET Core Web API (.NET 10), 5-project clean architecture:
- `Core` — entities
- `Common` — DTOs, repository/service interfaces
- `Data` — EF Core `DbContext`, repositories
- `Infrastructure` — services, AutoMapper profile, password hashing, file storage
- `API` — controllers, JWT/Swagger/CORS configuration

**Frontend** — Angular (standalone components), Angular Material, `@ngx-translate` for i18n.

**Database** — MySQL via Pomelo.EntityFrameworkCore.MySql, `UPPER_SNAKE_CASE` column naming.

**Data integrity patterns used throughout:**
- Soft delete (`IsDeleted` flag) with EF Core global query filters on every entity
- Full audit trail (`CreatedBy`, `CreationDate`, `ModifiedBy`, `ModificationDate`) on every table
- `IgnoreQueryFilters()` with a manual `IsDeleted` check where soft delete and `Include()` would otherwise interact incorrectly (e.g. position history staying visible for soft-deleted employees)
- Transactional multi-step writes via a `IUnitOfWork`/`BeginTransactionAsync` pattern (e.g. employee creation, which links a `User` row and an `Employee` row and must not partially succeed)
- AutoMapper profile for entity↔DTO mapping, reserved for genuine transformations and mismatches (renames, defaults, nested navigation lookups) rather than 1:1 field copies, with explicit `.Ignore()` guards protecting audit fields and immutable fields (e.g. `Username`) from being overwritten by client input on update

## Tech stack

**Backend:**
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core + Pomelo.EntityFrameworkCore.MySql
- AutoMapper
- JWT Bearer authentication
- PBKDF2 password hashing (built-in `System.Security.Cryptography`)

**Frontend:**
- Angular (standalone components)
- Angular Material
- `@ngx-translate/core` + `@ngx-translate/http-loader` for i18n
- Angular CDK Directionality for reactive RTL support
