using System.Diagnostics;
using System.Reflection;

namespace PostService.WindowsApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ReportUnhandledException((Exception)e.ExceptionObject);

        using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5080") };
        IPostServiceClient client = new PostServiceClient(httpClient);
        Application.Run(new PostingList(client));
    }

    private static void ReportUnhandledException(Exception exception)
    {
        while (exception is TargetInvocationException { InnerException: not null })
            exception = exception.InnerException;

        Debug.WriteLine($"Час: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Debug.WriteLine($"Тип: {exception.GetType().FullName}");
        Debug.WriteLine($"Повідомлення: {exception.Message}");
        Debug.WriteLine($"Стек викликів: {exception.StackTrace}");
        Debug.WriteLine(exception.ToString());
        MessageBox.Show("Сталася непередбачена помилка. Застосунок буде закрито.",
            "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
