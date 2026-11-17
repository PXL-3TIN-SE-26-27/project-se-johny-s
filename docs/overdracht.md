# CampusBook: overdrachtsdocument

_Dit document schreef het vorige team bij de overdracht. Het beschrijft wat
CampusBook doet, niet hoe de code in elkaar zit. Het is niet volledig en niet
overal even nauwkeurig: bij twijfel heeft de code gelijk, niet dit document._

## Wat is CampusBook?

Een reservatiesysteem voor de campus. Gebruikers reserveren een item voor een
periode, betalen indien nodig, halen het af en brengen het terug. Daarnaast
houdt het systeem statistieken bij, verstuurt het mails en schrijft het elke
actie weg in een auditlog.

Er is geen login (_opzettelijk, dit hoeft ook niet gefixt te worden_). In de frontend kies je rechtsboven gewoon als wie je werkt.

## Gebruikers

Er zijn drie rollen. De rol bepaalt wat je betaalt en wat je mag.

| Rol | In de code | Kort |
| --- | --- | --- |
| Student | `student` | Betaalt per uur, soms een waarborg, heeft een paar beperkingen |
| Personeel | `staff` | Betaalt niets |
| Vereniging | `association` | Betaalt meestal per dag, korting op lokalen tijdens de week |

De seed data bevat een paar gebruikers van elke rol.

## Items

| Soort | In de code | Voorbeelden |
| --- | --- | --- |
| Lokaal | `room` | Lokaal B.204, Auditorium A |
| Laptop | `laptop` | Laptop L-01 tot L-03 |
| Camera | `camera` | Canon R6 kit, Sony A7 IV |
| 3D-printer | `printer` | Prusa MK4 |

Elk item heeft een uurtarief en een dagtarief. Items die uit dienst genomen
zijn, blijven in de database staan als gearchiveerd item.

<div style="page-break-after: always;"></div>

## Levenscyclus van een reservatie

```
               bevestigen
In afwachting ──────────────────────> Bevestigd ──> Afgehaald ──> Teruggebracht
      │                                   │
      └──────────────> Geannuleerd <──────┘
```

| In de frontend | In de code | Betekenis |
| --- | --- | --- |
| In afwachting | `Pending` | Aangemaakt, nog niet bevestigd |
| Bevestigd | `Confirmed` | Klaar om af te halen |
| Afgehaald | `CheckedOut` | De gebruiker heeft het item mee |
| Teruggebracht | `Returned` | Afgesloten |
| Geannuleerd | `Cancelled` | Afgesloten zonder gebruik |

Een lokaal neem je natuurlijk niet mee. Daar haal je de sleutel af aan de
balie en breng je die terug. In het systeem heet dat ook afhalen en
terugbrengen, en dezelfde regels gelden: wie de sleutel te laat terugbrengt,
betaalt de boete.

Enkele regels die we kennen:

- Betalen verandert de status niet. Een reservatie van een student kan pas
  bevestigd worden als ze betaald is. Voor andere rollen ligt dat anders.
- Afhalen kan pas kort voor het begin van de reservatie, en niet meer als de
  periode voorbij is. Een openstaande waarborg moet eerst betaald zijn.
- Bij het terugbrengen wordt de waarborg automatisch terugbetaald. Wie te laat
  terugbrengt, betaalt een boete per begonnen uur.
- Wie annuleert na betaling, krijgt het betaalde bedrag terug. Wie een bevestigde
  reservatie kort voor het begin annuleert, betaalt een annulatiekost.

Bij het aanmaken controleert het systeem onder meer: de periode is geldig en
niet te lang, het item is niet al bezet, en een student heeft niet te veel
lopende reservaties. Een student mag ook niet eender hoe lang een lokaal
boeken. De precieze grenzen zoek je zelf op.

## Prijzen

De prijs wordt berekend op het moment dat de reservatie aangemaakt wordt, en
daarna bewaard. Ze verandert nog bij een te late teruggave.

### Basisprijs

Uren en dagen worden altijd naar boven afgerond: 2 uur en 10 minuten telt als
3 uur.

**Personeel** betaalt nooit iets, ook niet voor extra's.

**Studenten** betalen per uur. Voor lange reservaties van laptops en camera's
geldt een plafond: je betaalt nooit meer dan het dagtarief per begonnen dag.
Vanaf hoeveel uur dat plafond meetelt, verschilt per soort. Een 3D-printer
heeft bovenop het uurtarief een vaste opstartkost.

**Verenigingen** betalen voor lokalen, laptops en camera's per begonnen dag.
Op een lokaal krijgen ze 20 % korting tijdens de week. Een 3D-printer rekenen
we voor hen hetzelfde aan als voor studenten.

### Weekendtoeslag

Reservaties in het weekend kosten 25 % meer. Het percentage staat in
`campus.config.json`.

### Extra's

Bij een reservatie kan je extra's aanvinken. Dit zijn de basisprijzen:

| Extra | Prijs |
| --- | --- |
| Verzekering | 10 % voor laptops en camera's, een vast bedrag van € 5 voor de rest |
| Levering | € 15, niet mogelijk voor lokalen |
| Technieker | € 30 per uur |
| Schoonmaak | € 25 voor een lokaal, € 8 voor de rest |
| Verlengde terugbreng | € 7,50; je krijgt 12 uur extra om terug te brengen |

Een aantal combinaties wordt anders aangerekend. Levering samen met
verzekering is bijvoorbeeld goedkoper. Er zijn er nog andere; die vind je in
de code.

### Campagnekorting

Marketing kan een tijdelijke kortingscampagne instellen: een percentage en een
einddatum, ook in `campus.config.json`. Zolang de campagne loopt, gaat de
korting van de prijs af.

### Waarborg

Alleen studenten betalen een waarborg: € 150 voor een camera, en € 50 voor een
laptop die geleverd wordt. De waarborg wordt samen met de prijs betaald en bij
het terugbrengen teruggestort.

### Voorbeelden

1. Een student reserveert Laptop L-01 op een dinsdag van 10 tot 12 uur, zonder
   extra's. Dat is 2 uur aan € 4: **€ 8,00**.
2. Een vereniging reserveert Lokaal B.204 op een woensdag van 18 tot 23 uur.
   Dat is één begonnen dag aan € 80, min 20 % weekkorting: **€ 64,00**.

Reken deze zelf uit, en controleer je antwoord via de API:

3. Een student reserveert de Canon R6 kit op een zaterdag van 10 tot 16 uur,
   met verzekering.
4. Een student reserveert een laptop voor 30 uur, tijdens de week.

In welke volgorde de toeslagen en kortingen toegepast worden, staat nergens
beschreven. Dat maakt soms uit.

## Betalen

Betalingen lopen via PayLink, een externe betaalprovider. We gebruiken hun
client library (`LegacyPayGateway`), die we niet zelf onderhouden. Een
paar dingen die we geleerd hebben:

- PayLink werkt met bedragen in centen.
- De provider weigert bedragen boven een bepaalde limiet.
- Af en toe antwoordt PayLink niet. Opnieuw proberen lukt dan meestal wel.
- Gratis reservaties gaan niet langs PayLink.

## Wat gebeurt er nog bij een actie?

Naast de wijziging van de reservatie zelf:

- **Mails.** De gebruiker krijgt bij de meeste acties een mail. Bij sommige
  reservaties krijgt ook de facilitaire dienst er een. Mails worden niet echt
  verstuurd, maar als `.eml`-bestand weggeschreven in `mail-out`.
- **Auditlog.** Elke actie komt als één regel in `audit.log`.
- **Statistieken.** Het dashboard toont cijfers uit de database en een paar
  tellers die bijgehouden worden sinds de API gestart is.

Er is ook een herinneringsfunctie die mails stuurt voor reservaties van
morgen. Die wordt niet vanuit de frontend aangeroepen.

## De frontend

| Pagina | Pad | Wat kan je er doen? |
| --- | --- | --- |
| Ontdek | `/` | Kies een dag en zie welke items vrij zijn en wanneer ze bezet zijn. Filter op soort. Vanuit een item ga je meteen naar het reservatieformulier. |
| Reserveren | `/reserveren` | Kies een item, een periode, extra's en een opmerking. Na het reserveren zie je de prijs en de eventuele waarborg. |
| Mijn reservaties | `/mijn-reservaties` | Alle reservaties van de gekozen gebruiker, met knoppen om te betalen, bevestigen, afhalen, terugbrengen en annuleren. Je kan ook alles exporteren als CSV. |
| Stats | `/stats` | Een dashboard met aantallen per status, omzet van deze maand en bevestigingen sinds de opstart. |

De frontend beslist niets zelf. Alle knoppen staan er altijd; of een actie mag,
bepaalt de API. Als ze niet mag, krijg je de foutmelding van de API te zien.

De gekozen gebruiker wordt in je browser onthouden.

## De API

De frontend gebruikt deze endpoints:

| Methode | Pad | Doel |
| --- | --- | --- |
| GET | `/api/users` | Alle gebruikers |
| GET | `/api/resources?date=...` | Items met hun beschikbaarheid op een dag |
| GET | `/api/reservations?userId=...` | Reservaties van een gebruiker |
| POST | `/api/reservations` | Nieuwe reservatie |
| POST | `/api/reservations/{id}/{actie}` | `pay`, `confirm`, `checkout`, `return` of `cancel` |
| GET | `/api/reservations/export?userId=...` | CSV-export |
| GET | `/api/stats` | Cijfers voor het dashboard |

Er zijn nog een paar endpoints die de frontend niet gebruikt. Je vindt ze in de
controllers.

Statuscodes, ongeveer: `400` als je request niet klopt, `404` als iets niet
bestaat, `409` als de actie niet mag in de huidige toestand.

## Configuratie

| Waar | Wat |
| --- | --- |
| `appsettings.json` | Connection string naar de SQLite-database |
| `campus.config.json` | Bedrijfsparameters: weekendtoeslag, maximale duur, campagne, afzender en bestemmeling van mails, merchant-id voor PayLink |
| `Frontend/.env.*` | Adres van de API, ingelezen tijdens het bouwen |

## Bekende eigenaardigheden

- De database wordt bij de eerste start aangemaakt en gevuld. Een schema dat
  wijzigt, wordt niet automatisch bijgewerkt: verwijder dan het bestand.
- De seed data bevat vaste datums in dit semester.
- Niet alle tests in `CampusBook.Tests` zijn even betrouwbaar. We hadden geen
  tijd meer om dat uit te zoeken.
