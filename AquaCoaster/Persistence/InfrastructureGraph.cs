using System;
using System.Collections.Generic;
using System.Linq;
using AquaCoaster.Model;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.Persistence
{
    public class InfrastructureGraph
    {

        private class Node
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Cost { get; set; }
            public int Distance { get; set; }
            public int CostDistance => Cost + Distance;
            public Node Parent { get; set; }

            public void SetDistance(int targetX, int targetY)
            {
                this.Distance = Math.Abs(targetX - X) + Math.Abs(targetY - Y);
            }
        }

        private readonly Infrastructure[,] infrastructures = new Infrastructure[GameModel.DEFAULT_ROWS, GameModel.DEFAULT_COLUMNS];
        private readonly List<Infrastructure> availableInfrastructures = new List<Infrastructure>();

        public IEnumerable<Infrastructure> InfrastructueElements
        {
            get => infrastructures.Cast<Infrastructure>().Select(x => x).Distinct();
        }

        public IEnumerable<Infrastructure> AvailableInfrastructureElements
        {
            get => availableInfrastructures;
        }

        public IEnumerable<Facility> AvailableFacilityElements
        {
            get => availableInfrastructures.Where(e => e is Facility).Select(i => i as Facility);
        }

        /// <summary>
        /// Returns the infrastructure at a given coordinate.
        /// <para>Note: the dimensions of the infrastructure are also considered.</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Infrastructure this[Int32 x, Int32 y]
        {
            get
            {
                if (x < 0 || x > infrastructures.GetLength(0) || y < 0 || y > infrastructures.GetLength(1))
                    return null;

                return infrastructures[x, y];
            }
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        public Point CoordinatesOf(Infrastructure infrastructure)
        {
            for (int x = 0; x < infrastructures.GetLength(0); x++)
            {
                for (int y = 0; y < infrastructures.GetLength(1); y++)
                {
                    if (infrastructures[x, y] != null)
                    {
                        if (infrastructures[x, y].Equals(infrastructure))
                        {
                            return new Point(x, y);
                        }
                    }
                }
            }

            return new Point(-1, -1);
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        /// <param name="coord"></param>
        public Infrastructure GetInfrastructureWithType(Type t)
        {
            for (int x = 0; x < infrastructures.GetLength(0); x++)
            {
                for (int y = 0; y < infrastructures.GetLength(1); y++)
                {
                    if (infrastructures[x, y] != null)
                    {
                        if (infrastructures[x, y].GetType().Equals(t))
                        {
                            return infrastructures[x, y];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        /// <param name="coord"></param>
        public void AddInfrastructure(Infrastructure infrastructure, Point coord)
        {
            for (int i = coord.X; i < coord.X + infrastructure.Size.X; i++)
            {
                for (int j = coord.Y; j < coord.Y + infrastructure.Size.Y; j++)
                {
                    infrastructures[i, j] = infrastructure;
                }
            }

            CheckAvailableInfrastructures();
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        public void RemoveInfrastructure(Infrastructure infrastructure)
        {
            Point coord = CoordinatesOf(infrastructure);

            for (int i = coord.X; i < coord.X + infrastructure.Size.X; i++)
            {
                for (int j = coord.Y; j < coord.Y + infrastructure.Size.Y; j++)
                {
                    if (infrastructure is LandGame)
                        infrastructures[i, j] = new Dirt();
                    else if (infrastructure is WaterGame)
                        infrastructures[i, j] = new Water();
                    else if (infrastructure is Pier)
                        infrastructures[i, j] = new Water();
                    else
                        infrastructures[i, j] = new Dirt();
                }
            }

            CheckAvailableInfrastructures();
        }

        /// <summary></summary>
        /// <param name="coord"></param>
        public void RemoveInfrastructure(Point coord)
        {
            infrastructures[coord.X, coord.Y] = null;
            CheckAvailableInfrastructures();
        }

        public void CheckAvailableInfrastructures()
        {
            Infrastructure[,] copy = (Infrastructure[,])infrastructures.Clone();
            availableInfrastructures.Clear();

            Queue<Point> queue = new Queue<Point>();

            Infrastructure gate = GetInfrastructureWithType(typeof(Gate));

            if (gate != null)
            {
                Point gatePos = CoordinatesOf(gate);
                Point gateSize = gate.Size;
                for (int i = gatePos.X; i < gatePos.X + gateSize.X; i++)
                {
                    for (int j = gatePos.Y; j < gatePos.Y + gateSize.Y; j++)
                    {
                        queue.Enqueue(new Point(i, j));
                    }
                }
            }

            while (queue.Count != 0)
            {
                Point popped = queue.Dequeue();
                if (popped.X > 0 && !availableInfrastructures.Contains(this[popped.X - 1, popped.Y])) { availableInfrastructures.Add(this[popped.X - 1, popped.Y]); }

                if (popped.X + 1 < GameModel.DEFAULT_ROWS && !availableInfrastructures.Contains(this[popped.X + 1, popped.Y])) { availableInfrastructures.Add(this[popped.X + 1, popped.Y]); }

                if (popped.Y > 0 && !availableInfrastructures.Contains(this[popped.X, popped.Y - 1])) { availableInfrastructures.Add(this[popped.X, popped.Y - 1]); }

                if (popped.Y + 1 < GameModel.DEFAULT_COLUMNS && !availableInfrastructures.Contains(this[popped.X, popped.Y + 1])) { availableInfrastructures.Add(this[popped.X, popped.Y + 1]); }

                if (popped.X > 0 && copy[popped.X - 1, popped.Y] is Road)
                {
                    queue.Enqueue(new Point(popped.X - 1, popped.Y));
                    copy[popped.X - 1, popped.Y] = null;
                }

                if (popped.X + 1 < GameModel.DEFAULT_ROWS && copy[popped.X + 1, popped.Y] is Road)
                {
                    queue.Enqueue(new Point(popped.X + 1, popped.Y));
                    copy[popped.X + 1, popped.Y] = null;
                }

                if (popped.Y > 0 && copy[popped.X, popped.Y - 1] is Road)
                {
                    queue.Enqueue(new Point(popped.X, popped.Y - 1));
                    copy[popped.X, popped.Y - 1] = null;
                }

                if (popped.Y + 1 < GameModel.DEFAULT_COLUMNS && copy[popped.X, popped.Y + 1] is Road)
                {
                    queue.Enqueue(new Point(popped.X, popped.Y + 1));
                    copy[popped.X, popped.Y + 1] = null;
                }
            }
        }

        /// <summary></summary>
        /// <param name="from"></param>
        /// <param name="targetInfrastructure"></param>
        public Queue<Point> GetShortestPath(Point from, Infrastructure targetInfrastructure, Infrastructure currentInfrastructure = null)
        {            
            Node startNode = new Node();
            startNode.X = from.X;
            startNode.Y = from.Y;
            Node endNode = new Node();
            endNode.X = CoordinatesOf(targetInfrastructure).X;
            endNode.Y = CoordinatesOf(targetInfrastructure).Y;

            Infrastructure target = this[endNode.X, endNode.Y];

            startNode.SetDistance(endNode.X, endNode.Y);
            List<Node> activeNodes = new List<Node>();
            activeNodes.Add(startNode);
            List<Node> visitedNodes = new List<Node>();

            while (activeNodes.Any())
            {
                Node checkNode = activeNodes.OrderBy(x => x.CostDistance).First();
                if ((checkNode.X == endNode.X && checkNode.Y == endNode.Y) || (this[checkNode.X, checkNode.Y] == target))
                {
                    Node node = checkNode;
                    Stack<Point> path = new Stack<Point>();
                    while (node != null)
                    {                        
                        path.Push(new Point(node.X, node.Y));
                        node = node.Parent;                            
                    }
                    Queue<Point> shortestPath = new Queue<Point>(path);
                    return shortestPath;
                }
                visitedNodes.Add(checkNode);
                activeNodes.Remove(checkNode);
                List<Node> availableNodes = GetAvailableNodes(checkNode, endNode, target);
                foreach (Node availableNode in availableNodes)
                {
                    if (activeNodes.Any(x => x.X == availableNode.X && x.Y == availableNode.Y))
                    {
                        Node existingNode = activeNodes.First(x => x.X == availableNode.X && x.Y == availableNode.Y);
                        if (existingNode.CostDistance > checkNode.CostDistance)
                        {
                            activeNodes.Remove(existingNode);
                            activeNodes.Add(availableNode);
                        }
                    }
                    else
                    { 
                        activeNodes.Add(availableNode);
                    }
                }
            }

            return new Queue<Point>();
        }

        private List<Node> GetAvailableNodes(Node currentNode, Node targetNode, Infrastructure target)
        {
            List<Node> nodes = new List<Node>()
            {
                new Node { X = currentNode.X, Y = currentNode.Y - 1, Parent = currentNode, Cost = currentNode.Cost + 1 },
                new Node { X = currentNode.X, Y = currentNode.Y + 1, Parent = currentNode, Cost = currentNode.Cost + 1 },
                new Node { X = currentNode.X - 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1 },
                new Node { X = currentNode.X + 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1 },
            };
            nodes.ForEach(node => node.SetDistance(targetNode.X, targetNode.Y));
            return nodes
                        .Where(node => node.X >= 0 && node.X < GameModel.DEFAULT_ROWS)
                        .Where(node => node.Y >= 0 && node.Y < GameModel.DEFAULT_COLUMNS)
                        .Where(node => this[node.X, node.Y] is Road || this[node.X, node.Y] == target || this[node.X, node.Y] is Gate)
                        .ToList();
        }
    }
}
