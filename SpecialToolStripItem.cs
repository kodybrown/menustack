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
using System.Drawing;
using System.Windows.Forms;

namespace MenuStack
{
	public static class ToolStripExtensions
	{
		public static ToolStripSeparator AddTag( this ToolStripSeparator @ToolStripSeparator, Object Tag )
		{
			if (@ToolStripSeparator == null) {
				return null;
			}
			@ToolStripSeparator.Tag = Tag;
			return @ToolStripSeparator;
		}

		public static ToolStripMenuItem AddTag( this ToolStripMenuItem @ToolStripMenuItem, Object Tag )
		{
			if (@ToolStripMenuItem == null) {
				return null;
			}
			@ToolStripMenuItem.Tag = Tag;
			return @ToolStripMenuItem;
		}
	}

	/// <summary>
	/// ToolStripSeparator wrapper that adds a SortName property.
	/// </summary>
	public class SortableToolStripSeparator : ToolStripSeparator
	{
		/// <summary>
		/// Gets or sets the name to be sorted by.
		/// </summary>
		public String SortName
		{
			get { return _sortName ?? (_sortName = String.Empty); }
			set { _sortName = (value != null) ? value.Trim() : String.Empty; }
		}
		private String _sortName;

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		public SortableToolStripSeparator() : this(String.Empty) { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="SortName"></param>
		public SortableToolStripSeparator( String SortName )
			: base()
		{
			this.SortName = SortName;
		}
	}

	/// <summary>
	/// ToolStripMenuItem wrapper that adds a SortName property.
	/// </summary>
	public class SortableToolStripMenuItem : ToolStripMenuItem
	{
		/// <summary>
		/// Gets or sets the name to be sorted by.
		/// </summary>
		public String SortName
		{
			get { return _sortName ?? (_sortName = String.Empty); }
			set { _sortName = (value != null) ? value.Trim() : String.Empty; }
		}
		private String _sortName;

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		public SortableToolStripMenuItem( String text ) : this(text, null, null, null) { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="SortName"></param>
		public SortableToolStripMenuItem( String text, String SortName ) : this(text, null, null, SortName) { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="image"></param>
		public SortableToolStripMenuItem( String text, Image image ) : this(text, image, null, null) { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="OnClick"></param>
		public SortableToolStripMenuItem( String text, EventHandler OnClick ) : this(text, null, OnClick, null) { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="OnClick"></param>
		/// <param name="SortName"></param>
		public SortableToolStripMenuItem( String text, EventHandler OnClick, String SortName ) : this(text, null, OnClick, SortName) { }


		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="image"></param>
		/// <param name="OnClick"></param>
		public SortableToolStripMenuItem( String text, Image image, EventHandler OnClick ) : this(text, image, OnClick, null) { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="image"></param>
		/// <param name="OnClick"></param>
		/// <param name="SortName"></param>
		public SortableToolStripMenuItem( String text, Image image, EventHandler OnClick, String SortName )
			: base(text, image, OnClick)
		{
			this.SortName = (SortName != null) ? SortName.Trim() : String.Empty;
		}
	}
}
