# KT7_Algorithms

```csharp
namespace KT7_Algorithms
{
    internal class Program
    {
        // Значение "расстояние не определено / вершина недостижима".
        private const long Inf = long.MaxValue;

        // Строит списки смежности из рёбер вида { u, v, w }, вершины нумеруются с 1.
        private static List<(int to, long weight)>[] BuildGraph(int n, int[][] edges, bool undirected)
        {
            List<(int to, long weight)>[] graph = new List<(int to, long weight)>[n + 1];

            for (int i = 0; i <= n; i++)
                graph[i] = new List<(int to, long weight)>();

            foreach (int[] edge in edges)
            {
                graph[edge[0]].Add((edge[1], edge[2]));

                if (undirected)
                    graph[edge[1]].Add((edge[0], edge[2]));
            }

            return graph;
        }

        private static string Format(long distance)
        {
            return distance == Inf ? "INF" : distance.ToString();
        }


        // Задача 1
        // Нужно найти кратчайшие пути от одной вершины при неотрицательных весах.
        // Выбрал алгоритм Дейкстры с кучей: веса неотрицательные, поэтому вершина,
        // извлечённая из кучи с минимальным расстоянием, уже получила окончательный ответ.
        // Устаревшие записи в куче пропускаем. Время O((n + m) log n).
        public static long[] Dijkstra(int n, int[][] roads, int start)
        {
            List<(int to, long weight)>[] graph = BuildGraph(n, roads, false);
            long[] dist = new long[n + 1];
            Array.Fill(dist, Inf);
            dist[start] = 0;

            PriorityQueue<int, long> queue = new PriorityQueue<int, long>();
            queue.Enqueue(start, 0);

            while (queue.TryDequeue(out int node, out long distance))
            {
                if (distance > dist[node])
                    continue;

                foreach (var (next, weight) in graph[node])
                {
                    if (distance + weight < dist[next])
                    {
                        dist[next] = distance + weight;
                        queue.Enqueue(next, dist[next]);
                    }
                }
            }

            return dist;
        }


        // Задача 2
        // Нужно найти кратчайшие пути, когда есть отрицательные веса (но нет достижимых
        // отрицательных циклов). Дейкстра здесь неприменима, поэтому выбрал алгоритм
        // Беллмана-Форда: кратчайший путь содержит не больше n - 1 ребра, значит,
        // хватит n - 1 проходов по всем рёбрам. Если за проход ничего не изменилось,
        // останавливаемся раньше. Время O(n * m).
        public static long[] BellmanFord(int n, int[][] edges, int start)
        {
            long[] dist = new long[n + 1];
            Array.Fill(dist, Inf);
            dist[start] = 0;

            for (int round = 1; round < n; round++)
            {
                bool changed = false;

                foreach (int[] edge in edges)
                {
                    if (dist[edge[0]] != Inf && dist[edge[0]] + edge[2] < dist[edge[1]])
                    {
                        dist[edge[1]] = dist[edge[0]] + edge[2];
                        changed = true;
                    }
                }

                if (!changed)
                    break;
            }

            return dist;
        }


        // Задача 3
        // Нужно для каждой вершины найти ближайший пункт помощи (при равенстве - с
        // меньшим номером). Выбрал мультистартовый Дейкстру: все пункты кладём в кучу
        // с расстоянием 0 и запускаем один обход. Вместе с расстоянием храним номер
        // пункта, и в куче сравниваем пары (расстояние, пункт), поэтому при равных
        // расстояниях побеждает меньший пункт.
        public static (int center, long distance)[] NearestCenters(int n, int[][] roads, int[] centers)
        {
            List<(int to, long weight)>[] graph = BuildGraph(n, roads, true);
            long[] dist = new long[n + 1];
            int[] owner = new int[n + 1];
            Array.Fill(dist, Inf);

            PriorityQueue<int, (long, int)> queue = new PriorityQueue<int, (long, int)>();

            foreach (int center in centers)
            {
                dist[center] = 0;
                owner[center] = center;
                queue.Enqueue(center, (0, center));
            }

            while (queue.TryDequeue(out int node, out (long distance, int center) state))
            {
                if (state.distance != dist[node] || state.center != owner[node])
                    continue;

                foreach (var (next, weight) in graph[node])
                {
                    long newDistance = state.distance + weight;

                    if (newDistance < dist[next] || (newDistance == dist[next] && state.center < owner[next]))
                    {
                        dist[next] = newDistance;
                        owner[next] = state.center;
                        queue.Enqueue(next, (newDistance, state.center));
                    }
                }
            }

            (int center, long distance)[] result = new (int center, long distance)[n + 1];

            for (int i = 1; i <= n; i++)
                result[i] = dist[i] == Inf ? (-1, -1) : (owner[i], dist[i]);

            return result;
        }


        // Задача 4
        // Нужно найти кратчайший путь, где один раз можно проехать по дороге за w / 2.
        // Выбрал Дейкстру по слоям: состояние - пара (вершина, использован ли купон).
        // Из слоя 0 можно перейти в тот же слой за w или в слой 1 за w / 2,
        // из слоя 1 - только в слой 1 за w. Ответ - минимум по двум слоям вершины t.
        public static long CheapestWithCoupon(int n, int[][] roads, int start, int target)
        {
            List<(int to, long weight)>[] graph = BuildGraph(n, roads, false);
            long[] dist = new long[2 * (n + 1)];
            Array.Fill(dist, Inf);
            dist[2 * start] = 0;

            PriorityQueue<int, long> queue = new PriorityQueue<int, long>();
            queue.Enqueue(2 * start, 0);

            while (queue.TryDequeue(out int state, out long distance))
            {
                if (distance > dist[state])
                    continue;

                int node = state / 2;
                int used = state % 2;

                foreach (var (next, weight) in graph[node])
                {
                    long keep = distance + weight;

                    if (keep < dist[2 * next + used])
                    {
                        dist[2 * next + used] = keep;
                        queue.Enqueue(2 * next + used, keep);
                    }

                    if (used == 0)
                    {
                        long discount = distance + weight / 2;

                        if (discount < dist[2 * next + 1])
                        {
                            dist[2 * next + 1] = discount;
                            queue.Enqueue(2 * next + 1, discount);
                        }
                    }
                }
            }

            long best = Math.Min(dist[2 * target], dist[2 * target + 1]);
            return best == Inf ? -1 : best;
        }


        // Задача 5
        // Нужно отличить недостижимые вершины, вершины с конечным ответом и вершины,
        // до которых стоимость можно уменьшать бесконечно.
        // Выбрал Беллмана-Форда: после n - 1 проходов все конечные ответы окончательные.
        // Если ребро ещё улучшается, его конец лежит на достижимом отрицательном цикле
        // или за ним. Из таких вершин обходом в ширину помечаем всё достижимое:
        // для этих вершин стоимость не ограничена снизу.
        public static string[] ShortestPathsWithNegativeCycles(int n, int[][] edges, int start)
        {
            long[] dist = new long[n + 1];
            Array.Fill(dist, Inf);
            dist[start] = 0;

            for (int round = 1; round < n; round++)
            {
                bool changed = false;

                foreach (int[] edge in edges)
                {
                    if (dist[edge[0]] != Inf && dist[edge[0]] + edge[2] < dist[edge[1]])
                    {
                        dist[edge[1]] = dist[edge[0]] + edge[2];
                        changed = true;
                    }
                }

                if (!changed)
                    break;
            }

            List<int>[] graph = new List<int>[n + 1];

            for (int i = 0; i <= n; i++)
                graph[i] = new List<int>();

            foreach (int[] edge in edges)
                graph[edge[0]].Add(edge[1]);

            bool[] unbounded = new bool[n + 1];
            Queue<int> queue = new Queue<int>();

            foreach (int[] edge in edges)
            {
                if (dist[edge[0]] != Inf && dist[edge[0]] + edge[2] < dist[edge[1]] && !unbounded[edge[1]])
                {
                    unbounded[edge[1]] = true;
                    queue.Enqueue(edge[1]);
                }
            }

            while (queue.Count > 0)
            {
                int node = queue.Dequeue();

                foreach (int next in graph[node])
                {
                    if (!unbounded[next])
                    {
                        unbounded[next] = true;
                        queue.Enqueue(next);
                    }
                }
            }

            string[] result = new string[n];

            for (int i = 1; i <= n; i++)
            {
                if (dist[i] == Inf)
                    result[i - 1] = "*";
                else if (unbounded[i])
                    result[i - 1] = "-";
                else
                    result[i - 1] = dist[i].ToString();
            }

            return result;
        }


        // Задача 6
        // Нужно найти минимальную стоимость маршрута не более чем из K рёбер и
        // количество таких маршрутов.
        // Выбрал динамику по числу рёбер (как Беллман-Форд, но со счётчиком путей):
        // для каждой вершины на шаге i храним минимальную стоимость и число способов
        // достичь её ровно за i рёбер. Маршруты разной длины - разные маршруты,
        // поэтому для вершины t складываем количества по всем шагам с лучшей стоимостью.
        public static (long cost, long count)? CheapestRoutes(int n, int[][] edges, int start, int target, int maxEdges)
        {
            const long Mod = 1_000_000_007;

            long[] cost = new long[n + 1];
            long[] count = new long[n + 1];
            Array.Fill(cost, Inf);
            cost[start] = 0;
            count[start] = 1;

            long bestCost = Inf;
            long bestCount = 0;

            for (int step = 0; ; step++)
            {
                if (cost[target] < bestCost)
                {
                    bestCost = cost[target];
                    bestCount = count[target];
                }
                else if (cost[target] == bestCost && bestCost != Inf)
                {
                    bestCount = (bestCount + count[target]) % Mod;
                }

                if (step == maxEdges)
                    break;

                long[] nextCost = new long[n + 1];
                long[] nextCount = new long[n + 1];
                Array.Fill(nextCost, Inf);

                foreach (int[] edge in edges)
                {
                    if (cost[edge[0]] == Inf)
                        continue;

                    long newCost = cost[edge[0]] + edge[2];

                    if (newCost < nextCost[edge[1]])
                    {
                        nextCost[edge[1]] = newCost;
                        nextCount[edge[1]] = count[edge[0]];
                    }
                    else if (newCost == nextCost[edge[1]])
                    {
                        nextCount[edge[1]] = (nextCount[edge[1]] + count[edge[0]]) % Mod;
                    }
                }

                cost = nextCost;
                count = nextCount;
            }

            if (bestCost == Inf)
                return null;

            return (bestCost, bestCount);
        }


        static void Main(string[] args)
        {
            int[][] roads =
            {
                new int[] { 1, 2, 7 },
                new int[] { 1, 3, 2 },
                new int[] { 3, 2, 3 },
                new int[] { 2, 4, 1 }
            };


            // Задача 1
            Console.WriteLine(string.Join(" ", Array.ConvertAll(Dijkstra(5, roads, 1)[1..], Format)));


            // Задача 2
            int[][] negativeEdges =
            {
                new int[] { 1, 2, 4 },
                new int[] { 1, 3, 5 },
                new int[] { 3, 2, -3 },
                new int[] { 2, 4, 2 }
            };
            Console.WriteLine(string.Join(" ", Array.ConvertAll(BellmanFord(5, negativeEdges, 1)[1..], Format)));


            // Задача 3
            int[][] undirected =
            {
                new int[] { 1, 2, 2 },
                new int[] { 2, 3, 2 },
                new int[] { 3, 4, 2 },
                new int[] { 4, 5, 2 }
            };
            var nearest = NearestCenters(6, undirected, new int[] { 1, 5 });

            for (int i = 1; i <= 6; i++)
                Console.WriteLine($"{nearest[i].center} {nearest[i].distance}");


            // Задача 4
            int[][] tolls =
            {
                new int[] { 1, 2, 10 },
                new int[] { 2, 3, 10 },
                new int[] { 1, 3, 25 }
            };
            Console.WriteLine(CheapestWithCoupon(3, tolls, 1, 3));


            // Задача 5
            int[][] cycleEdges =
            {
                new int[] { 1, 2, 1 },
                new int[] { 2, 3, -5 },
                new int[] { 3, 2, 2 },
                new int[] { 3, 4, 1 },
                new int[] { 1, 5, 7 }
            };
            Console.WriteLine(string.Join(" ", ShortestPathsWithNegativeCycles(6, cycleEdges, 1)));


            // Задача 6
            int[][] routeEdges =
            {
                new int[] { 1, 2, 1 },
                new int[] { 1, 2, 1 },
                new int[] { 2, 3, 1 },
                new int[] { 1, 3, 2 }
            };
            var routes = CheapestRoutes(3, routeEdges, 1, 3, 2);
            Console.WriteLine(routes == null ? "IMPOSSIBLE" : $"{routes.Value.cost} {routes.Value.count}");
        }
    }
}
```
