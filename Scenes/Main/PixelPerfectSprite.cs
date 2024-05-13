using Godot;

public partial class PixelPerfectSprite: Node2D
{
	public override void _Ready()
	{
		_Process(0);
	}

	public ObjectState State;

	public override void _Process(double delta)
	{
		State.Process(delta);
		Position = new Vector2((int)State.Position.X, (int)State.Position.Y);
	}
}
