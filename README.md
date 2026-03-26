# FitraLife

FitraLife is an ASP.NET Core 8 fitness web app that helps users track health progress and get AI-assisted guidance.

## What the App Is About

FitraLife combines fitness tracking with personalized insights:

- User accounts and authentication using ASP.NET Core Identity.
- Profile data for age, weight, target weight, goals, and activity level.
- Fitness and meal logging.
- AI-generated workout and meal plans (via Gemini API).
- AI chat assistant with saved chat sessions/history.
- Analytics/prediction service for estimating progress toward target weight.

## Tech Stack

- .NET 8 (ASP.NET Core Razor Pages + API controllers)
- Entity Framework Core
- SQLite (local database)
- ASP.NET Core Identity
- MSTest + Moq for tests

## Prerequisites

Install the following:

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- (Optional) EF Core CLI tools:

dotnet tool install --global dotnet-ef

## Getting Started

From the repository root:

dotnet restore

## Configure Secrets (Gemini API Key)

The app expects a Gemini API key at `Gemini:ApiKey`.

### User Secrets

dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY"

## Database Setup

Apply migrations to create/update the local SQLite database:

dotnet ef database update

By default, the SQLite file is created as `fitralife.db` in the project root.

## Run the App

dotnet run
dotnet run --project FitraLife.csproj

Then open the URL shown in the terminal usually:

- `https://localhost:xxxx`
- `http://localhost:xxxx`

## Run Tests

Run all tests:

dotnet test

Run only the test project:

dotnet test FitraLife.Tests/FitraLife.Tests.csproj

## Project Structure (High Level)

- `Api/Controllers` - API endpoints (e.g., chat)
- `Areas/Identity` - Identity UI and auth pages
- `Data` - EF Core DbContext
- `Models` - Domain/entity models
- `Services` - Business logic and external integrations (Gemini, analytics)
- `Pages` - Razor Pages UI
- `Migrations` - EF Core migration history
- `FitraLife.Tests` - Unit tests
