## 1. Fase 0 - Terreno (deuda estructural, sin cambio de gameplay)

- [ ] 1.1 Crear `BattleDirector` y migrar de `GameManager` el flujo de turno (EndTurn, RevealCardsInPriorityOrder, GetZoneOutcome, EvaluateGame) y la gestión de pilas (DiscardCard, DestroyCard, MoveCard); verificar partida completa de principio a fin sin cambios de comportamiento
- [ ] 1.2 Crear `PowerCalculator` como único recálculo de poder de carta/zona y retirarlo de los 4 puntos actuales (PlayerController.cs:64, AIController.cs:66, GameManager.cs:686-694, Zone.cs:444-452); verificar con grep que no queda ninguna mutación de `data.power` en runtime
- [ ] 1.3 Introducir `RuntimeCardState` (basePower + lista de modificadores + `currentPower` derivado) y eliminar la mutación de `CardData` en `Zone.RegisterOngoing` (Zone.cs:293-295); verificar jugando dos partidas seguidas que el poder base de la carta no cambia
- [ ] 1.4 Consolidar el barajado duplicado (PlayerController.cs:12-21,47-53 y AIController.cs:13-22,39-46) en una utilidad `Deck`; verificar que no queda shuffle duplicado
- [ ] 1.5 Eliminar o archivar el código heredado sin uso: `EncounterManager` (stub vacío), `MapController`, `MapNode`, `MapEnums`; verificar que la compilación no reporta referencias rotas

## 2. Fase 1 - Nueva capa de campaña (meta-core)

- [ ] 2.1 Crear `CampaignConfig` (ScriptableObject) con vidas iniciales (2), tamaños de baraja (3 inicial, 8 tras draft, máximo 12), oferta de 3 por ronda, 5 batallas normales + 1 subjefe por loop, 3 loops, orden de rotación de recompensas y roster de enemigos; verificar que los valores se leen desde el inspector y desde los controllers
- [ ] 2.2 Crear `RunManager` (singleton limpio) con `deck`, `deckPerks`, `lives` (2), `battleIndex`, `loopIndex`, `winsInLoop` y `nextRewardType`; verificar que un run completo no deja estado residual al iniciar otro
- [ ] 2.3 Implementar `RunState` serializable (JSON, `Application.persistentDataPath`) con guardado automático tras cada batalla/recompensa y limpieza al finalizar el run; verificar guardar, volver al menú y reanudar el run
- [ ] 2.4 Ampliar `CharacterData` con deck inicial de 3 cartas y `draftPool` (fallback a `Resources/Cards/PlayerCards` si está vacía); verificar en editor que cada personaje ofrecerá las cartas esperadas
- [ ] 2.5 Crear `CardDraftController` con ofertas 1-de-3 y 5 rondas hasta 8 cartas; verificar que tras confirmar se navega a la primera batalla (cumple requisito "Draft inicial")
- [ ] 2.6 Crear `CampaignController` que resuelve el resultado de batalla: ganar avanza, perder resta 1 vida (0 vidas = derrota) y concede la recompensa rotada en ambos casos, con la misma batalla reintentable en derrota (cumple "Pérdida de vidas" y "Progresión de batallas")
- [ ] 2.7 Desacoplar `GameScene` del meta: recibir `BattleSetup` (deck del jugador desde `RunManager` + enemigo del roster/campaña) y devolver `BattleOutcome`; verificar una batalla completa desde CharacterSelect con esos datos
- [ ] 2.8 Implementar la navegación de escenas nueva: MainMenu - CharacterSelect (con draft) - GameScene - RewardHubScene - (loop) - GameOverScene/GameCompletedScene; verificar el flujo entero incluyendo reintento tras derrota
- [ ] 2.9 Definir en `CampaignConfig` el roster de enemigos por índice, la aparición de subjefes (batallas 6, 12 y 18) y el jefe final (batalla 22); reutilizar los `EnemyData` existentes como ladrillos; verificar que la selección de enemigo deriva del índice de campaña
- [ ] 2.10 Crear contenido de subjefes y jefe final: cartas en `Resources/Cards/SubBosses` y `Resources/Cards/Boss`, enemigos en `Resources/Enemies/Bosses` con sus mazos; verificar que cada subjefe/jefe tiene un `EnemyData` con deck asignado
- [ ] 2.11 Retirar del build las escenas del meta antiguo (WorldMapScene, ZoneScene, ShopScene, CollectionBuilderScene, CollectionTrackScene, DeckCharacterSelect) y eliminar sus controladores UI; verificar que la lista de build corresponde al nuevo flujo y no hay referencias de escena rotas
- [ ] 2.12 Retirar el sistema de apuestas SNAP: quitar botón/apuesta de `GameManager` (InitBetButton, betButton, betText), `PlayerProgress.TryPlaceBet/CancelBet/betActive`, `AIController.aiBet/TryRandomBet` y los textos/UI asociados; verificar que no queda referencia y que una partida normal no muestra apuestas (cumple "Supresión de apuestas")
- [ ] 2.13 Parametrizar el mensaje de fin de run: derrota (vidas 0 o jefe final perdido) vs victoria (jefe final ganado) en las pantallas finales; verificar ambos caminos muestran textos distintos (cumple "Fin de campaña")

## 3. Fase 2 - Sistema de Perks

- [ ] 3.1 Crear `PerkData` (ScriptableObject) que devuelve un `IModifier` para el `RuntimeCardState`; verificar que los perks no mutan el asset `CardData`
- [ ] 3.2 Implementar el slot único de perk por carta del run (`RunManager.SetPerk(cardId, perk)` con filtro de objetivos sin perk y sin reemplazo); verificar que una carta con perk no puede recibir otro (cumple "Un perk por carta")
- [ ] 3.3 Inyectar los `PerkModifier` en cada `CardInstance` al jugar la carta y heredarlos en copias vía `CopyRuntimeState`; verificar re-probar el efecto del perk en dos batallas seguidas del mismo run (cumple "Efecto del perk en runtime")
- [ ] 3.4 Crear la UI de oferta de perk (1-de-3 perks + elegir carta objetivo sin perk) en RewardHubScene, con estado "no disponible" si todas las cartas tienen perk; verificar ambos estados y el caso all-perked (cumple "Oferta de perk post-batalla")

## 4. Fase 3 - Rotación de recompensas y remoción

- [ ] 4.1 Crear `RewardHubController` que ejecuta la acción según `battleIndex % rotacionOrder` (carta/perk/remover) tras cada batalla ganada o perdida; verificar la secuencia batalla-+carta-batalla-perk-batalla-remover iteración tras iteración (cumple "Secuencia de recompensas post-batalla")
- [ ] 4.2 Crear `RemovalController`: elegir 1 carta de la baraja a eliminar, o saltar sin consecuencias; sin reemplazo inmediato en el mismo turno; verificar que la baraja queda con uno menos y la carta desaparece del run
- [ ] 4.3 Integrar el +1 carta post-batalla (DraftController) respetando el tope de 12 (bloqueo/skip cuando está lleno); verificar el escenario de baraja al máximo del spec card-drafting
- [ ] 4.4 Validar end-to-end la rotación completa en un run de prueba de 22 batallas simuladas; verificar que la recompensa se concede también en derrotas (Q1)

## 5. Fase 4 - Balance y pulido

- [ ] 5.1 Ajustar la dificultad de enemigos por índice de batalla con roster escalable y playtest con 2 vidas; verificar que el run completo es ganable y las derrotas por vidas son posibles pero no frustrantes (Q2)
- [ ] 5.2 Validar el volumen de la pool de draft (por personaje con fallback global) para que las ofertas 1-de-3 sean variadas; verificar en editor la generación de ofertas con la colección de Series 0-5
- [ ] 5.3 Añadir pruebas EditMode de `PowerCalculator` y de la rotación de recompensas usando el paquete com.unity.test-framework; verificar Test Runner en verde
- [ ] 5.4 Limpieza final: revisar strings del juego nuevo, build settings definitivos y la documentación README del flujo; verificar una sesión completa de juego manual sin warnings de consola relevantes

## 6. Verificación global

- [ ] 6.1 Ejecutar el flujo completo (elegir personaje, draftear a 8, ganar 5 + subjefe, x3 loops, jefe final) y comprobar que victoria y derrota terminan en las pantallas finales correctas
- [ ] 6.2 Correr `openspec validate refactor-campaign-flow` tras la implementación y confirmar que el change queda validado y listo para archivar