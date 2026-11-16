# CampusBook Web ✨

De webfrontend van CampusBook: Vue 3, Vite en Tailwind CSS.

Deze frontend is **bevroren**. Je past de code in `src/` niet aan. Ze hoort wel
bij jullie pipeline: ze moet gebouwd en gedeployed worden, en ze moet weten waar
de API draait.

## Lokaal draaien

Je hebt Node.js nodig (zie de vereisten van Vite voor de minimale versie).

```
npm install
npm run dev
```

De frontend draait dan op `http://localhost:5173` en praat met de API op het
adres uit `.env.development` (standaard `http://localhost:5080`). Start de API
dus eerst.

## Bouwen

```
npm run build
```

Het resultaat komt in `dist/`. Dat is een map met statische bestanden.

## Configuratie

| Variabele | Betekenis |
| --- | --- |
| `VITE_API_BASE` | Adres van de CampusBook API, zonder slash op het einde |

Zie `.env.example`. Let op: Vite leest deze waarde in **tijdens het bouwen**,
niet wanneer de site draait.

## Pagina's

| Pad | Pagina |
| --- | --- |
| `/` | Beschikbaarheid per dag |
| `/reserveren` | Nieuwe reservatie |
| `/mijn-reservaties` | Reservaties van de gekozen gebruiker, met acties en CSV-export |
| `/stats` | Dashboard |
