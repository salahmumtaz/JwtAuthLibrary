# Todo API with JWT Authentication

A robust RESTful API for managing tasks and users, built with ASP.NET Core and integrating custom JWT authentication for secure access. This project demonstrates a clean architecture with separate concerns for data access, business logic, and authentication.

## Table of Contents

* [Features](#features)
* [Technologies Used](#technologies-used)
* [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
    * [Installation](#installation)
    * [Database Setup](#database-setup)
    * [Configuration](#configuration)
    * [Running the Application](#running-the-application)
* [API Endpoints](#api-endpoints)
* [Authentication & Authorization](#authentication--authorization)
* [Project Structure](#project-structure)
* [Contributing](#contributing)
* [License](#license)

## Features

* **User Management:** Register new users.
* **Secure Authentication:** User login with JWT (JSON Web Token) generation.
* **Token Refresh:** Renew access tokens using refresh tokens to maintain user sessions.
* **Task Management:** (Planned/Future Feature based on `Task.cs` model) Create, read, update, and delete tasks associated with users.
* **Role-Based Authorization:** (Future Expansion) Ability to define and enforce user roles.
* **Layered Architecture:** Clear separation between API controllers, services, and data access.
* **Custom JWT Library:** Utilizes a dedicated `JwtAuthLibrary` for JWT handling.
* **SQL Server Integration:** Data persistence using Entity Framework Core with SQL Server.

## Technologies Used

* **ASP.NET Core:** Web API Framework.
* **.NET 9**
* **Entity Framework Core:** ORM for database interactions.
* **SQL Server:** Relational database.
* **JSON Web Tokens (JWT):** For secure API authentication.
* **`JwtAuthLibrary` (Custom Library):** For streamlined JWT creation and management.
* **`Microsoft.AspNetCore.Identity` (PasswordHasher):** For secure password hashing.
* **Swagger/OpenAPI:** For API documentation and testing (usually implicitly added in ASP.NET Core API templates).

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express, Developer, or full version)
* [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (Optional, but recommended for database management)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/downloads/) (Recommended IDE) or VS Code.

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/YOUR_USERNAME/YOUR_REPOSITORY.git](https://github.com/YOUR_USERNAME/YOUR_REPOSITORY.git)
    cd YOUR_REPOSITORY/Todo_Api
    ```
    *(Replace `YOUR_USERNAME` and `YOUR_REPOSITORY` with your actual GitHub username and repository name.)*

2.  **Restore NuGet packages:**
    ```bash
    dotnet restore
    ```

3.  **Build the solution:**
    ```bash
    dotnet build
    ```

### Database Setup

This project uses SQL Server and relies on a stored procedure for user registration.

1.  **Create a new SQL Server Database:**
    Open SQL Server Management Studio (SSMS) or use `sqlcmd` to create a new database. For example, name it `TodoAppDb`.

2.  **Update Connection String:**
    Open `appsettings.json` (or `appsettings.Development.json`) in the `Todo_Api` project and update the `DefaultConnection` string to point to your SQL Server instance and the newly created database.

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TodoAppDb;Integrated Security=True;TrustServerCertificate=True;"
        // Or use SQL Server Authentication:
        // "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TodoAppDb;User ID=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
      },
      // ... rest of appsettings.json
    }
    ```
    *Replace `YOUR_SERVER_NAME` with your SQL Server instance name.*

3.  **Run Migrations:**
    This will create the `Users` and `Tasks` tables (and other EF Core infrastructure) in your database.
    Navigate to the `Todo_Api` project directory in your terminal:
    ```bash
    cd Todo_Api # Make sure you are in the project folder with the .csproj file
    dotnet ef database update
    ```

4.  **Create the `SP_AddNewUser` Stored Procedure:**
    The `UserService` uses a stored procedure for adding new users. Execute the following SQL script in your `TodoAppDb` database:

    ```sql
    -- SP_AddNewUser.sql
    CREATE PROCEDURE SP_AddNewUser
        @Username NVARCHAR(100),
        @Email NVARCHAR(255),
        @Password NVARCHAR(255), -- This should be the HASHED password
        @Role TINYINT,
        @NewUserID UNIQUEIDENTIFIER OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;

        -- Check if user with this email already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            RAISERROR ('User with this email already exists.', 16, 1);
            RETURN;
        END

        SET @NewUserID = NEWID(); -- Generate a new GUID for the user ID

        INSERT INTO Users (ID, Username, Email, Password, Role, RefreshToken, RefreshTokenExpiry)
        VALUES (@NewUserID, @Username, @Email, @Password, @Role, NULL, GETDATE()); -- RefreshToken and Expiry are NULL initially

        SELECT @NewUserID; -- Return the newly generated ID
    END;
    ```
    *You can execute this script using SSMS by connecting to your database, right-clicking on `Databases` -> `TodoAppDb` -> `New Query`, pasting the script, and executing it.*

### Configuration

Open `appsettings.json` (or `appsettings.Development.json`) in the `Todo_Api` project and configure the JWT settings:

```json
{
  "Jwt": {
    "Key": "YOUR_SUPER_SECURE_JWT_KEY_MINIMUM_256_BITS", // This MUST be a strong, random key (e.g., 32+ characters)
    "Issuer": "[https://yourdomain.com](https://yourdomain.com)", // Replace with your API's domain or a suitable identifier
    "Audience": "[https://yourdomain.com](https://yourdomain.com)", // Replace with your API's domain or a suitable identifier
    "AccessTokenExpiryMinutes": 15, // Suggested: 5-15 minutes
    "RefreshTokenExpiryDays": 7    // Suggested: 7-30 days
  },
  // ... other configurations
}
