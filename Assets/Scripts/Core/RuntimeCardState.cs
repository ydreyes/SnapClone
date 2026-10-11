using System.Collections.Generic;

// Estado de poder de una carta durante la batalla (design.md D3).
// Nunca muta el ScriptableObject CardData: el poder base se congela al instanciar.
public class RuntimeCardState
{
	// Poder base congelado al instanciar. null = aún sin inicializar; en ese caso
	// PowerCalculator.ComputeBasePower usa el asset como fallback.
	public int? basePower;

	// Modificadores activos (ongoings, auras, perks...).
	public readonly List<IModifier> modifiers = new List<IModifier>();

	// Poder derivado = basePower + suma de modificadores.
	public int currentPower => PowerCalculator.Compute(this);
}