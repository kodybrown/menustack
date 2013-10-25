//
// Copyright (C) 2005-2007 Kody Brown (kody@bricksoft.com).
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
using System.Runtime.InteropServices;

namespace Bricksoft.PowerCode
{

#pragma warning disable 0169, 0414, 1591

	[StructLayout(LayoutKind.Sequential)]
	public struct SHFILEINFO
	{
		public IntPtr hIcon; // Handle to the icon that represents the file. 
		public IntPtr iIcon; // Index of the icon image within the system image list. 
		public uint dwAttributes; // Array of values that indicates the attributes of the file object. 
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName; // String that contains the name of the file as it appears in the Microsoft Windows Shell, or the path and file name of the file that contains the icon representing the file.
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName; // String that describes the type of file.
	};

	public class Win32
	{
		public Win32() { }

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern bool SetWindowPos(
			int hWnd,               // window handle
			int hWndInsertAfter,    // placement-order handle
			int X,                  // horizontal position
			int Y,                  // vertical position
			int cx,                 // width
			int cy,                 // height
			uint uFlags );          // window positioning flags

		public const int HWND_BOTTOM = 0x1;
		public const uint SWP_NOSIZE = 0x1;
		public const uint SWP_NOMOVE = 0x2;
		public const uint SWP_SHOWWINDOW = 0x40;
		public const uint SWP_HIDEWINDOW = 0x80;

		// file / icon stuff
		public const uint SHGFI_ICON = 0x000000100;              // get icon
		public const uint SHGFI_DISPLAYNAME = 0x000000200;       // get display name
		public const uint SHGFI_TYPENAME = 0x000000400;          // get type name
		public const uint SHGFI_ATTRIBUTES = 0x000000800;        // get attributes
		public const uint SHGFI_ICONLOCATION = 0x000001000;      // get icon location
		public const uint SHGFI_EXETYPE = 0x000002000;           // return exe type
		public const uint SHGFI_SYSICONINDEX = 0x000004000;      // get system icon index
		public const uint SHGFI_LINKOVERLAY = 0x000008000;       // put a link overlay on icon
		public const uint SHGFI_SELECTED = 0x000010000;          // show icon in selected state
		public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;    // get only specified attributes
		public const uint SHGFI_LARGEICON = 0x000000000;         // get large icon
		public const uint SHGFI_SMALLICON = 0x000000001;         // get small icon
		public const uint SHGFI_OPENICON = 0x000000002;          // get open icon
		public const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
		public const uint SHGFI_PIDL = 0x000000008;              // pszPath is a pidl
		public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010; // use passed dwFileAttribute
		public const uint SHGFI_ADDOVERLAYS = 0x000000020;       // apply the appropriate overlays
		public const uint SHGFI_OVERLAYINDEX = 0x000000040;      // Get the index of the overlay

		public const int ILD_TRANSPARENT = 0x1;

		public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
		public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

		[DllImport("shell32.dll")]
		public static extern IntPtr SHGetFileInfo( string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags );

		[DllImport("comctl32.dll", SetLastError = true)]
		public static extern IntPtr ImageList_GetIcon( IntPtr himl, long i, int flags );

		/*
				private const long TOGGLE_HIDEWINDOW = 0x80;
				private const long TOGGLE_SHOWWINDOW = 0x40;

				[DllImport("user32", CallingConvention=CallingConvention.StdCall)]
				public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

				[DllImport("user32", CallingConvention=CallingConvention.StdCall)]
				public static extern IntPtr SetWindowPos(IntPtr handleW1, long handleW1InsertWhere, long w, long X, long Y, long z, long wFlags);

				[DllImport("user32", CallingConvention=CallingConvention.StdCall)]
				public static extern int EnableWindow(IntPtr hWnd, bool bEnable);
		*/
		/*
Public Declare Function SetWindowPos Lib "user32" (ByVal handleW1 As Long, ByVal handleW1InsertWhere As Long, ByVal w As Long, ByVal X As Long, ByVal Y As Long, ByVal z As Long, ByVal wFlags As Long) As Long
Private Declare Function EnableWindow Lib "user32" (ByVal hWnd As IntPtr, ByVal bEnable As Boolean) As Integer

Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
Private Declare Function FindWindowEx Lib "user32" Alias "FindWindowExA" (ByVal hWnd1 As IntPtr, ByVal hWnd2 As IntPtr, ByVal lpsz1 As String, ByVal lpsz2 As String) As IntPtr
Private Declare Function ShowWindow Lib "user32" (ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As IntPtr
Private Declare Function SetWindowText Lib "user32" Alias "SetWindowTextA" (ByVal hWnd As IntPtr, ByVal lpString As String) As Boolean
Private Declare Function EnableWindow Lib "user32" (ByVal hWnd As IntPtr, ByVal bEnable As Boolean) As Integer
Private Declare Function GetWindowText Lib "user32" Alias "GetWindowTextA" (ByVal hWnd As IntPtr, ByVal lpString As String, ByVal nMaxCount As Integer) As Integer
Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
		*/



		/*

		Dim handle As IntPtr = prcSiclid.MainWindowHandle
		Dim Win32Help As New Win32Helper
		If Not IntPtr.Zero.Equals(handle) Then
		Win32Helper.ShowWindow(handle, 1)
		Win32Helper.SetForegroundWindow(handle)
		End If

		Public NotInheritable Class Win32Helper
		<System.Runtime.InteropServices.DllImport("user32.dll", _
		EntryPoint:="SetForegroundWindow", _
		CallingConvention:=Runtime.InteropServices.CallingConvention.StdCall, _
		CharSet:=Runtime.InteropServices.CharSet.Unicode, SetLastError:=True)> _
		Public Shared Function _
		SetForegroundWindow(ByVal handle As IntPtr) As Boolean
		' Leave function empty
		End Function

		<System.Runtime.InteropServices.DllImport("user32.dll", _
		EntryPoint:="ShowWindow", _
		CallingConvention:=Runtime.InteropServices.CallingConvention.StdCall, _
		CharSet:=Runtime.InteropServices.CharSet.Unicode, SetLastError:=True)> _
		Public Shared Function ShowWindow(ByVal handle As IntPtr, _
		ByVal nCmd As Int32) As Boolean
		' Leave function empty
		End Function

		End Class ' End Win32Helper

		*/

	}

#pragma warning restore 0169, 0414, 1591
}
