// Modificador de poder que aplica sobre un RuntimeCardState.
// Los efectos nunca mutan el asset CardData: añaden un modificador a la instancia.
public interface IModifier
{
	int Amount();
}