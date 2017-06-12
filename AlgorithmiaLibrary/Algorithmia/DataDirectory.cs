using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Algorithmia
{
	public class DataDirectory
	{
		public readonly Client client;
		private readonly String url;
		private readonly String path;
		private readonly String name;
		private readonly String parent;

		public DataDirectory(Client c, String dataUrl)
		{
			client = c;
			path = DataUtilities.getDataPath(dataUrl, false);
			url = DataUtilities.getDataUrl(path);
			name = everythingToLastSlashReplacementRegex.Replace(this.path, "");
			parent = everythingToLastSlashReplacementRegex.Match(this.path).Groups[1].Value;
		}


		private static Regex everythingToLastSlashReplacementRegex = new Regex("^(.*)/");
		public String getName()
		{
			return name;
		}

		public Boolean exists()
		{
			return client.headHelper(url) == System.Net.HttpStatusCode.OK;
		}

		public DataDirectory create()
		{
			HttpResponseAndData resAndData = client.postJsonHelper(
				DataUtilities.getDataUrl(parent), new CreateDataDirectory(name), null);
			Client.checkResult(resAndData, "Error creating data directory", true);
			return this;
		}

		public DataDirectory delete(Boolean force = false)
		{
			Dictionary<String, String> queryParams = force ? new Dictionary<String, String> { { "force", "true" } } : null;
			HttpResponseAndData resAndData = client.deleteHelper(url, queryParams);
			Client.checkResult(resAndData, "Error deleting data directory", true);
			return this;
		}

		public DataFile file(String child)
		{
			return new DataFile(client, path + "/" + child);
		}

		public DataDirectory dir(String child)
		{
			return new DataDirectory(client, path + "/" + child);
		}
	}

	public class CreateDataDirectory
	{
		public String name;

		// This need to be here for serialization
		public CreateDataDirectory()
		{
		}

		public CreateDataDirectory(String n)
		{
			name = n;
		}
	}
}
