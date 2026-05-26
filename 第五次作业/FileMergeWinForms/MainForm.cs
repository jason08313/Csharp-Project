namespace FileMergeWinForms;

public class MainForm : Form
{
    private readonly TextBox firstFileTextBox = new();
    private readonly TextBox secondFileTextBox = new();
    private readonly TextBox resultTextBox = new();
    private readonly Button mergeButton = new();

    public MainForm()
    {
        Text = "文件内容合并";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 320);
        Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 3,
            RowCount = 5
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        ConfigurePathTextBox(firstFileTextBox);
        ConfigurePathTextBox(secondFileTextBox);
        ConfigurePathTextBox(resultTextBox);

        var firstButton = CreateBrowseButton();
        firstButton.Click += (_, _) => SelectFile(firstFileTextBox);

        var secondButton = CreateBrowseButton();
        secondButton.Click += (_, _) => SelectFile(secondFileTextBox);

        mergeButton.Text = "合并文件";
        mergeButton.Dock = DockStyle.Left;
        mergeButton.Width = 120;
        mergeButton.Enabled = false;
        mergeButton.Click += (_, _) => MergeFiles();

        root.Controls.Add(CreateLabel("文件一"), 0, 0);
        root.Controls.Add(firstFileTextBox, 1, 0);
        root.Controls.Add(firstButton, 2, 0);
        root.Controls.Add(CreateLabel("文件二"), 0, 1);
        root.Controls.Add(secondFileTextBox, 1, 1);
        root.Controls.Add(secondButton, 2, 1);
        root.Controls.Add(mergeButton, 1, 2);
        root.Controls.Add(CreateLabel("新文件"), 0, 3);
        root.Controls.Add(resultTextBox, 1, 3);
        root.SetColumnSpan(resultTextBox, 2);

        Controls.Add(root);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Button CreateBrowseButton()
    {
        return new Button
        {
            Text = "选择文件",
            Dock = DockStyle.Fill
        };
    }

    private static void ConfigurePathTextBox(TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.ReadOnly = true;
        textBox.Margin = new Padding(0, 7, 12, 7);
    }

    private void SelectFile(TextBox target)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择要合并的文件",
            Filter = "文本文件|*.txt;*.csv;*.log;*.md|所有文件|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            target.Text = dialog.FileName;
            UpdateMergeButtonState();
        }
    }

    private void UpdateMergeButtonState()
    {
        mergeButton.Enabled = File.Exists(firstFileTextBox.Text) && File.Exists(secondFileTextBox.Text);
    }

    private void MergeFiles()
    {
        try
        {
            string firstContent = File.ReadAllText(firstFileTextBox.Text);
            string secondContent = File.ReadAllText(secondFileTextBox.Text);

            string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDirectory);

            string outputFile = Path.Combine(dataDirectory, $"Merged_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            string mergedContent = firstContent.EndsWith(Environment.NewLine)
                ? firstContent + secondContent
                : firstContent + Environment.NewLine + secondContent;

            File.WriteAllText(outputFile, mergedContent);
            resultTextBox.Text = outputFile;
            MessageBox.Show(this, "文件合并完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "合并失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
