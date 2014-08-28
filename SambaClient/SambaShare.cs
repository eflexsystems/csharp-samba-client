using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace SambaClient
{
	public interface ISambaShare
	{
		Task<string> SendFile(string path, string destination = null);
		Task<string> GetFile(string path, string destination);
	}

	public class SambaShare : ISambaShare
	{
		public string Address { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public SambaShare(string address)
		{
			UserName = "guest";
			this.Address = address;
		}

		public SambaShare(string address, string userName)
		{
			this.Address = address;
			this.UserName = userName;
		}

		public SambaShare(string address, string userName, string password) : this(address, userName)
		{
			this.Password = password;
		}

		// smbclient -U guest "//server1/f" -c "put hello.txt"
		public async Task<string> SendFile(string path, string destination = null)
		{
			var filePath   = Path.GetFileName(path);
			var workingDir = Path.GetDirectoryName(path);
			var cmd        = String.Format("put {0} {1}", filePath, destination);

			return await RunCommand(cmd, workingDir);
		}

		// smbclient -U guest "//server1/f" -c "put hello.txt" 
		public async Task<string> GetFile(string path, string destination)
		{
			var destinationFileName = Path.GetFileName(destination);
			var workingDir          = Path.GetDirectoryName(destination);
			var cmd                 = String.Format("get {0} {1}", path, destinationFileName);

			return await RunCommand(cmd, workingDir);
		}

		private string GetConnectionString()
		{
			var connStr = String.Format("-U {0} ", UserName);

			connStr += String.IsNullOrEmpty(Password) ? @"-N" : Password;

			return connStr;
		}

		private async Task<string> RunCommand(string cmd, string workingDir)
		{
			var arguments = string.Format(@"{0} -c '{1}' '{2}'", GetConnectionString(), cmd, Address);
			arguments     = ProcessHelper.EscapeArguments(arguments);
			var process   = ProcessHelper.Run("smbclient", arguments, workingDir);

			string result = "";
			while (!process.StandardOutput.EndOfStream)
				result += await process.StandardOutput.ReadLineAsync();

			string err = "";
			while (!process.StandardError.EndOfStream)
				err += await process.StandardError.ReadLineAsync();

			var output = (result + err).ToLower();

			// apparently samba is weird and writes successful output to standard error
			if (output.Contains("error") || output.Contains("fail"))
				throw new IOException("Could not run command against samba path");

			return result;
		}

		public static async Task<IList<SambaShare>> ListShares()
		{
			var parser = new SambaTreeParser();

			var process = ProcessHelper.Run("smbtree", "-U guest -N");

			return await parser.Parse(process.StandardOutput.BaseStream);
		}
	}
}

