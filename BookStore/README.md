# BookStore

BookStore is an ASP.NET Core MVC web application for browsing and managing books online. It supports catalog browsing, user authentication, shopping cart operations, checkout, and admin management for books, authors, genres, and orders.

## Features

- Browse books, authors, and genres
- View detailed book and author pages
- Register, log in, and log out
- Add books to a session-based shopping cart
- Checkout and place orders
- Admin CRUD for books, authors, genres, and orders
- Import book data from the Gutendex API
- Role-based access control with `Admin` and `User` roles

## Technology Stack

- ASP.NET Core MVC
- C#
- Entity Framework Core
- ASP.NET Identity
- SQLite / SQL Server
- Razor Views
- Bootstrap
- jQuery
- Session state and distributed memory cache

## Getting Started

1. Open the solution `BookStore.sln`.
2. Restore NuGet packages.
3. Update the connection string in `appsettings.json` if needed.
4. Run the application.

Example command line run:

```bash
dotnet restore
dotnet run --project BookStore/BookStore.csproj
```

## Notes

- The default route opens the books catalog.
- The application automatically applies migrations and seeds initial data on startup.
- Cart data is stored in session, not in the database.
- The app supports both SQLite and SQL Server depending on the configured connection string.

