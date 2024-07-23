using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MapGen : TileMap
{
	[Export]
	private GameplayControl gameplayControl;
	const int MainLayer = 0;
	const int MainAtlasID = 0;
	const int MainTerrainSet = 0;
	public Dictionary<int, int?> terrainCosts = new Dictionary<int, int?>(){
		{0,2}, // Plains
		{1,3}, // Forest
		{2,3}, // Hills
		{3,5}, // Swamp
		{4,4}, // Wasteland
		{5,5}, // Desert
		{6,2}, // City
		{7,null}, // Mountain
		{8,null}, // Lake
		{9,null} // Nothing
	};

	//Initial stack of Green Tiles and Brown Tiles
	int[] brownTiles = Enumerable.Range(16, 10).ToArray();
	int[] greenTiles = Enumerable.Range(2, 14).ToArray();
	int tileStackState = 2; //2 is green, 1 is brown, 0 is empty
	Stack<int> tileStack;
	public override void _Ready()
	{
		GD.Randomize();
		Random rSeed = new Random();
		//ShuffleArray(rSeed, greenTiles);
		tileStack = new Stack<int> (greenTiles.Shuffle());
		//GD.Print("GreenTiles: " + string.Join("\n", greenTiles));
		//tileStack = new Stack<int>(greenTiles);
	}

	// Don't know where to place this as can reuse for many things
	// Shuffle array function based on Fisher-Yate algorithm
	public static void ShuffleArray<T>(Random rSeed, T[] origArray)
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

	// On left click on Map tile, generate the 2x3x2 pattern of next random set of Map tiles 
	public void GenerateTile(Vector2I currentAtlasCoords, Vector2I posClicked)
	{
		// Cant put tiles behind inital tiles (posClicked.X > 0), can't put tiles above upper bound (posClicked.Y > (-3 * (posClicked.X + 1) - 1)),
		// can't put tiles below lower bound (posClicked.X < (int)Math.Ceiling(-1.5 * posClicked.Y) + 2))
		if ((this.tileStackState != 0) & (posClicked.X > 0) & (posClicked.Y > (-3 * (posClicked.X + 1) - 1)) & (posClicked.X < (int)Math.Ceiling(-1.5 * posClicked.Y) + 2))
		{
			GD.Print("No Pattern Detected");
			if (this.tileStackState != 0)
			{
				// Take pattern from randomized set from mainlayer of tileset and position at tilePos coords
				var tilePos = this.DetermineMapPlacement(posClicked);
				GD.Print("Add tile at " + tilePos.ToString());
				var honeyComb = TileSet.GetPattern(this.tileStack.Pop());
				SetPattern(MainLayer, tilePos, honeyComb);
				var patternMapCoords = getPatternMapCoords(tilePos);

				// Populate tokens on pattern if needed
				foreach (var patternTile in patternMapCoords)
				{
					// Take custom data on tile under "Token" if any
					var patternTileData = GetCellTileData(MainLayer, patternTile);
					string tokenData = patternTileData.GetCustomData("Token").ToString();
					//GD.Print("Point 1: "+ tokenData);
					//GD.Print("Point 2: "+ patternTileData.GetCustomData("Token").ToString());
					if (tokenData != "" && tokenData != "yellow")
					{
						// May need to add in switch statement for whether token is flipped
						// Generate monster from color stack, site fortifications from what site it's on, on what tile
						gameplayControl.MonsterGen(tokenData, (patternTileData.GetCustomData("Event").ToString() == "") ? 0 : 1, patternTile);
					}
					else if (tokenData == "yellow")
					{
						// May need to add in switch statement for whether token is flipped
						// Generate monster from color stack, site fortifications from what site it's on, on what tile
						gameplayControl.MonsterGen(tokenData, (patternTileData.GetCustomData("Event").ToString() == "") ? 0 : 1, patternTile);
					}
				}

				if (this.tileStack.Count == 0) // No tiles left in stack, rebuild stack with brown tiles
				{
					this.tileStack = new Stack<int>(this.brownTiles.Shuffle());
					this.tileStackState--;
				}
			}

			else
			{
				throw new InvalidOperationException("No tiles available");
			}
		}

	}
	// Generate map coordinates for patttern given origin coordinates of pattern
	// Returns array of map coords of each tile in pattern
	private Vector2I[] getPatternMapCoords(Vector2I patternOrigin)
	{
		Vector2I[] patternMapCoords = new Vector2I[7];
		patternMapCoords[0] = new Vector2I(patternOrigin.X + 1, patternOrigin.Y);
		patternMapCoords[1] = new Vector2I(patternOrigin.X + 2, patternOrigin.Y);
		patternMapCoords[2] = new Vector2I(patternOrigin.X , patternOrigin.Y + 1);
		patternMapCoords[3] = new Vector2I(patternOrigin.X + 1, patternOrigin.Y + 1);
		patternMapCoords[4] = new Vector2I(patternOrigin.X + 2, patternOrigin.Y + 1);
		patternMapCoords[5] = new Vector2I(patternOrigin.X, patternOrigin.Y + 2);
		patternMapCoords[6] = new Vector2I(patternOrigin.X + 1, patternOrigin.Y + 2);

		return patternMapCoords;
	}

	// Using math to determine how to position tile. Don't understand why it works, it just does
	private Vector2I DetermineMapPlacement(Vector2I posClicked)
	{
		var caseVal = (int)Mod((posClicked.X - (2 * posClicked.Y)), 7);
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
			case 2:
			case 3:
				{
					return new Vector2I(posClicked.X - (caseVal - 1), posClicked.Y);
				}
			// Case where position clicked is one of the bottom two hexes of a center tile
			case 4:
			case 5:
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
