using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Reflection;

namespace StealthReader
{
    public class TextSurface : Control
    {
        private List<string> lines = new List<string>();
        public List<int> lineSourceIdx = new List<int>();
        private int lineHeight = 18;
        private int pad = 10;
        public string Content;
        public Font TextFont;
        public Color TextColor;
        public int ScrollY;
        private Dictionary<char, float> charWidthCache = new Dictionary<char, float>();
        private Font cacheFontRef;

        public int MaxScroll
        {
            get { int total = lines.Count * lineHeight; return Math.Max(0, total - this.Height); }
        }

        public TextSurface()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.BackColor = Color.Magenta;
            TextFont = new Font("Microsoft YaHei", 12);
            TextColor = Color.White;
            cacheFontRef = TextFont;
        }

        public void Rebuild()
        {
            lines.Clear();
            lineSourceIdx.Clear();
            if (!object.ReferenceEquals(cacheFontRef, TextFont))
            {
                charWidthCache.Clear();
                cacheFontRef = TextFont;
            }
            int maxW = Math.Max(20, this.Width - 2 * pad);
            if (string.IsNullOrEmpty(Content)) { ClampScroll(); Invalidate(); return; }
            string[] paras = Content.Replace("\r", "").Split('\n');
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                lineHeight = (int)Math.Ceiling(g.MeasureString("汉", TextFont).Height);
                for (int srcIdx = 0; srcIdx < paras.Length; srcIdx++)
                {
                    string para = paras[srcIdx];
                    StringBuilder cur = new StringBuilder();
                    float width = 0;
                    foreach (char c in para)
                    {
                        float cw;
                        if (!charWidthCache.TryGetValue(c, out cw))
                        {
                            cw = g.MeasureString(c.ToString(), TextFont).Width;
                            charWidthCache[c] = cw;
                        }
                        if (width + cw > maxW && cur.Length > 0)
                        {
                            lines.Add(cur.ToString());
                            lineSourceIdx.Add(srcIdx);
                            cur.Length = 0;
                            width = 0;
                        }
                        cur.Append(c);
                        width += cw;
                    }
                    if (cur.Length > 0) { lines.Add(cur.ToString()); lineSourceIdx.Add(srcIdx); }
                    lines.Add("");
                    lineSourceIdx.Add(srcIdx);
                }
            }
            ClampScroll();
            Invalidate();
        }

        public void ClampScroll()
        {
            if (ScrollY > MaxScroll) ScrollY = MaxScroll;
            if (ScrollY < 0) ScrollY = 0;
        }

        public void ScrollLines(int delta)
        {
            ScrollY = Math.Max(0, Math.Min(MaxScroll, ScrollY + delta * lineHeight));
            Invalidate();
        }

        public void ScrollPixel(int dy)
        {
            ScrollY = Math.Max(0, Math.Min(MaxScroll, ScrollY + dy));
            Invalidate();
        }

        public void ScrollToSourceLine(int srcLine)
        {
            int targetDisplay = 0;
            for (int i = 0; i < lineSourceIdx.Count; i++)
            {
                if (lineSourceIdx[i] >= srcLine) { targetDisplay = i; break; }
            }
            ScrollY = targetDisplay * lineHeight;
            ClampScroll();
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Magenta);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Magenta);
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            if (lines.Count == 0 || lineHeight <= 0) return;
            int first = Math.Max(0, ScrollY / lineHeight - 1);
            int last = Math.Min(lines.Count, (ScrollY + this.Height) / lineHeight + 2);
            Color fill = TextColor;
            using (SolidBrush fb = new SolidBrush(fill))
            {
                for (int i = first; i < last; i++)
                {
                    float y = i * lineHeight - ScrollY;
                    if (y > this.Height) break;
                    if (y < -lineHeight) continue;
                    string line = lines[i];
                    if (line.Length == 0) continue;
                    g.DrawString(line, TextFont, fb, pad, y);
                }
            }
            int ms = MaxScroll;
            int pct = ms <= 0 ? 0 : (int)Math.Round((double)ScrollY / ms * 100);
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;
            using (Font pf = new Font("Microsoft YaHei", 9))
            using (SolidBrush pb = new SolidBrush(fill))
            {
                string s = pct + "%";
                SizeF sz = g.MeasureString(s, pf);
                g.DrawString(s, pf, pb, this.Width - sz.Width - 18, this.Height - sz.Height - 16);
            }
        }
    }

    public class HelpForm : Form
    {
        public HelpForm(string[] helpLines, bool chinese)
        {
            this.Text = chinese ? "帮助" : "Help";
            this.Width = 350; this.Height = 480;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            Label lb = new Label();
            lb.Text = string.Join("\n", helpLines);
            lb.Font = new Font("Microsoft YaHei", 10);
            lb.AutoSize = false;
            lb.Size = new Size(this.Width - 40, this.Height - 110);
            lb.Location = new Point(20, 10);
            lb.ForeColor = Color.FromArgb(40, 40, 40);
            this.Controls.Add(lb);
            Button close = new Button();
            close.Text = chinese ? "关闭" : "Close";
            close.Size = new Size(90, 30);
            close.Location = new Point((this.Width - 90) / 2, this.Height - 65);
            close.FlatStyle = FlatStyle.Flat;
            close.Click += (s, e) => this.Close();
            this.Controls.Add(close);
        }
    }

    public class ChapterForm : Form
    {
        public int SelectedLine = -1;
        public ChapterForm(List<string> titles, List<int> lines, bool chinese)
        {
            this.Text = chinese ? "章节列表" : "Chapters";
            this.Width = 320; this.Height = 420;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            ListBox lb = new ListBox();
            lb.Font = new Font("Microsoft YaHei", 10);
            lb.Dock = DockStyle.Fill;
            lb.Items.AddRange(titles.ToArray());
            lb.DoubleClick += (s, e) =>
            {
                if (lb.SelectedIndex >= 0)
                {
                    SelectedLine = lines[lb.SelectedIndex];
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };
            this.Controls.Add(lb);
            Button cancel = new Button();
            cancel.Text = chinese ? "取消" : "Cancel";
            cancel.Size = new Size(90, 28);
            cancel.Dock = DockStyle.Bottom;
            cancel.FlatStyle = FlatStyle.Flat;
            cancel.Click += (s, e) => this.Close();
            this.Controls.Add(cancel);
        }
    }

    public class FeedbackForm : Form
    {
        public FeedbackForm(string email, bool chinese)
        {
            this.Text = chinese ? "意见反馈" : "Feedback";
            this.Width = 360; this.Height = 200;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            Label tip = new Label();
            tip.Text = chinese ? "遇到问题或有建议？欢迎发邮件告诉我：" : "Found a bug or have a suggestion? Email me:";
            tip.Font = new Font("Microsoft YaHei", 10);
            tip.AutoSize = true;
            tip.ForeColor = Color.FromArgb(60, 60, 60);
            tip.Location = new Point(25, 20);
            this.Controls.Add(tip);
            Label emailLabel = new Label();
            emailLabel.Text = email;
            emailLabel.Font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
            emailLabel.AutoSize = true;
            emailLabel.ForeColor = Color.FromArgb(20, 100, 180);
            emailLabel.Location = new Point(25, 55);
            this.Controls.Add(emailLabel);
            Button copyBtn = new Button();
            copyBtn.Text = chinese ? "复制邮箱" : "Copy Email";
            copyBtn.Size = new Size(110, 32);
            copyBtn.Location = new Point(25, 110);
            copyBtn.FlatStyle = FlatStyle.Flat;
            copyBtn.Font = new Font("Microsoft YaHei", 10);
            copyBtn.Click += (s, e) =>
            {
                try { Clipboard.SetText(email); MessageBox.Show(chinese ? "邮箱已复制，去邮件里粘贴发送吧" : "Email copied. Paste it in your mail app.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); }
                catch { MessageBox.Show(chinese ? "复制失败，请手动选择邮箱复制" : "Copy failed, please select the email manually.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            this.Controls.Add(copyBtn);
            Button closeBtn = new Button();
            closeBtn.Text = chinese ? "关闭" : "Close";
            closeBtn.Size = new Size(90, 32);
            closeBtn.Location = new Point(160, 110);
            closeBtn.FlatStyle = FlatStyle.Flat;
            closeBtn.Font = new Font("Microsoft YaHei", 10);
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);
        }
    }

    public class DonateForm : Form
    {
        public DonateForm(Image wechat, Image alipay, bool chinese)
        {
            this.Text = chinese ? "打赏支持" : "Support / Donate";
            this.Width = 380; this.Height = 420;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            Label title = new Label();
            title.Text = chinese ? "如果这个小工具帮到了你，欢迎扫码支持 ❤" : "If this tool helped you, scan to support ❤";
            title.Font = new Font("Microsoft YaHei", 11);
            title.AutoSize = true;
            title.ForeColor = Color.FromArgb(220, 80, 60);
            title.Location = new Point(20, 15);
            this.Controls.Add(title);
            int y = 55;
            if (wechat != null) AddQr(wechat, chinese ? "微信打赏" : "WeChat", 20, y);
            if (alipay != null) AddQr(alipay, chinese ? "支付宝打赏" : "Alipay", 200, y);
            Button close = new Button();
            close.Text = chinese ? "关闭" : "Close";
            close.Size = new Size(80, 28);
            close.Location = new Point(150, this.Height - 70);
            close.FlatStyle = FlatStyle.Flat;
            close.Click += (s, e) => this.Close();
            this.Controls.Add(close);
        }
        private void AddQr(Image img, string label, int x, int y)
        {
            Label lb = new Label();
            lb.Text = label;
            lb.Font = new Font("Microsoft YaHei", 9);
            lb.AutoSize = true;
            lb.Location = new Point(x, y);
            this.Controls.Add(lb);
            PictureBox pic = new PictureBox();
            pic.Image = img;
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Size = new Size(160, 220);
            pic.Location = new Point(x, y + 25);
            this.Controls.Add(pic);
        }
    }

    public class ReaderForm : Form
    {
        private TextSurface textBox;
        private Panel topBar;
        private Button btnOpen, btnLang, btnHelp, btnColor, btnChapter, btnDonate, btnFeedback, btnHide;
        private Panel resizeGrip;
        private bool dragging = false, resizing = false;
        private Point dragPoint, resizeStart;
        private Size startSize;
        private bool autoScroll = false;
        private Timer scrollTimer, resizeTimer;
        private bool showTopBar = true;

        [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr h, int id, uint mod, uint vk);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr h, int id);
        const int HOTKEY_TILDE = 9000, HOTKEY_F1 = 9001;
        const uint MOD_NONE = 0;
        const int VK_OEM_3 = 0xC0, VK_F1 = 0x70;

        private string configPath;
        private int fontSize = 12;
        private bool isChinese = true;
        private bool isWhiteText = true;
        private List<string> chapterTitles = new List<string>();
        private List<int> chapterLines = new List<int>();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new ReaderForm());
        }

        public ReaderForm()
        {
            configPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "reader_config.txt");
            LoadConfig();
            this.Text = "隐阅";
            this.Width = 400; this.Height = 500;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(1000, 100);
            this.MinimumSize = new Size(300, 200);
            if (configWinX != 0 || configWinY != 0) this.Location = new Point(configWinX, configWinY);
            if (configWinW >= 300) this.Width = configWinW;
            if (configWinH >= 200) this.Height = configWinH;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            textBox = new TextSurface();
            textBox.TextFont = new Font("Microsoft YaHei", fontSize);
            textBox.TextColor = isWhiteText ? Color.White : Color.Black;
            textBox.Location = new Point(0, 30);
            textBox.Size = new Size(this.Width, this.Height - 30);
            textBox.DoubleClick += (s, e) => OpenTxt();
            textBox.TabStop = true;
            textBox.Cursor = Cursors.IBeam;

            topBar = new Panel();
            topBar.BackColor = Color.FromArgb(80, 80, 80);
            topBar.Location = new Point(0, 0);
            topBar.Size = new Size(this.Width, 30);
            topBar.MouseDown += TopBar_MouseDown;
            topBar.MouseMove += TopBar_MouseMove;
            topBar.MouseUp += TopBar_MouseUp;
            topBar.MouseDoubleClick += TopBar_MouseDoubleClick;

            btnOpen = MakeBtn(Translate("Open"), 5, 4, 50); btnOpen.Click += (s, e) => OpenTxt();
            btnLang = MakeBtn(isChinese ? "EN" : "中", 60, 4, 34); btnLang.Click += BtnLang_Click;
            btnHelp = MakeBtn("?", 98, 4, 24); btnHelp.Click += BtnHelp_Click;
            btnColor = MakeBtn(isChinese ? "黑白" : "B/W", 126, 4, 42); btnColor.Click += BtnColor_Click;
            btnChapter = MakeBtn(isChinese ? "章节" : "Ch", 172, 4, 46); btnChapter.Click += BtnChapter_Click;
            btnDonate = MakeBtn(isChinese ? "打赏" : "Tip", 222, 4, 48); btnDonate.Click += (s, e) => ShowDonate();
            btnFeedback = MakeBtn(isChinese ? "反馈" : "Fb", 275, 4, 56); btnFeedback.Click += (s, e) => OpenFeedbackMail();
            btnHide = MakeBtn(Translate("Hide"), this.Width - 60, 4, 55); btnHide.Click += (s, e) => this.Hide();

            topBar.Controls.Add(btnOpen);
            topBar.Controls.Add(btnLang);
            topBar.Controls.Add(btnHelp);
            topBar.Controls.Add(btnColor);
            topBar.Controls.Add(btnChapter);
            topBar.Controls.Add(btnDonate);
            topBar.Controls.Add(btnFeedback);
            topBar.Controls.Add(btnHide);
            this.Controls.Add(textBox);
            this.Controls.Add(topBar);

            resizeGrip = new Panel();
            resizeGrip.BackColor = Color.FromArgb(220, 220, 220);
            resizeGrip.Size = new Size(16, 16);
            resizeGrip.Location = new Point(this.Width - 16, this.Height - 16);
            resizeGrip.Cursor = Cursors.SizeNWSE;
            resizeGrip.MouseDown += ResizeGrip_MouseDown;
            resizeGrip.MouseMove += ResizeGrip_MouseMove;
            resizeGrip.MouseUp += ResizeGrip_MouseUp;
            this.Controls.Add(resizeGrip);
            resizeGrip.BringToFront();

            scrollTimer = new Timer();
            scrollTimer.Interval = 50;
            scrollTimer.Tick += (s, e) =>
            {
                if (textBox.ScrollY >= textBox.MaxScroll) { scrollTimer.Stop(); autoScroll = false; return; }
                textBox.ScrollPixel(1);
            };
            resizeTimer = new Timer();
            resizeTimer.Interval = 350;
            resizeTimer.Tick += (s, e) => { resizeTimer.Stop(); textBox.Rebuild(); textBox.Invalidate(); };

            this.MouseWheel += OnMouseWheel;
            this.HandleCreated += (s, e) => { RegisterHotKey(this.Handle, HOTKEY_TILDE, MOD_NONE, VK_OEM_3); RegisterHotKey(this.Handle, HOTKEY_F1, MOD_NONE, VK_F1); };
            this.HandleDestroyed += (s, e) => { UnregisterHotKey(this.Handle, HOTKEY_TILDE); UnregisterHotKey(this.Handle, HOTKEY_F1); };
            this.KeyPreview = true;
            this.KeyDown += OnKeyDown;
            this.Activated += (s, e) => { if (textBox != null) textBox.Focus(); };
            this.Resize += OnResize;
            this.FormClosing += OnClosing;

            SetHelpContent();

            if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
            {
                string c = ReadTxtAuto(configFile);
                if (c != null)
                {
                    LoadText(c);
                    if (configPos >= 0) { textBox.ScrollY = Math.Min(configPos, textBox.MaxScroll); textBox.Invalidate(); }
                }
            }
        }

        private Button MakeBtn(string text, int x, int y, int w)
        {
            Button b = new Button();
            b.Text = text; b.Size = new Size(w, 22);
            b.Location = new Point(x, y);
            b.FlatStyle = FlatStyle.Flat;
            b.ForeColor = Color.White;
            b.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
            return b;
        }

        private string Translate(string key)
        {
            if (key == "Open") return isChinese ? "打开" : "Open";
            if (key == "Hide") return isChinese ? "隐藏" : "Hide";
            return key;
        }

        private void LoadText(string txt)
        {
            textBox.Content = txt;
            textBox.Rebuild();
            textBox.ScrollY = 0;
            BuildChapters();
            textBox.Invalidate();
        }

        private void SetHelpContent()
        {
            LoadText(string.Join("\n", isChinese ? helpZh : helpEn));
        }

        private void BuildChapters()
        {
            chapterTitles.Clear();
            chapterLines.Clear();
            if (string.IsNullOrEmpty(textBox.Content)) return;
            Regex re = new Regex("^\\s*(第[0-9一二三四五六七八九十百千万零〇两]+[章节卷回部篇][^\\s]{0,20})");
            string[] all = textBox.Content.Replace("\r", "").Split('\n');
            int lineNo = 0;
            foreach (string para in all)
            {
                Match m = re.Match(para);
                if (m.Success) { chapterTitles.Add(m.Groups[1].Value); chapterLines.Add(lineNo); }
                lineNo++;
            }
        }

        private void BtnChapter_Click(object sender, EventArgs e)
        {
            if (chapterLines.Count == 0) { MessageBox.Show(isChinese ? "未识别到章节" : "No chapters found"); return; }
            ChapterForm cf = new ChapterForm(chapterTitles, chapterLines, isChinese);
            if (cf.ShowDialog() == DialogResult.OK && cf.SelectedLine >= 0)
            {
                textBox.ScrollToSourceLine(cf.SelectedLine);
            }
        }

        private void BtnLang_Click(object sender, EventArgs e)
        {
            isChinese = !isChinese;
            btnLang.Text = isChinese ? "EN" : "中";
            btnOpen.Text = Translate("Open");
            btnHide.Text = Translate("Hide");
            btnColor.Text = isChinese ? "黑白" : "B/W";
            btnChapter.Text = isChinese ? "章节" : "Ch";
            btnDonate.Text = isChinese ? "打赏" : "Tip";
            btnFeedback.Text = isChinese ? "反馈" : "Fb";
            SetHelpContent();
        }

        private void BtnColor_Click(object sender, EventArgs e)
        {
            isWhiteText = !isWhiteText;
            textBox.TextColor = isWhiteText ? Color.White : Color.Black;
            textBox.Invalidate();
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            new HelpForm(isChinese ? helpZh : helpEn, isChinese).ShowDialog();
        }

        private void OpenTxt()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = isChinese ? "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*" : "Text (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string c = ReadTxtAuto(dlg.FileName);
                if (c != null) { configFile = dlg.FileName; LoadText(c); }
                else MessageBox.Show(isChinese ? "无法读取该文件，编码可能不受支持" : "Cannot read file");
            }
        }

        private void ShowDonate()
        {
            Image wechat = LoadResImage("wechat_qr");
            Image alipay = LoadResImage("alipay_qr");
            if (wechat == null && alipay == null) { MessageBox.Show(isChinese ? "未找到内置收款码" : "QR not embedded"); return; }
            DonateForm df = new DonateForm(wechat, alipay, isChinese);
            df.ShowDialog();
            if (wechat != null) wechat.Dispose();
            if (alipay != null) alipay.Dispose();
        }

        private const string FeedbackEmail = "1051783236@qq.com";
        private void OpenFeedbackMail()
        {
            new FeedbackForm(FeedbackEmail, isChinese).ShowDialog();
        }

        private Image LoadResImage(string name)
        {
            try
            {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                { if (s == null) return null; return new Bitmap(s); }
            }
            catch { return null; }
        }

        private string ReadTxtAuto(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            try { return new UTF8Encoding(false, true).GetString(bytes); }
            catch
            {
                try { return Encoding.GetEncoding("GBK").GetString(bytes); }
                catch { try { return Encoding.GetEncoding("GB2312").GetString(bytes); } catch { return null; } }
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
                int nf = Math.Max(9, Math.Min(28, fontSize + (e.Delta > 0 ? 1 : -1)));
                if (nf != fontSize) { fontSize = nf; ChangeFont(); }
            }
            else if (e.Delta != 0)
            {
                int delta = e.Delta / 120;
                textBox.ScrollLines(-delta * 4);
            }
        }

        private void ChangeFont()
        {
            Font old = textBox.TextFont;
            textBox.TextFont = new Font("Microsoft YaHei", fontSize);
            if (old != null) old.Dispose();
            textBox.Rebuild();
            textBox.Invalidate();
        }

        private void OnResize(object sender, EventArgs e)
        {
            topBar.Width = this.Width;
            btnHide.Location = new Point(this.Width - 60, 4);
            if (showTopBar) { textBox.Width = this.Width; textBox.Height = this.Height - 30; }
            else { textBox.Width = this.Width; textBox.Height = this.Height; }
            resizeGrip.Location = new Point(this.Width - 16, this.Height - 16);
            textBox.Invalidate();
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void TopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { dragging = true; dragPoint = new Point(e.X, e.Y); }
        }
        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging) { this.Left += e.X - dragPoint.X; this.Top += e.Y - dragPoint.Y; }
        }
        private void TopBar_MouseUp(object sender, MouseEventArgs e) { dragging = false; }
        private void TopBar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.X > 280 && e.X < this.Width - 70) ShowDonate();
        }

        private void ResizeGrip_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { resizing = true; resizeStart = new Point(e.X, e.Y); startSize = new Size(this.Width, this.Height); resizeGrip.Capture = true; }
        }
        private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
        {
            if (resizing) { this.Width = Math.Max(300, startSize.Width + e.X - resizeStart.X); this.Height = Math.Max(200, startSize.Height + e.Y - resizeStart.Y); }
        }
        private void ResizeGrip_MouseUp(object sender, MouseEventArgs e) { resizing = false; resizeGrip.Capture = false; }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                autoScroll = !autoScroll;
                if (autoScroll) scrollTimer.Start(); else scrollTimer.Stop();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)) { fontSize = Math.Min(28, fontSize + 1); ChangeFont(); }
            else if (e.Control && (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)) { fontSize = Math.Max(9, fontSize - 1); ChangeFont(); }
            else if (e.Control && e.KeyCode == Keys.T) { BtnChapter_Click(null, EventArgs.Empty); e.SuppressKeyPress = true; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Down) { textBox.ScrollLines(3); return true; }
            if (key == Keys.Up) { textBox.ScrollLines(-3); return true; }
            if (key == Keys.PageDown) { textBox.ScrollPixel(textBox.Height); return true; }
            if (key == Keys.PageUp) { textBox.ScrollPixel(-textBox.Height); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0084)
            {
                int lp = m.LParam.ToInt32();
                short x = (short)(lp & 0xFFFF), y = (short)((lp >> 16) & 0xFFFF);
                Point c = this.PointToClient(new Point(x, y));
                int g2 = 8, w = this.Width, h = this.Height;
                bool l = c.X <= g2, r = c.X >= w - g2, t = c.Y <= g2, b = c.Y >= h - g2;
                int hit = 0;
                if (l && t) hit = 13; else if (r && t) hit = 14; else if (l && b) hit = 16;
                else if (r && b) hit = 17; else if (l) hit = 10; else if (r) hit = 11;
                else if (t) hit = 12; else if (b) hit = 15;
                if (hit != 0) { m.Result = (IntPtr)hit; return; }
                base.WndProc(ref m);
                return;
            }
            base.WndProc(ref m);
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                if (id == HOTKEY_TILDE) { if (this.Visible) this.Hide(); else { this.Show(); this.Activate(); } }
                else if (id == HOTKEY_F1)
                {
                    showTopBar = !showTopBar;
                    topBar.Visible = showTopBar;
                    if (showTopBar) { textBox.Location = new Point(0, 30); textBox.Height = this.Height - 30; }
                    else { textBox.Location = new Point(0, 0); textBox.Height = this.Height; }
                    textBox.Invalidate();
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Magenta);
        }

        private int configWinX = 0, configWinY = 0, configWinW = 0, configWinH = 0, configPos = -1;
        private string configFile = "";
        private void LoadConfig()
        {
            if (!File.Exists(configPath)) return;
            try
            {
                foreach (string line in File.ReadAllLines(configPath, Encoding.UTF8))
                {
                    int i = line.IndexOf('=');
                    if (i < 0) continue;
                    string k = line.Substring(0, i).Trim(), v = line.Substring(i + 1).Trim();
                    switch (k)
                    {
                        case "winX": int.TryParse(v, out configWinX); break;
                        case "winY": int.TryParse(v, out configWinY); break;
                        case "winW": int.TryParse(v, out configWinW); break;
                        case "winH": int.TryParse(v, out configWinH); break;
                        case "file": configFile = v; break;
                        case "pos": int.TryParse(v, out configPos); break;
                        case "font": int.TryParse(v, out fontSize); break;
                        case "lang": int l = 0; int.TryParse(v, out l); isChinese = l == 1; break;
                        case "white": int w = 1; int.TryParse(v, out w); isWhiteText = w == 1; break;
                    }
                }
            }
            catch { }
        }
        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("winX=" + this.Left);
                sb.AppendLine("winY=" + this.Top);
                sb.AppendLine("winW=" + this.Width);
                sb.AppendLine("winH=" + this.Height);
                sb.AppendLine("file=" + configFile);
                sb.AppendLine("pos=" + textBox.ScrollY);
                sb.AppendLine("font=" + fontSize);
                sb.AppendLine("lang=" + (isChinese ? 1 : 0));
                sb.AppendLine("white=" + (isWhiteText ? 1 : 0));
                File.WriteAllText(configPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
            scrollTimer.Stop();
            resizeTimer.Stop();
        }

        private string[] helpZh = {
            "  隐阅 · 透明悬浮阅读器",
            "",
            "  ── 快捷键 ──",
            "  ~           隐藏 / 显示窗口(全局)",
            "  F1          显示/隐藏顶栏",
            "  空格        开始/停止自动滚动",
            "  上/下箭头    翻页",
            "  PageUp/Dn   整页翻页",
            "  Ctrl + / -  调字号",
            "  Ctrl + T    章节列表",
            "  滚轮        翻页",
            "  Ctrl+滚轮   调字号",
            "  双击正文    打开TXT",
            "",
            "  ── 顶栏按钮 ──",
            "  打开 / 中英 / ? 帮助",
            "  黑白 / 章节 / 打赏 / 反馈 / 隐藏",
            "",
            "  ── 说明 ──",
            "  · 纯白/纯黑字，最隐蔽，按黑白键切换",
            "  · 进度百分比在右下角",
            "  · 反馈邮箱：1051783236@qq.com",
            "",
            "  喜欢的话，欢迎扫码支持 ❤"
        };
        private string[] helpEn = {
            "  YinYue · Stealth Floating Reader",
            "",
            "  ── Shortcuts ──",
            "  ~           Hide/Show (global)",
            "  F1          Show/Hide toolbar",
            "  Space       Start/Stop auto scroll",
            "  Up/Down     Page up/down",
            "  PageUp/Dn   Full page",
            "  Ctrl + / -  Font size",
            "  Ctrl + T    Chapters",
            "  Wheel       Page scroll",
            "  Ctrl+wheel  Font size",
            "  Dbl-click   Open TXT",
            "",
            "  ── Toolbar ──",
            "  Open / 中EN / ? Help",
            "  B&W / Ch / Tip / Fb / Hide",
            "",
            "  ── Note ──",
            "  · Pure white/black text, most stealthy",
            "  · Toggle B/W button to switch color",
            "  · Progress % at bottom-right",
            "  · Feedback: 1051783236@qq.com",
            "",
            "  Like it? Scan to support ❤"
        };
    }
}