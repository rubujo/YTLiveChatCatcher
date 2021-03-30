
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
            this.BtnStart = new System.Windows.Forms.Button();
            this.TBLog = new System.Windows.Forms.TextBox();
            this.TBChannelID = new System.Windows.Forms.TextBox();
            this.BtnStop = new System.Windows.Forms.Button();
            this.TBVideoID = new System.Windows.Forms.TextBox();
            this.LLiveChat = new System.Windows.Forms.Label();
            this.LChannelID = new System.Windows.Forms.Label();
            this.LVideoID = new System.Windows.Forms.Label();
            this.TBInterval = new System.Windows.Forms.TextBox();
            this.LInterval = new System.Windows.Forms.Label();
            this.LVLiveChatList = new System.Windows.Forms.ListView();
            this.LLog = new System.Windows.Forms.Label();
            this.BtnExport = new System.Windows.Forms.Button();
            this.PBProgress = new System.Windows.Forms.ProgressBar();
            this.BtnClear = new System.Windows.Forms.Button();
            this.RBtnStreaming = new System.Windows.Forms.RadioButton();
            this.RBtnReplay = new System.Windows.Forms.RadioButton();
            this.CBExportAuthorPhoto = new System.Windows.Forms.CheckBox();
            this.LChatCount = new System.Windows.Forms.Label();
            this.LVersion = new System.Windows.Forms.Label();
            this.LAuthorCount = new System.Windows.Forms.Label();
            this.LSuperChatCount = new System.Windows.Forms.Label();
            this.LSuperStickerCount = new System.Windows.Forms.Label();
            this.LMemberJoinCount = new System.Windows.Forms.Label();
            this.LMemberInRoomCount = new System.Windows.Forms.Label();
            this.CBEnableTTS = new System.Windows.Forms.CheckBox();
            this.LTempIncome = new System.Windows.Forms.Label();
            this.LUserAgent = new System.Windows.Forms.Label();
            this.TBUserAgent = new System.Windows.Forms.TextBox();
            this.CBRandomInterval = new System.Windows.Forms.CheckBox();
            this.CBLoadCookies = new System.Windows.Forms.CheckBox();
            this.LProfileFolderName = new System.Windows.Forms.Label();
            this.TBProfileFolderName = new System.Windows.Forms.TextBox();
            this.CBBrowser = new System.Windows.Forms.ComboBox();
            this.LNotice = new System.Windows.Forms.Label();
            this.CBEnableDebug = new System.Windows.Forms.CheckBox();
            this.BtnSearchUserAgent = new System.Windows.Forms.Button();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.BtnImport = new System.Windows.Forms.Button();
            this.BtnOpenVideoUrl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnStart
            // 
            this.BtnStart.Location = new System.Drawing.Point(630, 28);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(75, 23);
            this.BtnStart.TabIndex = 11;
            this.BtnStart.Text = "開始";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // TBLog
            // 
            this.TBLog.Location = new System.Drawing.Point(11, 465);
            this.TBLog.Multiline = true;
            this.TBLog.Name = "TBLog";
            this.TBLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TBLog.Size = new System.Drawing.Size(696, 109);
            this.TBLog.TabIndex = 29;
            // 
            // TBChannelID
            // 
            this.TBChannelID.Location = new System.Drawing.Point(12, 27);
            this.TBChannelID.Name = "TBChannelID";
            this.TBChannelID.Size = new System.Drawing.Size(303, 23);
            this.TBChannelID.TabIndex = 2;
            this.TBChannelID.TextChanged += new System.EventHandler(this.TBChannelID_TextChanged);
            // 
            // BtnStop
            // 
            this.BtnStop.Location = new System.Drawing.Point(711, 28);
            this.BtnStop.Name = "BtnStop";
            this.BtnStop.Size = new System.Drawing.Size(75, 23);
            this.BtnStop.TabIndex = 12;
            this.BtnStop.Text = "停止";
            this.BtnStop.UseVisualStyleBackColor = true;
            this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // TBVideoID
            // 
            this.TBVideoID.Location = new System.Drawing.Point(321, 28);
            this.TBVideoID.Name = "TBVideoID";
            this.TBVideoID.Size = new System.Drawing.Size(198, 23);
            this.TBVideoID.TabIndex = 4;
            this.TBVideoID.TextChanged += new System.EventHandler(this.TBVideoID_TextChanged);
            // 
            // LLiveChat
            // 
            this.LLiveChat.AutoSize = true;
            this.LLiveChat.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LLiveChat.Location = new System.Drawing.Point(13, 116);
            this.LLiveChat.Name = "LLiveChat";
            this.LLiveChat.Size = new System.Drawing.Size(67, 15);
            this.LLiveChat.TabIndex = 20;
            this.LLiveChat.Text = "聊天室內容";
            // 
            // LChannelID
            // 
            this.LChannelID.AutoSize = true;
            this.LChannelID.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LChannelID.Location = new System.Drawing.Point(12, 9);
            this.LChannelID.Name = "LChannelID";
            this.LChannelID.Size = new System.Drawing.Size(47, 15);
            this.LChannelID.TabIndex = 1;
            this.LChannelID.Text = "頻道 ID";
            // 
            // LVideoID
            // 
            this.LVideoID.AutoSize = true;
            this.LVideoID.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LVideoID.Location = new System.Drawing.Point(321, 9);
            this.LVideoID.Name = "LVideoID";
            this.LVideoID.Size = new System.Drawing.Size(47, 15);
            this.LVideoID.TabIndex = 3;
            this.LVideoID.Text = "影片 ID";
            // 
            // TBInterval
            // 
            this.TBInterval.Location = new System.Drawing.Point(525, 28);
            this.TBInterval.Name = "TBInterval";
            this.TBInterval.Size = new System.Drawing.Size(100, 23);
            this.TBInterval.TabIndex = 7;
            this.TBInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBInterval_KeyPress);
            // 
            // LInterval
            // 
            this.LInterval.AutoSize = true;
            this.LInterval.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LInterval.Location = new System.Drawing.Point(525, 8);
            this.LInterval.Name = "LInterval";
            this.LInterval.Size = new System.Drawing.Size(67, 15);
            this.LInterval.TabIndex = 6;
            this.LInterval.Text = "間隔（秒）";
            // 
            // LVLiveChatList
            // 
            this.LVLiveChatList.AllowDrop = true;
            this.LVLiveChatList.FullRowSelect = true;
            this.LVLiveChatList.GridLines = true;
            this.LVLiveChatList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LVLiveChatList.Location = new System.Drawing.Point(12, 137);
            this.LVLiveChatList.Name = "LVLiveChatList";
            this.LVLiveChatList.Size = new System.Drawing.Size(936, 305);
            this.LVLiveChatList.TabIndex = 21;
            this.LVLiveChatList.UseCompatibleStateImageBehavior = false;
            this.LVLiveChatList.View = System.Windows.Forms.View.Details;
            this.LVLiveChatList.DragDrop += new System.Windows.Forms.DragEventHandler(this.LVLiveChatList_DragDrop);
            this.LVLiveChatList.DragEnter += new System.Windows.Forms.DragEventHandler(this.LVLiveChatList_DragEnter);
            this.LVLiveChatList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LVLiveChatList_MouseClick);
            this.LVLiveChatList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LVLiveChatList_MouseDoubleClick);
            // 
            // LLog
            // 
            this.LLog.AutoSize = true;
            this.LLog.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LLog.Location = new System.Drawing.Point(11, 445);
            this.LLog.Name = "LLog";
            this.LLog.Size = new System.Drawing.Size(31, 15);
            this.LLog.TabIndex = 28;
            this.LLog.Text = "記錄";
            // 
            // BtnExport
            // 
            this.BtnExport.Location = new System.Drawing.Point(630, 108);
            this.BtnExport.Name = "BtnExport";
            this.BtnExport.Size = new System.Drawing.Size(75, 23);
            this.BtnExport.TabIndex = 24;
            this.BtnExport.Text = "匯出 *.xlsx";
            this.BtnExport.UseVisualStyleBackColor = true;
            this.BtnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // PBProgress
            // 
            this.PBProgress.Location = new System.Drawing.Point(849, 577);
            this.PBProgress.Name = "PBProgress";
            this.PBProgress.Size = new System.Drawing.Size(100, 23);
            this.PBProgress.TabIndex = 40;
            // 
            // BtnClear
            // 
            this.BtnClear.Location = new System.Drawing.Point(874, 108);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(75, 23);
            this.BtnClear.TabIndex = 27;
            this.BtnClear.Text = "清除";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // RBtnStreaming
            // 
            this.RBtnStreaming.AutoSize = true;
            this.RBtnStreaming.Checked = true;
            this.RBtnStreaming.Location = new System.Drawing.Point(630, 6);
            this.RBtnStreaming.Name = "RBtnStreaming";
            this.RBtnStreaming.Size = new System.Drawing.Size(49, 19);
            this.RBtnStreaming.TabIndex = 9;
            this.RBtnStreaming.TabStop = true;
            this.RBtnStreaming.Text = "直播";
            this.RBtnStreaming.UseVisualStyleBackColor = true;
            this.RBtnStreaming.CheckedChanged += new System.EventHandler(this.RBtnStreaming_CheckedChanged);
            // 
            // RBtnReplay
            // 
            this.RBtnReplay.AutoSize = true;
            this.RBtnReplay.Location = new System.Drawing.Point(711, 7);
            this.RBtnReplay.Name = "RBtnReplay";
            this.RBtnReplay.Size = new System.Drawing.Size(49, 19);
            this.RBtnReplay.TabIndex = 10;
            this.RBtnReplay.TabStop = true;
            this.RBtnReplay.Text = "重播";
            this.RBtnReplay.UseVisualStyleBackColor = true;
            this.RBtnReplay.CheckedChanged += new System.EventHandler(this.RBtnReplay_CheckedChanged);
            // 
            // CBExportAuthorPhoto
            // 
            this.CBExportAuthorPhoto.AutoSize = true;
            this.CBExportAuthorPhoto.Location = new System.Drawing.Point(551, 112);
            this.CBExportAuthorPhoto.Name = "CBExportAuthorPhoto";
            this.CBExportAuthorPhoto.Size = new System.Drawing.Size(74, 19);
            this.CBExportAuthorPhoto.TabIndex = 23;
            this.CBExportAuthorPhoto.Text = "匯出頭像";
            this.CBExportAuthorPhoto.UseVisualStyleBackColor = true;
            this.CBExportAuthorPhoto.CheckedChanged += new System.EventHandler(this.CBExportAuthorPhoto_CheckedChanged);
            // 
            // LChatCount
            // 
            this.LChatCount.AutoSize = true;
            this.LChatCount.Location = new System.Drawing.Point(714, 465);
            this.LChatCount.Name = "LChatCount";
            this.LChatCount.Size = new System.Drawing.Size(89, 15);
            this.LChatCount.TabIndex = 30;
            this.LChatCount.Text = "留言數量：0 個";
            // 
            // LVersion
            // 
            this.LVersion.AutoSize = true;
            this.LVersion.Location = new System.Drawing.Point(14, 583);
            this.LVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LVersion.Name = "LVersion";
            this.LVersion.Size = new System.Drawing.Size(43, 15);
            this.LVersion.TabIndex = 37;
            this.LVersion.Text = "版本號";
            // 
            // LAuthorCount
            // 
            this.LAuthorCount.AutoSize = true;
            this.LAuthorCount.Location = new System.Drawing.Point(714, 540);
            this.LAuthorCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LAuthorCount.Name = "LAuthorCount";
            this.LAuthorCount.Size = new System.Drawing.Size(89, 15);
            this.LAuthorCount.TabIndex = 35;
            this.LAuthorCount.Text = "留言人數：0 位";
            // 
            // LSuperChatCount
            // 
            this.LSuperChatCount.AutoSize = true;
            this.LSuperChatCount.Location = new System.Drawing.Point(714, 480);
            this.LSuperChatCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LSuperChatCount.Name = "LSuperChatCount";
            this.LSuperChatCount.Size = new System.Drawing.Size(89, 15);
            this.LSuperChatCount.TabIndex = 31;
            this.LSuperChatCount.Text = "超級留言：0 個";
            // 
            // LSuperStickerCount
            // 
            this.LSuperStickerCount.AutoSize = true;
            this.LSuperStickerCount.Location = new System.Drawing.Point(713, 495);
            this.LSuperStickerCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LSuperStickerCount.Name = "LSuperStickerCount";
            this.LSuperStickerCount.Size = new System.Drawing.Size(89, 15);
            this.LSuperStickerCount.TabIndex = 32;
            this.LSuperStickerCount.Text = "超級貼圖：0 個";
            // 
            // LMemberJoinCount
            // 
            this.LMemberJoinCount.AutoSize = true;
            this.LMemberJoinCount.Location = new System.Drawing.Point(714, 510);
            this.LMemberJoinCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LMemberJoinCount.Name = "LMemberJoinCount";
            this.LMemberJoinCount.Size = new System.Drawing.Size(89, 15);
            this.LMemberJoinCount.TabIndex = 33;
            this.LMemberJoinCount.Text = "加入會員：0 位";
            // 
            // LMemberInRoomCount
            // 
            this.LMemberInRoomCount.AutoSize = true;
            this.LMemberInRoomCount.Location = new System.Drawing.Point(714, 525);
            this.LMemberInRoomCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LMemberInRoomCount.Name = "LMemberInRoomCount";
            this.LMemberInRoomCount.Size = new System.Drawing.Size(89, 15);
            this.LMemberInRoomCount.TabIndex = 34;
            this.LMemberInRoomCount.Text = "會員人數：0 位";
            // 
            // CBEnableTTS
            // 
            this.CBEnableTTS.AutoSize = true;
            this.CBEnableTTS.Location = new System.Drawing.Point(86, 112);
            this.CBEnableTTS.Name = "CBEnableTTS";
            this.CBEnableTTS.Size = new System.Drawing.Size(74, 19);
            this.CBEnableTTS.TabIndex = 22;
            this.CBEnableTTS.Text = "啟用 TTS";
            this.CBEnableTTS.UseVisualStyleBackColor = true;
            this.CBEnableTTS.CheckedChanged += new System.EventHandler(this.CBEnableTTS_CheckedChanged);
            // 
            // LTempIncome
            // 
            this.LTempIncome.AutoSize = true;
            this.LTempIncome.Location = new System.Drawing.Point(713, 555);
            this.LTempIncome.Name = "LTempIncome";
            this.LTempIncome.Size = new System.Drawing.Size(116, 15);
            this.LTempIncome.TabIndex = 36;
            this.LTempIncome.Text = "預計收益：共 0 元整";
            // 
            // LUserAgent
            // 
            this.LUserAgent.AutoSize = true;
            this.LUserAgent.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LUserAgent.Location = new System.Drawing.Point(13, 61);
            this.LUserAgent.Name = "LUserAgent";
            this.LUserAgent.Size = new System.Drawing.Size(91, 15);
            this.LUserAgent.TabIndex = 17;
            this.LUserAgent.Text = "使用者代理字串";
            // 
            // TBUserAgent
            // 
            this.TBUserAgent.Location = new System.Drawing.Point(12, 79);
            this.TBUserAgent.Name = "TBUserAgent";
            this.TBUserAgent.Size = new System.Drawing.Size(613, 23);
            this.TBUserAgent.TabIndex = 18;
            this.TBUserAgent.TextChanged += new System.EventHandler(this.TBUserAgent_TextChanged);
            // 
            // CBRandomInterval
            // 
            this.CBRandomInterval.AutoSize = true;
            this.CBRandomInterval.Location = new System.Drawing.Point(525, 57);
            this.CBRandomInterval.Name = "CBRandomInterval";
            this.CBRandomInterval.Size = new System.Drawing.Size(98, 19);
            this.CBRandomInterval.TabIndex = 8;
            this.CBRandomInterval.Text = "隨機間隔秒數";
            this.CBRandomInterval.UseVisualStyleBackColor = true;
            this.CBRandomInterval.CheckedChanged += new System.EventHandler(this.CBRandomInterval_CheckedChanged);
            // 
            // CBLoadCookies
            // 
            this.CBLoadCookies.AutoSize = true;
            this.CBLoadCookies.Location = new System.Drawing.Point(792, 8);
            this.CBLoadCookies.Name = "CBLoadCookies";
            this.CBLoadCookies.Size = new System.Drawing.Size(98, 19);
            this.CBLoadCookies.TabIndex = 13;
            this.CBLoadCookies.Text = "載入 Cookies";
            this.CBLoadCookies.UseVisualStyleBackColor = true;
            this.CBLoadCookies.CheckedChanged += new System.EventHandler(this.CBLoadCookies_CheckedChanged);
            // 
            // LProfileFolderName
            // 
            this.LProfileFolderName.AutoSize = true;
            this.LProfileFolderName.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LProfileFolderName.Location = new System.Drawing.Point(791, 59);
            this.LProfileFolderName.Name = "LProfileFolderName";
            this.LProfileFolderName.Size = new System.Drawing.Size(103, 15);
            this.LProfileFolderName.TabIndex = 15;
            this.LProfileFolderName.Text = "設定檔資料夾名稱";
            // 
            // TBProfileFolderName
            // 
            this.TBProfileFolderName.Location = new System.Drawing.Point(791, 80);
            this.TBProfileFolderName.Name = "TBProfileFolderName";
            this.TBProfileFolderName.Size = new System.Drawing.Size(156, 23);
            this.TBProfileFolderName.TabIndex = 16;
            this.TBProfileFolderName.TextChanged += new System.EventHandler(this.TBProfileFolderName_TextChanged);
            // 
            // CBBrowser
            // 
            this.CBBrowser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBBrowser.FormattingEnabled = true;
            this.CBBrowser.ItemHeight = 15;
            this.CBBrowser.Items.AddRange(new object[] {
            "Google Chrome",
            "Microsoft Edge"});
            this.CBBrowser.Location = new System.Drawing.Point(792, 33);
            this.CBBrowser.Name = "CBBrowser";
            this.CBBrowser.Size = new System.Drawing.Size(156, 23);
            this.CBBrowser.TabIndex = 14;
            this.CBBrowser.SelectedIndexChanged += new System.EventHandler(this.CBBrowser_SelectedIndexChanged);
            // 
            // LNotice
            // 
            this.LNotice.AutoSize = true;
            this.LNotice.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.LNotice.ForeColor = System.Drawing.Color.Red;
            this.LNotice.Location = new System.Drawing.Point(606, 582);
            this.LNotice.Name = "LNotice";
            this.LNotice.Size = new System.Drawing.Size(101, 15);
            this.LNotice.TabIndex = 38;
            this.LNotice.Text = "※資訊僅供參考。";
            // 
            // CBEnableDebug
            // 
            this.CBEnableDebug.AutoSize = true;
            this.CBEnableDebug.Location = new System.Drawing.Point(716, 580);
            this.CBEnableDebug.Name = "CBEnableDebug";
            this.CBEnableDebug.Size = new System.Drawing.Size(122, 19);
            this.CBEnableDebug.TabIndex = 39;
            this.CBEnableDebug.Text = "啟用輸出除錯資訊";
            this.CBEnableDebug.UseVisualStyleBackColor = true;
            this.CBEnableDebug.CheckedChanged += new System.EventHandler(this.CBEnableDebug_CheckedChanged);
            // 
            // BtnSearchUserAgent
            // 
            this.BtnSearchUserAgent.Location = new System.Drawing.Point(630, 79);
            this.BtnSearchUserAgent.Name = "BtnSearchUserAgent";
            this.BtnSearchUserAgent.Size = new System.Drawing.Size(156, 23);
            this.BtnSearchUserAgent.TabIndex = 19;
            this.BtnSearchUserAgent.Text = "搜尋使用者代理字串";
            this.BtnSearchUserAgent.UseVisualStyleBackColor = true;
            this.BtnSearchUserAgent.Click += new System.EventHandler(this.BtnSearchUserAgent_Click);
            // 
            // BtnSearch
            // 
            this.BtnSearch.Location = new System.Drawing.Point(791, 108);
            this.BtnSearch.Margin = new System.Windows.Forms.Padding(2);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(73, 23);
            this.BtnSearch.TabIndex = 26;
            this.BtnSearch.Text = "搜尋";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // BtnImport
            // 
            this.BtnImport.Location = new System.Drawing.Point(711, 108);
            this.BtnImport.Name = "BtnImport";
            this.BtnImport.Size = new System.Drawing.Size(75, 23);
            this.BtnImport.TabIndex = 25;
            this.BtnImport.Text = "匯入 *.xlsx";
            this.BtnImport.UseVisualStyleBackColor = true;
            this.BtnImport.Click += new System.EventHandler(this.BtnImport_Click);
            // 
            // BtnOpenVideoUrl
            // 
            this.BtnOpenVideoUrl.Location = new System.Drawing.Point(414, 53);
            this.BtnOpenVideoUrl.Name = "BtnOpenVideoUrl";
            this.BtnOpenVideoUrl.Size = new System.Drawing.Size(105, 23);
            this.BtnOpenVideoUrl.TabIndex = 5;
            this.BtnOpenVideoUrl.Text = "開啟影片的網址";
            this.BtnOpenVideoUrl.UseVisualStyleBackColor = true;
            this.BtnOpenVideoUrl.Click += new System.EventHandler(this.BtnOpenVideoUrl_Click);
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(985, 610);
            this.Controls.Add(this.BtnOpenVideoUrl);
            this.Controls.Add(this.BtnImport);
            this.Controls.Add(this.BtnSearch);
            this.Controls.Add(this.BtnSearchUserAgent);
            this.Controls.Add(this.CBEnableDebug);
            this.Controls.Add(this.LNotice);
            this.Controls.Add(this.CBBrowser);
            this.Controls.Add(this.TBProfileFolderName);
            this.Controls.Add(this.LProfileFolderName);
            this.Controls.Add(this.CBLoadCookies);
            this.Controls.Add(this.CBRandomInterval);
            this.Controls.Add(this.TBUserAgent);
            this.Controls.Add(this.LUserAgent);
            this.Controls.Add(this.LTempIncome);
            this.Controls.Add(this.CBEnableTTS);
            this.Controls.Add(this.LMemberInRoomCount);
            this.Controls.Add(this.LMemberJoinCount);
            this.Controls.Add(this.LSuperStickerCount);
            this.Controls.Add(this.LSuperChatCount);
            this.Controls.Add(this.LAuthorCount);
            this.Controls.Add(this.LVersion);
            this.Controls.Add(this.LChatCount);
            this.Controls.Add(this.CBExportAuthorPhoto);
            this.Controls.Add(this.RBtnReplay);
            this.Controls.Add(this.RBtnStreaming);
            this.Controls.Add(this.BtnClear);
            this.Controls.Add(this.PBProgress);
            this.Controls.Add(this.BtnExport);
            this.Controls.Add(this.LLog);
            this.Controls.Add(this.LVLiveChatList);
            this.Controls.Add(this.LInterval);
            this.Controls.Add(this.TBInterval);
            this.Controls.Add(this.LVideoID);
            this.Controls.Add(this.LChannelID);
            this.Controls.Add(this.LLiveChat);
            this.Controls.Add(this.TBVideoID);
            this.Controls.Add(this.BtnStop);
            this.Controls.Add(this.TBChannelID);
            this.Controls.Add(this.TBLog);
            this.Controls.Add(this.BtnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FMain_FormClosing);
            this.Load += new System.EventHandler(this.FMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnStart;
        private System.Windows.Forms.TextBox TBLog;
        private System.Windows.Forms.TextBox TBChannelID;
        private System.Windows.Forms.Button BtnStop;
        private System.Windows.Forms.TextBox TBVideoID;
        private System.Windows.Forms.Label LLiveChat;
        private System.Windows.Forms.Label LChannelID;
        private System.Windows.Forms.Label LVideoID;
        private System.Windows.Forms.TextBox TBInterval;
        private System.Windows.Forms.Label LInterval;
        private System.Windows.Forms.ListView LVLiveChatList;
        private System.Windows.Forms.Label LLog;
        private System.Windows.Forms.Button BtnExport;
        private System.Windows.Forms.ProgressBar PBProgress;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.RadioButton RBtnStreaming;
        private System.Windows.Forms.RadioButton RBtnReplay;
        private System.Windows.Forms.CheckBox CBExportAuthorPhoto;
        private System.Windows.Forms.Label LChatCount;
        private System.Windows.Forms.Label LVersion;
        private System.Windows.Forms.Label LAuthorCount;
        private System.Windows.Forms.Label LSuperChatCount;
        private System.Windows.Forms.Label LSuperStickerCount;
        private System.Windows.Forms.Label LMemberJoinCount;
        private System.Windows.Forms.Label LMemberInRoomCount;
        private System.Windows.Forms.CheckBox CBEnableTTS;
        private System.Windows.Forms.Label LTempIncome;
        private Label LUserAgent;
        private TextBox TBUserAgent;
        private CheckBox CBRandomInterval;
        private CheckBox CBLoadCookies;
        private Label LProfileFolderName;
        private TextBox TBProfileFolderName;
        private ComboBox CBBrowser;
        private Label LNotice;
        private CheckBox CBEnableDebug;
        private Button BtnSearchUserAgent;
        private Button BtnSearch;
        private Button BtnImport;
        private Button BtnOpenVideoUrl;
    }
}

