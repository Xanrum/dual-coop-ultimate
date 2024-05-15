using Godot;
using System;

public partial class Mob : PixelPerfectSprite
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_health = FindChild("health") as ColorRect;
		_maxBar = _health!.Size.X;
	}

	private ColorRect _health;
	private float _maxBar;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		var mob = (State as MobState)!;
		_health.Size = new(( mob.Hp / mob.MaxHp)*_maxBar, _health.Size.Y);
	}
}
