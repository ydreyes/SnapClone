## Why

El meta-juego actual (mapa mundial con zonas y nodos, tienda, colección, editor de barajas) no corresponde al flujo de juego deseado: una campaña roguelike-lite de draft de baraja por run, con 2 vidas, subjefes y un jefe final. Además, la arquitectura acumula deuda que impide evolucionar con seguridad (GameManager monolítico, mutación de ScriptableObjects en runtime, persistencia frágil en PlayerPrefs y singletons). Este cambio refactoriza el flujo completo del juego y sienta las bases estructurales para los sistemas nuevos (perks, eliminación de cartas, rotación de recompensas).

## What Changes

- **BREAKING** — Nuevo flujo de campaña por run que reemplaza el meta de zonas/mapa mundial:
  1. El jugador elige un personaje; éste aporta 3 cartas iniciales (baraja máxima 12).
  2. Fase de draft: se muestran 3 cartas de una pool y el jugador elige 1, repitiendo hasta completar 8 cartas.
  3. Batalla 1v1 contra la IA. Ganar avanza; perder cuesta 1 vida.
  4. El jugador tiene **2 vidas**; perder ambas termina el run con mensaje de derrota.
  5. La secuencia de conflicto se compone de 5 victorias de batallas normales + la 6ª contra un **subjefe**; el loop completo se repite **3 veces** (3 subjefes distintos, dificultad creciente) y después se enfrenta al **jefe final**.
  6. Ganar el jefe final termina el run con mensaje de victoria (misma pantalla final, mensaje distinto).
- **BREAKING** — Nueva rotación de recompensas post-batalla, en todo momento tras una batalla ganada **o perdida**: batalla → elección de carta (+1) → batalla → perk → batalla → eliminar carta → y así cíclicamente.
- Sistema de **perks**: 1 perk equipable por carta, ofertado en la rotación, aplicado en runtime por run (jamás mutando el asset de carta).
- Sistema de **eliminación de cartas**: en su turno la rotación permite eliminar 1 carta de la baraja (liberando espacio hasta el máximo de 12).
- **BREAKING** — Se retira el sistema de apuestas SNAP (botón de apuesta, `betActive`, apuestas de la IA, vidas ±).
- **BREAKING** — Se retiran del build las escenas del meta antiguo (WorldMap, Zone, Shop, CollectionBuilder, CollectionTrack, DeckCharacterSelect) y su código asociado (MapController/MapNode/MapEnums, EncounterManager stub, ZoneController/WorldMapController).
- Refactor estructural preliminar: descomposición de `GameManager`, modelo de estado runtime de cartas (sin mutar `CardData`), cálculo de poder centralizado, y persistencia de run serializable.
- **Convención de entrega** — cada mejora que requiera cableado en el Editor (ScriptableObjects, efectos de carta, prefabs, escenas o UI) entrega además una **guía Markdown de implementación manual** (`<Feature>_GUIDE.md`) junto al código, con los pasos de Editor y el árbol de UI a modificar/crear. Ver `AGENTS.md` (regla permanente).

## Capabilities

### New Capabilities

- `campaign`: ciclo de vida del run — selección de personaje, 2 vidas, conteo de batallas y loops, aparición de subjefes, jefe final, fin de run (derrota/victoria) y secuenciación de recompensas post-batalla.
- `card-drafting`: ofertas de 1-de-3 desde una pool, draft inicial 3→8, y elección de carta post-batalla respetando el tope de 12.
- `perks`: oferta de perks en la rotación, un slot por carta, y aplicación de efectos de perk en runtime durante cada batalla.
- `card-removal`: acción de eliminar una carta de la baraja en su turno de la rotación, con opción de saltar.

### Modified Capabilities

- _(Ninguna: el proyecto no tiene specs previas.)_

## Impact

- **Código**: `GameManager.cs` (decomposición y retiro de apuestas), `PlayerProgress.cs` (modelo de vidas/recompensa → `RunManager`), `GameSession.cs`, `AIController.cs` (eliminar `aiBet`/`TryRandomBet`), `TurnManager.cs`, `PlayerController.cs`, `Zone.cs`, `ZoneController.cs`, `WorldMapController.cs` (retirada), `MainMenuController.cs` (nuevo flujo de entrada), `CharacterSelectController.cs` (draft), `ShopController.cs`/`CollectionBuilderController.cs` (retirada), `MapController.cs`/`MapNode.cs`/`MapEnums.cs`/`EncounterManager.cs` (retirada).
- **Nuevos sistemas**: `RunManager`, `CampaignController`, `CardDraftController`, `RewardHubController`, `PerkData` (+ slots en instancia), `CardRemovalController`, `CampaignConfig`, `RuntimeCardState`.
- **Contenido**: crear cartas de subjefes/jefe final (`Resources/Cards/SubBosses`, `Resources/Cards/Boss`), enemigos (`Resources/Enemies/Bosses`) y la pool de draft por personaje.
- **Build**: lista de compilación pasa de 12 escenas a las del nuevo flujo (MainMenu, CharacterSelect, Draft?, GameScene, GameOver/GameCompleted).
- **Persistencia**: reemplazo de PlayerPrefs sueltos por un estado de run serializable con guardado/carga.
- **Documentación**: una guía de implementación manual por mejora que toque assets, escenas, prefabs o UI (p. ej. `CampaignConfig_GUIDE.md`, `PerkData_GUIDE.md`, `Docs/UI/RewardHub_GUIDE.md`), conforme a la regla permanente de `AGENTS.md`.