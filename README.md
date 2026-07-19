# HomeBudget

Struktura rozwiązania dla aplikacji budżetu domowego w .NET 10.

Docelowo rozwiązanie ma mieć dwa klienty:

- `HomeBudget.Web` - Blazor Web App działający online.
- `HomeBudget.Maui` - .NET MAUI Blazor Hybrid dla Windows, macOS, Android i iOS.

Dane biznesowe mają być przechowywane wyłącznie po stronie serwera w PostgreSQL i udostępniane klientom przez `HomeBudget.Api`.

## Uruchamianie lokalne

API i webowy interfejs Blazor można uruchomić jedną komendą:

```bash
./scripts/dev.sh
```

Domyślne adresy:

- `HomeBudget.Api`: `http://localhost:5095`
- `HomeBudget.Web`: `http://localhost:5179`

Porty można nadpisać zmiennymi środowiskowymi:

```bash
HOMEBUDGET_API_URL=http://localhost:6095 HOMEBUDGET_WEB_URL=http://localhost:6179 ./scripts/dev.sh
```

Ten etap tworzy strukturę projektów i dokumentację odpowiedzialności. Projekty nie są jeszcze wypełnione klasami domenowymi, DTO, serwisami ani endpointami biznesowymi.
