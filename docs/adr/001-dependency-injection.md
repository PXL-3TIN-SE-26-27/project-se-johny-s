# ADR 001: Dependency injection voor ReservationService en zijn afhankelijkheden

- **Datum:** 2026-11-17
- **Status:** voorgesteld
- **Auteurs:** Manuel Bormans

## Context

De controllers maken hun services zelf aan. In `ReservationsController` staat in
bijna elke actie dezelfde regel:

```csharp
var service = new ReservationService(new ReservationRepository(_db), _db);
```

Hetzelfde patroon zit in `ResourcesController` (`ReservationService`),
`UsersController` (`new StatsService(new ReservationRepository(_db))`) en in de
export-actie van `ReservationsController` (`new ExportService(...)`).
`ReservationService` maakt op zijn beurt intern `new PriceCalculator()` aan.

Dat geeft een aantal problemen:

- **Sterke koppeling.** De controller moet weten hoe een `ReservationService`
  gebouwd wordt, inclusief welke repository en welke `CampusDbContext`. Elke
  wijziging aan de constructor van de service raakt alle controllers die hem
  aanmaken.
- **Duplicatie.** De aanmaakcode staat negen keer in `ReservationsController`
  alleen al.
- **Moeilijk testbaar.** `PriceCalculator` wordt binnen de service aangemaakt,
  dus een test kan die niet vervangen. De controllers kunnen niet getest worden
  zonder een echte database, omdat ze hun eigen service bouwen.
- **ASP.NET Core heeft al een DI-container** (`builder.Services`) die we enkel
  gebruiken voor `CampusDbContext`. We gebruiken het framework maar half.

Daarnaast maakt `ReservationService` ook `SmtpMailer`, `FileAuditLogger` en
`LegacyPayGateway` rechtstreeks met `new` aan, en leest hij
`AppConfig.Instance` (singleton). Die vallen buiten deze ADR (zie Gevolgen).

## Overwogen opties

1. **Status quo:** services blijven met `new` aangemaakt in de controllers.
   - Voordelen: geen werk; alles is expliciet zichtbaar op de plek waar het
     gebruikt wordt.
   - Nadelen: alle problemen uit de context blijven; elke nieuwe afhankelijkheid
     van `ReservationService` betekent opnieuw alle controllers aanpassen.

2. **Handmatige DI / factory:** één `ServiceFactory` (of een private methode
   per controller) die de objectgraaf opbouwt, bv.
   `factory.CreateReservationService(db)`.
   - Voordelen: duplicatie verdwijnt; aanmaaklogica staat op één plek; geen
     kennis van een container nodig.
   - Nadelen: we bouwen zelf na wat ASP.NET Core al biedt; levensduur (per
     request, singleton, ...) moeten we zelf beheren; controllers blijven
     afhankelijk van de factory en zijn dus nog steeds niet los te testen.

3. **Overschakelen naar de ingebouwde DI-container van ASP.NET Core:**
   services registreren in `Program.cs` en via constructor injection laten
   aanleveren aan controllers en services.

   ```csharp
   builder.Services.AddSingleton<PriceCalculator>();
   builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
   builder.Services.AddScoped<ReservationService>();
   ```

   ```csharp
   public class ReservationsController(ReservationService service, CampusDbContext db)
       : ControllerBase { ... }

   public class ReservationService(CampusDbContext db, IReservationRepository repo,
       PriceCalculator calc) { ... }
   ```

   - Voordelen: standaardaanpak in ASP.NET Core, geen extra libraries;
     controllers en services kennen enkel hun afhankelijkheden, niet hoe die
     gebouwd worden; afhankelijkheden zijn vervangbaar in tests (bv. een fake
     `IReservationRepository` of `PriceCalculator`); levensduur wordt centraal
     en expliciet geregeld in `Program.cs`.
   - Nadelen: de objectgraaf is minder zichtbaar ("waar komt dit vandaan?");
     fouten in registratie of levensduur (bv. een scoped `DbContext` in een
     singleton) verschijnen pas at runtime; teamleden moeten de lifetimes
     begrijpen.

4. **Service locator:** controllers vragen services zelf op via
   `HttpContext.RequestServices.GetService<ReservationService>()`.
   - Voordelen: gebruikt wel de container; kleine wijziging per actie.
   - Nadelen: afhankelijkheden zijn verborgen (niet zichtbaar in de
     constructor); algemeen beschouwd als anti-pattern; testen blijft lastig.

## Beslissing

We kiezen **optie 3: overschakelen naar de ingebouwde DI-container met
constructor injection.**

Het framework biedt dit al aan en we gebruiken het al voor `CampusDbContext`,
dus het is de kleinste stap met het grootste effect. Optie 2 lost de
duplicatie op maar bouwt de container slechter na, en houdt de controllers
afhankelijk van een concrete factory. Optie 4 verbergt de afhankelijkheden in
plaats van ze expliciet te maken.

Afspraken over levensduur:

| Type | Lifetime | Waarom |
| --- | --- | --- |
| `CampusDbContext` | Scoped (standaard van `AddDbContext`) | één context per HTTP-request |
| `IReservationRepository` → `ReservationRepository` | Scoped | hangt af van de scoped `DbContext` |
| `ReservationService` | Scoped | hangt af van `DbContext` en repository |
| `PriceCalculator` | Singleton | stateless, geen afhankelijkheden |

We kiezen bewust voor **Scoped** in plaats van Transient voor repository en
service: zo delen alle objecten binnen één request dezelfde `DbContext` en
bestaat er per request maar één instantie.

`StatsService` en `ExportService` volgen hetzelfde patroon in een volgende
stap.

### Schrijfwijze: primary constructors

Geïnjecteerde afhankelijkheden declareren we met een **primary constructor**
(C# 12), zoals in `ReservationService` en `ReservationsController`, en niet met
een aparte constructor en `private readonly`-velden.

Niet meer zo:

```csharp
public class ReservationService
{
    private readonly IReservationRepository _repo;
    private readonly CampusDbContext _db;

    public ReservationService(IReservationRepository repo, CampusDbContext db)
    {
        _repo = repo;
        _db = db;
    }
}
```

Wel zo:

```csharp
public class ReservationService(CampusDbContext db, IReservationRepository repo,
    PriceCalculator calc)
{
    // parameters db, repo en calc zijn overal in de klasse bruikbaar
}
```

Waarom: de klasse toont in één regel welke afhankelijkheden ze nodig heeft, en
de boilerplate van velden toewijzen in de constructor verdwijnt. De
parameternamen gebruiken we rechtstreeks (`repo`, `db`), zonder `_`-prefix.

## Gevolgen

- **Makkelijker:**
  - Een nieuwe afhankelijkheid toevoegen aan `ReservationService` vraagt enkel
    een extra constructorparameter en (indien nodig) één registratie in
    `Program.cs`; de controllers blijven ongewijzigd.
  - Unit tests kunnen `ReservationService` opbouwen met een fake
    repository of calculator, en controllers kunnen getest worden met een
    gemockte service.
  - De negen dubbele `new ReservationService(...)`-regels verdwijnen.
- **Moeilijker:**
  - Wie de code leest, moet in `Program.cs` kijken om te weten welke
    implementatie gebruikt wordt.
  - Een vergeten registratie geeft pas bij het opstarten of bij het eerste
    request een fout, niet bij het compileren.
- **In de gaten houden:**
  - Geen scoped of transient service (zoals `CampusDbContext`) injecteren in
    een singleton (captive dependency). Zet in Development
    `ValidateScopes`/`ValidateOnBuild` aan om dit vroeg te detecteren.
  - Alle plaatsen die nog `new ReservationService(...)` gebruiken
    (`ResourcesController`, `ReservationTests`) moeten mee omgezet worden,
    anders compileert de oplossing niet.
  - `SmtpMailer`, `FileAuditLogger`, `LegacyPayGateway` en `AppConfig.Instance`
    worden nog rechtstreeks aangemaakt. Die achter een interface
    (`IMailer`, `IAuditLogger`, `IPaymentGateway`) en `IOptions<T>` zetten is
    de logische volgende stap, maar krijgt een eigen ADR.
  - Parameters van een primary constructor zijn **niet `readonly`**: de
    compiler houdt niet tegen dat iemand `repo = ...` schrijft in een methode.
    Afspraak: we wijzen deze parameters nooit opnieuw toe. Een klasse waarbij
    dat echt moet gegarandeerd zijn, mag een expliciet `readonly`-veld
    gebruiken (`private readonly IReservationRepository _repo = repo;`).
  - `ReservationService` krijgt zowel `CampusDbContext` als
    `IReservationRepository` binnen. Op termijn zou de service enkel via de
    repository met de database mogen praten.

## Gerelateerd

- Branch `refactor/dependency-injection`
- [ADR 000: template](000-template.md)
- Microsoft docs: [Dependency injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
