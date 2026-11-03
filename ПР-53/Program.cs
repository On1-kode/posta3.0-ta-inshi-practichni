using System.Text;

Console.OutputEncoding = Encoding.UTF8;
string path = Path.Combine(AppContext.BaseDirectory, "numbers.txt");
Encoding encoding = new UTF8Encoding(false);
int first = 1, second = 25, third = 300, replacement = 42;

using (StreamWriter writer = new(path, false, encoding))
{
    writer.WriteLine(first.ToString("D8"));
}
Console.WriteLine($"1. Створено numbers.txt. Записано: {first:D8}");

using (StreamReader reader = new(path, encoding))
{
    int number = int.Parse(reader.ReadLine()!);
    Console.WriteLine($"2. Прочитане ціле число: {number}");
}

using (StreamWriter writer = new(path, true, encoding))
{
    writer.WriteLine(second.ToString("D8"));
    writer.WriteLine(third.ToString("D8"));
}
Console.WriteLine($"3. Дописано два рядки: {second:D8}, {third:D8}");

Console.WriteLine("4. Вміст файла до заміни:");
PrintFile(path, encoding);

// UTF-8 без BOM: вісім ASCII-цифр та переведення рядка першого запису.
long secondPosition = encoding.GetByteCount(first.ToString("D8") + Environment.NewLine);
using (FileStream stream = new(path, FileMode.Open, FileAccess.Write))
{
    stream.Seek(secondPosition, SeekOrigin.Begin);
    using (StreamWriter writer = new(stream, encoding))
    {
        writer.Write(replacement.ToString("D8"));
    }
}
Console.WriteLine("5. Вміст файла після заміни другого числа:");
PrintFile(path, encoding);

static void PrintFile(string path, Encoding encoding)
{
    using (StreamReader reader = new(path, encoding))
    {
        string? line;
        int lineNumber = 1;
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine($"{lineNumber++}: {line}");
        }
    }
}
