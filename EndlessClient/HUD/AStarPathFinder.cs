using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.HUD
{
    [AutoMappedType]
    public class AStarPathFinder : IPathFinder
    {
        private readonly IWalkValidationActions _walkValidationActions;

        public AStarPathFinder(IWalkValidationActions walkValidationActions)
        {
            _walkValidationActions = walkValidationActions;
        }

        public Queue<MapCoordinate> FindPath(MapCoordinate start, MapCoordinate finish)
        {
            // Using a separate HashSet to manage already processed nodes
            var openSet = new PriorityQueue<MapCoordinate, int>();
            var openSetHash = new HashSet<MapCoordinate>();
            openSet.Enqueue(start, 0);
            openSetHash.Add(start);

            var cameFrom = new Dictionary<MapCoordinate, MapCoordinate>();
            var scores = new Dictionary<MapCoordinate, int> { { start, 0 } };
            var guessScores = new Dictionary<MapCoordinate, int> { { start, heuristic(start, finish) } };

            while (openSet.Count != 0)
            {
                var current = openSet.Dequeue();
                openSetHash.Remove(current);

                if (current.Equals(finish))
                    return reconstructPath(cameFrom, finish);

                foreach (var neighbor in getNeighborsOptimized(current))
                {
                    var tentativeScore = scores[current] + 1;
                    if (!scores.ContainsKey(neighbor) || tentativeScore < scores[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        scores[neighbor] = tentativeScore;
                        guessScores[neighbor] = tentativeScore + heuristic(neighbor, finish);

                        // Avoid re-adding if already in open set, but update values as needed.
                        if (!openSetHash.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, guessScores[neighbor]);
                            openSetHash.Add(neighbor);
                        }
                    }
                }
            }

            return new Queue<MapCoordinate>();
        }

        private static int heuristic(MapCoordinate current, MapCoordinate goal) => Math.Abs(current.X - goal.X) + Math.Abs(current.Y - goal.Y);

        private Queue<MapCoordinate> reconstructPath(Dictionary<MapCoordinate, MapCoordinate> cameFrom, MapCoordinate current)
        {
            var path = new LinkedList<MapCoordinate>();
            while (cameFrom.ContainsKey(current))
            {
                path.AddFirst(current);
                current = cameFrom[current];
            }
            return new Queue<MapCoordinate>(path);
        }

        private IEnumerable<MapCoordinate> getNeighborsOptimized(MapCoordinate current)
        {
            var deltas = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dx, dy) in deltas)
            {
                var newX = current.X + dx;
                var newY = current.Y + dy;
                if (_walkValidationActions.CanMoveToCoordinates(newX, newY) == WalkValidationResult.Walkable)
                {
                    yield return new MapCoordinate(newX, newY);
                }
            }
        }
    }

    public interface IPathFinder
    {
        Queue<MapCoordinate> FindPath(MapCoordinate start, MapCoordinate finish);
    }
}