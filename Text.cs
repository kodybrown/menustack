/*!
	Copyright (C) 2008-2013 Kody Brown (kody@bricksoft.com).
	
	MIT License:
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to
	deal in the Software without restriction, including without limitation the
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
	sell copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
	DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Common methods for manipulating text.
	/// </summary>
	public static class Text
	{
		internal static char[] lineBreaks = new char[] { ' ', '.', ',', '-', ':', ';', '>', '"', ']', '}', '!', '?', ')', '\\', '/' };

		/* ----- WrapText() ----- */

		public static string Wrap( string Text ) { return Wrap(Text, null, null); }

		/// <summary>
		/// Wraps the specified <paramref name="Text"/> at <paramref name="WrapWidth"/>.
		/// </summary>
		/// <param name="Text">The text to wrap.</param>
		/// <param name="WrapWidth">The maximum number of characters per line.</param>
		/// <returns></returns>
		public static string Wrap( string Text, int WrapWidth ) { return Wrap(Text, WrapWidth, 0); }

		///// <summary>
		///// Wraps the specified <paramref name="Text"/> at <paramref name="WrapWidths"/>, 
		///// while indenting each line by <paramref name="Indentations"/> spaces.
		///// </summary>
		///// <param name="Text">The text to wrap.</param>
		///// <param name="WrapWidths">The maximum number of characters per line.</param>
		///// <param name="Indentations">The number of spaces to prepend onto each line.</param>
		///// <returns></returns>
		//public static string Wrap( string Text, int WrapWidths, params int[] Indentations )
		//{
		//	List<string> ar;
		//	string temp;
		//	int brk;
		//	int w;
		//	int idIndex;
		//	List<string> ids;

		//	if (WrapWidths <= 0) {
		//		WrapWidths = Console.WindowWidth;
		//	}

		//	brk = -1;
		//	ar = new List<string>(Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
		//	idIndex = 0;
		//	ids = new List<string>(Indentations.Length);

		//	Func<int> identIdx = delegate()
		//	{
		//		return Math.Min(idIndex, Indentations.Length - 1);
		//	};

		//	foreach (int indent in Indentations) {
		//		ids.Add(new string(' ', indent));
		//	}

		//	for (int i = 0; i < ar.Count; i++) {
		//		w = WrapWidths - Indentations[identIdx()] - 1;
		//		if (ar[i].Length > w) {
		//			temp = ar[i];
		//			brk = temp.Substring(0, w).LastIndexOfAny(lineBreaks) + 1;

		//			ar[i] = ids[identIdx()] + temp.Substring(0, brk);
		//			idIndex++;
		//			i++;

		//			//temp = temp.Substring(brk);
		//			if (brk < temp.Length - 1 && temp[brk] == ' ') {
		//				temp = temp.Substring(brk + 1);
		//			} else {
		//				temp = temp.Substring(brk);
		//			}

		//			while (temp.Length > 0) {
		//				w = WrapWidths - Indentations[identIdx()] - 1;
		//				if (temp.Length > w) {
		//					brk = temp.Substring(0, w).LastIndexOfAny(lineBreaks) + 1;
		//					if (brk == 0) {
		//						// The string could not be broken up nicely, 
		//						// so just cut the line at the wrap width..
		//						brk = w;
		//					}
		//				} else {
		//					brk = temp.Length;
		//				}

		//				ar.Insert(i, ids[identIdx()] + temp.Substring(0, brk));
		//				idIndex++;
		//				i++;

		//				if (brk < temp.Length - 1 && temp[brk] == ' ') {
		//					temp = temp.Substring(brk + 1);
		//				} else {
		//					temp = temp.Substring(brk);
		//				}
		//			}
		//		} else {
		//			ar[i] = ids[identIdx()] + ar[i];
		//		}

		//		idIndex++;
		//	}

		//	//return string.Join(Environment.NewLine + padding, ar);
		//	return string.Join("\n", ar);
		//}

		/// <summary>
		/// Wraps the specified <paramref name="Text"/> at <paramref name="WrapWidth"/>, 
		/// while indenting each line by <paramref name="Indentations"/> spaces.
		/// </summary>
		/// <param name="Text">The text to wrap.</param>
		/// <param name="WrapWidth">The maximum number of characters per line.</param>
		/// <param name="Indentations">The number of spaces to prepend onto each line.</param>
		/// <returns></returns>
		public static string Wrap( string Text, int WrapWidth, params int[] Indentations ) { return Wrap(Text, new int[] { WrapWidth }, Indentations); }

		public static string Wrap( string Text, int[] WrapWidths, int[] Indentations )
		{
			List<string> ar;
			string temp;
			int brk;
			int w;
			int wrapIndex, idIndex;
			List<string> ids;

			if (WrapWidths == null || WrapWidths.Length == 0) {
				WrapWidths = new int[] { Console.WindowWidth };
			}
			if (Indentations == null || Indentations.Length == 0) {
				Indentations = new int[] { 0 };
			}

			brk = -1;
			ar = new List<string>(Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
			wrapIndex = 0;
			idIndex = 0;
			ids = new List<string>(Indentations.Length);

			Func<int> wrapWidthIdx = delegate()
			{
				return Math.Min(wrapIndex, WrapWidths.Length - 1);
			};
			Func<int> identIdx = delegate()
			{
				return Math.Min(idIndex, Indentations.Length - 1);
			};

			foreach (int indent in Indentations) {
				ids.Add(new string(' ', indent));
			}

			for (int i = 0; i < ar.Count; i++) {
				w = WrapWidths[wrapWidthIdx()] - Indentations[identIdx()] - 1;
				if (ar[i].Length > w) {
					temp = ar[i];
					brk = temp.Substring(0, w).LastIndexOfAny(lineBreaks) + 1;

					ar[i] = ids[identIdx()] + temp.Substring(0, brk);
					wrapIndex++;
					idIndex++;
					i++;

					//temp = temp.Substring(brk);
					if (brk < temp.Length - 1 && temp[brk] == ' ') {
						temp = temp.Substring(brk + 1);
					} else {
						temp = temp.Substring(brk);
					}

					while (temp.Length > 0) {
						w = WrapWidths[wrapWidthIdx()] - Indentations[identIdx()] - 1;
						if (temp.Length > w) {
							brk = temp.Substring(0, w).LastIndexOfAny(lineBreaks) + 1;
							if (brk == 0) {
								// The string could not be broken up nicely, 
								// so just cut the line at the wrap width..
								brk = w;
							}
						} else {
							brk = temp.Length;
						}

						ar.Insert(i, ids[identIdx()] + temp.Substring(0, brk));
						wrapIndex++;
						idIndex++;
						i++;

						if (brk < temp.Length - 1 && temp[brk] == ' ') {
							temp = temp.Substring(brk + 1);
						} else {
							temp = temp.Substring(brk);
						}
					}
				} else {
					ar[i] = ids[identIdx()] + ar[i];
				}

				wrapIndex++;
				idIndex++;
			}

			//return string.Join(Environment.NewLine + padding, ar);
			return string.Join("\n", ar);
		}
	}
}
