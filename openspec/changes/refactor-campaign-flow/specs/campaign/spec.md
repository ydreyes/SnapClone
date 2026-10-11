## Purpose

Define el ciclo de vida de una campaña por run: selección de personaje, gestión de vidas, progresión de batallas con subjefes y jefe final, y la secuenciación de recompensas post-batalla que ocurre tras cada combate ganado o perdido.

## ADDED Requirements

### Requirement: Inicio de campaña

La campaña SHALL iniciar cuando el jugador confirma un personaje en la pantalla de selección de personaje. El personaje define las 3 cartas iniciales de la baraja del run. El jugador dispone de exactamente 2 vidas al inicio de la campaña.

#### Scenario: El jugador elige personaje e inicia el run
- **WHEN** el jugador confirma la selección de un personaje
- **THEN** se inicia un run con 2 vidas, una baraja inicial de 3 cartas definidas por el personaje, y el jugador entra en la fase de draft previa a la primera batalla

### Requirement: Pérdida de vidas

Cada batalla perdida SHALL restar 1 vida. Si el jugador pierde y con ello las vidas llegan a 0, la campaña termina de inmediato con el mensaje de derrota.

#### Scenario: El jugador pierde una batalla y conserva una vida
- **WHEN** el jugador pierde una batalla teniendo 2 vidas
- **THEN** las vidas bajan a 1, la batalla no se da por superada y el jugador recibe la recompensa post-batalla que toque según la rotación antes de reintentar

#### Scenario: El jugador pierde su última vida
- **WHEN** el jugador pierde una batalla teniendo 1 vida
- **THEN** la campaña termina con el mensaje de derrota en la pantalla de fin de run

### Requirement: Progresión de batallas

La campaña SHALL avanzar por victorias. Una batalla perdida no avanza el índice de campaña: el jugador debe volver a ganar esa misma batalla. Dentro de cada loop de campaña se requieren 5 victorias de batallas normales; la 6ª batalla del loop es contra un subjefe.

#### Scenario: El jugador gana una batalla normal
- **WHEN** el jugador gana una batalla normal
- **THEN** la victoria cuenta para el progreso del loop, se otorga la recompensa post-batalla y el jugador pasa a la siguiente batalla del loop

#### Scenario: El jugador pierde una batalla de subjefe
- **WHEN** el jugador pierde contra un subjefe y conserva vidas
- **THEN** el subjefe pendiente no se considera vencido y el jugador debe volver a enfrentarlo

#### Scenario: El jugador gana la sexta batalla del loop
- **WHEN** el jugador acumula 5 victorias normales y gana la 6ª batalla del loop contra el subjefe
- **THEN** el loop queda completado y el progreso pasa al siguiente loop (o al jefe final si era el tercer loop)

### Requirement: Subjefes y jefe final

La campaña SHALL componerse de 3 loops idénticos en estructura (5 batallas normales + 1 subjefe). Tras completar el tercer loop se desbloquea la batalla contra el jefe final. Cada loop usa un subjefe distinto con dificultad creciente.

#### Scenario: Se completa el primer y segundo loop
- **WHEN** el jugador completa un loop
- **THEN** el siguiente loop comienza con un subjefe distinto y enemigos de mayor dificultad

#### Scenario: Se completa el tercer loop
- **WHEN** el jugador completa el tercer loop
- **THEN** la siguiente batalla es la del jefe final, tras la cual la campaña termina en todos los casos

### Requirement: Fin de campaña

La campaña SHALL terminar en uno de dos resultados: victoria o derrota, mostrados en la pantalla de fin de run con mensajes distintos. Derrota: cuando el jugador pierde su última vida o pierde la batalla contra el jefe final. Victoria: cuando gana la batalla contra el jefe final.

#### Scenario: Derrota por vidas
- **WHEN** el jugador pierde su última vida
- **THEN** se muestra la pantalla de fin de run con el mensaje de derrota

#### Scenario: Derrota frente al jefe final
- **WHEN** el jugador pierde la batalla contra el jefe final
- **THEN** se muestra la pantalla de fin de run con el mensaje de derrota

#### Scenario: Victoria frente al jefe final
- **WHEN** el jugador gana la batalla contra el jefe final
- **THEN** se muestra la pantalla de fin de run con el mensaje de victoria

### Requirement: Supresión de apuestas

El sistema de apuestas SNAP SHALL quedar retirado: no existe opción de apostar vidas por multiplicadores en ningún momento de la campaña. Las pantallas y textos de apuesta desaparecen del flujo.

#### Scenario: Se inicia una batalla sin apuesta disponible
- **WHEN** el jugador inicia o juega cualquier batalla de la campaña
- **THEN** no se ofrece ninguna opción de apostar y no se aplican modificadores de vidas por apuestas en el resultado

### Requirement: Secuencia de recompensas post-batalla

Tras cada batalla, ganada o perdida, el jugador SHALL recibir una acción de recompensa determinada por una rotación cíclica de tres tipos: elección de +1 carta, oferta de perk, y eliminación de una carta. La rotación se repite indefinidamente según el índice de campaña: 1ª batalla → +1 carta, 2ª → perk, 3ª → eliminar, 4ª → +1 carta, 5ª → perk, 6ª → eliminar, y así sucesivamente. La recompensa se concede incluso si la batalla se perdió.

#### Scenario: Recompensa tras la primera batalla
- **WHEN** el jugador termina su primera batalla de la campaña (ganada o perdida)
- **THEN** se le presenta la acción de +1 carta (elegir 1 de 3)

#### Scenario: Recompensa tras la segunda batalla
- **WHEN** el jugador termina su segunda batalla de la campaña
- **THEN** se le presenta la acción de oferta de un perk

#### Scenario: Rotación continua
- **WHEN** el jugador termina una batalla de índice exactamente divisible por 3
- **THEN** se le presenta la acción de eliminar una carta de la baraja