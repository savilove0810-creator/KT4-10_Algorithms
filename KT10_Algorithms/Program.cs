namespace KT10_Algorithms
{
    internal class Program
    {
        // Задача 1
        // Нужно выбрать максимум непересекающихся заявок на промежутки [l, r).
        // Выбрал жадный алгоритм: сортируем заявки по времени окончания и берём каждую
        // заявку, которая начинается не раньше окончания последней выбранной.
        // Заявка, закончившаяся раньше всех, оставляет больше всего места остальным.
        public static int MaxNonOverlapping(int[][] requests)
        {
            int[][] sorted = (int[][])requests.Clone();
            Array.Sort(sorted, (a, b) => a[1].CompareTo(b[1]));

            int count = 0;
            long lastEnd = long.MinValue;

            foreach (int[] request in sorted)
            {
                if (request[0] >= lastEnd)
                {
                    count++;
                    lastEnd = request[1];
                }
            }

            return count;
        }


        // Задача 2
        // Нужно получать минимум и максимум в стеке за O(1) в худшем случае.
        // Выбрал стек, в котором вместе со значением хранится минимум и максимум
        // для всех элементов от дна до этой позиции. Тогда MIN и MAX - это
        // просто чтение верхнего элемента, а POP возвращает прежние значения сам.
        private class MinMaxStack
        {
            private readonly List<(int value, int min, int max)> items = new List<(int value, int min, int max)>();

            public void Push(int value)
            {
                if (items.Count == 0)
                    items.Add((value, value, value));
                else
                {
                    var top = items[items.Count - 1];
                    items.Add((value, Math.Min(value, top.min), Math.Max(value, top.max)));
                }
            }

            public void Pop()
            {
                items.RemoveAt(items.Count - 1);
            }

            public int Min()
            {
                return items[items.Count - 1].min;
            }

            public int Max()
            {
                return items[items.Count - 1].max;
            }
        }

        public static List<int> ProcessJournal(string[] commands)
        {
            MinMaxStack stack = new MinMaxStack();
            List<int> answers = new List<int>();

            foreach (string command in commands)
            {
                string[] parts = command.Split(' ');

                if (parts[0] == "PUSH")
                    stack.Push(int.Parse(parts[1]));
                else if (parts[0] == "POP")
                    stack.Pop();
                else if (parts[0] == "MIN")
                    answers.Add(stack.Min());
                else
                    answers.Add(stack.Max());
            }

            return answers;
        }


        // Задача 3
        // Нужно найти k-й по величине элемент массива за среднее O(n) и O(1) памяти.
        // Выбрал быстрый выбор (quickselect): как в быстрой сортировке разбиваем часть
        // массива вокруг случайного опорного элемента, но продолжаем только в той части,
        // где лежит позиция k. Разбиение на три части (меньше / равно / больше)
        // защищает от медленной работы при многих одинаковых значениях.
        public static int KthSmallest(int[] nums, int k)
        {
            Random random = new Random();
            int left = 0;
            int right = nums.Length - 1;
            int target = k - 1;

            while (true)
            {
                if (left == right)
                    return nums[left];

                int pivot = nums[random.Next(left, right + 1)];
                int less = left;
                int i = left;
                int greater = right;

                while (i <= greater)
                {
                    if (nums[i] < pivot)
                    {
                        (nums[less], nums[i]) = (nums[i], nums[less]);
                        less++;
                        i++;
                    }
                    else if (nums[i] > pivot)
                    {
                        (nums[i], nums[greater]) = (nums[greater], nums[i]);
                        greater--;
                    }
                    else
                        i++;
                }

                // Теперь [left, less) < pivot, [less, greater] == pivot, (greater, right] > pivot.
                if (target < less)
                    right = less - 1;
                else if (target > greater)
                    left = greater + 1;
                else
                    return pivot;
            }
        }


        // Задача 4
        // Нужно выбрать заказы с единичным временем и сроками так, чтобы прибыль была
        // максимальной.
        // Выбрал жадный алгоритм с минимальной кучей: заказы сортируем по сроку и
        // берём каждый в набор. Если выбранных заказов больше, чем слотов до его срока,
        // выбрасываем заказ с наименьшей прибылью. В куче всегда лучший допустимый набор.
        public static long MaxProfit(int[][] orders)
        {
            int[][] sorted = (int[][])orders.Clone();
            Array.Sort(sorted, (a, b) => a[0].CompareTo(b[0]));

            PriorityQueue<int, int> chosen = new PriorityQueue<int, int>();
            long total = 0;

            foreach (int[] order in sorted)
            {
                chosen.Enqueue(order[1], order[1]);
                total += order[1];

                if (chosen.Count > order[0])
                    total -= chosen.Dequeue();
            }

            return total;
        }


        // Задача 5
        // Нужен кэш с вытеснением наименее часто используемого ключа (при равенстве частот -
        // самого давно использованного), все операции за O(1).
        // Выбрал две хэш-таблицы и списки: для каждого ключа хранится его запись, а для каждой
        // частоты - двусвязный список ключей этой частоты в порядке использования
        // (в начале самый старый). Минимальную частоту minFrequency поддерживаем
        // отдельно, поэтому ключ для вытеснения - начало списка этой частоты.
        private class LfuCache
        {
            private class Entry
            {
                public int Key;
                public int Value;
                public int Frequency;
            }

            private readonly int capacity;
            private readonly Dictionary<int, LinkedListNode<Entry>> nodes = new Dictionary<int, LinkedListNode<Entry>>();
            private readonly Dictionary<int, LinkedList<Entry>> lists = new Dictionary<int, LinkedList<Entry>>();
            private int minFrequency;

            public LfuCache(int capacity)
            {
                this.capacity = capacity;
            }

            // Добавляет запись в конец списка её текущей частоты.
            private void Attach(Entry entry)
            {
                if (!lists.TryGetValue(entry.Frequency, out LinkedList<Entry>? list))
                {
                    list = new LinkedList<Entry>();
                    lists[entry.Frequency] = list;
                }

                nodes[entry.Key] = list.AddLast(entry);
            }

            // Увеличивает частоту записи на 1 и отмечает использование.
            private void Touch(Entry entry)
            {
                LinkedList<Entry> list = lists[entry.Frequency];
                list.Remove(nodes[entry.Key]);

                if (list.Count == 0)
                {
                    lists.Remove(entry.Frequency);

                    if (minFrequency == entry.Frequency)
                        minFrequency++;
                }

                entry.Frequency++;
                Attach(entry);
            }

            public int Get(int key)
            {
                if (!nodes.TryGetValue(key, out LinkedListNode<Entry>? node))
                    return -1;

                Entry entry = node.Value;
                Touch(entry);

                return entry.Value;
            }

            public void Put(int key, int value)
            {
                if (nodes.TryGetValue(key, out LinkedListNode<Entry>? node))
                {
                    node.Value.Value = value;
                    Touch(node.Value);
                    return;
                }

                if (nodes.Count == capacity)
                {
                    LinkedList<Entry> list = lists[minFrequency];
                    Entry evicted = list.First!.Value;
                    list.RemoveFirst();

                    if (list.Count == 0)
                        lists.Remove(minFrequency);

                    nodes.Remove(evicted.Key);
                }

                Entry entry = new Entry { Key = key, Value = value, Frequency = 1 };
                Attach(entry);
                minFrequency = 1;
            }
        }

        public static List<int> ProcessCache(int capacity, string[] commands)
        {
            LfuCache cache = new LfuCache(capacity);
            List<int> answers = new List<int>();

            foreach (string command in commands)
            {
                string[] parts = command.Split(' ');

                if (parts[0] == "GET")
                    answers.Add(cache.Get(int.Parse(parts[1])));
                else
                    cache.Put(int.Parse(parts[1]), int.Parse(parts[2]));
            }

            return answers;
        }


        // Задача 6
        // Нужно вывести k ближайших к началу координат пунктов в заданном порядке сравнения.
        // Выбрал топ k элементов через максимальную кучу размера k: в ней лежат k лучших
        // пунктов, а худший из них наверху. Новый пункт вытесняет худший, если он лучше.
        // Сравнение - кортеж (x^2 + y^2, x, y, номер). Время O(n log k), память O(k).
        // В конце k пунктов из кучи выводим по порядку.
        public static List<(int x, int y, int index)> NearestPoints(int[][] points, int k)
        {
            Comparer<(long, int, int, int)> worstFirst =
                Comparer<(long, int, int, int)>.Create((a, b) => b.CompareTo(a));

            PriorityQueue<int, (long, int, int, int)> heap =
                new PriorityQueue<int, (long, int, int, int)>(worstFirst);

            for (int i = 0; i < points.Length; i++)
            {
                long x = points[i][0];
                long y = points[i][1];

                heap.Enqueue(i, (x * x + y * y, points[i][0], points[i][1], i + 1));

                if (heap.Count > k)
                    heap.Dequeue();
            }

            (int x, int y, int index)[] result = new (int x, int y, int index)[k];

            for (int i = k - 1; i >= 0; i--)
            {
                int point = heap.Dequeue();
                result[i] = (points[point][0], points[point][1], point + 1);
            }

            return new List<(int x, int y, int index)>(result);
        }


        static void Main(string[] args)
        {
            // Задача 1
            int[][] requests =
            {
                new int[] { 1, 3 },
                new int[] { 2, 4 },
                new int[] { 3, 5 },
                new int[] { 0, 2 }
            };
            Console.WriteLine(MaxNonOverlapping(requests));


            // Задача 2
            string[] journal = { "PUSH 3", "PUSH 1", "PUSH 5", "MIN", "MAX", "POP", "POP", "MAX", "MIN" };
            Console.WriteLine(string.Join(" ", ProcessJournal(journal)));


            // Задача 3
            int[] nums = { 7, 2, 9, 4, 4, 1 };
            Console.WriteLine(KthSmallest(nums, 3));


            // Задача 4
            int[][] orders =
            {
                new int[] { 2, 100 },
                new int[] { 1, 19 },
                new int[] { 2, 27 },
                new int[] { 1, 25 },
                new int[] { 3, 15 }
            };
            Console.WriteLine(MaxProfit(orders));


            // Задача 5
            string[] commands =
            {
                "PUT 1 1", "PUT 2 2", "GET 1", "PUT 3 3", "GET 2", "GET 3", "PUT 4 4", "GET 1", "GET 3", "GET 4"
            };
            Console.WriteLine(string.Join(" ", ProcessCache(2, commands)));


            // Задача 6
            int[][] points =
            {
                new int[] { 3, 3 },
                new int[] { 5, -1 },
                new int[] { -2, 4 },
                new int[] { 1, 1 },
                new int[] { -1, 1 }
            };

            foreach (var point in NearestPoints(points, 3))
                Console.WriteLine($"{point.x} {point.y} {point.index}");
        }
    }
}
