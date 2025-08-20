public enum MapNodeType
{
	Enemy,   // Nodo de enemigo normal
	Shop,    // Nodo de tienda
	Boss     // Nodo de jefe final
}

public enum MapNodeState
{
	Locked,     // No se puede seleccionar aún
	Available,  // Disponible para hacer click
	Cleared     // Ya completado (enemigo derrotado / tienda usada)
}
