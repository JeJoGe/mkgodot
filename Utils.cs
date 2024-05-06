using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Utils : Node
{
	private static readonly Random random = new Random();
	private static readonly object syncLock = new object();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static int RandomNumber(int min, int max)
	{
		lock(syncLock)
		{
			return random.Next(min,max);
		}
	}
}

public static class EnumerableExtensions
{
	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
		return source.ShuffleIterator();
	}

	// shuffle using fisher-yates algo
	public static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source) {
		var buffer = source.ToList();
		for (int i = 0; i < buffer.Count; i++)
		{
			int j = Utils.RandomNumber(i, buffer.Count);
			yield return buffer[j];

			buffer[j] = buffer[i];
		}
	}
}