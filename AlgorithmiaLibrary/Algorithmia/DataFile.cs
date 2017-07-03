using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Algorithmia
{
    /// <summary>
    /// Represents a data file in the Algorithmia Data API or file that is accessed via a data connector.
    /// </summary>
    public class DataFile
    {
        private readonly Client client;
        private readonly string url;
        private readonly string path;
        private readonly string name;
        private DateTime lastModified;
        private long size;

        private static Regex upToLastSlash = new Regex("^.*/");

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Algorithmia.DataFile"/> class.
        /// This normally should not be called. Instead use the client's <c>file</c> method
        /// or any method in the <c>DataDirectory</c> object that returns one of these.
        /// </summary>
        /// <param name="client">Client that is configured to talk to the correct Algorithmia API endpoint</param>
        /// <param name="dataUrl">The path identifier for the file.</param>
        public DataFile(Client client, string dataUrl)
        {
            this.client = client;
            path = DataUtilities.getDataPath(dataUrl, true);
            url = DataUtilities.getDataUrl(path);
            name = upToLastSlash.Replace(url, "");
            lastModified = System.DateTime.MinValue;
            size = -1;
        }

        internal void setAttributes(long s, DateTime modTime)
        {
            size = s;
            lastModified = modTime;
        }

        /// <summary>
        /// Gets the size of the file in bytes. This value is only valid when it was created from a call to
        /// <c>DataDirectory.files</c>.
        /// </summary>
        /// <returns>The size of the file in bytes or -1 if the file size is unknown.</returns>
        public long getSize()
        {
            return size;
        }

        /// <summary>
        /// Gets the last modified time for the file. This value is only valid when it was created from a call to
        /// <c>DataDirectory.files</c>.
        /// </summary>
        /// <returns>The last modified time or <c>System.DateTime.MinValue</c> if it is unknown.</returns>
        public DateTime getlastModifiedTime()
        {
            return lastModified;
        }

        /// <summary>
        /// Gets the name of the file without the rest of the path (everything after the last slash).
        /// </summary>
        /// <returns>The file name.</returns>
        public string getName()
        {
            return name;
        }

        /// <summary>
        /// Checks if the file exists.
        /// </summary>
        /// <returns>True if the file exists.</returns>
        public bool exists()
        {
            return client.headHelper(url) == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// Deletes this file.
        /// </summary>
        /// <returns>Whether the delete was successful.</returns>
        public bool delete()
        {
            var resAndData = client.deleteHelper(url);
            return Client.checkResult(resAndData, "Delete failed", true);
        }

        /// <summary>
        /// Saves the input with the given encoding to the file. This overwrites the contents of the file.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="input">String to write to the file</param>
        /// <param name="encoding">The encoding of the string</param>
        public DataFile put(string input, System.Text.Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Client.DEFAULT_ENCODING;
            }

            var resAndData = client.putHelper(url, encoding.GetBytes(input));
            Client.checkResult(resAndData, "Updating file failed", true);
            return this;
        }

        /// <summary>
        /// Saves the byte array to the file. This overwrites the contents of the file.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="bytes">Byte array to write to the file</param>
        public DataFile put(byte[] bytes)
        {
            var resAndData = client.putHelper(url, bytes);
            Client.checkResult(resAndData, "Updating file failed", true);
            return this;
        }

        /// <summary>
        /// Reads the contents of the stream and saves it to the file. This overwrites the contents of the file.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="stream">The stream to write to the file.</param>
        public DataFile put(Stream stream)
        {
            var resAndData = client.putHelper(url, stream);
            Client.checkResult(resAndData, "Updating file failed", true);
            return this;
        }

        /// <summary>
        /// Gets the content of the file as a string with the corresponding encoding.
        /// </summary>
        /// <returns>The contents of the file.</returns>
        /// <param name="encoding">The encoding to use when turning the file contents into a string.</param>
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

        /// <summary>
        /// Gets the content of the file as a byte array.
        /// </summary>
        /// <returns>The contents of the file.</returns>
        public byte[] getBytes()
        {
            var resAndData = client.getHelper(url);
            Client.checkResult(resAndData, "Getting file failed", true);
            return resAndData.result;
        }

        /// <summary>
        /// Gets the content of the file as a file stream.
        /// </summary>
        /// <returns>The contents of the file.</returns>
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
