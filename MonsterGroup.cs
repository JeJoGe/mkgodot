using Godot;
using System;
using System.Collections.Generic;
public partial class MonsterGroup : Node2D
{
    public List<MapToken> MonsterList = new List<MapToken>();
    public Vector2I MapPosition { get; set; }

    public override void _Ready()
	{
        
	}
}