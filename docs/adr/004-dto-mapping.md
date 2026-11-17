# ADR 002: DTO's opsplitsen en mapping centraliseren

- **Datum:** 2026-11-17
- **Status:** voorgesteld
- **Auteurs:** Brent Caes

## Context

De drie DTO's (`CreateReservationRequest`, `ReservationDto` en
`ResourceAvailabilityDto`) stonden samen in `Models/Dtos.cs`. De omzetting
tussen DTO's en domeinmodellen gebeurde daarnaast op verschillende plekken in
`ReservationService`: de beschikbaarheidsresponse werd daar opgebouwd, de
create-request werd daar naar een `Reservation` gekopieerd en `ToDto` bouwde
de reserveringsresponse.

Daardoor staan de DTO-definities en hun mapping verspreid over een gezamenlijk
modelbestand en een service die ook validatie, prijsberekening en
reserveringslogica uitvoert. Bij het aanpassen van een DTO is het minder
duidelijk waar de bijbehorende conversie zit.

## Overwogen opties

1. **DTO's en mapping laten zoals ze waren.**
   - Voordelen: geen wijzigingen of extra bestanden.
   - Nadelen: alle DTO's delen één bestand en mapping blijft verweven met de
     service.
2. **DTO's opsplitsen, mapping in `ReservationService` laten.**
   - Voordelen: elke DTO is afzonderlijk terug te vinden.
   - Nadelen: de service blijft verantwoordelijk voor zowel de workflow als
     meerdere objectconversies.
3. **DTO's opsplitsen en expliciete mapping in `ReservationMapper` zetten.**
   - Voordelen: DTO-definities zijn afzonderlijk vindbaar en conversies staan
     op één plek; er is geen extra package nodig.
   - Nadelen: mappings moeten handmatig bijgewerkt worden wanneer velden
     veranderen.
4. **Een mappingbibliotheek toevoegen, zoals AutoMapper.**
   - Voordelen: conventionele property-mapping kan met minder toewijzingen
     worden beschreven.
   - Nadelen: extra dependency en configuratie voor een klein aantal eenvoudige
     mappings; de conversies worden minder expliciet.

## Beslissing

We kiezen **optie 3**:

- Elke DTO krijgt een eigen bestand onder `Models/DTOs`.
- `ReservationMapper` bevat de conversies tussen de create-request en
  `Reservation`, tussen `Reservation` en `ReservationDto`, en tussen resources
  met hun bezette tijdsloten en `ResourceAvailabilityDto`.
- De mapping blijft handmatig en expliciet; er wordt geen externe
  mappingbibliotheek toegevoegd.
- Validatie, prijsberekening, statusovergangen en opslag blijven
  verantwoordelijkheden van `ReservationService`, niet van de mapper.

Deze keuze scheidt DTO-definities en conversies met weinig extra complexiteit
en laat de bestaande API-responsevorm ongemoeid.

## Gevolgen

- **Makkelijker:**
  - DTO's zijn afzonderlijk te vinden en te wijzigen.
  - De objectconversies zijn op één plek te bekijken en te onderhouden.
  - De mapping kan onafhankelijk worden getest.
- **Moeilijker:**
  - Een nieuw of gewijzigd DTO-veld moet ook handmatig in de relevante mapping
    verwerkt worden.
  - Er zijn meer bestanden dan bij één gezamenlijk DTO-bestand.
- **In de gaten houden:**
  - Houd businessregels buiten de mapper.
  - Voeg tests toe voor mappings wanneer ze niet-triviale transformaties
    bevatten, zoals de lijst met extra's en bezette tijdsloten.
  - Herbekijk deze keuze als het aantal of de complexiteit van mappings sterk
    groeit.

## Gerelateerd

- [ADR 000: template](000-template.md)
- Branch: `refactor/dto-mapper`
- [ReservationMapper](../../API/src/CampusBook.Api/Models/Mappers/ReservationMapper.cs)
