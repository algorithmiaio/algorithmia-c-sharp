using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Json;

namespace Algorithmia
{
	public class DataFile
	{
		public readonly Client client;
		private readonly String url;
		private readonly String path;
		private DateTime lastModified;
		private long size;

		private static Regex upToLastSlash = new Regex("^.*/");

		public DataFile(Client c, String dataUrl)
		{
			client = c;
			path = DataUtilities.getDataPath(dataUrl, true);
			url = DataUtilities.getDataUrl(path);

			lastModified = new DateTime(0);
			size = -1;
		}

		public void setAttributes(long s, DateTime modTime)
		{
			size = s;
			lastModified = modTime;
		}

		public long getSize()
		{
			return size;
		}

		public DateTime getlastModifiedTime()
		{
			return lastModified;
		}

		public String getName()
		{
			// TODO: make this a variable instead
			return upToLastSlash.Replace(url, "");
		}

		public Boolean exists()
		{
			return client.headHelper(url) == System.Net.HttpStatusCode.OK;
		}

		public Boolean delete()
		{
			HttpResponseAndData resAndData = client.deleteHelper(url);
			return Client.checkResult(resAndData, "Delete failed", true);
		}

		public DataFile put(String s, System.Text.Encoding encoding=null)
		{
			if (encoding == null)
			{
				encoding = Client.DEFAULT_ENCODING;
			}

			HttpResponseAndData resAndData = client.putHelper(url, encoding.GetBytes(s));
			Client.checkResult(resAndData, "Updating file failed", true);
			return this;
		}

		public DataFile put(Byte[] bytes)
		{
			HttpResponseAndData resAndData = client.putHelper(url, bytes);
			Client.checkResult(resAndData, "Updating file failed", true);
			return this;
		}

		public DataFile put(Stream stream)
		{
			HttpResponseAndData resAndData = client.putHelper(url, stream);
			Client.checkResult(resAndData, "Updating file failed", true);
			return this;
		}

		public String getString(System.Text.Encoding encoding=null)
		{
			HttpResponseAndData resAndData = client.getHelper(url);
			Client.checkResult(resAndData, "Getting file failed", true);

			if (encoding == null)
			{
				encoding = Client.DEFAULT_ENCODING;
			}
			return encoding.GetString(resAndData.result);
		}

		public Byte[] getBytes()
		{
			HttpResponseAndData resAndData = client.getHelper(url);
			Client.checkResult(resAndData, "Getting file failed", true);
			return resAndData.result;
		}

		public FileStream getFile()
		{
			HttpResponseAndData resAndData = client.getHelper(url);
			Client.checkResult(resAndData, "Getting file failed", true);

			// Write to temporary location
			String fileName = Path.GetTempFileName();
			FileStream forWrite = File.OpenWrite(fileName);
			forWrite.Write(resAndData.result, 0, resAndData.result.Length);
			forWrite.Close();

			return File.OpenRead(fileName);
		}

		private void setAttributes()
		{
			// TODO: fill this in
		}
	}
}
