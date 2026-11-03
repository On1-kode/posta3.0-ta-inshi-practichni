using System.Net;
using PostService.Contracts;

namespace PostService.WindowsApp;

public class PostingList : Form
{
    private readonly IPostServiceClient client;
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public PostingList(IPostServiceClient client)
    {
        this.client = client;
        Text = "Поштові відправлення";
        Width = 1000;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        var refresh = new Button { Text = "Оновити", AutoSize = true };
        var add = new Button { Text = "Додати", AutoSize = true };
        var edit = new Button { Text = "Редагувати", AutoSize = true };
        var delete = new Button { Text = "Видалити", AutoSize = true };
        buttons.Controls.AddRange([refresh, add, edit, delete]);
        Controls.Add(grid);
        Controls.Add(buttons);
        Shown += async (_, _) => await ReloadAsync();
        refresh.Click += async (_, _) => await ReloadAsync();
        add.Click += async (_, _) =>
        {
            using var form = new PostingEditor(client, null);
            form.ShowDialog(this);
            await ReloadAsync();
        };
        edit.Click += async (_, _) => await EditAsync();
        delete.Click += async (_, _) => await DeleteAsync();
    }

    private async Task<bool> WithRetryAsync(Func<Task> action)
    {
        while (true)
        {
            try
            {
                await action();
                return true;
            }
            catch (HttpRequestException e) when (e.StatusCode is null)
            {
                if (MessageBox.Show(this, "Сервер недоступний. Повторити спробу?",
                    "З’єднання", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning)
                    != DialogResult.Retry) return false;
            }
        }
    }

    private async Task ReloadAsync()
    {
        await WithRetryAsync(async () =>
        {
            grid.DataSource = await client.GetPostingsAsync();
            string[] headers = ["Трек-номер", "Вага, кг", "Відправник", "Отримувач", "Статус", "Id"];
            string[] properties = ["TrackNumber", "Weight", "SenderAddress", "RecipientAddress", "Status", "Id"];
            for (int i = 0; i < properties.Length; i++)
                grid.Columns[properties[i]]!.HeaderText = headers[i];
        });
    }

    private async Task EditAsync()
    {
        if (grid.CurrentRow?.DataBoundItem is not PostingGetDto selected) return;
        try
        {
            PostingGetDto? posting = null;
            if (!await WithRetryAsync(async () => posting = await client.GetPostingByIdAsync(selected.Id))) return;
            using var form = new PostingEditor(client, posting);
            form.ShowDialog(this);
            await ReloadAsync();
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            MessageBox.Show(this, "Відправлення вже видалено на сервері.", "Запис відсутній");
            await ReloadAsync();
        }
    }

    private async Task DeleteAsync()
    {
        if (grid.CurrentRow?.DataBoundItem is not PostingGetDto selected) return;
        try
        {
            if (await WithRetryAsync(() => client.DeletePostingAsync(selected.Id))) await ReloadAsync();
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            MessageBox.Show(this, "Відправлення вже видалено на сервері.", "Запис відсутній");
            await ReloadAsync();
        }
    }
}
