﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

/**
	 * Auto-generated code below aims at helping you parse
	 * the standard input according to the problem statement.
	 **/
class Zone
{
	private int owner=-1;
	static private Random rnd = new Random();
	static public int MyID;

	public List<Zone> Links = new List<Zone>();
	public int ID;
	public bool EnemyBase;

	public List<int> DeadFrom = new List<int>();

	public Dictionary<int, int> Pods = new Dictionary<int, int>(); 
	public int Platinum;
	public int Owner
	{
		get { return this.owner;}
		set {
			if (this.owner != value) {
				this.owner = value;
				//Player.LogLine("Z#{0}: New Owner: {1}", this.ID, this.owner);
				this.DeadFrom.Clear ();
				if (value != MyID)
					VisitCount = 0;
			}
		}
	}

	public int VisitCount;

	public int ComputeScore(int from)
	{
		var offset = rnd.Next (10);
		if (this.EnemyBase)
			return int.MaxValue;
		var path = new List<int> ();
		path.Add (from);
		if (DeadEndPath (path)) {
			return 0;
		}
		if (owner == MyID) {
			offset += Links.Count - VisitCount;
		} else if (Platinum > 0) {
			offset += 500 * (Platinum + 1) + Links.Count;
		} else if (Owner != MyID) {
			offset += 100 + Links.Count;
		} else {
			offset += 5 + Links.Count;
		}
		return Math.Max(offset, 0);
	}

	public bool DeadEndPath(List<int> from)
	{
		var lastZone = from.Last();
		if (this.DeadFrom.Contains(lastZone))
			return true;
		if ((Platinum > 0 && Owner!=MyID) || EnemyBase || this.Links.Count>6)
			return false;
		var nextList = new List<int> (from);
		nextList.Add (ID);
		foreach (var next in this.Links) {
			if (nextList.Contains(next.ID))
				continue;
			if (!next.DeadEndPath (nextList))
				return false;
		}
		Player.LogLine("Zone #{0} is dead end from {1}!", ID, lastZone);
		this.DeadFrom.Add(lastZone);
		return true;
	}
}

class Pod
{
	public int CurrentZone;
}

class Player
{
	static void Main(string[] args)
	{
		LogLine ("dupdob: Algo V2.2");
		string[] inputs = Console.ReadLine().Split(' ');
		//		int playerCount = int.Parse(inputs[0]); // the amount of players (2 to 4)
		Zone.MyID = int.Parse(inputs[1]); // my player ID (0, 1, 2 or 3)
		LogLine ("MyId:{0}", Zone.MyID);
		int zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
		int linkCount = int.Parse(inputs[3]); // the amount of links between all zones
		var zones = new Dictionary<int, Zone>(zoneCount);
		for (var i = 0; i < zoneCount; i++)
		{
			var zone = new Zone();
			inputs = Console.ReadLine().Split(' ');
			zone.ID = int.Parse (inputs [0]);
			zones[zone.ID] = zone;
			zone.Platinum = int.Parse(inputs[1]);
		}
		for (var i = 0; i < linkCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			var z1 = int.Parse (inputs [0]);
			var z2 = int.Parse (inputs [1]);
			zones[z1].Links.Add(zones[z2]);
			zones[z2].Links.Add (zones [z1]);
		}

		//		DumpTerrain (zones);

		bool firstRound = true;
		// game loop
		for(;;)
		{
			var myPods = new List<Pod>(10);

			int platinum = int.Parse(Console.ReadLine()); // my available Platinum
			for (int i = 0; i < zoneCount; i++)
			{
				inputs = Console.ReadLine().Split(' ');
				var zone = zones[int.Parse(inputs[0])];
				zone.Owner = int.Parse(inputs[1]);
				for (var j = 0; j < 4; j++)
				{
					var pods = int.Parse(inputs[2 + j]);
					zone.Pods[j] = pods;
					if (j == Zone.MyID) {
						var pod = new Pod ();
						pod.CurrentZone = i;
						for (var k = 0; k < pods; k++)
							myPods.Add (pod);
					} else if (firstRound && pods > 0) {
						zone.EnemyBase = true;
					}
				}
			}

			// Write an action using Console.WriteLine()
			// To debug: Console.Error.WriteLine("Debug messages...");
			for (var i = 0; i < myPods.Count; i++) {
				var pod = myPods [i];
				var zone = zones [pod.CurrentZone];
				var nbNeighbour = zone.Links.Count;
				var scoring = new SortedList<int, int> (6);
				// pod logic: scan all neigbour, pick best one
				for (var j = 0; j < nbNeighbour; j++) {
					var neighbour = zone.Links [j];
					scoring [neighbour.ComputeScore (pod.CurrentZone)] = neighbour.ID;
				}

				Log ("Pod #{0} @ {1}:", i, pod.CurrentZone);
				foreach (var dump in scoring) {
					Log ("{0} {1};", dump.Value, dump.Key);
				}
				LogLine ();
				var nextZone = scoring.Last ().Value;
				zones [nextZone].VisitCount++;
				zones [nextZone].Owner = Zone.MyID;
				Console.Write ("1 {0} {1} ", pod.CurrentZone, nextZone);
			}
			Console.WriteLine(""); // first line for movement commands, second line for POD purchase (see the protocol in the statement for details)
			Console.WriteLine("WAIT");
			firstRound = false;
		}
	}

	static void DumpTerrain (Dictionary<int, Zone> zones)
	{
		foreach (var zone in zones) {
			Log ("#{0}: P:{1} O:{2};" , zone.Value.ID, zone.Value.Platinum, zone.Value.Owner);
			if (zone.Value.EnemyBase) {
				Console.Error.Write ("Enemmy base; ");
			}
			Log ("Next to: ");
			foreach(var next in zone.Value.Links)
			{
				Log ("{0},", next.ID);
			}
			LogLine ();
		}
	}	

	public static void Log(string fmt, params object[] pars)
	{
		Console.Error.Write (fmt, pars);
	}
	public static void LogLine(string fmt, params object[] pars)
	{
		Console.Error.WriteLine (fmt, pars);
	}
	public static void LogLine()
	{
		Console.Error.WriteLine ();
	}
} 
