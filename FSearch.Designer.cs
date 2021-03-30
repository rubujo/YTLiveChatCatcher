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
            this.LKeyword = new System.Windows.Forms.Label();
            this.TBKeyword = new System.Windows.Forms.TextBox();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.BtnClear = new System.Windows.Forms.Button();
            this.LVFilteredList = new System.Windows.Forms.ListView();
            this.LChatCount = new System.Windows.Forms.Label();
            this.PBProgress = new System.Windows.Forms.ProgressBar();
            this.BtnExport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LKeyword
            // 
            this.LKeyword.AutoSize = true;
            this.LKeyword.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LKeyword.Location = new System.Drawing.Point(12, 9);
            this.LKeyword.Name = "LKeyword";
            this.LKeyword.Size = new System.Drawing.Size(43, 15);
            this.LKeyword.TabIndex = 0;
            this.LKeyword.Text = "關鍵字";
            // 
            // TBKeyword
            // 
            this.TBKeyword.Location = new System.Drawing.Point(12, 27);
            this.TBKeyword.Name = "TBKeyword";
            this.TBKeyword.Size = new System.Drawing.Size(788, 23);
            this.TBKeyword.TabIndex = 1;
            // 
            // BtnSearch
            // 
            this.BtnSearch.Location = new System.Drawing.Point(806, 27);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(75, 23);
            this.BtnSearch.TabIndex = 2;
            this.BtnSearch.Text = "搜尋";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // BtnClear
            // 
            this.BtnClear.Location = new System.Drawing.Point(887, 26);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(75, 23);
            this.BtnClear.TabIndex = 3;
            this.BtnClear.Text = "清除";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // LVFilteredList
            // 
            this.LVFilteredList.FullRowSelect = true;
            this.LVFilteredList.GridLines = true;
            this.LVFilteredList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LVFilteredList.Location = new System.Drawing.Point(12, 56);
            this.LVFilteredList.Name = "LVFilteredList";
            this.LVFilteredList.Size = new System.Drawing.Size(950, 194);
            this.LVFilteredList.TabIndex = 4;
            this.LVFilteredList.UseCompatibleStateImageBehavior = false;
            this.LVFilteredList.View = System.Windows.Forms.View.Details;
            this.LVFilteredList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LVFilteredList_MouseClick);
            this.LVFilteredList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LVFilteredList_MouseDoubleClick);
            // 
            // LChatCount
            // 
            this.LChatCount.AutoSize = true;
            this.LChatCount.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LChatCount.Location = new System.Drawing.Point(12, 264);
            this.LChatCount.Name = "LChatCount";
            this.LChatCount.Size = new System.Drawing.Size(89, 15);
            this.LChatCount.TabIndex = 5;
            this.LChatCount.Text = "留言數量：0 個";
            // 
            // PBProgress
            // 
            this.PBProgress.Location = new System.Drawing.Point(862, 256);
            this.PBProgress.Name = "PBProgress";
            this.PBProgress.Size = new System.Drawing.Size(100, 23);
            this.PBProgress.TabIndex = 6;
            // 
            // BtnExport
            // 
            this.BtnExport.Location = new System.Drawing.Point(781, 256);
            this.BtnExport.Name = "BtnExport";
            this.BtnExport.Size = new System.Drawing.Size(75, 23);
            this.BtnExport.TabIndex = 7;
            this.BtnExport.Text = "匯出 *.xlsx";
            this.BtnExport.UseVisualStyleBackColor = true;
            this.BtnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // FSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(985, 295);
            this.Controls.Add(this.BtnExport);
            this.Controls.Add(this.PBProgress);
            this.Controls.Add(this.LChatCount);
            this.Controls.Add(this.LVFilteredList);
            this.Controls.Add(this.BtnClear);
            this.Controls.Add(this.BtnSearch);
            this.Controls.Add(this.TBKeyword);
            this.Controls.Add(this.LKeyword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FSearch";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FSearch_FormClosing);
            this.Load += new System.EventHandler(this.FSearch_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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