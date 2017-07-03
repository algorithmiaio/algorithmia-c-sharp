using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Algorithmia
{
    /// <summary>
    /// Represents a data directory in the Algorithmia Data API or directory that is accessed via a data connector.
    /// </summary>
    public class DataDirectory
    {
        private readonly Client client;
        private readonly string url;
        private readonly string path;
        private readonly string name;
        private readonly string parent;

        private static Regex everythingToLastSlashReplacementRegex = new Regex("^(.*)/");

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Algorithmia.DataDirectory"/> class.
        /// This normally should not be called. Instead use the client's <c>dir</c> method
        /// or any method in this class that returns one of these.
        /// </summary>
        /// <param name="client">Client that is configured to talk to the correct Algorithmia API endpoint.</param>
        /// <param name="dataUrl">The path identifier for the directory.</param>
        public DataDirectory(Client client, string dataUrl)
        {
            this.client = client;
            path = DataUtilities.getDataPath(dataUrl, false);
            url = DataUtilities.getDataUrl(path);
            name = everythingToLastSlashReplacementRegex.Replace(path, "");
            parent = everythingToLastSlashReplacementRegex.Match(path).Groups[1].Value;
        }

        /// <summary>
        /// Gets the name of the directory without the rest of the path.
        /// </summary>
        /// <returns>The name of the directory.</returns>
        public string getName()
        {
            return name;
        }

        /// <summary>
        /// Checks if the directory exists.
        /// </summary>
        /// <returns>True if the directory exists.</returns>
        public bool exists()
        {
            return client.headHelper(url) == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// Create the directory with the correct access modifiers.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="acl">The access level for the directory.</param>
        public DataDirectory create(ReadDataAcl acl = null)
        {
            var aclList = acl?.getAclStrings();
            var resAndData = client.postJsonHelper(
                DataUtilities.getDataUrl(parent), new CreateDataDirectory(name, aclList), null);
            Client.checkResult(resAndData, "Error creating data directory", true);
            return this;
        }

        /// <summary>
        /// Delete this directory. If the <c>force</c> parameter is false, and it is not empty, an exception will be thrown.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="force">Force the delete even if the directory is not empty.</param>
        public DataDirectory delete(bool force = false)
        {
            var queryParams = force ? new Dictionary<string, string> { { "force", "true" } } : null;
            var resAndData = client.deleteHelper(url, queryParams);
            Client.checkResult(resAndData, "Error deleting data directory", true);
            return this;
        }

        private string makeChildUrl(string child)
        {
            return path + "/" + child;
        }

        /// <summary>
        /// Creates the child file object for the given file in this directory.
        /// </summary>
        /// <returns>The child file.</returns>
        /// <param name="child">Name of the child file.</param>
        public DataFile file(string child)
        {
            return new DataFile(client, makeChildUrl(child));
        }

        /// <summary>
        /// Creates the child directory object for the given directory in this directory.
        /// </summary>
        /// <returns>The child directory.</returns>
        /// <param name="child">Name of the child directory.</param>
        public DataDirectory dir(string child)
        {
            return new DataDirectory(client, makeChildUrl(child));
        }

        /// <summary>
        /// Updates the access levels for this directory.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="acl">The new access level for the directory.</param>
        public DataDirectory updatePermissions(ReadDataAcl acl)
        {
            var resAndData = client.patchJsonHelper(url, new UpdateDataDirectory(acl.getAclStrings()));
            Client.checkResult(resAndData, "Error setting data directory permissions", true);
            return this;
        }

        /// <summary>
        /// Gets the access levels for this directory.
        /// </summary>
        /// <returns>The access control levels for this directory.</returns>
        public ReadDataAcl getPermissions()
        {
            var resAndData = client.getHelper(url, new Dictionary<string, string> { { "acl", "true" } });
            Client.checkResult(resAndData, "Error getting data directory permissions", true);
            var result = JsonConvert.DeserializeObject<DataResponse>(Client.DEFAULT_ENCODING.GetString(resAndData.result));

            return ReadDataAcl.fromAclStrings(result.acl.read);
        }

        /// <summary>
        /// Gets an iterator of <c>DataFile</c>s for all the child files in this directory.
        /// </summary>
        /// <returns>The child files.</returns>
        public DataIterator files()
        {
            return new DataIterator(this, true);
        }

        /// <summary>
        /// Gets an iterator of <c>DataDirectory</c>s for all the child directories in this directory.
        /// </summary>
        /// <returns>The child directories.</returns>
        public DataIterator dirs()
        {
            return new DataIterator(this, false);
        }

        /// <summary>
        /// An iterator of either all <c>DataFile</c> or all <c>DataDirectory</c> objects.
        /// </summary>
        public class DataIterator : System.Collections.IEnumerable
        {
            private DataDirectory parent;
            private string marker;
            private bool isFiles;

            internal DataIterator(DataDirectory p, bool i)
            {
                parent = p;
                marker = null;
                isFiles = i;
            }

            /// <summary>
            /// Gets the enumerator of either files or directories.
            /// </summary>
            /// <returns>The enumerator.</returns>
            public System.Collections.IEnumerator GetEnumerator()
            {
                do
                {
                    var markerQueryParams = (marker == null) ? null : new Dictionary<string, string> { { "marker", marker } };
                    var resAndData = parent.client.getHelper(parent.url, markerQueryParams);
                    Client.checkResult(resAndData, "Error getting data directory's children", true);
                    var result = JsonConvert.DeserializeObject<DataDirectoryContents>(Client.DEFAULT_ENCODING.GetString(resAndData.result));

                    marker = result.marker;
                    if (isFiles)
                    {
                        foreach (var e in result.files)
                        {
                            var df = parent.file(e.filename);
                            df.setAttributes(e.size, DateTime.ParseExact(e.last_modified, "yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.InvariantCulture));
                            yield return df;
                        }
                    }
                    else
                    {
                        foreach (var e in result.folders)
                        {
                            yield return parent.dir(e.name);
                        }
                    }
                } while (marker != null);
            }
        }


    }

    [DataContract()]
    class ReadAcl
    {
        [DataMember()]
        public List<string> read;

        public ReadAcl()
        {
        }

        public ReadAcl(List<string> r)
        {
            read = r;
        }
    }

    [DataContract()]
    class CreateDataDirectory
    {
        [DataMember()]
        public string name;

        [DataMember()]
        public ReadAcl acl;

        // This need to be here for serialization
        public CreateDataDirectory()
        {
        }

        public CreateDataDirectory(string n, List<string> r)
        {
            name = n;
            if (r != null)
            {
                acl = new ReadAcl(r);
            }
        }
    }

    [DataContract()]
    class UpdateDataDirectory
    {
        [DataMember()]
        public ReadAcl acl;

        public UpdateDataDirectory()
        {
        }

        public UpdateDataDirectory(List<string> r)
        {
            acl = new ReadAcl(r);
        }
    }

    [DataContract()]
    class DataDirectoryFileElement
    {
        [DataMember()]
        public string filename;

        [DataMember()]
        public string last_modified;

        [DataMember()]
        public long size;

        public DataDirectoryFileElement()
        {
            filename = null;
            last_modified = null;
            size = -1;
        }
    }

    [DataContract()]
    class DataDirectoryDirectoryElement
    {
        [DataMember()]
        public string name;

        public DataDirectoryDirectoryElement()
        {
            name = null;
        }
    }

    [DataContract()]
    class DataDirectoryContents
    {
        [DataMember()]
        public List<DataDirectoryFileElement> files;

        [DataMember()]
        public List<DataDirectoryDirectoryElement> folders;

        [DataMember()]
        public string marker;

        public DataDirectoryContents()
        {
            files = null;
            folders = null;
            marker = null;
        }
    }
}