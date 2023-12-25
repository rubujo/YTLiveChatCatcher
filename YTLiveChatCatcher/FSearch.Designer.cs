namespace YTLiveChatCatcher
{
    partial class FSearch
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            LKeyword = new Label();
            TBKeyword = new TextBox();
            BtnSearch = new Button();
            BtnClear = new Button();
            LVFilteredList = new ListView();
            LChatCount = new Label();
            PBProgress = new ProgressBar();
            BtnExport = new Button();
            SuspendLayout();
            // 
            // LKeyword
            // 
            LKeyword.AutoSize = true;
            LKeyword.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            LKeyword.Location = new Point(12, 9);
            LKeyword.Name = "LKeyword";
            LKeyword.Size = new Size(43, 15);
            LKeyword.TabIndex = 0;
            LKeyword.Text = "關鍵字";
            // 
            // TBKeyword
            // 
            TBKeyword.Location = new Point(12, 27);
            TBKeyword.Name = "TBKeyword";
            TBKeyword.Size = new Size(799, 23);
            TBKeyword.TabIndex = 1;
            // 
            // BtnSearch
            // 
            BtnSearch.Location = new Point(817, 27);
            BtnSearch.Name = "BtnSearch";
            BtnSearch.Size = new Size(75, 23);
            BtnSearch.TabIndex = 2;
            BtnSearch.Text = "搜尋";
            BtnSearch.UseVisualStyleBackColor = true;
            BtnSearch.Click += BtnSearch_Click;
            // 
            // BtnClear
            // 
            BtnClear.Location = new Point(898, 26);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new Size(75, 23);
            BtnClear.TabIndex = 3;
            BtnClear.Text = "清除";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;
            // 
            // LVFilteredList
            // 
            LVFilteredList.FullRowSelect = true;
            LVFilteredList.GridLines = true;
            LVFilteredList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            LVFilteredList.Location = new Point(12, 56);
            LVFilteredList.Name = "LVFilteredList";
            LVFilteredList.Size = new Size(961, 204);
            LVFilteredList.TabIndex = 4;
            LVFilteredList.UseCompatibleStateImageBehavior = false;
            LVFilteredList.View = View.Details;
            LVFilteredList.MouseClick += LVFilteredList_MouseClick;
            LVFilteredList.MouseDoubleClick += LVFilteredList_MouseDoubleClick;
            // 
            // LChatCount
            // 
            LChatCount.AutoSize = true;
            LChatCount.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            LChatCount.Location = new Point(12, 274);
            LChatCount.Name = "LChatCount";
            LChatCount.Size = new Size(89, 15);
            LChatCount.TabIndex = 5;
            LChatCount.Text = "留言數量：0 個";
            // 
            // PBProgress
            // 
            PBProgress.Location = new Point(873, 266);
            PBProgress.Name = "PBProgress";
            PBProgress.Size = new Size(100, 23);
            PBProgress.TabIndex = 6;
            // 
            // BtnExport
            // 
            BtnExport.Location = new Point(792, 266);
            BtnExport.Name = "BtnExport";
            BtnExport.Size = new Size(75, 23);
            BtnExport.TabIndex = 7;
            BtnExport.Text = "匯出 *.xlsx";
            BtnExport.UseVisualStyleBackColor = true;
            BtnExport.Click += BtnExport_Click;
            // 
            // FSearch
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(994, 301);
            Controls.Add(BtnExport);
            Controls.Add(PBProgress);
            Controls.Add(LChatCount);
            Controls.Add(LVFilteredList);
            Controls.Add(BtnClear);
            Controls.Add(BtnSearch);
            Controls.Add(TBKeyword);
            Controls.Add(LKeyword);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FSearch";
            FormClosing += FSearch_FormClosing;
            Load += FSearch_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label LKeyword;
        private TextBox TBKeyword;
        private Button BtnSearch;
        private Button BtnClear;
        private ListView LVFilteredList;
        private Label LChatCount;
        private ProgressBar PBProgress;
        private Button BtnExport;
    }
}