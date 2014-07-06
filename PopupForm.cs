//
// Copyright © 2004-2011 Kody Brown.
// All Rights Reserved.
//
// This program is unpublished proprietary source code of Bricksoft.com.
// You may not use or create derivative works from this code.
//
// Author: Kody Brown (kody@bricksoft.com)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Bricksoft.PowerCode;

namespace MenuStack
{
	public partial class PopupForm : System.Windows.Forms.Form
	{
		private string prodName = "Menu Stacker";
		private string envName = "menustack";
		private string envPrefix = "mstack_";

		private string[] arguments;

		public bool ShowDetails { get; set; }
		public bool ShowHiddenItems { get; set; }

		public bool WaitForHelp = false;

		private ToolStripTextBox searchBox;
		private System.Windows.Forms.Timer timer1;
		private Point mousePoint;

		/// <summary>
		/// Gets or sets the folder to load the menu items from.
		/// </summary>
		protected string FolderPath { get { return _folderPath ?? (_folderPath = String.Empty); } set { _folderPath = (value != null) ? value.Trim() : String.Empty; } }
		private string _folderPath { get; set; }

		/// <summary>
		/// Gets or sets whether sub folders should be displayed.
		/// </summary>
		protected bool subfolders { get; set; }

		/// <summary>
		/// Gets or sets whether the Control key was held during load.
		/// </summary>
		protected bool controlWasPressed { get; set; }

		/// <summary>
		/// Gets or sets the character (or key) that is used to sort menu items.
		/// i.e.: Where key is ']' -->  "C:\Shortcuts\10]lastfile.pdf", "C:\Shortcuts\01]firstfile.pdf", "C:\Shortcuts\5]secondfile.pdf", etc.
		/// </summary>
		protected string SortKey { get { return _sortKey ?? (_sortKey = String.Empty); } set { _sortKey = (value != null) ? value.Trim() : String.Empty; } }
		private string _sortKey { get; set; }

		/// <summary>
		/// Gets or sets the file pattern used to load the menu items for displaying.
		/// </summary>
		protected string FilePattern { get { return _filePattern ?? (_filePattern = String.Empty); } set { _filePattern = (value != null) ? value.Trim() : String.Empty; } }
		private string _filePattern { get; set; }

		/// <summary>
		/// Gets or sets whether the Search textbox is displayed.
		/// </summary>
		protected bool showSearch { get; set; }

		/// <summary>
		/// Gets or sets whether to combine folders and files together when sorting.
		/// </summary>
		protected bool combine { get; set; }

		/// <summary>
		/// Gets or sets whether to display each file's extension in the menu.
		/// </summary>
		protected bool showExts { get; set; }


		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="arguments"></param>
		public PopupForm( string[] arguments )
		{
			this.arguments = arguments;
			ShowDetails = true;
			ShowHiddenItems = false;

			if (Form.ModifierKeys == Keys.Control) {
				controlWasPressed = true;
			}

			InitializeComponent();

			this.KeyPreview = true;
			this.KeyDown += new KeyEventHandler(PopupForm_KeyDown);
		}

		private void PopupForm_Load( object sender, EventArgs e )
		{
			if (!ParseCommandLine(arguments)) {
				Application.Exit();
				return;
			}

			timer1 = new System.Windows.Forms.Timer();
			timer1.Enabled = false;
			timer1.Interval = 1000;
			timer1.Tick += new EventHandler(timer1_Tick);

			mousePoint = Cursor.Position;

			if (FolderPath == null || (FolderPath = FolderPath.Trim()).Length == 0) {
				MessageBox.Show(string.Format("The {0} is invalid", "path"), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				return;
			}

			contextMenuStrip1.Closed += new ToolStripDropDownClosedEventHandler(delegate( object delsender, ToolStripDropDownClosedEventArgs dele ) { Application.Exit(); });
			contextMenuStrip1.LostFocus += new EventHandler(delegate( object delsender, EventArgs dele ) { timer1.Stop(); });
			contextMenuStrip1.KeyDown += new KeyEventHandler(contextMenuStrip1_KeyDown);
			//contextMenuStrip1.PreviewKeyDown += new PreviewKeyDownEventHandler(contextMenuStrip1_PreviewKeyDown);

			//contextMenuStrip1.Items.Clear();
			//contextMenuStrip1.ImageList = new ImageList();
			//contextMenuStrip1.ImageList.Images.Add("find", Bitmap.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("MenuStack.Resources.find.png")));

			// Create the Search textbox.
			searchBox = new ToolStripTextBox("searchBox");
			searchBox.TextChanged += new EventHandler(searchBox_TextChanged);
			searchBox.KeyDown += new KeyEventHandler(searchBox_KeyDown);
			searchBox.GotFocus += new EventHandler(control_GotFocus);
			searchBox.BorderStyle = BorderStyle.Fixed3D;
			searchBox.Tag = "DONT_TOUCH";
			//searchBox.ImageIndex = 0;
			//searchBox.ImageKey = "find";
			contextMenuStrip1.Items.Add(searchBox);
			//searchBox.Dock = DockStyle.Fill;

			contextMenuStrip1.Items.Add(new ToolStripSeparator());

			// Load a menu item for each file in FolderPath.
			LoadMenu(contextMenuStrip1, FolderPath);

			// Add common menu items..
			contextMenuStrip1.Items.Add(new ToolStripSeparator());
			contextMenuStrip1.Items.Add(new ToolStripMenuItem("Open &Folder", null, openFolder_Click).AddTag("DONT_TOUCH"));
			contextMenuStrip1.Items.Add(new ToolStripMenuItem("Help", null, ShowHelp).AddTag("DONT_TOUCH"));
			contextMenuStrip1.Items.Add(new ToolStripMenuItem("&Close", null, delegate( object closeSender, EventArgs closeEvent ) { Application.Exit(); }).AddTag("DONT_TOUCH"));

			// Adjust the mousePoint slightly
			mousePoint = new Point(mousePoint.X - 20, mousePoint.Y);

			this.Opacity = 0;
			this.Location = mousePoint;

			contextMenuStrip1.Show(mousePoint);
		}

		private void PopupForm_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.KeyCode == Keys.Escape) {
				if (searchBox.Text.Length > 0) {
					searchBox.Text = String.Empty;
					e.SuppressKeyPress = true;
					e.Handled = true;
				}
			}
		}


		private void contextMenuStrip1_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.KeyCode == Keys.Escape
					|| e.Modifiers != 0
					|| e.KeyCode == Keys.Down
					|| e.KeyCode == Keys.Up) {
				// do nothing
			} else {
				if (showSearch) {
					//if ((e.KeyValue > 47 && e.KeyValue < 58)
					//        || (e.KeyValue > 64 && e.KeyValue < 91)
					//        || (e.KeyValue > 96 && e.KeyValue < 123)) {
					searchBox.Focus();
					searchBox.SelectionStart = searchBox.Text.Length;
					searchBox.SelectedText = ((char)e.KeyValue).ToString().ToLower();
					searchBox.SelectionStart = searchBox.Text.Length;
					e.SuppressKeyPress = true;
					e.Handled = true;
				}
			}
		}

		private void searchBox_KeyDown( object sender, KeyEventArgs e )
		{
			if ((e.Modifiers & Keys.Alt) == Keys.Alt) {
				// TODO: send the event back to the contextmenu!
				contextMenuStrip1.Focus();
			} else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up) {
				contextMenuStrip1.Focus();
				Thread.Sleep(150);
				if (e.KeyCode == Keys.Down) {
					contextMenuStrip1.Items[2].Select();
				} else if (e.KeyCode == Keys.Up) {
					contextMenuStrip1.Items[contextMenuStrip1.Items.Count - 1].Select();
				}
				e.SuppressKeyPress = true;
				e.Handled = true;
				//} else if (e.KeyValue > 96 && e.KeyValue < 123) {
				//    // Lower case upper case letters
				//    e.SuppressKeyPress = true;
				//    e.Handled = true;
				//    searchBox.SelectionStart = searchBox.Text.Length;
				//    searchBox.SelectedText = ((char)(e.KeyValue - 32)).ToString();
				//    searchBox.SelectionStart = searchBox.Text.Length;
			}
		}

		private void timer1_Tick( object sender, EventArgs e ) { Application.Exit(); }

		private void control_GotFocus( object sender, EventArgs e ) { timer1.Stop(); }

		/// <summary>
		/// Update the visible menu items based on the text entered.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void searchBox_TextChanged( object sender, EventArgs e )
		{
			Type t;
			Type matchType;
			//bool found;

			timer1.Stop();
			matchType = typeof(SortableToolStripMenuItem);

			if (searchBox.Text.Length == 0 && searchBox.Text.Trim().Length == 0) {
				foreach (ToolStripItem item in contextMenuStrip1.Items) {
					item.Visible = true;
				}
			} else {
				foreach (ToolStripItem item in contextMenuStrip1.Items) {
					t = item.GetType();
					if ((t == matchType || t.IsSubclassOf(matchType)) && item.Text != null && item.Tag != null && item.Tag.ToString() != "DONT_TOUCH") {
						if (item.Text.IndexOf(searchBox.Text, StringComparison.InvariantCultureIgnoreCase) > -1) {
							item.Visible = true;
						} else {
							item.Visible = false;
						}
					}
				}
			}

			// Hide duplicate separators when there is more than one in sequence.
			matchType = null;
			bool visible = false;

			foreach (ToolStripItem item in contextMenuStrip1.Items) {
				t = item.GetType();
				visible = true;

				if (item.Visible == true) {
					if (matchType != null && (matchType == typeof(ToolStripSeparator) || matchType.IsSubclassOf(typeof(ToolStripSeparator)))) {
						if (t == typeof(SortableToolStripSeparator) || t.IsSubclassOf(typeof(SortableToolStripSeparator))) {
							if (item.Tag == null || item.Tag.ToString() != "DONT_TOUCH") {
								visible = false;
							}
						}
					}
					item.Visible = visible;
					matchType = t;
				}
			}

			// Add a 'no files found' menu item
			//found = false;

			//foreach (ToolStripItem item in contextMenuStrip1.Items) {
			//    t = item.GetType();
			//    if (t != typeof(ToolStripSeparator) && !t.IsSubclassOf(typeof(ToolStripSeparator))) {
			//        if (item.Tag != null && item.Tag.ToString() != "DONT_TOUCH") {
			//            found = true;
			//            break;
			//        }
			//    }
			//}

			//if (!found) {
			//    contextMenuStrip1.Items.Insert(2, new ToolStripMenuItem("<no items found>")); 
			//}
		}

		private void openFolder_Click( object sender, EventArgs e )
		{
			MessageBox.Show(string.Format("Opening folder:\r\n\t{0}", FolderPath), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Information);

			if (null == FolderPath || 0 == FolderPath.Length) {
				return;
			}


			if (!Directory.Exists(FolderPath)) {
				MessageBox.Show(string.Format("Could not find folder:\r\n\t{0}", FolderPath), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try {
				Process p;
				ProcessStartInfo startInfo;

				startInfo = new ProcessStartInfo();
				startInfo.FileName = FolderPath;
				startInfo.Verb = "open";
				//startInfo.UseShellExecute = true;

				p = Process.Start(startInfo);

				Thread.Sleep(100);
			} catch (Exception ex) {
				MessageBox.Show(string.Format("Failed to open file:\r\n\t{0}", FolderPath) + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace, "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			Application.Exit();
		}

		private void item_Click( object sender, EventArgs e )
		{
			SortableToolStripMenuItem item;
			string filePath;

			item = sender as SortableToolStripMenuItem;
			if (item == null) {
				return;
			}

			filePath = item.Tag.ToString();
			if (!File.Exists(filePath)) {
				MessageBox.Show(string.Format("Could not find file:\r\n\t{0}", filePath), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try {
				Process p;
				ProcessStartInfo startInfo;

				startInfo = new ProcessStartInfo();
				startInfo.FileName = filePath;
				//startInfo.Verb = "open"; 
				//startInfo.UseShellExecute = true;

				p = Process.Start(startInfo);

				Thread.Sleep(100);
			} catch (Exception ex) {
				MessageBox.Show(string.Format("Failed to open file:\r\n\t{0}", filePath) + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace, "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			Application.Exit();
		}

		//private void contextMenuStrip1_KeyDown(object sender, KeyEventArgs e) {
		//   if (e.KeyCode == Keys.Escape) {
		//      if (searchBox.Text.Length > 0) {
		//         searchBox.Text = String.Empty;
		//         e.SuppressKeyPress = true;
		//         e.Handled = true;
		//      }
		//   } else if ((e.Modifiers & Keys.Alt) != Keys.Alt) {
		//      if ((e.KeyValue > 47 && e.KeyValue < 58)
		//            || (e.KeyValue > 64 && e.KeyValue < 91)
		//            || (e.KeyValue > 96 && e.KeyValue < 123)) {
		//         searchBox.Focus();
		//         searchBox.SelectionStart = searchBox.Text.Length;
		//         searchBox.SelectedText = ((char)e.KeyValue).ToString();
		//         //searchBox.SelectionStart = searchBox.Text.Length;
		//      }
		//      //e.SuppressKeyPress = true;
		//      //e.Handled = true;
		//   }
		//}


		private bool ParseCommandLine( string[] arguments )
		{
			string a, al;
			bool isopt;

			// default options
			subfolders = false;

			for (int i = 0; i < arguments.Length; i++) {
				a = arguments[i].Trim();

				if (a.StartsWith("/") || a.StartsWith("-") || a.StartsWith("!")) {
					isopt = true;
					while (a.StartsWith("/") || a.StartsWith("-")) {
						a = a.Substring(1);
					}
				} else {
					isopt = false;
				}

				al = a.ToLowerInvariant();

				if (a == "?" || al.StartsWith("h")) {
					ShowHelp();
					return false;
				}

				if (isopt) {
					//
					// Include subfolders automatically.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to false if not specified.
					// If specified but empty, sets to TRUE.
					//
					if (al.StartsWith("s") || al.Equals("r")) {
						subfolders = true;
						continue;
					} else if (al.StartsWith("!s") || al.Equals("!r")) {
						subfolders = false;
						continue;
					}

					//
					// Combine folder and files together during sort.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to false if not specified.
					// If specified but empty, sets to TRUE.
					//
					if (al.StartsWith("c")) {
						combine = true;
						continue;
					} else if (al.StartsWith("!c")) {
						combine = false;
						continue;
					}

					//
					// Show file extensions.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to true if not specified.
					// If specified but empty, sets to TRUE.
					//
					if (al.StartsWith("e")) {
						showExts = true;
						continue;
					} else if (al.StartsWith("!e")) {
						showExts = false;
						continue;
					}

					//
					// Show the Search textbox.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to TRUE if not specified.
					// If specified but empty, sets to TRUE.
					//
					if (al.StartsWith("hides")) {
						showSearch = false;
						continue;
					} else if (al.StartsWith("!hides")) {
						showSearch = true;
						continue;
					}

					//
					// The sort key.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to ] if not specified.
					// If specified but empty, sets to ].
					//
					if (al.StartsWith("k:")) {
						SortKey = a.Substring(2);
						continue;
					} else if (al.StartsWith("key:")) {
						SortKey = a.Substring(4);
						continue;
					}
					if (al.StartsWith("k")) {
						if (i < arguments.Length - 2) {
							SortKey = arguments[++i];
						} else {
							Console.Error.WriteLine("Missing second argument to: {0}", a);
						}
						continue;
					}

					//
					// File pattern.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to *.* if not specified.
					// If specified but empty, sets to *.*.
					//
					if (al.StartsWith("f:")) {
						FilePattern = a.Substring(2);
						continue;
					}
					if (al.StartsWith("f")) {
						if (i < arguments.Length - 2) {
							FilePattern = arguments[++i];
						} else {
							Console.Error.WriteLine("Missing second argument to: {0}", a);
						}
						continue;
					}
				} else {
					//
					// Folder path.
					// -----------------------------------------------------------------------------------------------------------------------------
					// Defaults to the current folder if not specified.
					//
					FolderPath = a;
				}
			}

			// Defaults and overrides

			if (controlWasPressed) {
				// The Control key forces the subfolders.
				subfolders = true;
			}

			if (SortKey.Trim().Length == 0) {
				SortKey = "]";
			}

			if (FilePattern.Trim().Length == 0) {
				FilePattern = "*.*;*-";
			} else {
				bool foundPattern = false;
				foreach (string pattern in FilePattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
					if (pattern == "*-") {
						foundPattern = true;
						break;
					}
				}
				if (!foundPattern) {
					FilePattern += ";*-";
				}
			}

			if (FolderPath.Trim().Length == 0) {
				FolderPath = Environment.CurrentDirectory;
			}
			if (!Directory.Exists(FolderPath)) {
				Console.Error.WriteLine("Directory was not found: {0}", FolderPath);
				return false;
			}

			return true;
		}

		private int LoadMenu( object parent, string folder )
		{
			SortableToolStripMenuItem item;
			Type t;
			ToolStripItemCollection items;

			SHFILEINFO shinfo;
			IntPtr imageHandle;
			uint flags;
			string text,
				sortName,
				lastText = "";
			DirectoryInfo folderInfo;
			List<DirectoryInfo> dirs;
			List<FileInfo> files;

			t = parent.GetType();
			if (t == typeof(ContextMenuStrip)) {
				items = ((ContextMenuStrip)parent).Items;
			} else if (t == typeof(SortableToolStripMenuItem)) {
				items = ((SortableToolStripMenuItem)parent).DropDownItems;
			} else {
				return 0;
			}

			shinfo = new SHFILEINFO();
			flags = Win32.SHGFI_TYPENAME | Win32.SHGFI_DISPLAYNAME | Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON | Win32.SHGFI_SYSICONINDEX;
			text = string.Empty;
			sortName = string.Empty;

			folderInfo = new DirectoryInfo(folder);
			if (!folderInfo.Exists) {
				MessageBox.Show(string.Format("The {0} is invalid", folderInfo.FullName), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				return 0;
			}

			if (subfolders) {
				dirs = new List<DirectoryInfo>(folderInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly));
				dirs.Sort(delegate( DirectoryInfo a, DirectoryInfo b )
				{
					return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
				});

				foreach (DirectoryInfo dir in dirs) {
					sortName = dir.Name;
					text = CleanName(dir.Name);

					if (text.Equals("-")) {
						items.Add(new SortableToolStripSeparator(sortName));
						continue;
					}

					item = new SortableToolStripMenuItem(text);
					item.SortName = sortName.Trim();
					item.Tag = dir.FullName;

					if (LoadMenu(item, dir.FullName) > 0) {
						items.Add(item);
					}
				}
			}

			files = new List<FileInfo>();

			foreach (string pattern in FilePattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
				foreach (FileInfo f in folderInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly)) {
					if (files.Find(fe => fe.FullName.Equals(f.FullName, StringComparison.CurrentCultureIgnoreCase)) == null) {
						files.Add(f);
					}
				}
			}

			files.Sort(delegate( FileInfo a, FileInfo b )
			{
				return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
			});

			foreach (FileInfo file in files) {
				if (file.Name.Equals("desktop.ini", StringComparison.InvariantCultureIgnoreCase)) {
					continue;
				} else if (file.Name.Equals("thumbs.db", StringComparison.InvariantCultureIgnoreCase)) {
					continue;
				}

				Icon icon;
				IntPtr PtrIcon;

				imageHandle = Win32.SHGetFileInfo(file.FullName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags); // Get the details of the file
				PtrIcon = Win32.ImageList_GetIcon(imageHandle, shinfo.iIcon.ToInt64(), Win32.ILD_TRANSPARENT);
				icon = Icon.FromHandle(PtrIcon);
				//icon = Icon.ExtractAssociatedIcon(file.FullName);

				sortName = file.Name;
				text = CleanName(file.Name).TrimEnd(".lnk");

				if (!showExts && text.IndexOf(".") > -1) {
					text = Path.GetFileNameWithoutExtension(text);
				}

				//
				// Allow 'macros' in the file name..
				//

				// If the file name has [#s] then it will 
				// only be displayed if subfolders is true.
				if (text.IndexOf("[#s]") > -1) {
					if (!subfolders) {
						continue;
					}
					text = text.Replace("[#s]", "").Trim();
				}

				//
				// If the file is only '-' characters, change it to just one '-' only.
				//
				//if (text.IndexOf("-") > -1) {
				//	string tmp = text;
				//	while (tmp.IndexOf("--") > -1) {
				//		tmp = tmp.Replace("--", "-");
				//	}
				//	if (tmp.Equals("-")) {
				//		text = tmp;
				//	}
				//}

				//
				// Create the menu item or separator
				//
				if (text.Equals("-")) {
					items.Add(new SortableToolStripSeparator(sortName));
					continue;
				} else {
					// Create the menu item for the file.
					if (icon != null && icon.Height > 0 && icon.Width > 0) {
						item = new SortableToolStripMenuItem(text, icon.ToBitmap());
						icon.Dispose();
					} else {
						item = new SortableToolStripMenuItem(text);
					}
					if (item == null) {
						continue;
					}

					item.SortName = sortName.Trim();
					item.Tag = file.FullName;
					item.Click += new EventHandler(item_Click);

					items.Add(item);
				}
			}

			// Sort the folders and files together alphabetically.
			if (combine) {
				SortItems(items);
			}

			if (showSearch) {
				searchBox_TextChanged(null, null);
			}

			// Remove duplicate separators.
			for (int i = items.Count - 1; i >= 0; i--) {
				if (items[i] is SortableToolStripSeparator) {
					if (lastText == "-") {
						items.RemoveAt(i);
					}
					lastText = "-";
					continue;
				}
				lastText = items[i].Text;
			}

			return items.Count;
		}

		private String CleanName( String text )
		{
			int pos;

			pos = text.IndexOf(SortKey);
			if (pos > -1) {
				text = text.Replace(SortKey + SortKey, "CUSTOMSORTKEY");
				pos = text.IndexOf(SortKey);
				if (pos > -1) {
					text = text.Substring(pos + 1).Trim();
				}
				text = text.Replace("CUSTOMSORTKEY", SortKey);
			}

			return text;
		}

		/// <summary>
		/// Sorts the ToolStripItemCollection collection.
		/// </summary>
		/// <param name="items"></param>
		private void SortItems( ToolStripItemCollection items )
		{
			List<ToolStripItem> specs;

			if (items == null || items.Count == 0) {
				return;
			}

			specs = new List<ToolStripItem>();

			foreach (ToolStripItem item in items) {
				specs.Add(item);
			}

			specs.Sort(delegate( ToolStripItem a, ToolStripItem b )
			{
				string sortA;
				string sortB;

				if (a.GetType().Equals(typeof(SortableToolStripMenuItem))) {
					sortA = ((SortableToolStripMenuItem)a).SortName;
				} else if (a.GetType().Equals(typeof(SortableToolStripSeparator))) {
					sortA = ((SortableToolStripSeparator)a).SortName;
				} else {
					sortA = string.Empty;
				}

				if (b.GetType().Equals(typeof(SortableToolStripMenuItem))) {
					sortB = ((SortableToolStripMenuItem)b).SortName;
				} else if (b.GetType().Equals(typeof(SortableToolStripSeparator))) {
					sortB = ((SortableToolStripSeparator)b).SortName;
				} else {
					sortB = string.Empty;
				}

				return string.Compare(sortA, sortB, StringComparison.InvariantCultureIgnoreCase);
			});

			items.Clear();

			foreach (ToolStripItem item in specs) {
				items.Add(item);
			}
		}


		private void ShowHelp( object sender, EventArgs e ) { ShowHelp(); }

		private void ShowHelp()
		{
			WaitForHelp = true;

			ContentForm f = new ContentForm();
			RichTextBoxEx r = f.richTextControl;

			Font normalFont = new System.Drawing.Font("Courier New", 10, FontStyle.Regular);
			Font sectionFont = new System.Drawing.Font("Courier New", 10, FontStyle.Bold);

			Color normalColor = Color.FromArgb(70, 70, 70);
			Color sectionColor = Color.Black;

			r.DetectUrls = true;
			r.WordWrap = false;
			r.Font = normalFont;
			r.ForeColor = normalColor;

			r.LinkClicked += delegate( object sender, LinkClickedEventArgs e )
			{
				string cmd;
				if (e.LinkText.Trim() == "@wasatchwizard") {
					cmd = "http://wasatchwizard.com/";
				} else {
					cmd = e.LinkText;
				}
				System.Diagnostics.Process.Start(cmd);
			};

			r.SelectionColor = sectionColor;
			r.SelectionFont = sectionFont;
			r.SelectedText = (string.Format("{0}.exe - {1}", envName, prodName));

			r.SelectionColor = normalColor;
			r.SelectionFont = normalFont;
			r.SelectedText = ("\nCreated 2008-2013 ");
			int start = r.SelectionStart;
			//r.InsertLink("http://wasatchwizard.com");
			r.SelectedText = (".");


			r.SelectedText = ("\nNo warranties expressed or implied. Use at your own risk.");
			r.SelectedText = ("\n");

			r.SelectionColor = sectionColor;
			r.SelectionFont = sectionFont;
			r.SelectedText = ("\nUSAGE:");
			r.SelectedText = ("\n");

			r.ForeColor = normalColor;
			r.SelectedText = (string.Format("\n  {0}.exe [options] [path] ", envName));
			r.SelectedText = ("\n");

			r.SelectionColor = sectionColor;
			r.SelectionFont = sectionFont;
			r.SelectedText = ("\nOPTIONS:");
			r.SelectedText = ("\n");

			r.SelectionColor = normalColor;
			r.SelectionFont = normalFont;
			r.SelectedText = ("\n  path           The full path to the folder containing files to be displayed in the menu. If not specified it will use the current / working directory.");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n  /sub           Include sub-folders. Press and hold the Ctrl key during start to force `/sub`.");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n  /key:X         Used in sorting. the full filename is used to sort, when key is specified, everything before and including the key is removed before displaying in the menu.");
			r.SelectedText = ("\n                 For instance when --key is ']' the following files:");
			r.SelectedText = ("\n                   '02] file1.pdf', '01] file2.pdf'");
			r.SelectedText = ("\n                 they will be displayed like:");
			r.SelectedText = ("\n                   'file2.pdf', 'file1.pdf'");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n  /file:ptrn     Specifies which files will be displayed in the menu. The default is `*.*`.");
			r.SelectedText = ("\n                 You can also combine patterns by using the `;` character.");
			r.SelectedText = ("\n                 For example: /file=*.sln, /file=*.pdf;*.epub, /file=form*.txt");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n  /combine       Combine and sort folders and files together. The default behavior is not combined.)");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n  /ext           Show file extensions. The default behavior is to hide the file extensions.)");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n  /search        Show the search textbox. The default behavior is to show the search box.)");
			r.SelectedText = ("\n");
			//r.SelectedText = ("\n  /set           Displays the current environment variables then exits. All other options are ignored.");
			//r.SelectedText = ("\n");

			r.SelectionColor = sectionColor;
			r.SelectionFont = sectionFont;
			r.SelectedText = ("\nNOTES:");
			r.SelectedText = ("\n");

			r.SelectionColor = normalColor;
			r.SelectionFont = normalFont;
			r.SelectedText = ("\n * The '-', '--', and '/' option prefixes can be used interchangeably.");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n * Prefix options with a '!' to indicate the opposite value (option prefix not necessary). For example use `!combine` to not combine.");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n * Surround file pattern option with '/' (begining and end) to indicate");
			r.SelectedText = ("\n   the file pattern is a regular expression.");
			r.SelectedText = ("\n   For instance: `--file /.*\\.min$` would only include files that ");
			r.SelectedText = ("\n   end in `.js` and not include `*.min.js");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n * Boolean options (options start with a /) can include an equal sign with");
			r.SelectedText = ("\n   true or false (without spaces). for instance: /pause=false or /v=true, etc.");
			r.SelectedText = ("\n");
			r.SelectedText = ("\n * You can also prefix a ! in front of boolean options to indicate false.");
			r.SelectedText = ("\n   This overrides all other values for that option. for instance: /!search, /!sub.");
			r.SelectedText = ("\n");

			r.SelectionColor = normalColor;
			r.SelectionFont = normalFont;

			//r.SelectedText = ( "\n");
			//r.SelectedText = ( ("  ENVIRONMENT VARIABLES:");
			//r.SelectedText = ( "\n");
			//r.SelectedText = ( (string.Format("     {0}path=C:\\xyz             sets -path 'C:\\xyz'", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}find=xyz                sets -find 'xyz'", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}file=*.*                sets -file '*.*'", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}recursive=true|false    sets /r or /!r", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}matchcase=true|false    sets /m or /!m", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}verbose=true|false      sets /v or /!v", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}wholeword=true|false    sets /w or /!w", envPrefix));
			//r.SelectedText = ( (string.Format("     {0}pause=true|false        sets /pause or /!pause", envPrefix));
			//r.SelectedText = ( "\n");
			//r.SelectedText = ( ("     *command-line arguments override environment variables");

			r.InsertLink("@wasatchwizard", start);
			r.SelectionLength = 0;
			r.SelectionStart = 0;

			f.Title = "MenuStack Usage";
			f.ShowDialog();
		}




		private void ShowSetVars( RichTextBox r )
		{
			StringBuilder result;
			List<DictionaryEntry> entries;
			int maxChars;

			result = new StringBuilder();

			r.SelectedText = ("\n");
			result.Append("Environment Variables: ");
			r.SelectedText = ("\n");

			maxChars = 0;
			entries = new List<DictionaryEntry>();

			foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables()) {
				if (entry.Key.ToString().StartsWith(envPrefix)) {
					entries.Add(entry);
					maxChars = Math.Max(maxChars, entry.Key.ToString().Length);
				}
			}

			foreach (DictionaryEntry entry in entries) {
				result.Append(string.Format("  {0}{1} = {2}", entry.Key, DupeChar(' ', maxChars - entry.Key.ToString().Length), entry.Value));
			}

			r.SelectedText = ("\n");

			MessageBox.Show(result.ToString(), "Environment Variables");
		}

		private string DupeChar( char ch, int len )
		{
			StringBuilder result;

			result = new StringBuilder();

			for (int i = 0; i < len; i++) {
				result.Append(ch);
			}

			return result.ToString();
		}


	}
}
