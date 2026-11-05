using System.Net;
using PostService.Dtos;
namespace PostService.App;
public class PostingList : Form
{
    private readonly IPostServiceClient client;
    private readonly Label countLabel;
    private PostingGetDto[] allPostings = [];
    private readonly DataGridView gridPostings = new()
    {
        Name = "gridPostings", Dock = DockStyle.Fill, ReadOnly = true,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
    };
    public PostingList(IPostServiceClient client)
    {
        this.client = client; Text = "Облік поштових відправлень";
        AppStyle.Apply(this);
        Width = 1320; Height = 780; MinimumSize = new Size(980, 600); StartPosition = FormStartPosition.CenterScreen;
        string[] fields = ["Id", "SenderName", "From", "RecipientName", "To", "Content", "DeliveryType", "Weight", "Price", "SenderPhone", "SenderAddress", "RecipientPhone", "RecipientAddress", "Width", "Height", "Depth", "Value", "Description", "CreatedAt"];
        string[] titles = ["Id", "Відправник", "Місто відправлення", "Отримувач", "Місто отримання", "Вміст", "Доставка", "Вага, кг", "Вартість, грн", "Телефон відправника", "Адреса відправлення", "Телефон отримувача", "Адреса отримання", "Ширина, см", "Висота, см", "Глибина, см", "Цінність, грн", "Коментар", "Створено"];
        for (int i = 0; i < fields.Length; i++) gridPostings.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = fields[i], DataPropertyName = fields[i], HeaderText = titles[i],
            Visible = fields[i] is not ("Id" or "CreatedAt"),
            Width = fields[i] is "SenderName" or "RecipientName" ? 170 : fields[i] is "From" or "To" ? 135 : fields[i] is "Weight" or "Price" ? 100 : 150
        });
        gridPostings.CellFormatting += (_, e) =>
        {
            if (gridPostings.Columns[e.ColumnIndex].Name == "DeliveryType" && e.Value is PostService.CommonTypes.DeliveryType type)
            { e.Value = type switch { PostService.CommonTypes.DeliveryType.Department => "Відділення", PostService.CommonTypes.DeliveryType.Courier => "Курʼєр", _ => "Експрес" }; e.FormattingApplied = true; }
        };
        gridPostings.BackgroundColor = Color.White; gridPostings.BorderStyle = BorderStyle.None;
        gridPostings.RowHeadersVisible = false; gridPostings.EnableHeadersVisualStyles = false;
        gridPostings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; gridPostings.ColumnHeadersHeight = 64;
        gridPostings.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(250, 250, 250), ForeColor = AppStyle.Muted, Font = new Font("Segoe UI", 9), Padding = new Padding(8), WrapMode = DataGridViewTriState.True };
        gridPostings.DefaultCellStyle.Padding = new Padding(8); gridPostings.RowTemplate.Height = 54;
        gridPostings.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 237, 232); gridPostings.DefaultCellStyle.SelectionForeColor = AppStyle.Ink;
        gridPostings.GridColor = Color.FromArgb(235, 236, 238);
        gridPostings.Columns["Price"].DefaultCellStyle.Format = "0.00";
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 112, Padding = new Padding(24, 12, 0, 0), BackColor = Color.White };
        var title = new Label { Text = "PostService  /  Мої відправлення", Dock = DockStyle.Top, Height = 88, Padding = new Padding(24, 24, 0, 0), Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = AppStyle.Ink };
        var search = new TextBox { Name = "txtSearch", PlaceholderText = "Пошук за ім’ям, містом або вмістом", Width = 390, Margin = new Padding(0, 3, 18, 0) };
        search.Margin = new Padding(0, 0, 0, 10);
        bar.Controls.Add(search); bar.SetFlowBreak(search, true);
        search.TextChanged += (_, _) => FilterRows(search.Text);
        var status = new Label { Name = "lblCount", Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(24, 12, 0, 0), ForeColor = AppStyle.Muted, Text = "Завантаження відправлень…" };
        countLabel = status;
        void Button(string name, string text, Func<Task> action)
        {
            var b = AppStyle.Button(name, text, name == "btnNew");
            b.Click += async (_, _) => await action(); bar.Controls.Add(b);
        }
        Button("btnRefresh", "Оновити", LoadPostingsAsync);
        Button("btnNew", "Нове відправлення", async () =>
        { using var f = new PostingEditor(client, null); f.ShowDialog(this); await LoadPostingsAsync(); });
        Button("btnEdit", "Редагувати", () => OpenAsync(false));
        Button("btnDetails", "Деталі", () => OpenAsync(true));
        Button("btnDelete", "Видалити", DeleteAsync);
        Button("btnHome", "Головна", () => { ShowInfo(true); return Task.CompletedTask; });
        Button("btnAbout", "Про програму", () => { ShowAbout(); return Task.CompletedTask; });
        var tablePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 18, 24, 0) }; tablePanel.Controls.Add(gridPostings);
        Controls.Add(tablePanel); Controls.Add(status); Controls.Add(bar); Controls.Add(title);
        Load += async (_, _) => await LoadPostingsAsync();
    }
    private void ShowAbout() => ShowInfo(false);
    private void ShowInfo(bool home)
    {
        using var form = new Form { Text = home ? "PostService" : "Про програму", ClientSize = new Size(620, 360), StartPosition = FormStartPosition.CenterParent, MaximizeBox = false };
        AppStyle.Apply(form);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(28), FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.White };
        panel.Controls.Add(new Label { Text = home ? "Посилки під контролем" : "Про PostService", AutoSize = true, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = AppStyle.Ink, Margin = new Padding(0, 0, 0, 24) });
        panel.Controls.Add(new Label { Text = home ? "Створюйте відправлення, редагуйте контакти та переглядайте деталі доставки." : "Навчальний застосунок для обліку поштових відправлень. Вебклієнт і Windows-застосунок використовують спільні дані. Вартість розраховується за умовними навчальними тарифами.", AutoSize = true, MaximumSize = new Size(540, 0), ForeColor = AppStyle.Muted, Margin = new Padding(0, 0, 0, 28) });
        var close = AppStyle.Button("btnCloseInfo", home ? "До відправлень" : "Закрити", true); close.Click += (_, _) => form.Close(); panel.Controls.Add(close);
        if (home) { var about = AppStyle.Button("btnAboutInfo", "Про програму"); about.Click += (_, _) => ShowInfo(false); panel.Controls.Add(about); }
        form.Controls.Add(panel); form.ShowDialog(this);
    }
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
    private void FilterRows(string query)
    {
        string q = query.Trim();
        var rows = allPostings.Where(p => string.Join(" ", p.SenderName, p.RecipientName, p.From, p.To, p.Content, p.Id).Contains(q, StringComparison.CurrentCultureIgnoreCase)).ToArray();
        gridPostings.DataSource = rows; countLabel.Text = $"Усього: {allPostings.Length}  ·  Показано: {rows.Length}";
    }
    private async Task LoadPostingsAsync() => await WithRetryAsync(async () =>
    {
        allPostings = await client.GetPostingsAsync();
        FilterRows(Controls.Find("txtSearch", true).OfType<TextBox>().First().Text);
    });
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
