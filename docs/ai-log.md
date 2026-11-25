<!--
AI-log

Noteer een item telkens wanneer een AI-voorstel een beslissing beïnvloed heeft:
een refactoring, een test, een pipelinestap, een review. Ook (en vooral) als je
het voorstel verworpen hebt.

- Elk teamlid heeft een eigen sectie. Voeg je items toe onder je eigen naam,
  nieuwste bovenaan.
- Plak geen lappen code. Verwijs naar de commit of PR.
- Kort is prima: een item is meestal 5 tot 10 regels.

Deze log telt niet mee voor je punten. Het is wel een bron voor de vragen
op de verdediging.

Kopieer voor elk nieuw item het blok "Template" hieronder.
-->

# AI-log

## Template

### JJJJ-MM-DD: korte titel

- **Tool:** welke assistent, welke modus (chat, inline, agent)
- **Context:** waar werkte je aan? (bestand, klasse, PR)
- **Vraag:** wat vroeg je, in een of twee zinnen
- **Voorstel:** wat kreeg je terug, samengevat
- **Beslissing:** overgenomen | aangepast | verworpen
- **Waarom:** wat klopte er wel of niet, en hoe heb je dat gecontroleerd?
- **Herformulering:** welke context ontbrak in je vraag? Hoe zou je ze nu stellen, en wat veranderde er aan het voorstel?
- **Link:** commit of PR

---

## Manuel Bormans
- **Tool:** claude opus 5.5
- **Context:** backend algemeen
- **Vraag:** 'op dit moment word bij elke controller functie een try catch gebruikt, dit lijkt me extreem, maak een global exception handler die alle exceptions afvangt en een nette response teruggeeft'
- **Voorstel:** alle try catch blocks zijn verwijderd en er is een global exception handler toegevoegd die alle exceptions afvangt en een nette response teruggeeft
- **Beslissing:** overgenomen
- **Waarom:** alles werkt naar behoren, en ziet er veel netter uit
- **Herformulering:** /
- **Link:** https://github.com/PXL-3TIN-SE-26-27/project-se-johny-s/pull/12

## Naam teamlid 2

## Naam teamlid 3

## Naam teamlid 4
