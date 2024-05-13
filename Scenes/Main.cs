using Godot;
using System;
using System.Collections.Generic;

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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		State.Instance.Process(delta);
		var deleteMobs = new List<MobState>();
		foreach (var p in State.Instance.Mobs)
		{
			if (!p.Created)
			{
				var pl = MobScene.Instantiate<PixelPerfectSprite>();
				pl.State = p;
				p.Node = pl;
				AddChild(pl);
				p.Created = true;
			}

			if (p.Deleted)
			{
				deleteMobs.Add(p);
			}
		}
		foreach (var p in deleteMobs)
		{
			State.Instance.Mobs.Remove(p);
			RemoveChild(p.Node);
		}
		
		var deleteBullets = new List<BulletState>();
		foreach (var p in State.Instance.Bullets)
		{
			if (!p.Created)
			{
				var pl = BulletScene.Instantiate<PixelPerfectSprite>();
				pl.State = p;
				p.Node = pl;
				AddChild(pl);
				p.Created = true;
			}

			if (p.Deleted)
			{
				deleteBullets.Add(p);
			}
		}
		foreach (var p in deleteBullets)
		{
			State.Instance.Bullets.Remove(p);
			RemoveChild(p.Node);
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
