using System;
using UnityEngine;

public enum GravityDirection
{
	Top, Down
}

public sealed class GravityManager : Singleton<GravityManager>
{
	private const float GRAVITY = -9.81f;
	
	public event Action<GravityDirection> OnGravityDirectionChanged;

	public GravityDirection GravityDirection { get; private set; } = GravityDirection.Down;
	
	public void ChangeGravity(GravityDirection gravityDirection)
	{
		GravityDirection = gravityDirection;
		switch(GravityDirection)
		{
			case GravityDirection.Top:
				Physics2D.gravity = new Vector2(0f, -GRAVITY);
				break;
			case GravityDirection.Down:
				Physics2D.gravity = new Vector2(0f, GRAVITY);
				break;
		}
		OnGravityDirectionChanged?.Invoke(GravityDirection);
	}	
}