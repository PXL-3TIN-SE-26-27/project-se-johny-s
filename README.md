# CampusBook

CampusBook is de reservatietoepassing van de campus. Studenten, personeel en
studentenverenigingen reserveren er lokalen, laptops, camera's en 3D-printers.

De toepassing draait al een tijdje en werkt. Jullie team neemt het onderhoud over.

## Inhoud

| Map | Wat |
| --- | --- |
| `API/src/CampusBook.Api` | ASP.NET Core Web API met EF Core en SQLite |
| `API/tests/CampusBook.Tests` | xUnit-tests |
| `Frontend` | Vue-applicatie (Vite), zie `Frontend/README.md` |
| `docs` | Overdrachtsdocument van het vorige team, ADR-template en AI-log |

Begin met `docs/overdracht.md`: wat CampusBook doet, hoe prijzen berekend
worden en wat je in de frontend kan.

## Lokaal draaien

### API

Je hebt de .NET SDK nodig (zie de `TargetFramework` in de projectbestanden).

```
cd API
dotnet build
dotnet test
dotnet run --project src/CampusBook.Api
```

De API luistert op `http://localhost:5080`. De database (`campusbook.db`) wordt
bij de eerste start aangemaakt en gevuld met seed data. Wil je opnieuw
beginnen met de seed data, stop dan de API en verwijder het bestand.

Mails worden niet echt verstuurd maar als `.eml`-bestand weggeschreven in
`mail-out`. Elke actie op een reservatie komt ook in `audit.log`.

### Frontend

Je hebt Node.js nodig (zie de vereisten van Vite voor de minimale versie).

```
cd Frontend
npm install
npm run dev
```

De frontend draait op `http://localhost:5173` en praat met de API op het adres
uit `.env.development`. Start de API dus eerst. Er is geen login: je kiest
rechtsboven wie je bent.

## Afspraken

- `API/src/CampusBook.Api/Infrastructure/LegacyPayGateway.cs` is code van de
  betaalprovider. Die pas je niet aan.
- De code van de frontend hoef je niet aan te passen. Configuratie voor build en deploy mag je wel aanpassen.
- Gedrag van de API verander je enkel bewust, in een aparte pull request met
  een test die het nieuwe gedrag vastlegt.
