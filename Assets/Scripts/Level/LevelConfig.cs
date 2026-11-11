using System.Collections.Generic;
using UnityEngine;

namespace Game.Level
{
    public enum LilyPadType
    {
        Normal,
        Breakable,
        WinPad,
        StartPad,
        Gate,
        Trap
    }

    public enum LilyPadDirection
    {
        All,
        Horizontal,
        Vertical,
    }

    public static class LevelConfig
    {
        public class LevelData
        {
            public int levelId;
            public int rows;
            public int cols;
            public int[,] gridMask;
            public LilyPadType[,] lilyPadTypes;
            public LilyPadDirection[,] lilyPadDirections;
        }

        static readonly Vector2Int[] Cardinal =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public static LevelData Generate(int levelId, int rows, int cols)
        {
            rows = Mathf.Max(3, rows);
            cols = Mathf.Max(4, cols);
            Random.InitState(LevelSeeds.For(levelId));

            for (int attempt = 0; attempt < 30; attempt++)
            {
                LevelData data = TryGenerate(levelId, rows, cols);
                if (data != null && CanReachWin(data))
                    return data;
            }

            return Fallback(rows, cols);
        }

        static LevelData TryGenerate(int levelId, int rows, int cols)
        {
            int mainLength = Mathf.Clamp(8 + levelId, 8, Mathf.Min(14, rows + cols));
            List<Vector2Int> main = CarvePath(rows, cols, mainLength);
            if (main == null || main.Count < 5)
                return null;

            var occupied = new HashSet<Vector2Int>(main);
            var sideCells = new List<Vector2Int>();
            var entrances = new List<Vector2Int>();

            int spurs = Mathf.Clamp(1 + levelId / 2, 2, 3);
            for (int i = 0; i < spurs; i++)
                GrowSpur(main, occupied, sideCells, entrances, rows, cols);

            TryAddFork(main, occupied, sideCells, rows, cols);

            var traps = new List<Vector2Int>();
            AddArrowTraps(main, occupied, traps, rows, cols);

            if (sideCells.Count == 0)
                return null;
            if (Span(main) < 2)
                return null;

            return Build(levelId, main, occupied, entrances, traps);
        }

        static List<Vector2Int> CarvePath(int rows, int cols, int length)
        {
            var path = new List<Vector2Int>();
            var occupied = new HashSet<Vector2Int>();
            Vector2Int start = new Vector2Int(Random.Range(0, Mathf.Max(1, cols / 3)), Random.Range(0, rows));
            path.Add(start);
            occupied.Add(start);

            int guard = 0;
            while (path.Count < length && guard++ < 80)
            {
                Vector2Int current = path[path.Count - 1];
                List<Vector2Int> open = OpenNeighbors(current, rows, cols, occupied);
                if (open.Count == 0)
                    break;

                Vector2Int heading = path.Count >= 2 ? current - path[path.Count - 2] : Vector2Int.zero;
                List<Vector2Int> thin = new List<Vector2Int>();
                for (int i = 0; i < open.Count; i++)
                {
                    if (!WouldFillSquare(open[i], occupied))
                        thin.Add(open[i]);
                }

                if (thin.Count == 0)
                    break;

                List<Vector2Int> turns = new List<Vector2Int>();
                for (int i = 0; i < thin.Count; i++)
                {
                    if (thin[i] - current != heading)
                        turns.Add(thin[i]);
                }

                List<Vector2Int> pool = turns.Count > 0 && Random.value < 0.45f ? turns : thin;
                Vector2Int next = pool[Random.Range(0, pool.Count)];
                path.Add(next);
                occupied.Add(next);
            }

            return path.Count >= 5 ? path : null;
        }

        static void GrowSpur(List<Vector2Int> main, HashSet<Vector2Int> occupied, List<Vector2Int> sideCells, List<Vector2Int> entrances, int rows, int cols)
        {
            int anchorIndex = Random.Range(1, main.Count - 1);
            Vector2Int cursor = main[anchorIndex];
            int spurLength = 2;

            for (int step = 0; step < spurLength; step++)
            {
                List<Vector2Int> options = EmptyNeighbors(cursor, occupied, rows, cols);
                List<Vector2Int> isolated = new List<Vector2Int>();
                for (int i = 0; i < options.Count; i++)
                {
                    if (TouchesOnly(options[i], cursor, occupied) && !WouldFillSquare(options[i], occupied))
                        isolated.Add(options[i]);
                }

                if (isolated.Count == 0)
                    return;

                cursor = isolated[Random.Range(0, isolated.Count)];
                occupied.Add(cursor);
                sideCells.Add(cursor);
                if (step == 0)
                    entrances.Add(cursor);
            }
        }

        static void TryAddFork(List<Vector2Int> main, HashSet<Vector2Int> occupied, List<Vector2Int> sideCells, int rows, int cols)
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                int index = Random.Range(1, main.Count - 1);
                Vector2Int from = main[index];
                List<Vector2Int> firstSteps = EmptyNeighbors(from, occupied, rows, cols);
                if (firstSteps.Count == 0)
                    continue;

                Vector2Int mid = firstSteps[Random.Range(0, firstSteps.Count)];
                if (!TouchesOnly(mid, from, occupied) || WouldFillSquare(mid, occupied))
                    continue;

                List<Vector2Int> secondSteps = EmptyNeighbors(mid, occupied, rows, cols);
                for (int i = 0; i < secondSteps.Count; i++)
                {
                    Vector2Int land = secondSteps[i];

                    for (int j = index + 2; j < main.Count - 1; j++)
                    {
                        if (!Adjacent(land, main[j]))
                            continue;
                        if (!TouchesOnly(land, main[j], occupied))
                            continue;

                        occupied.Add(mid);
                        bool closesSquare = WouldFillSquare(land, occupied);
                        occupied.Remove(mid);
                        if (closesSquare)
                            continue;

                        occupied.Add(mid);
                        occupied.Add(land);
                        sideCells.Add(mid);
                        sideCells.Add(land);
                        return;
                    }
                }
            }
        }

        static void AddArrowTraps(List<Vector2Int> main, HashSet<Vector2Int> occupied, List<Vector2Int> traps, int rows, int cols)
        {
            int placed = 0;
            for (int pass = 0; pass < 2 && placed < 2; pass++)
            {
                for (int i = 0; i < main.Count - 1 && placed < 2; i++)
                {
                    if (TryAddTrap(main[i], occupied, traps, rows, cols, pass == 0))
                        placed++;
                }
            }
        }

        static bool TryAddTrap(Vector2Int pad, HashSet<Vector2Int> occupied, List<Vector2Int> traps, int rows, int cols, bool sameAxisOnly)
        {
            bool horizontal = false;
            bool vertical = false;
            for (int i = 0; i < Cardinal.Length; i++)
            {
                if (!occupied.Contains(pad + Cardinal[i]))
                    continue;
                if (Cardinal[i].x != 0)
                    horizontal = true;
                if (Cardinal[i].y != 0)
                    vertical = true;
            }

            if (sameAxisOnly && horizontal == vertical)
                return false;

            var options = new List<Vector2Int>();
            for (int i = 0; i < Cardinal.Length; i++)
            {
                Vector2Int step = Cardinal[i];
                if (sameAxisOnly && horizontal && step.x == 0)
                    continue;
                if (sameAxisOnly && vertical && step.y == 0)
                    continue;

                Vector2Int next = pad + step;
                if (next.x < 0 || next.y < 0 || next.x >= cols || next.y >= rows)
                    continue;
                if (occupied.Contains(next))
                    continue;
                if (!TouchesOnly(next, pad, occupied) || WouldFillSquare(next, occupied))
                    continue;
                options.Add(next);
            }

            if (options.Count == 0)
                return false;

            Vector2Int trap = options[Random.Range(0, options.Count)];
            occupied.Add(trap);
            traps.Add(trap);
            return true;
        }

        static LevelData Build(int levelId, List<Vector2Int> main, HashSet<Vector2Int> occupied, List<Vector2Int> entrances, List<Vector2Int> traps)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Vector2Int cell in occupied)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.y < minY) minY = cell.y;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.y > maxY) maxY = cell.y;
            }

            int rows = maxY - minY + 1;
            int cols = maxX - minX + 1;
            var data = new LevelData
            {
                levelId = levelId,
                rows = rows,
                cols = cols,
                gridMask = new int[rows, cols],
                lilyPadTypes = new LilyPadType[rows, cols],
                lilyPadDirections = new LilyPadDirection[rows, cols]
            };

            var shifted = new Dictionary<Vector2Int, Vector2Int>();
            foreach (Vector2Int cell in occupied)
            {
                Vector2Int local = new Vector2Int(cell.x - minX, cell.y - minY);
                shifted[cell] = local;
                data.gridMask[local.y, local.x] = 1;
            }

            Vector2Int start = shifted[main[0]];
            Vector2Int win = shifted[main[main.Count - 1]];
            data.lilyPadTypes[start.y, start.x] = LilyPadType.StartPad;
            data.lilyPadTypes[win.y, win.x] = LilyPadType.WinPad;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (data.gridMask[y, x] == 0)
                        continue;

                    data.lilyPadDirections[y, x] = DirectionFromNeighbors(data, new Vector2Int(x, y));
                }
            }

            for (int i = 0; i < entrances.Count; i++)
            {
                if (!shifted.ContainsKey(entrances[i]))
                    continue;

                Vector2Int pos = shifted[entrances[i]];
                if (data.lilyPadTypes[pos.y, pos.x] == LilyPadType.StartPad || data.lilyPadTypes[pos.y, pos.x] == LilyPadType.WinPad)
                    continue;

                data.lilyPadTypes[pos.y, pos.x] = LilyPadType.Gate;
            }

            if (main.Count >= 6)
            {
                int index = Random.Range(2, main.Count - 2);
                Vector2Int road = shifted[main[index]];
                if (data.lilyPadTypes[road.y, road.x] == LilyPadType.Normal)
                    data.lilyPadTypes[road.y, road.x] = LilyPadType.Breakable;
            }

            for (int i = 0; i < traps.Count; i++)
            {
                if (!shifted.ContainsKey(traps[i]))
                    continue;

                Vector2Int pos = shifted[traps[i]];
                data.lilyPadTypes[pos.y, pos.x] = LilyPadType.Trap;
                Vector2Int link = Vector2Int.zero;
                for (int c = 0; c < Cardinal.Length; c++)
                {
                    Vector2Int next = pos + Cardinal[c];
                    if (Inside(data, next) && data.gridMask[next.y, next.x] == 1)
                    {
                        link = Cardinal[c];
                        break;
                    }
                }

                data.lilyPadDirections[pos.y, pos.x] = link.x != 0
                    ? LilyPadDirection.Vertical
                    : LilyPadDirection.Horizontal;
            }

            return data;
        }

        static LevelData Fallback(int rows, int cols)
        {
            var main = new List<Vector2Int>
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1),
                new Vector2Int(3, 2)
            };
            var occupied = new HashSet<Vector2Int>(main);
            var side = new List<Vector2Int>();
            Vector2Int dead = new Vector2Int(1, 0);
            if (dead.y < rows && dead.x < cols)
            {
                occupied.Add(dead);
                side.Add(dead);
            }

            return Build(1, main, occupied, side, new List<Vector2Int>());
        }

        static LilyPadDirection DirectionFromNeighbors(LevelData data, Vector2Int pos)
        {
            bool horizontal = false;
            bool vertical = false;
            for (int i = 0; i < Cardinal.Length; i++)
            {
                Vector2Int next = pos + Cardinal[i];
                if (!Inside(data, next) || data.gridMask[next.y, next.x] == 0)
                    continue;

                if (Cardinal[i].x != 0)
                    horizontal = true;
                if (Cardinal[i].y != 0)
                    vertical = true;
            }

            if (horizontal && vertical)
                return LilyPadDirection.All;
            if (horizontal)
                return LilyPadDirection.Horizontal;
            return LilyPadDirection.Vertical;
        }

        public static bool CanReachWin(LevelData data)
        {
            Vector2Int start = Find(data, LilyPadType.StartPad);
            Vector2Int win = Find(data, LilyPadType.WinPad);
            if (start.x < 0 || win.x < 0)
                return false;

            var seen = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            seen.Add(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current == win)
                    return true;

                for (int i = 0; i < Cardinal.Length; i++)
                {
                    Vector2Int next = current + Cardinal[i];
                    if (!Inside(data, next) || data.gridMask[next.y, next.x] == 0 || seen.Contains(next))
                        continue;
                    if (!Allows(data.lilyPadDirections[current.y, current.x], Cardinal[i]))
                        continue;

                    seen.Add(next);
                    queue.Enqueue(next);
                }
            }

            return false;
        }

        static bool Allows(LilyPadDirection direction, Vector2Int step)
        {
            if (direction == LilyPadDirection.All)
                return true;
            if (direction == LilyPadDirection.Horizontal)
                return step.y == 0;
            return step.x == 0;
        }

        static int Span(List<Vector2Int> path)
        {
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i].y < minY) minY = path[i].y;
                if (path[i].y > maxY) maxY = path[i].y;
            }
            return maxY - minY;
        }

        static List<Vector2Int> OpenNeighbors(Vector2Int pos, int rows, int cols, HashSet<Vector2Int> occupied)
        {
            var list = new List<Vector2Int>();
            for (int i = 0; i < Cardinal.Length; i++)
            {
                Vector2Int next = pos + Cardinal[i];
                if (next.x < 0 || next.y < 0 || next.x >= cols || next.y >= rows)
                    continue;
                if (!occupied.Contains(next))
                    list.Add(next);
            }
            return list;
        }

        static List<Vector2Int> EmptyNeighbors(Vector2Int pos, HashSet<Vector2Int> occupied, int rows, int cols)
        {
            var list = new List<Vector2Int>();
            for (int i = 0; i < Cardinal.Length; i++)
            {
                Vector2Int next = pos + Cardinal[i];
                if (next.x < 0 || next.y < 0 || next.x >= cols || next.y >= rows)
                    continue;
                if (!occupied.Contains(next))
                    list.Add(next);
            }
            return list;
        }

        static bool TouchesOnly(Vector2Int cell, Vector2Int allowed, HashSet<Vector2Int> occupied)
        {
            for (int i = 0; i < Cardinal.Length; i++)
            {
                Vector2Int next = cell + Cardinal[i];
                if (next == allowed)
                    continue;
                if (occupied.Contains(next))
                    return false;
            }
            return true;
        }

        static bool WouldFillSquare(Vector2Int cell, HashSet<Vector2Int> occupied)
        {
            Vector2Int[] corners =
            {
                new Vector2Int(0, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, -1)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2Int origin = cell + corners[i];
                int filled = 0;
                for (int y = 0; y <= 1; y++)
                {
                    for (int x = 0; x <= 1; x++)
                    {
                        Vector2Int spot = origin + new Vector2Int(x, y);
                        if (spot == cell || occupied.Contains(spot))
                            filled++;
                    }
                }

                if (filled == 4)
                    return true;
            }

            return false;
        }

        static bool Adjacent(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }

        static int NeighborCount(LevelData data, Vector2Int pos)
        {
            int count = 0;
            for (int i = 0; i < Cardinal.Length; i++)
            {
                Vector2Int next = pos + Cardinal[i];
                if (Inside(data, next) && data.gridMask[next.y, next.x] == 1)
                    count++;
            }
            return count;
        }

        static bool Inside(LevelData data, Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < data.cols && pos.y < data.rows;
        }

        static Vector2Int Find(LevelData data, LilyPadType type)
        {
            for (int y = 0; y < data.rows; y++)
            {
                for (int x = 0; x < data.cols; x++)
                {
                    if (data.gridMask[y, x] == 1 && data.lilyPadTypes[y, x] == type)
                        return new Vector2Int(x, y);
                }
            }
            return new Vector2Int(-1, -1);
        }
    }
}
