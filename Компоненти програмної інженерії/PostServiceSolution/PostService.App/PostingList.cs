using System.Net;
using PostService.Dtos;
namespace PostService.App;
public class PostingList : Form
{
    private readonly IPostServiceClient client;
    private readonly DataGridView gridPostings = new()
    {
        Name = "gridPostings", Dock = DockStyle.Fill, ReadOnly = true,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    };
    public PostingList(IPostServiceClient client)
    {
        this.client = client; Text = "Облік поштових відправлень";
        Width = 1250; Height = 600; StartPosition = FormStartPosition.CenterScreen;
        string[] fields = ["Id", "From", "To", "Content", "DeliveryType", "Weight", "Width", "Height", "Depth", "Value", "Price", "CreatedAt"];
        string[] titles = ["Id", "Відправник", "Отримувач", "Вміст", "Доставка", "Вага, кг", "Ширина, см", "Висота, см", "Глибина, см", "Цінність, грн", "Вартість, грн", "Створено"];
        for (int i = 0; i < fields.Length; i++) gridPostings.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = fields[i], DataPropertyName = fields[i], HeaderText = titles[i],
            Visible = fields[i] is not ("Id" or "CreatedAt")
        });
        gridPostings.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex == 4 && e.Value is PostService.CommonTypes.DeliveryType type)
            { e.Value = type switch { PostService.CommonTypes.DeliveryType.Department => "Відділення", PostService.CommonTypes.DeliveryType.Courier => "Курʼєр", _ => "Експрес" }; e.FormattingApplied = true; }
        };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
        void Button(string name, string text, Func<Task> action)
        {
            var b = new Button { Name = name, Text = text, AutoSize = true };
            b.Click += async (_, _) => await action(); bar.Controls.Add(b);
        }
        Button("btnRefresh", "Оновити", LoadPostingsAsync);
        Button("btnNew", "Нове відправлення", async () =>
        { using var f = new PostingEditor(client, null); f.ShowDialog(this); await LoadPostingsAsync(); });
        Button("btnEdit", "Редагувати", () => OpenAsync(false));
        Button("btnDetails", "Деталі", () => OpenAsync(true));
        Button("btnDelete", "Видалити", DeleteAsync);
        Button("btnHome", "Головна", () =>
        {
            using var f = new Form { Text = "PostService", Size = new Size(470, 220), StartPosition = FormStartPosition.CenterParent };
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown };
            panel.Controls.Add(new Label { Text = "Облік поштових відправлень", AutoSize = true });
            var list = new Button { Text = "Список відправлень", AutoSize = true }; list.Click += (_, _) => f.Close();
            var about = new Button { Text = "Про програму", AutoSize = true }; about.Click += (_, _) => ShowAbout();
            panel.Controls.AddRange([list, about]); f.Controls.Add(panel); f.ShowDialog(this); return Task.CompletedTask;
        });
        Button("btnAbout", "Про програму", () => { ShowAbout(); return Task.CompletedTask; });
        Controls.Add(gridPostings); Controls.Add(bar);
        Load += async (_, _) => await LoadPostingsAsync();
    }
    private void ShowAbout() => MessageBox.Show(this, "PostService — навчальний застосунок для обліку поштових відправлень. Windows Forms і вебклієнт працюють зі спільним REST API та SQLite.", "Про програму");
    private async Task<bool> WithRetryAsync(Func<Task> action)
    {
        while (true)
        {
            try { await action(); return true; }
            catch (HttpRequestException e) when (e.StatusCode is null)
            {
                if (MessageBox.Show(this, "Сервер недоступний. Повторити спробу?", "З’єднання", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) != DialogResult.Retry) return false;
            }
        }
    }
    private async Task LoadPostingsAsync() => await WithRetryAsync(async () => gridPostings.DataSource = await client.GetPostingsAsync());
    private async Task OpenAsync(bool readOnly)
    {
        if (gridPostings.CurrentRow?.DataBoundItem is not PostingGetDto selected) return;
        try
        {
            PostingGetDto? posting = null;
            if (!await WithRetryAsync(async () => posting = await client.GetPostingByIdAsync(selected.Id))) return;
            using var form = new PostingEditor(client, posting, readOnly); form.ShowDialog(this);
            await LoadPostingsAsync();
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        { MessageBox.Show(this, "Відправлення вже видалено на сервері."); await LoadPostingsAsync(); }
    }
    private async Task DeleteAsync()
    {
        if (gridPostings.CurrentRow?.DataBoundItem is not PostingGetDto selected) return;
        if (MessageBox.Show(this, $"Видалити відправлення №{selected.Id}?", "Підтвердження видалення", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { if (await WithRetryAsync(() => client.DeletePostingAsync(selected.Id))) await LoadPostingsAsync(); }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        { MessageBox.Show(this, "Відправлення вже видалено на сервері."); await LoadPostingsAsync(); }
    }
}
