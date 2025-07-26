# JwtAuthLibrary

A .NET Standard library designed to provide essential and reusable functionalities for handling JSON Web Tokens (JWT) in your applications. This library aims to simplify the process of creating Access Tokens and Refresh Tokens, as well as providing the necessary tools for validating tokens and extracting information from them.

## Table of Contents

* [Features](#features)
* [Installation](#installation)
* [Configuration](#configuration)
* [How to Use](#how-to-use)
    * [Registering Services in Program.cs (for ASP.NET Core projects)](#1-registering-services-in-programcs-for-aspnet-core-projects)
    * [Using IJwtAuthService](#2-using-ijwtauthservice)
    * [Used DTOs](#3-used-dtos)
* [Core Concepts](#core-concepts)
* [Important Security Considerations](#important-security-considerations)
* [License](#license)

## Features

* **Access Token Creation:** Generate signed Access Tokens based on user claims and JWT settings.
* **Refresh Token Generation:** Generate strong, cryptographically random Refresh Tokens.
* **Claims Extraction:** Ability to extract claims from an expired (but validly signed) Access Token to facilitate the token refresh process.
* **Easy Configuration:** Provides an Extension Method to simplify the setup of JWT services within the Dependency Injection system in ASP.NET Core.
* **Separation of Concerns:** Separates token creation logic from your application's core business logic and database interactions.

## Installation

To add the library to your project, you can use the NuGet Package Manager.

**Via NuGet Package Manager Console:**

```powershell
Install-Package JwtAuthLibrary
