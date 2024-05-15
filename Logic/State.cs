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
	public List<PlayerState> Players;
	public IEnumerable<ObjectState> VisualObjects => Players.Cast<ObjectState>().Concat(Mobs).Concat(Bullets);
	public int PlayersCount = 2;

	public StateInstance()
	{
		Players = new PlayerState[]
		{
			new() { Index = 0, Position = new(640/2-40, 360/2) },
			new() { Index = 1, Position = new(640/2+40, 360/2) },
		}.Take(PlayersCount).ToList();
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
				var mp = new Vector2(GD.Randf() * 640, GD.Randf() * 360);
				while (!Players.All(p => (p.Position - mp).Length() > 120))
				{
					mp = new Vector2(GD.Randf() * 640, GD.Randf() * 360);
				}
				Mobs.Add(new MobState { Position = mp, Index = _mobSpawned});
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
	public abstract void Process(double delta);
}

public class PlayerState : ObjectState
{
	public int Index;
	private bool FirePressed;
	private double FromLastFire;
	private Vector2 LastMove;
	public float Stamina = 10;
	public readonly float MaxStamina = 10;
	public float Hp = 3;
	public float MaxHp = 3;
	public float Mana = 10;
	public float MaxMana = 10;

	public override void Process(double delta)
	{
		foreach (var m in State.Instance.Mobs.ToList())
		{
			if ((m.Position-Position).Length() < 5)
			{
				State.Instance.Mobs.Remove(m);
				Hp--;
			}
		}
		
		if (Hp <= 0)
		{
			State.Instance.GameOver = true;
			return;
		}
		Stamina += (float)delta;
		if (Stamina > MaxStamina) Stamina = MaxStamina;

		if (FromLastFire > 3) Mana += (float)delta * 7;
		if (Mana > MaxMana) Mana = MaxMana;

		
		
		//Hp += (float)delta/200;
		if (Hp > MaxHp) Hp = MaxHp;


		var md = 1f;
		if (Input.GetActionStrength($"player{Index}_rl") > 0 && Stamina > 0)
		{
			md = 5;
			Stamina -= (float)delta * 5f;
		}
		
		var mv = Input.GetVector($"player{Index}_left", $"player{Index}_right", $"player{Index}_up",
			$"player{Index}_down");
		if (mv.Length() > 0)
		{
			LastMove = mv;
			Position += mv * 70 * (float)delta * md;
		}

		
		FromLastFire += delta;
		var fire = Input.GetActionStrength($"player{Index}_rb") > 0;
		if (fire && (!FirePressed || FromLastFire > 1) && FromLastFire > 0.3 && Mana > 0)
		{
			var direction = Input.GetVector($"player{Index}_direction_left", $"player{Index}_direction_right", $"player{Index}_direction_up", $"player{Index}_direction_down");

			if (direction.Length() == 0)
			{
				direction = LastMove;
			}

			direction = direction.Normalized();
			var bullet = new BulletState();
			bullet.Position = Position;
			bullet.MoveVector = direction.Rotated(Mathf.Pi*(GD.Randf()*0.06f-0.03f));
			bullet.PlayerOwner = Index;
			State.Instance.Bullets.Add(bullet);
			FromLastFire = 0;
			Mana -= 0.8f;
		}
		FirePressed = fire;
	}
}

public class MobState : ObjectState
{
	public int Index;
	public float Hp = 3;
	public float MaxHp = 3;
	public double Live;
	
	public override void Process(double delta)
	{
		Hp += (float)delta/8;
		if (Hp > MaxHp) Hp = MaxHp;
		
		Live += delta;
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
		
		
		if (Hp >= 2 && Hp <= 2.8)
		{
			foreach (var b in State.Instance.Bullets)
			{
				if (b.Position.DistanceTo(Position) > 1600) continue;
				var v = Geometry2D.LineIntersectsLine(b.Position, b.MoveVector, Position,
					(Position - nearestPlayer).Rotated(Mathf.Pi / 2));
				if (v.VariantType == Variant.Type.Nil) continue;
				var vector = v.AsVector2();
				if ((b.Position + b.MoveVector).DistanceTo(vector) > (b.Position).DistanceTo(vector))
				{
					continue;
				}

				if (Position.DistanceTo(vector) < 10)
				{
					nearestPlayer = (vector - Position) * -10 + Position;
					break;
				}
			}
		}

		if (Hp >= 2)
		{
			Position = Position.MoveToward(nearestPlayer, (float)(delta * (40 + (3 - Hp) * 30 + 10 * Live)));
		}
		else
		{
			if (Position.X is >= 20 and <= 620 && Position.Y is >= 20 and <= 340)
			{
				Position = Position.MoveToward(Position - (nearestPlayer - Position),
					(float)(delta * (40 + (3 - Hp) * 30 + 10 * Live)));
			}

			if (Position.X < 30) Position.X = 30;
			if (Position.X > 620) Position.X = 620;
			if (Position.Y < 30) Position.Y = 30;
			if (Position.Y > 340) Position.Y = 340;
		}
	}
}

public class BulletState : ObjectState
{
	public Vector2 MoveVector;
	public int PlayerOwner;
	
	public override void Process(double delta)
	{
		if (Position.X is < 0 or > 640 || Position.Y is < 0 or > 360)
		{
			State.Instance.Bullets.Remove(this);
			return;
		}
		Position = Position.MoveToward(Position + MoveVector*4, 4);
		
		foreach (var m in State.Instance.Mobs.ToList())
		{
			if ((m.Position-Position).Length() < 8)
			{
				m.Hp-=1.5f;
				State.Instance.Bullets.Remove(this);
				if (m.Hp <= 0)
				{
					State.Instance.Mobs.Remove(m);
					break;
				}
			}
		}
		
		foreach (var m in State.Instance.Players)
		{
			if (m.Index != PlayerOwner && (m.Position-Position).Length() < 8)
			{
				m.Hp-=1f;
				State.Instance.Bullets.Remove(this);
			}
		}
	}
}
