using System;
using System.Windows.Forms;

namespace MenuStack
{
	public partial class ContentForm : Form
	{
		public string Title { get; set; }
		public string RtfContent { get; set; }
		public string TextContent { get; set; }
		public bool AutoWrap { get; set; }

		public RichTextBoxEx richTextControl { get { return richText; } }

		public ContentForm()
		{
			InitializeComponent();

			Title = "";
			RtfContent = "";
			TextContent = "";
			AutoWrap = true;
		}

		private void ContentForm_Load( object sender, EventArgs e )
		{
			Text = Title ?? "";

			//richText.Clear();
			richText.ReadOnly = true;

			//if (RtfContent.Length > 0) {
			//	richText.SelectedRtf = RtfContent;
			//} else {
			//	richText.SelectedText = TextContent ?? "<no content specified>";
			//}

			//richText.SelectionStart = 0;
			//richText.SelectionLength = 0;

			if (AutoWrap) {
				WrapContent();
			}
		}

		private void ContentForm_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.KeyCode == Keys.Cancel || e.KeyCode == Keys.Return) {
				Close();
			}
		}

		private void ContentForm_KeyUp( object sender, KeyEventArgs e )
		{
			//if (e.KeyCode == Keys.Cancel || e.KeyCode == Keys.Return) {
			//	Close();
			//}
		}

		private void WrapContent()
		{

		}
	}
}
