# ADR 005: Globale exception handler in plaats van try/catch in controllers

- **Datum:** 2026-11-25
- **Status:** voorgesteld
- **Auteurs:** Manuel Bormans

## Context

`ReservationService` meldt fouten door exceptions te gooien:
`ArgumentException` voor ongeldige invoer, `KeyNotFoundException` als een
gebruiker, resource of reservering niet bestaat en `InvalidOperationException`
voor een ongeldige statusovergang of een mislukte betaling.

`ReservationsController` vertaalde die exceptions in elke actie zelf naar een
HTTP-statuscode. `Create`, `Confirm`, `CheckOut`, `Return`, `Cancel` en `Pay`
hadden elk een eigen `try`-blok met twee of drie `catch`-blokken die bijna
identiek waren:

```csharp
catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
catch (InvalidOperationException ex) { return Conflict(ex.Message); }
```

Dat gaf deze problemen:

- **Duplicatie.** Dezelfde mapping van exception naar statuscode stond zes keer
  in de controller. Een wijziging moest op elke plek gebeuren.
- **Inconsistentie.** Elke actie ving alleen de exceptions op waaraan de
  auteur gedacht had. `Confirm` ving bijvoorbeeld geen `ArgumentException` op.
  Een exception die niet expliciet opgevangen werd (zoals de
  `NotSupportedException` uit `ArchivedResource` of de
  `InvalidOperationException` uit `PriceCalculator`) eindigde als een kale 500
  met de standaard foutpagina.
- **Ruis.** De eigenlijke logica van een actie (de service aanroepen en het
  resultaat teruggeven) zat verstopt tussen de foutafhandeling.
- **Geen vast responseformaat.** Fouten kwamen terug als platte tekst, een
  onverwachte fout als HTML.

## Overwogen opties

1. **Try/catch in de controllers laten.**
   - Voordelen: geen wijziging en per actie expliciet zichtbaar.
   - Nadelen: alle bovenstaande problemen blijven, en elke nieuwe actie moet
     het patroon opnieuw kopiëren.
2. **Een MVC exception filter (`IExceptionFilter`).**
   - Voordelen: centraal en werkt met de MVC-pipeline.
   - Nadelen: vangt alleen exceptions binnen MVC-acties op, niet die uit
     middleware of minimal API-endpoints zoals `MapGet("/")`.
3. **Zelfgeschreven exception middleware.**
   - Voordelen: vangt alles op in de hele pipeline.
   - Nadelen: we bouwen zelf na wat het framework al aanbiedt (response
     schrijven, logging, controleren of de response al gestart is).
4. **De ingebouwde `IExceptionHandler` met `UseExceptionHandler` en
   `ProblemDetails`.**
   - Voordelen: het standaardmechanisme van ASP.NET Core (sinds .NET 8).
     Het vangt alles in de pipeline op, geeft responses in het
     RFC 9457-formaat (`application/problem+json`) en is te registreren en te
     testen als gewone klasse via DI.
   - Nadelen: de mapping van exception naar statuscode zit niet meer in de
     controller zelf, dus wie de controller leest ziet niet meteen welke
     fouten mogelijk zijn.

## Beslissing

We kiezen **optie 4**:

- `Middleware/GlobalExceptionHandler` implementeert `IExceptionHandler` en
  bepaalt de statuscode op basis van het type exception:

  | Exception                   | Status                    |
  | --------------------------- | ------------------------- |
  | `ArgumentException`         | 400 Bad Request           |
  | `KeyNotFoundException`      | 404 Not Found             |
  | `InvalidOperationException` | 409 Conflict              |
  | `NotSupportedException`     | 409 Conflict              |
  | alle andere                 | 500 Internal Server Error |

- De response is een `ProblemDetails` met de foutboodschap in `title`. Bij een
  500 sturen we een algemene boodschap en loggen we de exception, zodat er
  geen interne details naar de client lekken.
- `Program.cs` registreert `AddProblemDetails()` en
  `AddExceptionHandler<GlobalExceptionHandler>()`, en zet
  `app.UseExceptionHandler()` vooraan in de pipeline.
- De try/catch-blokken in `ReservationsController` zijn verwijderd. De
  eenvoudige invoercontroles die al vóór de service-aanroep stonden
  (`return BadRequest(...)`) blijven staan, want dat is gewone validatie en
  geen exceptionafhandeling.

De mapping is dezelfde als die in de controllers al gebruikt werd, dus de
statuscodes van de API veranderen niet. Alleen `NotSupportedException` gaf
vroeger een 500 en geeft nu een 409. De frontend (`api.js`) toont bij een
JSON-fout al het veld `title`, dus foutmeldingen blijven daar zichtbaar.

## Gevolgen

- **Makkelijker:**
  - Controlleracties zijn kort en tonen alleen de happy path.
  - Nieuwe acties krijgen automatisch dezelfde foutafhandeling.
  - Alle fouten hebben hetzelfde JSON-formaat, met een `traceId` om ze in de
    logs terug te vinden.
  - Onverwachte fouten worden gelogd in plaats van stil als HTML terug te
    komen.
- **Moeilijker:**
  - De betekenis van een exceptiontype ligt nu vast voor de hele API. Een
    `InvalidOperationException` geeft altijd 409, ook als die uit een bug komt
    en eigenlijk een 500 zou moeten zijn. `PriceCalculator` gooit dit type
    bijvoorbeeld bij een onbekend resourcetype.
  - De responsebody verandert van platte tekst naar `application/problem+json`.
    Andere clients dan onze eigen frontend moeten `title` lezen.
- **In de gaten houden:**
  - Als het onderscheid tussen businessfouten en bugs belangrijk wordt, voeg
    eigen exceptiontypes toe (bv. `NotFoundException`, `ConflictException`)
    en map die in plaats van de algemene .NET-types.
  - Gooi in services geen exceptions met gevoelige informatie in de
    boodschap: bij 4xx gaat `Message` rechtstreeks naar de client.

## Gerelateerd

- [ADR 000: template](000-template.md)
- [ADR 001: dependency injection](001-dependency-injection.md)
- Branch: `refactor/global-exception-handler`
- [GlobalExceptionHandler](../../API/src/CampusBook.Api/Middleware/GlobalExceptionHandler.cs)
