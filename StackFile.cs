using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace AppMenuStack
{
	public class FileLink
	{
		private FileInfo fInfo;

		public string Text { get; set; }
		public string Arguments { get; set; }
		public Image Image { get; set; }

		public string FullName
		{
			get
			{
				if (fInfo != null) {
					return fInfo.FullName;
				}
				return string.Empty;
			}
		}

		public FileLink(string fileName) : this(fileName, string.Empty, null) { }

		public FileLink(string fileName, string arguments) : this(fileName, arguments, null) { }

		public FileLink(string fileName, string arguments, Image image)
		{
			fInfo = new FileInfo(fileName);
			Arguments = arguments;
			Image = image;
		}
	}
}
