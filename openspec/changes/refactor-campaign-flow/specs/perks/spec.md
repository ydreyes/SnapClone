## Purpose

Define el sistema de perks de la campaña: oferta de perks en la rotación de recompensas post-batalla, la regla de un único perk por carta, y la aplicación persistente del efecto del perk al jugar la carta durante el resto del run.

## ADDED Requirements

### Requirement: Oferta de perk post-batalla

En su turno de la rotación, la recompensa post-batalla SHALL presentar 3 perks candidatos y el jugador elige 1 para equipar. La oferta se presenta igual si la batalla fue ganada o perdida.

#### Scenario: El jugador elige un perk
- **WHEN** el jugador llega a su turno de perk en la rotación
- **THEN** se muestran 3 perks distintos, el jugador elige 1 y elige la carta de su baraja que lo recibe

### Requirement: Un perk por carta

Cada carta de la baraja SHALL admitir como máximo 1 perk equipado. Las cartas que ya tienen un perk no son objetivos válidos para una nueva equipación; si todas las cartas tienen perk, la oferta de perk debe presentarse como no disponible, sin permitir reemplazar un perk existente.

#### Scenario: La carta objetivo ya tiene un perk
- **WHEN** el jugador intenta equipar un perk a una carta que ya tiene uno
- **THEN** el sistema rechaza el objetivo y el jugador debe elegir una carta sin perk

#### Scenario: Todas las cartas tienen perk
- **WHEN** todas las cartas de la baraja tienen un perk y la rotación indica oferta de perk
- **THEN** la acción de perk se muestra como no disponible y el jugador continúa sin equipar

### Requirement: Efecto del perk en runtime

El perk equipado SHALL modificar el comportamiento de la carta durante cada batalla del run (por ejemplo, poder, coste, inmunidades o efectos adicionales) y se reaplica en cada aparición de la carta. La equipación de un perk jamás modifica el asset de datos de la carta compartido entre runs.

#### Scenario: El perk se aplica al jugar la carta
- **WHEN** el jugador juega una carta con perk durante una batalla del run
- **THEN** la carta muestra el efecto del perk en esa y en cada batalla posterior del mismo run

#### Scenario: Los datos base de la carta no cambian
- **WHEN** se equipa un perk a una carta
- **THEN** los datos base de la carta (coste, poder de referencia, efectos propios) permanecen intactos para futuros runs

### Requirement: Persistencia del perk

Los perks equipados SHALL formar parte del estado del run: se conservan entre batallas y quedan reflejados en la baraja hasta el fin de la campaña. Si la carta que porta un perk se elimina de la baraja, el perk desaparece con ella.

#### Scenario: El perk sobrevive entre batallas
- **WHEN** el jugador equipa un perk en una recompensa post-batalla
- **THEN** el perk se mantiene asociado a esa carta en todas las batallas restantes del run

#### Scenario: Eliminación de una carta con perk
- **WHEN** el jugador elimina de la baraja una carta que tiene un perk equipado
- **THEN** la carta y su perk salen de la baraja para el resto del run