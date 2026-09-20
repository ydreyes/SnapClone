[System.Serializable]
public class PowerModifier : IModifier
{
	public int amount;

	public PowerModifier() { }

	public PowerModifier(int amount)
	{
		this.amount = amount;
	}

	public int Amount() => amount;
}