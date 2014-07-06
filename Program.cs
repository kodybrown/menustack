//
// Copyright © 2004-2011 Bricksoft.com.
// All Rights Reserved.
//
// This program is unpublished proprietary source code of Bricksoft.com.
// You may not use or create derivative works from this code.
//
// Author: Kody Brown (kody@bricksoft.com)
//

using System;
using System.Diagnostics;
using Bricksoft.PowerCode;
using Application = System.Windows.Forms.Application;

namespace MenuStack
{
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main( string[] arguments )
		{
			Process.GetCurrentProcess().TerminateOthers();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PopupForm(arguments));
		}
	}
}
