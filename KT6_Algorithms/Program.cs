using System.Text;

namespace KT6_Algorithms
{
    internal class Program
    {
        private class ListNode
        {
            public int Value;
            public ListNode? Next;

            public ListNode(int value)
            {
                Value = value;
            }
        }

        // Строит односвязный список из массива с сохранением порядка.
        private static ListNode? BuildList(int[] nums)
        {
            ListNode? head = null;
            ListNode? tail = null;

            foreach (int x in nums)
            {
                ListNode node = new ListNode(x);

                if (head == null)
                    head = node;
                else
                    tail!.Next = node;

                tail = node;
            }

            return head;
        }

        private static string ListToString(ListNode? head)
        {
            StringBuilder builder = new StringBuilder();

            for (ListNode? node = head; node != null; node = node.Next)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                builder.Append(node.Value);
            }

            return builder.ToString();
        }


        // Задача 1
        // Нужно развернуть односвязный список без новых узлов и без доп. коллекций.
        // Выбрал три ссылки (prev, current, next): идём по списку и в каждом узле
        // переключаем ссылку Next на предыдущий узел. Время O(n), память O(1).
        private static ListNode? ReverseList(ListNode? head)
        {
            ListNode? previous = null;
            ListNode? current = head;

            while (current != null)
            {
                ListNode? next = current.Next;
                current.Next = previous;
                previous = current;
                current = next;
            }

            return previous;
        }


        // Система непересекающихся множеств (DSU) для задачи 2:
        // сжатие путей и объединение по размеру делают операции почти за O(1).
        private class DisjointSet
        {
            private readonly int[] parent;
            private readonly int[] size;

            public DisjointSet(int n)
            {
                parent = new int[n + 1];
                size = new int[n + 1];

                for (int i = 0; i <= n; i++)
                {
                    parent[i] = i;
                    size[i] = 1;
                }
            }

            public int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }

                return x;
            }

            public void Union(int a, int b)
            {
                int rootA = Find(a);
                int rootB = Find(b);

                if (rootA == rootB)
                    return;

                if (size[rootA] < size[rootB])
                    (rootA, rootB) = (rootB, rootA);

                parent[rootB] = rootA;
                size[rootA] += size[rootB];
            }
        }

        // Задача 2
        // Нужно добавлять связи и проверять, связаны ли две станции.
        // Выбрал систему непересекающихся множеств (Union-Find): связи только
        // добавляются, а компоненты связности только сливаются. Станции связаны,
        // если у них один и тот же корень.
        public static List<string> ProcessConnections(int n, int[][] operations)
        {
            DisjointSet set = new DisjointSet(n);
            List<string> answers = new List<string>();

            foreach (int[] operation in operations)
            {
                if (operation[0] == 1)
                    set.Union(operation[1], operation[2]);
                else
                    answers.Add(set.Find(operation[1]) == set.Find(operation[2]) ? "YES" : "NO");
            }

            return answers;
        }


        // Задача 3
        // Нужно найти порядок этапов с учётом требований "a раньше b", а из всех
        // порядков - лексикографически наименьший.
        // Выбрал топологическую сортировку (алгоритм Кана) с минимальной кучей:
        // из всех этапов без невыполненных требований всегда берём с наименьшим номером.
        // Если взяли не все этапы, в требованиях есть цикл и ответа нет.
        public static int[]? SmallestTopologicalOrder(int n, int[][] requirements)
        {
            List<int>[] graph = new List<int>[n + 1];
            int[] inDegree = new int[n + 1];

            for (int i = 1; i <= n; i++)
                graph[i] = new List<int>();

            foreach (int[] requirement in requirements)
            {
                graph[requirement[0]].Add(requirement[1]);
                inDegree[requirement[1]]++;
            }

            PriorityQueue<int, int> ready = new PriorityQueue<int, int>();

            for (int i = 1; i <= n; i++)
            {
                if (inDegree[i] == 0)
                    ready.Enqueue(i, i);
            }

            int[] order = new int[n];
            int count = 0;

            while (ready.Count > 0)
            {
                int node = ready.Dequeue();
                order[count++] = node;

                foreach (int next in graph[node])
                {
                    inDegree[next]--;

                    if (inDegree[next] == 0)
                        ready.Enqueue(next, next);
                }
            }

            return count == n ? order : null;
        }


        // Задача 4
        // Нужно развернуть каждый полный блок из k узлов, неполный хвост оставить.
        // Выбрал разворот по ссылкам на месте: перед каждым блоком проверяем, что в нём
        // есть k узлов, разворачиваем его теми же тремя ссылками, что и в задаче 1,
        // и пристыковываем к предыдущему блоку. Время O(n), память O(1).
        private static ListNode? ReverseInGroups(ListNode? head, int k)
        {
            ListNode dummy = new ListNode(0);
            dummy.Next = head;
            ListNode groupPrevious = dummy;

            while (true)
            {
                // Проверяем, что дальше есть полный блок из k узлов.
                ListNode? check = groupPrevious.Next;
                int available = 0;

                while (check != null && available < k)
                {
                    check = check.Next;
                    available++;
                }

                if (available < k)
                    break;

                ListNode groupFirst = groupPrevious.Next!;
                ListNode? previous = check;
                ListNode? current = groupFirst;

                for (int i = 0; i < k; i++)
                {
                    ListNode? next = current!.Next;
                    current.Next = previous;
                    previous = current;
                    current = next;
                }

                groupPrevious.Next = previous;
                groupPrevious = groupFirst;
            }

            return dummy.Next;
        }


        // DSU с чётностью для задачи 5: для каждой вершины храним, в одной ли она группе
        // со своим родителем (0) или в разных (1).
        private class ParityDisjointSet
        {
            private readonly int[] parent;
            private readonly int[] parity;
            private readonly int[] size;

            public ParityDisjointSet(int n)
            {
                parent = new int[n + 1];
                parity = new int[n + 1];
                size = new int[n + 1];

                for (int i = 0; i <= n; i++)
                {
                    parent[i] = i;
                    size[i] = 1;
                }
            }

            // Возвращает корень, а parity[x] после вызова - чётность относительно корня.
            // Рекурсия неглубокая, потому что объединение идёт по размеру.
            private int Find(int x)
            {
                if (parent[x] == x)
                    return x;

                int oldParent = parent[x];
                int root = Find(oldParent);

                parity[x] ^= parity[oldParent];
                parent[x] = root;

                return root;
            }

            // Добавляет утверждение: relation = 0 - одна группа, 1 - разные.
            // Возвращает false, если утверждение противоречит прежним.
            public bool Add(int a, int b, int relation)
            {
                int rootA = Find(a);
                int rootB = Find(b);

                if (rootA == rootB)
                    return (parity[a] ^ parity[b]) == relation;

                int needed = parity[a] ^ parity[b] ^ relation;

                if (size[rootA] < size[rootB])
                    (rootA, rootB) = (rootB, rootA);

                parent[rootB] = rootA;
                parity[rootB] = needed;
                size[rootA] += size[rootB];

                return true;
            }
        }

        // Задача 5
        // Нужно после каждого утверждения "в одной группе / в разных" проверять,
        // что все утверждения ещё совместимы.
        // Выбрал DSU с чётностью: он хранит для каждого человека, совпадает ли его
        // группа с группой родителя. Противоречие возникает, когда два человека уже
        // в одном множестве, а требуемое отношение отличается от имеющегося.
        public static List<string> CheckStatements(int n, int[][] statements)
        {
            ParityDisjointSet set = new ParityDisjointSet(n);
            List<string> answers = new List<string>();
            bool broken = false;

            foreach (int[] statement in statements)
            {
                if (!broken && !set.Add(statement[0], statement[1], statement[2]))
                    broken = true;

                answers.Add(broken ? "NO" : "YES");
            }

            return answers;
        }


        // Задача 6
        // Нужно восстановить порядок букв по отсортированному списку слов.
        // Выбрал топологическую сортировку: для соседних слов первая различающаяся
        // позиция даёт требование "буква раньше буквы". Если слово длиннее и оно
        // стоит перед своим началом, порядка нет. Из готовых букв берём наименьшую
        // (минимальная куча), чтобы ответ был лексикографически минимальным.
        public static string? RestoreAlphabet(string[] words, int k)
        {
            bool[,] edge = new bool[k, k];
            List<int>[] graph = new List<int>[k];
            int[] inDegree = new int[k];

            for (int i = 0; i < k; i++)
                graph[i] = new List<int>();

            for (int i = 1; i < words.Length; i++)
            {
                string previous = words[i - 1];
                string current = words[i];
                int length = Math.Min(previous.Length, current.Length);
                int j = 0;

                while (j < length && previous[j] == current[j])
                    j++;

                if (j == length)
                {
                    if (previous.Length > current.Length)
                        return null;

                    continue;
                }

                int from = previous[j] - 'a';
                int to = current[j] - 'a';

                if (!edge[from, to])
                {
                    edge[from, to] = true;
                    graph[from].Add(to);
                    inDegree[to]++;
                }
            }

            PriorityQueue<int, int> ready = new PriorityQueue<int, int>();

            for (int i = 0; i < k; i++)
            {
                if (inDegree[i] == 0)
                    ready.Enqueue(i, i);
            }

            StringBuilder order = new StringBuilder();

            while (ready.Count > 0)
            {
                int letter = ready.Dequeue();
                order.Append((char)('a' + letter));

                foreach (int next in graph[letter])
                {
                    inDegree[next]--;

                    if (inDegree[next] == 0)
                        ready.Enqueue(next, next);
                }
            }

            return order.Length == k ? order.ToString() : null;
        }


        static void Main(string[] args)
        {
            // Задача 1
            int[] nums = { 1, 2, 3, 4, 5 };
            Console.WriteLine(ListToString(ReverseList(BuildList(nums))));


            // Задача 2
            int[][] connections =
            {
                new int[] { 1, 1, 2 },
                new int[] { 2, 1, 3 },
                new int[] { 1, 2, 3 },
                new int[] { 2, 1, 3 },
                new int[] { 2, 4, 4 }
            };
            Console.WriteLine(string.Join(" ", ProcessConnections(4, connections)));


            // Задача 3
            int[][] requirements =
            {
                new int[] { 3, 1 },
                new int[] { 4, 1 },
                new int[] { 2, 4 }
            };
            int[]? order = SmallestTopologicalOrder(4, requirements);
            Console.WriteLine(order == null ? "-1" : string.Join(" ", order));


            // Задача 4
            int[] nums2 = { 1, 2, 3, 4, 5 };
            Console.WriteLine(ListToString(ReverseInGroups(BuildList(nums2), 2)));


            // Задача 5
            int[][] statements =
            {
                new int[] { 1, 2, 1 },
                new int[] { 2, 3, 1 },
                new int[] { 1, 3, 0 },
                new int[] { 1, 3, 1 }
            };
            Console.WriteLine(string.Join(" ", CheckStatements(3, statements)));


            // Задача 6
            string[] words = { "baa", "abcd", "abca", "cab", "cad" };
            Console.WriteLine(RestoreAlphabet(words, 4) ?? "IMPOSSIBLE");
        }
    }
}
