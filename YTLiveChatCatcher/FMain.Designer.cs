﻿
namespace YTLiveChatCatcher
{
    partial class FMain
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
            BtnStart = new Button();
            TBLog = new TextBox();
            TBChannelID = new TextBox();
            BtnStop = new Button();
            TBVideoID = new TextBox();
            LLiveChat = new Label();
            LChannelID = new Label();
            LVideoID = new Label();
            TBInterval = new TextBox();
            LInterval = new Label();
            LVLiveChatList = new ListView();
            LLog = new Label();
            BtnExport = new Button();
            PBProgress = new ProgressBar();
            BtnClear = new Button();
            CBExportAuthorPhoto = new CheckBox();
            LChatCount = new Label();
            LVersion = new Label();
            LAuthorCount = new Label();
            LSuperChatCount = new Label();
            LSuperStickerCount = new Label();
            LMemberJoinCount = new Label();
            LMemberInRoomCount = new Label();
            CBEnableTTS = new CheckBox();
            LTempIncome = new Label();
            LUserAgent = new Label();
            TBUserAgent = new TextBox();
            CBLoadCookie = new CheckBox();
            LProfileFolderName = new Label();
            TBProfileFolderName = new TextBox();
            CBBrowser = new ComboBox();
            LNotice = new Label();
            CBEnableDebug = new CheckBox();
            BtnSearchUserAgent = new Button();
            BtnSearch = new Button();
            BtnImport = new Button();
            BtnOpenVideoUrl = new Button();
            LSecChUa = new Label();
            TBSecChUa = new TextBox();
            SuspendLayout();
            // 
            // BtnStart
            // 
            BtnStart.Location = new Point(630, 28);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(75, 23);
            BtnStart.TabIndex = 8;
            BtnStart.Text = "開始";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // TBLog
            // 
            TBLog.Location = new Point(11, 465);
            TBLog.Multiline = true;
            TBLog.Name = "TBLog";
            TBLog.ScrollBars = ScrollBars.Vertical;
            TBLog.Size = new Size(696, 109);
            TBLog.TabIndex = 28;
            // 
            // TBChannelID
            // 
            TBChannelID.Location = new Point(12, 27);
            TBChannelID.Name = "TBChannelID";
            TBChannelID.Size = new Size(303, 23);
            TBChannelID.TabIndex = 2;
            TBChannelID.TextChanged += TBChannelID_TextChanged;
            // 
            // BtnStop
            // 
            BtnStop.Location = new Point(711, 28);
            BtnStop.Name = "BtnStop";
            BtnStop.Size = new Size(75, 23);
            BtnStop.TabIndex = 9;
            BtnStop.Text = "停止";
            BtnStop.UseVisualStyleBackColor = true;
            BtnStop.Click += BtnStop_Click;
            // 
            // TBVideoID
            // 
            TBVideoID.Location = new Point(321, 28);
            TBVideoID.Name = "TBVideoID";
            TBVideoID.Size = new Size(198, 23);
            TBVideoID.TabIndex = 4;
            TBVideoID.TextChanged += TBVideoID_TextChanged;
            // 
            // LLiveChat
            // 
            LLiveChat.AutoSize = true;
            LLiveChat.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LLiveChat.Location = new Point(14, 149);
            LLiveChat.Name = "LLiveChat";
            LLiveChat.Size = new Size(67, 15);
            LLiveChat.TabIndex = 25;
            LLiveChat.Text = "聊天室內容";
            // 
            // LChannelID
            // 
            LChannelID.AutoSize = true;
            LChannelID.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LChannelID.Location = new Point(12, 9);
            LChannelID.Name = "LChannelID";
            LChannelID.Size = new Size(47, 15);
            LChannelID.TabIndex = 1;
            LChannelID.Text = "頻道 ID";
            // 
            // LVideoID
            // 
            LVideoID.AutoSize = true;
            LVideoID.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LVideoID.Location = new Point(321, 9);
            LVideoID.Name = "LVideoID";
            LVideoID.Size = new Size(47, 15);
            LVideoID.TabIndex = 3;
            LVideoID.Text = "影片 ID";
            // 
            // TBInterval
            // 
            TBInterval.Enabled = false;
            TBInterval.Location = new Point(525, 28);
            TBInterval.Name = "TBInterval";
            TBInterval.Size = new Size(100, 23);
            TBInterval.TabIndex = 7;
            TBInterval.KeyPress += TBInterval_KeyPress;
            // 
            // LInterval
            // 
            LInterval.AutoSize = true;
            LInterval.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LInterval.Location = new Point(525, 8);
            LInterval.Name = "LInterval";
            LInterval.Size = new Size(67, 15);
            LInterval.TabIndex = 6;
            LInterval.Text = "間隔（秒）";
            // 
            // LVLiveChatList
            // 
            LVLiveChatList.AllowDrop = true;
            LVLiveChatList.FullRowSelect = true;
            LVLiveChatList.GridLines = true;
            LVLiveChatList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            LVLiveChatList.Location = new Point(12, 169);
            LVLiveChatList.Name = "LVLiveChatList";
            LVLiveChatList.Size = new Size(961, 273);
            LVLiveChatList.TabIndex = 26;
            LVLiveChatList.UseCompatibleStateImageBehavior = false;
            LVLiveChatList.View = View.Details;
            LVLiveChatList.DragDrop += LVLiveChatList_DragDrop;
            LVLiveChatList.DragEnter += LVLiveChatList_DragEnter;
            LVLiveChatList.MouseClick += LVLiveChatList_MouseClick;
            LVLiveChatList.MouseDoubleClick += LVLiveChatList_MouseDoubleClick;
            // 
            // LLog
            // 
            LLog.AutoSize = true;
            LLog.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LLog.Location = new Point(11, 445);
            LLog.Name = "LLog";
            LLog.Size = new Size(31, 15);
            LLog.TabIndex = 27;
            LLog.Text = "記錄";
            // 
            // BtnExport
            // 
            BtnExport.Location = new Point(791, 140);
            BtnExport.Name = "BtnExport";
            BtnExport.Size = new Size(75, 23);
            BtnExport.TabIndex = 22;
            BtnExport.Text = "匯出 *.xlsx";
            BtnExport.UseVisualStyleBackColor = true;
            BtnExport.Click += BtnExport_Click;
            // 
            // PBProgress
            // 
            PBProgress.Location = new Point(873, 576);
            PBProgress.Name = "PBProgress";
            PBProgress.Size = new Size(100, 23);
            PBProgress.TabIndex = 39;
            // 
            // BtnClear
            // 
            BtnClear.Location = new Point(898, 140);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new Size(75, 23);
            BtnClear.TabIndex = 24;
            BtnClear.Text = "清除";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;
            // 
            // CBExportAuthorPhoto
            // 
            CBExportAuthorPhoto.AutoSize = true;
            CBExportAuthorPhoto.Location = new Point(792, 116);
            CBExportAuthorPhoto.Name = "CBExportAuthorPhoto";
            CBExportAuthorPhoto.Size = new Size(74, 19);
            CBExportAuthorPhoto.TabIndex = 23;
            CBExportAuthorPhoto.Text = "匯出頭像";
            CBExportAuthorPhoto.UseVisualStyleBackColor = true;
            CBExportAuthorPhoto.CheckedChanged += CBExportAuthorPhoto_CheckedChanged;
            // 
            // LChatCount
            // 
            LChatCount.AutoSize = true;
            LChatCount.Location = new Point(714, 465);
            LChatCount.Name = "LChatCount";
            LChatCount.Size = new Size(89, 15);
            LChatCount.TabIndex = 29;
            LChatCount.Text = "留言數量：0 個";
            // 
            // LVersion
            // 
            LVersion.AutoSize = true;
            LVersion.Location = new Point(14, 583);
            LVersion.Margin = new Padding(2, 0, 2, 0);
            LVersion.Name = "LVersion";
            LVersion.Size = new Size(43, 15);
            LVersion.TabIndex = 36;
            LVersion.Text = "版本號";
            // 
            // LAuthorCount
            // 
            LAuthorCount.AutoSize = true;
            LAuthorCount.Location = new Point(714, 540);
            LAuthorCount.Margin = new Padding(2, 0, 2, 0);
            LAuthorCount.Name = "LAuthorCount";
            LAuthorCount.Size = new Size(89, 15);
            LAuthorCount.TabIndex = 34;
            LAuthorCount.Text = "留言人數：0 位";
            // 
            // LSuperChatCount
            // 
            LSuperChatCount.AutoSize = true;
            LSuperChatCount.Location = new Point(714, 480);
            LSuperChatCount.Margin = new Padding(2, 0, 2, 0);
            LSuperChatCount.Name = "LSuperChatCount";
            LSuperChatCount.Size = new Size(89, 15);
            LSuperChatCount.TabIndex = 30;
            LSuperChatCount.Text = "超級留言：0 個";
            // 
            // LSuperStickerCount
            // 
            LSuperStickerCount.AutoSize = true;
            LSuperStickerCount.Location = new Point(713, 495);
            LSuperStickerCount.Margin = new Padding(2, 0, 2, 0);
            LSuperStickerCount.Name = "LSuperStickerCount";
            LSuperStickerCount.Size = new Size(89, 15);
            LSuperStickerCount.TabIndex = 31;
            LSuperStickerCount.Text = "超級貼圖：0 個";
            // 
            // LMemberJoinCount
            // 
            LMemberJoinCount.AutoSize = true;
            LMemberJoinCount.Location = new Point(714, 510);
            LMemberJoinCount.Margin = new Padding(2, 0, 2, 0);
            LMemberJoinCount.Name = "LMemberJoinCount";
            LMemberJoinCount.Size = new Size(89, 15);
            LMemberJoinCount.TabIndex = 32;
            LMemberJoinCount.Text = "加入會員：0 位";
            // 
            // LMemberInRoomCount
            // 
            LMemberInRoomCount.AutoSize = true;
            LMemberInRoomCount.Location = new Point(714, 525);
            LMemberInRoomCount.Margin = new Padding(2, 0, 2, 0);
            LMemberInRoomCount.Name = "LMemberInRoomCount";
            LMemberInRoomCount.Size = new Size(89, 15);
            LMemberInRoomCount.TabIndex = 33;
            LMemberInRoomCount.Text = "會員人數：0 位";
            // 
            // CBEnableTTS
            // 
            CBEnableTTS.AutoSize = true;
            CBEnableTTS.Location = new Point(630, 116);
            CBEnableTTS.Name = "CBEnableTTS";
            CBEnableTTS.Size = new Size(74, 19);
            CBEnableTTS.TabIndex = 19;
            CBEnableTTS.Text = "啟用 TTS";
            CBEnableTTS.UseVisualStyleBackColor = true;
            CBEnableTTS.CheckedChanged += CBEnableTTS_CheckedChanged;
            // 
            // LTempIncome
            // 
            LTempIncome.AutoSize = true;
            LTempIncome.Location = new Point(713, 555);
            LTempIncome.Name = "LTempIncome";
            LTempIncome.Size = new Size(116, 15);
            LTempIncome.TabIndex = 35;
            LTempIncome.Text = "預計收益：共 0 元整";
            // 
            // LUserAgent
            // 
            LUserAgent.AutoSize = true;
            LUserAgent.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LUserAgent.Location = new Point(13, 61);
            LUserAgent.Name = "LUserAgent";
            LUserAgent.Size = new Size(91, 15);
            LUserAgent.TabIndex = 14;
            LUserAgent.Text = "使用者代理字串";
            // 
            // TBUserAgent
            // 
            TBUserAgent.Location = new Point(12, 79);
            TBUserAgent.Name = "TBUserAgent";
            TBUserAgent.Size = new Size(613, 23);
            TBUserAgent.TabIndex = 15;
            TBUserAgent.TextChanged += TBUserAgent_TextChanged;
            // 
            // CBLoadCookie
            // 
            CBLoadCookie.AutoSize = true;
            CBLoadCookie.Location = new Point(792, 8);
            CBLoadCookie.Name = "CBLoadCookie";
            CBLoadCookie.Size = new Size(93, 19);
            CBLoadCookie.TabIndex = 10;
            CBLoadCookie.Text = "載入 Cookie";
            CBLoadCookie.UseVisualStyleBackColor = true;
            CBLoadCookie.CheckedChanged += CBLoadCookie_CheckedChanged;
            // 
            // LProfileFolderName
            // 
            LProfileFolderName.AutoSize = true;
            LProfileFolderName.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LProfileFolderName.Location = new Point(791, 59);
            LProfileFolderName.Name = "LProfileFolderName";
            LProfileFolderName.Size = new Size(103, 15);
            LProfileFolderName.TabIndex = 12;
            LProfileFolderName.Text = "設定檔資料夾名稱";
            // 
            // TBProfileFolderName
            // 
            TBProfileFolderName.Location = new Point(791, 80);
            TBProfileFolderName.Name = "TBProfileFolderName";
            TBProfileFolderName.Size = new Size(182, 23);
            TBProfileFolderName.TabIndex = 13;
            TBProfileFolderName.TextChanged += TBProfileFolderName_TextChanged;
            // 
            // CBBrowser
            // 
            CBBrowser.DropDownStyle = ComboBoxStyle.DropDownList;
            CBBrowser.FormattingEnabled = true;
            CBBrowser.ItemHeight = 15;
            CBBrowser.Items.AddRange(new object[] { "Brave", "Brave Beta", "Brave Nightly", "Google Chrome", "Google Chrome Beta", "Google Chrome Canary", "Chromium", "Microsoft Edge", "Microsoft Edge Insider Beta", "Microsoft Edge Insider Dev", "Microsoft Edge Insider Canary", "Opera", "Opera Beta", "Opera Developer", "Opera GX", "Opera Crypto", "Vivaldi", "Mozilla Firefox" });
            CBBrowser.Location = new Point(792, 33);
            CBBrowser.Name = "CBBrowser";
            CBBrowser.Size = new Size(181, 23);
            CBBrowser.TabIndex = 11;
            CBBrowser.SelectedIndexChanged += CBBrowser_SelectedIndexChanged;
            // 
            // LNotice
            // 
            LNotice.AutoSize = true;
            LNotice.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LNotice.ForeColor = Color.Red;
            LNotice.Location = new Point(606, 582);
            LNotice.Name = "LNotice";
            LNotice.Size = new Size(101, 15);
            LNotice.TabIndex = 37;
            LNotice.Text = "※資訊僅供參考。";
            // 
            // CBEnableDebug
            // 
            CBEnableDebug.AutoSize = true;
            CBEnableDebug.Location = new Point(716, 580);
            CBEnableDebug.Name = "CBEnableDebug";
            CBEnableDebug.Size = new Size(122, 19);
            CBEnableDebug.TabIndex = 38;
            CBEnableDebug.Text = "啟用輸出除錯資訊";
            CBEnableDebug.UseVisualStyleBackColor = true;
            CBEnableDebug.CheckedChanged += CBEnableDebug_CheckedChanged;
            // 
            // BtnSearchUserAgent
            // 
            BtnSearchUserAgent.Location = new Point(630, 79);
            BtnSearchUserAgent.Name = "BtnSearchUserAgent";
            BtnSearchUserAgent.Size = new Size(156, 23);
            BtnSearchUserAgent.TabIndex = 16;
            BtnSearchUserAgent.Text = "搜尋使用者代理字串";
            BtnSearchUserAgent.UseVisualStyleBackColor = true;
            BtnSearchUserAgent.Click += BtnSearchUserAgent_Click;
            // 
            // BtnSearch
            // 
            BtnSearch.Location = new Point(630, 140);
            BtnSearch.Margin = new Padding(2);
            BtnSearch.Name = "BtnSearch";
            BtnSearch.Size = new Size(73, 23);
            BtnSearch.TabIndex = 20;
            BtnSearch.Text = "搜尋";
            BtnSearch.UseVisualStyleBackColor = true;
            BtnSearch.Click += BtnSearch_Click;
            // 
            // BtnImport
            // 
            BtnImport.Location = new Point(711, 140);
            BtnImport.Name = "BtnImport";
            BtnImport.Size = new Size(75, 23);
            BtnImport.TabIndex = 21;
            BtnImport.Text = "匯入 *.xlsx";
            BtnImport.UseVisualStyleBackColor = true;
            BtnImport.Click += BtnImport_Click;
            // 
            // BtnOpenVideoUrl
            // 
            BtnOpenVideoUrl.Location = new Point(414, 53);
            BtnOpenVideoUrl.Name = "BtnOpenVideoUrl";
            BtnOpenVideoUrl.Size = new Size(105, 23);
            BtnOpenVideoUrl.TabIndex = 5;
            BtnOpenVideoUrl.Text = "開啟影片的網址";
            BtnOpenVideoUrl.UseVisualStyleBackColor = true;
            BtnOpenVideoUrl.Click += BtnOpenVideoUrl_Click;
            // 
            // LSecChUa
            // 
            LSecChUa.AutoSize = true;
            LSecChUa.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LSecChUa.Location = new Point(12, 105);
            LSecChUa.Name = "LSecChUa";
            LSecChUa.Size = new Size(72, 15);
            LSecChUa.TabIndex = 17;
            LSecChUa.Text = "Sec-CH-UA";
            // 
            // TBSecChUa
            // 
            TBSecChUa.Location = new Point(12, 123);
            TBSecChUa.Name = "TBSecChUa";
            TBSecChUa.Size = new Size(613, 23);
            TBSecChUa.TabIndex = 18;
            TBSecChUa.TextChanged += TBSecChUa_TextChanged;
            // 
            // FMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(994, 611);
            Controls.Add(TBSecChUa);
            Controls.Add(LSecChUa);
            Controls.Add(BtnOpenVideoUrl);
            Controls.Add(BtnImport);
            Controls.Add(BtnSearch);
            Controls.Add(BtnSearchUserAgent);
            Controls.Add(CBEnableDebug);
            Controls.Add(LNotice);
            Controls.Add(CBBrowser);
            Controls.Add(TBProfileFolderName);
            Controls.Add(LProfileFolderName);
            Controls.Add(CBLoadCookie);
            Controls.Add(TBUserAgent);
            Controls.Add(LUserAgent);
            Controls.Add(LTempIncome);
            Controls.Add(CBEnableTTS);
            Controls.Add(LMemberInRoomCount);
            Controls.Add(LMemberJoinCount);
            Controls.Add(LSuperStickerCount);
            Controls.Add(LSuperChatCount);
            Controls.Add(LAuthorCount);
            Controls.Add(LVersion);
            Controls.Add(LChatCount);
            Controls.Add(CBExportAuthorPhoto);
            Controls.Add(BtnClear);
            Controls.Add(PBProgress);
            Controls.Add(BtnExport);
            Controls.Add(LLog);
            Controls.Add(LVLiveChatList);
            Controls.Add(LInterval);
            Controls.Add(TBInterval);
            Controls.Add(LVideoID);
            Controls.Add(LChannelID);
            Controls.Add(LLiveChat);
            Controls.Add(TBVideoID);
            Controls.Add(BtnStop);
            Controls.Add(TBChannelID);
            Controls.Add(TBLog);
            Controls.Add(BtnStart);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FMain";
            FormClosing += FMain_FormClosing;
            Load += FMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button BtnStart;
        private TextBox TBLog;
        private TextBox TBChannelID;
        private Button BtnStop;
        private TextBox TBVideoID;
        private Label LLiveChat;
        private Label LChannelID;
        private Label LVideoID;
        private TextBox TBInterval;
        private Label LInterval;
        private ListView LVLiveChatList;
        private Label LLog;
        private Button BtnExport;
        private ProgressBar PBProgress;
        private Button BtnClear;
        private CheckBox CBExportAuthorPhoto;
        private Label LChatCount;
        private Label LVersion;
        private Label LAuthorCount;
        private Label LSuperChatCount;
        private Label LSuperStickerCount;
        private Label LMemberJoinCount;
        private Label LMemberInRoomCount;
        private CheckBox CBEnableTTS;
        private Label LTempIncome;
        private Label LUserAgent;
        private TextBox TBUserAgent;
        private CheckBox CBLoadCookie;
        private Label LProfileFolderName;
        private TextBox TBProfileFolderName;
        private ComboBox CBBrowser;
        private Label LNotice;
        private CheckBox CBEnableDebug;
        private Button BtnSearchUserAgent;
        private Button BtnSearch;
        private Button BtnImport;
        private Button BtnOpenVideoUrl;
        private Label LSecChUa;
        private TextBox TBSecChUa;
    }
}

