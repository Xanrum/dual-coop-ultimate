using Godot;
using System;

public partial class Inventory : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	private bool DetectNotStart;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.GetActionStrength("start") > 0 && DetectNotStart)
		{
			GetTree().ChangeSceneToFile("Scenes/Main.tscn");
		} 
		
		if (Input.GetActionStrength("start") == 0) {
			DetectNotStart = true;
		}
	}
}
