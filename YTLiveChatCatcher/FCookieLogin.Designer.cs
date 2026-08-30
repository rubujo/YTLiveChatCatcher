using Microsoft.Web.WebView2.WinForms;

namespace YTLiveChatCatcher
{
    partial class FCookieLogin
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
            WebViewLogin = new WebView2();
            LManualCookie = new Label();
            TBManualCookie = new TextBox();
            CBRememberCookie = new CheckBox();
            LStatus = new Label();
            BtnConfirm = new Button();
            BtnLogout = new Button();
            BtnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)WebViewLogin).BeginInit();
            SuspendLayout();
            //
            // WebViewLogin
            //
            WebViewLogin.AllowExternalDrop = true;
            WebViewLogin.CreationProperties = null;
            WebViewLogin.DefaultBackgroundColor = Color.White;
            WebViewLogin.Location = new Point(12, 12);
            WebViewLogin.Name = "WebViewLogin";
            WebViewLogin.Size = new Size(860, 520);
            WebViewLogin.TabIndex = 0;
            WebViewLogin.AccessibleName = "YouTube 登入頁面";
            WebViewLogin.ZoomFactor = 1D;
            //
            // LManualCookie
            //
            LManualCookie.AutoSize = true;
            LManualCookie.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Bold);
            LManualCookie.Location = new Point(12, 544);
            LManualCookie.Name = "LManualCookie";
            LManualCookie.Size = new Size(320, 15);
            LManualCookie.TabIndex = 1;
            LManualCookie.Text = "或者手動貼上 Cookie 字串（適用於上方登入無法使用時）：";
            //
            // TBManualCookie
            //
            TBManualCookie.Location = new Point(12, 562);
            TBManualCookie.Name = "TBManualCookie";
            TBManualCookie.Size = new Size(860, 23);
            TBManualCookie.TabIndex = 2;
            TBManualCookie.AccessibleName = "手動貼上 Cookie 字串";
            //
            // CBRememberCookie
            //
            CBRememberCookie.AutoSize = true;
            CBRememberCookie.Location = new Point(12, 595);
            CBRememberCookie.Name = "CBRememberCookie";
            CBRememberCookie.Size = new Size(280, 19);
            CBRememberCookie.TabIndex = 3;
            CBRememberCookie.AccessibleName = "記住我";
            CBRememberCookie.Text = "記住我（以 Windows DPAPI 加密儲存在本機）";
            CBRememberCookie.UseVisualStyleBackColor = true;
            //
            // LStatus
            //
            LStatus.AutoSize = true;
            LStatus.Location = new Point(12, 622);
            LStatus.Name = "LStatus";
            LStatus.Size = new Size(65, 15);
            LStatus.TabIndex = 4;
            LStatus.Text = "尚未登入。";
            //
            // BtnConfirm
            //
            BtnConfirm.Location = new Point(12, 655);
            BtnConfirm.Name = "BtnConfirm";
            BtnConfirm.Size = new Size(160, 30);
            BtnConfirm.TabIndex = 5;
            BtnConfirm.AccessibleName = "使用以上登入內容";
            BtnConfirm.Text = "使用以上登入內容";
            BtnConfirm.UseVisualStyleBackColor = true;
            BtnConfirm.Click += BtnConfirm_Click;
            //
            // BtnLogout
            //
            BtnLogout.Location = new Point(184, 655);
            BtnLogout.Name = "BtnLogout";
            BtnLogout.Size = new Size(180, 30);
            BtnLogout.TabIndex = 6;
            BtnLogout.AccessibleName = "登出／清除已儲存資料";
            BtnLogout.Text = "登出／清除已儲存資料";
            BtnLogout.UseVisualStyleBackColor = true;
            BtnLogout.Click += BtnLogout_Click;
            //
            // BtnCancel
            //
            BtnCancel.Location = new Point(745, 655);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(127, 30);
            BtnCancel.TabIndex = 7;
            BtnCancel.AccessibleName = "取消";
            BtnCancel.Text = "取消";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            //
            // FCookieLogin
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 697);
            Controls.Add(BtnCancel);
            Controls.Add(BtnLogout);
            Controls.Add(BtnConfirm);
            Controls.Add(LStatus);
            Controls.Add(CBRememberCookie);
            Controls.Add(TBManualCookie);
            Controls.Add(LManualCookie);
            Controls.Add(WebViewLogin);
            AcceptButton = BtnConfirm;
            CancelButton = BtnCancel;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FCookieLogin";
            StartPosition = FormStartPosition.CenterParent;
            FormClosing += FCookieLogin_FormClosing;
            Load += FCookieLogin_Load;
            ((System.ComponentModel.ISupportInitialize)WebViewLogin).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private WebView2 WebViewLogin;
        private Label LManualCookie;
        private TextBox TBManualCookie;
        private CheckBox CBRememberCookie;
        private Label LStatus;
        private Button BtnConfirm;
        private Button BtnLogout;
        private Button BtnCancel;
    }
}
