using System.Net;
using PostService.Contracts;

namespace PostService.WindowsApp;

public class PostingEditor : Form
{
    private readonly IPostServiceClient client;
    private readonly int? postingId;
    private readonly TextBox track = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown weight = new() { DecimalPlaces = 2, Maximum = 100000, Dock = DockStyle.Fill };
    private readonly TextBox sender = new() { Dock = DockStyle.Fill };
    private readonly TextBox recipient = new() { Dock = DockStyle.Fill };
    private readonly TextBox status = new() { Dock = DockStyle.Fill };
    private readonly Button save = new() { Text = "Зберегти", AutoSize = true };

    public PostingEditor(IPostServiceClient client, PostingGetDto? posting)
    {
        this.client = client;
        postingId = posting?.Id;
        Text = posting is null ? "Нове відправлення" : "Редагування відправлення";
        ClientSize = new Size(520, 260);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        string[] labels = ["Трек-номер", "Вага, кг", "Адреса відправника", "Адреса отримувача", "Статус доставки"];
        Control[] inputs = [track, weight, sender, recipient, status];
        for (int i = 0; i < inputs.Length; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.Controls.Add(new Label { Text = labels[i], AutoSize = true }, 0, i);
            layout.Controls.Add(inputs[i], 1, i);
        }
        layout.Controls.Add(save, 1, 5);
        Controls.Add(layout);
        if (posting is not null)
        {
            track.Text = posting.TrackNumber;
            weight.Value = (decimal)posting.Weight;
            sender.Text = posting.SenderAddress;
            recipient.Text = posting.RecipientAddress;
            status.Text = posting.Status;
        }
        save.Click += async (_, _) => await SaveAsync();
    }

    private async Task SaveAsync()
    {
        save.Enabled = false;
        try
        {
            while (true)
            {
                try
                {
                    var dto = new PostingPutDto
                    {
                        TrackNumber = track.Text, Weight = (double)weight.Value,
                        SenderAddress = sender.Text, RecipientAddress = recipient.Text, Status = status.Text
                    };
                    if (postingId.HasValue) await client.UpdatePostingAsync(postingId.Value, dto);
                    else await client.CreatePostingAsync(dto);
                    DialogResult = DialogResult.OK;
                    return;
                }
                catch (HttpRequestException e) when (e.StatusCode is null)
                {
                    if (MessageBox.Show(this, "Сервер недоступний. Повторити спробу?",
                        "З’єднання", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning)
                        != DialogResult.Retry) return;
                }
                catch (PostingValidationException e)
                {
                    MessageBox.Show(this, string.Join(Environment.NewLine, e.Errors.Values.SelectMany(x => x)),
                        "Перевірте дані", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    MessageBox.Show(this, "Відправлення вже видалено на сервері.", "Запис відсутній");
                    DialogResult = DialogResult.Cancel;
                    return;
                }
            }
        }
        finally
        {
            save.Enabled = true;
        }
    }
}
