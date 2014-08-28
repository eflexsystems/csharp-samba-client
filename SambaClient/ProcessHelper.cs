using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SambaClient
{
	internal static class ProcessHelper
	{
		public static Process Run(string command, string args = "", string workingDir = null)
		{
			var process = new Process();

			process.StartInfo.FileName = command;
			process.StartInfo.Arguments = args;

			if (!String.IsNullOrEmpty(workingDir))
				process.StartInfo.WorkingDirectory = workingDir;

			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;

			process.Start();
			return process;
		}

		public static string EscapeArguments(string arg)
		{
			var backslashOrQuote = new Regex(@"\\|""");
			return backslashOrQuote.Replace(arg, (match) => @"\" + match);
		}

	}
}

