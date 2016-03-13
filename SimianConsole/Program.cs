#region license
// Copyright (c) 2016, Wm. Barrett Simms
// 
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimianOutputCleaner
{
	class Program
	{
		static int Main(string[] args)
		{
			string filename = "simian.xml";
			if (args.Length > 0)
			{
				filename = args[0];
			}
			if (!File.Exists(filename))
			{
				Usage();
				Console.WriteLine("File: "+ filename+" does not exist");
				return -1;
			}

			bool showResults = args.Length == 2 && !string.IsNullOrEmpty(args[1]) && args[1] == "yes";

			var lines = File.ReadAllLines(filename);
			var q = new Queue<string>(lines);
			if (!q.First().StartsWith("<?"))
			{
				q.Dequeue();
				q.Dequeue();
				q.Dequeue();
			}
			File.Delete(filename);
			File.WriteAllLines(filename,q);

			if (showResults)
			{
				XmlDocument xdoc = new XmlDocument();
				xdoc.Load(filename);
				var iterator = xdoc.CreateNavigator().Select("//simian/check/summary");
				if (iterator.Count == 0)
				{
					throw new ApplicationException("Unable to find summary in file");
				}

				var summary = iterator.MoveNext();
				var duplicateFileCount = iterator.Current.GetAttribute("duplicateFileCount","");
				var duplicateLineCount = iterator.Current.GetAttribute("duplicateLineCount", "");
				var duplicateBlockCount = iterator.Current.GetAttribute("duplicateBlockCount", "");
				var totalFileCount = iterator.Current.GetAttribute("totalFileCount", "");
				var totalRawLineCount = iterator.Current.GetAttribute("totalRawLineCount", "");
				var totalSignificantLineCount = iterator.Current.GetAttribute("totalSignificantLineCount", "");
				var processingTime = iterator.Current.GetAttribute("processingTime", "");

				StringBuilder output = new StringBuilder("======== Simian Results ====================\r\n");
				output.AppendLine("DuplicateFileCount : " + duplicateFileCount);
				output.AppendLine("DuplicateLineCount : " + duplicateLineCount);
				output.AppendLine("DuplicateBlockCount : " + duplicateBlockCount);
				output.AppendLine("TotalFileCount : " + totalFileCount);
				output.AppendLine("TotalRawLineCount : " + totalRawLineCount);
				output.AppendLine("TotalSignificantLineCount : " + totalSignificantLineCount);
				output.AppendLine("ProcessingTime : " + processingTime);
				double dup = (Convert.ToDouble(totalFileCount)/Convert.ToDouble(totalSignificantLineCount));
				var percentDup = dup.ToString("P"); 
				output.AppendLine("% Duplication : " + percentDup);
				output.AppendLine("==========================================");

			}

			return 0;
		}

		static void Usage()
		{
			Console.WriteLine("Usage: SimianConsole.exe [filename] [yes]");
			Console.WriteLine("Example: SimianConsole.exe simian.xml // clean only");
			Console.WriteLine("Example: SimianConsole.exe simian.xml yes // clean and display results");
		}
	}
}
