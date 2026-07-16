# HomeBudget.Application

Warstwa przypadków użycia i logiki aplikacyjnej.

Docelowo tutaj powinny trafić serwisy aplikacyjne, walidacje, porty/interfejsy dla repozytoriów, logika raportów, budżetowania oraz autoryzacja operacji domenowych.

Ten projekt może zależeć od `HomeBudget.Domain` i `HomeBudget.Contracts`, ale nie powinien zależeć od EF Core, PostgreSQL, Blazora ani ASP.NET Core.

Na tym etapie projekt zawiera bazowe abstrakcje CQRS dla komend, zapytań i ich handlerów.
