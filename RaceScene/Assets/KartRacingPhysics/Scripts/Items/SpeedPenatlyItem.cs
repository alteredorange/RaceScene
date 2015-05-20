using System;
public class SpeedPenatlyItem : PickupItemBase
{
	protected override void OnPickupCollected(KartController kart)
	{
		kart.SpeedPenalty();
		kart.Spin(2f);
	}
}
