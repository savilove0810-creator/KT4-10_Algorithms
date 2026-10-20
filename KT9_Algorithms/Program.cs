namespace KT9_Algorithms
{
    internal class Program
    {
        // Значение "состояние недостижимо" для динамики на максимум.
        private const long Negative = long.MinValue / 2;

        // Строит списки смежности дерева по рёбрам { u, v }, вершины нумеруются с 1.
        private static List<int>[] BuildTree(int n, int[][] edges)
        {
            List<int>[] graph = new List<int>[n + 1];

            for (int i = 0; i <= n; i++)
                graph[i] = new List<int>();

            foreach (int[] edge in edges)
            {
                graph[edge[0]].Add(edge[1]);
                graph[edge[1]].Add(edge[0]);
            }

            return graph;
        }


        // Задача 1
        // Нужно разбить строку на минимальное число палиндромов.
        // Выбрал динамическое программирование в два этапа: сначала таблица
        // isPalindrome[i, j] (подстрока i..j - палиндром), затем cuts[i] - минимальное
        // число частей для префикса длины i. Для префикса перебираем последнюю часть
        // s[j..i-1], если она палиндром. Время и память O(n^2).
        public static int MinPalindromeParts(string s)
        {
            int n = s.Length;
            bool[,] isPalindrome = new bool[n, n];

            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = i; j < n; j++)
                    isPalindrome[i, j] = s[i] == s[j] && (j - i < 2 || isPalindrome[i + 1, j - 1]);
            }

            int[] parts = new int[n + 1];

            for (int i = 1; i <= n; i++)
            {
                parts[i] = int.MaxValue;

                for (int j = 0; j < i; j++)
                {
                    if (isPalindrome[j, i - 1] && parts[j] + 1 < parts[i])
                        parts[i] = parts[j] + 1;
                }
            }

            return parts[n];
        }


        // Задача 2
        // Нужно посчитать маршруты вправо и вниз по карте с препятствиями.
        // Выбрал динамическое программирование по клеткам: в клетку можно прийти сверху или
        // слева, поэтому paths[i][j] = paths[i-1][j] + paths[i][j-1], а в заблокированной
        // клетке 0. Достаточно одной строки таблицы, память O(m).
        public static int CountPaths(string[] map)
        {
            const int Mod = 1_000_000_007;
            int n = map.Length;
            int m = map[0].Length;
            int[] paths = new int[m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (map[i][j] == '#')
                    {
                        paths[j] = 0;
                        continue;
                    }

                    if (i == 0 && j == 0)
                        paths[j] = 1;
                    else
                    {
                        int fromTop = i > 0 ? paths[j] : 0;
                        int fromLeft = j > 0 ? paths[j - 1] : 0;

                        paths[j] = (fromTop + fromLeft) % Mod;
                    }
                }
            }

            return paths[m - 1];
        }


        // Задача 3
        // Нужно выбрать станции максимальной ценности так, чтобы соседние не выбирались.
        // Выбрал динамическое программирование на дереве: для каждой вершины считаем
        // take (вершина выбрана, дети - нет) и skip (вершина не выбрана, каждый ребёнок
        // выбирается по лучшему из двух вариантов). Обход без рекурсии: порядок вершин
        // получаем обходом в ширину и идём по нему с конца, поэтому дети готовы раньше родителя.
        public static long MaxIndependentSet(int[] values, int[][] edges)
        {
            int n = values.Length;
            List<int>[] graph = BuildTree(n, edges);

            int[] parent = new int[n + 1];
            int[] order = new int[n];
            int head = 0;
            int tail = 0;
            order[tail++] = 1;

            while (head < tail)
            {
                int node = order[head++];

                foreach (int next in graph[node])
                {
                    if (next != parent[node])
                    {
                        parent[next] = node;
                        order[tail++] = next;
                    }
                }
            }

            long[] take = new long[n + 1];
            long[] skip = new long[n + 1];

            for (int i = n - 1; i >= 0; i--)
            {
                int node = order[i];
                take[node] += values[node - 1];

                if (parent[node] != 0)
                {
                    take[parent[node]] += skip[node];
                    skip[parent[node]] += Math.Max(take[node], skip[node]);
                }
            }

            return Math.Max(take[1], skip[1]);
        }


        // Задача 4
        // Нужно объединить соседние группы с минимальной суммарной стоимостью.
        // Выбрал динамическое программирование по отрезкам: cost[i, j] - минимальная
        // стоимость слияния групп i..j в одну. Последнее слияние разбивает отрезок
        // на две части в точке k, и оно стоит сумму всего отрезка (считаем префиксными
        // суммами). Время O(n^3), при n <= 500 этого достаточно.
        public static long MinMergeCost(int[] sizes)
        {
            int n = sizes.Length;
            long[] prefix = new long[n + 1];

            for (int i = 0; i < n; i++)
                prefix[i + 1] = prefix[i] + sizes[i];

            long[,] cost = new long[n, n];

            for (int length = 2; length <= n; length++)
            {
                for (int i = 0; i + length - 1 < n; i++)
                {
                    int j = i + length - 1;
                    long best = long.MaxValue;

                    for (int k = i; k < j; k++)
                        best = Math.Min(best, cost[i, k] + cost[k + 1, j]);

                    cost[i, j] = best + prefix[j + 1] - prefix[i];
                }
            }

            return cost[0, n - 1];
        }


        // Задача 5
        // Нужно пройти по таблице вправо и вниз с максимальной суммой, обнулив
        // не более k отрицательных клеток.
        // Выбрал динамическое программирование с третьим измерением: best[i, j, c] -
        // лучшая сумма на пути в клетку (i, j), если использовано c экранов.
        // Клетку можно взять как есть или (если она отрицательная) обнулить за один экран.
        // Хранятся только текущая и предыдущая строки.
        public static long MaxPathWithScreens(int[][] table, int k)
        {
            int n = table.Length;
            int m = table[0].Length;
            int width = k + 1;

            long[] previous = new long[m * width];
            long[] current = new long[m * width];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    long value = table[i][j];

                    for (int c = 0; c <= k; c++)
                    {
                        // Лучшее состояние перед входом в клетку с c использованными экранами.
                        long before = Negative;
                        long beforeLess = Negative;

                        if (i == 0 && j == 0)
                        {
                            before = c == 0 ? 0 : Negative;
                            beforeLess = c == 1 ? 0 : Negative;
                        }
                        else
                        {
                            if (i > 0)
                            {
                                before = Math.Max(before, previous[j * width + c]);

                                if (c > 0)
                                    beforeLess = Math.Max(beforeLess, previous[j * width + c - 1]);
                            }

                            if (j > 0)
                            {
                                before = Math.Max(before, current[(j - 1) * width + c]);

                                if (c > 0)
                                    beforeLess = Math.Max(beforeLess, current[(j - 1) * width + c - 1]);
                            }
                        }

                        long best = before <= Negative ? Negative : before + value;

                        if (value < 0 && beforeLess > Negative)
                            best = Math.Max(best, beforeLess);

                        current[j * width + c] = best;
                    }
                }

                (previous, current) = (current, previous);
            }

            long result = Negative;

            for (int c = 0; c <= k; c++)
                result = Math.Max(result, previous[(m - 1) * width + c]);

            return result;
        }


        // Задача 6
        // Нужно выбрать связную часть дерева ровно из k вершин с максимальной суммой.
        // Выбрал динамическое программирование на дереве с рюкзаком по детям:
        // dp[v][s] - лучшая сумма связной части из s вершин, содержащей v и лежащей в
        // поддереве v. Детей добавляем по одному, как предметы в рюкзак, размер
        // перебирая от большего к меньшему. Ответ - максимум dp[v][k] по всем v.
        public static long MaxConnectedSubtree(int[] values, int[][] edges, int k)
        {
            int n = values.Length;
            List<int>[] graph = BuildTree(n, edges);
            long[][] dp = new long[n + 1][];
            int[] size = new int[n + 1];
            long result = Negative;

            // Глубина рекурсии не больше n <= 300.
            void Visit(int node, int parent)
            {
                dp[node] = new long[k + 1];
                Array.Fill(dp[node], Negative);
                dp[node][1] = values[node - 1];
                size[node] = 1;

                foreach (int child in graph[node])
                {
                    if (child == parent)
                        continue;

                    Visit(child, node);

                    for (int s = Math.Min(k, size[node] + size[child]); s >= 2; s--)
                    {
                        for (int t = 1; t <= Math.Min(size[child], s - 1); t++)
                        {
                            if (dp[node][s - t] > Negative && dp[child][t] > Negative)
                                dp[node][s] = Math.Max(dp[node][s], dp[node][s - t] + dp[child][t]);
                        }
                    }

                    size[node] += size[child];
                }

                result = Math.Max(result, dp[node][k]);
            }

            Visit(1, 0);

            return result;
        }


        static void Main(string[] args)
        {
            // Задача 1
            Console.WriteLine(MinPalindromeParts("aab"));


            // Задача 2
            string[] map =
            {
                "...",
                ".#.",
                "..."
            };
            Console.WriteLine(CountPaths(map));


            // Задача 3
            int[] values = { 3, 2, 1, 10 };
            int[][] edges =
            {
                new int[] { 1, 2 },
                new int[] { 2, 3 },
                new int[] { 3, 4 }
            };
            Console.WriteLine(MaxIndependentSet(values, edges));


            // Задача 4
            int[] sizes = { 4, 1, 2, 3 };
            Console.WriteLine(MinMergeCost(sizes));


            // Задача 5
            int[][] table =
            {
                new int[] { 1, -5, 2 },
                new int[] { -4, -3, 1 },
                new int[] { 2, -6, 3 }
            };
            Console.WriteLine(MaxPathWithScreens(table, 1));


            // Задача 6
            int[] values2 = { 1, -2, 3, 4, -1 };
            int[][] edges2 =
            {
                new int[] { 1, 2 },
                new int[] { 1, 3 },
                new int[] { 3, 4 },
                new int[] { 3, 5 }
            };
            Console.WriteLine(MaxConnectedSubtree(values2, edges2, 3));
        }
    }
}
