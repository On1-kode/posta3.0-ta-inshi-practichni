using System.Globalization;
using System.Net;
using PostService.CommonTypes;
using PostService.Dtos;
namespace PostService.App;
public class PostingEditor : Form
{
    private readonly IPostServiceClient client;
    private readonly int? postingId;
    private readonly Dictionary<string, TextBox> inputs = new();
    private readonly Dictionary<string, Label> hints = new();
    private readonly ComboBox delivery = new() { Name = "DeliveryType", DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly Button save = new() { Name = "btnSave", Text = "Зберегти", AutoSize = true };
    public PostingEditor(IPostServiceClient client, PostingGetDto? posting, bool readOnly = false)
    {
        this.client = client; postingId = posting?.Id;
        Text = readOnly ? "Деталі відправлення" : posting is null ? "Нове відправлення" : "Редагування відправлення";
        ClientSize = new Size(820, 540); StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; errors.ContainerControl = this;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(16), AutoScroll = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        string[] fields = ["From", "To", "Content", "DeliveryType", "Weight", "Width", "Height", "Depth", "Value", "Description", "Price", "CreatedAt"];
        string[] titles = ["Відправник", "Отримувач", "Вміст", "Спосіб доставки", "Вага, кг", "Ширина, см", "Висота, см", "Глибина, см", "Цінність, грн", "Опис", "Вартість, грн", "Створено UTC"];
        delivery.Items.AddRange(["Відділення", "Курʼєр", "Експрес-курʼєр"]); delivery.SelectedIndex = 0;
        for (int i = 0; i < fields.Length; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.Controls.Add(new Label { Text = titles[i], AutoSize = true, Padding = new Padding(0, 5, 0, 0) }, 0, i);
            Control input;
            if (fields[i] == "DeliveryType") input = delivery;
            else
            {
                var box = new TextBox { Name = fields[i], Dock = DockStyle.Fill, ReadOnly = readOnly || fields[i] is "Price" or "CreatedAt" };
                inputs[fields[i]] = box; input = box;
                if (posting is not null) box.Text = Convert.ToString(typeof(PostingGetDto).GetProperty(fields[i])!.GetValue(posting), CultureInfo.CurrentCulture) ?? "";
            }
            layout.Controls.Add(input, 1, i);
            var hint = new Label { AutoSize = true, ForeColor = Color.Firebrick, MaximumSize = new Size(290, 0), Padding = new Padding(14, 3, 0, 0) };
            hints[fields[i]] = hint; layout.Controls.Add(hint, 2, i);
        }
        if (posting is not null) delivery.SelectedIndex = (int)posting.DeliveryType;
        else { inputs["Price"].Text = "Розраховує сервер"; inputs["CreatedAt"].Text = "Після збереження"; }
        delivery.Enabled = !readOnly; save.Visible = !readOnly;
        var cancel = new Button { Text = readOnly ? "Закрити" : "Скасувати", DialogResult = DialogResult.Cancel, AutoSize = true };
        var buttons = new FlowLayoutPanel { AutoSize = true }; buttons.Controls.AddRange([save, cancel]);
        layout.Controls.Add(buttons, 1, fields.Length); Controls.Add(layout); CancelButton = cancel;
        save.Click += async (_, _) => await SaveAsync();
        FormClosed += (_, _) => errors.Dispose();
    }
    private void ShowErrors(Dictionary<string, string[]> messages)
    {
        foreach (var pair in messages)
        {
            Control? input = pair.Key == "DeliveryType" ? delivery : inputs.GetValueOrDefault(pair.Key);
            if (input is null) continue;
            string text = string.Join(" ", pair.Value); errors.SetError(input, text);
            input.BackColor = Color.MistyRose; hints[pair.Key].Text = "! " + text;
        }
    }
    private async Task SaveAsync()
    {
        errors.Clear(); foreach (var h in hints.Values) h.Text = "";
        foreach (var b in inputs.Values) b.BackColor = b.ReadOnly ? SystemColors.Control : SystemColors.Window;
        var local = new Dictionary<string, string[]>();
        float Number(string field, bool optional = false)
        {
            string text = inputs[field].Text.Trim();
            if (optional && text == "") return 0;
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var n) && float.IsFinite(n)) return n;
            local[field] = ["Введіть коректне число."]; return 0;
        }
        var dto = new PostingPutDto
        {
            From = inputs["From"].Text, To = inputs["To"].Text, Content = inputs["Content"].Text,
            DeliveryType = (DeliveryType)delivery.SelectedIndex, Weight = Number("Weight"), Width = Number("Width"),
            Height = Number("Height"), Depth = Number("Depth"), Value = inputs["Value"].Text.Trim() == "" ? null : Number("Value", true),
            Description = inputs["Description"].Text
        };
        if (local.Count > 0) { ShowErrors(local); return; }
        save.Enabled = false;
        try
        {
            while (true)
            {
                try
                {
                    if (postingId.HasValue) await client.UpdatePostingAsync(postingId.Value, dto);
                    else await client.CreatePostingAsync(dto);
                    DialogResult = DialogResult.OK; return;
                }
                catch (HttpRequestException e) when (e.StatusCode is null)
                {
                    if (MessageBox.Show(this, "Сервер недоступний. Повторити спробу?", "З’єднання", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) != DialogResult.Retry) return;
                }
                catch (PostingValidationException e) { ShowErrors(e.Errors); return; }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                { MessageBox.Show(this, "Відправлення вже видалено на сервері."); DialogResult = DialogResult.Cancel; return; }
            }
        }
        finally { save.Enabled = true; }
    }
}
