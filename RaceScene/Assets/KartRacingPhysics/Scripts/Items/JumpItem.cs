using System;
public class JumpItem : PickupItemBase
{
	protected override void OnPickupCollected(KartController kart)
	{
		kart.Jump(1f);
	}
}
