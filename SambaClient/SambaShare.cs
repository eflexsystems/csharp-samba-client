using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace SambaClient
{
	public interface ISambaShare
	{
		void SendFile(string path, string destination = null);
		void GetFile(string path, string destination);
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
		public void SendFile(string path, string destination = null)
		{
			var filePath   = Path.GetFileName(path);
			var workingDir = Path.GetDirectoryName(path);
			var cmd        = String.Format("put {0} {1}", filePath, destination);

			RunCommand(cmd, workingDir);
		}

		// smbclient -U guest "//server1/f" -c "put hello.txt" 
		public void GetFile(string path, string destination)
		{
			var destinationFileName = Path.GetFileName(destination);
			var workingDir          = Path.GetDirectoryName(destination);
			var cmd                 = String.Format("get {0} {1}", path, destinationFileName);

			RunCommand(cmd, workingDir);
		}

		// smbclient -U guest "//server1/f" -c "del hello.txt" 
		public void DeleteFile(string path)
		{
			var cmd = String.Format("del {0}", path);
			RunCommand(cmd, "/");
		}

		private string GetConnectionString()
		{
			var connStr = String.Format("-U {0} ", UserName);

			if (String.IsNullOrEmpty(Password))
				connStr += @"-N";

			return connStr;
		}

		private void RunCommand(string cmd, string workingDir)
		{
			var arguments = string.Format(@"{0} -c '{1}' '{2}' '{3}'", GetConnectionString(), cmd, Address, Password);

			arguments     = ProcessHelper.EscapeArguments(arguments);
			var process   = ProcessHelper.Run("smbclient", arguments, workingDir);

			process.WaitForExit();

			if (process.ExitCode != 0)
				throw new IOException("Could not run command against samba path");
		}

		public static async Task<IList<SambaShare>> ListShares()
		{
			var parser = new SambaTreeParser();

			var process = ProcessHelper.Run("smbtree", "-U guest -N");

			return await parser.Parse(process.StandardOutput.BaseStream);
		}
	}
}

