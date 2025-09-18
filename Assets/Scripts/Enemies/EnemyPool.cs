using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Pool", fileName = "EnemyPool_")]
public class EnemyPool : ScriptableObject
{
	public string poolName = "Zone 0";
	public EnemyData[] enemies;     // arrastra aquí 5 enemigos, por ejemplo

	[Header("Selección ponderada (opcional)")]
	public int[] weights;           // mismo tamaño que 'enemies'. Si está vacío o tamaños no coinciden, usa uniforme.

	public EnemyData GetRandom()
	{
		if (enemies == null || enemies.Length == 0) return null;
		if (weights == null || weights.Length != enemies.Length)
			return enemies[Random.Range(0, enemies.Length)];

		int total = 0;
		for (int i = 0; i < weights.Length; i++) total += Mathf.Max(0, weights[i]);
		if (total <= 0) return enemies[Random.Range(0, enemies.Length)];

		int r = Random.Range(0, total);
		int acc = 0;
		for (int i = 0; i < enemies.Length; i++)
		{
			acc += Mathf.Max(0, weights[i]);
			if (r < acc) return enemies[i];
		}
		return enemies[enemies.Length - 1];
	}
}
