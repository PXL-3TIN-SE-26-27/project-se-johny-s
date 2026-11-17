# ADR 001: Gebruik van enums voor vaste waarden en typesafe statusbeheer

- **Datum:** 17-11-2026
- **Status:** Aanvaard
- **Auteurs:** [Wout Verhemeldonck]

## Contextgit

In de oorspronkelijke implementatie werden vaste waarden en statussen op verschillende plaatsen in de code voorgesteld door middel van strings en/of numerieke waarden.

Dit zorgde voor een aantal nadelen:

- Dezelfde waarden werden op meerdere plaatsen afzonderlijk gedefinieerd.
- Er was geen garantie dat overal dezelfde schrijfwijze werd gebruikt.
- Typfouten konden pas tijdens runtime ontdekt worden.
- De code was moeilijker te onderhouden doordat mogelijke waarden niet centraal gedefinieerd waren.
- Bij het toevoegen van nieuwe waarden moesten verschillende delen van de code mogelijk aangepast worden.

Om de consistentie, leesbaarheid en onderhoudbaarheid van de code te verbeteren, werd beslist om deze vaste waarden te vervangen door enums.

## Overwogen opties

1. **Strings en numerieke waarden behouden**
   - Voordelen:
     - Eenvoudig te implementeren.
     - Geen extra enum-definities nodig.
   - Nadelen:
     - Gevoelig voor typfouten en inconsistente waarden.
     - Geen sterke typecontrole.
     - Mogelijke waarden zijn niet altijd duidelijk uit de code af te leiden.
     - Meer kans op duplicatie.

2. **Constanten gebruiken**
   - Voordelen:
     - Centrale definitie van vaste waarden.
     - Minder duplicatie en typfouten.
   - Nadelen:
     - Bieden niet dezelfde typeveiligheid als enums.
     - Het blijft mogelijk om ongeldige waarden door te geven.

3. **Enums gebruiken**
   - Voordelen:
     - Typeveiligheid en een beperkte, duidelijk gedefinieerde set mogelijke waarden.
     - Centrale definitie van toegestane waarden.
     - Betere leesbaarheid en onderhoudbaarheid.
     - Minder kans op fouten door typfouten of inconsistente waarden.
     - Betere ondersteuning door IDE's en compiler.
   - Nadelen:
     - Vereist een initiële refactoring van bestaande code.
     - Nieuwe waarden moeten expliciet aan de enum toegevoegd worden.
     - Extra aandacht nodig bij conversies naar externe formaten, zoals JSON of databasewaarden.

## Beslissing

Er werd gekozen om de bestaande implementatie te refactoren en enums te gebruiken voor waarden en statussen waarvan de mogelijke opties vooraf vastliggen.

Alle relevante waarden worden voortaan centraal in enums gedefinieerd. De code gebruikt deze enums in plaats van losse strings of numerieke waarden.

Hierdoor worden ongeldige waarden zoveel mogelijk voorkomen en wordt de code consistenter en eenvoudiger te onderhouden.

De bestaande implementatie werd aangepast zodat de relevante klassen en methodes met deze enums werken.

## Gevolgen

**Positieve gevolgen:**

- Betere typeveiligheid.
- Minder kans op runtimefouten door ongeldige waarden.
- Betere leesbaarheid doordat de betekenis van waarden expliciet is.
- Centrale plaats voor het beheren van toegestane waarden.
- Eenvoudiger uitbreidbaarheid wanneer nieuwe waarden nodig zijn.
- Betere ondersteuning van code completion en refactoring door de IDE.

**Aandachtspunten:**

- Bij het toevoegen van nieuwe enum-waarden moet nagegaan worden of alle relevante `switch`-statements en andere controles correct worden bijgewerkt.
- Bij communicatie met externe systemen moet rekening gehouden worden met de mapping tussen enums en externe waarden.
- Bestaande data en API-contracten moeten compatibel blijven met de nieuwe implementatie.

## Gerelateerd

- Pull request: [Link naar PR]
- Issue: [Link naar issue indien van toepassing]
