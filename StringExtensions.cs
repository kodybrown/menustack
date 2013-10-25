//
// Copyright (C) 2006-2011 Kody Brown (kody@bricksoft.com).
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Options for splitting strings.
	/// </summary>
	[Flags]
	public enum SplitOptions
	{
		/// <summary>
		/// No option specified.
		/// </summary>
		None = 0,
		/// <summary>
		/// Trims each entry.
		/// </summary>
		TrimEachEntry = (1 << 0),
		/// <summary>
		/// Removes empty entries.
		/// </summary>
		RemoveEmptyEntries = (1 << 1),
	}

	/// <summary>
	/// Common extensions for strings.
	/// </summary>
	public static class StringExtensions
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static ConsoleKey ConvertToConsoleKey( string key )
		{
			ConsoleKey k;

			k = new ConsoleKey();

			return k;
		}


		// ----- StringBuilder AppendIf() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Appends the specified value, if expression evaluates to true.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="Value"></param>
		public static StringBuilder AppendIf( this StringBuilder builder, bool expression, Object Value )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.Append(Value);
			}
			return builder;
		}

		/// <summary>
		/// Appends the specified value, if expression evaluates to true.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		public static StringBuilder AppendIf( this StringBuilder builder, bool expression, Object value1, Object value2 )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.Append(value1);
			} else {
				builder.Append(value2);
			}
			return builder;
		}

		/// <summary>
		/// Appends the specified value, if expression evaluates to true.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="Value"></param>
		public static StringBuilder AppendLineIf( this StringBuilder builder, bool expression, string Value )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.AppendLine(Value);
			}
			return builder;
		}

		/// <summary>
		/// Appends the formatted string and a newline.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="format"></param>
		/// <param name="arg"></param>
		public static StringBuilder AppendLineFormat( this StringBuilder builder, string format, Object arg )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			builder.AppendFormat(format, arg);
			builder.AppendLine();
			return builder;
		}

		/// <summary>
		/// Appends the formatted string and a newline.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		public static StringBuilder AppendLineFormat( this StringBuilder builder, string format, params Object[] args )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			builder.AppendFormat(format, args);
			builder.AppendLine();
			return builder;
		}

		/// <summary>
		/// Appends the formatted string and a newline.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="format"></param>
		/// <param name="arg"></param>
		public static StringBuilder AppendLineFormatIf( this StringBuilder builder, bool expression, string format, Object arg )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.AppendFormat(format, arg);
				builder.AppendLine();
			}
			return builder;
		}

		/// <summary>
		/// Appends the formatted string and a newline.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		public static StringBuilder AppendLineFormatIf( this StringBuilder builder, bool expression, string format, params Object[] args )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.AppendFormat(format, args);
				builder.AppendLine();
			}
			return builder;
		}

		/// <summary>
		/// Appends the formatted string, if expression evaluates to true.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		public static StringBuilder AppendFormatIf( this StringBuilder builder, bool expression, string format, params Object[] args )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.AppendFormat(format, args);
			}
			return builder;
		}

		/// <summary>
		/// Appends the formatted string, if expression evaluates to true.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="expression"></param>
		/// <param name="format"></param>
		/// <param name="arg"></param>
		public static StringBuilder AppendFormatIf( this StringBuilder builder, bool expression, string format, Object arg )
		{
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}
			if (expression) {
				builder.AppendFormat(format, arg);
			}
			return builder;
		}

		// ----- Format() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns the formatted string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static string format( this string Value, params Object[] values )
		{
			if (Value == null) {
				return string.Empty;
			}

			return string.Format(Value, values);
		}

		// ----- Encoders -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Replaces &lt; and *gt; with &amp;lt; and &amp;gt; respectively.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static string EncodeHtml( this string Value )
		{
			if (Value == null) {
				return string.Empty;
			}

			if (Value.IndexOfAny(new char[] { '<', '>' }) > -1) {
				return Value.Replace("<", "&lt;").Replace(">", "&gt;");
			} else {
				return Value;
			}
		}

		/// <summary>
		/// Wraps value inside CDATA tags if it contains one of the following: &gt;, &lt;, &amp;.
		/// Also replaces ampersands (&amp;) with &amp;amp;.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static string EncodeXmlCData( this string Value )
		{
			if (Value == null) {
				return string.Empty;
			}

			if (Value.IndexOfAny(new char[] { '&', '<', '>' }) > -1) {
				return "<![CDATA[" + Value.Replace("&", "&amp;") + "]]>";
			} else {
				return Value;
			}
		}

		// ----- Split() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns a string collection that contains the substrings that are
		/// delimited by <paramref name="separator"/>. 
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="separator"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static List<string> Split( this string Value, string separator, SplitOptions options )
		{
			List<string> result;
			string[] array;
			string temp;

			result = new List<string>();
			array = Value.Split(new string[] { separator },
					(options & SplitOptions.RemoveEmptyEntries) == SplitOptions.RemoveEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			foreach (string val in array) {
				if ((options & SplitOptions.TrimEachEntry) == SplitOptions.TrimEachEntry) {
					temp = val.Trim();
				} else {
					temp = val;
				}

				if ((options & SplitOptions.RemoveEmptyEntries) == SplitOptions.RemoveEmptyEntries) {
					if (temp.Length > 0) {
						result.Add(temp);
					}
				} else {
					result.Add(temp);
				}
			}

			return result;
		}

		// ----- Join() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Joins each item in the collection separated by <paramref name="separator"/>.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string Join( this List<string> values, string separator )
		{
			return string.Join(separator, values.ToArray());
		}

		/// <summary>
		/// Joins each item in the collection separated by <paramref name="separator"/>.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string Join( this List<string> values, char separator )
		{
			return string.Join(separator.ToString(), values.ToArray());
		}

		/* ----- Repeat() ----------------------------------------------------------------------------------------------------------------------------- */

		/// <summary>
		/// Repeats <paramref name="Value"/> the specified times.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		public static string Repeat( this char Value, int Count ) { return Repeat(Value.ToString(), Count); }

		/// <summary>
		/// Repeats <paramref name="Value"/> the specified times.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		public static string Repeat( this string Value, int Count )
		{
			StringBuilder result;

			if (Value == null) {
				throw new ArgumentNullException("Value");
			}

			if (Count > 0) {
				result = new StringBuilder();

				for (int i = 0; i < Count; i++) {
					result.Append(Value);
				}

				return result.ToString();
			}

			return string.Empty;
		}

		/* ----- Replace() ----- */

		/// <summary>
		/// Returns a new string in which all occurrences of a specified string in the
		/// current instance are replaced with another specified string.
		/// </summary>
		/// <param name="me">
		/// The source string.
		/// </param>
		/// <param name="oldValue">
		/// The string to be replaced.
		/// </param>
		/// <param name="newValue">
		/// The string to replace all occurrences of oldValue.
		/// </param>
		/// <param name="comparison">
		/// The string comparison method to use.
		/// </param>
		/// <returns>
		/// A string that is equivalent to the current string except that all instances
		/// of oldValue are replaced with newValue.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// oldValue is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// oldValue is the empty string ("").
		/// </exception>
		public static string Replace( this string me, string oldValue, string newValue, StringComparison comparison )
		{
			StringBuilder sb;
			int previousIndex;
			int index;

			sb = new StringBuilder();
			previousIndex = 0;
			index = me.IndexOf(oldValue, comparison);

			while (index != -1) {
				sb.Append(me.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = me.IndexOf(oldValue, index, comparison);
			}

			sb.Append(me.Substring(previousIndex));

			return sb.ToString();
		}
		/* ----- Center(), PadCenter(), PadLeft(), PadRight() ----------------------------------------------------------------------------------------------------------------------------- */

		/// <summary>
		/// Centers <paramref name="me"/> in a new string <paramref name="Length"/> characters wide.
		/// A space is used for the padding.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Length">The total length of the string to be returned, with <paramref name="me"/> centered in.</param>
		public static string Center( this string me, int Length ) { return Center(me, Length, ' '); }

		///// <summary>
		///// Centers <paramref name="Expression"/> in a new string <paramref name="Length"/> characters wide.
		///// The <paramref name="PaddingChar"/> is used for the padding.
		///// </summary>
		///// <param name="me"></param>
		///// <param name="Length">The total length of the string to be returned, with <paramref name="Expression"/> centered in.</param>
		///// <param name="PaddingChar">The character used to pad each side of <paramref name="Expression"/> with.</param>
		//public static string PadCenter(this string me, int Length, char PaddingChar) { return PadCenter(me, Length, PaddingChar); }

		/// <summary>
		/// Centers <paramref name="me"/> in a new string <paramref name="Length"/> characters wide.
		/// The <paramref name="PaddingChar"/> is used for the padding.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Length">The total length of the string to be returned including the padding, with <paramref name="me"/> centered in.</param>
		/// <param name="PaddingChar">The character used to pad each side of <paramref name="me"/> with.</param>
		public static string Center( this string me, int Length, char PaddingChar )
		{
			int chrs;
			string pad;

			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (Length <= me.Length) {
				return me;
			}

			chrs = Math.Max(0, Convert.ToInt32((Length - me.Length) / 2));
			pad = PaddingChar.Repeat(chrs);

			if (pad.Length + me.Length + pad.Length < Length) {
				return pad + me + pad + PaddingChar;
			} else {
				return pad + me + pad;
			}
		}

		/// <summary>
		/// Aligns <paramref name="me"/> to the right, in a new string <paramref name="Length"/> characters wide.
		/// A space is used for the padding character.
		/// If the current value is longer than Length, it is returned whole.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Length">The total length of the string to be returned including the padding.</param>
		/// <returns></returns>
		public static string PadLeft( this string me, int Length ) { return PadLeft(me, Length, ' '); }

		/// <summary>
		/// Aligns <paramref name="me"/> to the right, in a new string <paramref name="Length"/> characters wide.
		/// The <paramref name="PaddingChar"/> is used for the padding character.
		/// If the current value is longer than Length, it is returned whole.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Length">The total length of the string to be returned including the padding.</param>
		/// <param name="PaddingChar">The character used to pad the left side of <paramref name="me"/> with.</param>
		public static string PadLeft( this string me, int Length, char PaddingChar )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (Length <= me.Length) {
				return me;
			}

			return PaddingChar.Repeat(Length - me.Length) + me;
		}

		/// <summary>
		/// Aligns <paramref name="me"/> to the right, in a new string <paramref name="Length"/> characters wide.
		/// The <paramref name="PaddingChar"/> is used for the padding character.
		/// If the current value is longer than Length, it is returned whole.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Length">The total length of the string to be returned including the padding.</param>
		/// <param name="PaddingChar">The character used to pad the left side of <paramref name="me"/> with.</param>
		public static string PadLeft( this int me, int Length, char PaddingChar )
		{
			if (Length <= me.ToString().Length) {
				return me.ToString();
			}

			return PaddingChar.Repeat(Length - me.ToString().Length) + me;
		}

		/// <summary>
		/// Aligns <paramref name="me"/> to the left, in a new string <paramref name="Length"/> characters wide.
		/// The <paramref name="PaddingChar"/> is used for the padding character.
		/// If the current value is longer than Length, it is returned whole.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Length">The total length of the string to be returned including the padding.</param>
		/// <param name="PaddingChar">The character used to pad the right side of <paramref name="me"/> with.</param>
		public static string PadRight( this string me, int Length, char PaddingChar )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (Length <= me.Length) {
				return me;
			}

			return me + PaddingChar.Repeat(Length - me.Length);
		}

		// ----- WrapString() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public static List<string> WrapString( this string me, int width ) { return WrapString(me, width, new char[] { ' ', '\\', '/', '-', '_', '.', ',', '?', '!', '=', '+' }); }

		/// <summary>
		/// Returns the current string after wrapping its lines at width.
		/// It wraps the string at the closest character (in chrs) that occurrs before width.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="width"></param>
		/// <param name="chrs"></param>
		/// <returns></returns>
		public static List<string> WrapString( this string me, int width, char[] chrs ) { return WrapString(me, width, chrs, true); }

		/// <summary>
		/// Returns the current string after wrapping its lines at width.
		/// It wraps the string at the closest character (in chrs) that occurrs before width.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="width"></param>
		/// <param name="chrs"></param>
		/// <param name="reformatLines">Indicates whether to remove existing line breaks before wrapping.</param>
		/// <returns></returns>
		public static List<string> WrapString( this string me, int width, char[] chrs, bool reformatLines )
		{
			List<string> retVal;
			char[] whitespace;
			int pos;
			string temp;

			if (me == null || (me = me.Trim()).Length == 0 || width < 1) {
				return new List<string>();
			}

			retVal = new List<string>();
			whitespace = new char[] { '\r', '\n', '\t' };

			if (reformatLines) {
				while (me.IndexOfAny(whitespace) > -1) {
					me = me.Replace("\r", string.Empty);
					me = me.Replace("\n", string.Empty);
					me = me.Replace("\t", string.Empty);
				}
			}

			if (me.Length < width) {
				retVal.Add(me);
				return retVal;
			}

			while (me.Length > width) {
				pos = me.IndexOfAny(chrs);

				if (pos == -1 || pos > width) {
					pos = width - 1; // just break up at the fixed width..
				}

				if (me.Length > pos + 1) {
					temp = me.Substring(0, pos + 1);
					me = me.Substring(pos + 1);
				} else {
					temp = me;
					me = string.Empty;
				}

				retVal.Add(temp);
			}

			if (me.Length > 0) {
				retVal.Add(me);
			}

			return retVal;
		}

		// ----- IsEmpty() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns whether the string is empty or not.
		/// The string is trimmed before checking for empty.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsEmpty( this string Value )
		{
			if (Value == null || Value.Trim().Length == 0) {
				return true;
			}
			return false;
		}

		// ----- MakePossessive() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns the plural form of the string.
		/// The end of the string is trimmed before checking.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static string MakePossessive( this string Value )
		{
			if (Value == null || (Value = Value.TrimEnd()).Length == 0) {
				return string.Empty;
			}
			if (Value.EndsWith("s", StringComparison.InvariantCultureIgnoreCase)) {
				return Value + "'";
			} else {
				return Value + "'s";
			}
		}

		// ----- List<string> TrimQuotes() ------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns list after removing surrounding quotes from each element.
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static List<string> TrimQuotes( this List<string> list )
		{
			List<string> newList;
			string item;

			if (list == null) {
				throw new ArgumentNullException("list");
			}

			newList = new List<string>();

			for (int i = 0; i < list.Count; i++) {
				item = list[i];

				if (item.StartsWith("\"") && item.EndsWith("\"")) {
					item = item.Substring(1, item.Length - 2);
				} else if (item.StartsWith("'") && item.EndsWith("'")) {
					item = item.Substring(1, item.Length - 2);
				}

				newList.Add(item);
			}

			return newList;
		}

		// ----- TrimQuotes() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes a single set of surrounding quotes if string starts and ends with them.
		/// The string is trimmed before checking.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static string TrimQuotes( this string Value )
		{
			if (Value == null || (Value = Value.Trim()).Length == 0) {
				return string.Empty;
			}
			if (Value.StartsWith("\"") && Value.EndsWith("\"")) {
				return Value.Substring(1, Value.Length - 2).Trim();
			}
			return Value;
		}

		// ----- TrimParens() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes a single set of surrounding parenthesis if string starts and ends with them.
		/// The string is trimmed before checking.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static string TrimParens( this string Value )
		{
			if (Value == null || (Value = Value.Trim()).Length == 0) {
				return string.Empty;
			}
			if (Value.StartsWith("(") && Value.EndsWith(")")) {
				return Value.Substring(1, Value.Length - 2).Trim();
			}
			return Value;
		}

		// ----- TrimStart() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the beginning of the string.
		/// Performs a InvariantCulture comparison.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string TrimStart( this string Value, params string[] trimStrings ) { return Value.TrimStart(StringComparison.InvariantCulture, trimStrings); }

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the beginning of the string.
		/// Performs a InvariantCulture comparison.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="MaxChars"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string TrimStart( this string Value, int MaxChars, params string[] trimStrings ) { return Value.TrimStart(MaxChars, StringComparison.InvariantCulture, trimStrings); }

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the beginning of the string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="comparison"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string TrimStart( this string Value, StringComparison comparison, params string[] trimStrings ) { return TrimStart(Value, -1, comparison, trimStrings); }

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the beginning of the string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="MaxChars"></param>
		/// <param name="comparison"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string TrimStart( this string Value, int MaxChars, StringComparison comparison, params string[] trimStrings )
		{
			string tempValue;
			bool foundOne;
			int foundCount;

			if (trimStrings == null || trimStrings.Length == 0) {
				return Value;
			}

			tempValue = Value;
			foundCount = 0;

			do {
				foundOne = false;

				foreach (string st in trimStrings) {
					if (tempValue.StartsWith(st, comparison)) {
						tempValue = tempValue.Substring(st.Length);
						foundOne = true;
						foundCount++;
					}
				}

				if (MaxChars > 0) {
					if (foundCount >= MaxChars) {
						break;
					}
				}

			} while (foundOne);

			return tempValue;
		}

		// ----- TrimEnd() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the end of the string.
		/// Performs a InvariantCulture comparison.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string TrimEnd( this string Value, params string[] trimStrings ) { return Value.TrimEnd(StringComparison.InvariantCulture, trimStrings); }

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the end of the string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="comparison"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string TrimEnd( this string Value, StringComparison comparison, params string[] trimStrings )
		{
			string tempValue;
			bool foundOne;

			if (trimStrings == null || trimStrings.Length == 0) {
				return Value;
			}

			tempValue = Value;

			do {
				foundOne = false;

				foreach (string st in trimStrings) {
					if (tempValue.EndsWith(st, comparison)) {
						tempValue = tempValue.Substring(0, tempValue.Length - st.Length);
						foundOne = true;
					}
				}
			} while (foundOne);

			return tempValue;
		}

		// ----- Trim() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the beginning and end of the string.
		/// Performs a InvariantCulture comparison.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string Trim( this string Value, params string[] trimStrings ) { return Value.Trim(StringComparison.InvariantCulture, trimStrings); }

		/// <summary>
		/// Removes all occurrances of <paramref name="trimStrings"/> from the beginning and end of the string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="comparison"></param>
		/// <param name="trimStrings"></param>
		/// <returns></returns>
		public static string Trim( this string Value, StringComparison comparison, params string[] trimStrings )
		{
			string tempValue;

			if (trimStrings == null || trimStrings.Length == 0) {
				return Value;
			}

			tempValue = Value.TrimStart(comparison, trimStrings);
			tempValue = tempValue.TrimEnd(comparison, trimStrings);

			return tempValue;
		}

		// ----- TrimStartAndEnd() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// If the string starts with trimStart and ends with trimEnd, 
		/// it will remove them from the string.
		/// </summary>
		/// <remarks>Performs an InvariantCulture comparison.</remarks>
		/// <param name="Value"></param>
		/// <param name="trimStart"></param>
		/// <param name="trimEnd"></param>
		/// <returns></returns>
		public static string TrimStartAndEnd( this string Value, string trimStart, string trimEnd ) { return Value.TrimStartAndEnd(StringComparison.InvariantCulture, trimStart, trimEnd); }

		/// <summary>
		/// If the string starts with trimStart and ends with trimEnd, 
		/// it will remove them from the string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="comparison"></param>
		/// <param name="trimStart"></param>
		/// <param name="trimEnd"></param>
		/// <returns></returns>
		public static string TrimStartAndEnd( this string Value, StringComparison comparison, string trimStart, string trimEnd )
		{
			string result;

			if ((trimStart == null || trimStart.Length == 0) || (trimEnd == null || trimEnd.Length == 0)) {
				return Value;
			}

			if (Value.StartsWith(trimStart, comparison) && Value.EndsWith(trimEnd, comparison)) {
				result = Value.TrimStart(comparison, trimStart);
				result = result.TrimEnd(comparison, trimEnd);
				return result;
			} else {
				return Value;
			}
		}

		// ----- RemoveStart() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string RemoveStart( ref string Value, int length )
		{
			string tempValue;

			if (Value == null) {
				throw new ArgumentNullException("Value");
			}
			if (Value.Length == 0) {
				return string.Empty;
			}
			if (length < 0) {
				throw new ArgumentNullException("length");
			}
			if (length > Value.Length) {
				length = Value.Length;
			}

			tempValue = Value.Substring(0, length);
			Value = Value.Substring(length);

			return tempValue;
		}

		///// <summary>
		///// Returns the len of this string up to <paramref name="length"/> long, then removes it from this string.
		///// </summary>
		///// <param name="Value"></param>
		///// <param name="length"></param>
		///// <returns></returns>
		//public static string RemoveStart(this string Value, int length) {
		//   string tempValue;

		//   if (Value == null) {
		//      throw new ArgumentNullException("Value");
		//   }
		//   if (Value.Length == 0) {
		//      return string.Empty;
		//   }
		//   if (length < 0) {
		//      throw new ArgumentNullException("length");
		//   }
		//   if (length > Value.Length) {
		//      length = Value.Length;
		//   }

		//   tempValue = Value.Substring(0, length);
		//   Value = Value.Substring(length + 1);

		//   return tempValue;
		//}

		// ----- RemoveEnd() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns the end of this string up to <paramref name="index"/> long, then removes it from this string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string RemoveEnd( this string Value, int index )
		{
			string tempValue;

			if (Value == null || Value.Length == 0) {
				throw new ArgumentNullException("Value");
			}
			if (index < 0 || index > Value.Length) {
				throw new ArgumentNullException("index");
			}

			tempValue = Value.Substring(0, index);
			Value = Value.Substring(index);

			return tempValue;
		}

		// ----- List<char> and char[] ToString() -----------------------------------------------------------------------------------------------------------------------------

		/*/// <summary>
		/// Returns Value as a string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static string ToString(this char[] Value) {
			return new List<char>(Value).ToString();
		}

		/// <summary>
		/// Returns Value as a string.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static string ToString(this List<char> Value) {
			StringBuilder result;

			result = new StringBuilder();

			foreach (char ch in Value) {
				result.Append(ch);
			}

			return result.ToString();
		}*/

		// ----- RemoveCharacters() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Removes all characters from Value that are not indicative of a number.
		/// (ie: Only the following characters will be returned: .-0123456789)
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static string RemoveCharacters( this string Value ) { return Value.RemoveCharacters("0123456789.-"); }

		/// <summary>
		/// Removes all characters from Value, except <paramref name="allowed"/>.
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="allowed"></param>
		/// <returns></returns>
		public static string RemoveCharacters( this string Value, string allowed )
		{
			StringBuilder result;

			result = new StringBuilder();

			for (int i = 0; i < Value.Length; i++) {
				if (allowed.IndexOf(Value[i]) > -1) {
					result.Append(Value[i]);
				} else if (i == 0 && Value[i] == '-') {
					result.Append(Value[i]);
				}
			}

			if (result.Length > 0) {
				return result.ToString();
			} else {
				return string.Empty;
			}
		}

		/* ----- List<string> EndsWith(string Value) -------------------------------------------------------------- */

		/// <summary>
		/// Returns whether one of the elements of Col ends with the specified Value.
		/// </summary>
		/// <param name="Col"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool EndsWith( this string[] Col, string Value ) { return EndsWith(new List<string>(Col), Value, StringComparison.InvariantCultureIgnoreCase); }

		/// <summary>
		/// Returns whether one of the elements of Col ends with the specified Value.
		/// </summary>
		/// <param name="Col"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool EndsWith( this string[] Col, string Value, StringComparison StringComparison ) { return EndsWith(new List<string>(Col), Value, StringComparison); }

		/// <summary>
		/// Returns whether one of the elements of Col ends with the specified Value.
		/// </summary>
		/// <param name="Col"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool EndsWith( this List<string> Col, string Value ) { return EndsWith(Col, Value, StringComparison.InvariantCultureIgnoreCase); }

		/// <summary>
		/// Returns whether one of the elements of Col ends with the specified Value.
		/// </summary>
		/// <param name="Col"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool EndsWith( this List<string> Col, string Value, StringComparison StringComparison )
		{
			if (Col == null) {
				throw new ArgumentNullException("Col");
			}

			if (Col.Count == 0) {
				return false;
			}

			foreach (string val in Col) {
				if (val.EndsWith(Value, StringComparison)) {
					return true;
				}
			}

			return false;
		}

		/* ----- string EndsWith(string[] Strings) -------------------------------------------------------------- */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Strings"></param>
		/// <returns></returns>
		public static bool EndsWith( string me, string[] Strings ) { return EndsWith(me, Strings, StringComparison.CurrentCulture); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Strings"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool EndsWith( this string me, string[] Strings, StringComparison StringComparison )
		{
			if (Strings != null && Strings.Length > 0) {
				for (int i = 0; i < Strings.Length; i++) {
					if (me.EndsWith(Strings[i], StringComparison)) {
						return true;
					}
				}
			}

			return false;
		}

		/* ----- string EqualsOneOf(string[] parameters) -------------------------------------------------------------- */

		/// <summary>
		/// Returns true if any of <paramref name="parameters"/> is found in <paramref name="me"/>.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static bool EqualsOneOf( this string me, params string[] parameters ) { return me.EqualsOneOf(StringComparison.InvariantCultureIgnoreCase, parameters); }

		/// <summary>
		/// Returns true if any of <paramref name="parameters"/> is found in <paramref name="me"/>.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="StringComparison"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static bool EqualsOneOf( this string me, StringComparison StringComparison, params string[] parameters )
		{
			if (me == null) {
				return false;
			}
			if (parameters == null || parameters.Length == 0) {
				return false;
			}

			foreach (string param in parameters) {
				if (me.Equals(param, StringComparison)) {
					return true;
				}
			}
			return false;
		}

		/* ----- string Contains() -------------------------------------------------------------- */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool Contains( this string me, string Value, StringComparison StringComparison )
		{
			if (me == null && Value == null) {
				return true;
			}
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (Value == null) {
				return false;
				//throw new ArgumentNullException("Value"); 
			}

			return me.IndexOf(Value, StringComparison) > -1;
		}

		/* ----- List<string> Contains() -------------------------------------------------------------- */

		/// <summary>
		/// Returns whether the collection contains the specified value.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="MatchingPaths">True to treat each string as a path (separated by a back-slash '\').</param>
		/// <returns></returns>
		public static bool Contains( this List<string> me, string Value, bool MatchingPaths )
		{
			return me.Contains(Value, StringComparison.InvariantCulture, MatchingPaths);
		}

		/// <summary>
		/// Returns whether the collection contains the specified value.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool Contains( this List<string> me, string Value, StringComparison StringComparison )
		{
			return me.Contains(Value, StringComparison, false);
		}

		/// <summary>
		/// Returns whether the collection contains the specified value.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool Contains( this string[] me, string Value, StringComparison StringComparison )
		{
			return new List<string>(me).Contains(Value, StringComparison, false);
		}

		/// <summary>
		/// Returns whether the collection contains the specified value.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <param name="MatchingPaths">True to treat each string as a path (separated by a back-slash '\').</param>
		/// <returns></returns>
		public static bool Contains( this string[] me, string Value, StringComparison StringComparison, bool MatchingPaths )
		{
			return new List<string>(me).Contains(Value, StringComparison, MatchingPaths);
		}

		/// <summary>
		/// Returns whether the collection contains the specified value.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <param name="MatchingPaths">True to treat each string as a path (separated by a back-slash '\').</param>
		/// <returns></returns>
		public static bool Contains( this List<string> me, string Value, StringComparison StringComparison, bool MatchingPaths )
		{
			string item;

			if (me == null) {
				throw new ArgumentNullException("List");
			}

			if (MatchingPaths) {
				while (Value.IndexOf("\\\\") > -1) {
					Value = Value.Replace("\\\\", "\\");
				}
				if (!Value.EndsWith("\\")) {
					Value += "\\";
				}
			}

			for (int i = 0; i < me.Count; i++) {
				item = me[i];
				if (MatchingPaths) {
					while (item.IndexOf("\\\\") > -1) {
						item = item.Replace("\\\\", "\\");
					}
					if (!item.EndsWith("\\")) {
						item += "\\";
					}
				}
				if (item.Equals(Value, StringComparison)) {
					return true;
				}
			}

			return false;
		}

		/* ----- List<string> Remove() -------------------------------------------------------------- */

		/// <summary>
		/// Removes the first occurance of the specified value from the string[] array.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool Remove( this string[] me, string Value, StringComparison StringComparison )
		{
			return new List<string>(me).Contains(Value, StringComparison);
		}

		/// <summary>
		/// Removes the first occurance of the specified value from the List&lt;string&gt;.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool Remove( this List<string> me, string Value, StringComparison StringComparison )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			for (int i = 0; i < me.Count; i++) {
				if (me[i].Equals(Value, StringComparison)) {
					me.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		/* ----- char Equals() ---------------------------------------------------------------------- */

		/// <summary>
		/// Determines whether this char and a specified System.char object have the same value, when compared using the specified casing option.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="value">A System.char object to compare against.</param>
		/// <param name="ignoreCase"></param>
		/// <returns>true if the value of the value parameter is the same as this char; otherwise, false.</returns>
		public static bool Equals( this char me, char value, bool ignoreCase )
		{
			return Equals(me, value, (ignoreCase) ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Determines whether this char and a specified System.char object have the same value, when compared using the specified comparison option.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="value">A System.char object to compare against.</param>
		/// <param name="comparisonType">One of the System.StringComparison values that determines how this char and value are compared.</param>
		/// <returns>true if the value of the value parameter is the same as this char; otherwise, false.</returns>
		public static bool Equals( this char me, char value, StringComparison comparisonType )
		{
			CultureInfo culture;
			culture = (comparisonType == StringComparison.CurrentCulture || comparisonType == StringComparison.CurrentCultureIgnoreCase) ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
			return me.ToString(culture).Equals(value.ToString(culture), comparisonType);
		}

		/* ----- char[] ConvertToString() ----------------------------------------------------------- */

		/// <summary>
		/// Converts and returns the char[] into a normal string.
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static string ConvertToString( this char[] me )
		{
			StringBuilder result;

			if (me == null || me.Length == 0) {
				return string.Empty;
			}
			if (me.Length == 1) {
				return me[0].ToString();
			}

			result = new StringBuilder();
			result.Append(me);

			return result.ToString();
		}

		/* ----- string ToBoolean() ----------------------------------------------------------------- */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static bool ToBoolean( this string me )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (me.StartsWith("t", StringComparison.CurrentCultureIgnoreCase)
					|| me.StartsWith("y", StringComparison.CurrentCultureIgnoreCase)
					|| !me.Equals("0", StringComparison.CurrentCultureIgnoreCase)) {
				return true;
			} else if (me.StartsWith("f", StringComparison.CurrentCultureIgnoreCase)
					|| me.StartsWith("n", StringComparison.CurrentCultureIgnoreCase)
					|| me.Equals("0", StringComparison.CurrentCultureIgnoreCase)) {
				return false;
			}

			throw new InvalidCastException("Cannot parse string into bool.");
		}

		/* ----- char ToLower() --------------------------------------------------------------------- */

		/// <summary>
		/// Returns a copy of this System.char converted to lowercase, using the casing rules of CultureInfo.CurrentCulture.
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static char ToLower( this char me )
		{
			return me.ToString(CultureInfo.CurrentCulture).ToLower(CultureInfo.CurrentCulture)[0];
		}

		/// <summary>
		/// Returns a copy of this System.char converted to lowercase, using the casing rules of the specified culture.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public static char ToLower( this char me, CultureInfo culture )
		{
			return me.ToString(culture).ToLower(culture)[0];
		}

		/// <summary>
		/// Returns a copy of this System.char converted to uppercase, using the casing rules of CultureInfo.CurrentCulture.
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static char ToUpper( this char me )
		{
			return me.ToString(CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture)[0];
		}

		/// <summary>
		/// Returns a copy of this System.char converted to uppercase, using the casing rules of the specified culture.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public static char ToUpper( this char me, CultureInfo culture )
		{
			return me.ToString(culture).ToUpper(culture)[0];
		}

		/* ----- List<char> Contains() -------------------------------------------------------------- */

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static string ToProperCase( this string me ) { return ToProperCase(me, CultureInfo.CurrentUICulture); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public static string ToProperCase( this string me, CultureInfo culture )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (me.Length < 1) {
				return me.ToUpper(culture);
			}
			return me[0].ToUpper(culture) + me.Substring(1).ToLower(culture);
		}

		/* ----- List<char> Contains() -------------------------------------------------------------- */

		/// <summary>
		/// Returns a value indicating whether the specified System.char object occurs within this char array, when compared using the StringComparison.CurrentCulture comparison option.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="value">The System.char object to seek.</param>
		/// <returns>true if the value parameter occurs within this string; otherwise, false.</returns>
		public static bool Contains( this char[] me, char value )
		{
			return new List<char>(me).Contains(value, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Returns a value indicating whether the specified System.char object occurs within this char array, when compared using the StringComparison.CurrentCulture comparison option.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="value">The System.char object to seek.</param>
		/// <returns>true if the value parameter occurs within this string; otherwise, false.</returns>
		public static bool Contains( this List<char> me, char value )
		{
			return me.Contains(value, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Returns a value indicating whether the specified System.char object occurs within this char array, when compared using the specified comparison option.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="value">The System.char object to seek.</param>
		/// <param name="comparisonType">One of the System.StringComparison values that determines how this char array and value are compared.</param>
		/// <returns>true if the value parameter occurs within this string; otherwise, false.</returns>
		public static bool Contains( this char[] me, char value, StringComparison comparisonType )
		{
			return new List<char>(me).Contains(value, comparisonType);
		}

		/// <summary>
		/// Returns a value indicating whether the specified System.char object occurs within this char array, when compared using the specified comparison option.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="value">The System.char object to seek.</param>
		/// <param name="comparisonType">One of the System.StringComparison values that determines how this char collection and value are compared.</param>
		/// <returns>true if the value parameter occurs within this string; otherwise, false.</returns>
		public static bool Contains( this List<char> me, char value, StringComparison comparisonType )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (me.Count == 0) {
				return false;
			}

			foreach (char curCh in me) {
				if (Equals(curCh, value, comparisonType)) {
					return true;
				}
			}

			return false;
		}

		// ----- ContainsKey() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsKey( this Dictionary<string, string> me, string Value, StringComparison StringComparison )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			foreach (KeyValuePair<string, string> item in me) {
				if (item.Key.Equals(Value, StringComparison)) {
					return true;
				}
			}

			return false;
		}

		// ----- CountOccurrances() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static int CountOccurrences( this string me, string Value )
		{
			int result;
			int pos;

			if (me == null) {
				throw new ArgumentNullException("me");
			}

			result = 0;
			pos = 0 - Value.Length;

			while ((pos = me.IndexOf(Value, pos + Value.Length)) > -1) {
				result++;
			}

			return result;
		}

		// ----- IsNumeric() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns whether <paramref name="Value"/> is a (long) integer value.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsNumeric( this string Value )
		{
			Int64 result;

			if (Int64.TryParse(Value, out result)) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns whether <paramref name="Value"/> is a (short) integer value.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsInt16( this string Value )
		{
			Int16 result;

			if (short.TryParse(Value, out result)) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns whether <paramref name="Value"/> is a integer value (int).
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsInt32( this Object Value )
		{
			int result;

			if (Value == null) {
				return false;
			}

			if (int.TryParse(Value.ToString(), out result)) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns whether <paramref name="Value"/> is an integer value (int).
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsInt32( this string Value )
		{
			int result;

			if (int.TryParse(Value, out result)) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns whether <paramref name="Value"/> is a (long) integer value.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsInt64( this string Value )
		{
			Int64 result;

			if (long.TryParse(Value, out result)) {
				return true;
			}

			return false;
		}

		// ----- MakePathSafe() -----------------------------------------------------------------------------------------------------------------------------

		/*/// <summary>
		/// Returns <paramref name="path"/> with all invalid characters removed (or replaced).
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string MakePathSafe(this string path) {
			StringBuilder result;
			char[] badChars;

			result = new StringBuilder();
			result.Append(path.Trim());

			badChars = Path.GetInvalidPathChars();

			for (int i = 0; i < badChars.Length; i++) {
				result.Replace(badChars[i].ToString(), "");
			}

			result.Replace("=", "");
			result.Replace(":", "-");
			result.Replace("?", "");
			result.Replace("*", "");
			result.Replace("/", "-");

			return result.ToString();
		}*/

		// ----- IsEmail() -----------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Returns whether the string is a valid email.
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public static bool IsEmail( this string Value )
		{
			Regex r;
			Match m;

			if (Value == null || (Value = Value.Trim()).Length == 0) {
				return false;
			}
			if (Value.IndexOf('\'') > -1 || Value.IndexOf('"') > -1) {
				return false;
			}

			r = new Regex("(?<login>[^@]+)@(?<host>.+)\\.(?<domain>.+)");
			m = r.Match(Value);

			if (m.Success) {
				return true;
			} else {
				return false;
			}
		}




		// ----- List<string> CompareTo() ------------------------------------------------------------------------------------------------------------------------------------------------------

		/* ----- List<string> CompareTo(me, list) ----- */

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool CompareTo( this string[] me, string[] list ) { return CompareTo(me, list, StringComparison.InvariantCulture); }

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool CompareTo( this string[] me, List<string> list ) { return CompareTo(me, list, StringComparison.InvariantCulture); }

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool CompareTo( this List<string> me, string[] list ) { return CompareTo(me, list, StringComparison.InvariantCulture); }

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool CompareTo( this List<string> me, List<string> list ) { return CompareTo(me, list, StringComparison.InvariantCulture); }

		/* ----- List<string> CompareTo(me, list, StringComparison) ----- */

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool CompareTo( this string[] me, string[] list, StringComparison StringComparison ) { return CompareTo(me, list, StringComparison, true); }

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool CompareTo( this string[] me, List<string> list, StringComparison StringComparison ) { return CompareTo(me, list, StringComparison, true); }

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool CompareTo( this List<string> me, string[] list, StringComparison StringComparison ) { return CompareTo(me, list, StringComparison, true); }

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// Ignores the ordering of the elements of each collection.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool CompareTo( this List<string> me, List<string> list, StringComparison StringComparison ) { return CompareTo(me, list, StringComparison, true); }

		/* ----- List<string> CompareTo(me, list, StringComparison, IgnoreOrder) ----- */

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <param name="IgnoreOrder"></param>
		/// <returns></returns>
		public static bool CompareTo( this string[] me, string[] list, StringComparison StringComparison, bool IgnoreOrder )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (list == null) {
				return false;
			} else if (me.Length != list.Length) {
				return false;
			}

			return CompareTo(new List<string>(me), new List<string>(list), StringComparison, IgnoreOrder);
		}

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <param name="IgnoreOrder"></param>
		/// <returns></returns>
		public static bool CompareTo( this string[] me, List<string> list, StringComparison StringComparison, bool IgnoreOrder )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (list == null) {
				return false;
			} else if (me.Length != list.Count) {
				return false;
			}

			return CompareTo(new List<string>(me), list, StringComparison, IgnoreOrder);
		}

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <param name="IgnoreOrder"></param>
		/// <returns></returns>
		public static bool CompareTo( this List<string> me, string[] list, StringComparison StringComparison, bool IgnoreOrder )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (list == null) {
				return false;
			} else if (me.Count != list.Length) {
				return false;
			}

			return CompareTo(me, new List<string>(list), StringComparison, IgnoreOrder);
		}

		/// <summary>
		/// Returns true if <paramref name="list"/> matches the current collection, otherwise false.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <param name="IgnoreOrder"></param>
		/// <returns></returns>
		public static bool CompareTo( this List<string> me, List<string> list, StringComparison StringComparison, bool IgnoreOrder )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}

			if (list == null) {
				return true;
			} else if (me.Count != list.Count) {
				return false;
			}

			if (IgnoreOrder) {
				if (!ContainsAllOfList(me, list, StringComparison)) {
					return false;
				}
				if (!ContainsAllOfList(list, me, StringComparison)) {
					return false;
				}
			} else {
				for (int i = 0; i < me.Count; i++) {
					if (!me[i].Equals(list[i], StringComparison)) {
						return false;
					}
				}
			}

			return true;
		}

		// ----- List<string> ContainsAllOfList() ------------------------------------------------------------------------------------------------------------------------------------------------------

		/* ----- List<string> ContainsAllOfList(list1, list) ----- */

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( string[] list1, string[] list2 ) { return ContainsAllOfList(list1, list2, StringComparison.InvariantCulture); }

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( string[] list1, List<string> list2 ) { return ContainsAllOfList(list1, list2, StringComparison.InvariantCulture); }

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( List<string> list1, string[] list2 ) { return ContainsAllOfList(list1, list2, StringComparison.InvariantCulture); }

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Performs a case-sensitive comparison by default, ignoring the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( List<string> list1, List<string> list2 ) { return ContainsAllOfList(list1, list2, StringComparison.InvariantCulture); }

		/* ----- List<string> ContainsAllOfList(list1, list, StringComparison) ----- */

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( string[] list1, string[] list2, StringComparison StringComparison )
		{
			if (list1 == null || list2 == null) {
				return false;
			} else if (list1.Length != list2.Length) {
				return false;
			}

			return ContainsAllOfList(new List<string>(list1), new List<string>(list2), StringComparison);
		}

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( string[] list1, List<string> list2, StringComparison StringComparison )
		{
			if (list1 == null || list2 == null) {
				return false;
			} else if (list1.Length != list2.Count) {
				return false;
			}

			return ContainsAllOfList(new List<string>(list1), list2, StringComparison);
		}

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( List<string> list1, string[] list2, StringComparison StringComparison )
		{
			if (list1 == null || list2 == null) {
				return false;
			} else if (list1.Count != list2.Length) {
				return false;
			}

			return ContainsAllOfList(list1, new List<string>(list2), StringComparison);
		}

		/// <summary>
		/// Returns true if <paramref name="list1"/> matches <paramref name="list"/>, otherwise false.
		/// Ignores the order of the elements of each collection.
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsAllOfList( List<string> list1, List<string> list2, StringComparison StringComparison )
		{
			if (list1 == null || list2 == null) {
				return false;
			} else if (list1.Count != list2.Count) {
				return false;
			}

			foreach (string s in list1) {
				if (!list2.Contains(s, StringComparison)) {
					return false;
				}
			}

			//bool foundStr;
			//foreach (string strA in list1) {
			//   foundStr = false;
			//   foreach (string strB in list) {
			//      if (strA.Equals(strB, StringComparison)) {
			//         foundStr = true;
			//         break;
			//      }
			//   }
			//   if (!foundStr) {
			//      return false;
			//   }
			//}

			return true;
		}

		/* ----- ContainsAllOf() -------------------------------------------------- */

		/// <summary>
		/// Returns true if the current collection (<paramref name="me"/>) contains every item in <paramref name="list"/>, otherwise false.
		/// The order of items within each collection is ignored.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool ContainsAllOf( this string[] me, List<string> list ) { return ContainsAllOf(me, list, StringComparison.CurrentCultureIgnoreCase); }

		/// <summary>
		/// Returns true if the current collection (<paramref name="me"/>) contains every item in <paramref name="list"/>, otherwise false.
		/// The order of items within each collection is ignored.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsAllOf( this string[] me, List<string> list, StringComparison StringComparison ) { return ContainsAllOf(new List<string>(me), list, StringComparison.CurrentCultureIgnoreCase); }

		/// <summary>
		/// Returns true if the current collection (<paramref name="me"/>) contains every item in <paramref name="list"/>, otherwise false.
		/// The order of items within each collection is ignored.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool ContainsAllOf( this List<string> me, List<string> list ) { return ContainsAllOf(me, list, StringComparison.CurrentCultureIgnoreCase); }

		/// <summary>
		/// Returns true if the current collection (<paramref name="me"/>) contains every item in <paramref name="list"/>, otherwise false.
		/// The order of items within each collection is ignored.
		/// </summary>
		/// <param name="me"></param>
		/// <param name="list"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public static bool ContainsAllOf( this List<string> me, List<string> list, StringComparison StringComparison )
		{
			if (me == null || list == null) {
				return false;
			} else if (me.Count > list.Count) {
				return false;
			}

			foreach (string s in list) {
				if (!me.Contains(s, StringComparison)) {
					return false;
				}
			}

			return true;
		}

		/* ----- RemoveDuplicates() -------------------------------------------------- */

		/// <summary>
		/// Removes all duplicate entries, leaving the first item found.
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static int RemoveDuplicates( this string[] me ) { return RemoveDuplicates(new List<string>(me)); }

		/// <summary>
		/// Removes all duplicate entries, leaving the first item found.
		/// </summary>
		/// <param name="me"></param>
		/// <returns></returns>
		public static int RemoveDuplicates( this List<string> me )
		{
			int removedCount = 0;

			for (int i = 0; i < me.Count; i++) {
				for (int j = me.Count - 1; j >= i; j--) { // TODO j >= i or 0 ?
					if (i != j && me[i] == me[j]) {
						me.RemoveAt(j);
						removedCount++;
						break;
					}
				}
			}

			return removedCount;
		}

		// ----- List<string> LoadFromFile() and SaveToFile() ------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static bool LoadFromFile( this List<string> me, string fileName )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (fileName == null || (fileName = fileName.Trim()).Length == 0) {
				throw new ArgumentNullException("fileName");
			}

			me.Clear();

			try {
				if (!File.Exists(fileName)) {
					return false;
				}

				using (StreamReader reader = File.OpenText(fileName)) {
					while (!reader.EndOfStream) {
						me.Add(reader.ReadLine());
					}
					reader.Close();
				}
			} catch (Exception) {

			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static bool SaveToFile( this List<string> me, string fileName ) { return SaveToFile(me, fileName, true); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="fileName"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public static bool SaveToFile( this List<string> me, string fileName, bool overwrite )
		{
			if (fileName == null || (fileName = fileName.Trim()).Length == 0) {
				throw new ArgumentNullException("fileName");
			}

			try {
				if (File.Exists(fileName)) {
					if (!overwrite) {
						return false;
					} else {
						File.SetAttributes(fileName, FileAttributes.Normal);
						File.Delete(fileName);
					}
				}

				using (StreamWriter writer = File.CreateText(fileName)) {
					// note: if me is null, then we're just
					// going to write out an empty file.
					if (me != null) {
						foreach (string line in me) {
							writer.WriteLine(line);
						}
					}
					writer.Flush();
					writer.Close();
				}
			} catch (Exception) {

			}

			return true;
		}


		// ----- List<string> LoadFromFile() and SaveToFile() ------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="me"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static bool LoadFromFile( this StringBuilder me, string fileName )
		{
			if (me == null) {
				throw new ArgumentNullException("me");
			}
			if (fileName == null || (fileName = fileName.Trim()).Length == 0) {
				throw new ArgumentNullException("fileName");
			}

			me.Length = 0;

			try {
				if (!File.Exists(fileName)) {
					return false;
				}

				using (StreamReader reader = File.OpenText(fileName)) {
					while (!reader.EndOfStream) {
						me.AppendLine(reader.ReadLine());
					}
					reader.Close();
				}
			} catch (Exception) {

			}

			return true;
		}


	}


	/*public class SafeStringList : List<string> {
		private object _addLock = null;

		public SafeStringList() { }

		public SafeStringList(List<string> list) {
			foreach (string item in list) {
				base.Add(item);
			}
		}

		public new bool Add(string Value) {
			lock (_addLock) {
				if (!base.Contains(Value)) {
					base.Add(Value);
					return true;
				}
			}
			return false;
		}

		public new bool AddRange(IEnumerable<string> collection) {
			
			return false;
		}
	}*/

}
