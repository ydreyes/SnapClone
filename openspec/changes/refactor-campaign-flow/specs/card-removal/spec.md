## Purpose

Define la acción de eliminar cartas de la baraja en su turno de la rotación de recompensas: permite quitar una carta del mazo durante el run para liberar espacio, con la opción de saltar la acción.

## ADDED Requirements

### Requirement: Eliminación de una carta

En su turno de la rotación, la recompensa post-batalla SHALL presentar la acción de eliminar 1 carta de la baraja. El jugador elige una carta de su baraja para retirarla, o decide saltar la acción sin consecuencias.

#### Scenario: El jugador elimina una carta
- **WHEN** el jugador llega a su turno de eliminación en la rotación y elige una carta de su baraja
- **THEN** la carta elegida se retira de la baraja y queda fuera para el resto del run

#### Scenario: El jugador salta la eliminación
- **WHEN** el jugador llega a su turno de eliminación y no quiere eliminar ninguna carta
- **THEN** la baraja permanece igual y el jugador continúa con la siguiente batalla

### Requirement: Liberación de espacio

Eliminar una carta SHALL liberar un hueco en la baraja hasta el tope de 12 cartas, permitiendo que una futura acción de +1 carta vuelva a estar disponible si la baraja estaba al máximo.

#### Scenario: Se elimina una carta con la baraja al máximo
- **WHEN** la baraja tiene 12 cartas y el jugador elimina 1
- **THEN** la baraja queda con 11 cartas y la siguiente acción de +1 carta vuelve a estar disponible

### Requirement: Sin reemplazo inmediato

La acción de eliminación SHALL no ofrecer reparación y no agregar una carta nueva en el mismo turno: la carta nueva sólo puede llegar en una acción de +1 carta posterior de la rotación.

#### Scenario: La eliminación no agrega cartas
- **WHEN** el jugador elimina una carta
- **THEN** la baraja queda con una carta menos y no se presenta ninguna oferta de carta nueva en ese turno