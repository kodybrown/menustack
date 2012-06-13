using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AppMenuStack
{
	public class Stack
	{
		public string fileName { get; set; }
		public string type { get; set; }
		public string folder { get; set; }
		public bool subfolders { get; set; }
		public bool combine { get; set; }
		public string sortKey { get; set; }

		public bool Exists { get { return fileName.Length > 0 && File.Exists(fileName); } }
		public bool FolderExists { get { return folder.Length > 0 && Directory.Exists(folder); } }

		public Stack()
		{
			fileName = string.Empty;
			type = string.Empty;
			folder = string.Empty;
			subfolders = false;
			combine = false;
			sortKey = string.Empty;
		}

		

		/// <summary>
		/// Creates and returns a Stack object from a settings file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static Stack Load(string fileName)
		{
			if (!File.Exists(fileName)) {
				throw new Exception("The path specified does not exist: " + fileName);
			}

			Stack stack;

			stack = new Stack();
			stack.fileName = fileName;

			using (StreamReader reader = File.OpenText(fileName)) {
				string line;

				while (null != (line = reader.ReadLine().Trim())) {
					if (line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("//")) {
						continue;
					}

					if (line.StartsWith("type=", StringComparison.InvariantCultureIgnoreCase)) {
						stack.type = line.Substring("type=".Length).Trim();
					} else if (line.StartsWith("folder=", StringComparison.InvariantCultureIgnoreCase)) {
						stack.folder = line.Substring("folder=".Length).Trim();
					} else if (line.StartsWith("subfolders=", StringComparison.InvariantCultureIgnoreCase)) {
						stack.subfolders = line.Substring("subfolders=".Length).Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase) ? true : false;
					} else if (line.StartsWith("combine=", StringComparison.InvariantCultureIgnoreCase)) {
						stack.combine = line.Substring("combine=".Length).Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase) ? true : false;
					} else if (line.StartsWith("sortKey=", StringComparison.InvariantCultureIgnoreCase)) {
						stack.sortKey = line.Substring("sortKey=".Length).Trim();
					}
				}

				reader.Close();
				reader.Dispose();
			}


			return stack;
		}
	}
}
