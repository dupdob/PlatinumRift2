using System;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class ClonesHard
{

    static void Log(string msg, params object[] pars)
    {
        Console.Error.Write(msg, pars);
    }

    static void LogLine()
    {
        Console.Error.WriteLine();
    }

    static void LogLine(string msg, params object[] pars)
    {
        Console.Error.WriteLine(msg, pars);
    }

    class Context
    {
        public enum Actions
        {
            Wait,
            Block,
            Lift
        };
        private readonly Dictionary<int, List<int>> elevatorPos;
        private readonly Dictionary<int, List<int>> blockerPos;

        public int NbTotalClones; // number of generated clones
        public int NbRounds;
        public int NbLifts;
        public int ExitFloor;
        public int ExitPos;
        private int freeLifts;
        private readonly int nbFloors;
        //private readonly int CloneFrequency ;

        public Context(int nbFloors)
        {
            this.nbFloors = nbFloors;
            this.blockerPos = new Dictionary<int, List<int>>(nbFloors);
            this.elevatorPos = new Dictionary<int, List<int>>(nbFloors);
        }

        public Context(Context context)
        {
            this.elevatorPos = new Dictionary<int, List<int>>(context.elevatorPos);
            this.blockerPos = new Dictionary<int, List<int>>(context.blockerPos);
            this.NbTotalClones = context.NbTotalClones;
            this.NbRounds = context.NbRounds;
            this.NbLifts = context.NbLifts;
            this.freeLifts = context.freeLifts;
            this.nbFloors = context.nbFloors;
        }

        public void Init()
        {
            this.freeLifts = this.NbLifts;
            // compute nb of 'free lifts' to shorten pah
			for (var i = 0; i < Math.Min(this.nbFloors, this.ExitFloor-1); i++)
            {
                if (i == this.ExitFloor)
                    continue;
                if (!this.elevatorPos.ContainsKey(i) || this.elevatorPos[i].Count == 0)
                {
                    this.freeLifts--;
                }
            }
            LogLine("Free Lifts: {0} ", this.freeLifts);
        }

        public void Wait()
        {
            Console.WriteLine("WAIT"); // action: WAIT or BLOCK
        }

        public void AddElevator(int elevatorFloor, int elevatorPosX)
        {
//            LogLine("Elevator at {0} {1}", elevatorFloor, elevatorPosX);
            if (!this.elevatorPos.ContainsKey(elevatorFloor))
            {
                this.elevatorPos[elevatorFloor] = new List<int>(2);
            }
            this.elevatorPos[elevatorFloor].Add(elevatorPosX);
            this.elevatorPos[elevatorFloor].Sort();
        }

        private int FirstElevatorOnLeft(int floor, int x)
        {
            var result = -1;
            // no lift at this level
			if (this.elevatorPos.ContainsKey (floor)) {
				foreach (var val in this.elevatorPos[floor])
				{
					if (val > x)
						break;
					result = val;
				}
			}

            return result;
        }

        private int FirstElevatorOnRight(int floor, int x)
        {
            var result = -1;
			if (this.elevatorPos.ContainsKey (floor)) {
				var lifts = this.elevatorPos[floor];
				for(var i = lifts.Count-1; i >= 0; i--)
				{
					var val = lifts[i];
					if (val < x) break;
					result = val;	
				}
			}

            return result;
        }

        public void ShortestPath(int floor, int x, string direction, out Actions action)
        {
            iShortestPath(floor, floor, x, direction, out action);
        }

        private int iShortestPath(int floor, int refFloor, int x, string direction, out Actions action)
        {
            var left = this.FirstElevatorOnLeft(floor, x);
            var right = this.FirstElevatorOnRight(floor, x);
			var leftDist = int.MaxValue;
            var rightDist = leftDist;
            Actions leftAction = Actions.Wait, rightAction = Actions.Wait;
            Actions dump;

			if (floor == this.ExitFloor) {
				if (floor == refFloor)
					LogLine ("Reached last level");
				if (x < this.ExitPos) {
					// exit is on right
					if (right < this.ExitPos) {
						action = Actions.Wait;
						// there is a lift on front, we can't reach exist
						return leftDist;
					}
					rightDist = this.ExitPos - x;
					if (direction == "LEFT") {
						action = Actions.Block;
						rightDist += 3;
					} else
						action = Actions.Wait;
					return rightDist;
				} else if (x>this.ExitPos) {
					// exit is on left
					if (left > this.ExitPos) {
						// there is a lift on front, we can't reach exit
						action = Actions.Block;
						return leftDist;
					}
					leftDist = x-this.ExitPos;
					if (direction == "RIGHT") {
						action = Actions.Block;
						leftDist += 3;
					} else
						action = Actions.Wait;
					return leftDist;
				}
				action = Actions.Wait;
				return 0;
			}

			bool noLift = (right == -1) && (left == -1);
            if (left != -1)
            {
				// lift on left
                string nextDir;
				var nbClones = this.NbTotalClones;
                leftDist = x - left;
                if (direction == "RIGHT")
                {
					// adjust left distance if we need a blocker clone
					if (NbTotalClones > 1) {
						nextDir = "LEFT";
						leftAction = Actions.Block;
						leftDist += 3;
						this.NbTotalClones--;
					} else {
						leftDist = int.MaxValue;
						nextDir = direction;
					}
                }
                else
                {
                    nextDir = direction;
                }	
                // we can try the left lift
				var next = this.iShortestPath (floor + 1, refFloor, left, nextDir, out dump);
				this.NbTotalClones = nbClones;
				leftDist = (next==int.MaxValue) ? next : leftDist + next;
            }
			if (NbTotalClones>1 && (this.freeLifts > 0 || noLift))
			{
				var tmpFreeLifts = this.freeLifts;
				var nbClones = this.NbTotalClones;
				if (!noLift)
					this.freeLifts--;
				this.NbTotalClones--;
				var next = this.iShortestPath (floor + 1, refFloor, x, direction, out dump);
				next = (next==int.MaxValue) ? next : next + 3;
				if (floor == refFloor) {
					LogLine ("If we build a lift at {0}:{1}, path is {2}", x, refFloor, next);
				}
				if (next < leftDist) {
					leftDist = next;
					leftAction = Actions.Lift;
					left = 0;
				}
				this.freeLifts = tmpFreeLifts;
				this.NbTotalClones = nbClones;
			}
			// lift on right
            if (right != -1)
            {
                string nextDir;
				var nbClones = this.NbTotalClones;
                // we can try the right lift
                rightDist = right - x;
                if (direction == "LEFT" && rightDist > 0)
                {
					if (this.NbTotalClones > 1) {
						nextDir = "RIGHT";
						rightAction = Actions.Block;
						this.NbTotalClones--;
						rightDist += 3;
					} else {
						nextDir = direction;
						rightDist = int.MaxValue;
					}
                }
                else
                {
                    nextDir = direction;
                }
				var next = this.iShortestPath (floor + 1, refFloor, right, nextDir, out dump);
				rightDist = (next==int.MaxValue) ? next: rightDist + next;
				this.NbTotalClones = nbClones;
            }

			if (Math.Min (rightDist, leftDist) == int.MaxValue && (floor == this.ExitFloor - 1) && this.NbTotalClones > 1 && this.freeLifts>0) {
				// exit cannot be reached, there is a trap
				if (x == this.ExitPos) {
					rightAction = Actions.Lift;
					rightDist = 3;
				}
				if (x < this.ExitPos && (right == -1 || right > this.ExitPos)) {
					// we can put a lift on the right
					rightDist = 3 + this.ExitPos - x;
					if (direction == "LEFT") {
						rightAction = Actions.Block;
						if (this.NbTotalClones > 2) {
							rightDist += 3;
						} else {
							rightDist = int.MaxValue;
						}
					} else {
						rightAction = Actions.Wait;
					}
				}
				if (x > this.ExitPos && (left==-1|| left < this.ExitPos)) {
					// we can put a lift on the left
					leftDist = 3 + x - this.ExitPos;
					if (direction == "RIGHT") {
						leftAction = Actions.Block;
						if (this.NbTotalClones > 2) {
							leftDist += 3;
						} else {
							leftDist = int.MaxValue;
						}
					} else {
						leftAction = Actions.Wait;
					}
				}
			}
			var retDist = int.MaxValue;
			if (floor == refFloor) {
				LogLine ("left: {0}, right : {1}", leftDist, rightDist);
			}
            if ((leftDist < rightDist) || (leftDist == rightDist && leftAction == Actions.Wait))
            {
                if (floor == refFloor)
                {
                    LogLine("Shortest path is left");
                }
                action = leftAction;
                retDist= leftDist;
            }
            else
            {
                if (floor == refFloor)
                {
                    LogLine("Shortest path is right");
                }
                action = rightAction;
                retDist = rightDist;
            }
			return retDist;
        }

        public void Block(int blockerFloor, int blockerPosX)
        {
            if (!this.blockerPos.ContainsKey(blockerFloor))
            {
                this.blockerPos[blockerFloor] = new List<int>(2);
            }
            else if (this.blockerPos[blockerFloor].Contains(blockerPosX))
            {
                this.Wait();
                return;
            }
            LogLine("Blocker at {0} {1}", blockerFloor, blockerPosX);
            this.blockerPos[blockerFloor].Add(blockerPosX);
            this.blockerPos[blockerFloor].Sort();

            this.NbTotalClones--;

            Console.WriteLine("BLOCK"); // action: WAIT or BLOCK
        }

        public void Elevator(int level, int x)
        {
			if (this.elevatorPos.ContainsKey (level)) {
				this.freeLifts--;
				LogLine("Free Lift at {0} {1}", level, x);
			} else {
				LogLine("Lift at {0} {1}", level, x);
			}
            this.AddElevator(level, x);
            this.NbLifts--;
			this.NbTotalClones--;
            Console.WriteLine("ELEVATOR"); // action: WAIT or BLOCK
        }
    }

    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var nbFloors = int.Parse(inputs[0]); // number of floors
        var width = int.Parse(inputs[1]); // width of the area
        var context = new Context(nbFloors);
        context.NbRounds = int.Parse(inputs[2]); // maximum number of rounds
        var exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
        var exitPos = int.Parse(inputs[4]); // position of the exit on its floor
        context.NbTotalClones = int.Parse(inputs[5]); // number of generated clones
        context.NbLifts = int.Parse(inputs[6]); // ignore (always zero)
        context.ExitFloor = exitFloor;
        context.ExitPos = exitPos;
        var nbElevators = int.Parse(inputs[7]); // number of elevators

        LogLine("Nb Rounds: {0}", context.NbRounds);
        LogLine("Nb Clones: {0}", context.NbTotalClones);
        LogLine("Nb Lifts allowed :{0}", context.NbLifts);

        for (int i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPosX = int.Parse(inputs[1]); // position of the elevator on its floor
            context.AddElevator(elevatorFloor, elevatorPosX);
        }

        context.Init();
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            var cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            var clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            var direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

            if (cloneFloor < 0)
            {
                Console.Error.WriteLine("No Clone");
                context.Wait();
                continue;
            }
            Console.Error.WriteLine("Clone at {0} {1} toward {2}", cloneFloor, clonePos, direction);

            Context.Actions act;
            context.ShortestPath(cloneFloor, clonePos, direction, out act);
            switch (act)
            {
                case Context.Actions.Wait:
                    context.Wait();
                    break;
                case Context.Actions.Block:
                    context.Block(cloneFloor, clonePos);
                    break;
                case Context.Actions.Lift:
                    context.Elevator(cloneFloor, clonePos);
                    break;
            }
            context.NbRounds--;
        }
    }
}