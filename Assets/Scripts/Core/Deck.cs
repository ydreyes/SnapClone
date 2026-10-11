using System.Collections.Generic;
using UnityEngine;

// Utilidad de baraja: barajado centralizado (Fisher-Yates, idéntico al original).
public static class Deck
{
	public static void Shuffle<T>(List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			var temp = list[i];
			int randomIndex = Random.Range(i, list.Count);
			list[i] = list[randomIndex];
			list[randomIndex] = temp;
		}
	}
}