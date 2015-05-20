using System;
public class OilSlickItem : PickupItemBase
{
	protected override void OnPickupCollected(KartController kart)
	{
		kart.SpeedPenalty(0.25f, 1f, 1f);
		kart.Wiggle(1f);
	}
}
