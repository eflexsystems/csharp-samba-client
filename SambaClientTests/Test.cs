using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Text;
using SambaClient;

namespace SambaClientTests
{
	[TestFixture]
	public class SambaTreeParserTest
	{
		private const string ExampleOutput = 
			"WORKGROUP" +
			"\tWIN-UJAJRHOB2BH\t\t\n" +
			"\t\tWIN-UJAJRHOB2BH\\print$\tPrinter Drivers\n" +
			"\t\tWIN-UJAJRHOB2BH\\MSSQLSERVER\tSQL Server FILESTREAM share\n" +
			"\t\tWIN-UJAJRHOB2BH\\backup\t\n" +
			"\tSERVER1\t\t\n" +
			"\t\tSERVER1\\SQL2012_DEV2\tSQL Server FILESTREAM share\n" +
			"\t\tSERVER1\\F\t\n" +
			"\t\tSERVER1\\ADMIN$\tRemote Admin\n" +
			"\tFACS\t\t\n" +
			"\tEFLEXMONGO\t\teflexmongo server (Samba, Ubuntu)\n" +
			"\tBUILD\t\tbuild server (Samba, Ubuntu)\n" +
			"\t\tBUILD\\print$\tPrinter Drivers\n" +
			"\t\tBUILD\\IPC$\tIPC Service (build server (Samba, Ubuntu))\n" +
			"\teflexplclistener server (Samba, Ubuntu)\n" +
			"MSHOME\n";

		private SambaTreeParser _subject;

		[SetUp]
		public void Setup()
		{
			_subject = new SambaTreeParser();
		}

		[Test]
		public async void ParseOutputTest()
		{
			var stream = new MemoryStream(Encoding.UTF8.GetBytes(ExampleOutput));
			var results = await _subject.Parse(stream);

			Assert.AreEqual(8, results.Count());

			Assert.AreEqual(results[0].Address, @"WIN-UJAJRHOB2BH\print$");
			Assert.AreEqual(results[1].Address, @"WIN-UJAJRHOB2BH\MSSQLSERVER");
			Assert.AreEqual(results[2].Address, @"WIN-UJAJRHOB2BH\backup");

			Assert.AreEqual(results[3].Address, @"SERVER1\SQL2012_DEV2");
			Assert.AreEqual(results[4].Address, @"SERVER1\F");
			Assert.AreEqual(results[5].Address, @"SERVER1\ADMIN$");

			Assert.AreEqual(results[6].Address, @"BUILD\print$");
			Assert.AreEqual(results[7].Address, @"BUILD\IPC$");
		}
	}
}

