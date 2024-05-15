using Godot;
using System;

public partial class Player : PixelPerfectSprite
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_health = FindChild("health") as ColorRect;
		_stamina = FindChild("stamina") as ColorRect;
		_mana = FindChild("mana") as ColorRect;
		_maxBar = _health!.Size.X;
	}

	private ColorRect _health;
	private ColorRect _stamina;
	private ColorRect _mana;
	private float _maxBar;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		var player = (State as PlayerState)!;
		_stamina.Size = new(( player.Stamina / player.MaxStamina)*_maxBar, _stamina.Size.Y);
		_health.Size = new(( player.Hp / player.MaxHp)*_maxBar, _health.Size.Y);
		_mana.Size = new(( player.Mana / player.MaxMana)*_maxBar, _mana.Size.Y);
	}
}
