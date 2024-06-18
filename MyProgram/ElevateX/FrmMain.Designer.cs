namespace Net.LoongTech.ElevateX
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ListViewItem listViewItem3 = new ListViewItem("");
            ListViewItem listViewItem4 = new ListViewItem("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            textBox1 = new TextBox();
            label1 = new Label();
            button1 = new Button();
            button2 = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            listViewEx1 = new ListViewEx(components);
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            tabPage2 = new TabPage();
            lvTxtLog = new ListViewEx(components);
            ch_datetime = new ColumnHeader();
            ch_content = new ColumnHeader();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(62, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(236, 23);
            textBox1.TabIndex = 0;
            textBox1.Text = "海顺退费";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(44, 17);
            label1.TabIndex = 1;
            label1.Text = "关键字";
            // 
            // button1
            // 
            button1.Location = new Point(304, 12);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "搜索";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(483, 12);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 5;
            button2.Text = "访问";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(1, 41);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1068, 647);
            tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Location = new Point(4, 26);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1060, 617);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "预览";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(webView21);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listViewEx1);
            splitContainer1.Size = new Size(1054, 611);
            splitContainer1.SplitterDistance = 549;
            splitContainer1.TabIndex = 5;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(3, 3);
            webView21.Name = "webView21";
            webView21.Size = new Size(541, 603);
            webView21.TabIndex = 4;
            webView21.ZoomFactor = 1D;
            // 
            // listViewEx1
            // 
            listViewEx1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listViewEx1.Dock = DockStyle.Fill;
            listViewEx1.FullRowSelect = true;
            listViewEx1.GridLines = true;
            listViewEx1.Items.AddRange(new ListViewItem[] { listViewItem3 });
            listViewEx1.Location = new Point(0, 0);
            listViewEx1.Name = "listViewEx1";
            listViewEx1.Size = new Size(499, 609);
            listViewEx1.TabIndex = 0;
            listViewEx1.UseCompatibleStateImageBehavior = false;
            listViewEx1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "结果标题";
            columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "结果URL";
            columnHeader2.Width = 200;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(lvTxtLog);
            tabPage2.Location = new Point(4, 26);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1060, 617);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "日志";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvTxtLog
            // 
            lvTxtLog.Columns.AddRange(new ColumnHeader[] { ch_datetime, ch_content });
            lvTxtLog.Dock = DockStyle.Fill;
            lvTxtLog.Items.AddRange(new ListViewItem[] { listViewItem4 });
            lvTxtLog.Location = new Point(3, 3);
            lvTxtLog.Name = "lvTxtLog";
            lvTxtLog.Size = new Size(1054, 611);
            lvTxtLog.TabIndex = 0;
            lvTxtLog.UseCompatibleStateImageBehavior = false;
            lvTxtLog.View = View.Details;
            // 
            // ch_datetime
            // 
            ch_datetime.Text = "时间";
            ch_datetime.Width = 160;
            // 
            // ch_content
            // 
            ch_content.Text = "内容";
            ch_content.Width = 500;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1073, 691);
            Controls.Add(tabControl1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FrmMain";
            Text = "Form1";
            Load += FrmMain_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Label label1;
        private Button button1;
        private Button button2;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private SplitContainer splitContainer1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private ListViewEx listViewEx1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private TabPage tabPage2;
        private ListViewEx lvTxtLog;
        private ColumnHeader ch_datetime;
        private ColumnHeader ch_content;
    }
}
