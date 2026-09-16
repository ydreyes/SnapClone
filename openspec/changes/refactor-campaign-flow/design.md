## Context

Ver proposal.md - Why. Punto de partida técnico:

- `GameManager` (Assets/Scripts/Core/GameManager.cs, 1.168 líneas) orquesta batalla, poder, efectos y meta; es el punto de acoplamiento principal.
- `CardData` es un ScriptableObject compartido que algunos flujos mutan en runtime (p.ej. `Zone.RegisterOngoing` hace `c.data.power += 1`, Zone.cs:293-295), corrompiendo el asset entre runs.
- El poder de una carta se recalcula en al menos 4 sitios (PlayerController.cs:64, AIController.cs:66, GameManager.cs:686-694, Zone.cs:444-452).
- El meta actual (WorldMap/Zone/Shop/Collection/DeckBuilder + apuestas SNAP) será reemplazado por el nuevo flujo de campaña.
- Persistencia actual: PlayerPrefs aislados (`ClaimedRewards`, `HeroPoints`, `PendingMark`, `LastNodeIndex`) y singletons `DontDestroyOnLoad` (PlayerProgress, GameSession, GameManager).

## Goals / Non-Goals

**Goals:**
- Separar el **battle core** (que funciona y se conserva) de la nueva **capa de campaña** (run, draft, recompensas, fin de run).
- Modelo de estado runtime de cartas por run: los modificadores (poder, perks, inmunidades) viven en la instancia, nunca en el asset `CardData`.
- Un único punto de recálculo de poder (sistema de ongoings/sinergias reutilizable por perks).
- Estado de run serializable y guardable (deck + perks + vidas + índice + rotación).
- Retirar apuestas SNAP y el meta antiguo con su código asociado.

**Non-Goals:**
- No rediseñar el combate en sí (6 turnos, 3 zonas, energía 1→6, prioridad/reveal se mantienen).
- No mejorar la IA estratégica (sólo lo mínimo para que la escalación por índice funcione).
- No implementar multijugador, eventos ni contenido online.

## Decisions

### D1. Arquitectura en dos capas: Campaign vs Battle Core
```
+------------------------------------------------------+
|            CAPA CAMPAIGN (nueva)                     |
| CampaignConfig(SO)  RunManager  CampaignController  |
| CardDraftController RewardHubController              |
+------------------------------------------------------+
              | deck + perks + lives + battleIndex
              v
+------------------------------------------------------+
|            BATTLE CORE (refactorizado)               |
| BattleDirector  PowerCalculator  EffectResolver      |
| PlayerController  AIController  Zone  TurnManager    |
+------------------------------------------------------+
```
- **Decisión**: la escena `GameScene` sólo recibe como entrada un `BattleSetup` (deck enemigo + deck jugador) y devuelve un `BattleOutcome` (victoria/derrota + zonas ganadas). La capa de campaña orquesta qué enemigo, qué hacer tras el resultado y cuándo terminar el run.
- **Alternativa descartada**: evolucionar `PlayerProgress`/`GameSession` como orquestadores. Se descarta porque mezclan estado del meta viejo (zonesCompleted, ownedCards, betActive) con el nuevo, y el run necesita su propia estructura limpia.

### D2. `RunState` serializable + `RunManager`
- `RunManager` (MonoBehaviour `DontDestroyOnLoad`, o estático) posee:
  - `deck: List<CardData>`, `deckPerks: Dictionary<string, PerkData>` (clave = id único de la carta en el run, no `CardData` compartido, porque el mazo puede tener dos copias de la misma carta).
  - `lives` (2 iniciales), `battleIndex` (1..22), `loopIndex` (1..3), `winsInLoop` (0..5), `nextRewardType` (equivalente a `battleIndex % 3`).
  - `selectedEnemy` derivado del roster (`CampaignConfig`), `selectedCharacter`.
- Serialización a JSON (`UnityEngine.JsonUtility` con wrapper serializable) en `Application.persistentDataPath` para el run; se borra al terminar o reiniciar. Sustituye a los PlayerPrefs aislados.
- **Alternativa descartada**: mantener PlayerPrefs. Frágil y sin estructura para listas anidadas (deck + perks).

### D3. `RuntimeCardState` y cero mutación de `CardData`
- Se introduce un modelo de modificadores por instancia:
  - `CardInstance.basePower` (fijo, de `CardData.power`)
  - `List<IModifier> modifiers` con tipos: `PowerModifer`, `CostModifier`, `ImmunityModifier`, `PerkModifier`
  - `CardInstance.currentPower` siempre se deriva de `PowerCalculator.Compute(card, zone)`; ningún código muta `data.power`, `data.energyCost` ni otros campos del asset.
- Elimina el bug de `Zone.RegisterOngoing` (Zone.cs:293-295) y el doble `permanentPowerBonus` (CardData vs CardInstance): sólo existe a nivel de instancia.
- Esto es un prerequisito estructural de los perks (D5).

### D4. `PowerCalculator` y `BattleDirector` (decomposición de GameManager)
- `PowerCalculator` (estático/servicio): recálculo de poder base + ongoings + zona + perks, usado por zonas, cartas y el fin de partida. Reemplaza `RecalculateGlobalOngoingBuffs`, `RecalculateHandSizeOngoing`, `RecalculateAdjacentLocationBonuses`, etc.
- `BattleDirector`: flujo de turno (EndTurn), orden de reveal, evaluación de victoria (`GetZoneOutcome`) y resolución de fin de partida. `GameManager` queda como bootstrap de escena de batalla y puente hacia `RunManager`.
- **Alternativa descartada**: dividir por archivo con la misma estructura actual. No resuelve acoplamiento; se descarta.

### D5. Sistema de perks como modificadores
```
PerkData (ScriptableObject)
  ├── displayName, description, icon
  ├── modifierAplicar(card)  →  devuelve IModifier
  └── validTargets(cardsDeckRun)
```
- `PerkData` no muta cartas; se equipa sobre un id de carta del run (`RunManager.SetPerk(cardId, perk)`) y al instanciar la carta en cada batalla se inyecta `PerkModifier` en su `RuntimeCardState`.
- Slot: 1 por carta (Q3 confirmado). La UI del reward hub filtra cartas objetivo a las que no tienen perk.
- Copias generadas por efectos (p.ej. `SpawnCopyInZone`, GameManager.cs:901) heredan los modificadores del origen vía `CopyRuntimeState`.
- **Alternativa descartada**: perks como subclases de `CardEffectBase` reutilizando los triggers actuales. Se descarta porque los ongoings actuales dependen de `GameManager.Instance` y estado global; un modelo de modificadores es más limpio y portable.

### D6. Rotación de recompensas como disco con tres controladores
- Tras cada batalla (ganada o perdida) la campaña navega a una escena/panel `RewardHubScene` que ejecuta según `battleIndex % 3`:
  - `DraftController` (+1 carta, 1-de-3 de la pool, respetando tope 12),
  - `PerkController` (1-de-3 perks + objetivo),
  - `RemovalController` (elegir carta a eliminar o saltar).
- Los tres comparten un patrón UI común (`OfferPanel` con 3 slots + botón "saltar" cuando aplique). 
- Derrota: se resta 1 vida **antes** de mostrar la recompensa; si vidas llegan a 0, se salta la recompensa y se va directo a fin de run.

### D7. Roster de campaña en `CampaignConfig`
```
CampaignConfig (ScriptableObject)
  ├── startingLives = 2
  ├── startingDeckSize = 3, draftTargetSize = 8, maxDeckSize = 12
  ├── draftOffersPerRonda = 3
  ├── normalBattlesPerLoop = 5, subBossIndexInLoop = 6, loops = 3
  ├── rewardRotationOrder = [Draft, Perk, Removal]
  ├── rosterPorLoop[3]  → lista de EnemyData escalonada (1..5 por loop)
  └── subJefes[3], jefeFinal
```
- `CampaignController` deriva de `battleIndex`: enemigo normal (`roster[(loop-1)*5 + (winInLoop)]`), subjefe (`subJefes[loopIndex-1]`) y jefe final.
- Los `EnemyData` actuales se reutilizan como ladrillos; `Resources/Enemies/Bosses` y las cartas de `Resources/Cards/Boss` + `SubBosses` se crean como contenido nuevo.

### D8. Escenas del nuevo flujo
- `MainMenu -` CharacterSelect` (con panel de draft inicial integrado, 5 rondas) -` GameScene -` RewardHubScene -` (loop) -` GameOverScene` / `GameCompletedScene` (misma pantalla final, mensaje según resultado).
- Se retiran del build: WorldMapScene, ZoneScene, ShopScene, CollectionBuilderScene, CollectionTrackScene, DeckCharacterSelect. Los scripts famélicos de ese meta (MapController, MapNode, MapEnums, EncounterManager, WorldMapController, ZoneController, ShopController, CollectionBuilderController y sus controles UI) se eliminan; los assets `Resources/Cards/PlayerCards (Series 0-5)` y `IACards` persisten como pool de draft.
- **Alternativa descartada**: unificar draft y recompensa en la escena de batalla. Separa mejor las responsabilidades y la UI.

### D9. Pool de draft y datos de personaje
- `CharacterData` pasa a definir: 3 cartas iniciales + `draftPool` (lista de `CardData` de la que salen las ofertas 1-de-3). Si `draftPool` está vacío, se usa la colección completa de `Resources/Cards/PlayerCards` como fallback (asumido; ver Open Questions).
- `DeckData` sigue valiendo para los mazos enemigos (`EnemyData.deck`).

## Risks / Trade-offs

- [Decomponer GameManager puede romper flujos de efecto que hoy dependen de su estado global] → Mitigación: Fase 0 aislada (sin cambio de gameplay), con smoke test manual de partida tras cada extracción; mantener `GameManager.Instance` como fachada mientras se migran los consumidores.
- [El nuevo draft/pool sin contenido suficiente produce ofertas repetidas] → La pool tolera repetir (spec card-drafting permite reoferta); crear contenido de series 1-5 ya existente permite reoferta razonable.
- [Balance con 2 vidas: run muy corto si la IA no escala] → D7 parametriza la dificultad en `CampaignConfig`; ajuste de dificultad como tarea de la Fase 4.
- [Segundo icono de "fin de run" (victoria derrota) y estados de GameOver en code antiguo pueden quedar huérfanos] → `GameOverScene`/`GameCompletedScene` se repurpose: una sola escena con mensaje parametrizado, o mantener las dos y elegir por resultado.
- [Perks pueden duplicar sinergias con ongoings actuales] → D5 modela perks como modificadores que `PowerCalculator` integra; los effects de carta existentes no cambian de contrato.

## Migration Plan

1. **Fase 0 - Terreno**: separar battle core (D1/D3/D4) sin tocar gameplay; los cambios de `CardData` runtime se corrigen con el modelo de instancia.
2. **Fase 1 - Flujo**: `RunManager` + `CampaignConfig` + draft + resultado/recompensa nav. La escena de batalla empieza a tomar `BattleSetup` del `RunManager`; se retira el flujo WorldMap/Zone.
3. **Fase 2 - Perks**: `PerkData` + slot + inyección en `RuntimeCardState` + UI de equipar.
4. **Fase 3 - Rotación completa y remoción**: `RewardHubController` ejecuta las tres acciones con skip.
5. **Fase 4 - Balance y pulido**: dificultad por índice, mensajes de fin de run, contenido subjefes/jefe final.
- Rollback: el battle core queda separado; si la campaña falla, se mantiene `GameScene` jugable con un setup por defecto y el flujo viejo puede reanudarse hasta retirar sus escenas al final de la Fase 1.

## Open Questions

- Pool de draft por personaje vs global: nunca resuelve una spec ni la arquitectura; decidido por defecto por personaje con fallback global.
- Tamaño y distribución del roster de enemigos por índice: tunear en Fase 4.
- Si el run debe guardarse y reanudarse tras cerrar la app (suspend) o sólo en memoria: no cambia specs ni diseño (D2 ya serializa); se decide en implementación.