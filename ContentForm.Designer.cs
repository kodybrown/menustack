namespace MenuStack
{
	partial class ContentForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
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
		private void InitializeComponent()
		{
			this.richText = new MenuStack.RichTextBoxEx();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// richText
			// 
			this.richText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richText.HideSelection = false;
			this.richText.Location = new System.Drawing.Point(12, 12);
			this.richText.Name = "richText";
			this.richText.Size = new System.Drawing.Size(418, 133);
			this.richText.TabIndex = 0;
			this.richText.Text = "";
			this.richText.WordWrap = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(184, 151);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// ContentForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(442, 186);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.richText);
			this.KeyPreview = true;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(450, 220);
			this.Name = "ContentForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MenuStack Usage";
			this.Load += new System.EventHandler(this.ContentForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ContentForm_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ContentForm_KeyUp);
			this.ResumeLayout(false);

		}

		#endregion

		private MenuStack.RichTextBoxEx richText;
		private System.Windows.Forms.Button okButton;
	}
}