using System.Collections.Generic;
using System.Linq;
using Godot;

public static class State
{
	public static StateInstance Instance;

	public static void Reset()
	{
		Instance = new();
	}
}

public class StateInstance
{
	public bool GameOver;
	public bool Win;
	
	public List<MobState> Mobs = new();
	public List<BulletState> Bullets = new();
	public PlayerState[] Players;

	public StateInstance()
	{
		Players = new PlayerState[]
		{
			new() { Index = 0, Position = new(140, 90) },
			new() { Index = 1, Position = new(180, 90) },
		};
	}

	private double _elapsed;
	private int _mobSpawned;

	public void Process(double delta)
	{
		_elapsed += delta;
		if (_mobSpawned != 20)
		{
			for (var i = _mobSpawned; i < (int)(_elapsed / 1); i++)
			{
				if (_mobSpawned == 20) break;
				var mp = new Vector2(GD.Randf() * 320, GD.Randf() * 180);
				while (!Players.All(p => (p.Position - mp).Length() > 60))
				{
					mp = new Vector2(GD.Randf() * 320, GD.Randf() * 180);
				}
				Mobs.Add(new MobState { Position = mp});
				_mobSpawned++;
			}
		}

		if (_mobSpawned == 20 && Mobs.Count == 0)
		{
			Win = true;
		}
	}
}

public abstract class ObjectState
{
	public Vector2 Position;
	public bool Created;
	public bool Deleted;
	public Node Node;
	public abstract void Process(double delta);
}

public class PlayerState : ObjectState
{
	public int Index;
	private bool FirePressed;
	private double FromLastFire;

	public override void Process(double delta)
	{
		var v = Input.GetVector($"player{Index}_left", $"player{Index}_right", $"player{Index}_up",
			$"player{Index}_down") * 40 * (float)delta;
		Position += v;
		
		foreach (var m in State.Instance.Mobs)
		{
			if ((m.Position-Position).Length() < 5)
			{
				//State.Instance.GameOver = true;
			}
		}

		FromLastFire += delta;
		var fire = Input.GetActionStrength($"player{Index}_rb") > 0;
		var direction = Input.GetVector($"player{Index}_direction_left", $"player{Index}_direction_right",
			$"player{Index}_direction_up",
			$"player{Index}_direction_down");
		if (fire && !FirePressed && FromLastFire > 0.3 && direction.Length() > 0.2)
		{
			var bullet = new BulletState();
			bullet.Position = Position;
			bullet.MoveVector = direction;
			State.Instance.Bullets.Add(bullet);
			FromLastFire = 0;
		}
		FirePressed = fire;
	}
}

public class MobState : ObjectState
{
	public override void Process(double delta)
	{
		var nearestPlayer = Position;
		var record = -1.0f;
		foreach (var playerState in State.Instance.Players)
		{
			var dist = (Position - playerState.Position).Length();
			if (record < 0 || dist < record)
			{
				record = dist;
				nearestPlayer = playerState.Position;
			}
		}

		Position = Position.MoveToward(nearestPlayer, (float)delta * 80);
	}
}

public class BulletState : ObjectState
{
	public Vector2 MoveVector;
	public override void Process(double delta)
	{
		Position = Position.MoveToward(Position + MoveVector*4, 4);
		
		foreach (var m in State.Instance.Mobs)
		{
			if ((m.Position-Position).Length() < 5)
			{
				m.Deleted = true;
				Deleted = true;
				break;
			}
		}
	}
}
