using System.Text;

Console.OutputEncoding = Encoding.UTF8;
int[] fullArray = new int[1000];
Random random = new();
for (int i = 0; i < fullArray.Length; i++) fullArray[i] = random.Next(1, 11);

List<int> evenList = new();
List<int> oddList = new();
int nextIndex = 0;
object sync = new();

void DistributeNumbers()
{
    while (true)
    {
        lock (sync)
        {
            if (nextIndex >= fullArray.Length) return;
            int number = fullArray[nextIndex++];
            if (number % 2 == 0) evenList.Add(number);
            else oddList.Add(number);
        }
    }
}

Thread[] threads = new Thread[3];
for (int i = 0; i < threads.Length; i++)
{
    threads[i] = new Thread(DistributeNumbers);
    threads[i].Start();
}
foreach (Thread thread in threads) thread.Join();

Console.WriteLine("Парні елементи:");
Console.WriteLine(string.Join(" ", evenList));
Console.WriteLine("Непарні елементи:");
Console.WriteLine(string.Join(" ", oddList));
