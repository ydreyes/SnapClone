using UnityEngine;

// Único punto de recálculo de poder de cartas/zonas (design.md D4).
// Ningún código del runtime muta los campos del asset CardData.
public static class PowerCalculator
{
	// Poder base de un CardData (asset) al instanciar una carta. Nunca muta el asset.
	public static int ComputeBase(CardData data)
	{
		if (data == null) return 0;
		return data.power + data.permanentPowerBonus;
	}

	// Poder derivado de un estado runtime: basePower congelado + suma de modificadores.
	public static int Compute(RuntimeCardState state)
	{
		if (state == null) return 0;
		int total = state.basePower ?? 0;
		foreach (var modifier in state.modifiers)
		{
			if (modifier != null) total += modifier.Amount();
		}
		return total;
	}

	// Poder base congelado de una instancia (fallback al asset si aún no fue inicializado).
	public static int ComputeBasePower(CardInstance card)
	{
		if (card == null) return 0;
		if (card.state != null && card.state.basePower.HasValue)
			return card.state.basePower.Value;
		return ComputeBase(card.data);
	}
}