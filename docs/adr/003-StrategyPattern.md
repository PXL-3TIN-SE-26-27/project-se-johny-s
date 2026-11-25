<!--
Template voor een Architecture Decision Record.

Gebruik:
1. Kopieer dit bestand naar een nieuw bestand met het volgende vrije nummer
   en een korte titel, bv. 001-wat-we-aanpakken.md.
2. Vul elke sectie in. Verwijder deze commentaar.
3. Voeg de ADR toe via een pull request, net als code.

Pas een aanvaarde ADR niet meer aan als je van mening verandert. Schrijf een
nieuwe ADR en zet de status van de oude op "vervangen door ADR 0xx".
Deze template zelf (000) laat je staan.
-->

# ADR 000: If-else vervangen door switch in PriceCalculator

- **Datum:** 17-11-2026
- **Status:** voorgesteld
- **Auteurs:** Mathias Beaulen

## Context

In PriceCalculator.cs, meer bepaald in de methode Calculate(), werd gebruikgemaakt van een if-else blok om verschillende gebruikersrollen te controleren.

Er werd gecontroleerd of een gebruiker de rol staff, student of association had. Afhankelijk van de rol werd een andere prijsberekening uitgevoerd.

Dit maakte de code minder overzichtelijk doordat meerdere mogelijke gevallen via if-else-structuren werden afgehandeld.

## Overwogen opties

1. **Optie A: De bestaande if-else-structuur behouden** De bestaande code behouden en de prijsberekeningen blijven uitvoeren via if-else-voorwaarden.

   - Voordelen:
      - Eenvoudig te begrijpen.
      - Geen aanpassingen aan de bestaande code nodig.
      - Geschikt voor eenvoudige voorwaarden.
   - Nadelen:
      - Minder overzichtelijk wanneer meerdere mogelijke waarden gecontroleerd worden.
      - De structuur wordt moeilijker leesbaar wanneer er extra gebruikersrollen bijkomen.
2. **Optie B: If-else vervangen door switch** De if-else-structuur vervangen door een switch-statement op basis van user.Role.
Voor elke gebruikersrol wordt een afzonderlijke case voorzien. De bestaande switch-statements voor de verschillende resourcetypes blijven behouden.
   - Voordelen:
      - Overzichtelijkere structuur bij meerdere mogelijke waarden.
      - Elke gebruikersrol heeft een duidelijk afgebakende case.
      - Onbekende gebruikersrollen worden afgehandeld via de default-case.
      - Kleine refactoring zonder grote wijzigingen aan de bestaande architectuur.
   - Nadelen:
      - Nieuwe gebruikersrollen vereisen nog steeds een aanpassing aan de switch.
      - De verschillende prijsberekeningen blijven in dezelfde klasse staan.
## Beslissing

We kiezen ervoor om de if-else-structuur in PriceCalculator.Calculate() te vervangen door een switch-statement op basis van user.Role.

Een switch-statement maakt de bestaande code al overzichtelijker zonder de architectuur onnodig uit te breiden.

De bestaande prijsberekeningen en de verdere logica voor toeslagen, verzekeringen, leveringen en kortingen blijven ongewijzigd.

## Gevolgen

- Wat wordt makkelijker?
   - De verschillende gebruikersrollen herkennen in de code.
   - De prijsberekening per gebruikersrol volgen.
   - Nieuwe cases toevoegen aan de bestaande switch.
   - De code onderhouden en begrijpen.
- Wat wordt moeilijker?
   - Nieuwe gebruikersrollen vereisen nog steeds een wijziging aan de bestaande switch.
   - De prijsberekeningen blijven afhankelijk van de gebruikersrol binnen dezelfde klasse.
- Wat moeten we in de gaten houden?
   - Het bestaande gedrag van de prijsberekening moet behouden blijven.
   - Tests moeten aantonen dat dezelfde invoer nog steeds dezelfde prijs oplevert.
   - Onbekende gebruikersrollen moeten dezelfde exception blijven veroorzaken.
   - Switch-statements enkel gebruiken waar meerdere mogelijke waarden gecontroleerd worden. Eenvoudige voorwaarden blijven gewone if-statements.
## Gerelateerd
- Bestand: API/src/Services/PriceCalculator.cs
- Gerelateerde ADR's: ADR 001 (Dependency Injection)
