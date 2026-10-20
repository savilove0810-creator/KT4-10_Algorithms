# KT5_Algorithms

```csharp
using System.Text;

namespace KT5_Algorithms
{
    internal class Program
    {
        // Префиксное дерево для задач 2 и 5. В каждой вершине храним, сколько слов
        // лежит в её поддереве (включая слово, которое заканчивается в самой вершине),
        // и признак того, что в вершине заканчивается слово.
        private class WordTrie
        {
            private class Node
            {
                public Node?[] Next = new Node?[26];
                public int Count;
                public bool IsWord;
            }

            private readonly Node root = new Node();

            // Слово перед добавлением в словаре отсутствует (по условию).
            public void Add(string word)
            {
                Node node = root;
                node.Count++;

                foreach (char c in word)
                {
                    int index = c - 'a';

                    if (node.Next[index] == null)
                        node.Next[index] = new Node();

                    node = node.Next[index]!;
                    node.Count++;
                }

                node.IsWord = true;
            }

            // Слово перед удалением в словаре присутствует (по условию).
            public void Remove(string word)
            {
                Node node = root;
                node.Count--;

                foreach (char c in word)
                {
                    node = node.Next[c - 'a']!;
                    node.Count--;
                }

                node.IsWord = false;
            }

            private Node? Find(string prefix)
            {
                Node? node = root;

                foreach (char c in prefix)
                {
                    node = node.Next[c - 'a'];

                    if (node == null)
                        return null;
                }

                return node;
            }

            public int CountPrefix(string prefix)
            {
                Node? node = Find(prefix);
                return node == null ? 0 : node.Count;
            }

            // k-е слово среди слов с данным префиксом в лексикографическом порядке.
            // Сначала идёт само слово-префикс (оно короче всех остальных),
            // потом поддеревья букв от 'a' до 'z'.
            public string? Kth(string prefix, int k)
            {
                Node? node = Find(prefix);

                if (node == null || node.Count < k)
                    return null;

                StringBuilder word = new StringBuilder(prefix);

                while (true)
                {
                    if (node!.IsWord)
                    {
                        if (k == 1)
                            return word.ToString();

                        k--;
                    }

                    Node? chosen = null;

                    for (int i = 0; i < 26; i++)
                    {
                        Node? child = node.Next[i];

                        if (child == null)
                            continue;

                        if (k <= child.Count)
                        {
                            chosen = child;
                            word.Append((char)('a' + i));
                            break;
                        }

                        k -= child.Count;
                    }

                    node = chosen;
                }
            }
        }


        // Задача 1
        // Нужно вывести все правильные скобочные последовательности из n пар.
        // Выбрал поиск с возвратом (backtracking): строим строку по символу и
        // ставим '(' если открывающих меньше n, а ')' если закрывающих меньше открывающих.
        // Сначала пробуем '(' - так последовательности получаются в лексикографическом порядке.
        public static List<string> GenerateParentheses(int n)
        {
            List<string> result = new List<string>();
            char[] current = new char[2 * n];

            void Build(int position, int open, int close)
            {
                if (position == 2 * n)
                {
                    result.Add(new string(current));
                    return;
                }

                if (open < n)
                {
                    current[position] = '(';
                    Build(position + 1, open + 1, close);
                }

                if (close < open)
                {
                    current[position] = ')';
                    Build(position + 1, open, close + 1);
                }
            }

            Build(0, 0, 0);

            return result;
        }


        // Задача 2
        // Нужно обрабатывать ADD, REMOVE и COUNT по префиксу.
        // Выбрал префиксное дерево со счётчиками: путь по префиксу занимает O(длина),
        // а счётчик в конечной вершине сразу даёт количество слов с этим префиксом.
        public static List<int> ProcessDictionary(string[] operations)
        {
            WordTrie trie = new WordTrie();
            List<int> answers = new List<int>();

            foreach (string operation in operations)
            {
                string[] parts = operation.Split(' ');

                if (parts[0] == "ADD")
                    trie.Add(parts[1]);
                else if (parts[0] == "REMOVE")
                    trie.Remove(parts[1]);
                else
                    answers.Add(trie.CountPrefix(parts[1]));
            }

            return answers;
        }


        // Задача 3
        // Нужно найти два числа, которые встречаются по одному разу, остальные - по два.
        // Выбрал манипуляцию битами (XOR): парные числа взаимно уничтожаются,
        // остаётся x ^ y. Любой единичный бит этой суммы - бит, в котором числа
        // различаются, поэтому по нему делим все числа на две группы и в каждой
        // снова делаем XOR. Память O(1), время O(n).
        public static int[] FindTwoSingles(int[] nums)
        {
            int xor = 0;

            foreach (int x in nums)
                xor ^= x;

            int lowestBit = xor & -xor;
            int first = 0;
            int second = 0;

            foreach (int x in nums)
            {
                if ((x & lowestBit) != 0)
                    first ^= x;
                else
                    second ^= x;
            }

            return new int[] { Math.Min(first, second), Math.Max(first, second) };
        }


        // Задача 4
        // Нужно посчитать расстановки n ферзей с запрещёнными клетками.
        // Выбрал поиск с возвратом по строкам с битовыми масками: занятые столбцы и
        // две диагонали хранятся в числах, поэтому свободные клетки строки
        // находим одной операцией, а запрещённые клетки строки - ещё одна маска.
        public static long CountQueens(int n, int[][] forbidden)
        {
            int[] forbiddenRow = new int[n];

            foreach (int[] cell in forbidden)
                forbiddenRow[cell[0] - 1] |= 1 << (cell[1] - 1);

            int all = (1 << n) - 1;

            long Place(int row, int columns, int diagonal1, int diagonal2)
            {
                if (row == n)
                    return 1;

                long count = 0;
                int free = all & ~(columns | diagonal1 | diagonal2 | forbiddenRow[row]);

                while (free != 0)
                {
                    int bit = free & -free;
                    free ^= bit;

                    count += Place(row + 1,
                        columns | bit,
                        ((diagonal1 | bit) << 1) & all,
                        (diagonal2 | bit) >> 1);
                }

                return count;
            }

            return Place(0, 0, 0, 0);
        }


        // Задача 5
        // Нужно искать k-е слово с данным префиксом в изменяющемся множестве.
        // Выбрал префиксное дерево со счётчиками поддеревьев: спускаемся по префиксу,
        // а дальше на каждом шаге пропускаем целые поддеревья букв, пока k не попадёт
        // внутрь одного из них. Порядок букв даёт лексикографический порядок слов.
        public static List<string> ProcessKth(string[] operations)
        {
            WordTrie trie = new WordTrie();
            List<string> answers = new List<string>();

            foreach (string operation in operations)
            {
                string[] parts = operation.Split(' ');

                if (parts[0] == "ADD")
                    trie.Add(parts[1]);
                else if (parts[0] == "REMOVE")
                    trie.Remove(parts[1]);
                else
                    answers.Add(trie.Kth(parts[1], int.Parse(parts[2])) ?? "NONE");
            }

            return answers;
        }


        // Задача 6
        // Нужно найти максимальный XOR непустого подмассива.
        // XOR подмассива равен prefix[r] ^ prefix[l], поэтому для каждого префикса
        // ищем уже встретившийся префикс, дающий максимальный XOR.
        // Выбрал бинарное префиксное дерево: идём от старшего бита к младшему и
        // на каждом шаге стараемся пойти в ветку с противоположным битом.
        public static int MaxSubarrayXor(int[] nums)
        {
            const int bits = 30;

            // Дерево хранится в массиве: у вершины v дети лежат в child[2 * v] и child[2 * v + 1].
            int[] child = new int[2 * (nums.Length + 1) * bits + 2];
            int nodes = 1;

            void Insert(int value)
            {
                int node = 0;

                for (int bit = bits - 1; bit >= 0; bit--)
                {
                    int index = 2 * node + ((value >> bit) & 1);

                    if (child[index] == 0)
                        child[index] = nodes++;

                    node = child[index];
                }
            }

            int FindBest(int value)
            {
                int node = 0;
                int best = 0;

                for (int bit = bits - 1; bit >= 0; bit--)
                {
                    int wanted = 1 - ((value >> bit) & 1);

                    if (child[2 * node + wanted] != 0)
                    {
                        best |= 1 << bit;
                        node = child[2 * node + wanted];
                    }
                    else
                        node = child[2 * node + (1 - wanted)];
                }

                return best;
            }

            int result = 0;
            int prefix = 0;
            Insert(0);

            foreach (int x in nums)
            {
                prefix ^= x;
                result = Math.Max(result, FindBest(prefix));
                Insert(prefix);
            }

            return result;
        }


        static void Main(string[] args)
        {
            // Задача 1
            foreach (string s in GenerateParentheses(3))
                Console.WriteLine(s);


            // Задача 2
            string[] operations = { "ADD apple", "ADD app", "ADD apt", "COUNT ap", "REMOVE app", "COUNT app", "COUNT ap" };
            Console.WriteLine(string.Join(" ", ProcessDictionary(operations)));


            // Задача 3
            int[] nums = { 1, 2, 1, 3, 2, 5 };
            Console.WriteLine(string.Join(" ", FindTwoSingles(nums)));


            // Задача 4
            int[][] forbidden = { new int[] { 1, 1 } };
            Console.WriteLine(CountQueens(6, forbidden));


            // Задача 5
            string[] operations2 = { "ADD app", "ADD apple", "ADD apt", "ADD bat", "KTH ap 1", "KTH ap 2", "KTH ap 3", "KTH ap 4", "REMOVE app", "KTH a 1" };
            Console.WriteLine(string.Join(" ", ProcessKth(operations2)));


            // Задача 6
            int[] nums2 = { 8, 1, 2, 12, 7, 6 };
            Console.WriteLine(MaxSubarrayXor(nums2));
        }
    }
}
```
