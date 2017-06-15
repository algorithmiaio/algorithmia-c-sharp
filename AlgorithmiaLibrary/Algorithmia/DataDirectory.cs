﻿using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;

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

		public DataDirectory create(ReadDataAcl acl=null)
		{
			List<String> aclList = (acl == null) ? null : acl.getAclStrings();
			HttpResponseAndData resAndData = client.postJsonHelper(
				DataUtilities.getDataUrl(parent), new CreateDataDirectory(name, aclList), null);
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

		public DataDirectory updatePermissions(ReadDataAcl acl)
		{
			HttpResponseAndData resAndData = client.patchJsonHelper(url, new UpdateDataDirectory(acl.getAclStrings()));
			Client.checkResult(resAndData, "Error setting data directory permissions", true);
			return this;
		}

		public ReadDataAcl getPermissions()
		{
			HttpResponseAndData resAndData = client.getHelper(url, new Dictionary<String, String> { { "acl", "true" } });
			Client.checkResult(resAndData, "Error getting data directory permissions", true);
			DataResponse result = JsonConvert.DeserializeObject<DataResponse>(Client.DEFAULT_ENCODING.GetString(resAndData.result));

			return ReadDataAcl.fromAclStrings(result.acl.read);
		}
	}


	public class ReadAcl
	{
		public List<String> read;
		public ReadAcl()
		{
		}

		public ReadAcl(List<String> r)
		{
			read = r;
		}
	}
	public class CreateDataDirectory
	{
		public String name;
		public ReadAcl acl;

		// This need to be here for serialization
		public CreateDataDirectory()
		{
		}

		public CreateDataDirectory(String n, List<String> r)
		{
			name = n;
			if (r != null)
			{
				acl = new ReadAcl(r);
			}
		}
	}

	public class UpdateDataDirectory
	{
		public ReadAcl acl;
		public UpdateDataDirectory()
		{
		}

		public UpdateDataDirectory(List<String> r)
		{
			acl = new ReadAcl(r);
		}
	}
}
