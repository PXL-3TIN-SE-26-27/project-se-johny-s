ADR 001: Database-aanroepen centraliseren via services en repositories

Datum: 17 november 2026

Status: voorgesteld

Auteurs: Joran

Context

In de oorspronkelijke structuur van CampusBook.Api werden database-aanroepen rechtstreeks vanuit controllers uitgevoerd. Daarnaast werden services en repositories soms manueel aangemaakt binnen controllers.

Dit zorgde voor een sterke koppeling tussen controllers, services en de database. Hierdoor werd de code moeilijker onderhoudbaar en waren onderdelen moeilijker afzonderlijk te testen.

Dit probleem kwam onder andere voor in:

ReservationsController

ResourcesController

UsersController

Overwogen opties

Database-aanroepen rechtstreeks in controllers behouden

Voordelen:

Eenvoudig en snel te implementeren.

Nadelen:

Controllers bevatten te veel verantwoordelijkheden.

Moeilijker om afzonderlijk te testen.

Sterke koppeling met de database.

Werken met services en repositories

Voordelen:

Duidelijke scheiding van verantwoordelijkheden.

Betere onderhoudbaarheid.

Services en repositories zijn afzonderlijk testbaar.

Dependency Injection maakt de code flexibeler.

Nadelen:

Meer klassen en interfaces nodig.

Aanvankelijk meer refactoringwerk.

Beslissing

We kiezen voor optie 2.

Controllers communiceren met services via Dependency Injection. Services bevatten de applicatielogica en gebruiken repositories voor databasebewerkingen. Repositories zijn verantwoordelijk voor de communicatie met CampusDbContext.

De gewenste structuur is:

Controller → Service → Repository → Database

Gevolgen

Controllers bevatten minder databasegerelateerde code.

De applicatielogica is beter gescheiden.

Code wordt eenvoudiger te onderhouden en te testen.

Er zijn extra services en repositories nodig.

We moeten erop letten dat services niet rechtstreeks databasebewerkingen uitvoeren wanneer die via repositories horen te verlopen.

Bestaande functionaliteit en tests moeten na de refactoring gecontroleerd worden.

Gerelateerd

Pull Request: Refactor database calls into services and repositories

Branch: refactor/db-to-service-calls