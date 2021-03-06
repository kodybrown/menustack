//
// Copyright (C) 2002-2013 Kody Brown (kody@bricksoft.com).
// 
// MIT License:
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Represents a console-based application with numerous
	/// utilities and helper methods.
	/// </summary>
	public abstract class ConsoleApplication
	{
		delegate void Func();

		#region DLLImports, Structs, Low-Level stuff, etc.

		private int hConsoleHandle;
		private const int STD_OUTPUT_HANDLE = -11;
		private const byte EMPTY = 32;

		[StructLayout(LayoutKind.Sequential)]
		struct COORD
		{
			public short x;
			public short y;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct SMALL_RECT
		{
			public short Left;
			public short Top;
			public short Right;
			public short Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct CONSOLE_SCREEN_BUFFER_INFO
		{
			public COORD dwSize;
			public COORD dwCursorPosition;
			public int wAttributes;
			public SMALL_RECT srWindow;
			public COORD dwMaximumWindowSize;
		}

		[DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern int GetStdHandle( int nStdHandle );

		[DllImport("kernel32.dll", EntryPoint = "FillConsoleOutputCharacter", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern int FillConsoleOutputCharacter( int hConsoleOutput, byte cCharacter, int nLength, COORD dwWriteCoord, ref int lpNumberOfCharsWritten );

		[DllImport("kernel32.dll", EntryPoint = "GetConsoleScreenBufferInfo", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern int GetConsoleScreenBufferInfo( int hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo );

		[DllImport("kernel32.dll", EntryPoint = "SetConsoleCursorPosition", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern int SetConsoleCursorPosition( int hConsoleOutput, COORD dwCursorPosition );

		#endregion

		#region Application Properties

		/// <summary>
		/// Gets or sets the author's name.
		/// </summary>
		public string AuthorName
		{
			get { return _authorName ?? (_authorName = ""); }
			protected set
			{
				if (value != null) {
					_authorName = value;
					_authorNameSet = true;
				}
			}
		}
		private string _authorName = "";
		private bool _authorNameSet = false;

		/// <summary>
		/// Gets or sets the author's email address.
		/// </summary>
		public string AuthorEmail
		{
			get { return _authorEmail ?? (_authorEmail = ""); }
			protected set
			{
				if (value != null) {
					_authorEmail = value;
					_authorEmailSet = true;
				}
			}
		}
		private string _authorEmail = "";
		private bool _authorEmailSet = false;

		///// <summary>
		///// Gets or sets the author's web address.
		///// </summary>
		//public string AuthorWebsite
		//{
		//	get { return _authorWebsite ?? (_authorWebsite = ""); }
		//	protected set
		//	{
		//		if (value != null) {
		//			_authorWebsite = value;
		//			_authorWebsiteSet = true;
		//		}
		//	}
		//}
		//private string _authorWebsite = "";
		//private bool _authorWebsiteSet = false;

		/// <summary>
		/// Gets or sets the author's twitter name.
		/// </summary>
		public string AuthorTwitter
		{
			get { return _authorTwitter ?? (_authorTwitter = ""); }
			protected set
			{
				_authorTwitter = (value != null) ? value.Trim() : "";
				_authorTwitterSet = true;
			}
		}
		private string _authorTwitter = "";
		private bool _authorTwitterSet = false;

		/// <summary>
		/// Gets or sets the application name.
		/// If the name is not manually set by the subclass, then the name
		/// specified in the assembly's metadata is returned.
		/// </summary>
		public string AppName
		{
			get { return _appNameSet ? _appName : _readAppName; }
			protected set
			{
				if (value != null) {
					_appName = value.Trim();
					_appNameSet = true;
				}
			}
		}
		private string _appName = "";
		private string _readAppName = "";
		private bool _appNameSet = false;

		/// <summary>
		/// Gets or sets the application description.
		/// If the description is not manually set by the subclass, then the
		/// description specified in the assembly's metadata is returned.
		/// </summary>
		public string AppDescription
		{
			get { return _appDescriptionSet ? _appDescription : _readAppDescription; }
			protected set
			{
				if (value != null) {
					_appDescription = value.Trim();
					_appDescriptionSet = true;
				}
			}
		}
		private string _appDescription = "";
		private string _readAppDescription = "";
		private bool _appDescriptionSet = false;

		/// <summary>
		/// Gets or sets the application copyright.
		/// If the copyright is not manually set by the subclass, then the
		/// copyright specified in the assembly's metadata is returned.
		/// </summary>
		public string AppCopyright
		{
			get { return _appCopyrightSet ? _appCopyright : _readAppCopyright; }
			protected set
			{
				if (value != null) {
					_appCopyright = value.Trim();
					_appCopyrightSet = true;
				}
			}
		}
		private string _appCopyright = "";
		private string _readAppCopyright = "";
		private bool _appCopyrightSet = false;

		/// <summary>
		/// Gets the application version major number (ie: version AppMajor.AppMinor.AppBuild.AppRevision).
		/// </summary>
		public int AppMajor
		{
			get { return _appMajor; }
			private set { _appMajor = value; }
		}
		private int _appMajor = 0;

		/// <summary>
		/// Gets the application version minor number (ie: version AppMajor.AppMinor.AppBuild.AppRevision).
		/// </summary>
		public int AppMinor
		{
			get { return _appMinor; }
			private set { _appMinor = value; }
		}
		private int _appMinor = 0;

		/// <summary>
		/// Gets the application version build number (ie: version AppMajor.AppMinor.AppBuild.AppRevision).
		/// </summary>
		public int AppBuild
		{
			get { return _appBuild; }
			private set { _appBuild = value; }
		}
		private int _appBuild = 0;

		/// <summary>
		/// Gets the application version revision number (ie: version AppMajor.AppMinor.AppBuild.AppRevision).
		/// </summary>
		public int AppRevision
		{
			get { return _appRevision; }
			private set { _appRevision = value; }
		}
		private int _appRevision = 0;

		/// <summary>
		/// Gets or sets the application's website (for instance,
		/// a company's product page or a github project url).
		/// </summary>
		public string AppWebsite
		{
			get { return _appWebsite; }
			protected set
			{
				if (value != null) {
					_appWebsite = value;
					_appWebsiteSet = true;
				}
			}
		}
		private string _appWebsite = "";
		private bool _appWebsiteSet = false;

		/// <summary>
		/// Gets or sets the application's license.
		/// </summary>
		public string AppLicense
		{
			get { return _appLicense; }
			protected set
			{
				if (value != null) {
					_appLicense = value;
					_appLicenseSet = true;
				}
			}
		}
		private string _appLicense = "";
		private bool _appLicenseSet = false;

		/// <summary>
		/// Gets or sets the url to learn more about the application's license.
		/// </summary>
		public string AppLicenseUrl
		{
			get { return _appLicenseUrl; }
			protected set
			{
				if (value != null) {
					_appLicenseUrl = value;
					_appLicenseUrlSet = true;
				}
			}
		}
		private string _appLicenseUrl = "";
		private bool _appLicenseUrlSet = false;

		/// <summary>
		/// Sets application info properties to defaults and
		/// sets some of them from the current assembly (file) definition.
		/// </summary>
		protected void SetApplicationVariables()
		{
			FileVersionInfo fi;

			fi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

			_authorName = "author";
			_authorEmail = "author@email";
			//_authorWebsite = "author-website";
			_authorTwitter = "author-twitter";

			_readAppName = Path.GetFileNameWithoutExtension(fi.OriginalFilename);
			_readAppDescription = fi.FileDescription; //fi.Comments
			_readAppCopyright = fi.LegalCopyright;

			_appMajor = fi.FileMajorPart;
			_appMinor = fi.FileMinorPart;
			_appBuild = fi.ProductBuildPart;
			_appRevision = fi.ProductPrivatePart;

			_appLicense = "license";
			_appLicenseUrl = "license-url";
		}

		/// <summary>
		/// Gets the application's environment variable prefix.
		/// </summary>
		public string AppEnvPrefix
		{
			get { return (_appEnvPrefixSet ? _appEnvPrefix : AppName) + "_"; }
			protected set
			{
				if (value != null) {
					_appEnvPrefix = value.TrimEnd("_");
					_appEnvPrefixSet = true;
				}
			}
		}
		private string _appEnvPrefix = "";
		private bool _appEnvPrefixSet = false;

		/// <summary>
		/// 
		/// </summary>
		public virtual string ResourcePath { get { return "Resources"; } }

		/// <summary>
		/// 
		/// </summary>
		public int ErrorCode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Provides a parameter name for what command-line argument the error occurred on..
		/// TODO
		/// </summary>
		public string ErrorHelpContext { get; set; }

		/// <summary>
		/// Gets the applications current directory.
		/// </summary>
		protected DirectoryInfo CurPath { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		protected Settings settings { get; private set; }

		#endregion

		#region Default Console Colors

		/// <summary>
		/// 
		/// </summary>
		public ConsoleColor normalColor = Console.ForegroundColor;

		/// <summary>
		/// 
		/// </summary>
		public ConsoleColor errorColor = ConsoleColor.Red;

		/// <summary>
		/// 
		/// </summary>
		public ConsoleColor highlightColor = ConsoleColor.White;

		/// <summary>
		/// 
		/// </summary>
		public ConsoleColor hiddenColor = ConsoleColor.Cyan;

		#endregion

		private int __minFlagPadLen = 19;
		private int __maxFlagPadLen = 30;

		protected string[] _arguments;

		#region Default Command-line Argument Properties

		/// <summary>
		/// Gets whether debug information is output to the login or not.
		/// </summary>
		public CommandLineArg<bool> debug
		{
			get
			{
				if (_debug == null) {
					_debug = new CommandLineArg<bool>();
					_debug.Name = "debug";
					_debug.Description = "Displays debug/internal processing details.";
					_debug.DisplayMode = DisplayMode.Hidden;
					_debug.Options = CommandLineArgumentOptions.NameOnly;
					_debug.Value = true;
					_debug.Keys = new string[] { "debug" };
					_debug.Default = false;
					_debug.Group = -10;
					_debug.SortIndex = 0;
					_debug.Enabled = true;
					_debug.AllowConfig = true;
					_debug.AllowEnvironmentVariable = false;
				}
				return _debug;
			}
			protected set { _debug = value; }
		}
		private CommandLineArg<bool> _debug = null;

		/// <summary>
		/// Gets whether to display the help information or not.
		/// </summary>
		public CommandLineArg<string> help
		{
			get
			{
				if (_help == null) {
					_help = new CommandLineArg<string>();
					_help.Name = "help";
					_help.Description = "Displays the help information.\nUse '--help cmd' to show details about 'cmd'.";
					_help.HelpContent = "{this.Description}\n\n {this.Keys}";
					_help.DisplayMode = DisplayMode.Always;
					_help.Options = CommandLineArgumentOptions.NameValueOptional;
					_help.Keys = new string[] { "?", "help", "usage" };
					_help.Group = -10;
					_help.SortIndex = -10;
					_help.Enabled = true;
					_help.AllowEnvironmentVariable = false;
				}
				return _help;
			}
			protected set { _help = value; }
		}
		private CommandLineArg<string> _help = null;

		/// <summary>
		/// Gets whether to display the application information when run or not.
		/// </summary>
		public CommandLineArg<bool> nologo
		{
			get
			{
				if (_nologo == null) {
					_nologo = new CommandLineArg<bool>();
					_nologo.Name = "nologo";
					_nologo.Description = "Prevents displaying application information when run.";
					_nologo.DisplayMode = DisplayMode.Always;
					// For NameOnly arguments, the .Value is left untouched when the argument is present.
					// If the argument is not present, .Value is set to .Default.
					_nologo.Options = CommandLineArgumentOptions.NameOnly;
					_nologo.Value = true;
					_nologo.Keys = new string[] { "nologo" };
					_nologo.Default = false;
					_nologo.Group = -10;
					_nologo.SortIndex = 0;
					_nologo.Enabled = true;
					_nologo.AllowConfig = true;
					_nologo.AllowEnvironmentVariable = false;
				}
				return _nologo;
			}
			protected set { _nologo = value; }
		}
		private CommandLineArg<bool> _nologo = null;

		/// <summary>
		/// Gets the version of the application.
		/// </summary>
		public CommandLineArg<bool> version
		{
			get
			{
				if (_version == null) {
					_version = new CommandLineArg<bool>();
					_version.Name = "version";
					_version.Description = "Displays the version.";
					_version.DisplayMode = DisplayMode.Always;
					_version.Options = CommandLineArgumentOptions.NameOnly;
					_version.Value = true;
					_version.Keys = new string[] { "version", "ver" };
					_version.Group = -10;
					_version.SortIndex = -9;
					_version.Enabled = true;
					_version.AllowEnvironmentVariable = false;
				}
				return _version;
			}
			protected set { _version = value; }
		}
		private CommandLineArg<bool> _version = null;

		/// <summary>
		/// Gets whether to pause before exiting.
		/// </summary>
		public CommandLineArg<bool> pause
		{
			get
			{
				if (_pause == null) {
					_pause = new CommandLineArg<bool>();
					_pause.Name = "pause";
					_pause.Description = "Pauses before exiting.";
					_pause.HelpContent = "{this.Description}\n\n {this.Keys}";
					_pause.DisplayMode = DisplayMode.Always;
					_pause.Options = CommandLineArgumentOptions.NameOnly;
					_pause.Value = true;
					_pause.Keys = new string[] { "pause", "p" };
					_pause.Default = false;
					_pause.Group = -10;
					_pause.SortIndex = 0;
					_pause.Enabled = true;
					_pause.AllowConfig = true;
					_pause.AllowEnvironmentVariable = false;
				}
				return _pause;
			}
			protected set { _pause = value; }
		}
		private CommandLineArg<bool> _pause = null;
		//public bool AllowPauseCommand { get { return pause.Enabled; } set { pause.Enabled = value; } }

		/// <summary>
		/// Displays verbose details as the application outputs information.
		/// </summary>
		public CommandLineArg<bool> verbose
		{
			get
			{
				if (_verbose == null) {
					_verbose = new CommandLineArg<bool>();
					_verbose.Name = "verbose";
					_verbose.Description = "Displays verbose details as the application outputs information.";
					_verbose.DisplayMode = DisplayMode.Always;
					_verbose.Options = CommandLineArgumentOptions.NameOnly;
					_verbose.Value = true;
					_verbose.Keys = new string[] { "verbose", "v" };
					_verbose.Default = false;
					_verbose.Group = -10;
					_verbose.SortIndex = 0;
					_verbose.Enabled = false;
					_verbose.AllowConfig = true;
				}
				return _verbose;
			}
			protected set { _verbose = value; }
		}
		private CommandLineArg<bool> _verbose = null;
		//public bool AllowVerboseCommand { get { return verbose.Enabled; } set { verbose.Enabled = value; } }

		/// <summary>
		/// Gets or sets whether all output should be prevented (opposite of verbose).
		/// </summary>
		public CommandLineArg<bool> quiet
		{
			get
			{
				if (_quiet == null) {
					_quiet = new CommandLineArg<bool>();
					_quiet.Name = "quiet";
					_quiet.Description = "Hides all output (opposite of verbose).";
					_quiet.DisplayMode = DisplayMode.Always;
					_quiet.Options = CommandLineArgumentOptions.NameOnly;
					_quiet.Value = true;
					_quiet.Keys = new string[] { "quiet", "q" };
					_quiet.Default = false;
					_quiet.Group = -10;
					_quiet.SortIndex = 0;
					_quiet.Enabled = false;
					_quiet.AllowConfig = true;
				}
				return _quiet;
			}
			protected set { _quiet = value; }
		}
		private CommandLineArg<bool> _quiet = null;
		//public bool AllowQuietCommand { get { return quiet.Enabled; } set { quiet.Enabled = value; } }

		/// <summary>
		/// 
		/// </remarks>
		public CommandLineArg<bool> hidden
		{
			get
			{
				if (_hidden == null) {
					_hidden = new CommandLineArg<bool>();
					_hidden.Name = "hidden";
					_hidden.Description = "Displays hidden and/or advanced features.";
					_hidden.DisplayMode = DisplayMode.Hidden;
					_hidden.Options = CommandLineArgumentOptions.NameOnly;
					_hidden.Value = true;
					_hidden.Keys = new string[] { "hidden" };
					_hidden.Default = false;
					_hidden.Group = -10;
					_hidden.SortIndex = 0;
					_hidden.Enabled = true;
					_hidden.AllowEnvironmentVariable = false;
				}
				return _hidden;
			}
			protected set { _hidden = value; }
		}
		private CommandLineArg<bool> _hidden = null;
		//public bool AllowHiddenCommand { get { return hidden.Enabled; } set { hidden.Enabled = value; } }

		/// <summary>
		/// Gets whether to display the current environment variables or not.
		/// </summary>
		public CommandLineArg<bool> envars
		{
			get
			{
				if (_envars == null) {
					_envars = new CommandLineArg<bool>();
					_envars.Name = "env";
					_envars.Description = "Displays the current environment variables.";
					_envars.DisplayMode = DisplayMode.Hidden;
					_envars.Options = CommandLineArgumentOptions.NameOnly;
					_envars.Value = true;
					_envars.Keys = new string[] { "env", "set" };
					_envars.Default = false;
					_envars.Group = -10;
					_envars.SortIndex = 10;
					_envars.Enabled = false;
					_envars.AllowEnvironmentVariable = false;
				}
				return _envars;
			}
			protected set { _envars = value; }
		}
		private CommandLineArg<bool> _envars = null;
		//protected bool AllowEnvironmentVariables { get { return envars.Enabled; } set { envars.Enabled = value; } }

		/// <summary>
		/// Gets whether to process items recursively.
		/// </summary>
		public CommandLineArg<bool> recursive
		{
			get
			{
				// TODO create a property of the CommandLineArg() class that stores
				//      which of the keys were used on the command-line..
				if (_recursive == null) {
					_recursive = new CommandLineArg<bool>();
					_recursive.Name = "recursive";
					_recursive.Description = "Processes items recursively.";
					_recursive.DisplayMode = DisplayMode.Always;
					_recursive.Options = CommandLineArgumentOptions.NameValueOptional;
					_recursive.Keys = new string[] { "recursive", "r", "s" };
					_recursive.Default = false;
					_recursive.Group = 0;
					_recursive.SortIndex = 0;
					_recursive.Enabled = false;
					_recursive.AllowConfig = true;
				}
				return _recursive;
			}
			protected set { _recursive = value; }
		}
		private CommandLineArg<bool> _recursive = null;
		//protected bool AllowRecursive { get { return recursive.Enabled; } set { recursive.Enabled = value; } }

		/// <summary>
		/// Gets whether to process items ignoreCasely.
		/// </summary>
		public CommandLineArg<bool> ignoreCase
		{
			get
			{
				if (_ignoreCase == null) {
					_ignoreCase = new CommandLineArg<bool>();
					_ignoreCase.Name = "ignoreCase";
					_ignoreCase.Description = "Whether to ignore case or not.";
					_ignoreCase.DisplayMode = DisplayMode.Always;
					_ignoreCase.Options = CommandLineArgumentOptions.NameValueOptional;
					_ignoreCase.Keys = new string[] { "ignoreCase", "i" };
					_ignoreCase.Default = false;
					_ignoreCase.Group = 0;
					_ignoreCase.SortIndex = 0;
					_ignoreCase.Enabled = false;
					_ignoreCase.AllowConfig = true;
				}
				return _ignoreCase;
			}
			protected set { _ignoreCase = value; }
		}
		private CommandLineArg<bool> _ignoreCase = null;
		//protected bool AllowIgnoreCase { get { return ignoreCase.Enabled; } set { ignoreCase.Enabled = value; } }

		public CommandLineArg<bool> emailArg
		{
			get
			{
				if (_email == null) {
					_email = new CommandLineArg<bool>();
					_email.Name = "email";
					_email.Description = "Creates an email addressed to the application author.";
					_email.DisplayMode = DisplayMode.Hidden;
					_email.Options = CommandLineArgumentOptions.NameValueOptional;
					_email.Value = true;
					_email.Keys = new string[] { "authemail" };
					_email.Default = false;
					_email.Group = -5;
					_email.Enabled = true;
					_email.AllowEnvironmentVariable = false;
				}
				return _email;
			}
			protected set { _email = value; }
		}
		private CommandLineArg<bool> _email = null;

		public CommandLineArg<bool> websiteArg
		{
			get
			{
				if (_website == null) {
					_website = new CommandLineArg<bool>();
					_website.Name = "website";
					_website.Description = "Opens the application's website.";
					_website.DisplayMode = DisplayMode.Hidden;
					_website.Options = CommandLineArgumentOptions.NameValueOptional;
					_website.Value = true;
					_website.Keys = new string[] { "website" };
					_website.Default = false;
					_website.Group = -5;
					_website.Enabled = true;
					_website.AllowEnvironmentVariable = false;
				}
				return _website;
			}
			protected set { _website = value; }
		}
		private CommandLineArg<bool> _website = null;

		public CommandLineArg<bool> licenseArg
		{
			get
			{
				if (_license == null) {
					_license = new CommandLineArg<bool>();
					_license.Name = "license";
					_license.Description = "Opens the url for the application's license.";
					_license.DisplayMode = DisplayMode.Hidden;
					_license.Options = CommandLineArgumentOptions.NameValueOptional;
					_license.Value = true;
					_license.Keys = new string[] { "license" };
					_license.Default = false;
					_license.Group = -5;
					_license.Enabled = true;
					_license.AllowEnvironmentVariable = false;
				}
				return _license;
			}
			protected set { _license = value; }
		}
		private CommandLineArg<bool> _license = null;

		/// <summary>
		/// Gets whether to apply the command-line arguments to the config/settings file.
		/// If the config argument is all by itself (no other arguments are specified) then the current config values are displayed.
		/// </summary>
		public CommandLineArg<string> config
		{
			get
			{
				if (_config == null) {
					_config = new CommandLineArg<string>();
					_config.Name = "config";
					_config.Description = "Either displays the saved config values or writes the current command-line arguments to config, depending on 'cmd'.";//\nIf 'cmd' is empty, 'read' is assumed.";
					_config.ExpressionLabel = "'cmd'";
					// If the value of 'config' is an empty string, it will be set to Default.
					_config.ExpressionsAllowed = new Dictionary<string, string>();
					_config.ExpressionsAllowed.Add("", "If 'cmd' is omitted, 'read' will be assumed.");
					_config.ExpressionsAllowed.Add("read", "Displays the saved config values.");
					_config.ExpressionsAllowed.Add("write", "Saves the current command-line arguments to config.");
					_config.DisplayMode = DisplayMode.Always;
					_config.Options = CommandLineArgumentOptions.NameValueOptional;
					_config.Keys = new string[] { "config" };
					_config.Default = "read";
					_config.Group = -9;
					_config.SortIndex = 0;
					_config.Enabled = true;
					_config.AllowEnvironmentVariable = false;
					_config.AllowConfig = false;
					//_config.Validate += delegate( CommandLine cmdLine, CommandLineArg<string> arg )
					//{
					//	if (cmdLine.OriginalCmdLine.Length == 1) {
					//		arg.Value = "read";
					//	} else {
					//		arg.Value = "write";
					//	}
					//	arg.HasValue = true;
					//	return new Result(0);
					//};
				}
				return _config;
			}
			protected set { _config = value; }
		}
		private CommandLineArg<string> _config = null;
		//protected bool AllowConfig { get { return config.Enabled; } set { config.Enabled = value; } }

		#endregion

		#region Console Properties

		/// <summary>
		/// Gets or sets the command-line arguments for the current console application.
		/// </summary>
		public CommandLine CmdLine { get { return cmdLine; } set { cmdLine = value; } }
		/// <summary>
		/// 
		/// </summary>
		protected CommandLine cmdLine = null;

		/// <summary>
		/// Gets a value indicating whether the CAPS LOCK keyboard toggle is turned on or turned off.
		/// </summary>
		public bool CapsLock { get { return Console.CapsLock; /*overrideCapsLock*/ } /*set { overrideCapsLock = value; }*/ }

		/// <summary>
		/// Gets a value indicating whether the combination of the System.ConsoleModifiers.Control modifier key and the System.ConsoleKey.C console key (Ctrl+C) is treated as ordinary input or as an interruption that is handled by the operating system.
		/// </summary>
		public bool TreatControlCAsInput { get { return Console.TreatControlCAsInput; } set { Console.TreatControlCAsInput = value; } }

		/// <summary>
		/// Gets a value indicating whether the NUM LOCK keyboard toggle is turned on or turned off.
		/// </summary>
		public bool NumberLock { get { return Console.NumberLock; } }

		/// <summary>
		/// Gets a value indicating whether a key press is available in the input stream;
		/// </summary>
		public bool KeyAvailable { get { return Console.KeyAvailable; } }

		/// <summary>
		/// Gets or sets the background color of the console.
		/// </summary>
		public ConsoleColor BackgroundColor { get { return Console.BackgroundColor; } set { Console.BackgroundColor = value; } }

		/// <summary>
		/// Gets or sets the foreground color of the console.
		/// </summary>
		public ConsoleColor ForegroundColor { get { return Console.ForegroundColor; } set { Console.ForegroundColor = value; } }

		/// <summary>
		/// Gets or sets the height of the buffer area.
		/// </summary>
		public int BufferHeight { get { return Console.BufferHeight; } set { Console.BufferHeight = value; } }

		/// <summary>
		/// Gets or sets the width of the buffer area.
		/// </summary>
		public int BufferWidth { get { return Console.BufferWidth; } set { Console.BufferWidth = value; } }

		/// <summary>
		/// Gets or sets the column position of the cursor within the buffer area.
		/// </summary>
		public int CursorLeft { get { return Console.CursorLeft; } set { Console.CursorLeft = value; } }

		/// <summary>
		/// Gets or sets the row position of the cursor within the buffer area.
		/// </summary>
		public int CursorTop { get { return Console.CursorTop; } set { Console.CursorTop = value; } }

		/// <summary>
		/// Gets or sets the height of the cursor within a character cell.
		/// </summary>
		public int CursorSize { get { return Console.CursorSize; } set { Console.CursorSize = value; } }

		/// <summary>
		/// Gets or sets a value indicating whether the cursor is visible.
		/// </summary>
		public bool CursorVisible { get { return Console.CursorVisible; } set { Console.CursorVisible = value; } }

		/// <summary>
		/// Gets the largest value of console window rows, based on the current font and screen resolution.
		/// </summary>
		public int LargestWindowHeight { get { return Console.LargestWindowHeight; } }

		/// <summary>
		/// Gets the largest value of console window columns, based on the current font and screen resolution.
		/// </summary>
		public int LargestWindowWidth { get { return Console.LargestWindowWidth; } }

		/// <summary>
		/// Gets or sets the title to display in the console title bar.
		/// </summary>
		public string Title { get { return Console.Title; } set { Console.Title = (null != value) ? value : string.Empty; } }

		/// <summary>
		/// Gets or sets the height of the console window.
		/// </summary>
		public int WindowHeight { get { return Console.WindowHeight; } set { Console.WindowHeight = value; } }

		/// <summary>
		/// Gets or sets the leftmost position of the console window area relative to the screen buffer.
		/// </summary>
		public int WindowLeft { get { return Console.WindowLeft; } set { Console.WindowLeft = value; } }

		/// <summary>
		/// Gets or sets the top position of the console window area relative to the screen buffer.
		/// </summary>
		public int WindowTop { get { return Console.WindowTop; } set { Console.WindowTop = value; } }

		/// <summary>
		/// Gets or sets the width of the console window.
		/// </summary>
		public int WindowWidth { get { return Console.WindowWidth; } set { Console.WindowWidth = value; } }

		/// <summary>
		/// Gets the standard output stream.
		/// </summary>
		public TextWriter Out { get { return Console.Out; } }

		/// <summary>
		/// Gets the standard error output stream.
		/// </summary>
		public TextWriter Error { get { return Console.Error; } }

		/// <summary>
		/// Gets the standard input stream.
		/// </summary>
		public TextReader In { get { return Console.In; } }

		#endregion

		/* Constructors */

		///// <summary>
		///// Creates a new instance of the class.
		///// </summary>
		//public ConsoleApplication() : this(null) { }

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		/// <param name="arguments"></param>
		public ConsoleApplication( string[] arguments )
		{
			hConsoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);

			ErrorCode = 0;
			ErrorMessage = string.Empty;

			CurPath = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

			settings = new Settings(Assembly.GetEntryAssembly().Location + ".settings");
			settings.read();

			SetApplicationVariables();

			_arguments = arguments;
			CmdLine = new CommandLine(arguments);
		}

		/* Parse Methods */

		protected StringBuilder RunErrors;
		protected int RunResult;

		public int RunApp()
		{
			RunErrors = new StringBuilder();

			// 
			// flag precedence:
			// 
			//   debug
			//   verbose
			//   nologo
			//   quiet
			// 
			//if ((debug.Enabled && debug.Value)
			//		|| (verbose.Enabled && verbose.Value)
			//		|| (!nologo.Enabled || !nologo.Value)
			//		|| (!quiet.Enabled || !quiet.Value)) {
			//}

			if (!GetCommandLineProperties() || ErrorCode != Result.Success) {
				// An error occurred during processing the arguments,
				// such as:
				//    an invalid value (for a name/value pair)
				//    duplicate key was found
				//    a required argument is missing
				//    etc.
				return OutputError();
			}

			if (hidden.Enabled && (hidden.Exists || (help.Enabled && help.HasValue && help.Value.Equals("hidden", StringComparison.CurrentCultureIgnoreCase)))) {
				return OutputHiddenHelp();
			}

			if (help.Enabled && help.Exists) {
				return OutputHelp(help.Value);
			}

			if (version.Enabled && version.Value) {
				return OutputVersion();
			}

			if (envars.Enabled && envars.Value) {
				return OutputEnvironmentVariables();
			}

			if (config.Enabled && config.Exists && config.Value.Equals("read", StringComparison.CurrentCultureIgnoreCase)) {
				return OutputConfig();
			}

			if ((debug.Enabled && debug.Value) || (verbose.Enabled && verbose.Value) || (!nologo.Enabled || !nologo.Value)) {
				DisplayAppTitle();
			}

			if (emailArg.Enabled && emailArg.Exists && emailArg.IsArgument) {
				if (_authorEmailSet) {
					if (_authorNameSet) {
						LaunchUrl("mailto:" + AuthorName + " <" + AuthorEmail + ">");
					} else {
						LaunchUrl("mailto:" + AuthorEmail);
					}
					return ErrorCode = 0;
				}
				return ErrorCode = 1;
			}

			if (websiteArg.Enabled && websiteArg.Exists && websiteArg.IsArgument && _appWebsiteSet) {
				LaunchUrl(AppWebsite);
				return ErrorCode = 0;
			}

			if (licenseArg.Enabled && licenseArg.Exists && licenseArg.IsArgument && _appLicenseUrlSet) {
				LaunchUrl(AppLicenseUrl);
				return ErrorCode = 0;
			}

			if (config.Enabled && config.Exists && config.Value.Equals("write", StringComparison.CurrentCultureIgnoreCase)) {
				WriteAppConfig();
			}

			// 
			// Parse()
			RunResult = Run();
			// 
			// 

			if (RunErrors.Length > 0) {
				Write(RunErrors.ToString());
			}

			if ((Console.In != null) && (debug.Value || (!quiet.Value && pause.Enabled && pause.Value))) {
				string str = "Press any key to continue: ";
				Console.Write(str);
				Console.ReadKey(true);
				Console.CursorLeft = 0;
				Console.Write(new string(' ', str.Length + 1));
				Console.CursorLeft = 0;
			}

			return RunResult;
		}

		/// <summary>
		/// Base method that must be overridden by the subclass.
		/// </summary>
		public abstract int Run();

		/* Validation */

		/// <summary>
		/// Performs a basic validation of command-line arguments (when necessary)
		/// and displays the help (/?), version (/v), and shows environment variables (/set) when necessary.
		/// </summary>
		[Obsolete("use RunApp() instead. it performs all this did and more..", true)]
		protected bool ValidateCommandLineArguments() { return true; }

		/* GetCommandLineProperties() */

		private bool GetCommandLineProperties()
		{
			Type objectType;
			Type type;
			List<string> keys;
			Result result;
			CommandLineArg baseObj;

			keys = new List<string>();

			objectType = this.GetType();

			foreach (PropertyInfo p in objectType.GetProperties()) {
				if (p.MemberType != MemberTypes.Property) {
					continue;
				}
				if (!p.CanWrite) {
					continue;
				}

				type = p.PropertyType;

				// FIRST, verify there are no duplicate Keys in all of the supported command-line arguments.
				if (type == typeof(CommandLineArg) || type.IsSubclassOf(typeof(CommandLineArg))) {

					baseObj = p.GetValue(this, null) as CommandLineArg;
					foreach (string key in baseObj.Keys) {
						if (baseObj.Enabled) {
							if (!keys.Contains(key)) {
								keys.Add(key);
							} else {
								ErrorCode = 9;
								ErrorMessage = "Duplicate key found! " + key;
								return false;
							}
						}
					}

				} else {
					continue;
				}

				if (type == typeof(CommandLineArg<string>)) {
					// ****************************************************************************************************
					// ***** string ***************************************************************************************
					// ****************************************************************************************************
					CommandLineArg<string> obj;

					obj = p.GetValue(this, null) as CommandLineArg<string>;
					if (obj == null) {
						continue;
					}

					if (CmdLine.Contains(obj.Keys)) {
						// The property exists on the command-line.
						obj.IsArgument = true;
						// If the property is NameOnly, then don't bother getting its value.
						if ((obj.Options & CommandLineArgumentOptions.NameOnly) != CommandLineArgumentOptions.NameOnly) {
							obj.Value = CmdLine.GetString("", obj.Keys);
							if (obj.Value != "") {
								obj.HasValue = true;
							} else {
								obj.Value = obj.Default;
							}
						} else {
							//obj.Value = obj.Value;
						}
					} else if (config.Enabled && obj.AllowConfig && obj.Keys.Length > 0 && settings.contains(obj.Keys[0])) {
						obj.IsConfigItem = true;
						obj.Value = settings.attr<string>(obj.Keys[0]);
						if (obj.Value != null) {
							obj.HasValue = true;
						} else {
							obj.Value = obj.Default;
						}
					} else if (envars.Enabled && obj.AllowEnvironmentVariable && obj.Keys.Length > 0 && EnvironmentVariables.Exists(AppEnvPrefix + obj.Keys[0])) {
						// The property is set in an environment variable.
						obj.IsEnvironmentVariable = true;
						obj.Value = EnvironmentVariables.GetString(null, AppEnvPrefix + obj.Keys[0]);
						if (obj.Value != null) {
							obj.HasValue = true;
						} else {
							obj.Value = obj.Default;
						}
					} else if (obj.Required) {
						// The property is required and not provided.
						setObjectRequiredError<string>(obj);
						return false;
					} else {
						// Use the default value.
						obj.IsDefault = true;
						obj.Value = obj.Default;
					}

					// Verify that the value provided is allowed.
					if (obj.ExpressionsAllowed.Count > 0) {
						if (!obj.ExpressionsAllowed.ContainsKey(obj.Value, StringComparison.CurrentCultureIgnoreCase)) {
							setObjectExpressionsAllowedError<string>(obj);
							return false;
						}
					}

					result = obj.OnValidate(CmdLine, obj);
					if (result.code != Result.Okay) {
						ErrorMessage = result.message;
						ErrorCode = result.code;
						ErrorHelpContext = obj.Name;
						return false;
					}

				} else if (type == typeof(CommandLineArg<string[]>)) {
					// ****************************************************************************************************
					// ***** string[] *************************************************************************************
					// ****************************************************************************************************
					CommandLineArg<string[]> obj;
					// TODO
					//string s;

					obj = p.GetValue(this, null) as CommandLineArg<string[]>;
					if (obj == null) {
						continue;
					}

					//if (CmdLine.Contains(obj.Keys)) {
					//	// The property exists on the command-line.
					//	obj.IsArgument = true;
					//	// If the property is NameOnly, then don't bother getting its value.
					//	if ((obj.Options & CommandLineArgumentOptions.NameOnly) != CommandLineArgumentOptions.NameOnly) {
					//		if ((obj.Options & CommandLineArgumentOptions.NameRemainingValues) == CommandLineArgumentOptions.NameRemainingValues) {
					//			CmdLine.GetEverythingAfter(CmdLine.GetIndexOfArgument("add"));

					//		} else {
					//			obj.Value = CmdLine.GetString("", obj.Keys);
					//			if (obj.Value != "") {
					//				obj.HasValue = true;
					//			} else {
					//				obj.Value = obj.Default;
					//			}
					//		}
					//	} else {
					//		//obj.Value = obj.Value;
					//	}
					//} else if (config.Enabled && obj.AllowConfig && obj.Keys.Length > 0 && settings.contains(obj.Keys[0])) {
					//	obj.IsConfigItem = true;
					//	obj.Value = settings.attr<string>(obj.Keys[0]);
					//	if (obj.Value != null) {
					//		obj.HasValue = true;
					//	} else {
					//		obj.Value = obj.Default;
					//	}
					//} else if (envars.Enabled && obj.AllowEnvironmentVariable && obj.Keys.Length > 0 && EnvironmentVariables.Exists(AppEnvPrefix + obj.Keys[0])) {
					//	// The property is set in an environment variable.
					//	obj.IsEnvironmentVariable = true;
					//	obj.Value = EnvironmentVariables.GetString(null, AppEnvPrefix + obj.Keys[0]);
					//	if (obj.Value != null) {
					//		obj.HasValue = true;
					//	} else {
					//		obj.Value = obj.Default;
					//	}
					//} else if (obj.Required) {
					//	// The property is required and not provided.
					//	setObjectRequiredError<string>(obj);
					//	return false;
					//} else {
					//	// Use the default value.
					//	obj.IsDefault = true;
					//	obj.Value = obj.Default;
					//}

					//// Verify that the value provided is allowed.
					//if (obj.ExpressionsAllowed.Count > 0) {
					//	if (!obj.ExpressionsAllowed.ContainsKey(obj.Value, StringComparison.CurrentCultureIgnoreCase)) {
					//		setObjectExpressionsAllowedError<string>(obj);
					//		return false;
					//	}
					//}

					result = obj.OnValidate(CmdLine, obj);
					if (result.code != Result.Okay) {
						ErrorMessage = result.message;
						ErrorCode = result.code;
						ErrorHelpContext = obj.Name;
						return false;
					}

				} else if (type == typeof(CommandLineArg<bool>)) {
					// ****************************************************************************************************
					// ***** bool *****************************************************************************************
					// ****************************************************************************************************
					CommandLineArg<bool> obj;

					obj = p.GetValue(this, null) as CommandLineArg<bool>;
					if (obj == null) {
						continue;
					}

					if (obj.ExpressionsAllowed.Count > 0) {
						setObjectError<bool>(obj, "The ExpressionsAllowed property is not allowed for boolean arguments");
						return false;
					}

					if (CmdLine.Contains(obj.Keys)) {
						// The property exists on the command-line.
						obj.IsArgument = true;
						// If the property is NameOnly, then don't bother getting its value.
						if ((obj.Options & CommandLineArgumentOptions.NameOnly) != CommandLineArgumentOptions.NameOnly) {
							obj.Value = CmdLine.GetBoolean(obj.Value, obj.Keys);
						} else {
							//obj.Value = obj.Value;
						}
					} else if (!obj.Exists && envars.Enabled && obj.AllowEnvironmentVariable && EnvironmentVariables.Exists(AppEnvPrefix + obj.Keys[0])) {
						// The property is set in an environment variable.
						obj.IsEnvironmentVariable = true;
						obj.Value = EnvironmentVariables.GetBoolean(obj.Default, AppEnvPrefix + obj.Keys[0]);
					} else if (obj.Required) {
						// The property is required and not provided.
						setObjectRequiredError<bool>(obj);
						return false;
					} else {
						// Use the default value.
						obj.IsDefault = true;
						obj.Value = obj.Default;
					}

					result = obj.OnValidate(CmdLine, obj);
					if (result.code != Result.Okay) {
						ErrorMessage = result.message;
						ErrorCode = result.code;
						return false;
					}

				} else if (type == typeof(CommandLineArg<int>)) {
					// ****************************************************************************************************
					// ***** int ******************************************************************************************
					// ****************************************************************************************************
					CommandLineArg<int> obj;

					obj = p.GetValue(this, null) as CommandLineArg<int>;
					if (obj == null) {
						continue;
					}

					if (CmdLine.Contains(obj.Keys)) {
						// The property exists on the command-line.
						obj.IsArgument = true;
						// If the property is NameOnly, then don't bother getting its value.
						if ((obj.Options & CommandLineArgumentOptions.NameOnly) != CommandLineArgumentOptions.NameOnly) {
							obj.Value = CmdLine.GetInt32(int.MinValue, obj.Keys);
							if (obj.Value != int.MinValue) {
								obj.HasValue = true;
							} else {
								obj.Value = obj.Default;
							}
						} else {
							//obj.Value = obj.Value;
						}
					} else if (!obj.Exists && envars.Enabled && obj.AllowEnvironmentVariable && EnvironmentVariables.Exists(AppEnvPrefix + obj.Keys[0])) {
						// The property is set in an environment variable.
						obj.IsEnvironmentVariable = true;
						obj.Value = EnvironmentVariables.GetInt32(int.MinValue, AppEnvPrefix + obj.Keys[0]);
						if (obj.Value != int.MinValue) {
							obj.HasValue = true;
						} else {
							obj.Value = obj.Default;
						}
					} else if (obj.Required) {
						// The property is required and not provided.
						setObjectRequiredError<int>(obj);
						return false;
					} else {
						// Use the default value.
						obj.IsDefault = true;
						obj.Value = obj.Default;
					}

					// Verify that the value provided is allowed.
					if (obj.ExpressionsAllowed.Count > 0) {
						if (!obj.ExpressionsAllowed.ContainsKey(obj.Value.ToString(), StringComparison.CurrentCultureIgnoreCase)) {
							setObjectExpressionsAllowedError<int>(obj);
							return false;
						}
					}

					result = obj.OnValidate(CmdLine, obj);
					if (result.code != Result.Okay) {
						ErrorMessage = result.message;
						ErrorCode = result.code;
						return false;
					}

				} else if (type == typeof(CommandLineArg<DateTime>)) {
					// ****************************************************************************************************
					// ***** datetime *************************************************************************************
					// ****************************************************************************************************
					CommandLineArg<DateTime> obj;

					obj = p.GetValue(this, null) as CommandLineArg<DateTime>;
					if (obj == null) {
						continue;
					}

					if (obj.ExpressionsAllowed.Count > 0) {
						setObjectError<DateTime>(obj, "The ExpressionsAllowed property is not allowed for DateTime arguments, use the OnValidate() event instead.");
						return false;
					}

					if (CmdLine.Contains(obj.Keys)) {
						// The property exists on the command-line.
						obj.IsArgument = true;
						// If the property is NameOnly, then don't bother getting its value.
						if ((obj.Options & CommandLineArgumentOptions.NameOnly) != CommandLineArgumentOptions.NameOnly) {
							obj.Value = CmdLine.GetDateTime(DateTime.MinValue, obj.Keys);
							if (obj.Value != DateTime.MinValue) {
								obj.HasValue = true;
							} else {
								obj.HasValue = false;
								obj.Value = obj.Default;
							}
						} else {
							//obj.Value = obj.Value;
						}
					} else if (envars.Enabled && obj.AllowEnvironmentVariable && EnvironmentVariables.Exists(AppEnvPrefix + obj.Keys[0])) {
						// The property is set in an environment variable.
						obj.IsEnvironmentVariable = true;
						obj.Value = EnvironmentVariables.GetDateTime(DateTime.MinValue, AppEnvPrefix + obj.Keys[0]);
					} else if (obj.Required) {
						// The property is required and not provided.
						setObjectRequiredError<DateTime>(obj);
						return false;
					} else {
						// Use the default value.
						obj.IsDefault = true;
						obj.Value = obj.Default;
					}

					if (obj.Value != null && obj.Value != DateTime.MinValue) {
						obj.HasValue = true;
					}

					result = obj.OnValidate(CmdLine, obj);
					if (result.code != Result.Okay) {
						ErrorMessage = result.message;
						ErrorCode = result.code;
						return false;
					}

				} else {
					// ****************************************************************************************************
					// ***** unknown **************************************************************************************
					// ****************************************************************************************************
					if (type.ToString().StartsWith("Bricksoft.PowerCode.CommandLineArg`1")) {
						WriteLine(errorColor, "NOT SUPPORTED: " + type.ToString());
					}
					continue;
				}
			}

			return true;
		}

		private int setObjectRequiredError<T>( CommandLineArg<T> obj )
		{
			ErrorMessage = string.Format("Missing argument for {0}:\n   {1}\n   {2}", obj.Name, obj.Description, obj.MissingText);
			ErrorHelpContext = obj.Name;
			return ErrorCode = 10;
		}

		private int setObjectExpressionsAllowedError<T>( CommandLineArg<T> obj )
		{
			StringBuilder s = new StringBuilder();
			int len = 14;

			s.AppendLineFormat("Invalid argument for {0}.", obj.Name);
			s.AppendLineFormat("Allowed values for {0} include:", obj.ExpressionLabel);

			foreach (KeyValuePair<string, string> p in obj.ExpressionsAllowed) {
				s.Append(Text.Wrap(string.Format("{0,-" + len + "}", p.Key.Length > 0 ? p.Key : "(empty)"), Console.WindowWidth, 2, len + 2));

				if (p.Key.Length > len + 2) {
					s.AppendLine();
					s.Append(' ', len + 2);
				}

				s.AppendLine(Text.Wrap(p.Value, Console.WindowWidth, 0, len + 2));
			}

			ErrorMessage = s.ToString();
			ErrorHelpContext = obj.Name;
			return ErrorCode = 11;
		}

		private int setObjectError<T>( CommandLineArg<T> obj, string t )
		{
			ErrorMessage = string.Format("An error occurred in argument '{0}':\n   {1}", obj.Name, t);
			ErrorHelpContext = obj.Name;
			return ErrorCode = 10;
		}

		/* Misc helpers */

		#region ***** Embedded Resources Methods

		/// <summary>
		/// Returns the contents of the <paramref name="FileName"/> specified, from the resources folder.
		/// The <paramref name="FileName"/> must not include the path.
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public string GetEmbeddedFile( string FileName )
		{
			Assembly asm;
			Stream strm;
			StringBuilder result;

			asm = Assembly.GetExecutingAssembly();
			strm = asm.GetManifestResourceStream(ResourcePath + "." + ResourcePath + "." + FileName);

			if (strm == null) {
				return string.Empty;
			}

			result = new StringBuilder();

			using (StreamReader reader = new StreamReader(strm)) {
				result.Append(reader.ReadToEnd());
				reader.Close();
			}

			result
					.Replace("{AuthorName}", AuthorName.ToString())
					.Replace("{AuthorEmail}", AuthorEmail.ToString())
				//.Replace("{AuthorWebsite}", AuthorWebsite.ToString())
					.Replace("{AppName}", AppName)
					.Replace("{AppDesc}", AppDescription)
					.Replace("{AppCopyright}", AppCopyright)
					.Replace("{AppWebsite}", AppWebsite)
					.Replace("{Major}", AppMajor.ToString())
					.Replace("{Minor}", AppMinor.ToString())
					.Replace("{Build}", AppBuild.ToString())
					.Replace("{Revision}", AppRevision.ToString())
					.Replace("{AppLicense}", AppRevision.ToString())
					.Replace("{AppLicenseUrl}", AppRevision.ToString());

			return result.ToString();
		}

		#endregion

		#region ***** Metadata Helpers

		/* Default handlers for getting the help/usage, etc. */

		public int OutputError()
		{
			DisplayAppTitle(false);
			DisplayAppError(ErrorMessage);
			DisplayAppUsage(ErrorHelpContext);
			return ErrorCode;
		}

		public int OutputHiddenHelp()
		{
			DisplayAppTitle(true);
			DisplayAppUsage("");
			return ErrorCode = 0;
		}

		public int OutputUsage( string helpContext ) { return OutputHelp(helpContext); }

		public int OutputHelp() { return OutputHelp(""); }

		public int OutputHelp( string helpContext )
		{
			DisplayAppTitle(false);
			DisplayAppUsage(helpContext);
			return ErrorCode = 0;
		}

		public int OutputVersion()
		{
			DisplayAppTitle(false);
			return ErrorCode = 0;
		}

		public int OutputEnvironmentVariables()
		{
			DisplayAppTitle();
			DisplayAppEnvironmentVariables();
			return ErrorCode = 0;
		}

		public int OutputConfig()
		{
			DisplayAppTitle();
			DisplayAppConfig();
			return ErrorCode = 0;
		}

		/* Default handlers for getting the help/usage, etc. */

		public void DisplayAppTitle() { DisplayAppTitle(false); }

		public void DisplayAppTitle( bool everything )
		{
			int padLen = AppName.Length;
			string padding = new string(' ', padLen);
			bool showKey = (help.Enabled && help.Exists) || (hidden.Enabled && hidden.Exists);

			// Show the application name and version.
			Console.Write("{0} | ", AppName);
			Write(version.Exists ? highlightColor : normalColor, "version: {0}.{1}.{2}.{3}", AppMajor, AppMinor, AppBuild, AppRevision);
			Console.WriteLine("{0}", showKey ? " (--version)" : "");

			// Display the application's description (only if set).
			// NOTE: I'm using the _appDescriptionSet variable, because I only want to display the description
			// if the subclass specified the description; I don't want to show the assembly's 'Description' property..
			if (_appDescriptionSet) {
				Console.WriteLine("{0} | {1}", padding, AppDescription);
			}

			// Display the author's name and/or email address.
			if (_authorNameSet) {
				if (_authorEmailSet) {
					Console.WriteLine("{0} | created by {1} <{2}>{3}", padding, AuthorName, AuthorEmail, showKey ? " (--email)" : "");
				} else {
					Console.WriteLine("{0} | created by {1}", padding, AuthorName);
				}
			} else if (_authorEmailSet) {
				Console.WriteLine("{0} | created by {1}{2}", padding, AuthorEmail, showKey ? " (--email)" : "");
			}

			if (everything) {
				//// Display the author's website.
				//if (_authorWebsiteSet) {
				//	Console.WriteLine("{0} | created by {1}{2}", padding, AuthorWebsite, showParam ? " (--website)" : "");
				//}

				// Show the app website.
				if (_appWebsiteSet) {
					Console.WriteLine("{0} | {1}{2}", padding, AppWebsite, showKey ? " (--website)" : "");
				}

				// Display the author's twitter name.
				if (_authorTwitterSet) {
					Console.WriteLine("{0} | follow on @{1}{2}", padding, AuthorTwitter.TrimStart('@'), showKey ? " (--twitter)" : "");
				}

				// Show the license information.
				if (_appLicenseSet) {
					Console.WriteLine("{0} | released under the {1} license", padding, AppLicense);
					if (_appLicenseUrlSet) {
						Console.WriteLine("{0} | {1}{2}", padding, AppLicenseUrl, showKey ? " (--license)" : "");
					}
				} else if (_appLicenseUrlSet) {
					Console.WriteLine("{0} | {1}{2}", padding, AppLicenseUrl, showKey ? " (--license)" : "");
				}
			}

			if (help.Enabled && !help.Exists && !envars.Exists && !hidden.Exists) {
				Console.WriteLine("{0} | display usage information (--help)", padding);
			}

			Console.WriteLine();
		}

		public void DisplayAppUsage( string helpContext )
		{
			int group = 0;
			int W = Math.Max(Console.WindowWidth - 1, 30);
			string separator;
			string flag;
			CommandLineArg arg;
			string addtlArgs = "";
			StringBuilder temp = new StringBuilder();
			int flagLen = Math.Max(__minFlagPadLen, Math.Min(__maxFlagPadLen, AppName.Length + 1 + 2));
			string flagIndent = new string(' ', flagLen);
			string flagPre = "  ";
			int valLen = flagLen - 5;
			string valIndent = flagIndent;
			string valPre = new string(' ', flagLen - valLen);

			Type objectType = this.GetType();
			List<PropertyInfo> properties = new List<PropertyInfo>();

			foreach (PropertyInfo p in objectType.GetProperties()) {
				if (p.MemberType == MemberTypes.Property && (p.PropertyType == typeof(CommandLineArg) || p.PropertyType.IsSubclassOf(typeof(CommandLineArg)))) {
					properties.Add(p);

					arg = (CommandLineArg)p.GetValue(this, null);
					if (arg.Keys.Length == 0) {
						flag = "'" + arg.Name + "'";
						if (arg.Required) {
							addtlArgs += " " + flag;
						} else {
							addtlArgs += " [" + flag + "]";
						}
					}
				}
			}

			#region properties.Sort()

			CommandLineArg ca;
			CommandLineArg cb;
			int result;

			properties.Sort(delegate( PropertyInfo a, PropertyInfo b )
			{
				ca = (CommandLineArg)a.GetValue(this, null);
				cb = (CommandLineArg)b.GetValue(this, null);

				if ((result = ca.Group.CompareTo(cb.Group)) == 0) {
					if ((result = ca.SortIndex.CompareTo(cb.SortIndex)) == 0) {
						return string.Compare(ca.Name, cb.Name, StringComparison.InvariantCultureIgnoreCase);
					}
				}

				return result;
			});

			#endregion

			Write(highlightColor, "Usage: ".ToUpper());
			Console.WriteLine("\n  {0} [options]{1}", AppName, addtlArgs);

			foreach (PropertyInfo property in properties) {
				ca = (CommandLineArg)property.GetValue(this, null);

				if (!ca.Enabled) {
					continue;
				}

				if (ca.DisplayMode == DisplayMode.Always || (hidden.Enabled && hidden.Value && ca.DisplayMode == DisplayMode.Hidden)) {
					temp.Clear();

					if (ca.DisplayMode == DisplayMode.Hidden) {
						Console.ForegroundColor = hiddenColor;
					} else {
						Console.ForegroundColor = normalColor;
					}

					if (group != ca.Group) {
						Console.WriteLine();
						group = ca.Group;
					}

					separator = (ca.Options & CommandLineArgumentOptions.NameOnly) == CommandLineArgumentOptions.NameOnly ? "/" : "--";

					if (ca.Keys.Length > 0) {
						if (ca.Keys[0].StartsWith("/") || ca.Keys[0].StartsWith("-")) {
							flag = ca.Keys[0];
						} else {
							flag = separator + ca.Keys[0];
						}
						if (ca.ExpressionLabel.Length > 0) {
							flag += " " + ca.ExpressionLabel;
						}
						temp.Append(flagPre);
						temp.Append(string.Format("{0,-" + (flagLen - flagPre.Length) + "}", flag));
						if (ca.Name == helpContext) {
							if (help.Enabled && help.Exists && help.Value == helpContext) {
								Write(hiddenColor, temp.ToString());
							} else {
								Write(errorColor, temp.ToString());
							}
						} else {
							Console.Write(Text.Wrap(temp.ToString(), W, 0, flagLen - flagPre.Length));
						}
					} else {
						temp.Append(flagPre);
						flag = "'" + ca.Name + "'";
						temp.Append(string.Format("{0,-" + (flagLen - flagPre.Length) + "}", flag));
						Console.Write(Text.Wrap(temp.ToString(), W, 0, flagLen - flagPre.Length));
					}

					if (temp.Length > W - flagLen) {
						Console.WriteLine();
						Console.Write(flagIndent);
					}

					temp.Clear();

					if (ca.Keys.Length < 2) {
						temp.Append(ca.Description);
					} else {
						temp.Append(ca.Description).Append(" (flags: ");
						for (int i = 0; i < ca.Keys.Length; i++) {
							string t = ca.Keys[i];
							//temp.Append(separator + string.Join(" | " + separator, ca.Keys) + ")");
							if (t.StartsWith("/") || t.StartsWith("-")) {
								temp.Append(t);
							} else {
								temp.Append(separator).Append(t);
							}
							if (i < ca.Keys.Length - 1) {
								temp.Append(" | ");
							}
						}
						temp.Append(")");
					}

					if (ca.Required) {
						temp.Append(" (*Required)");
					}

					//Console.WriteLine(Text.Wrap(temp.ToString(), W, 0, flagLen));
					Console.WriteLine(Text.Wrap(temp.ToString(), new int[] { W - flagLen, W }, new int[] { 0, flagLen }));

					if (ca.ExpressionsAllowed.Count > 0
							&& ((ca.Options & CommandLineArgumentOptions.NameValueOptional) == CommandLineArgumentOptions.NameValueOptional
							 || (ca.Options & CommandLineArgumentOptions.NameValueRequired) == CommandLineArgumentOptions.NameValueRequired)) {
						Console.WriteLine("{0," + flagLen + "}Allowed values for {1} include:", valPre, ca.ExpressionLabel);

						foreach (KeyValuePair<string, string> p in ca.ExpressionsAllowed) {
							Console.Write(Text.Wrap(string.Format("{0,-" + valLen + "}", p.Key.Length > 0 ? p.Key : "(empty)"), W, flagLen + 2));

							if (p.Key.Length > valLen) {
								Console.WriteLine();
								Console.Write(flagIndent + valIndent + 2);
							}

							Console.WriteLine(Text.Wrap(p.Value, W, 0, flagLen + valIndent.Length + 2));
						}
					}

				}
			}

			Console.ForegroundColor = normalColor;

			Console.WriteLine();
			Console.WriteLine(" * use ! to set any option to opposite value.");
			Console.WriteLine("   for example:");
			Console.WriteLine("   use /!v to not use verbose (even overwriting a environment variable).");
		}

		/// <summary>
		/// Returns the maximum flag length.. not really implemented/used yet.
		/// </summary>
		public int GetMaxFlagLength()
		{
			int len = 0;
			Type objectType = this.GetType();
			List<PropertyInfo> properties = new List<PropertyInfo>();
			CommandLineArg ca;

			foreach (PropertyInfo p in objectType.GetProperties()) {
				if (p.MemberType == MemberTypes.Property && (p.PropertyType == typeof(CommandLineArg) || p.PropertyType.IsSubclassOf(typeof(CommandLineArg)))) {
					properties.Add(p);
				}
			}

			foreach (PropertyInfo property in properties) {
				ca = (CommandLineArg)property.GetValue(this, null);

				if (!ca.Enabled) {
					continue;
				}

				if (ca.DisplayMode != DisplayMode.Never) {
					if (ca.Keys.Length > 0) {
						len = Math.Max(len, ca.Keys[0].Length);
					} else {
						len = Math.Max(len, ca.Name.Length + 2); // add two for the ' characters surrounding the name..
					}
				}
			}

			return Math.Max(__minFlagPadLen, Math.Min(__maxFlagPadLen, len));
		}

		public void DisplayAppEnvironmentVariables()
		{
			Type objectType = this.GetType();
			List<PropertyInfo> properties = new List<PropertyInfo>();
			bool exists;
			string envKey, envVal;
			int padLen = AppName.Length;

			foreach (PropertyInfo p in objectType.GetProperties()) {
				if (p.MemberType == MemberTypes.Property && (p.PropertyType == typeof(CommandLineArg) || p.PropertyType.IsSubclassOf(typeof(CommandLineArg)))) {
					properties.Add(p);
				}
			}

			#region properties.Sort()

			CommandLineArg ca;
			CommandLineArg cb;
			int result;

			properties.Sort(delegate( PropertyInfo a, PropertyInfo b )
			{
				ca = (CommandLineArg)a.GetValue(this, null);
				cb = (CommandLineArg)b.GetValue(this, null);

				if ((result = ca.Group.CompareTo(cb.Group)) == 0) {
					if ((result = ca.SortIndex.CompareTo(cb.SortIndex)) == 0) {
						return string.Compare(ca.Name, cb.Name, StringComparison.InvariantCultureIgnoreCase);
					}
				}

				return result;
			});

			#endregion

			WriteLine(highlightColor, "Environment Variables: ".ToUpper());

			foreach (PropertyInfo p in properties) {
				CommandLineArg arg = (CommandLineArg)p.GetValue(this, null);

				if (arg == null || !arg.Enabled || !arg.AllowEnvironmentVariable || arg.Keys.Length == 0) {
					continue;
				}

				envKey = AppEnvPrefix + arg.Keys[0];
				exists = EnvironmentVariables.Contains(envKey);
				envVal = EnvironmentVariables.GetString("<not set>", envKey);

				WriteLine(exists ? hiddenColor : normalColor, "  {0,-" + padLen + "} {1}", envKey, envVal);
			}
		}

		public void DisplayAppConfig()
		{
			//Console.WriteLine("\nCURRENT WINDOW:");
			//Console.WriteLine("  width  = {0,3}", Console.WindowWidth);
			//Console.WriteLine("  height = {0,3}", Console.WindowHeight);
			//Console.WriteLine("\nSAVED CONFIG:");
			//Console.WriteLine("  width  = {0}", settings.contains("width") ? settings.attr<int>("width").ToString() : "not set");
			//Console.WriteLine("  height = {0}", settings.contains("height") ? settings.attr<int>("height").ToString() : "not set");
			//Console.WriteLine("  center = {0}", settings.contains("center") ? settings.attr<bool>("center").ToString().ToLower() : defaultCenter.ToString().ToLower());

			Type objectType = this.GetType();
			List<PropertyInfo> properties = new List<PropertyInfo>();
			bool exists;
			string key, value;
			int padLen = AppName.Length;

			foreach (PropertyInfo p in objectType.GetProperties()) {
				if (p.MemberType == MemberTypes.Property && (p.PropertyType == typeof(CommandLineArg) || p.PropertyType.IsSubclassOf(typeof(CommandLineArg)))) {
					properties.Add(p);
				}
			}

			#region properties.Sort()

			CommandLineArg ca;
			CommandLineArg cb;
			int result;

			properties.Sort(delegate( PropertyInfo a, PropertyInfo b )
			{
				ca = (CommandLineArg)a.GetValue(this, null);
				cb = (CommandLineArg)b.GetValue(this, null);

				if ((result = ca.Group.CompareTo(cb.Group)) == 0) {
					if ((result = ca.SortIndex.CompareTo(cb.SortIndex)) == 0) {
						return string.Compare(ca.Name, cb.Name, StringComparison.InvariantCultureIgnoreCase);
					}
				}

				return result;
			});

			#endregion

			WriteLine(highlightColor, "SAVED CONFIG: ");

			foreach (PropertyInfo p in properties) {
				CommandLineArg arg = (CommandLineArg)p.GetValue(this, null);

				if (arg == null || !arg.Enabled || !arg.AllowConfig || arg.Keys.Length == 0) {
					continue;
				}

				key = arg.Keys[0];
				exists = settings.contains(key);

				if (exists) {
					value = settings.attr<string>(key);
					WriteLine(value.Length > 0 ? hiddenColor : normalColor, "  {0,-" + padLen + "} {1}", key, value.Length > 0 ? value : "<not set>");
				} else {
					Console.WriteLine("  {0,-" + padLen + "} {1}", key, "<not set>");
				}
			}
		}

		public void WriteAppConfig()
		{
			Type objectType = this.GetType();
			List<PropertyInfo> properties = new List<PropertyInfo>();
			Type type;

			// Update the settings, just in case.
			settings.read();

			foreach (PropertyInfo p in objectType.GetProperties()) {
				if (p.MemberType == MemberTypes.Property && (p.PropertyType == typeof(CommandLineArg) || p.PropertyType.IsSubclassOf(typeof(CommandLineArg)))) {
					properties.Add(p);
				}
			}

			#region properties.Sort()

			CommandLineArg ca;
			CommandLineArg cb;
			int result;

			properties.Sort(delegate( PropertyInfo a, PropertyInfo b )
			{
				ca = (CommandLineArg)a.GetValue(this, null);
				cb = (CommandLineArg)b.GetValue(this, null);

				if ((result = ca.Group.CompareTo(cb.Group)) == 0) {
					if ((result = ca.SortIndex.CompareTo(cb.SortIndex)) == 0) {
						return string.Compare(ca.Name, cb.Name, StringComparison.InvariantCultureIgnoreCase);
					}
				}

				return result;
			});

			#endregion

			WriteLine(highlightColor, "Writing to Config: ".ToUpper());

			foreach (PropertyInfo p in properties) {
				CommandLineArg arg = (CommandLineArg)p.GetValue(this, null);

				if (arg == null || !arg.Enabled || !arg.AllowConfig || arg.Keys.Length == 0) {
					continue;
				}

				type = p.PropertyType;

				if (type != typeof(CommandLineArg) && !type.IsSubclassOf(typeof(CommandLineArg))) {
					continue;
				}

				if (type == typeof(CommandLineArg<string>)) {
					// ***** string *************************************************************************************
					CommandLineArg<string> obj = p.GetValue(this, null) as CommandLineArg<string>;
					if (obj == null) {
						continue;
					}
					settings.attr<string>(obj.Keys[0], obj.Value);
				} else if (type == typeof(CommandLineArg<bool>)) {
					// ***** bool *************************************************************************************
					CommandLineArg<bool> obj = p.GetValue(this, null) as CommandLineArg<bool>;
					if (obj == null) {
						continue;
					}
					settings.attr<bool>(obj.Keys[0], obj.Value);
				} else if (type == typeof(CommandLineArg<int>)) {
					// ***** int *************************************************************************************
					CommandLineArg<int> obj = p.GetValue(this, null) as CommandLineArg<int>;
					if (obj == null) {
						continue;
					}
					settings.attr<int>(obj.Keys[0], obj.Value);
				} else if (type == typeof(CommandLineArg<DateTime>)) {
					// ***** datetime *************************************************************************************
					CommandLineArg<DateTime> obj = p.GetValue(this, null) as CommandLineArg<DateTime>;
					if (obj == null) {
						continue;
					}
					settings.attr<DateTime>(obj.Keys[0], obj.Value);
				} else {
					// ***** unknown
					continue;
				}
			}

			// Write the updated settings back out to the file.
			settings.write();
		}

		public void DisplayAppError( string Error )
		{
			if (Error != null && Error.Length > 0) {
				WriteLine(errorColor, "****ERROR: {0}", Error);
			}
		}

		#endregion

		/* Console Wrappers */

		#region ***** Beep()

		/// <summary>
		/// Plays the sound of a beep through the console speaker.
		/// </summary>
		public void Beep() { Console.Beep(); }

		/// <summary>
		/// Plays the sound of a beep through the console speaker.
		/// </summary>
		/// <param name="frequency">The frequency of the beep, ranging from 37 to 32767 hertz.</param>
		/// <param name="duration">The duration of the beep measured in milliseconds.</param>
		public void Beep( int frequency, int duration ) { Console.Beep(frequency, duration); }

		#endregion

		#region ***** SetColor()

		/// <summary>
		/// Sets the foreground color.
		/// </summary>
		/// <param name="foreColor"></param>
		/// <returns></returns>
		public ConsoleApplication SetColor( ConsoleColor foreColor )
		{
			Console.ForegroundColor = foreColor;
			return this;
		}

		/// <summary>
		/// Sets the foreground and background colors.
		/// </summary>
		/// <param name="foreColor"></param>
		/// <param name="backColor"></param>
		/// <returns></returns>
		public ConsoleApplication SetColor( ConsoleColor foreColor, ConsoleColor backColor )
		{
			Console.ForegroundColor = foreColor;
			Console.BackgroundColor = backColor;
			return this;
		}

		#endregion

		#region ***** Clear()

		/// <summary>
		/// Clears the console buffer and corresponding console window of display information.
		/// </summary>
		public void Clear() { Console.Clear(); }

		//public void Clear() {
		//   int hWrittenChars = 0;
		//   CONSOLE_SCREEN_BUFFER_INFO strConsoleInfo = new CONSOLE_SCREEN_BUFFER_INFO();
		//   COORD Home;
		//   Home.x = Home.y = 0;
		//   GetConsoleScreenBufferInfo(hConsoleHandle, ref strConsoleInfo);
		//   FillConsoleOutputCharacter(hConsoleHandle, EMPTY, strConsoleInfo.dwSize.x * strConsoleInfo.dwSize.y, Home, ref hWrittenChars);
		//   SetConsoleCursorPosition(hConsoleHandle, Home);
		//}

		/// <summary>
		/// Clears the specified rectangle of the console window.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Clear( int left, int top, int width, int height )
		{
			int row;
			string s = new string(' ', width - left);

			for (row = top; row < height; row++) {
				SetCursorPosition(left, row);
				Write(s);
			}
		}

		/// <summary>
		/// Clears the specified rectangle of the console window, using the specified <paramref name="bgColor"/>.
		/// </summary>
		/// <param name="bgColor"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Clear( ConsoleColor bgColor, int left, int top, int width, int height )
		{
			ConsoleColor backupBgColor = Console.BackgroundColor;
			int row;
			string s = new string(' ', width - left);

			Console.BackgroundColor = bgColor;

			for (row = top; row < height; row++) {
				SetCursorPosition(left, row);
				Write(s);
			}

			Console.BackgroundColor = backupBgColor;
		}

		/// <summary>
		/// Clears the current line.
		/// </summary>
		public void ClearLine()
		{
			int bufWidth = Console.BufferWidth;

			Console.BufferWidth = Math.Max(Console.BufferWidth, Console.WindowWidth + 1);

			Console.SetCursorPosition(0, Console.CursorTop);
			Write(new string(' ', Console.WindowWidth));

			Console.SetCursorPosition(0, Console.CursorTop);
			Console.BufferWidth = bufWidth;
		}

		/// <summary>
		/// Clears the current line using the specified <param name="bgColor"/>.
		/// </summary>
		public void ClearLine( ConsoleColor bgColor )
		{
			int bufWidth = Console.BufferWidth;
			ConsoleColor backupBgColor = Console.BackgroundColor;

			Console.BufferWidth = Math.Max(Console.BufferWidth, Console.WindowWidth + 1);

			Console.BackgroundColor = bgColor;
			Console.SetCursorPosition(0, Console.CursorTop);
			Write(new string(' ', Console.WindowWidth));

			Console.BackgroundColor = backupBgColor;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.BufferWidth = bufWidth;
		}

		/// <summary>
		/// Clears the to the end of the current line.
		/// </summary>
		public void ClearToEnd()
		{
			int bufWidth = Console.BufferWidth;
			int cursorLeft = Console.CursorLeft;

			Console.BufferWidth = Math.Max(Console.BufferWidth, Console.WindowWidth + 1);

			Write(new string(' ', Console.WindowWidth - Console.CursorLeft));

			Console.SetCursorPosition(cursorLeft, Console.CursorTop);
			Console.BufferWidth = bufWidth;
		}

		/// <summary>
		/// Clears the to the end of the current line using the specified <param name="bgColor"/>.
		/// </summary>
		public void ClearToEnd( ConsoleColor bgColor )
		{
			int bufWidth = Console.BufferWidth;
			int cursorLeft = Console.CursorLeft;
			ConsoleColor backupBgColor = Console.BackgroundColor;

			Console.BufferWidth = Math.Max(Console.BufferWidth, Console.WindowWidth + 1);

			Write(new string(' ', Console.WindowWidth - Console.CursorLeft));

			Console.BackgroundColor = backupBgColor;
			Console.SetCursorPosition(cursorLeft, Console.CursorTop);
			Console.BufferWidth = bufWidth;
		}

		#endregion

		#region ***** Color, Cursor, Window methods

		/// <summary>
		/// Sets the foreground and background console colors to their defaults.
		/// </summary>
		public void ResetColor() { Console.ResetColor(); }

		/// <summary>
		/// Sets the position of the cursor.
		/// </summary>
		/// <param name="left">The column position of the cursor.</param>
		/// <param name="top">The row position of the cursor.</param>
		public void SetCursorPosition( int left, int top ) { Console.SetCursorPosition(left, top); }

		/// <summary>
		/// Sets the width and height of the screen buffer area to the specified values.
		/// </summary>
		/// <param name="width">The width of the buffer area measured in columns.</param>
		/// <param name="height">The height of the buffer area measured in rows.</param>
		public void SetBufferSize( int width, int height ) { Console.SetBufferSize(width, height); }

		/// <summary>
		/// Sets the position of the console window relative to the screen buffer.
		/// </summary>
		/// <param name="left">The column position of the upper left corner of the console window.</param>
		/// <param name="top">The row position of the upper left corner of the console window.</param>
		public void SetWindowPosition( int left, int top ) { Console.SetWindowPosition(left, top); }

		/// <summary>
		/// Sets the width and height of the console window to the specified values.
		/// </summary>
		/// <param name="width">The width of the console window measured in columns.</param>
		/// <param name="height">The height of the console window measured in rows.</param>
		public void SetWindowSize( int width, int height ) { Console.SetWindowSize(width, height); }

		#endregion

		/* Read methods */

		#region ***** Read()

		/// <summary>
		/// Reads the next character from the console standard input stream.
		/// </summary>
		public int Read() { return Console.Read(); }

		#endregion

		#region ***** ReadKey()

		/// <summary>
		/// Obtains the next key pressed by the login. The pressed key is displayed on the console window.
		/// </summary>
		public ConsoleKeyCode ReadKey() { return new ConsoleKeyCode(Console.ReadKey()); }

		/// <summary>
		/// Obtains the next key pressed by the login.
		/// </summary>
		/// <param name="intercept">Determines whether to display the pressed key on the console window or not.</param>
		public ConsoleKeyCode ReadKey( bool intercept ) { return new ConsoleKeyCode(Console.ReadKey(intercept)); }

		/// <summary>
		/// Obtains the next key pressed by the login. Modifiers are ignored.
		/// </summary>
		/// <param name="allowedKeys">The collection of ConsoleKey's that are allowed to be accepted. In other words, this method will not return unless one of the keys in allowedKeys was pressed.</param>
		public ConsoleKeyCode ReadKey( params ConsoleKey[] allowedKeys ) { return ReadKey(false, allowedKeys); }

		/// <summary>
		/// Obtains the next key pressed by the login. Modifiers are ignored.
		/// </summary>
		/// <param name="intercept">Determines whether to display the pressed key on the console window or not.</param>
		/// <param name="allowedKeys">The collection of ConsoleKeyInfo's that are allowed to be accepted. In other words, this method will not return unless one of the keys in allowedKeys was pressed.</param>
		public ConsoleKeyCode ReadKey( bool intercept, params ConsoleKey[] allowedKeys )
		{
			List<ConsoleKeyCode> keyCodes = new List<ConsoleKeyCode>(allowedKeys.Length);
			foreach (ConsoleKey conKey in allowedKeys) {
				keyCodes.Add(new ConsoleKeyCode(conKey, false, false, false, true));
			}
			return ReadKey(intercept, keyCodes.ToArray());
		}

		/// <summary>
		/// Obtains the next key pressed by the login. The pressed key is displayed on the console window.
		/// </summary>
		/// <param name="allowedKeys">The collection of ConsoleKeyInfo's that are allowed to be accepted. In other words, this method will not return unless one of the keys in allowedKeys was pressed.</param>
		public ConsoleKeyCode ReadKey( params ConsoleKeyCode[] allowedKeys ) { return ReadKey(false, allowedKeys); }

		/// <summary>
		/// Obtains the next key pressed by the login.
		/// </summary>
		/// <param name="intercept">Determines whether to display the pressed key on the console window or not.</param>
		/// <param name="allowedKeys">The collection of ConsoleKeyInfo's that are allowed to be accepted. In other words, this method will not return unless one of the keys in allowedKeys was pressed.</param>
		public ConsoleKeyCode ReadKey( bool intercept, params ConsoleKeyCode[] allowedKeys )
		{
			ConsoleKeyCode keyInfo; //= new ConsoleKeyInfo('d', ConsoleKey.D, true, true, true);
			bool bFoundIt = false;

			do {
				keyInfo = new ConsoleKeyCode(Console.ReadKey(intercept)); //.KeyChar.ToString().ToUpper().ToCharArray()[0];
				bFoundIt = false;

				foreach (ConsoleKeyCode allowedKey in allowedKeys) {
					if (allowedKey.Equals(keyInfo)) {
						bFoundIt = true;
						break;
					}
				}

			} while (!bFoundIt);

			return keyInfo;
		}

		#endregion

		#region ***** ReadPassword()

		private const String DEFAULT_ALLOWED_PASSWORD_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$^()[]{}-_=+,.;:~";

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="password"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( out String password ) { return ReadPassword(ConsoleApplication.DEFAULT_ALLOWED_PASSWORD_CHARS, out password, '*'); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="pwChar"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( out String password, Char? pwChar ) { return ReadPassword(ConsoleApplication.DEFAULT_ALLOWED_PASSWORD_CHARS, out password, pwChar); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="password"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( String allowed, out String password ) { return ReadPassword(allowed.ToCharArray(), out password, '*'); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="password"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( Char[] allowed, out String password ) { return ReadPassword(allowed, out password, '*'); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="password"></param>
		/// <param name="pwChar"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( String allowed, out String password, Char? pwChar ) { return ReadPassword(allowed.ToCharArray(), out password, pwChar); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="password"></param>
		/// <param name="pwChar"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( Char[] allowed, out String password, Char? pwChar ) { return ReadPassword(allowed, out password, pwChar, false); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="password"></param>
		/// <param name="pwChar"></param>
		/// <param name="DisallowedCharCausesError"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( String allowed, out String password, Char? pwChar, Boolean DisallowedCharCausesError ) { return ReadPassword(allowed.ToCharArray(), out password, pwChar, false); }

		/// <summary>
		/// Reads a password from the console.
		/// The Escape key will cancel.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="password"></param>
		/// <param name="pwChar"></param>
		/// <param name="DisallowedCharCausesError"></param>
		/// <returns>The last key pressed.</returns>
		public ConsoleKeyInfo ReadPassword( Char[] allowed, out String password, Char? pwChar, Boolean DisallowedCharCausesError )
		{
			StringBuilder result;
			ConsoleKeyInfo keyInfo;

			password = string.Empty;
			result = new StringBuilder();


			keyInfo = Console.ReadKey(true);
			WriteIf(pwChar != null && pwChar.HasValue, pwChar);

			while (keyInfo != null && keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape) {
				// TODO: Validate the keys typed
				//if (allowed.IndexOf(keyInfo.KeyChar) > -1) {
				if (allowed.Contains(keyInfo.KeyChar)) {
					result.Append(keyInfo.KeyChar);
				} else if (DisallowedCharCausesError) {
					return keyInfo; // Invalid character entered in password
				}

				keyInfo = Console.ReadKey(true);

				if (keyInfo.Key != ConsoleKey.Escape && keyInfo.Key != ConsoleKey.Enter) {
					WriteIf(pwChar != null && pwChar.HasValue, pwChar);
				}
			}

			if (keyInfo.Key == ConsoleKey.Enter) {
				Console.Out.WriteLine();
			}

			if (result.Length == 0) {
				Console.Error.WriteLine("Invalid password");
			} else {
				password = result.ToString();
			}

			return keyInfo;
		}

		#endregion

		#region ***** ReadString()

		/// <summary>
		/// Reads a string from the console only accepting characters in <paramref name="allowed"/>.
		/// </summary>
		/// <param name="allowed"></param>
		/// <returns></returns>
		public string ReadString( List<string> allowed ) { return ReadString(allowed, true, true); }

		/// <summary>
		/// Reads a string from the console only accepting characters in <paramref name="allowed"/>.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public string ReadString( List<string> allowed, bool ignoreCase ) { return ReadString(allowed, ignoreCase, true); }

		/// <summary>
		/// Reads a string from the console only accepting characters in <paramref name="allowed"/>.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="intercept"></param>
		/// <returns></returns>
		public string ReadString( List<string> allowed, bool ignoreCase, bool intercept )
		{
			// allowed[0] = "a"
			// allowed[1] = "b"
			// allowed[2] = "c"
			// allowed[3] = "abc"

			bool found = false;
			string retVal = string.Empty;
			StringComparison sc;

			if (ignoreCase) {
				sc = StringComparison.InvariantCultureIgnoreCase;// CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase;
			} else {
				sc = StringComparison.InvariantCulture; // CompareOptions.IgnoreSymbols;
			}

			while (found == false) {
				ConsoleKey key = Console.ReadKey(intercept).Key;

				if (key == ConsoleKey.Escape || key == ConsoleKey.LeftArrow || key == ConsoleKey.Backspace) {
					return "'" + key.ToString() + "'";
				}

				string temp = retVal + key.ToString();

				found = false;
				for (int i = 0; i < allowed.Count; i++) {
					if (allowed[i].StartsWith(temp, sc)) {

					}
				}

				if (found) {

				}
			}

			return retVal;
		}

		/// <summary>
		/// Reads a string from the console only accepting characters in <paramref name="allowed"/>.
		/// </summary>
		/// <param name="allowed"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="intercept"></param>
		/// <returns></returns>
		public string ReadString( char[] allowed, bool ignoreCase, bool intercept )
		{
			List<string> strs;

			strs = new List<string>(allowed.Length);

			for (int i = 0; i < allowed.Length; i++) {
				strs.Add(allowed[i].ToString());
			}

			return ReadString(strs, ignoreCase, intercept);
		}

		#endregion

		#region ***** ReadLine()

		/// <summary>
		/// Reads the next line of characters from the standard input stream.
		/// </summary>
		public string ReadLine() { return Console.ReadLine(); }

		/// <summary>
		/// Reads the next line of characters from the standard input stream and compares them to Allowed.
		/// </summary>
		/// <param name="Allowed">The collection of strings that are allowed to be accepted.
		/// In other words, this method will not return unless one of the strings in Allowed was entered.</param>
		public string ReadLine( string[] Allowed ) { return ReadLine(Allowed, false); }

		/// <summary>
		/// Reads the next line of characters from the standard input stream and compares them to Allowed.
		/// </summary>
		/// <param name="Allowed">The collection of strings that are allowed to be accepted.
		/// In other words, this method will not return unless one of the strings in Allowed was entered.</param>
		/// <param name="IgnoreCase">Whether or not to ignore the case of the string entered.</param>
		public string ReadLine( string[] Allowed, bool IgnoreCase )
		{
			string retVal = "";
			StringComparison sc;

			if (IgnoreCase) {
				sc = StringComparison.InvariantCultureIgnoreCase;// CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase;
			} else {
				sc = StringComparison.InvariantCulture; // CompareOptions.IgnoreSymbols;
			}

			do {
				string temp = Console.ReadLine();
				foreach (string tempAllowed in Allowed) {
					if (0 == string.Compare(tempAllowed, temp, sc)) {
						retVal = temp;
						break;
					}
				}
			} while (retVal.Equals(""));

			return retVal;
		}

		#endregion

		/* Write methods */

		#region ***** Write()

		/* ----- Write() ----- */

		/// <summary>
		/// Writes the text representation of a DateTime followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void Write( DateTime value ) { Out.Write(value.ToString(CultureInfo.CurrentCulture)); }
		/// <summary>
		/// Writes the text representation of a Boolean followed by a line terminator
		/// to the text stream.
		/// </summary>
		/// <param name="value">
		/// The Boolean to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( bool value ) { Out.Write(value.ToString(CultureInfo.CurrentCulture).ToLower()); }
		/// <summary>
		/// Writes a character followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The character to write to the text stream.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( char value ) { Out.Write(value); }
		/// <summary>
		/// Writes an array of characters followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( char[] buffer ) { Out.Write(buffer); }
		/// <summary>
		/// Writes the text representation of a decimal value followed by a line terminator
		/// to the text stream.
		/// </summary>
		/// <param name="value">
		/// The decimal value to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( decimal value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of a 8-byte floating-point value followed
		/// by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte floating-point value to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( double value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of a 4-byte floating-point value followed
		/// by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte floating-point value to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( float value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of a 4-byte signed integer followed by a line
		/// terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte signed integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( int value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of an 8-byte signed integer followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte signed integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( long value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of an object by calling ToString on this object,
		/// followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The object to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( object value ) { Out.Write(value); }
		/// <summary>
		/// Writes a string followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The string to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		[SecuritySafeCritical]
		public void Write( string value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of a 4-byte unsigned integer followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( uint value ) { Out.Write(value); }
		/// <summary>
		/// Writes the text representation of an 8-byte unsigned integer followed by
		/// a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( ulong value ) { Out.Write(value); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatted string.
		/// </param>
		/// <param name="arg0">
		/// The object to write into the formatted string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void Write( string format, object arg0 ) { Out.Write(format, arg0); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatting string.
		/// </param>
		/// <param name="parameters">
		/// The object array to write into format string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// A string or object is passed in as null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// arg.Length.
		/// </exception>
		public void Write( string format, params object[] parameters ) { Out.Write(format, parameters); }
		/// <summary>
		/// Writes a subarray of characters followed by a line terminator to the text
		/// stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <param name="index">
		/// The index into buffer at which to begin reading.
		/// </param>
		/// <param name="count">
		/// The maximum number of characters to write.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// The buffer length minus index is less than count.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// The buffer parameter is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// index or count is negative.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Write( char[] buffer, int index, int count ) { Out.Write(buffer, index, count); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatting string.
		/// </param>
		/// <param name="arg0">
		/// The object to write into the format string.
		/// </param>
		/// <param name="arg1">
		/// The object to write into the format string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void Write( string format, object arg0, object arg1 ) { Out.Write(format, arg0, arg1); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatting string.
		/// </param>
		/// <param name="arg0">
		/// The object to write into the format string.
		/// </param>
		/// <param name="arg1">
		/// The object to write into the format string.
		/// </param>
		/// <param name="arg2">
		/// The object to write into the format string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void Write( string format, object arg0, object arg1, object arg2 ) { Out.Write(format, arg0, arg1, arg2); }

		/// <summary>
		/// Writes the text representation of the specified object to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="foreColor">The ConsoleColor used to display the text. This color only affects this output of text; successive outputs will use the ForegroundColor.</param>
		/// <param name="value">The string to write to the console window.</param>
		public void Write( ConsoleColor foreColor, string value )
		{
			ConsoleColor backupForeColor = ForegroundColor;
			Console.ForegroundColor = foreColor;
			Console.Write(value);
			Console.ForegroundColor = backupForeColor;
		}

		/// <summary>
		/// Writes the text representation of the specified object to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="foreColor">The ConsoleColor used to display the text. This color only affects this output of text; successive outputs will use the ForegroundColor.</param>
		/// <param name="format">The format string.</param>
		/// <param name="parameters">The object or collection of objects to apply to the format string.</param>
		public void Write( ConsoleColor foreColor, string format, params object[] parameters )
		{
			ConsoleColor backupForeColor = Console.ForegroundColor;
			Console.ForegroundColor = foreColor;
			Console.Write(format, parameters);
			Console.ForegroundColor = backupForeColor;
		}

		/// <summary>
		/// Writes the text representation of the specified object to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="foreColor">The ConsoleColor used to display the text. This color only affects this output of text; successive outputs will use the <seealso cref="ForegroundColor"/>.</param>
		/// <param name="bgColor">The ConsoleColor used for the background. This color only affects this output of text; successive outputs will use the <seealso cref="BackgroundColor"/>.</param>
		/// <param name="format">The format string.</param>
		/// <param name="parameters">The object or collection of objects to apply to the format string.</param>
		public void Write( ConsoleColor foreColor, ConsoleColor bgColor, string format, params object[] parameters )
		{
			ConsoleColor backupForeColor = ForegroundColor;
			ConsoleColor backupBackColor = BackgroundColor;

			Console.ForegroundColor = foreColor;
			Console.BackgroundColor = bgColor;

			Console.Write(format, parameters);

			Console.ForegroundColor = backupForeColor;
			Console.BackgroundColor = backupBackColor;
		}

		/* ----- WriteIf() ----- */

		/// <summary>
		/// Writes the text representation of the specified object to the standard output stream using the specified format information, IF <paramref name="Expression"/> evaluates to true.
		/// </summary>
		/// <param name="Expression"></param>
		/// <param name="value"></param>
		public void WriteIf( bool Expression, object value )
		{
			if (Expression) {
				Write(value);
			}
		}

		#endregion

		#region ***** WriteLine()

		/* ----- WriteLine() ----- */

		/// <summary>
		/// Writes the current line terminator to the standard output stream.
		/// </summary>
		public void WriteLine() { Out.WriteLine(); }
		/// <summary>
		/// Writes the text representation of a Boolean followed by a line terminator
		/// to the text stream.
		/// </summary>
		/// <param name="value">
		/// The Boolean to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( bool value ) { Out.WriteLine(value.ToString(CultureInfo.CurrentCulture).ToLower()); }
		/// <summary>
		/// Writes a character followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The character to write to the text stream.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( char value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes an array of characters followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( char[] buffer ) { Out.WriteLine(buffer); }
		/// <summary>
		/// Writes the text representation of a decimal value followed by a line terminator
		/// to the text stream.
		/// </summary>
		/// <param name="value">
		/// The decimal value to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( decimal value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of a 8-byte floating-point value followed
		/// by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte floating-point value to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( double value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of a 4-byte floating-point value followed
		/// by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte floating-point value to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( float value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of a 4-byte signed integer followed by a line
		/// terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte signed integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( int value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of an 8-byte signed integer followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte signed integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( long value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of an object by calling ToString on this object,
		/// followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The object to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( object value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes a string followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The string to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		[SecuritySafeCritical]
		public void WriteLine( string value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of a 4-byte unsigned integer followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( uint value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes the text representation of an 8-byte unsigned integer followed by
		/// a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( ulong value ) { Out.WriteLine(value); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatted string.
		/// </param>
		/// <param name="arg0">
		/// The object to write into the formatted string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void WriteLine( string format, object arg0 ) { Out.WriteLine(format, arg0); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatting string.
		/// </param>
		/// <param name="parameters">
		/// The object array to write into format string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// A string or object is passed in as null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// arg.Length.
		/// </exception>
		public void WriteLine( string format, params object[] parameters ) { Out.WriteLine(format, parameters); }
		/// <summary>
		/// Writes a subarray of characters followed by a line terminator to the text
		/// stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <param name="index">
		/// The index into buffer at which to begin reading.
		/// </param>
		/// <param name="count">
		/// The maximum number of characters to write.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// The buffer length minus index is less than count.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// The buffer parameter is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// index or count is negative.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WriteLine( char[] buffer, int index, int count ) { Out.WriteLine(buffer, index, count); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatting string.
		/// </param>
		/// <param name="arg0">
		/// The object to write into the format string.
		/// </param>
		/// <param name="arg1">
		/// The object to write into the format string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void WriteLine( string format, object arg0, object arg1 ) { Out.WriteLine(format, arg0, arg1); }
		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatting string.
		/// </param>
		/// <param name="arg0">
		/// The object to write into the format string.
		/// </param>
		/// <param name="arg1">
		/// The object to write into the format string.
		/// </param>
		/// <param name="arg2">
		/// The object to write into the format string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void WriteLine( string format, object arg0, object arg1, object arg2 ) { Out.WriteLine(format, arg0, arg1, arg2); }

		/// <summary>
		/// Writes the text representation of a DateTime followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void WriteLine( DateTime value ) { Out.WriteLine(value.ToString(CultureInfo.CurrentCulture)); }

		//* ----- ConsoleColor ----- */

		/// <summary>
		/// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="foreColor">The ConsoleColor used to display the text. This color only affects this output of text; successive outputs will use the ForegroundColor.</param>
		/// <param name="format">The format string.</param>
		/// <param name="parameters">The object or collection of objects to apply to the format string.</param>
		public void WriteLine( ConsoleColor foreColor, string format, params object[] parameters ) { WriteLine(foreColor, string.Format(format, parameters)); }

		/// <summary>
		/// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="foreColor">The ConsoleColor used to display the text. This color only affects this output of text; successive outputs will use the ForegroundColor.</param>
		/// <param name="value">The string to write to the console.</param>
		public void WriteLine( ConsoleColor foreColor, string value ) { WriteLine(foreColor, Console.BackgroundColor, value); }

		/// <summary>
		/// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="foreColor">The ConsoleColor used to display the text. This color only affects this output of text; successive outputs will use the <seealso cref="ForegroundColor"/>.</param>
		/// <param name="bgColor">The ConsoleColor used for the background. This color only affects this output of text; successive outputs will use the <seealso cref="BackgroundColor"/>.</param>
		/// <param name="format">The format string.</param>
		/// <param name="parameters">The object or collection of objects to apply to the format string.</param>
		public void WriteLine( ConsoleColor foreColor, ConsoleColor bgColor, string format, params object[] parameters )
		{
			ConsoleColor backupForeColor = ForegroundColor;
			ConsoleColor backupBackColor = BackgroundColor;

			Console.ForegroundColor = foreColor;
			Console.BackgroundColor = bgColor;

			Console.Out.WriteLine(format, parameters);

			Console.ForegroundColor = backupForeColor;
			Console.BackgroundColor = backupBackColor;
		}

		// Note: The ColoredString Write methods are now in the ColoredStringConsoleExtensions class.
		//public void WriteLine( ColoredString value ) { WriteLine(value); }

		#endregion

		#region ***** WriteVerbose()

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public void WriteVerbose( string message )
		{
			if (verbose.Enabled && verbose.Value) {
				Out.WriteLine(message);
			}
		}

		#endregion

		#region ***** WriteError()

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( bool value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( char value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( char[] value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( decimal value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( double value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( float value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( int value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( uint value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( long value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( ulong value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public void WriteError( object value ) { WriteError(value.ToString()); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		///// </summary>
		///// <param name="format">The format string.</param>
		///// <param name="parameters">The object or collection of objects to apply to the format string.</param>
		//public void WriteError( string format, params object[] parameters ) { WriteError(0, string.Format(format, parameters)); }

		/// <summary>
		/// Writes the text representation of the specified object and a line terminator to the standard output stream using the specified format information.
		/// </summary>
		/// <param name="ErrorCode">The error code.</param>
		/// <param name="format">The format string.</param>
		/// <param name="parameters">The object or collection of objects to apply to the format string.</param>
		public void WriteError( int ErrorCode, string format, params object[] parameters ) { WriteError(ErrorCode, string.Format(format, parameters)); }

		///// <summary>
		///// Writes the text representation of the specified object and a line terminator to the standard output stream.
		///// </summary>
		///// <param name="ErrorMessage">The value to write.</param>
		//public void WriteError( string ErrorMessage ) { WriteError(0, ErrorMessage); }

		/// <summary>
		/// Writes the text representation of the specified object and a line terminator to the standard output stream.
		/// </summary>
		/// <param name="ErrorCode">The error code.</param>
		/// <param name="ErrorMessage">The value to write.</param>
		public void WriteError( int ErrorCode, string ErrorMessage )
		{
			ConsoleColor backup;
			string msg;

			backup = ForegroundColor;
			ForegroundColor = errorColor;

			msg = string.Format("**** Error #{0}: ", ErrorCode);

			Out.WriteLine(Text.Wrap(msg + ErrorMessage, Console.WindowWidth, msg.Length));

			ForegroundColor = backup;
		}

		#endregion

		/* Wrapping methods */

		#region ***** Wrap()

		/// <summary>
		/// Writes the text representation of a DateTime followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void Wrap( DateTime value ) { Out.Write(Text.Wrap(value.ToString(CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of a Boolean followed by a line terminator
		/// to the text stream.
		/// </summary>
		/// <param name="value">
		/// The Boolean to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Wrap( bool value ) { Out.Write(Text.Wrap(value.ToString(CultureInfo.CurrentCulture).ToLower(), Console.WindowWidth)); }

		/// <summary>
		/// Writes an array of characters followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Wrap( char[] buffer ) { Out.Write(Text.Wrap(new string(buffer), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of an object by calling ToString on this object,
		/// followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The object to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Wrap( object value ) { Out.Write(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes a string followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The string to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		[SecuritySafeCritical]
		public void Wrap( string value ) { Out.Write(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of a 4-byte unsigned integer followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Wrap( uint value ) { Out.Write(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of an 8-byte unsigned integer followed by
		/// a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Wrap( ulong value ) { Out.Write(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatted string.
		/// </param>
		/// <param name="parameters">
		/// The object to write into the formatted string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void Wrap( string format, params object[] parameters ) { Out.Write(Text.Wrap(string.Format(format, parameters), Console.WindowWidth)); }

		/// <summary>
		/// Writes a subarray of characters followed by a line terminator to the text
		/// stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <param name="index">
		/// The index into buffer at which to begin reading.
		/// </param>
		/// <param name="count">
		/// The maximum number of characters to write.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// The buffer length minus index is less than count.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// The buffer parameter is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// index or count is negative.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void Wrap( char[] buffer, int index, int count ) { Out.Write(Text.Wrap(new string(buffer, index, count), Console.WindowWidth)); }

		#endregion

		#region ***** WrapLine()

		/// <summary>
		/// Writes the text representation of a DateTime followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void WrapLine( DateTime value ) { Out.WriteLine(Text.Wrap(value.ToString(CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of a Boolean followed by a line terminator
		/// to the text stream.
		/// </summary>
		/// <param name="value">
		/// The Boolean to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WrapLine( bool value ) { Out.WriteLine(Text.Wrap(value.ToString(CultureInfo.CurrentCulture).ToLower(), Console.WindowWidth)); }

		/// <summary>
		/// Writes an array of characters followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WrapLine( char[] buffer ) { Out.WriteLine(Text.Wrap(new string(buffer), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of an object by calling ToString on this object,
		/// followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The object to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WrapLine( object value ) { Out.WriteLine(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes a string followed by a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The string to write. If value is null, only the line termination characters
		/// are written.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		[SecuritySafeCritical]
		public void WrapLine( string value ) { Out.WriteLine(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of a 4-byte unsigned integer followed by a
		/// line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 4-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WrapLine( uint value ) { Out.WriteLine(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes the text representation of an 8-byte unsigned integer followed by
		/// a line terminator to the text stream.
		/// </summary>
		/// <param name="value">
		/// The 8-byte unsigned integer to write.
		/// </param>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WrapLine( ulong value ) { Out.WriteLine(Text.Wrap(Convert.ToString(value, CultureInfo.CurrentCulture), Console.WindowWidth)); }

		/// <summary>
		/// Writes out a formatted string and a new line, using the same semantics as
		/// System.String.Format(System.String,System.Object).
		/// </summary>
		/// <param name="format">
		/// The formatted string.
		/// </param>
		/// <param name="parameters">
		/// The object to write into the formatted string.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// format is null.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The format specification in format is invalid.-or- The number indicating
		/// an argument to be formatted is less than zero, or larger than or equal to
		/// the number of provided objects to be formatted.
		/// </exception>
		public void WrapLine( string format, params object[] parameters ) { Out.WriteLine(Text.Wrap(string.Format(format, parameters), Console.WindowWidth)); }

		/// <summary>
		/// Writes a subarray of characters followed by a line terminator to the text
		/// stream.
		/// </summary>
		/// <param name="buffer">
		/// The character array from which data is read.
		/// </param>
		/// <param name="index">
		/// The index into buffer at which to begin reading.
		/// </param>
		/// <param name="count">
		/// The maximum number of characters to write.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// The buffer length minus index is less than count.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// The buffer parameter is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// index or count is negative.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The System.IO.TextWriter is closed.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public void WrapLine( char[] buffer, int index, int count ) { Out.WriteLine(Text.Wrap(new string(buffer, index, count), Console.WindowWidth)); }

		#endregion

		#region ***** WrapLineIf()

		///// <summary>
		///// Writes a string followed by a line terminator to the text stream.
		///// </summary>
		///// <param name="value">
		///// The value to write. If value is null, only the line termination characters
		///// are written.
		///// </param>
		///// <exception cref="System.ObjectDisposedException">
		///// The System.IO.TextWriter is closed.
		///// </exception>
		///// <exception cref="System.IO.IOException">
		///// An I/O error occurs.
		///// </exception>
		//[SecuritySafeCritical]
		//public void WrapLineIf( bool expression, string value )
		//{
		//   if (expression) {
		//      Out.WriteLine(Text.Wrap(value, Console.WindowWidth - 1));
		//   }
		//}

		///// <summary>
		///// Writes a string followed by a line terminator to the text stream.
		///// </summary>
		///// <param name="value">
		///// The string to write. If value is null, only the line termination characters
		///// are written.
		///// </param>
		///// <param name="wrapWidth">
		///// </param>
		///// <exception cref="System.ObjectDisposedException">
		///// The System.IO.TextWriter is closed.
		///// </exception>
		///// <exception cref="System.IO.IOException">
		///// An I/O error occurs.
		///// </exception>
		//[SecuritySafeCritical]
		//public void WrapLineIf( bool expression, string value, int wrapWidth )
		//{
		//   if (expression) {
		//      Out.WriteLine(Text.Wrap(value, wrapWidth));
		//   }
		//}

		///// <summary>
		///// Writes a string followed by a line terminator to the text stream.
		///// </summary>
		///// <param name="value">
		///// The string to write. If value is null, only the line termination characters
		///// are written.
		///// </param>
		///// <param name="wrapWidth">
		///// </param>
		///// <param name="indentations">
		///// </param>
		///// <exception cref="System.ObjectDisposedException">
		///// The System.IO.TextWriter is closed.
		///// </exception>
		///// <exception cref="System.IO.IOException">
		///// An I/O error occurs.
		///// </exception>
		//[SecuritySafeCritical]
		//public void WrapLineIf( bool expression, string value, int wrapWidth, params int[] indentations )
		//{
		//   if (expression) {
		//      Out.WriteLine(Text.Wrap(value, wrapWidth, indentations));
		//   }
		//}

		#endregion

		#region ***** WrapLineIf()

		///// <summary>
		///// Writes a string followed by a line terminator to the text stream.
		///// </summary>
		///// <param name="value">
		///// The value to write. If value is null, only the line termination characters
		///// are written.
		///// </param>
		///// <exception cref="System.ObjectDisposedException">
		///// The System.IO.TextWriter is closed.
		///// </exception>
		///// <exception cref="System.IO.IOException">
		///// An I/O error occurs.
		///// </exception>
		//[SecuritySafeCritical]
		//public void WrapLineIf( bool expression, string value )
		//{
		//   if (expression) {
		//      Out.WriteLine(Text.Wrap(value, Console.WindowWidth - 1));
		//   }
		//}

		///// <summary>
		///// Writes a string followed by a line terminator to the text stream.
		///// </summary>
		///// <param name="value">
		///// The string to write. If value is null, only the line termination characters
		///// are written.
		///// </param>
		///// <param name="wrapWidth">
		///// </param>
		///// <exception cref="System.ObjectDisposedException">
		///// The System.IO.TextWriter is closed.
		///// </exception>
		///// <exception cref="System.IO.IOException">
		///// An I/O error occurs.
		///// </exception>
		//[SecuritySafeCritical]
		//public void WrapLineIf( bool expression, string value, int wrapWidth )
		//{
		//   if (expression) {
		//      Out.WriteLine(Text.Wrap(value, wrapWidth));
		//   }
		//}

		///// <summary>
		///// Writes a string followed by a line terminator to the text stream.
		///// </summary>
		///// <param name="value">
		///// The string to write. If value is null, only the line termination characters
		///// are written.
		///// </param>
		///// <param name="wrapWidth">
		///// </param>
		///// <param name="indentations">
		///// </param>
		///// <exception cref="System.ObjectDisposedException">
		///// The System.IO.TextWriter is closed.
		///// </exception>
		///// <exception cref="System.IO.IOException">
		///// An I/O error occurs.
		///// </exception>
		//[SecuritySafeCritical]
		//public void WrapLineIf( bool expression, string value, int wrapWidth, params int[] indentations )
		//{
		//   if (expression) {
		//      Out.WriteLine(Text.Wrap(value, wrapWidth, indentations));
		//   }
		//}

		#endregion

		/* Console Helper Methods */

		#region ***** Console Helper Methods

		//public int GetColorInt(ConsoleColor color)
		//{
		//   switch (color)
		//   {
		//      case ConsoleColor.Black: return 
		//   }
		//   return 0;
		//}

		#endregion

		/* Misc. helper methods */

		public void LaunchUrl( string url )
		{
			ProcessStartInfo info = new ProcessStartInfo();

			info.Verb = "open";
			info.FileName = url;

			Process.Start(info);
		}
	}

#if BRICKSOFT_CMDLINE
	/// <summary>
	/// 
	/// </summary>
	public class Result
	{
		/// <summary>
		/// 
		/// </summary>
		public const int Okay = 0;

		/// <summary>
		/// 
		/// </summary>
		public const int Success = 0;

		/// <summary>
		/// 
		/// </summary>
		public const int Error = 1;

		//public readonly Result Success = new Result(0);
		//public readonly Result Error = new Result(1);

		/// <summary>
		/// 
		/// </summary>
		public int code { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public Result Code( int code )
		{
			this.code = code;
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		public string message { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public Result Message( string message )
		{
			this.message = message;
			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		public Result() : this(0, string.Empty) { }

		/// <summary>
		/// 
		/// </summary>
		public Result( int code ) : this(code, string.Empty) { }

		/// <summary>
		/// 
		/// </summary>
		public Result( int code, string message )
		{
			this.code = code;
			this.message = message;
		}
	}
#endif

	//// Summary:
	////     Specifies constants that define foreground and background colors for the
	////     console.
	//[Serializable]
	//public enum ConsoleColor {
	//	/// <summary>
	//	/// 
	//	/// </summary>
	//	NotSet = -1,
	//	// Summary:
	//	//     The color black.
	//	Black = 0,
	//	//
	//	// Summary:
	//	//     The color dark blue.
	//	DarkBlue = 1,
	//	//
	//	// Summary:
	//	//     The color dark green.
	//	DarkGreen = 2,
	//	//
	//	// Summary:
	//	//     The color dark cyan (dark blue-green).
	//	DarkCyan = 3,
	//	//
	//	// Summary:
	//	//     The color dark red.
	//	DarkRed = 4,
	//	//
	//	// Summary:
	//	//     The color dark magenta (dark purplish-red).
	//	DarkMagenta = 5,
	//	//
	//	// Summary:
	//	//     The color dark yellow (ochre).
	//	DarkYellow = 6,
	//	//
	//	// Summary:
	//	//     The color gray.
	//	Gray = 7,
	//	//
	//	// Summary:
	//	//     The color dark gray.
	//	DarkGray = 8,
	//	//
	//	// Summary:
	//	//     The color blue.
	//	Blue = 9,
	//	//
	//	// Summary:
	//	//     The color green.
	//	Green = 10,
	//	//
	//	// Summary:
	//	//     The color cyan (blue-green).
	//	Cyan = 11,
	//	//
	//	// Summary:
	//	//     The color red.
	//	Red = 12,
	//	//
	//	// Summary:
	//	//     The color magenta (purplish-red).
	//	Magenta = 13,
	//	//
	//	// Summary:
	//	//     The color yellow.
	//	Yellow = 14,
	//	//
	//	// Summary:
	//	//     The color white.
	//	White = 15,
	//}

	//public static class ConsoleColorExtensions {
	//	//public static System.ConsoleColor ==(ConsoleColor color) {} 

	//}

	//public enum MyEnum { A, B, C }
	//public static class MyEnumExt
	//{
	//	public static int Value(this MyEnum foo) { return (int)foo; }
	//	static void Main() {
	//		MyEnum val = MyEnum.A;
	//		int i = val.Value();
	//	}
	//}

	//public static class ConsoleTextWriterExtensions
	//{
	//   /// <summary>
	//   /// Writes a string followed by a line terminator to the text stream.
	//   /// </summary>
	//   /// <param name="value">
	//   /// The string to write. If value is null, only the line termination characters
	//   /// are written.
	//   /// </param>
	//   /// <exception cref="System.ObjectDisposedException">
	//   /// The System.IO.TextWriter is closed.
	//   /// </exception>
	//   /// <exception cref="System.IO.IOException">
	//   /// An I/O error occurs.
	//   /// </exception>
	//   [SecuritySafeCritical]
	//   public static void WriteWrappedLine( this TextWriter textWriter, string value )
	//   {
	//      textWriter.WriteLine(Text.Wrap(value, Console.WindowWidth - 1));
	//   }

	//   /// <summary>
	//   /// Writes a string followed by a line terminator to the text stream.
	//   /// </summary>
	//   /// <param name="value">
	//   /// The string to write. If value is null, only the line termination characters
	//   /// are written.
	//   /// </param>
	//   /// <param name="wrapWidth">
	//   /// </param>
	//   /// <exception cref="System.ObjectDisposedException">
	//   /// The System.IO.TextWriter is closed.
	//   /// </exception>
	//   /// <exception cref="System.IO.IOException">
	//   /// An I/O error occurs.
	//   /// </exception>
	//   [SecuritySafeCritical]
	//   public static void WriteWrappedLine( this TextWriter textWriter, string value, int wrapWidth )
	//   {
	//      textWriter.WriteLine(Text.Wrap(value, wrapWidth));
	//   }

	//   /// <summary>
	//   /// Writes a string followed by a line terminator to the text stream.
	//   /// </summary>
	//   /// <param name="value">
	//   /// The string to write. If value is null, only the line termination characters
	//   /// are written.
	//   /// </param>
	//   /// <param name="wrapWidth">
	//   /// </param>
	//   /// <param name="indentations">
	//   /// </param>
	//   /// <exception cref="System.ObjectDisposedException">
	//   /// The System.IO.TextWriter is closed.
	//   /// </exception>
	//   /// <exception cref="System.IO.IOException">
	//   /// An I/O error occurs.
	//   /// </exception>
	//   [SecuritySafeCritical]
	//   public static void WriteWrappedLine( this TextWriter textWriter, string value, int wrapWidth, params int[] indentations )
	//   {
	//      textWriter.WriteLine(Text.Wrap(value, wrapWidth, indentations));
	//   }

	//   /// <summary>
	//   /// Writes a string followed by a line terminator to the text stream.
	//   /// </summary>
	//   /// <param name="value">
	//   /// The string to write. If value is null, only the line termination characters
	//   /// are written.
	//   /// </param>
	//   /// <exception cref="System.ObjectDisposedException">
	//   /// The System.IO.TextWriter is closed.
	//   /// </exception>
	//   /// <exception cref="System.IO.IOException">
	//   /// An I/O error occurs.
	//   /// </exception>
	//   [SecuritySafeCritical]
	//   public static void WriteLineIf( this TextWriter textWriter, bool expression, string value )
	//   {
	//      if (expression) {
	//         textWriter.WriteLine(Text.Wrap(value, Console.WindowWidth - 1));
	//      }
	//   }

	//   /// <summary>
	//   /// Writes a string followed by a line terminator to the text stream.
	//   /// </summary>
	//   /// <param name="value">
	//   /// The string to write. If value is null, only the line termination characters
	//   /// are written.
	//   /// </param>
	//   /// <param name="wrapWidth">
	//   /// </param>
	//   /// <exception cref="System.ObjectDisposedException">
	//   /// The System.IO.TextWriter is closed.
	//   /// </exception>
	//   /// <exception cref="System.IO.IOException">
	//   /// An I/O error occurs.
	//   /// </exception>
	//   [SecuritySafeCritical]
	//   public static void WriteLineIf( this TextWriter textWriter, bool expression, string value, int wrapWidth )
	//   {
	//      if (expression) {
	//         textWriter.WriteLine(Text.Wrap(value, wrapWidth));
	//      }
	//   }

	//   /// <summary>
	//   /// Writes a string followed by a line terminator to the text stream.
	//   /// </summary>
	//   /// <param name="value">
	//   /// The string to write. If value is null, only the line termination characters
	//   /// are written.
	//   /// </param>
	//   /// <param name="wrapWidth">
	//   /// </param>
	//   /// <param name="indentations">
	//   /// </param>
	//   /// <exception cref="System.ObjectDisposedException">
	//   /// The System.IO.TextWriter is closed.
	//   /// </exception>
	//   /// <exception cref="System.IO.IOException">
	//   /// An I/O error occurs.
	//   /// </exception>
	//   [SecuritySafeCritical]
	//   public static void WriteLineIf( this TextWriter textWriter, bool expression, string value, int wrapWidth, params int[] indentations )
	//   {
	//      if (expression) {
	//         textWriter.WriteLine(Text.Wrap(value, wrapWidth, indentations));
	//      }
	//   }

	//}

	/// <summary>
	/// Provides a wrapper for the ConsoleKey and ConsoleKeyInfo object.
	/// </summary>
	public class ConsoleKeyCode
	{
		/// <summary>
		/// Gets or sets the ConsoleKey.
		/// </summary>
		public ConsoleKey ConsoleKey { get; set; }

		/// <summary>
		/// Gets or sets the shift modifier.
		/// </summary>
		public bool Shift { get; set; }

		/// <summary>
		/// Gets or sets the alt modifier.
		/// </summary>
		public bool Alt { get; set; }

		/// <summary>
		/// Gets or sets the control modifier.
		/// </summary>
		public bool Control { get; set; }

		/// <summary>
		/// Gets or sets whether to ignore modifiers.
		/// </summary>
		public bool IgnoreModifiers { get; set; }

		/// <summary>
		/// Create an instance of the class. Modifiers are ignored.
		/// </summary>
		public ConsoleKeyCode( ConsoleKey ConsoleKey ) : this(ConsoleKey, false, false, false, true) { }

		/// <summary>
		/// Create an instance of the class.
		/// </summary>
		public ConsoleKeyCode( ConsoleKey ConsoleKey, bool Shift ) : this(ConsoleKey, Shift, false, false, false) { }

		/// <summary>
		/// Create an instance of the class.
		/// </summary>
		public ConsoleKeyCode( ConsoleKey ConsoleKey, bool Shift, bool Alt, bool Control ) : this(ConsoleKey, Shift, Alt, Control, false) { }

		/// <summary>
		/// Create an instance of the class.
		/// </summary>
		public ConsoleKeyCode( ConsoleKey ConsoleKey, bool Shift, bool Alt, bool Control, bool IgnoreModifiers )
		{
			this.ConsoleKey = ConsoleKey;
			this.Shift = Shift;
			this.Alt = Alt;
			this.Control = Control;
			this.IgnoreModifiers = IgnoreModifiers;
		}

		/// <summary>
		/// Create an instance of the class.
		/// </summary>
		public ConsoleKeyCode( ConsoleKeyInfo consoleKeyInfo )
		{
			this.ConsoleKey = consoleKeyInfo.Key;
			this.Shift = (consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift ? true : false;
			this.Alt = (consoleKeyInfo.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt ? true : false;
			this.Control = (consoleKeyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control ? true : false;
		}

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		public override int GetHashCode() { return base.GetHashCode(); }

		/// <summary>
		/// Returns whether this ConsoleKeyCode matches the obj passed in. This performs a value comparison not an 'object' comparison.
		/// </summary>
		public override bool Equals( object obj )
		{
			if (obj.GetType() == typeof(ConsoleKeyCode) || obj.GetType().IsSubclassOf(typeof(ConsoleKeyCode))) {
				return Equals((ConsoleKeyCode)obj);
			}
			return base.Equals(obj);
		}

		/// <summary>
		/// Returns whether this ConsoleKeyCode matches the obj passed in. This performs a value comparison not an 'object' comparison. If you want to perform a case in-sensitive comparison include the ignoreCase parameter.
		/// </summary>
		public bool Equals( ConsoleKeyCode keyCode ) { return Equals(keyCode, false); }

		/// <summary>
		/// Returns whether this ConsoleKeyCode matches the obj passed in. This performs a value comparison not an 'object' comparison.
		/// </summary>
		public bool Equals( ConsoleKeyCode keyCode, bool ignoreCase )
		{
			if (!IgnoreModifiers) {
				if (!ignoreCase && Shift != keyCode.Shift) {
					return false;
				}
				if (Alt != keyCode.Alt) {
					return false;
				}
				if (Control != keyCode.Control) {
					return false;
				}
			}
			if (this.ConsoleKey != keyCode.ConsoleKey) {
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Provides a simple way to get the command-line arguments.
	/// <remarks>See CommandLineArguments.cs.txt for details on how to use this class.</remarks>
	/// </summary>
	internal static class EnvironmentVariables
	{
		/// <summary>
		/// Returns whether the environment variable specified exists.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static bool Exists( string variable ) { return Exists(EnvironmentVariableTarget.Process, variable); }

		/// <summary>
		/// Returns whether the environment variable specified exists.
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool Exists( EnvironmentVariableTarget target, string variable ) { return Environment.GetEnvironmentVariable(variable, target) != null; }

		/// <summary>
		/// Returns whether the specified <paramref name="arguments"/> contain a non-null and non-empty value.
		/// Returns true at the first argument that has a value. If no arguments are found it returns false.
		/// </summary>
		/// <param name="variables"></param>
		/// <returns></returns>
		public static bool HasValue( params string[] variables ) { return HasValue(EnvironmentVariableTarget.Process, variables); }

		/// <summary>
		/// Returns whether the specified <paramref name="arguments"/> contain a non-null and non-empty value.
		/// Returns true at the first argument that has a value. If no arguments are found it returns false.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="variables"></param>
		/// <returns></returns>
		public static bool HasValue( EnvironmentVariableTarget target, params string[] variables )
		{
			if (variables == null) {
				throw new ArgumentNullException("variables");
			}

			for (int i = 0; i < variables.Length; i++) {
				if (Contains(target, variables) && GetString(string.Empty, target, variables[i]).Length > 0) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///  Returns whether at least one item in <paramref name="arguments"/> exists on the command-line.
		/// </summary>
		/// <param name="variables"></param>
		/// <returns></returns>
		public static bool Contains( params string[] variables ) { return IndexOf(EnvironmentVariableTarget.Process, variables) > -1; }

		/// <summary>
		///  Returns whether at least one item in <paramref name="arguments"/> exists on the command-line.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="variables"></param>
		/// <returns></returns>
		public static bool Contains( EnvironmentVariableTarget target, params string[] variables ) { return IndexOf(target, variables) > -1; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="variables"></param>
		/// <returns></returns>
		public static int IndexOf( params string[] variables ) { return IndexOf(EnvironmentVariableTarget.Process, variables); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="variables"></param>
		/// <returns></returns>
		public static int IndexOf( EnvironmentVariableTarget target, params string[] variables )
		{
			for (int i = 0; i < variables.Length; i++) {
				if (Exists(target, variables[i])) {
					return i;
				}
			}

			return -1;
		}


		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist, an empty string is returned.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static string GetString( string variable ) { return GetString(string.Empty, variable); }

		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static string GetString( string defaultValue, string variable ) { return GetString(defaultValue, EnvironmentVariableTarget.Process, variable); }

		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string GetString( string defaultValue, EnvironmentVariableTarget target, string variable )
		{
			string result;

			if (variable == null || variable.Length == 0) {
				throw new ArgumentNullException("variable");
			}

			result = Environment.GetEnvironmentVariable(variable, target);
			if (result != null && result.Length > 0) {
				return result;
			}

			return defaultValue;
		}


		/// <summary>
		/// Returns a collection of values.
		/// </summary>
		/// <param name="defaultValues"></param>
		/// <param name="variable"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static List<String> GetStringList( List<String> defaultValues, string variable, String separator ) { return GetStringList(defaultValues, EnvironmentVariableTarget.Process, variable, separator); }

		/// <summary>
		/// Returns a collection of values.
		/// </summary>
		/// <param name="defaultValues"></param>
		/// <param name="target"></param>
		/// <param name="variable"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static List<String> GetStringList( List<String> defaultValues, EnvironmentVariableTarget target, string variable, String separator )
		{
			String result;

			if (separator == null || separator.Length == 0) {
				throw new ArgumentNullException("separator");
			}
			if (variable == null || variable.Length == 0) {
				throw new ArgumentNullException("variable");
			}

			result = GetString(String.Empty, target, variable);

			if (result.Length == 0) {
				return defaultValues;
			} else {
				return new List<string>(result.Split(separator, SplitOptions.RemoveEmptyEntries));
			}
		}


		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, false is returned.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static bool GetBoolean( string variable ) { return GetBoolean(false, variable); }

		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static bool GetBoolean( bool defaultValue, string variable ) { return GetBoolean(defaultValue, EnvironmentVariableTarget.Process, variable); }

		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool GetBoolean( bool defaultValue, EnvironmentVariableTarget target, string variable )
		{
			string temp;
			bool result;

			temp = GetString(defaultValue.ToString().ToLower(), target, variable);
			if (temp != null && temp.Length > 0) {
				if (bool.TryParse(temp, out result)) {
					return result;
				} else if (temp.StartsWith("t", StringComparison.InvariantCultureIgnoreCase)
						|| temp.StartsWith("y", StringComparison.InvariantCultureIgnoreCase)
						|| temp.Equals("1")) {
					return true;
				} else {
					return false;
				}
			}

			return defaultValue;
		}


		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static int GetInt32( int defaultValue, string variable ) { return GetInt32(defaultValue, EnvironmentVariableTarget.Process, variable); }

		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static int GetInt32( int defaultValue, EnvironmentVariableTarget target, string variable )
		{
			string temp;
			int result;

			temp = GetString(defaultValue.ToString().ToLower(), target, variable);
			if (temp != null && temp.Length > 0) {
				if (int.TryParse(temp, out result)) {
					return result;
				}
			}

			return defaultValue;
		}


		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// The target (scope) is the current process.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static DateTime GetDateTime( DateTime defaultValue, string variable ) { return GetDateTime(defaultValue, EnvironmentVariableTarget.Process, variable); }

		/// <summary>
		/// Retrieves the value of an environment variable.
		/// If it does not exist or is empty, <paramref name="defaultValue"/> is returned.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="variable"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static DateTime GetDateTime( DateTime defaultValue, EnvironmentVariableTarget target, string variable )
		{
			string temp;
			DateTime result;

			temp = GetString(defaultValue.ToString().ToLower(), target, variable);
			if (temp != null && temp.Length > 0) {
				if (DateTime.TryParse(temp, out result)) {
					return result;
				}
			}

			return defaultValue;
		}
	}

#if !BRICKSOFT_SETTINGS
	public class Settings
	{
		private Dictionary<string, object> data;
		private string file;

		//public object this[string key]
		//{
		//	get
		//	{
		//		if (data.ContainsKey(key)) {
		//			return data[key];
		//		} else {
		//			return null;
		//		}
		//	}
		//	set
		//	{
		//		if (data.ContainsKey(key)) {
		//			data[key] = value;
		//		} else {
		//			data.Add(key, value);
		//		}
		//	}
		//}

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		public Settings()
		{
			data = new Dictionary<string, object>();
		}

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		public Settings( string file )
			: this()
		{
			this.file = file;
		}

		public void clear() { data.Clear(); }

		public bool read() { return read(file); }

		public bool read( string file )
		{
			string[] ar;
			string l, value, name;
			short shortVal;
			int intVal;
			long longVal;
			ulong ulongVal;
			DateTime dtVal;

			//this.file = file;
			data.Clear();

			if (!File.Exists(file)) {
				return false;
			}

			using (StreamReader reader = File.OpenText(file)) {
				while (!reader.EndOfStream) {
					l = reader.ReadLine();
					if (l == null || l.Length == 0 || l.StartsWith(";") || l.StartsWith("#") || !l.Contains("=")) {
						continue;
					}

					ar = l.Split(new char[] { '=' }, 2);
					if (ar.Length != 2) {
						continue;
					}

					name = ar[0].Trim();
					value = ar[1];

					if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)) {
						this.attr(name, true);
					} else if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
						this.attr(name, false);
						//} else if (value.StartsWith("\"") && value.EndsWith("\"")) {
						//	// string enclosed in double-quotes
						//	value = value.Substring(1, value.Length - 2);
						//	while (value.IndexOf("@\\r") > -1 || value.IndexOf("@\\n") > -1) {
						//		value = value.Replace("@\\r", "\r").Replace("@\\n", "\n");
						//	}
						//	this.attr(name, value);
					} else if (value.StartsWith("[\"") && value.EndsWith("\"]")) {
						// TODO string[]
						this.attr(name, value);
					} else if (short.TryParse(value, out shortVal)) {
						this.attr(name, shortVal);
					} else if (int.TryParse(value, out intVal)) {
						this.attr(name, intVal);
					} else if (long.TryParse(value, out longVal)) {
						this.attr(name, longVal);
					} else if (ulong.TryParse(value, out ulongVal)) {
						this.attr(name, ulongVal);
					} else if (DateTime.TryParse(value, out dtVal)) {
						this.attr(name, dtVal);
					} else {
						while (value.IndexOf("@\\r") > -1 || value.IndexOf("@\\n") > -1) {
							value = value.Replace("@\\r", "\r").Replace("@\\n", "\n");
						}
						this.attr(name, value);
					}
				}

				reader.Close();
			}

			return true;
		}

		public bool write() { return write(file); }

		public bool write( string file )
		{
			string value;

			//this.file = file;

			if (File.Exists(file)) {
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}

			using (StreamWriter w = File.CreateText(file)) {
				foreach (KeyValuePair<string, object> p in this.data) {
					if (p.Value != null) {
						if (p.Value is bool) {
							w.WriteLine(p.Key + "=" + p.Value.ToString().ToLower());
						} else if (p.Value is DateTime) {
							w.WriteLine(p.Key + "=" + p.Value.ToString());
						} else if (p.Value is string) {
							value = p.Value.ToString();
							while (value.IndexOfAny(new char[] { '\r', '\n' }) > -1) {
								value = value.Replace("\r", "@\\r").Replace("\n", "@\\n");
							}
							w.WriteLine(p.Key + "=" + value);
						} else {
							w.WriteLine(p.Key + "=" + p.Value.ToString());
						}
					}
				}
				w.Close();
			}

			return true;
		}

		/// <summary>
		/// Returns whether the specified <paramref name="key"/> exists in the settings.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool contains( string key ) { return data.ContainsKey(key); }

		/// <summary>
		/// Returns whether the specified <paramref name="key"/> exists in the settings.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="comparison"></param>
		/// <returns></returns>
		public bool contains( string key, StringComparison comparison )
		{
			foreach (KeyValuePair<string, object> entry in data) {
				if (entry.Key.Equals(key, comparison)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Removes the specified <paramref name="key"/> from the settings and returns it.
		/// If the item was not found, null is returned.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object remove( string key )
		{
			object value;

			if (data.ContainsKey(key)) {
				value = data[key];
				data.Remove(key);
				return value;
			}

			return null;
		}

		/// <summary>
		/// Gets the value of <paramref name="name"/> from the settings.
		/// Returns it as type T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public T attr<T>( string name )
		{
			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				if (typeof(T) == typeof(bool) || typeof(T).IsSubclassOf(typeof(bool))) {
					if ((object)data[name] != null) {
						return (T)(object)(data[name].ToString().StartsWith("t", StringComparison.CurrentCultureIgnoreCase));
					}
				} else if (typeof(T) == typeof(DateTime) || typeof(T).IsSubclassOf(typeof(DateTime))) {
					DateTime dt;
					if ((object)data[name] != null && DateTime.TryParse(data[name].ToString(), out dt)) {
						return (T)(object)dt;
					}
				} else if (typeof(T) == typeof(short) || typeof(T).IsSubclassOf(typeof(short))) {
					short i;
					if ((object)data[name] != null && short.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(int) || typeof(T).IsSubclassOf(typeof(int))) {
					int i;
					if ((object)data[name] != null && int.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(long) || typeof(T).IsSubclassOf(typeof(long))) {
					long i;
					if ((object)data[name] != null && long.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(ulong) || typeof(T).IsSubclassOf(typeof(ulong))) {
					ulong i;
					if ((object)data[name] != null && ulong.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(string) || typeof(T).IsSubclassOf(typeof(string))) {
					if ((object)data[name] != null) {
						return (T)(object)(data[name]).ToString();
					}
				} else {
					throw new InvalidOperationException("unknown or unsupported data type");
				}
			}

			return default(T);
		}

		/// <summary>
		/// Sets the value of <paramref name="name"/> in the settings to the <paramref name="value"/> specified.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public T attr<T>( string name, T value )
		{
			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				data[name] = value;
			} else {
				data.Add(name, value);
			}

			return value;
		}
	}
#endif

}
