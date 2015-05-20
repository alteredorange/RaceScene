using System;
public class SpeedBoostItem : PickupItemBase
{
	protected override void OnPickupCollected(KartController kart)
	{
		kart.SpeedBoost();
	}
}
