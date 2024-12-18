using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ChallengePopUp : Control
{	
	[Export]
	private ChallengeWindow ChallengeWindow;
	[Export]
	private ChallengeOptions ChallengeOptions;
	[Export]
	private Button Challenge;
	[Export]
	private Button Cancel;
	List<CheckBox> enemyCheckboxes;
	GameplayControl gameplayControl;
	MapGen mapGen;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		var optionOffset = 0;
		gameplayControl = GetNode<GameplayControl>("../..");
		mapGen = GetNode<MapGen>("../../MapGen");
		enemyCheckboxes = new List<CheckBox>();
		GetNode<ColorRect>("ColorRect").Size = new Vector2 (350, 132 + (32 * GameSettings.ChallengeList.Count));
		GetNode<Window>("..").Size = new Vector2I(350, 164 + (32 * GameSettings.ChallengeList.Count));
		GetNode<PanelContainer>("PanelContainer").Size = new Vector2 (310, 32 * GameSettings.ChallengeList.Count);
		ChallengeOptions.Size = new Vector2 (310, 32 * GameSettings.ChallengeList.Count);
		foreach (var enemy in GameSettings.ChallengeList)
		{
			enemyCheckboxes.Add(ChallengeOptions.createChallengeOption(enemy.TokenId, optionOffset, enemy.PosColour, enemy.OldPosColour, enemy.Colour, enemy.Facedown));
			optionOffset += 32;
		}
		Challenge.Position = new Vector2 (0, 132 + optionOffset);
		Cancel.Position = new Vector2 (175, 132 + optionOffset);
		//GetTree().Paused = true;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_challenge_pressed()
	{
		foreach (var checkBox in enemyCheckboxes)
		{
			for (var enemy = GameSettings.ChallengeList.Count -1; enemy >= 0; enemy--)
			{	
				var tokenName = "";
				// enemy is Placeholder/Unknown Color Token when Token ID = -1
				if(GameSettings.ChallengeList[enemy].TokenId == -1)
				{
					// if token is place holder token and name includes a color of matching unchecked checkbox for a placeholder token that includes that color
					if(checkBox.Text.Contains("Token") && checkBox.Text.Contains(GameSettings.ChallengeList[enemy].Colour) && checkBox.ButtonPressed == false)
					{
						GameSettings.ChallengeList.Remove(GameSettings.ChallengeList[enemy]);
						break;
					}
				}
				else
				{
					// separate if statement to avoid -1 index when checking Bestiary
					tokenName = Utils.Bestiary[GameSettings.ChallengeList[enemy].TokenId].Name;
					//if checkbox text (subject to change) equals monster id and it's checkbox is unchecked. go backwards because removing from list while iterating
					if (checkBox.Text == Utils.Bestiary[GameSettings.ChallengeList[enemy].TokenId].Name && checkBox.ButtonPressed == false)
					{
						GameSettings.ChallengeList.Remove(GameSettings.ChallengeList[enemy]);
						break;
					}
				}
			}
		}
		if (GameSettings.ChallengeList.Count != 0){
			foreach (var enemy in GameSettings.ChallengeList)
			{
				// Enemy is placeholder if Token ID = -1
				if(enemy.TokenId == -1)
				{
					foreach (var monster in mapGen.MapData[enemy.MapPosition].MonsterGroup.ToList())
					{
						if(enemy.Colour == monster.Colour)
						{
							mapGen.MapData[enemy.MapPosition].MonsterGroup.Remove(monster);
							var replacementMonster = gameplayControl.MonsterGen(enemy.Colour, enemy.SiteFortifications, enemy.MapPosition);
							replacementMonster.Facedown = false;
							mapGen.MapData[enemy.MapPosition].MonsterGroup.Add(replacementMonster);
							GameSettings.EnemyList.Add((mapGen.MapData[enemy.MapPosition].MonsterGroup[mapGen.MapData[enemy.MapPosition].MonsterGroup.Count-1].TokenId, enemy.SiteFortifications, enemy.PosColour));
							break;
						}	
					}
				}
				else
				{
					GameSettings.EnemyList.Add((enemy.TokenId, enemy.SiteFortifications, enemy.PosColour));
				}	
			}
			//TODO: put in source of monster so only monstergen when monster stays on map
			gameplayControl.MapUpdateOnPlayerMovement(ChallengeWindow.posClicked, ChallengeWindow.cellTerrain, ChallengeWindow.movementMod, ChallengeWindow.mapEvent);
			GetNode<Player>("../../Player").InitiateCombat();
		}
		//GetTree().Paused = false;
		GetNode<ChallengeWindow>("..").QueueFree();
	}
	private void _on_cancel_pressed()
	{
		//Utils.undoRedo.Undo();
		GameSettings.ChallengeList.Clear();
		gameplayControl.UpdateTokenColors(GetNode<Player>("../../Player").PlayerPos);
		//GetTree().Paused = false;
		GetNode<ChallengeWindow>("..").QueueFree();
	}
}
