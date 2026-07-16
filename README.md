# HomeBudget

Struktura rozwiązania dla aplikacji budżetu domowego w .NET 10.

Docelowo rozwiązanie ma mieć dwa klienty:

- `HomeBudget.Web` - Blazor Web App działający online.
- `HomeBudget.Maui` - .NET MAUI Blazor Hybrid dla Windows, macOS, Android i iOS.

Dane biznesowe mają być przechowywane wyłącznie po stronie serwera w PostgreSQL i udostępniane klientom przez `HomeBudget.Api`.

Ten etap tworzy strukturę projektów i dokumentację odpowiedzialności. Projekty nie są jeszcze wypełnione klasami domenowymi, DTO, serwisami ani endpointami biznesowymi.
