namespace MonoMod.Installer {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ProgressPanel = new MonoMod.Installer.CustomPanel();
            this.ProgressProgressLabel = new MonoMod.Installer.CustomLabel();
            this.MainPanel = new MonoMod.Installer.CustomPanel();
            this.MainVersionPanel = new MonoMod.Installer.CustomPanel();
            this.MainVersionList = new MonoMod.Installer.CustomListBox();
            this.MainVersionLabel = new MonoMod.Installer.CustomLabel();
            this.MainPathPanel = new MonoMod.Installer.CustomPanel();
            this.MainPathBox = new MonoMod.Installer.CustomTextBox();
            this.MainUninstallButton = new MonoMod.Installer.CustomButton();
            this.MainBrowseButton = new MonoMod.Installer.CustomButton();
            this.MainExeStatusLabel = new MonoMod.Installer.CustomLabel();
            this.MainInstallButton = new MonoMod.Installer.CustomButton();
            this.MainStep1Label = new MonoMod.Installer.CustomLabel();
            this.HeaderPanel = new MonoMod.Installer.CustomPanel();
            this.MinimizeButton = new MonoMod.Installer.CustomButton();
            this.CloseButton = new MonoMod.Installer.CustomButton();
            this.HeaderPicture = new MonoMod.Installer.CustomPictureBox();
            this.ProgressPanel.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.MainVersionPanel.SuspendLayout();
            this.MainPathPanel.SuspendLayout();
            this.HeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // ProgressPanel
            // 
            this.ProgressPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ProgressPanel.Controls.Add(this.ProgressProgressLabel);
            this.ProgressPanel.Location = new System.Drawing.Point(461, 130);
            this.ProgressPanel.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
            this.ProgressPanel.Name = "ProgressPanel";
            this.ProgressPanel.Size = new System.Drawing.Size(458, 469);
            this.ProgressPanel.TabIndex = 4;
            // 
            // ProgressProgressLabel
            // 
            this.ProgressProgressLabel.AutoSize = true;
            this.ProgressProgressLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ProgressProgressLabel.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgressProgressLabel.ForeColor = System.Drawing.Color.White;
            this.ProgressProgressLabel.Location = new System.Drawing.Point(32, 4);
            this.ProgressProgressLabel.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.ProgressProgressLabel.Name = "ProgressProgressLabel";
            this.ProgressProgressLabel.Size = new System.Drawing.Size(73, 21);
            this.ProgressProgressLabel.TabIndex = 6;
            this.ProgressProgressLabel.Text = "Progress:";
            // 
            // MainPanel
            // 
            this.MainPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.MainPanel.Controls.Add(this.MainVersionPanel);
            this.MainPanel.Controls.Add(this.MainVersionLabel);
            this.MainPanel.Controls.Add(this.MainPathPanel);
            this.MainPanel.Controls.Add(this.MainUninstallButton);
            this.MainPanel.Controls.Add(this.MainBrowseButton);
            this.MainPanel.Controls.Add(this.MainExeStatusLabel);
            this.MainPanel.Controls.Add(this.MainInstallButton);
            this.MainPanel.Controls.Add(this.MainStep1Label);
            this.MainPanel.Location = new System.Drawing.Point(1, 130);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(458, 469);
            this.MainPanel.TabIndex = 1;
            // 
            // MainVersionPanel
            // 
            this.MainVersionPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.MainVersionPanel.Controls.Add(this.MainVersionList);
            this.MainVersionPanel.Location = new System.Drawing.Point(32, 114);
            this.MainVersionPanel.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.MainVersionPanel.Name = "MainVersionPanel";
            this.MainVersionPanel.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.MainVersionPanel.Size = new System.Drawing.Size(382, 303);
            this.MainVersionPanel.TabIndex = 6;
            // 
            // MainVersionList
            // 
            this.MainVersionList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.MainVersionList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MainVersionList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainVersionList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.MainVersionList.ForeColor = System.Drawing.Color.White;
            this.MainVersionList.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.MainVersionList.HighlightForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.MainVersionList.Location = new System.Drawing.Point(8, 4);
            this.MainVersionList.Name = "MainVersionList";
            this.MainVersionList.Size = new System.Drawing.Size(366, 295);
            this.MainVersionList.TabIndex = 6;
            this.MainVersionList.SelectedIndexChanged += new System.EventHandler(this.MainVersionList_SelectedIndexChanged);
            // 
            // MainVersionLabel
            // 
            this.MainVersionLabel.AutoSize = true;
            this.MainVersionLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.MainVersionLabel.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainVersionLabel.ForeColor = System.Drawing.Color.White;
            this.MainVersionLabel.Location = new System.Drawing.Point(32, 89);
            this.MainVersionLabel.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.MainVersionLabel.Name = "MainVersionLabel";
            this.MainVersionLabel.Size = new System.Drawing.Size(156, 21);
            this.MainVersionLabel.TabIndex = 7;
            this.MainVersionLabel.Text = "Step 2: Select Version";
            // 
            // MainPathPanel
            // 
            this.MainPathPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.MainPathPanel.Controls.Add(this.MainPathBox);
            this.MainPathPanel.Location = new System.Drawing.Point(32, 29);
            this.MainPathPanel.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.MainPathPanel.Name = "MainPathPanel";
            this.MainPathPanel.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.MainPathPanel.Size = new System.Drawing.Size(340, 31);
            this.MainPathPanel.TabIndex = 5;
            // 
            // MainPathBox
            // 
            this.MainPathBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.MainPathBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MainPathBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPathBox.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainPathBox.ForeColor = System.Drawing.Color.White;
            this.MainPathBox.Location = new System.Drawing.Point(8, 4);
            this.MainPathBox.Name = "MainPathBox";
            this.MainPathBox.ReadOnly = true;
            this.MainPathBox.Size = new System.Drawing.Size(324, 22);
            this.MainPathBox.TabIndex = 0;
            // 
            // MainUninstallButton
            // 
            this.MainUninstallButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MainUninstallButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.MainUninstallButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MainUninstallButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(119)))), ((int)(((byte)(105)))), ((int)(((byte)(134)))));
            this.MainUninstallButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MainUninstallButton.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainUninstallButton.ForeColor = System.Drawing.Color.White;
            this.MainUninstallButton.Location = new System.Drawing.Point(291, 427);
            this.MainUninstallButton.Name = "MainUninstallButton";
            this.MainUninstallButton.Size = new System.Drawing.Size(123, 31);
            this.MainUninstallButton.TabIndex = 4;
            this.MainUninstallButton.Text = "Uninstall";
            this.MainUninstallButton.UseVisualStyleBackColor = false;
            this.MainUninstallButton.Click += new System.EventHandler(this.MainUninstallButton_Click);
            // 
            // MainBrowseButton
            // 
            this.MainBrowseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MainBrowseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.MainBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MainBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(119)))), ((int)(((byte)(105)))), ((int)(((byte)(134)))));
            this.MainBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MainBrowseButton.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainBrowseButton.ForeColor = System.Drawing.Color.White;
            this.MainBrowseButton.Location = new System.Drawing.Point(382, 29);
            this.MainBrowseButton.Name = "MainBrowseButton";
            this.MainBrowseButton.Size = new System.Drawing.Size(32, 31);
            this.MainBrowseButton.TabIndex = 2;
            this.MainBrowseButton.Text = "...";
            this.MainBrowseButton.UseVisualStyleBackColor = false;
            this.MainBrowseButton.Click += new System.EventHandler(this.MainBrowseButton_Click);
            // 
            // MainExeStatusLabel
            // 
            this.MainExeStatusLabel.AutoSize = true;
            this.MainExeStatusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.MainExeStatusLabel.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainExeStatusLabel.ForeColor = System.Drawing.Color.White;
            this.MainExeStatusLabel.Location = new System.Drawing.Point(32, 64);
            this.MainExeStatusLabel.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.MainExeStatusLabel.Name = "MainExeStatusLabel";
            this.MainExeStatusLabel.Size = new System.Drawing.Size(0, 21);
            this.MainExeStatusLabel.TabIndex = 3;
            // 
            // MainInstallButton
            // 
            this.MainInstallButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MainInstallButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.MainInstallButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MainInstallButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(119)))), ((int)(((byte)(105)))), ((int)(((byte)(134)))));
            this.MainInstallButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MainInstallButton.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainInstallButton.ForeColor = System.Drawing.Color.White;
            this.MainInstallButton.Location = new System.Drawing.Point(32, 427);
            this.MainInstallButton.Name = "MainInstallButton";
            this.MainInstallButton.Size = new System.Drawing.Size(249, 31);
            this.MainInstallButton.TabIndex = 3;
            this.MainInstallButton.Text = "Step 3: Install";
            this.MainInstallButton.UseVisualStyleBackColor = false;
            this.MainInstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // MainStep1Label
            // 
            this.MainStep1Label.AutoSize = true;
            this.MainStep1Label.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.MainStep1Label.Font = new System.Drawing.Font("Selawik Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainStep1Label.ForeColor = System.Drawing.Color.White;
            this.MainStep1Label.Location = new System.Drawing.Point(32, 4);
            this.MainStep1Label.Margin = new System.Windows.Forms.Padding(1, 4, 1, 0);
            this.MainStep1Label.Name = "MainStep1Label";
            this.MainStep1Label.Size = new System.Drawing.Size(121, 21);
            this.MainStep1Label.TabIndex = 0;
            this.MainStep1Label.Text = "Step 1: Select {0}";
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.HeaderPanel.Controls.Add(this.MinimizeButton);
            this.HeaderPanel.Controls.Add(this.CloseButton);
            this.HeaderPanel.Controls.Add(this.HeaderPicture);
            this.HeaderPanel.Location = new System.Drawing.Point(1, 1);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Size = new System.Drawing.Size(458, 128);
            this.HeaderPanel.TabIndex = 0;
            // 
            // MinimizeButton
            // 
            this.MinimizeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MinimizeButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.MinimizeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.MinimizeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(119)))), ((int)(((byte)(105)))), ((int)(((byte)(134)))));
            this.MinimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MinimizeButton.Image = global::MonoMod.Installer.Properties.Resources.minimize;
            this.MinimizeButton.Location = new System.Drawing.Point(397, 8);
            this.MinimizeButton.Margin = new System.Windows.Forms.Padding(1, 8, 0, 0);
            this.MinimizeButton.Name = "MinimizeButton";
            this.MinimizeButton.Size = new System.Drawing.Size(26, 26);
            this.MinimizeButton.TabIndex = 2;
            this.MinimizeButton.UseVisualStyleBackColor = false;
            this.MinimizeButton.Click += new System.EventHandler(this.MinimizeButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.CloseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(59)))), ((int)(((byte)(45)))), ((int)(((byte)(74)))));
            this.CloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(119)))), ((int)(((byte)(105)))), ((int)(((byte)(134)))));
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Image = global::MonoMod.Installer.Properties.Resources.close;
            this.CloseButton.Location = new System.Drawing.Point(424, 8);
            this.CloseButton.Margin = new System.Windows.Forms.Padding(1, 8, 8, 0);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(26, 26);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // HeaderPicture
            // 
            this.HeaderPicture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.HeaderPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeaderPicture.Location = new System.Drawing.Point(0, 0);
            this.HeaderPicture.Name = "HeaderPicture";
            this.HeaderPicture.Size = new System.Drawing.Size(458, 128);
            this.HeaderPicture.TabIndex = 0;
            this.HeaderPicture.TabStop = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(920, 600);
            this.Controls.Add(this.ProgressPanel);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.HeaderPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(460, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ProgressPanel.ResumeLayout(false);
            this.ProgressPanel.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.MainVersionPanel.ResumeLayout(false);
            this.MainPathPanel.ResumeLayout(false);
            this.MainPathPanel.PerformLayout();
            this.HeaderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HeaderPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CustomPanel HeaderPanel;
        private CustomPanel MainPanel;
        private CustomPictureBox HeaderPicture;
        private CustomLabel MainStep1Label;
        private CustomButton MainInstallButton;
        private CustomLabel MainExeStatusLabel;
        private CustomButton MainBrowseButton;
        private CustomPanel ProgressPanel;
        private CustomButton MainUninstallButton;
        private CustomPanel MainPathPanel;
        private CustomTextBox MainPathBox;
        private CustomLabel ProgressProgressLabel;
        private CustomListBox MainVersionList;
        private CustomLabel MainVersionLabel;
        private CustomPanel MainVersionPanel;
        private CustomButton CloseButton;
        private CustomButton MinimizeButton;
    }
}

