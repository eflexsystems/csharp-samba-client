using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SambaClient
{
	public class SambaTreeParser
	{
		public async Task<IList<SambaShare>> Parse(Stream stream)
		{
			List<SambaShare> shares = new List<SambaShare>();

			using (var reader = new StreamReader(stream))
			{
				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();
					var row = line.Split('\t');

					if (row.Length > 2 && !String.IsNullOrWhiteSpace(row[2]))
					{
						var address = row[2].Trim();
						shares.Add(new SambaShare(address));
					}
				}
			}

			return shares;
		}
	}
}

