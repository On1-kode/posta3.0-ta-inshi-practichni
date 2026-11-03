using System.Text;

Console.OutputEncoding = Encoding.UTF8;
int[] numbers = new int[20];
Random random = new();
for (int i = 0; i < numbers.Length; i++) numbers[i] = random.Next(1, 13);

Thread[] threads = new Thread[numbers.Length];
for (int i = 0; i < numbers.Length; i++)
{
    int num = numbers[i];
    threads[i] = new Thread(() =>
    {
        int numF = 1;
        for (int j = 2; j <= num; j++) numF *= j;
        Console.WriteLine($"факторіал числа: {num}! = {numF}");
    });
    threads[i].Start();
}

foreach (Thread thread in threads) thread.Join();
Console.WriteLine("Розрахунок закінчено.");
