using Godot;
using System;
using System.Linq;

public partial class MapGen : TileMap
{
	const int MainLayer = 0;
	const int MainAtlasID = 0;
	const int MainTerrainSet = 0;

	Godot.Collections.Array greenTiles = new Godot.Collections.Array{2,3,4,5,6,7,8,9,10,11,12,13,14,15};
	Godot.Collections.Array brownTiles = new Godot.Collections.Array{16,17,18,19,20,21,22,23,24,25};
	public override void _Ready()
	{
		GD.Randomize();
		Random rSeed = new Random();
		greenTiles.Shuffle();
		brownTiles.Shuffle();
		// shuffleArray(rSeed, greenTiles);
		// shuffleArray(rSeed, brownTiles);

	}
	// Don't know where to place this as can reuse for many things
	// Shuffle array function based on Fisher-Yate algorithm
	public static void shuffleArray(Random rSeed, Godot.Collections.Array origArray)
	{
		//Step 1: For each unshuffled item in the collection
		for (int n = origArray.Count() - 1; n > 0; --n)
		{
			//Step 2: Randomly pick an item which has not been shuffled
			int k = rSeed.Next(n + 1);

			//Step 3: Swap the selected item with the last "unstruck" letter in the collection
			var temp = origArray[n];
			origArray[n] = origArray[k];
			origArray[k] = temp;
		}
	}
	// Copied Modulus function from online as C# % is remainder, not Modulus.
	// Implements a MOD b
	public static float Mod(float a, float b)
	{
    	float c = a % b;
    	if ((c < 0 && b > 0) || (c > 0 && b < 0))
    	{
        	c += b;
    	}
    	return c;
	}

	// Called on Input Event. For now, should only process mouse event Leftclick
	// On left click on Map tile, generate the 2x3x2 pattern of next random set of Map tiles 
    public override void _Input(InputEvent @event)
    {
		// Need to redeclare greenTiles for var functions instead of array
        if (@event is InputEventMouseButton eventMouseButton)
			if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
			{
				// Converting global pixel coordinates to coordinates on the MapGen node then converting to the Hex coordinates of MapGen
				var globalClicked = eventMouseButton.Position;
				var posClicked = LocalToMap(ToLocal(globalClicked));
				GD.Print("TileMap: " + posClicked.ToString());

				// Atlas coordinates are the tile's coordinates on the atlas the tilemap is pulling tiles from
				var currentAtlasCoords = GetCellAtlasCoords(MainLayer, posClicked);
				GD.Print("Atlas: " + currentAtlasCoords.ToString());
				if (currentAtlasCoords is (-1,-1)) // No tile from atlas exists here
				{
					GD.Print("No Pattern Detected");
					if (this.greenTiles[0] == null) // No green tiles left
					{
						if (this.brownTiles[0] == null) // No brown tiles left
						{
							throw new InvalidOperationException("No tiles available");
						}

						else
						{
							// Take pattern from randomized set from mainlayer of tileset and position at tilePos coords
							var tilePos = DetermineMapPlacement(posClicked);
							GD.Print("Add tile at " + tilePos.ToString());
							SetPattern(MainLayer, tilePos, TileSet.GetPattern(this.brownTiles[^1])); 
							var array1 = [3, 2];

						}
					}

					else
					{
						var tilePos = DetermineMapPlacement(posClicked);
							GD.Print("Add tile at " + tilePos.ToString());
							SetPattern(MainLayer, tilePos, TileSet.GetPattern(this.greenTiles[^1])); 
							var array1 = [3, 2];
					}
				}

				else
				{
					var cellTerrain = GetCellTileData(MainLayer, posClicked).Terrain; // Get terrain of tile
					GD.Print("Terrain: " + TileSet.GetTerrainName(MainTerrainSet, cellTerrain));
				}
			}
    }
	// Using math to determine how to position tile. Don't understand why it works, it just does
	private Vector2I DetermineMapPlacement(Vector2I posClicked)
	{
		var caseVal = (int)Mod( (posClicked.X - (2 * posClicked.Y)) , 7 );
		switch (caseVal)
		{
			// Case where position clicked would be a center tile
			case 0:
			{
				return new Vector2I(posClicked.X - 1, posClicked.Y - 1);
			}
			// Case where position clicked is immediately to the right of a center tile
			case 1:
			{
				return new Vector2I(posClicked.X - 2, posClicked.Y - 1);
			}
			// Case where position clicked is one of the top two hexes of a center tile
			case 2: case 3:
			{
				return new Vector2I(posClicked.X - (caseVal - 1), posClicked.Y);
			}
			// Case where position clicked is one of the bottom two hexes of a center tile
			case 4: case 5:
			{
				return new Vector2I(posClicked.X - (caseVal - 4), posClicked.Y - 2);
			}
			// Case (6) where position clicked is immediately to the left of a center tile
			default:
			{
				return new Vector2I(posClicked.X, posClicked.Y - 1);
			}
		}
	}

}
