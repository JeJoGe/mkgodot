using Godot;
using System;

public partial class Player : Node2D
{
	const int MainLayer = 0;
	public override void _Ready()
	{	
		var mapGen = GetNode<MapGen>("../MapGen");
		//GD.Print(mapGen.ToGlobal(mapGen.MapToLocal(new Vector2I(0,0))));
		GlobalPosition = mapGen.ToGlobal(mapGen.MapToLocal(new Vector2I(0,0)));
	}
	public override void _Input(InputEvent @event)
    {
		var mapGen = GetNode<MapGen>("../MapGen");
		// Called on Input Event. For now, should only process mouse event Leftclick
		if (@event is InputEventMouseButton eventMouseButton)
			if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				// Converting global pixel coordinates to coordinates on the MapGen node then converting to the Hex coordinates of MapGen
				var globalClicked = GetGlobalMousePosition();    
				var posClicked = mapGen.LocalToMap(mapGen.ToLocal(globalClicked));
				GD.Print("TileMap: " + posClicked.ToString());
				// Atlas coordinates are the tile's coordinates on the atlas the tilemap is pulling tiles from
				var currentAtlasCoords = mapGen.GetCellAtlasCoords(MainLayer, posClicked);
				GD.Print("Atlas: " + currentAtlasCoords.ToString());

				mapGen.generateTile(currentAtlasCoords, posClicked);
			}
	}
}
