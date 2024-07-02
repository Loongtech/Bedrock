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
            ListViewItem listViewItem1 = new ListViewItem("");
            ListViewItem listViewItem2 = new ListViewItem("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            btnRun = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            tabPage2 = new TabPage();
            label2 = new Label();
            label4 = new Label();
            tabControl2 = new TabControl();
            tabPage3 = new TabPage();
            splitContainer2 = new SplitContainer();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            listViewEx1 = new ListViewEx(components);
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            tabPage4 = new TabPage();
            lvTxtLog = new ListViewEx(components);
            ch_datetime = new ColumnHeader();
            ch_content = new ColumnHeader();
            numRunTime = new NumericUpDown();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRunTime).BeginInit();
            SuspendLayout();
            // 
            // btnRun
            // 
            btnRun.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRun.Location = new Point(986, 6);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(75, 23);
            btnRun.TabIndex = 5;
            btnRun.Text = "开始";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += button2_Click;
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
            splitContainer1.Size = new Size(1054, 611);
            splitContainer1.SplitterDistance = 449;
            splitContainer1.TabIndex = 5;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 26);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1060, 617);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "日志";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(788, 9);
            label2.Name = "label2";
            label2.Size = new Size(20, 17);
            label2.TabIndex = 7;
            label2.Text = "每";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(867, 9);
            label4.Name = "label4";
            label4.Size = new Size(80, 17);
            label4.TabIndex = 9;
            label4.Text = "分钟执行一次";
            // 
            // tabControl2
            // 
            tabControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Controls.Add(tabPage4);
            tabControl2.Location = new Point(2, 6);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(1068, 683);
            tabControl2.TabIndex = 8;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(splitContainer2);
            tabPage3.Location = new Point(4, 26);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(1060, 653);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "预览";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = BorderStyle.FixedSingle;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(3, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(webView21);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(listViewEx1);
            splitContainer2.Size = new Size(1054, 647);
            splitContainer2.SplitterDistance = 449;
            splitContainer2.TabIndex = 5;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(3, 3);
            webView21.Name = "webView21";
            webView21.Size = new Size(441, 643);
            webView21.TabIndex = 5;
            webView21.ZoomFactor = 1D;
            // 
            // listViewEx1
            // 
            listViewEx1.Activation = ItemActivation.OneClick;
            listViewEx1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listViewEx1.Dock = DockStyle.Fill;
            listViewEx1.FullRowSelect = true;
            listViewEx1.GridLines = true;
            listViewEx1.Items.AddRange(new ListViewItem[] { listViewItem1 });
            listViewEx1.Location = new Point(0, 0);
            listViewEx1.Name = "listViewEx1";
            listViewEx1.Size = new Size(599, 645);
            listViewEx1.TabIndex = 1;
            listViewEx1.UseCompatibleStateImageBehavior = false;
            listViewEx1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "结果标题";
            columnHeader1.Width = 350;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "结果URL";
            columnHeader2.Width = 600;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(lvTxtLog);
            tabPage4.Location = new Point(4, 26);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(1060, 653);
            tabPage4.TabIndex = 1;
            tabPage4.Text = "日志";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // lvTxtLog
            // 
            lvTxtLog.Columns.AddRange(new ColumnHeader[] { ch_datetime, ch_content });
            lvTxtLog.Dock = DockStyle.Fill;
            lvTxtLog.Items.AddRange(new ListViewItem[] { listViewItem2 });
            lvTxtLog.Location = new Point(3, 3);
            lvTxtLog.Name = "lvTxtLog";
            lvTxtLog.Size = new Size(1054, 647);
            lvTxtLog.TabIndex = 1;
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
            // numRunTime
            // 
            numRunTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numRunTime.Location = new Point(808, 7);
            numRunTime.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numRunTime.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRunTime.Name = "numRunTime";
            numRunTime.Size = new Size(56, 23);
            numRunTime.TabIndex = 10;
            numRunTime.TextAlign = HorizontalAlignment.Center;
            numRunTime.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1073, 691);
            Controls.Add(numRunTime);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(btnRun);
            Controls.Add(tabControl2);
            Controls.Add(tabControl1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FrmMain";
            Text = "自动点击";
            Load += FrmMain_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numRunTime).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnRun;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private SplitContainer splitContainer1;
        private TabPage tabPage2;
        private Label label3;
        private NumericUpDown numericUpDown1;
        private Label label2;
        private Label label4;
        private TabControl tabControl2;
        private TabPage tabPage3;
        private SplitContainer splitContainer2;
        private TabPage tabPage4;
        private NumericUpDown numRunTime;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private ListViewEx listViewEx1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ListViewEx lvTxtLog;
        private ColumnHeader ch_datetime;
        private ColumnHeader ch_content;
    }
}
