namespace BlazorWinFormsApp
{
	partial class Form1
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
			groupBox1 = new System.Windows.Forms.GroupBox();
			_webViewActionButton = new System.Windows.Forms.Button();
			label1 = new System.Windows.Forms.Label();
			button1 = new System.Windows.Forms.Button();
			blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			option1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			sendScriptalertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			tabControl1 = new System.Windows.Forms.TabControl();
			tabPage1 = new System.Windows.Forms.TabPage();
			tabPage2 = new System.Windows.Forms.TabPage();
			customFilesBlazorWebView = new BlazorWpfApp.CustomFilesBlazorWebView();
			_useServicesButton = new System.Windows.Forms.Button();
			groupBox1.SuspendLayout();
			menuStrip1.SuspendLayout();
			tabControl1.SuspendLayout();
			tabPage1.SuspendLayout();
			tabPage2.SuspendLayout();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox1.Controls.Add(_useServicesButton);
			groupBox1.Controls.Add(_webViewActionButton);
			groupBox1.Controls.Add(label1);
			groupBox1.Controls.Add(button1);
			groupBox1.Location = new System.Drawing.Point(7, 24);
			groupBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new System.Windows.Forms.Padding(2, 1, 2, 1);
			groupBox1.Size = new System.Drawing.Size(568, 76);
			groupBox1.TabIndex = 10;
			groupBox1.TabStop = false;
			groupBox1.Text = "Native Windows Forms UI";
			// 
			// _webViewActionButton
			// 
			_webViewActionButton.Location = new System.Drawing.Point(306, 28);
			_webViewActionButton.Name = "_webViewActionButton";
			_webViewActionButton.Size = new System.Drawing.Size(106, 23);
			_webViewActionButton.TabIndex = 21;
			_webViewActionButton.Text = "WebView &alert";
			_webViewActionButton.UseVisualStyleBackColor = true;
			_webViewActionButton.Click += _webViewActionButton_Click;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(15, 32);
			label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(76, 15);
			label1.TabIndex = 10;
			label1.Text = "This is a label";
			// 
			// button1
			// 
			button1.Location = new System.Drawing.Point(116, 28);
			button1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			button1.Name = "button1";
			button1.Size = new System.Drawing.Size(184, 22);
			button1.TabIndex = 20;
			button1.Text = "&Click to see counter value";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// blazorWebView1
			// 
			blazorWebView1.Dock = System.Windows.Forms.DockStyle.Fill;
			blazorWebView1.Location = new System.Drawing.Point(3, 3);
			blazorWebView1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			blazorWebView1.Name = "blazorWebView1";
			blazorWebView1.Size = new System.Drawing.Size(554, 257);
			blazorWebView1.StartPath = "/";
			blazorWebView1.TabIndex = 20;
			// 
			// menuStrip1
			// 
			menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2 });
			menuStrip1.Location = new System.Drawing.Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new System.Drawing.Size(582, 24);
			menuStrip1.TabIndex = 21;
			menuStrip1.Text = "menuStrip1";
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { option1ToolStripMenuItem, sendScriptalertToolStripMenuItem });
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(93, 20);
			toolStripMenuItem1.Text = "&Menu options";
			// 
			// option1ToolStripMenuItem
			// 
			option1ToolStripMenuItem.Name = "option1ToolStripMenuItem";
			option1ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.D1;
			option1ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
			option1ToolStripMenuItem.Text = "Option &1";
			option1ToolStripMenuItem.Click += option1ToolStripMenuItem_Click;
			// 
			// sendScriptalertToolStripMenuItem
			// 
			sendScriptalertToolStripMenuItem.Name = "sendScriptalertToolStripMenuItem";
			sendScriptalertToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.A;
			sendScriptalertToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
			sendScriptalertToolStripMenuItem.Text = "Send script &alert";
			sendScriptalertToolStripMenuItem.Click += sendScriptalertToolStripMenuItem_Click;
			// 
			// toolStripMenuItem2
			// 
			toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutToolStripMenuItem });
			toolStripMenuItem2.Name = "toolStripMenuItem2";
			toolStripMenuItem2.Size = new System.Drawing.Size(44, 20);
			toolStripMenuItem2.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
			aboutToolStripMenuItem.Text = "About...";
			aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
			// 
			// tabControl1
			// 
			tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabControl1.Controls.Add(tabPage1);
			tabControl1.Controls.Add(tabPage2);
			tabControl1.Location = new System.Drawing.Point(7, 104);
			tabControl1.Name = "tabControl1";
			tabControl1.SelectedIndex = 0;
			tabControl1.Size = new System.Drawing.Size(568, 291);
			tabControl1.TabIndex = 22;
			// 
			// tabPage1
			// 
			tabPage1.Controls.Add(blazorWebView1);
			tabPage1.Location = new System.Drawing.Point(4, 24);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(560, 263);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "BlazorWebView";
			tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			tabPage2.Controls.Add(customFilesBlazorWebView);
			tabPage2.Location = new System.Drawing.Point(4, 24);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(560, 263);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Custom Files BlazorWebView";
			tabPage2.UseVisualStyleBackColor = true;
			// 
			// customFilesBlazorWebView
			// 
			customFilesBlazorWebView.Dock = System.Windows.Forms.DockStyle.Fill;
			customFilesBlazorWebView.Location = new System.Drawing.Point(3, 3);
			customFilesBlazorWebView.Name = "customFilesBlazorWebView";
			customFilesBlazorWebView.Size = new System.Drawing.Size(554, 257);
			customFilesBlazorWebView.StartPath = "/";
			customFilesBlazorWebView.TabIndex = 0;
			customFilesBlazorWebView.Text = "blazorWebView2";
			// 
			// _useServicesButton
			// 
			_useServicesButton.Location = new System.Drawing.Point(418, 28);
			_useServicesButton.Name = "_useServicesButton";
			_useServicesButton.Size = new System.Drawing.Size(106, 23);
			_useServicesButton.TabIndex = 22;
			_useServicesButton.Text = "Use &services";
			_useServicesButton.UseVisualStyleBackColor = true;
			_useServicesButton.Click += _useServicesButton_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(582, 407);
			Controls.Add(tabControl1);
			Controls.Add(groupBox1);
			Controls.Add(menuStrip1);
			MainMenuStrip = menuStrip1;
			Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			Name = "Form1";
			Text = "Blazor Web in Windows Forms";
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			tabControl1.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView1;
		private System.Windows.Forms.Button _webViewActionButton;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem option1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem sendScriptalertToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private BlazorWpfApp.CustomFilesBlazorWebView customFilesBlazorWebView;
		private System.Windows.Forms.Button _useServicesButton;
	}
}
