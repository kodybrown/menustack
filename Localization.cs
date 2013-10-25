//
// Copyright (C) 2008-2010 Kody Brown (kody@bricksoft.com).
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

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Localization methods
	/// </summary>
	public class Localization
	{
		//static public string GetText(string text) {
		//   return text;
		//}

		/// <summary>
		/// Returns the format item in <paramref name="format"/> with the text equivalent of the value of the corresponding object instance in <paramref name="args"/>.
		/// This will get the localized string BEFORE formatting the string output with <paramref name="args"/>.
		/// For example: "Como estas Kody Brown?" ==> GetText("How are you {0} {1}?", "Kody", "Brown")...
		/// First, it will get the matching localized string 'Como estas {0} {1}' or 'Watashiwa {1} {0} dis' etc.
		/// Then perform the string replacement/formatting such as 'Como estas Kody Brown' or 'Watashiwa Brown Kody dis' etc.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string GetText( string format, params object[] args )
		{
			// Clear out any numbered brackets that are not present in args 
			// and will not be replaced.
			// This allows a single localized string to include 
			// optional parameters.
			// i.e.: Loc.GetText('Message here: {0} {1}', value) -- where {1} will simply be cleared..
			//       Loc.GetText('Message here: {0} {2}', value) -- performs sequential search, so {2} will cause string.Format() to FAIL, because it's an invalid format
			//
			//int i = args.Length - 1;
			//while (true) {
			//	if (format.IndexOf("{" + (++i) + "}") > -1) { // L10N=Safe
			//		format = format.Replace("{" + i + "}", string.Empty); // L10N=Safe
			//	} else {
			//		break;
			//	}
			//}

			return string.Format(GetText(format), args);
		}

		/// <summary>
		/// Returns localized counterpart of <paramref name="text"/>.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetText( string text )
		{
			// todo:
			//if (text.Equals(ConfigClass.Names.UsernamePasswordCredential, StringComparison.CurrentCultureIgnoreCase)) {
			//   text = "Username Credential"; // L10N=Safe
			//} else if (text.Equals(ConfigClass.Names.X509Certificate, StringComparison.CurrentCultureIgnoreCase)) {
			//   text = "Certificate"; // L10N=Safe
			//}

			return text;

			// LEAVE THIS CODE HERE!!
			// By enabling this code, it will easily display all strings in the product
			// that are properly sent to GetText() for localization, thus making it simple
			// to find those strings and literals that are not correct.
			// NOTE: You must also change the JavaScript Loc.GetText() function in the 
			// Web/WebAdmin/scripts/Localization.aspx file.

			//string str = string.Empty;
			//char ch;
			//bool skip = false;

			//for (int i = 0 ; i < text.Length; i++) {
			//   ch = text[i];
			//   if (!skip && ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))) { // L10N=Safe
			//      if (i % 3 == 0) {
			//         str += "8"; // L10N=Safe
			//      } else {
			//         str += "88"; // L10N=Safe
			//      }
			//   } else {
			//      str += ch;

			//      if (ch == '<') { // L10N=Safe
			//         skip = true;
			//      } else if (ch == '>') { // L10N=Safe
			//         skip = false;
			//      }
			//   }
			//}

			//return str;
		}

		/// <summary>
		/// Writes the localized version of the input to the Console.
		/// </summary>
		/// <param name="text"></param>
		public static void Write( string text )
		{
			Console.Write(Loc.GetText(text));
		}

		/// <summary>
		/// Writes the localized version of the input to the Console.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Write( string format, params object[] args )
		{
			Console.Write(Loc.GetText(format, args));
		}

		/// <summary>
		/// Writes the localized version of the input to the Console.
		/// </summary>
		/// <param name="text"></param>
		public static void WriteLine( string text )
		{
			Console.WriteLine(Loc.GetText(text));
		}

		/// <summary>
		/// Writes the localized version of the input to the Console.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void WriteLine( string format, params object[] args )
		{
			Console.WriteLine(Loc.GetText(format, args));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Loc : Localization { }
}
