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
		/// <summary>
		/// Gets the command-line arguments;
		/// </summary>
		private CommandLineArguments args = null;

		private string prodName = "Menu Stacker";
		private string envName = "menustack";
		private string envPrefix = "mstack_";

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
			if (Form.ModifierKeys == Keys.Control) {
				controlWasPressed = true;
			}
			InitializeComponent();
			ParseCommandLine(arguments);
			this.KeyPreview = true;
			this.KeyDown += new KeyEventHandler(PopupForm_KeyDown);
		}

		private void PopupForm_Load( object sender, EventArgs e )
		{
			if (args.Contains("h", "?", "help")) {
				ShowUsage(true);
				Application.Exit();
				return;
			}

			timer1 = new System.Windows.Forms.Timer();
			timer1.Enabled = false;
			timer1.Interval = 250;
			timer1.Tick += new EventHandler(timer1_Tick);

			mousePoint = Cursor.Position;

			if (FolderPath == null || (FolderPath = FolderPath.Trim()).Length == 0) {
				MessageBox.Show(Loc.GetText("The {0} is invalid", "path"), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			contextMenuStrip1.Items.Add(new ToolStripMenuItem(Loc.GetText("Open &Folder"), null, openFolder_Click).AddTag("DONT_TOUCH"));
			contextMenuStrip1.Items.Add(new ToolStripMenuItem(Loc.GetText("&Close"), null, delegate( object closeSender, EventArgs closeEvent ) { Application.Exit(); }).AddTag("DONT_TOUCH"));

			// Adjust the mousePoint slightly
			mousePoint = new Point(mousePoint.X - 20, mousePoint.Y);

			this.Opacity = 0;
			this.Location = mousePoint;

			contextMenuStrip1.Show(mousePoint);
		}

		private void ParseCommandLine( string[] arguments )
		{
			args = new CommandLineArguments(arguments);

			//
			// Include subfolders automatically.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to false if not specified.
			// If specified but empty, sets to TRUE.
			//
			if (args.Contains("!s", "!sub")) {
				subfolders = false;
			} else if (args.Contains("s", "sub")) {
				subfolders = args.GetBoolean(true, "s", "sub");
			} else {
				subfolders = false;
			}
			// Override argument/envvar if Control key is held while starting.
			if (controlWasPressed) {
				subfolders = true;
			}

			//
			// Combine folder and files together during sort.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to false if not specified.
			// If specified but empty, sets to TRUE.
			//
			if (args.Contains("!c", "!combine")) {
				combine = false;
			} else if (args.Contains("c", "combine")) {
				combine = args.GetBoolean(true, "c", "combine");
			} else {
				combine = false;
			}

			//
			// Show file extensions.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to true if not specified.
			// If specified but empty, sets to TRUE.
			//
			if (args.Contains("!e", "!ext")) {
				showExts = false;
			} else if (args.Contains("e", "ext")) {
				showExts = args.GetBoolean(true, "e", "ext");
			} else {
				showExts = true;
			}

			//
			// Show the Search textbox.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to TRUE if not specified.
			// If specified but empty, sets to TRUE.
			//
			if (args.Contains("!search")) {
				showSearch = false;
			} else if (args.Contains("search")) {
				showSearch = args.GetBoolean(true, "search");
			} else {
				showSearch = true;
			}

			//
			// The sort key.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to ] if not specified.
			// If specified but empty, sets to ].
			//
			if (args.Contains("k", "key")) {
				SortKey = args.GetString("]", "k", "key");
			} else {
				SortKey = "]";
			}

			//
			// Include subfolders automatically.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to false if not specified.
			// If specified but empty, sets to TRUE.
			//
			if (args.Contains("f", "files")) {
				FilePattern = args.GetString("*.*", "f", "files");
			} else {
				FilePattern = "*.*";
			}

			//
			// Folder path.
			// -----------------------------------------------------------------------------------------------------------------------------
			// Defaults to the current folder if not specified.
			//
			if (args.HasValue(CommandLineArguments.UnnamedItem + "1")) {
				FolderPath = args.GetString(Environment.CurrentDirectory, CommandLineArguments.UnnamedItem + "1");
			} else {
				FolderPath = Environment.CurrentDirectory;
			}

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

		private int LoadMenu( object parent, string folder )
		{
			SortableToolStripMenuItem item;
			Type t;
			ToolStripItemCollection items;

			SHFILEINFO shinfo;
			IntPtr imageHandle;
			uint flags;
			string text;
			string sortName;
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
				MessageBox.Show(Loc.GetText("The {0} is invalid", folderInfo.FullName), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				return 0;
			}

			if (subfolders) {
				dirs = new List<DirectoryInfo>(folderInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly));
				dirs.Sort(delegate( DirectoryInfo a, DirectoryInfo b ) {
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

			files = new List<FileInfo>(folderInfo.GetFiles(FilePattern, SearchOption.TopDirectoryOnly));
			files.Sort(delegate( FileInfo a, FileInfo b ) {
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

				if (text.Equals("-")) {
					items.Add(new SortableToolStripSeparator(sortName));
					continue;
				}

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

			searchBox_TextChanged(null, null);

			// Sort the folders and files together alphabetically.
			if (combine) {
				SortItems(items);
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

			specs.Sort(delegate( ToolStripItem a, ToolStripItem b ) {
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

		private void openFolder_Click( object sender, EventArgs e )
		{
			if (null == FolderPath || 0 == FolderPath.Length) {
				return;
			}

			if (!Directory.Exists(FolderPath)) {
				MessageBox.Show(Loc.GetText("Could not find folder:\r\n\t{0}", FolderPath), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try {
				Process p;
				ProcessStartInfo startInfo;

				startInfo = new ProcessStartInfo();
				startInfo.FileName = FolderPath;
				//startInfo.Verb = "open"; 
				//startInfo.UseShellExecute = true;

				p = Process.Start(startInfo);

				Thread.Sleep(100);
			} catch (Exception ex) {
				MessageBox.Show(Loc.GetText("Failed to open file:\r\n\t{0}", FolderPath) + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace, "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				MessageBox.Show(Loc.GetText("Could not find file:\r\n\t{0}", filePath), "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				MessageBox.Show(Loc.GetText("Failed to open file:\r\n\t{0}", filePath) + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace, "MenuStack", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			Application.Exit();
		}


		/// <summary>
		/// Displays the help screen.
		/// </summary>
		/// <param name="showDetails"></param>
		private void ShowUsage( bool showDetails ) { ShowUsage(showDetails, false); }

		/// <summary>
		/// Displays the help screen.
		/// </summary>
		/// <param name="showDetails"></param>
		/// <param name="showHidden"></param>
		private void ShowUsage( bool showDetails, bool showHidden )
		{
			StringBuilder result;
			ConsoleColor originalColor;
			ConsoleColor hiddenColor;

			result = new StringBuilder();

			if (!showDetails) {
				result.AppendLine(Loc.GetText(string.Empty));
				result.AppendLine(Loc.GetText("type '{0}.exe /?' for help", envName));
				return;
			}

			originalColor = Console.ForegroundColor;
			hiddenColor = ConsoleColor.Cyan;

			result.AppendLine(Loc.GetText(string.Empty));
			result.AppendLine(Loc.GetText("{0}.exe - {1}", envName, prodName));
			result.AppendLine(Loc.GetText("Copyright (C) 2008-2010 {0}. All Rights Reserved.", "Bricksoft.com"));
			result.AppendLine(Loc.GetText("No warranties expressed or implied. Use at your own risk."));
			result.AppendLine(Loc.GetText(string.Empty));

			result.AppendLine(Loc.GetText("Usage:"));
			result.AppendLine(Loc.GetText("  {0}.exe [options] [\"path\"] ", envName));
			result.AppendLine(Loc.GetText(string.Empty));
			result.AppendLine(Loc.GetText("     {0}     the full path to the folder containing files to be displayed in the menu", "path"));
			result.AppendLine(Loc.GetText("              (defaults to current directory if not specified)"));
			result.AppendLine(Loc.GetText(string.Empty));
			result.AppendLine(Loc.GetText("  options:"));
			result.AppendLine(Loc.GetText("     /sub[=true|false]       include sub-folders"));
			result.AppendLine(Loc.GetText("                             (press and hold the Ctrl key during start to force /sub.)"));
			result.AppendLine(Loc.GetText("     /key=X                  used in sorting. the full filename is used to sort,"));
			result.AppendLine(Loc.GetText("                             when key is specified, everything before and including"));
			result.AppendLine(Loc.GetText("                             the key is removed before displaying in the menu."));
			result.AppendLine(Loc.GetText("                             for instance when /key is \"]\" --> "));
			result.AppendLine(Loc.GetText("                               \"02] file1.pdf\", \"01] file2.pdf\" displays \"file2.pdf\", \"file1.pdf\""));
			result.AppendLine(Loc.GetText("                             (also accepts /sort. default is empty.)"));
			result.AppendLine(Loc.GetText("     /files=wildcard         specifies which files will be displayed in the menu."));
			result.AppendLine(Loc.GetText("                             for instance: /files=*.sln, /files=*.pdf, /files=form*.txt"));
			result.AppendLine(Loc.GetText("                             (also accepts /f. default is \"*.*\".)"));
			result.AppendLine(Loc.GetText("     /combine[=true|false]   combine and sort folders and files together."));
			result.AppendLine(Loc.GetText("                             (also accepts /c. default behavior is false.)"));
			result.AppendLine(Loc.GetText("     /ext[=true|false]       show file extensions."));
			result.AppendLine(Loc.GetText("                             (also accepts /x. default behavior is false.)"));
			result.AppendLine(Loc.GetText("     /search[=true|false]    show the search textbox."));
			result.AppendLine(Loc.GetText("                             (default behavior is true.)"));
			result.AppendLine(Loc.GetText(string.Empty));

			result.AppendLine(Loc.GetText(string.Empty));
			result.AppendLine(Loc.GetText("     /set               displays the current environment variables"));
			result.AppendLine(Loc.GetText("                        then exits. All other options are ignored."));
			result.AppendLine(Loc.GetText(string.Empty));

			result.AppendLine(Loc.GetText("     *use ! to set any option to opposite value. overrides option."));
			result.AppendLine(Loc.GetText("      for example use /!w to not search for whole word."));
			result.AppendLine(Loc.GetText(string.Empty));


			if (showHidden) {
				Console.ForegroundColor = hiddenColor;
				result.AppendLine(Loc.GetText("     *Prefix the files option with \"regex:\" to indicate the text is a regular"));
				result.AppendLine(Loc.GetText("      expression. for instance: -files"));
				result.AppendLine(Loc.GetText(string.Empty));
				result.AppendLine(Loc.GetText("     *Boolean options (options start with a /) can include an equal sign with"));
				result.AppendLine(Loc.GetText("      true or false (without spaces). for instance: /pause=false or /v=true, etc."));
				result.AppendLine(Loc.GetText("     *You can also prefix a ! in front of boolean options to indicate false."));
				result.AppendLine(Loc.GetText("      This overrides all other values for that option. for instance: /!search, /!sub."));
				result.AppendLine(Loc.GetText(string.Empty));
				Console.ForegroundColor = originalColor;
			}

			result.AppendLine(Loc.GetText("  environment variables:"));
			result.AppendLine(Loc.GetText(string.Empty));
			result.AppendLine(Loc.GetText("     {0}path=C:\\xyz             sets -path \"C:\\xyz\"", envPrefix));
			result.AppendLine(Loc.GetText("     {0}find=xyz                sets -find \"xyz\"", envPrefix));
			result.AppendLine(Loc.GetText("     {0}file=*.*                sets -file \"*.*\"", envPrefix));
			result.AppendLine(Loc.GetText("     {0}recursive=true|false    sets /r or /!r", envPrefix));
			result.AppendLine(Loc.GetText("     {0}matchcase=true|false    sets /m or /!m", envPrefix));
			result.AppendLine(Loc.GetText("     {0}verbose=true|false      sets /v or /!v", envPrefix));
			result.AppendLine(Loc.GetText("     {0}wholeword=true|false    sets /w or /!w", envPrefix));
			result.AppendLine(Loc.GetText("     {0}pause=true|false        sets /pause or /!pause", envPrefix));
			result.AppendLine(Loc.GetText(string.Empty));
			result.AppendLine(Loc.GetText("     *command-line arguments override environment variables"));
			result.AppendLine(Loc.GetText(string.Empty));

			MessageBox.Show(result.ToString(), "Command-line Usage Help");
		}


		private void ShowSetVars()
		{
			StringBuilder result;
			List<DictionaryEntry> entries;
			int maxChars;

			result = new StringBuilder();

			result.Append(Loc.GetText(string.Empty));
			result.Append(Loc.GetText("Environment Variables: "));
			result.Append(Loc.GetText(string.Empty));

			maxChars = 0;
			entries = new List<DictionaryEntry>();

			foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables()) {
				if (entry.Key.ToString().StartsWith(envPrefix)) {
					entries.Add(entry);
					maxChars = Math.Max(maxChars, entry.Key.ToString().Length);
				}
			}

			foreach (DictionaryEntry entry in entries) {
				result.Append(Loc.GetText("  {0}{1} = {2}", entry.Key, DupeChar(' ', maxChars - entry.Key.ToString().Length), entry.Value));
			}


			result.Append(Loc.GetText(string.Empty));

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
