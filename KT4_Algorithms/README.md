# KT4_Algorithms

```csharp
using System.Globalization;

namespace KT4_Algorithms
{
    internal class Program
    {
        // Задача 1
        // Нужно найти сумму значений вершин на каждом уровне дерева.
        // Выбрал обход в ширину (BFS): он идёт строго по уровням,
        // поэтому сумму каждого уровня считаем за один проход по очереди.
        // Дерево задано массивами: values[i], left[i], right[i], корень - вершина 0.
        public static long[] LevelSums(int[] values, int[] left, int[] right)
        {
            List<long> result = new List<long>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(0);

            while (queue.Count > 0)
            {
                int levelSize = queue.Count;
                long sum = 0;

                for (int i = 0; i < levelSize; i++)
                {
                    int node = queue.Dequeue();
                    sum += values[node];

                    if (left[node] != -1)
                        queue.Enqueue(left[node]);

                    if (right[node] != -1)
                        queue.Enqueue(right[node]);
                }

                result.Add(sum);
            }

            return result.ToArray();
        }


        // Задача 2
        // Нужно найти k самых частых чисел: сначала по убыванию частоты,
        // при равной частоте - сначала меньшее число.
        // Выбрал топ k элементов через кучу размера k: считаем частоты в словаре,
        // а в минимальной куче держим k лучших, худший из них всегда наверху.
        // Это O(n log k) вместо полной сортировки всех различных чисел.
        public static int[] TopKFrequent(int[] nums, int k)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();

            for (int i = 0; i < nums.Length; i++)
            {
                if (counts.ContainsKey(nums[i]))
                    counts[nums[i]]++;
                else
                    counts[nums[i]] = 1;
            }

            // Приоритет (частота, -значение): чем он меньше, тем элемент хуже.
            PriorityQueue<int, (int, long)> heap = new PriorityQueue<int, (int, long)>();

            foreach (var pair in counts)
            {
                heap.Enqueue(pair.Key, (pair.Value, -(long)pair.Key));

                if (heap.Count > k)
                    heap.Dequeue();
            }

            int[] result = new int[k];

            for (int i = k - 1; i >= 0; i--)
                result[i] = heap.Dequeue();

            return result;
        }


        // Задача 3
        // После каждого нового числа нужно знать медиану всех чисел.
        // Выбрал две кучи (two heaps): в максимальной куче лежит нижняя половина чисел,
        // в минимальной - верхняя. Медиана всегда находится на вершинах куч,
        // а добавление нового числа стоит O(log n).
        public static string[] RunningMedians(int[] nums)
        {
            PriorityQueue<long, long> low = new PriorityQueue<long, long>();
            PriorityQueue<long, long> high = new PriorityQueue<long, long>();
            string[] result = new string[nums.Length];

            for (int i = 0; i < nums.Length; i++)
            {
                long x = nums[i];

                if (low.Count == 0 || x <= low.Peek())
                    low.Enqueue(x, -x);
                else
                    high.Enqueue(x, x);

                if (low.Count > high.Count + 1)
                {
                    long moved = low.Dequeue();
                    high.Enqueue(moved, moved);
                }
                else if (high.Count > low.Count)
                {
                    long moved = high.Dequeue();
                    low.Enqueue(moved, -moved);
                }

                double median;

                if (low.Count > high.Count)
                    median = low.Peek();
                else
                    median = (low.Peek() + high.Peek()) / 2.0;

                result[i] = median.ToString("F1", CultureInfo.InvariantCulture);
            }

            return result;
        }


        // Задача 4
        // Нужно найти маршрут с максимальной суммой, который может начинаться и
        // заканчиваться в любых вершинах.
        // Выбрал обход дерева (от листьев к корню): для каждой вершины считаем лучший
        // путь вниз и лучший путь, который "изгибается" в этой вершине.
        // Обход сделан без рекурсии, потому что дерево из 200 000 вершин может быть
        // глубоким и вызвать переполнение стека.
        public static long MaxPathSum(int[] values, int[] left, int[] right)
        {
            int n = values.Length;

            // Порядок обхода в ширину: родитель всегда раньше своих детей.
            int[] order = new int[n];
            int head = 0;
            int tail = 0;
            order[tail++] = 0;

            while (head < tail)
            {
                int node = order[head++];

                if (left[node] != -1)
                    order[tail++] = left[node];

                if (right[node] != -1)
                    order[tail++] = right[node];
            }

            long[] down = new long[n];
            long best = long.MinValue;

            // Идём в обратном порядке, поэтому дети обрабатываются раньше родителя.
            for (int i = n - 1; i >= 0; i--)
            {
                int node = order[i];

                long leftGain = left[node] == -1 ? 0 : Math.Max(0, down[left[node]]);
                long rightGain = right[node] == -1 ? 0 : Math.Max(0, down[right[node]]);

                down[node] = values[node] + Math.Max(leftGain, rightGain);
                best = Math.Max(best, values[node] + leftGain + rightGain);
            }

            return best;
        }


        // Задача 5
        // Нужно найти k-ю наименьшую сумму A[i] + B[j] среди всех пар.
        // Выбрал топ k элементов через минимальную кучу: массивы отсортированы,
        // поэтому для каждого i суммы с B растут. Кладём в кучу пары (i, 0),
        // достаём наименьшую и добавляем её следующую пару (i, j + 1).
        // Всего k извлечений, поэтому все n * m пар перебирать не нужно.
        public static long KthSmallestPairSum(int[] a, int[] b, int k)
        {
            PriorityQueue<(int i, int j), long> heap = new PriorityQueue<(int i, int j), long>();
            int starts = Math.Min(a.Length, k);

            for (int i = 0; i < starts; i++)
                heap.Enqueue((i, 0), (long)a[i] + b[0]);

            long result = 0;

            for (int step = 0; step < k; step++)
            {
                heap.TryDequeue(out var pair, out result);

                if (pair.j + 1 < b.Length)
                    heap.Enqueue((pair.i, pair.j + 1), (long)a[pair.i] + b[pair.j + 1]);
            }

            return result;
        }


        // Задача 6
        // Нужно найти медиану в каждом окне длины k.
        // Выбрал две кучи (two heaps) с отложенным удалением: когда число уходит
        // из окна, мы не ищем его в куче, а только запоминаем в словаре delayed
        // и удаляем, когда оно окажется на вершине. Размеры половин считаем отдельно.
        public static string[] SlidingWindowMedians(int[] nums, int k)
        {
            PriorityQueue<long, long> low = new PriorityQueue<long, long>();
            PriorityQueue<long, long> high = new PriorityQueue<long, long>();
            Dictionary<long, int> delayed = new Dictionary<long, int>();

            int lowSize = 0;
            int highSize = 0;

            // Убирает с вершины кучи числа, которые уже вышли из окна.
            void Prune(PriorityQueue<long, long> heap)
            {
                while (heap.Count > 0
                    && delayed.TryGetValue(heap.Peek(), out int count)
                    && count > 0)
                {
                    delayed[heap.Peek()] = count - 1;
                    heap.Dequeue();
                }
            }

            // Держит размеры половин: в нижней столько же элементов или на один больше.
            void Balance()
            {
                while (lowSize > highSize + 1)
                {
                    long moved = low.Dequeue();
                    high.Enqueue(moved, moved);
                    lowSize--;
                    highSize++;
                    Prune(low);
                }

                while (lowSize < highSize)
                {
                    long moved = high.Dequeue();
                    low.Enqueue(moved, -moved);
                    highSize--;
                    lowSize++;
                    Prune(high);
                }
            }

            void Add(long x)
            {
                if (lowSize == 0 || x <= low.Peek())
                {
                    low.Enqueue(x, -x);
                    lowSize++;
                }
                else
                {
                    high.Enqueue(x, x);
                    highSize++;
                }

                Balance();
            }

            void Remove(long x)
            {
                if (delayed.ContainsKey(x))
                    delayed[x]++;
                else
                    delayed[x] = 1;

                if (x <= low.Peek())
                {
                    lowSize--;

                    if (x == low.Peek())
                        Prune(low);
                }
                else
                {
                    highSize--;

                    if (x == high.Peek())
                        Prune(high);
                }

                Balance();
            }

            string[] result = new string[nums.Length - k + 1];

            for (int i = 0; i < k; i++)
                Add(nums[i]);

            for (int i = k; ; i++)
            {
                double median;

                if (k % 2 == 1)
                    median = low.Peek();
                else
                    median = (low.Peek() + high.Peek()) / 2.0;

                result[i - k] = median.ToString("F1", CultureInfo.InvariantCulture);

                if (i == nums.Length)
                    break;

                Add(nums[i]);
                Remove(nums[i - k]);
            }

            return result;
        }


        static void Main(string[] args)
        {
            // Дерево для задач 1 и 4:
            //        1
            //      /   \
            //     2     3
            //    / \     \
            //   4  -5     6
            int[] values = { 1, 2, 3, 4, -5, 6 };
            int[] left = { 1, 3, -1, -1, -1, -1 };
            int[] right = { 2, 4, 5, -1, -1, -1 };


            // Задача 1
            Console.WriteLine(string.Join(" ", LevelSums(values, left, right)));


            // Задача 2
            int[] nums = { 1, 1, 1, 2, 2, 3, 3, 4 };
            Console.WriteLine(string.Join(" ", TopKFrequent(nums, 3)));


            // Задача 3
            int[] stream = { 5, 15, 1, 3 };
            Console.WriteLine(string.Join(" ", RunningMedians(stream)));


            // Задача 4
            Console.WriteLine(MaxPathSum(values, left, right));


            // Задача 5
            int[] a = { 1, 7, 11 };
            int[] b = { 2, 4, 6 };
            Console.WriteLine(KthSmallestPairSum(a, b, 4));


            // Задача 6
            int[] window = { 1, 3, -1, -3, 5, 3, 6, 7 };
            Console.WriteLine(string.Join(" ", SlidingWindowMedians(window, 3)));
        }
    }
}
```
