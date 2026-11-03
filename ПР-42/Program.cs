using System.Text;

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("Введіть ціле число: ");
    try
    {
        int number = Int32.Parse(Console.ReadLine()!);
        Console.WriteLine($"Отримане число: {number}");
        break;
    }
    catch (FormatException)
    {
        Console.WriteLine("Введене значення не є числом. Введіть число знову.");
    }
    catch (OverflowException)
    {
        Console.WriteLine("Значення виходить за межі допустимого діапазону Int32. Введіть число знову.");
    }
    catch (Exception)
    {
        Console.WriteLine("Не вдалося перетворити значення на число. Введіть число знову.");
    }
}
