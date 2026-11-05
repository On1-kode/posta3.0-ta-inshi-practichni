namespace PostService.App;
internal static class AppStyle
{
    public static readonly Color Accent = Color.FromArgb(216, 58, 50);
    public static readonly Color Ink = Color.FromArgb(37, 42, 49);
    public static readonly Color Muted = Color.FromArgb(112, 119, 128);
    public static readonly Color Background = Color.FromArgb(245, 245, 246);
    public static void Apply(Form form)
    {
        form.Font = new Font("Segoe UI", 10);
        form.ForeColor = Ink; form.BackColor = Background;
    }
    public static Button Button(string name, string text, bool primary = false)
    {
        var button = new Button { Name = name, Text = text, AutoSize = true, Height = 38,
            Padding = new Padding(12, 6, 12, 6), Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat, BackColor = primary ? Accent : Color.White,
            ForeColor = primary ? Color.White : Ink, Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = primary ? Accent : Color.FromArgb(215, 218, 221);
        return button;
    }
}
