//
// Copyright (C) 2003-2013 Kody Brown (kody@bricksoft.com).
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

#define BRICKSOFT_CMDLINE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

namespace Bricksoft.PowerCode
{
	/// <summary>
	/// Provides a simple way to get the command-line arguments.
	/// <remarks>
	/// See CommandLineArguments.cs.txt for details on how to use this class.
	/// </remarks>
	/// </summary>
	public class CommandLine : OrderedDictionary
	{
		/// <summary>
		/// Represents an un-named command-line argument.
		/// Unnamed items in the collection have their index appended, matching the order entered on the command-line.
		/// Unnamed items are one-based.
		/// <remarks>These arguments do not begin with a - nor /, and do not have a named item preceding them.</remarks>
		/// </summary>
		public const string UnnamedItem = "UnnamedItem";


		private Object Caller { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string[] OriginalCmdLine { get { return _originalCmdLine; } }
		private string[] _originalCmdLine;

		/// <summary>
		/// Gets the first un-named item if it exists, otherwise 
		/// returns an empty string.
		/// </summary>
		public string UnnamedItem1 { get { return GetString(string.Empty, "UnnamedItem1"); } }

		/// <summary>
		/// 
		/// </summary>
		public List<string> UnnamedItems
		{
			get
			{
				List<string> l;
				IDictionaryEnumerator enumerator;

				l = new List<string>();
				enumerator = GetEnumerator();

				while (enumerator.MoveNext()) {
					if (enumerator.Key.ToString().StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {
						l.Add(enumerator.Value as string);
					}
				}

				return l;
			}
		}

		// **** Indexer ---------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Index"></param>
		/// <returns></returns>
		public new Object this[int Index]
		{
			get { return base[Index]; }
			set { base[Index] = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		public Object this[string Key]
		{
			get
			{
				int index;

				index = GetIndexOfArgument(Key, StringComparison.CurrentCultureIgnoreCase);
				if (index == -1) {
					throw new ArgumentException("Key");
				}

				return base[index];
			}
			set
			{
				int index;

				index = GetIndexOfArgument(Key, StringComparison.CurrentCultureIgnoreCase);
				if (index == -1) {
					throw new ArgumentException("Key");
				}

				base[index] = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Key"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public Object this[string Key, StringComparison StringComparison]
		{
			get
			{
				int index;

				index = GetIndexOfArgument(Key, StringComparison);
				if (index == -1) {
					throw new ArgumentException("Key");
				}

				return base[index];
			}
			set
			{
				int index;

				index = GetIndexOfArgument(Key, StringComparison);
				if (index == -1) {
					throw new ArgumentException("Key");
				}

				base[index] = value;
			}
		}

		// **** Constructor(s) --------------------------------------------------

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		/// <param name="Arguments"></param>
		public CommandLine( string[] Arguments )
		{
			_originalCmdLine = Arguments;
			ParseCommandLine(Arguments);
		}

		// **** Parsers ---------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		private void ParseCommandLine( string[] arguments )
		{
			char[] anyOf = new char[] { '=', ':' };
			int pos = -1;

			#region Supported arguments:

			// the / indicates a single parameter
			// the - and -- indicate a parameter with a trailing value and are interchangeable..
			// 
			// /name                    ( name = true )
			// /"name"                  ( name = true )
			// /"name one"              ( name one = true )
			// "/name one"              ( name = true )
			// 
			// /name=value              ( name=value = true )
			// /name:value              ( name = value )
			// /name="value here"       ( name = value here )
			// 
			// -name value              ( name = value )
			// -name "-value"           ( name = -value )
			// -name -value             ( name = -value )
			// -"name 4" "value"        ( name 4 = value )
			// "-name 4" "value"        ( name 4 = value )
			// "-name 4" "value one"    ( name 4 = value one )
			// 
			// -name1 -name2            ( name1 = name2 )
			// 
			// -name=value              ( name = value )
			// -"name"=value            ( name = value )
			// -name="value"            ( name = value )
			// -"name"="value"          ( name = value )
			// -"name=value"            ( name = value )
			// "-name=value"            ( name = value )
			// 
			// -name="value one"        ( name = value one )
			// -"name=value one"        ( name = value one )
			// "-name=value one"        ( name = value one )
			// 
			// 
			// 
			// /name "value"            ( name = true ) and ( value = true )  <-- notice the /
			// -name "value"            ( name = value )  <-- notice the -
			// 
			// 
			// -"name 1"                
			// 

			#endregion

			string arg;
			string name;
			string value;
			bool needsValue;
			int unnamedItemCount;

			name = string.Empty;
			value = string.Empty;
			needsValue = false;
			unnamedItemCount = 0;

			if (arguments == null || arguments.Length == 0) {
				return;
			}

			for (int i = 0; i < arguments.Length; i++) {
				arg = arguments[i];

				if (needsValue && name != null && name.Length > 0) {

					// Get the value for a NameValueArg argument.
					value = arg.Trim();
					while (value.StartsWith("\"") && value.EndsWith("\"")) {
						value = value.Substring(1, value.Length - 2);
					}

					Add(name, value);
					needsValue = false;

				} else if (arg.StartsWith("-")) {

					// NameValueOptional | NameValueRequired
					name = arg.Trim();
					while (name.StartsWith("-") || (name.StartsWith("\"") && name.EndsWith("\""))) {
						name = name.TrimStart('-');
						if (name.StartsWith("\"") && name.EndsWith("\"")) {
							name = name.Substring(1, name.Length - 2);
						}
					}

					pos = name.IndexOfAny(anyOf);
					if (pos > -1) {
						value = name.Substring(pos + 1);
						if (value.StartsWith("\"") && value.EndsWith("\"")) {
							value = value.Substring(1, value.Length - 2);
						}
						name = name.Substring(0, pos);
						Add(name, value);
						needsValue = false;
					} else {
						needsValue = true;
					}

				} else if (arg.StartsWith("/")) {

					// NameOnly
					name = arg.Trim();
					while (name.StartsWith("/") || (name.StartsWith("\"") && name.EndsWith("\""))) {
						name = name.TrimStart('/');
						if (name.StartsWith("\"") && name.EndsWith("\"")) {
							name = name.Substring(1, name.Length - 2);
						}
					}

					pos = name.IndexOfAny(anyOf);
					if (pos > -1) {
						value = name.Substring(pos + 1);
						if (value.StartsWith("\"") && value.EndsWith("\"")) {
							value = value.Substring(1, value.Length - 2);
						}
						name = name.Substring(0, pos - 1);
						Add(name, value);
					} else {
						Add(name, null);
					}
					needsValue = false;

				} else {

					// UnnamedItem
					Add(UnnamedItem + (++unnamedItemCount), arg);

				}

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static CommandLine Parse( string[] arguments ) { return new CommandLine(arguments); }

		// **** Add() ---------------------------------------------------------------------------

		/// <summary>
		/// Adds an argument with the specified <paramref name="key"/> and <paramref name="value"/> 
		/// into the collection with the lowest available index.
		/// <remarks>If an existing item exists in the collection, it will be overwritten.</remarks>
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. This value can be null.</param>
		[Obsolete("Use Add(string, object) instead.", true)]
		public new void Add( object key, object value ) { Add((string)key, value); }

		/// <summary>
		/// Adds an argument with the specified <paramref name="key"/> and <paramref name="value"/> 
		/// into the collection with the lowest available index.
		/// <remarks>If an existing item exists in the collection, it will be overwritten.</remarks>
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. This value can be null.</param>
		public void Add( string key, object value ) { Add(StringComparison.CurrentCultureIgnoreCase, key, value); }

		/// <summary>
		/// Adds an argument with the specified <paramref name="key"/> and <paramref name="value"/> 
		/// into the collection with the lowest available index.
		/// <remarks>If an existing item exists in the collection, it will be overwritten.</remarks>
		/// </summary>
		/// <param name="StringComparison">Whether to ignore the case of <paramref name="key"/>.</param>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. This value can be null.</param>
		public void Add( StringComparison StringComparison, string key, object value )
		{
			if (Contains(StringComparison, key)) {
				Remove(key);
			}
			base.Add(key, value);
		}

		// **** Remove() ---------------------------------------------------------------------------

		/// <summary>
		/// Removes the entry with the specified key from the System.Collections.Specialized.OrderedDictionary collection.
		/// </summary>
		/// <param name="key">The key of the entry to remove.</param>
		/// <exception cref="System.NotSupportedException">The System.Collections.Specialized.OrderedDictionary collection is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		public new void Remove( object key ) { base.Remove(key.ToString()); }

		/// <summary>
		/// Removes the entry with the specified key from the System.Collections.Specialized.OrderedDictionary collection.
		/// </summary>
		/// <param name="parameters">The keys of the entries to remove.</param>
		/// <exception cref="System.NotSupportedException">The System.Collections.Specialized.OrderedDictionary collection is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		public void Remove( params string[] parameters )
		{
			foreach (string p in parameters) {
				base.Remove(p);
			}
		}

		// **** ToArray() ---------------------------------------------------------------------------

		/// <summary>
		/// Output all arguments as it would be entered on the command-line.
		/// </summary>
		/// <returns></returns>
		public string[] ToArray()
		{
			List<string> result;
			IDictionaryEnumerator enumerator;
			string name;
			string value;
			int pos;

			result = new List<string>();
			enumerator = GetEnumerator();

			while (enumerator.MoveNext()) {
				name = (string)enumerator.Key;
				value = enumerator.Value as string;

				if (name.StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {

					// UnnamedItem
					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Add("\"" + name + "\"");
					} else {
						result.Add(name);
					}

				} else if (value == null) {

					// StandAloneArg (/arg)
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Add("/\"" + name + "\"");
					} else {
						result.Add("/" + name);
					}

				} else {

					// NameValueArg (-name value)
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Add("-\"" + name + "\"");
					} else {
						result.Add("-" + name);
					}

					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Add("\"" + name + "\"");
					} else {
						result.Add(name);
					}

				}
			}

			return result.ToArray();
		}

		// **** ToString() ---------------------------------------------------------------------------

		/// <summary>
		/// Output all arguments as it would be entered on the command-line.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder result;
			IDictionaryEnumerator enumerator;
			string name;
			string value;
			int pos;

			result = new StringBuilder();
			enumerator = GetEnumerator();

			while (enumerator.MoveNext()) {
				name = (string)enumerator.Key;
				value = enumerator.Value as string;

				if (result.Length > 0) {
					result.Append(' ');
				}

				if (name.StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {

					// UnnamedItem
					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(value).Append('"');
					} else {
						result.Append(value);
					}

				} else if (value == null) {

					// StandAloneArg (/arg)
					result.Append('/');
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(name).Append('"');
					} else {
						result.Append(name);
					}

				} else {

					// NameValueArg (-name value)
					result.Append('-');
					pos = name.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(name).Append('"');
					} else {
						result.Append(name);
					}
					result.Append(' ');
					pos = value.IndexOf(' ');
					if (pos > -1) {
						result.Append('"').Append(value).Append('"');
					} else {
						result.Append(value);
					}

				}
			}

			return result.ToString();
		}

		// **** Contains() ---------------------------------------------------------------------------

		/// <summary>
		/// Returns whether <paramref name="argument"/> exists on the command-line.
		/// </summary>
		/// <remarks>Performs a case-insensitive comparison of <paramref name="argument"/>.</remarks>
		/// <param name="argument">The named item to search for on the command-line.</param>
		/// <returns></returns>
		public new bool Contains( object argument )
		{
			Type t;

			if (argument == null) {
				throw new ArgumentNullException("argument");
			}

			t = argument.GetType();
			if (t.Equals(typeof(string)) || t.IsSubclassOf(typeof(string))) {
				return Contains((string)argument);
			}

			throw new ArgumentException("argument must be a string.", "argument");
		}

		/// <summary>
		/// Returns whether at least one item in <paramref name="arguments"/> exists on the command-line.
		/// </summary>
		/// <remarks>Performs a case-insensitive comparison of each item in <paramref name="arguments"/>.</remarks>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool Contains( params string[] arguments ) { return Contains(StringComparison.CurrentCultureIgnoreCase, arguments); }

		/// <summary>
		/// Returns whether at least one item in <paramref name="arguments"/> exists on the command-line.
		/// </summary>
		/// <param name="IgnoreCase">Whether to ignore the case of each item in <paramref name="arguments"/>.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("Use Contains(StringComparison, params string[]) instead.", true)]
		public bool Contains( bool IgnoreCase, params string[] arguments ) { return Contains(IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture, arguments); }

		/// <summary>
		/// Returns whether at least one item in <paramref name="arguments"/> exists on the command-line.
		/// </summary>
		/// <param name="StringComparison">The type of comparison to perform.</param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public bool Contains( StringComparison StringComparison, params string[] arguments )
		{
			IDictionaryEnumerator enumerator;
			string Key;

			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			enumerator = GetEnumerator();

			while (enumerator.MoveNext()) {
				Key = (string)enumerator.Key;
				for (int i = 0; i < arguments.Length; i++) {
					if (Key.Equals(arguments[i], StringComparison)) {
						return true;
					}
				}
			}

			return false;
		}

		// **** ContainsAllOf() ---------------------------------------------------------------------------

		/// <summary>
		/// Returns whether all items in <paramref name="arguments"/> exist on the command-line.
		/// <remarks>Performs a case-insensitive comparison of each item in <paramref name="arguments"/>.</remarks>
		/// </summary>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool ContainsAllOf( params string[] arguments ) { return ContainsAllOf(StringComparison.CurrentCultureIgnoreCase, arguments); }

		/// <summary>
		/// Returns whether all items in <paramref name="arguments"/> exist on the command-line.
		/// </summary>
		/// <param name="StringComparison">Whether to ignore the case of each item in <paramref name="arguments"/>.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool ContainsAllOf( StringComparison StringComparison, params string[] arguments )
		{
			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string arg in arguments) {
				if (arg == null) {
					throw new ArgumentNullException("arguments", "An element in arguments is null");
				}
				if (!Contains(StringComparison, arg)) {
					return false;
				}
			}

			return true;
		}

		// **** IsUnnamedItem() --------------------------------------------------

		/// <summary>
		/// Returns whether <paramref name="index"/> is an un-named argument.
		/// </summary>
		/// <param name="index">The index of the argument to check whether it is an un-named argument.</param>
		/// <returns></returns>
		public bool IsUnnamedItem( int index )
		{
			int i;

			if (Keys.Count <= index) {
				return false;
				//throw new IndexOutOfRangeException("index cannot exceed collection count.");
			}

			i = 0;

			foreach (string name in Keys) {
				if (i++ == index) {
					if (name.StartsWith(UnnamedItem, StringComparison.InvariantCulture)) {
						return true;
					} else {
						return false;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the numeric index of the <paramref name="argument"/> specified.
		/// Performs comparison ignoring the case.
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		public int GetIndexOfArgument( string argument ) { return GetIndexOfArgument(argument, StringComparison.CurrentCultureIgnoreCase); }

		/// <summary>
		/// Returns the numeric index of the <paramref name="argument"/> specified.
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="StringComparison"></param>
		/// <returns></returns>
		public int GetIndexOfArgument( string argument, StringComparison StringComparison )
		{
			IDictionaryEnumerator enumerator;
			string name;
			int index;
			bool foundIt;

			enumerator = GetEnumerator();
			index = 0;
			foundIt = false;

			while (enumerator.MoveNext()) {
				name = (string)enumerator.Key;
				//value = enumerator.Value as string;
				if (name.Equals(argument, StringComparison)) {
					foundIt = true;
					break;
				}
				index++;
			}

			if (foundIt) {
				return index;
			} else {
				return -1;
			}
		}

		// **** GetRemainingString() --------------------------------------------------

		/// <summary>
		/// Returns the value of the first command-line argument found in <paramref name="arguments"/> as a string
		/// INCLUDING everything on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public string GetRemainingString( string defaultValue, params string[] arguments )
		{
			foreach (string argument in arguments) {
				if (Contains(argument) && this[argument] != null) {
					StringBuilder val = new StringBuilder();
					val.Append((string)this[argument]).Append(' ');
					for (int i = GetIndexOfArgument(argument) + 1; i < this.Count; i++) {
						val.Append((string)this[i]).Append(' ');
					}
					return val.ToString().Trim();
				}
			}
			return defaultValue;
		}

		/// <summary>
		/// Returns the value of the command-line argument as a string, found at position <paramref name="index"/>,
		/// INCLUDING everything on the command-line that followed, otherwise returns an empty string.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public string GetRemainingString( int index ) { return GetRemainingString(string.Empty, index); }

		/// <summary>
		/// Returns the value of the command-line argument as a string, found at position <paramref name="index"/>,
		/// INCLUDING everything on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public string GetRemainingString( string defaultValue, int index )
		{
			StringBuilder val;

			if (this[index] != null) {
				val = new StringBuilder();
				val.Append((string)this[index]).Append(' ');
				for (int i = index + 1; i < this.Count; i++) {
					val.Append((string)this[i]).Append(' ');
				}
				return val.ToString().Trim();
			}

			return defaultValue;
		}

		// **** GetEverythingAfter() --------------------------------------------------

		/// <summary>
		/// Finds the first command-line argument found in <paramref name="arguments"/> and returns
		/// everything AFTER it on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public string GetEverythingAfter( string defaultValue, params string[] arguments )
		{
			foreach (string argument in arguments) {
				if (Contains(argument) && this[argument] != null) {
					StringBuilder val = new StringBuilder();
					//val.Append((string)this[argument]).Append(' ');
					for (int i = GetIndexOfArgument(argument) + 1; i < this.Count; i++) {
						val.Append((string)this[i]).Append(' ');
					}
					return val.ToString().Trim();
				}
			}
			return defaultValue;
		}

		/// <summary>
		/// Finds the first command-line argument found in <paramref name="arguments"/> and returns
		/// everything AFTER it on the command-line that followed, otherwise returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public string[] GetEverythingAfter( string[] defaultValue, params string[] arguments )
		{
			List<string> result = new List<string>();

			foreach (string argument in arguments) {
				if (Contains(argument) && this[argument] != null) {
					StringBuilder val = new StringBuilder();
					//val.Append((string)this[argument]).Append(' ');
					for (int i = GetIndexOfArgument(argument) + 1; i < this.Count; i++) {
						result.Add(this.GetString(i));
					}
					return result.ToArray();
				}
			}

			return defaultValue;
		}

		// **** Exists() --------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public bool Exists( params string[] arguments ) { return Exists(StringComparison.CurrentCultureIgnoreCase, arguments); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public bool Exists( StringComparison StringComparison, params string[] arguments )
		{
			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string arg in arguments) {
				if (Contains(StringComparison, arg)) {
					return true;
				}
			}

			return false;
		}

		// **** HasValue() --------------------------------------------------

		/// <summary>
		/// Returns whether the argument(s) contain a non-null and non-empty value.
		/// If no arguments are found it returns false.
		/// Performs comparison ignoring case.
		/// </summary>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool HasValue( params string[] arguments ) { return HasValue(StringComparison.CurrentCultureIgnoreCase, arguments); }

		/// <summary>
		/// Returns whether the argument(s) contain a non-null and non-empty value.
		/// If no arguments are found it returns false.
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool HasValue( StringComparison StringComparison, params string[] arguments )
		{
			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string arg in arguments) {
				if (Contains(StringComparison, arg) && GetString(StringComparison, string.Empty, arg).Length > 0) {
					return true;
				}
			}

			return false;
		}

		// **** ReadValuesFromFile() --------------------------------------------------

		/// <summary>
		/// Returns a collection of values loaded from the specified file.
		/// One item in collection for each line in file.
		/// Lines starting with a semi-colon (;) are ignored.
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="Values"></param>
		/// <returns></returns>
		static public bool ReadValuesFromFile( string FileName, out List<String> Values )
		{
			if (FileName == null) {
				throw new ArgumentNullException("name");
			}
			if (!File.Exists(FileName)) {
				Values = null;
				return false;
			}

			Values = new List<string>();

			try {
				foreach (string line in File.ReadAllLines(FileName)) {
					// Ignore comments in file, just in case!
					if (line.StartsWith(";")) {
						continue;
					}
					Values.Add(line);
				}
			} catch (Exception ex) {
				//values = string.Empty;
				Values.Clear();
				Values.Add(ex.Message);
				return false;
			}

			return true;
		}

		#region ************************* Getters *************************

		#region **** GetValueOf()

		/// <summary>
		/// Returns the value of the first item in <paramref name="arguments"/> found.
		/// If no arguments are found it returns null.
		/// If an argument is null, it will be returned.
		/// </summary>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetValueOf(object defaultValue, string[] arguments) instead.", false)]
		public object GetValueOf( params string[] arguments ) { return GetValueOf(null, arguments); }

		/// <summary>
		/// Returns the value of the first item in <paramref name="arguments"/> found.
		/// If no arguments are found it returns <paramref name="defaultValue"/>.
		/// If an argument is null, it will be returned.
		/// </summary>
		/// <param name="defaultValue">The value to return if none of <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetValueOf(bool ignoreCase, object defaultValue, string[] arguments) instead.", false)]
		public object GetValueOf( object defaultValue, params string[] arguments ) { return GetValueOf(StringComparison.CurrentCultureIgnoreCase, defaultValue, arguments); }

		/// <summary>
		/// Returns the value of the first item in <paramref name="arguments"/> found.
		/// If no arguments are found it returns <paramref name="defaultValue"/>.
		/// If an argument is null, it will be returned.
		/// </summary>
		/// <param name="StringComparison">Whether to ignore the case of each item in <paramref name="arguments"/>.</param>
		/// <param name="defaultValue">The value to return if none of <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public object GetValueOf( StringComparison StringComparison, object defaultValue, params string[] arguments )
		{
			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string param in arguments) {
				if (Contains(StringComparison, param)) {
					return this[param];
				}
			}

			return defaultValue;
		}

		#endregion

		#region **** GetString()

		/// <summary>
		/// Returns the string value of the argument at <paramref name="index"/>.
		/// If the <paramref name="index"/> is not found, returns null.
		/// <remarks>An argument that exists but is empty will return an empty string.</remarks>
		/// </summary>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		public string GetString( int index ) { return GetString(null, index); }

		/// <summary>
		/// Returns the string value of the argument at <paramref name="index"/>.
		/// If the <paramref name="index"/> is not found, returns <paramref name="defaultValue"/>.
		/// <remarks>An argument that exists but is empty will return an empty string.</remarks>
		/// </summary>
		/// <param name="defaultValue">The string value to return if the <paramref name="index"/> is not found.</param>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		public string GetString( string defaultValue, int index )
		{
			object retVal;

			if (index < 0) {
				throw new ArgumentException("index must be zero or greater.", "index");
			}
			if (index >= Count) {
				return defaultValue;
			}

			retVal = this[index];

			if (retVal != null) {
				return (string)retVal;
			}

			return defaultValue;
		}

		/// <summary>
		/// Returns the string value of the first named argument found in <paramref name="argument"/>. 
		/// If each of <paramref name="argument"/> are not found, returns null.
		/// An argument that exists but is empty will be ignored (treated as not found).
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		public string GetString( string argument ) { return GetString(StringComparison.CurrentCultureIgnoreCase, null, argument); }

		/// <summary>
		/// Returns the string value of the first named argument found in <paramref name="arguments"/>. 
		/// If each of <paramref name="arguments"/> are not found, returns <paramref name="defaultValue"/>.
		/// An argument that exists but is empty will be ignored (treated as not found).
		/// </summary>
		/// <param name="defaultValue">The string value to return if none of the <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public string GetString( string defaultValue, params string[] arguments ) { return GetString(StringComparison.CurrentCultureIgnoreCase, defaultValue, arguments); }

		/// <summary>
		/// Returns the string value of the first named argument found in <paramref name="arguments"/>. 
		/// If each of <paramref name="arguments"/> are not found, returns <paramref name="defaultValue"/>.
		/// An argument that exists but is empty will be ignored (treated as not found).
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="defaultValue">The string value to return if none of the <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public string GetString( StringComparison StringComparison, string defaultValue, params string[] arguments )
		{
			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string argument in arguments) {
				if (Contains(StringComparison, argument) && this[argument, StringComparison.CurrentCultureIgnoreCase] != null) {
					return (string)this[argument, StringComparison];
				}
			}

			return defaultValue;
		}

		#endregion

		#region **** GetStringList()

		/// <summary>
		/// Returns a collection of values.
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="separator"></param>
		/// <param name="defaultValues"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public List<string> GetStringList( StringComparison StringComparison, string separator, List<string> defaultValues, params string[] arguments )
		{
			string result;

			if (separator == null) {
				throw new ArgumentNullException("separator");
			}
			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			result = GetString(StringComparison, string.Empty, arguments);

			if (result.Length == 0) {
				return defaultValues;
			} else {
				return new List<string>(result.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		// ----- GetBoolean() --------------------------------------------------

		/// <summary>
		/// Returns the boolean value of the argument at <paramref name="index"/>.
		/// If the <paramref name="index"/> is not found, returns false.
		/// If the <paramref name="index"/> exists but is empty or not true* will be ignored (treated as not found).
		/// <remarks>*True values (ignoring case) include: true, yes, and 1. Anything else is false.</remarks>
		/// <remarks>To assume true for an empty argument, use Exists() instead.</remarks>
		/// </summary>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetBoolean(bool defaultValue, int index) instead.", false)]
		public bool GetBoolean( int index ) { return GetBoolean(false, index); }

		/// <summary>
		/// Returns the value of the argument at <paramref name="index"/>.
		/// If the <paramref name="index"/> is not found, returns <paramref name="defaultValue"/>.
		/// If the <paramref name="index"/> exists but is empty or not true* will be ignored (treated as not found).
		/// <remarks>*True values (ignoring case) include: true, yes, and 1. Anything else is false.</remarks>
		/// <remarks>To assume true for an empty argument, use Exists() instead.</remarks>
		/// </summary>
		/// <param name="defaultValue">The boolean value to return if the index is not found.</param>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		public bool GetBoolean( bool defaultValue, int index )
		{
			string str;
			object retVal;

			if (index < 0) {
				throw new ArgumentException("index must be zero or greater.", "index");
			}
			if (index >= Count) {
				return defaultValue;
			}

			retVal = this[index];

			if (retVal != null) {
				str = retVal.ToString().Trim();
				if (str.Length > 0) {
					char p = str.ToLower(CultureInfo.InvariantCulture)[0];
					if (p.Equals('t') || p.Equals('y') || p.Equals('1')) {
						return true;
					} else if (p.Equals('f') || p.Equals('n') || p.Equals('0')) {
						return false;
					}
				}
			}

			return defaultValue;
		}

		/// <summary>
		/// Returns the boolean value of the first named argument found in <paramref name="arguments"/>. 
		/// If none of <paramref name="arguments"/> are found, returns false.
		/// An argument that exists but is empty or not true* will be ignored (treated as not found).
		/// <remarks>*True values (ignoring case) include: true, yes, and 1. Anything else is false.</remarks>
		/// <remarks>To assume true for an empty argument, use Exists() instead.</remarks>
		/// </summary>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetBoolean(bool defaultValue, string[] arguments) instead.", false)]
		public bool GetBoolean( params string[] arguments ) { return GetBoolean(false, arguments); }

		/// <summary>
		/// Returns the boolean value of the first named argument found in <paramref name="arguments"/>. 
		/// If each of <paramref name="arguments"/> are not found, returns <paramref name="defaultValue"/>.
		/// An argument that exists but is empty or not true* will be ignored (treated as not found).
		/// <remarks>True values (ignoring case) include: true, yes, and 1. Anything else is false.</remarks>
		/// <remarks>To assume true for an empty argument, use Exists() instead.</remarks>
		/// </summary>
		/// <param name="defaultValue">The boolean value to return if none of the <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool GetBoolean( bool defaultValue, params string[] arguments ) { return GetBoolean(StringComparison.CurrentCultureIgnoreCase, defaultValue, arguments); }

		/// <summary>
		/// Returns the boolean value of the first named argument found in <paramref name="arguments"/>. 
		/// If each of <paramref name="arguments"/> are not found, returns <paramref name="defaultValue"/>.
		/// An argument that exists but is empty or not true* will be ignored (treated as not found).
		/// <remarks>True values (ignoring case) include: true, yes, and 1. Anything else is false.</remarks>
		/// <remarks>To assume true for an empty argument, use Exists() instead.</remarks>
		/// </summary>
		/// <param name="StringComparison">Whether to ignore the case of each item in <paramref name="arguments"/>.</param>
		/// <param name="defaultValue">The boolean value to return if none of the <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public bool GetBoolean( StringComparison StringComparison, bool defaultValue, params string[] arguments )
		{
			string str;

			if (arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string argument in arguments) {
				if (Contains(StringComparison, argument) && this[argument] != null) {
					if (this[argument].GetType().Equals(typeof(bool))) {
						return (bool)this[argument];
					} else {
						str = this[argument].ToString().Trim();
						if (str.Length > 0) {
							char p = str.ToLower(CultureInfo.InvariantCulture)[0];
							if (p.Equals('t') || p.Equals('y') || p.Equals('1')) {
								return true;
							} else if (p.Equals('f') || p.Equals('n') || p.Equals('0')) {
								return false;
							}
						}
					}
				}
			}

			return defaultValue;
		}

		#endregion

		#region **** GetInt16()

		/// <summary>
		/// Returns the integer value of the argument at <paramref name="Index"/>.
		/// If the index is not found or is empty, returns <paramref name="DefaultValue"/>.
		/// </summary>
		/// <param name="DefaultValue">The integer value to return if the <paramref name="Index"/> is not found.</param>
		/// <param name="Index">The index of the argument to return.</param>
		/// <returns></returns>
		public short GetInt16( short DefaultValue, int Index )
		{
			string str;
			short result;
			object retVal;

			if (Index < 0) {
				throw new ArgumentException("index must be zero or greater.", "index");
			}
			if (Index >= Count) {
				return DefaultValue;
			}

			retVal = this[Index];

			if (retVal != null) {
				str = Convert.ToString(retVal, CultureInfo.InvariantCulture).Trim();
				if (short.TryParse(str, out result)) {
					return result;
				}
			}

			return DefaultValue;
		}

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="DefaultValue">The integer value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public short GetInt16( short DefaultValue, params string[] Arguments )
		{
			return GetInt16(StringComparison.CurrentCultureIgnoreCase, DefaultValue, Arguments);
		}

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="DefaultValue">The integer value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public short GetInt16( StringComparison StringComparison, short DefaultValue, params string[] Arguments )
		{
			short ret;

			if (Arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string argument in Arguments) {
				if (Contains(StringComparison, argument) && this[argument] != null) {
					if (this[argument].GetType().Equals(typeof(int))) {
						return (short)this[argument];
					} else {
						if (short.TryParse(this[argument].ToString(), out ret)) {
							return ret;
						}
					}
				}
			}

			return DefaultValue;
		}

		#endregion

		#region **** GetInt32()

		/// <summary>
		/// Returns the integer value of the argument at <paramref name="Index"/>.
		/// If the index is not found or is empty, returns 0.
		/// </summary>
		/// <param name="Index">The index of the argument to return.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetInt32(int DefaultValue, int Index) instead.", true)]
		public int GetInt32( int Index ) { return GetInt32(0, Index); }

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns 0.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetInt32(int DefaultValue, string[] Arguments) instead.", true)]
		public int GetInt32( params string[] Arguments ) { return GetInt32(0, Arguments); }

		/// <summary>
		/// Returns the integer value of the argument at <paramref name="Index"/>.
		/// If the index is not found or is empty, returns <paramref name="DefaultValue"/>.
		/// </summary>
		/// <param name="DefaultValue">The integer value to return if the <paramref name="Index"/> is not found.</param>
		/// <param name="Index">The index of the argument to return.</param>
		/// <returns></returns>
		public int GetInt32( int DefaultValue, int Index )
		{
			string str;
			int result;
			object retVal;

			if (Index < 0) {
				throw new ArgumentException("Index must be zero or greater.", "Index");
			}
			if (Index >= Count) {
				return DefaultValue;
			}

			retVal = this[Index];

			if (retVal != null) {
				str = retVal.ToString().Trim();
				if (int.TryParse(str, out result)) {
					return result;
				}
			}

			return DefaultValue;
		}

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="DefaultValue">The integer value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public int GetInt32( int DefaultValue, params string[] Arguments ) { return GetInt32(StringComparison.CurrentCultureIgnoreCase, DefaultValue, Arguments); }

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="DefaultValue">The integer value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public int GetInt32( StringComparison StringComparison, int DefaultValue, params string[] Arguments )
		{
			int ret;

			if (Arguments == null) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string argument in Arguments) {
				if (Contains(StringComparison, argument) && this[argument] != null) {
					if (this[argument].GetType().Equals(typeof(int))) {
						return (int)this[argument];
					} else {
						if (int.TryParse(this[argument].ToString(), out ret)) {
							return ret;
						}
					}
				}
			}

			return DefaultValue;
		}

		#endregion

		#region **** GetInt64()

		/// <summary>
		/// Returns the integer value of the argument at <paramref name="index"/>.
		/// If the index is not found or is empty, returns 0.
		/// </summary>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetInt64(long defaultValue, int index) instead.", true)]
		public long GetInt64( int index )
		{
			return GetInt64(0, index);
		}

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns 0.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetInt64(long defaultValue, string[] arguments) instead.", true)]
		public long GetInt64( params string[] Arguments )
		{
			return GetInt64(StringComparison.CurrentCultureIgnoreCase, 0, Arguments);
		}

		/// <summary>
		/// Returns the integer value of the argument at <paramref name="Index"/>.
		/// If the index is not found or is empty, returns <paramref name="DefaultValue"/>.
		/// </summary>
		/// <param name="DefaultValue">The integer value to return if the <paramref name="Index"/> is not found.</param>
		/// <param name="Index">The index of the argument to return.</param>
		/// <returns></returns>
		public long GetInt64( long DefaultValue, int Index )
		{
			string str;
			long result;
			object retVal;

			if (Index < 0) {
				throw new ArgumentException("Index must be zero or greater.", "Index");
			}
			if (Index >= Count) {
				return DefaultValue;
			}

			retVal = this[Index];

			if (retVal != null) {
				str = retVal.ToString().Trim();
				if (long.TryParse(str, out result)) {
					return result;
				}
			}

			return DefaultValue;
		}

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="DefaultValue">The integer value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public long GetInt64( long DefaultValue, params string[] Arguments )
		{
			return GetInt64(StringComparison.CurrentCultureIgnoreCase, DefaultValue, Arguments);
		}

		/// <summary>
		/// Returns the integer value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid integer will be ignored (treated as not found).
		/// </summary>
		/// <param name="StringComparison"></param>
		/// <param name="DefaultValue">The integer value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public long GetInt64( StringComparison StringComparison, long DefaultValue, params string[] Arguments )
		{
			long ret;

			if (Arguments == null || Arguments.Length == 0) {
				throw new ArgumentNullException("Arguments");
			}

			foreach (string argument in Arguments) {
				if (Contains(StringComparison, argument) && this[argument] != null) {
					if (this[argument].GetType().Equals(typeof(long))) {
						return (long)this[argument];
					} else {
						if (long.TryParse(this[argument].ToString(), out ret)) {
							return ret;
						}
					}
				}
			}

			return DefaultValue;
		}

		#endregion

		#region **** GetDateTime()

		/// <summary>
		/// Returns the DateTime value of the argument at <paramref name="index"/>.
		/// If the index is not found or is empty, returns 0.
		/// </summary>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetDateTime(DateTime defaultValue, int index) instead.", true)]
		public DateTime GetDateTime( int index )
		{
			return GetDateTime(DateTime.MinValue, index);
		}

		/// <summary>
		/// Returns the DateTime value of the argument at <paramref name="index"/>.
		/// If the index is not found or is empty, returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue">The DateTime value to return if the <paramref name="index"/> is not found.</param>
		/// <param name="index">The index of the argument to return.</param>
		/// <returns></returns>
		public DateTime GetDateTime( DateTime defaultValue, int index )
		{
			string str;
			DateTime result;
			object retVal;

			if (index < 0) {
				throw new ArgumentException("Index must be zero or greater.", "Index");
			}
			if (index >= Count) {
				return defaultValue;
			}

			retVal = this[index];

			if (retVal != null) {
				str = retVal.ToString().Trim();
				if (DateTime.TryParse(str, out result)) {
					return result;
				}
			}

			return defaultValue;
		}

		/// <summary>
		/// Returns the DateTime value of the first named argument found in <paramref name="arguments"/>.
		/// If none of <paramref name="arguments"/> are found, returns DateTime.MinValue.
		/// An argument that exists but is empty or is not a valid DateTime will be ignored (treated as not found).
		/// </summary>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		[Obsolete("It is recommended to use GetDateTime(DateTime defaultValue, string[] arguments) instead.", true)]
		public DateTime GetDateTime( params string[] arguments )
		{
			return GetDateTime(DateTime.MinValue, arguments);
		}

		/// <summary>
		/// Returns the DateTime value of the first named argument found in <paramref name="arguments"/>.
		/// If none of <paramref name="arguments"/> are found, returns <paramref name="defaultValue"/>.
		/// An argument that exists but is empty or is not a valid DateTime will be ignored (treated as not found).
		/// </summary>
		/// <param name="defaultValue">The DateTime value to return if none of the <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public DateTime GetDateTime( DateTime defaultValue, params string[] arguments )
		{
			return GetDateTime(StringComparison.CurrentCultureIgnoreCase, defaultValue, arguments);
		}

		/// <summary>
		/// Returns the DateTime value of the first named argument found in <paramref name="arguments"/>.
		/// If none of <paramref name="arguments"/> are found, returns <paramref name="defaultValue"/>.
		/// An argument that exists but is empty or is not a valid DateTime will be ignored (treated as not found).
		/// </summary>
		/// <param name="defaultValue">The DateTime value to return if none of the <paramref name="arguments"/> are found.</param>
		/// <param name="arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public DateTime GetDateTime( DateTime defaultValue, string allowedFormat, params string[] arguments )
		{
			return GetDateTime(StringComparison.CurrentCultureIgnoreCase, defaultValue, arguments);
		}

		/// <summary>
		/// Returns the DateTime value of the first named argument found in <paramref name="Arguments"/>.
		/// If none of <paramref name="Arguments"/> are found, returns <paramref name="DefaultValue"/>.
		/// An argument that exists but is empty or is not a valid DateTime will be ignored (treated as not found).
		/// </summary>
		/// <param name="stringComparison"></param>
		/// <param name="DefaultValue">The DateTime value to return if none of the <paramref name="Arguments"/> are found.</param>
		/// <param name="Arguments">A collection of named items to search the command-line for.</param>
		/// <returns></returns>
		public DateTime GetDateTime( StringComparison stringComparison, DateTime defaultValue, params string[] arguments )
		{
			DateTime ret;

			if (arguments == null || arguments.Length == 0) {
				throw new ArgumentNullException("arguments");
			}

			foreach (string argument in arguments) {
				if (Contains(stringComparison, argument) && this[argument] != null) {
					if (this[argument].GetType().Equals(typeof(DateTime))) {
						return (DateTime)this[argument];
					} else {
						if (DateTime.TryParse(this[argument].ToString(), out ret)) {
							return ret;
						}
					}
				}
			}

			return defaultValue;
		}

		#endregion

		#region **** GetEnum()

		// TODO:
		///// <summary>
		///// Returns the enum value of the argument at <paramref name="index"/>.
		///// If the index is not found or is empty, returns null.
		///// </summary>
		///// <param name="index">The index of the argument to return.</param>
		///// <returns></returns>
		//public object GetEnum(Type type, object defaultValue, int index) {
		//}

		#endregion

		#endregion ************************* Getters *************************
	}

	/// <summary>
	/// 
	/// </summary>
	public class CommandLineArg
	{
		/// <summary>
		/// 
		/// </summary>
		public const int DEFAULT_INDEX = (int.MaxValue / 2);

		/// <summary>
		/// 
		/// </summary>
		public const int DEFAULT_GROUP = (int.MaxValue / 2);

		/// <summary>
		/// 
		/// </summary>
		public int SortIndex { get { return _sortIndex; } set { _sortIndex = value; } }
		private int _sortIndex = DEFAULT_INDEX;

		/// <summary>
		/// 
		/// </summary>
		public int Group { get { return _group; } set { _group = value; } }
		private int _group = DEFAULT_GROUP;

		/// <summary>
		/// 
		/// </summary>
		public string Name { get { return _name ?? (_name = string.Empty); } set { _name = value ?? string.Empty; } }
		private string _name = string.Empty;

		/// <summary>
		/// Gets or sets the summary description displayed in the usage.
		/// </summary>
		public string Description { get { return _description ?? (_description = string.Empty); } set { _description = value ?? string.Empty; } }
		private string _description = string.Empty;

		/// <summary>
		/// Gets or sets the additional help content that is displayed when you call '--help cmd', where cmd is the current CommandLineArg.
		/// </summary>
		public string HelpContent { get { return _helpContent ?? (_helpContent = string.Empty); } set { _helpContent = value ?? string.Empty; } }
		private string _helpContent = string.Empty;

		/// <summary>
		/// Gets or sets the error text when the command-line argument was not provided and was set to <seealso cref="Required"/>.
		/// </summary>
		/// <remarks>
		/// If not specified the <seealso name="AppDescription"/> is used in the error message.
		/// </remarks>
		public string MissingText { get { return _missingText ?? (_missingText = string.Empty); } set { _missingText = value ?? string.Empty; } }
		private string _missingText = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public bool Required { get { return _required; } set { _required = value; } }
		private bool _required = false;

		/// <summary>
		/// 
		/// </summary>
		public string[] Keys { get { return _keys ?? (_keys = new string[] { }); } set { _keys = value ?? new string[] { }; } }
		private string[] _keys = new string[] { };

		/// <summary>
		/// 
		/// </summary>
		public DisplayMode DisplayMode { get { return _displayMode; } set { _displayMode = value; } }
		private DisplayMode _displayMode = DisplayMode.Always;

		/// <summary>
		/// 
		/// </summary>
		public CommandLineArgumentOptions Options { get { return _options; } set { _options = value; } }
		private CommandLineArgumentOptions _options = CommandLineArgumentOptions.NameValueOptional;

		/// <summary>
		/// 
		/// </summary>
		public Type InteractiveClass { get { return _interactiveClass; } set { _interactiveClass = value; } }
		private Type _interactiveClass = null;

		/// <summary>
		/// The name of the value 'cmd'.
		/// usage: blah
		///   -arg 'cmd'
		/// </summary>
		public string ExpressionLabel { get { return _expressionLabel ?? (_expressionLabel = string.Empty); } set { _expressionLabel = value ?? string.Empty; } }
		private string _expressionLabel = string.Empty;

		/// <summary>
		/// The list of allowed values of 'cmd'.
		/// usage: blah
		///   -arg 'cmd'   allowed values for 'cmd' include: ExpressionsAllowed.. 
		/// </summary>
		public Dictionary<string, string> ExpressionsAllowed { get { return _expressionsAllowed ?? (_expressionsAllowed = new Dictionary<string, string>()); } set { _expressionsAllowed = value ?? new Dictionary<string, string>(); } }
		private Dictionary<string, string> _expressionsAllowed = new Dictionary<string, string>();
		//public string[] ExpressionsAllowed { get { return _expressionsAllowed ?? (_expressionsAllowed = new string[] { }); } set { _expressionsAllowed = value ?? new string[] { }; } }
		//private string[] _expressionsAllowed = new string[] { };

		[Obsolete("use ExpressionsAllowed instead.", true)]
		public string[] AllowedExpressions { get; set; }

		/// <summary>
		/// Gets or sets whether the current command line argument is enabled or not.
		/// </summary>
		public bool Enabled { get { return _enabled; } set { _enabled = value; } }
		private bool _enabled = true;

		/// <summary>
		/// Gets whether the argument is present on the command-line or in the environment variables.
		/// <seealso cref="Exists"/> tells you whether or not the argument exists on the command-line,
		/// where <seealso cref="HasValue"/> tells you that it exists on the command-line and has a (non-empty) value.
		/// </summary>
		public bool Exists { get { return IsArgument || IsEnvironmentVariable; } }

		/// <summary>
		/// Gets whether the argument has been set.
		/// <seealso cref="Exists"/> tells you whether or not the argument exists on the command-line,
		/// where <seealso cref="HasValue"/> tells you that it exists on the command-line and has a (non-empty) value.
		/// </summary>
		public bool HasValue { get { return _hasValue; } protected internal set { _hasValue = value; } }
		private bool _hasValue = false;

		/// <summary>
		/// Gets or sets whether the value was read from the command-line arguments.
		/// </summary>
		public bool IsArgument { get { return _isArgument; } set { _isArgument = value; } }
		private bool _isArgument = false;

		/// <summary>
		/// 
		/// </summary>
		public EventHandler Handler { get; set; }

		/// <summary>
		/// Gets or sets whether the value can be read from the environment variables.
		/// A value specified on the command-line will ALWAYS take precedence over the environment variable.
		/// </summary>
		public bool AllowEnvironmentVariable { get { return _allowEnvironmentVariable; } set { _allowEnvironmentVariable = value; } }
		private bool _allowEnvironmentVariable = true;

		/// <summary>
		/// Gets or sets whether the value was read from the environment variable.
		/// </summary>
		public bool IsEnvironmentVariable { get { return _isEnvironmentVariable; } set { _isEnvironmentVariable = value; } }
		private bool _isEnvironmentVariable = false;

		/// <summary>
		/// Gets or sets whether the value is the default value (and not found in the command-line arguments, nor in the environment variables).
		/// </summary>
		public bool IsDefault { get { return _isDefault; } set { _isDefault = value; } }
		private bool _isDefault = false;

		/// <summary>
		/// Gets or sets whether the value can be read and written to the config (settings) file.
		/// </summary>
		public bool AllowConfig { get { return _allowConfig; } set { _allowConfig = value; } }
		private bool _allowConfig = false;

		/// <summary>
		/// Gets or sets whether the value was read from config (the settings file).
		/// </summary>
		public bool IsConfigItem { get { return _isConfigItem; } set { _isConfigItem = value; } }
		private bool _isConfigItem = false;
	}

	/// <summary>
	/// 
	/// </summary>
	public class CommandLineArg<T> : CommandLineArg
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate Result ValidateEventHandler( CommandLine CmdLine, CommandLineArg<T> arg );

#pragma warning disable 67

		/// <summary>
		/// Provides a method to ensure the validity of the command-line argument.
		/// </summary>
		public event ValidateEventHandler Validate;

#pragma warning restore 67

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		public Result OnValidate( CommandLine cmdLine, CommandLineArg<T> arg )
		{
			if (Validate != null) {
				return Validate(cmdLine, arg);
			}
			return new Result(Result.Okay, "");
		}

		/// <summary>
		/// Gets or sets the value of this command-line argument.
		/// </summary>
		public T Value { get { return _value != null ? _value : (_value = default(T)); } set { _value = value != null ? value : default(T); } }
		private T _value = default(T);

		/// <summary>
		/// Gets or sets the default value used if the Value was not set.
		/// </summary>
		public T Default { get { return _default != null ? _default : (_default = default(T)); } set { _default = value != null ? value : default(T); } }
		private T _default = default(T);

		#region Operator overloads

		// relational operators

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <returns></returns>
		public static bool operator ==( CommandLineArg<T> obj1, CommandLineArg<T> obj2 )
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(obj1, obj2)) {
				return true;
			}

			// If one is null, but not both, return false.
			// The ^ is an exclusive-or.
			if ((object)obj1 == null ^ (object)obj2 == null) {
				return false;
			}

			return obj1.Equals(obj2);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <returns></returns>
		public static bool operator !=( CommandLineArg<T> obj1, CommandLineArg<T> obj2 ) { return !(obj1 == obj2); }

		//public static bool operator <( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value < val2.Value; }
		//public static bool operator <( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value < val2.Value; }
		//public static bool operator <( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value < val2.Value; }

		//public static bool operator <=( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value <= val2.Value; }
		//public static bool operator <=( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value <= val2.Value; }
		//public static bool operator <=( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value <= val2.Value; }

		//public static bool operator >( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value > val2.Value; }
		//public static bool operator >( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value > val2.Value; }
		//public static bool operator >( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value > val2.Value; }

		//public static bool operator >=( CommandLineArg<short> val1, CommandLineArg<short> val2 ) { return val1.Value >= val2.Value; }
		//public static bool operator >=( CommandLineArg<int> val1, CommandLineArg<int> val2 ) { return val1.Value >= val2.Value; }
		//public static bool operator >=( CommandLineArg<long> val1, CommandLineArg<long> val2 ) { return val1.Value >= val2.Value; }

		// assignment and cast operators

		//public static explicit operator CommandLineArg<T>( T Value ) { return new CommandLineArg<T>(Value); }

		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator string( CommandLineArg<T> value )
		//{
		//	//if (value.Value is string && typeof(T) == typeof(string)) {
		//	return Convert.ToString(value.Value);
		//	//}
		//	//throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator bool( CommandLineArg<T> value )
		//{
		//	if (value.Value is bool && typeof(T) == typeof(bool)) {
		//		if (value.Value != null) {
		//			return Convert.ToBoolean(value.Value);
		//		} else {
		//			return false;
		//		}
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator DateTime( CommandLineArg<T> value )
		//{
		//	if (value.Value is DateTime && typeof(T) == typeof(DateTime)) {
		//		return Convert.ToDateTime(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static explicit operator Int16( CommandLineArg<T> value )
		//{
		//	if (value.Value is Int16 && typeof(T) == typeof(Int16)) {
		//		return Convert.ToInt16(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator Int32( CommandLineArg<T> value )
		//{
		//	if (value.Value is Int32 && typeof(T) == typeof(Int32)) {
		//		return Convert.ToInt32(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator Int64( CommandLineArg<T> value )
		//{
		//	if (value.Value is Int64 && typeof(T) == typeof(Int64)) {
		//		return Convert.ToInt64(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator UInt16( CommandLineArg<T> value )
		//{
		//	if (value.Value is UInt16 && typeof(T) == typeof(UInt16)) {
		//		return Convert.ToUInt16(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator UInt32( CommandLineArg<T> value )
		//{
		//	if (value.Value is UInt32 && typeof(T) == typeof(UInt32)) {
		//		return Convert.ToUInt32(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		///// <summary>
		///// 
		///// </summary>
		///// <param name="value"></param>
		///// <returns></returns>
		//public static implicit operator UInt64( CommandLineArg<T> value )
		//{
		//	if (value.Value is UInt64 && typeof(T) == typeof(UInt64)) {
		//		return Convert.ToUInt64(value.Value);
		//	}
		//	throw new InvalidCastException();
		//}
		//public static explicit operator int[]( CommandLineArg<int[]> value )
		//{
		//   if (value.Value is int[]) {
		//      return value.Value;
		//   }
		//   throw new InvalidCastException();
		//}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		public CommandLineArg() : this(default(T)) { }

		/// <summary>
		/// 
		/// </summary>
		public CommandLineArg( T DefaultValue ) { Default = DefaultValue; }

		/// <summary>
		/// 
		/// </summary>
		public override int GetHashCode() { return Value.GetHashCode(); }

		/// <summary>
		/// 
		/// </summary>
		public override bool Equals( object value )
		{
			CommandLineArg<T> tmp;

			// If parameter is null return false.
			if (value == null) {
				return false;
			}

			// If parameter cannot be cast to CommandLineArg<T>, return false.
			tmp = value as CommandLineArg<T>;
			if ((System.Object)tmp == null) {
				return false;
			}

			return base.Equals(tmp);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Equals( CommandLineArg<T> value )
		{
			// TODO 
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public override string ToString() { return Value.ToString(); }
	}

	/// <summary>
	/// Indicates what to expect and what is allowed for the command-line argument's values.
	/// </summary>
	[Flags]
	public enum CommandLineArgumentOptions
	{
		///// <summary>
		///// 
		///// </summary>
		//NotSet = 0,
		/// <summary>
		/// There will not be any value specified. (No value allowed.)
		/// </summary>
		NameOnly = 1,
		/// <summary>
		/// There may or may not be a value specified. (A value is optional.)
		/// </summary>
		NameValueOptional = 2,
		/// <summary>
		/// There will be a value specified. (A value is required.)
		/// </summary>
		NameValueRequired = 4,
		/// <summary>
		/// There may be multiple values specified. (At least one value is required.)
		/// </summary>
		NameRemainingValues = 8,
		/// <summary>
		/// There is no name. The value is the first argument without a prefix. (The name is optional.)
		/// </summary>
		UnnamedItem = 16,
		/// <summary>
		/// There is no name. The value is the first argument without a prefix. (The name is required.)
		/// </summary>
		UnnamedItemRequired = 32,
		/// <summary>
		/// 
		/// </summary>
		NameOnlyInteractive = 64
	}

	/// <summary>
	/// Indicates when (if ever) the command-line argument should be displayed in the usage content.
	/// </summary>
	public enum DisplayMode
	{
		/// <summary>
		/// Always displays the command-line argument.
		/// </summary>
		Always,
		/// <summary>
		/// Displays the command-line argument only when the /hidden flag was specified.
		/// </summary>
		Hidden,
		/// <summary>
		/// Will never display the command-line argument as even existing.
		/// </summary>
		Never
	}

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
}
