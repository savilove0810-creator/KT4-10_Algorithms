using System.Text;

namespace KT8_Algorithms
{
    internal class Program
    {
        // Задача 1
        // Нужно найти минимальную стоимость пути робота, который прыгает на 1 или 2 платформы.
        // Выбрал динамическое программирование: стоимость прихода на платформу i
        // зависит только от двух предыдущих платформ,
        // dp[i] = min(dp[i - 1] + |h[i] - h[i - 1]|, dp[i - 2] + |h[i] - h[i - 2]|).
        public static long MinJumpCost(int[] h)
        {
            int n = h.Length;
            long[] dp = new long[n];
            dp[1] = Math.Abs((long)h[1] - h[0]);

            for (int i = 2; i < n; i++)
            {
                long oneStep = dp[i - 1] + Math.Abs((long)h[i] - h[i - 1]);
                long twoSteps = dp[i - 2] + Math.Abs((long)h[i] - h[i - 2]);

                dp[i] = Math.Min(oneStep, twoSteps);
            }

            return dp[n - 1];
        }


        // Задача 2
        // Нужно выбрать предметы (каждый не более одного раза) с суммарной массой до W
        // и максимальной полезностью.
        // Выбрал рюкзак 0/1 с одномерным массивом: dp[c] - лучшая полезность при массе не
        // больше c. Массу перебираем от большей к меньшей, чтобы один предмет
        // не использовался дважды. Время O(n * W), память O(W).
        public static long Knapsack(int[] weights, int[] values, int capacity)
        {
            long[] dp = new long[capacity + 1];

            for (int i = 0; i < weights.Length; i++)
            {
                for (int c = capacity; c >= weights[i]; c--)
                    dp[c] = Math.Max(dp[c], dp[c - weights[i]] + values[i]);
            }

            return dp[capacity];
        }


        // Задача 3
        // Нужно найти наибольшую общую подпоследовательность двух строк (длину).
        // Выбрал динамическое программирование по префиксам: dp[i][j] - ответ для первых i
        // символов s и первых j символов t. Для строки таблицы нужна только предыдущая,
        // поэтому храним две строки и память получается O(длина t).
        public static int LongestCommonSubsequence(string s, string t)
        {
            int[] previous = new int[t.Length + 1];
            int[] current = new int[t.Length + 1];

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    if (s[i - 1] == t[j - 1])
                        current[j] = previous[j - 1] + 1;
                    else
                        current[j] = Math.Max(previous[j], current[j - 1]);
                }

                (previous, current) = (current, previous);
            }

            return previous[t.Length];
        }


        // Задача 4
        // Нужно набрать массу ровно C неограниченным числом грузов: максимум прибыли,
        // а при равной прибыли - минимум грузов.
        // Выбрал рюкзак с неограниченным числом предметов: dp[c] - пара (прибыль, число
        // грузов) для массы ровно c. Пары сравниваются сначала по прибыли, потом по
        // количеству, и такое сравнение согласовано со сложением, поэтому динамика верна.
        // Недостижимую массу помечаем прибылью -1.
        public static (long profit, int count)? UnboundedKnapsack(int[] weights, int[] profits, int capacity)
        {
            long[] bestProfit = new long[capacity + 1];
            int[] bestCount = new int[capacity + 1];
            Array.Fill(bestProfit, -1);
            bestProfit[0] = 0;

            for (int c = 1; c <= capacity; c++)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i] > c || bestProfit[c - weights[i]] < 0)
                        continue;

                    long profit = bestProfit[c - weights[i]] + profits[i];
                    int count = bestCount[c - weights[i]] + 1;

                    if (profit > bestProfit[c] || (profit == bestProfit[c] && count < bestCount[c]))
                    {
                        bestProfit[c] = profit;
                        bestCount[c] = count;
                    }
                }
            }

            if (bestProfit[capacity] < 0)
                return null;

            return (bestProfit[capacity], bestCount[capacity]);
        }


        // Задача 5
        // Нужно найти длину наибольшей возрастающей подпоследовательности и число таких.
        // Для каждого элемента ответ - лучшая пара (длина, количество) среди элементов
        // слева с меньшим значением, плюс сам элемент. Чтобы искать её быстро,
        // выбрал дерево Фенвика по сжатым значениям: в нём храним пары и объединяем
        // их так: длиннее - лучше, при равной длине количества складываются.
        // Время O(n log n) вместо O(n^2).
        public static (int length, long count) CountLongestIncreasing(int[] a)
        {
            const long Mod = 1_000_000_007;
            int n = a.Length;

            int[] sorted = (int[])a.Clone();
            Array.Sort(sorted);
            int distinct = 0;

            for (int i = 0; i < n; i++)
            {
                if (i == 0 || sorted[i] != sorted[i - 1])
                    sorted[distinct++] = sorted[i];
            }

            int[] treeLength = new int[distinct + 1];
            long[] treeCount = new long[distinct + 1];

            // Объединяет пару (length, count) с узлом дерева.
            void Merge(int node, int length, long count)
            {
                if (length > treeLength[node])
                {
                    treeLength[node] = length;
                    treeCount[node] = count;
                }
                else if (length == treeLength[node])
                {
                    treeCount[node] = (treeCount[node] + count) % Mod;
                }
            }

            (int length, long count) Query(int position)
            {
                int length = 0;
                long count = 0;

                for (int i = position; i > 0; i -= i & -i)
                {
                    if (treeLength[i] > length)
                    {
                        length = treeLength[i];
                        count = treeCount[i];
                    }
                    else if (treeLength[i] == length)
                    {
                        count = (count + treeCount[i]) % Mod;
                    }
                }

                return (length, count);
            }

            for (int i = 0; i < n; i++)
            {
                // Номер значения среди различных (с единицы).
                int rank = Array.BinarySearch(sorted, 0, distinct, a[i]) + 1;

                var (length, count) = Query(rank - 1);

                if (length == 0)
                    count = 1;

                length++;

                for (int j = rank; j <= distinct; j += j & -j)
                    Merge(j, length, count);
            }

            return Query(distinct);
        }


        // Задача 6
        // Нужно найти кратчайшую строку, в которой s и t - подпоследовательности,
        // а из таких - лексикографически наименьшую.
        // Выбрал динамическое программирование по суффиксам: длина ответа для суффиксов
        // s[i..] и t[j..] равна (n - i) + (m - j) - LCS(s[i..], t[j..]).
        // Дальше строим ответ по символу: из двух возможных ходов (взять s[i] или t[j])
        // берём те, что сохраняют минимальную длину, и выбираем меньший символ.
        // Если символы равны, продвигаемся сразу в обеих строках.
        public static string ShortestCommonSupersequence(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] lcs = new int[n + 1, m + 1];

            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = m - 1; j >= 0; j--)
                {
                    if (s[i] == t[j])
                        lcs[i, j] = lcs[i + 1, j + 1] + 1;
                    else
                        lcs[i, j] = Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
                }
            }

            int Length(int i, int j)
            {
                return (n - i) + (m - j) - lcs[i, j];
            }

            StringBuilder result = new StringBuilder();
            int a = 0;
            int b = 0;

            while (a < n || b < m)
            {
                char bestChar = '\0';
                int nextA = a;
                int nextB = b;

                // Ход 1: выписываем символ s[a].
                if (a < n)
                {
                    char c = s[a];
                    int ni = a + 1;
                    int nj = (b < m && t[b] == c) ? b + 1 : b;

                    if (1 + Length(ni, nj) == Length(a, b))
                    {
                        bestChar = c;
                        nextA = ni;
                        nextB = nj;
                    }
                }

                // Ход 2: выписываем символ t[b].
                if (b < m)
                {
                    char c = t[b];
                    int ni = (a < n && s[a] == c) ? a + 1 : a;
                    int nj = b + 1;

                    if (1 + Length(ni, nj) == Length(a, b) && (bestChar == '\0' || c < bestChar))
                    {
                        bestChar = c;
                        nextA = ni;
                        nextB = nj;
                    }
                }

                result.Append(bestChar);
                a = nextA;
                b = nextB;
            }

            return result.ToString();
        }


        static void Main(string[] args)
        {
            // Задача 1
            int[] heights = { 10, 30, 40, 20 };
            Console.WriteLine(MinJumpCost(heights));


            // Задача 2
            int[] weights = { 1, 3, 4, 5 };
            int[] values = { 1, 4, 5, 7 };
            Console.WriteLine(Knapsack(weights, values, 7));


            // Задача 3
            Console.WriteLine(LongestCommonSubsequence("abcde", "ace"));


            // Задача 4
            int[] cargoWeights = { 2, 3 };
            int[] cargoProfits = { 5, 7 };
            var cargo = UnboundedKnapsack(cargoWeights, cargoProfits, 7);
            Console.WriteLine(cargo == null ? "-1" : $"{cargo.Value.profit} {cargo.Value.count}");


            // Задача 5
            int[] nums = { 1, 3, 5, 4, 7 };
            var lis = CountLongestIncreasing(nums);
            Console.WriteLine($"{lis.length} {lis.count}");


            // Задача 6
            Console.WriteLine(ShortestCommonSupersequence("abac", "cab"));
        }
    }
}
