using System.Net.Http.Json;
using PostService.Dtos;
namespace PostService.App;
public class PostingList : Form
{
    private readonly HttpClient httpClient = new() { BaseAddress = new Uri("http://localhost:5082") };
    private readonly DataGridView gridPostings = new()
    {
        Name = "gridPostings", Dock = DockStyle.Fill, ReadOnly = true,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    };
    public PostingList()
    {
        Text = "Поштові відправлення"; Width = 1250; Height = 600;
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

        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        var btnRefresh = new Button { Name = "btnRefresh", Text = "Оновити", AutoSize = true };
        var btnDelete = new Button { Name = "btnDelete", Text = "Видалити", AutoSize = true };
        bar.Controls.AddRange([btnRefresh, btnDelete]); Controls.Add(gridPostings); Controls.Add(bar);
        Load += async (_, _) => await LoadPostingsAsync();
        btnRefresh.Click += async (_, _) => await LoadPostingsAsync();
        btnDelete.Click += async (_, _) => await DeleteAsync();
        FormClosed += (_, _) => httpClient.Dispose();
    }
    private async Task LoadPostingsAsync()
    {
        try { gridPostings.DataSource = await httpClient.GetFromJsonAsync<PostingGetDto[]>("/postings"); }
        catch (HttpRequestException) { MessageBox.Show(this, "Не вдалося отримати список із сервера."); }
    }
    private async Task DeleteAsync()
    {
        if (gridPostings.CurrentRow?.DataBoundItem is not PostingGetDto p) return;
        try
        {
            using var response = await httpClient.DeleteAsync($"/postings/{p.Id}");
            if (!response.IsSuccessStatusCode) MessageBox.Show(this, "Сервер не підтвердив видалення.");
            await LoadPostingsAsync();
        }
        catch (HttpRequestException) { MessageBox.Show(this, "Не вдалося з'єднатися із сервером."); }
    }
}
