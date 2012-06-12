using System;
using System.Diagnostics;
using System.Windows.Forms;
using Bricksoft.PowerCode;

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
