## Purpose

Define cómo el jugador construye su baraja a lo largo de la campaña mediante ofertas de cartas de tipo 1-de-3: el draft inicial desde 3 cartas hasta 8, y la elección de +1 carta en la rotación de recompensas post-batalla, respetando el tope de 12 cartas.

## ADDED Requirements

### Requirement: Draft inicial

Al iniciar la campaña, la baraja SHALL contener las 3 cartas iniciales del personaje. Antes de la primera batalla, el jugador pasa por una fase de draft en la que se le muestran 3 cartas candidatas y elige 1, repitiendo el proceso hasta alcanzar 8 cartas en la baraja.

#### Scenario: El jugador completa el draft inicial
- **WHEN** el jugador confirma un personaje y completa las 5 rondas de draft (una carta por ronda)
- **THEN** la baraja tiene exactamente 8 cartas y el jugador pasa a la primera batalla

#### Scenario: Las rondas de draft se muestran como ofertas de 3
- **WHEN** el jugador entra en una ronda de draft
- **THEN** se muestran exactamente 3 cartas candidatas distintas y el jugador elige exactamente 1

### Requirement: Ofertas de cartas desde la pool del flujo

Las cartas candidatas de cada oferta (draft inicial y post-batalla) SHALL extraerse de la pool de cartas del flujo configurada para la campaña. Una carta ya presente en la baraja puede volver a ofrecerse en rondas posteriores.

#### Scenario: La oferta sale de la pool configurada
- **WHEN** se genera una oferta de 1-de-3
- **THEN** las 3 candidatas provienen de la pool de cartas de la campaña y la carta elegida se agrega a la baraja

### Requirement: Elección de +1 carta post-batalla

En su turno de la rotación, la recompensa post-batalla SHALL permitir elegir 1 de 3 cartas candidatas para agregar a la baraja. Si la baraja ya tiene 12 cartas, la elección no está disponible y el jugador debe saltar la acción.

#### Scenario: El jugador agrega una carta tras una batalla
- **WHEN** el jugador llega a su turno de +1 carta en la rotación con menos de 12 cartas en la baraja
- **THEN** se muestran 3 candidatas, el jugador elige 1 y la baraja crece en 1

#### Scenario: La baraja está al máximo
- **WHEN** la baraja tiene 12 cartas y la rotación indica +1 carta
- **THEN** la acción de +1 carta se marca como no disponible y el jugador continúa sin agregar cartas