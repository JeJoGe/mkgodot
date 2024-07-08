using Godot;
using System;

public partial class MapTokenControl : Control
{
	Player player;
	MapGen mapGen;
	MapToken mapToken;
	private bool hover;
	const int MainLayer = 0;
	const int MainTerrainSet = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//this.MouseFilter = MouseFilterEnum.Stop;
		player = GetNode<Player>("../../Player");
		mapGen = GetNode<MapGen>("../../MapGen");
		mapToken = GetNode<MapToken>("..");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	// Zoom if shift pressed and hovering token
	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("shift") && hover == true)
		{
			GetNode<Sprite2D>("Sprite2D").Scale = new Vector2((float)1,(float)1);
			GetNode<Sprite2D>("Sprite2D").ZIndex = 1;
		}
		else if (Input.IsActionJustReleased("shift") || hover == false)
		{
			GetNode<Node2D>("Sprite2D").Scale = new Vector2((float)0.25,(float)0.25);
			GetNode<Sprite2D>("Sprite2D").ZIndex = 0;
		}
		
	}
	private void OnMouseEntered() {
		hover = true;
	}

	private void OnMouseExited() {
		hover = false;
	}
	public override void _GuiInput(InputEvent @event)
	{
		if (@event.IsActionPressed("leftClick"))
		{
			if (mapGen.GetSurroundingCells(player.playerPos).Contains(mapToken.MapPosition))
			{
				if(mapToken.Colour != "green" && mapToken.Colour != "red")
				{
					var globalClicked = GetGlobalMousePosition();
					var posClicked = mapGen.LocalToMap(mapGen.ToLocal(globalClicked));
					var cellTerrain = mapGen.GetCellTileData(MainLayer, posClicked).Terrain;
					if (player.MovePoints >= mapGen.terrainCosts[cellTerrain])
					{
						var wallBetweenPlayerAndEnemy = player.IsWallBetween(player.playerPos, mapToken.MapPosition);
						var monsterTerrain = mapGen.TileSet.GetTerrainName(MainTerrainSet, mapGen.GetCellTileData(MainLayer, mapToken.MapPosition).Terrain);
						if (wallBetweenPlayerAndEnemy == true && (monsterTerrain == "tower" || monsterTerrain == "keep" || monsterTerrain.Contains("castle"))) // double fortified
						{
							GameSettings.EnemyList.Add(new Vector2I(mapToken.MonsterId, 2));
						}
						else if (monsterTerrain == "tower" || monsterTerrain == "keep" || monsterTerrain.Contains("castle"))
						{
							GameSettings.EnemyList.Add(new Vector2I(mapToken.MonsterId, 1));
						}
						else
						{
							GameSettings.EnemyList.Add(new Vector2I(mapToken.MonsterId, 0));
						}
						player.PerformMovement(posClicked, cellTerrain);
					}
					else
					{
						GD.Print("Terrain costs more movement than you currently have.");
					}
				}
			}	
		}
	}
}
