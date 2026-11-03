using System.Text;

Console.OutputEncoding = Encoding.UTF8;
string path = Path.Combine(AppContext.BaseDirectory, "students.bin");
List<Student> students = new()
{
    new Student { OrderNo = 1, StudentName = "Іван" },
    new Student { OrderNo = 2, StudentName = "Олена" },
    new Student { OrderNo = 3, StudentName = "Андрій" },
    new Student { OrderNo = 4, StudentName = "Марія Коваль" },
    new Student { OrderNo = 5, StudentName = "" }
};

using (FileStream stream = new(path, FileMode.Create))
using (BinaryWriter writer = new(stream))
{
    foreach (Student student in students) WriteStudent(writer, student);
}

List<Student> restored = new();
using (FileStream stream = new(path, FileMode.Open))
using (BinaryReader reader = new(stream))
{
    while (stream.Position < stream.Length) restored.Add(ReadStudent(reader));
}

Console.WriteLine("Відновлений список студентів:");
foreach (Student student in restored)
{
    Console.WriteLine($"{student.OrderNo}: {student.StudentName}");
}
bool equal = students.Count == restored.Count;
for (int i = 0; equal && i < students.Count; i++)
{
    equal = students[i].OrderNo == restored[i].OrderNo &&
            students[i].StudentName == restored[i].StudentName;
}
Console.WriteLine(equal ? "Відновлений список збігається з вихідним." : "Списки відрізняються.");

static void WriteStudent(BinaryWriter writer, Student student)
{
    byte[] nameBytes = Encoding.UTF8.GetBytes(student.StudentName);
    writer.Write(student.OrderNo);
    writer.Write(nameBytes.Length);
    writer.Write(nameBytes);
}

static Student ReadStudent(BinaryReader reader)
{
    int orderNo = reader.ReadInt32();
    int length = reader.ReadInt32();
    if (length < 0 || length > reader.BaseStream.Length - reader.BaseStream.Position)
        throw new InvalidDataException("Некоректна довжина імені.");
    byte[] nameBytes = reader.ReadBytes(length);
    if (nameBytes.Length != length) throw new EndOfStreamException();
    return new Student { OrderNo = orderNo, StudentName = Encoding.UTF8.GetString(nameBytes) };
}

public class Student
{
    public int OrderNo { get; set; }
    public string StudentName { get; set; } = "";
}
