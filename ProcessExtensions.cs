//
// Copyright (C) 2006-2007 Kody Brown (kody@bricksoft.com).
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
using System.Diagnostics;

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Common methods for dealing with Processes.
	/// </summary>
	/// <remarks>
	/// Example usages:
	/// 
	/// Process.GetCurrentProcess().TerminateOthers()
	/// Process.GetCurrentProcess().TerminateOthers("name")
	/// Process.GetCurrentProcess().AlreadyExists()
	/// Process.GetCurrentProcess().AlreadyExists("name")
	/// 
	/// </remarks>
	public static class ProcessExtensions
	{
		/// <summary>
		/// Terminates all processes that match the current process's name.
		/// Returns true if at least one process was found and terminated.
		/// </summary>
		/// <remarks>This calls the TerminateOthers() method.</remarks>
		/// <param name="Process"></param>
		/// <returns></returns>
		public static bool KillOthers( this Process @Process ) { return @Process.TerminateOthers(); }

		/// <summary>
		/// Terminates all other processes that match the specified process name.
		/// Returns true if at least one process was found and terminated.
		/// </summary>
		/// <remarks>This calls the TerminateOthers() method.</remarks>
		/// <param name="Process"></param>
		/// <returns></returns>
		public static bool KillOthers( this Process @Process, String Name ) { return @Process.TerminateOthers(Name); }

		/// <summary>
		/// Terminates all other processes that match the current process's name.
		/// Returns true if at least one process was found and terminated.
		/// </summary>
		/// <param name="Process"></param>
		/// <returns></returns>
		public static bool TerminateOthers( this Process @Process )
		{
			if (@Process == null) {
				@Process = Process.GetCurrentProcess();
			}

			return @Process.TerminateOthers(@Process.ProcessName);
		}

		/// <summary>
		/// Terminates all other processes that match the specified process name.
		/// Returns true if at least one process was found and terminated.
		/// </summary>
		/// <param name="Process"></param>
		/// <returns></returns>
		public static bool TerminateOthers( this Process @Process, String Name )
		{
			Process[] ps;
			int id;
			bool killed;

			if (@Process == null) {
				@Process = Process.GetCurrentProcess();
			}
			killed = false;
			id = @Process.Id;
			ps = Process.GetProcessesByName(@Process.ProcessName);

			foreach (Process p in ps) {
				if (p != null && p.Id != id) {
					p.Kill();
					killed = true;
				}
			}

			return killed;
		}

		/// <summary>
		/// Returns the number of processes that share the current process's name.
		/// </summary>
		/// <param name="Process"></param>
		/// <returns></returns>
		public static bool AlreadyExists( this Process @Process )
		{
			if (@Process == null) {
				@Process = Process.GetCurrentProcess();
			}

			return Process.GetProcessesByName(@Process.ProcessName).Length > 0;
		}

		/// <summary>
		/// Returns the number of processes that share the specified name.
		/// </summary>
		/// <param name="Process"></param>
		/// <param name="Name"></param>
		/// <returns></returns>
		public static bool AlreadyExists( this Process @Process, String Name ) { return Process.GetProcessesByName(Name).Length > 0; }
	}
}
