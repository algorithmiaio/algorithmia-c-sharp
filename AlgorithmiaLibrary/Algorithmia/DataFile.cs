using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Algorithmia
{
    public class DataFile
    {
        private readonly Client client;
        private readonly string url;
        private readonly string path;
        private readonly string name;
        private DateTime lastModified;
        private long size;

        private static Regex upToLastSlash = new Regex("^.*/");

        public DataFile(Client c, string dataUrl)
        {
            client = c;
            path = DataUtilities.getDataPath(dataUrl, true);
            url = DataUtilities.getDataUrl(path);
            name = upToLastSlash.Replace(url, "");
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

        public string getName()
        {
            return name;
        }

        public bool exists()
        {
            return client.headHelper(url) == System.Net.HttpStatusCode.OK;
        }

        public bool delete()
        {
            var resAndData = client.deleteHelper(url);
            return Client.checkResult(resAndData, "Delete failed", true);
        }

        public DataFile put(string s, System.Text.Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Client.DEFAULT_ENCODING;
            }

            var resAndData = client.putHelper(url, encoding.GetBytes(s));
            Client.checkResult(resAndData, "Updating file failed", true);
            return this;
        }

        public DataFile put(byte[] bytes)
        {
            var resAndData = client.putHelper(url, bytes);
            Client.checkResult(resAndData, "Updating file failed", true);
            return this;
        }

        public DataFile put(Stream stream)
        {
            var resAndData = client.putHelper(url, stream);
            Client.checkResult(resAndData, "Updating file failed", true);
            return this;
        }

        public string getString(System.Text.Encoding encoding = null)
        {
            var resAndData = client.getHelper(url);
            Client.checkResult(resAndData, "Getting file failed", true);

            if (encoding == null)
            {
                encoding = Client.DEFAULT_ENCODING;
            }
            return encoding.GetString(resAndData.result);
        }

        public byte[] getBytes()
        {
            var resAndData = client.getHelper(url);
            Client.checkResult(resAndData, "Getting file failed", true);
            return resAndData.result;
        }

        public FileStream getFile()
        {
            var resAndData = client.getHelper(url);
            Client.checkResult(resAndData, "Getting file failed", true);

            // Write to temporary location
            var fileName = Path.GetTempFileName();
            var forWrite = File.OpenWrite(fileName);
            forWrite.Write(resAndData.result, 0, resAndData.result.Length);
            forWrite.Close();

            return File.OpenRead(fileName);
        }
    }
}
