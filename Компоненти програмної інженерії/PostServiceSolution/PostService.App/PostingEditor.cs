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
    private readonly Button save;
    public PostingEditor(IPostServiceClient client, PostingGetDto? posting, bool readOnly = false)
    {
        this.client = client; postingId = posting?.Id;
        Text = readOnly ? "Деталі відправлення" : posting is null ? "Нове відправлення" : "Редагування відправлення";
        AppStyle.Apply(this);
        ClientSize = new Size(1180, 780); MinimumSize = new Size(1000, 660); StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; errors.ContainerControl = this;
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(24) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68)); shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        var heading = new Label { Text = Text, Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, ForeColor = AppStyle.Ink };
        shell.Controls.Add(heading, 0, 0);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var cards = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 0, 18, 0) };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        delivery.Items.AddRange(["Відділення", "Курʼєр", "Експрес-курʼєр"]); delivery.SelectedIndex = 0;
        Control Field(string field, string title)
        {
            var cell = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 0, 12, 4), Margin = new Padding(0) };
            cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58)); cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            var label = new Label { Text = title, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            cell.Controls.Add(label, 0, 0); cell.SetColumnSpan(label, 2);
            Control input;
            if (field == "DeliveryType") input = delivery;
            else
            {
                var box = new TextBox { Name = field, Dock = DockStyle.Fill, ReadOnly = readOnly, Margin = new Padding(0, 0, 14, 0) };
                inputs[field] = box; input = box;
                if (posting is not null) box.Text = Convert.ToString(typeof(PostingGetDto).GetProperty(field)!.GetValue(posting), CultureInfo.CurrentCulture) ?? "";
            }
            cell.Controls.Add(input, 0, 1);
            var hint = new Label { AutoSize = true, ForeColor = Color.Firebrick, Dock = DockStyle.Fill, Margin = new Padding(8, 3, 0, 0), MaximumSize = new Size(210, 0) };
            hints[field] = hint; cell.Controls.Add(hint, 1, 1); return cell;
        }
        Control InlineField(string field, string title)
        {
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Padding = new Padding(0, 2, 12, 4), Margin = new Padding(0) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            row.Controls.Add(new Label { Text = title, AutoSize = true, Padding = new Padding(0, 3, 0, 0) }, 0, 0);
            var box = new TextBox { Name = field, Dock = DockStyle.Fill, ReadOnly = readOnly };
            if (posting is not null) box.Text = Convert.ToString(typeof(PostingGetDto).GetProperty(field)!.GetValue(posting), CultureInfo.CurrentCulture) ?? "";
            inputs[field] = box; row.Controls.Add(box, 1, 0);
            var hint = new Label { AutoSize = true, ForeColor = Color.Firebrick, Dock = DockStyle.Fill, MaximumSize = new Size(450, 0), Padding = new Padding(15, 3, 0, 0) };
            hints[field] = hint; row.Controls.Add(hint, 2, 0); return row;
        }
        void Group(int column, int row, string title, (string Field, string Title)[] fields, bool wide = false)
        {
            var card = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, BackColor = Color.White, Padding = new Padding(14), Margin = new Padding(0, 0, column == 0 && !wide ? 16 : 0, 18), ColumnCount = wide ? 2 : 1 };
            if (wide) { card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); }
            var label = new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = AppStyle.Accent, Margin = new Padding(0, 0, 0, 10) };
            card.Controls.Add(label, 0, 0); if (wide) card.SetColumnSpan(label, 2);
            if (!wide)
                for (int i = 0; i < fields.Length; i++) card.Controls.Add(Field(fields[i].Field, fields[i].Title), 0, i + 1);
            else
            {
                for (int i = 0; i < 4; i++) card.Controls.Add(Field(fields[i].Field, fields[i].Title), i % 2, i / 2 + 1);
                int nextRow = 3;
                foreach (var field in new[] { ("Depth", "Довжина, см *"), ("Width", "Ширина, см *"), ("Height", "Висота, см *"), ("Description", "Коментар") })
                {
                    var input = InlineField(field.Item1, field.Item2); card.Controls.Add(input, 0, nextRow++); card.SetColumnSpan(input, 2);
                }
            }
            cards.Controls.Add(card, column, row); if (wide) cards.SetColumnSpan(card, 2);
        }
        Group(0, 0, "01  Відправник", [("SenderName", "ПІБ *"), ("SenderPhone", "Телефон *"), ("From", "Місто *"), ("SenderAddress", "Адреса або відділення *")]);
        Group(1, 0, "02  Отримувач", [("RecipientName", "ПІБ *"), ("RecipientPhone", "Телефон *"), ("To", "Місто *"), ("RecipientAddress", "Адреса або відділення *")]);
        Group(0, 1, "03  Посилка та доставка", [("Content", "Вміст посилки *"), ("DeliveryType", "Спосіб доставки"), ("Weight", "Вага, кг *"), ("Value", "Оголошена цінність, грн"), ("Width", "Ширина, см *"), ("Height", "Висота, см *"), ("Depth", "Глибина, см *"), ("Description", "Коментар до відправлення")], true);
        scroll.Controls.Add(cards); shell.Controls.Add(scroll, 0, 1);
        if (posting is not null) delivery.SelectedIndex = (int)posting.DeliveryType;
        delivery.Enabled = !readOnly;
        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 14, 0, 0) };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var summary = new Label { AutoSize = true, Text = posting is null ? "Вартість і дата — після збереження за навчальними тарифами." : $"Вартість: {posting.Price:0.00} грн  ·  Створено: {posting.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}", ForeColor = AppStyle.Muted, MaximumSize = new Size(640, 0) };
        bottom.Controls.Add(summary, 0, 0);
        save = AppStyle.Button("btnSave", posting is null ? "Створити відправлення" : "Зберегти зміни", true); save.Visible = !readOnly;
        var cancel = AppStyle.Button("btnCancel", readOnly ? "Закрити" : "Скасувати"); cancel.DialogResult = DialogResult.Cancel;
        var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false }; buttons.Controls.AddRange([cancel, save]); bottom.Controls.Add(buttons, 1, 0);
        shell.Controls.Add(bottom, 0, 2); Controls.Add(shell); CancelButton = cancel;
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
            SenderName = inputs["SenderName"].Text, SenderPhone = inputs["SenderPhone"].Text, SenderAddress = inputs["SenderAddress"].Text,
            RecipientName = inputs["RecipientName"].Text, RecipientPhone = inputs["RecipientPhone"].Text, RecipientAddress = inputs["RecipientAddress"].Text,
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
