# HomeBudget.Infrastructure.Server

Serwerowa infrastruktura aplikacji.

Zawiera konfigurację EF Core, PostgreSQL, implementacje repozytoriów agregatów, Unit of Work z dispatchowaniem domain events w transakcji oraz tabelę outbox dla późniejszego publikowania komunikatów integracyjnych.

Ten projekt jest używany przez `HomeBudget.Api` i nie powinien być referencjonowany przez klientów webowych ani MAUI.

Migracje, integracje OAuth oraz konfiguracja użytkowników i tenantów pozostają do dodania w kolejnych etapach.
