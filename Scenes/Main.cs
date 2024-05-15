using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var p in State.Instance.Players)
		{
			var pl = PlayerScene.Instantiate<PixelPerfectSprite>();
			pl.State = p;
			AddChild(pl);
		}
	}

	[Export] public PackedScene MobScene { get; set; }
	[Export] public PackedScene PlayerScene { get; set; }
	[Export] public PackedScene BulletScene { get; set; }

	private Dictionary<ObjectState, VisualObject> VisualObjects = new();

	private class VisualObject
	{
		public PixelPerfectSprite Node;
		public bool Present;
	}


	private bool DetectNotStart;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.GetActionStrength("start") > 0 && DetectNotStart)
		{
			GetTree().ChangeSceneToFile("Scenes/Inventory.tscn");
		} 
		
		if (Input.GetActionStrength("start") == 0) {
			DetectNotStart = true;
		}
		
		State.Instance.Process(delta);
		foreach (var v in VisualObjects.Values)
		{
			v.Present = false;
		}

		foreach (var inst in State.Instance.VisualObjects)
		{
			if (!VisualObjects.TryGetValue(inst, out var v))
			{
				var scene = inst switch
				{
					PlayerState => PlayerScene,
					MobState => MobScene,
					BulletState => BulletScene,
					_ => throw new Exception("unknown v type")
				};
				var n = scene.Instantiate<PixelPerfectSprite>();
				n.State = inst;
				v = VisualObjects[inst] = new()
				{
					Node = n
				};
				AddChild(n);
			}

			v.Present = true;
		}


		foreach (var v in VisualObjects.Values.Where(p => !p.Present).ToList())
		{
			RemoveChild(v.Node);
		}

		if (State.Instance.GameOver)
		{
			GetTree().ChangeSceneToFile("Scenes/GameOver.tscn");
		}

		if (State.Instance.Win)
		{
			GetTree().ChangeSceneToFile("Scenes/Win.tscn");
		}
	}
}
