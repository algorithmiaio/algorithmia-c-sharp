using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Algorithmia
{
    public class DataDirectory
    {
        public readonly Client client;
        private readonly string url;
        private readonly string path;
        private readonly string name;
        private readonly string parent;

        public DataDirectory(Client c, string dataUrl)
        {
            client = c;
            path = DataUtilities.getDataPath(dataUrl, false);
            url = DataUtilities.getDataUrl(path);
            name = everythingToLastSlashReplacementRegex.Replace(path, "");
            parent = everythingToLastSlashReplacementRegex.Match(path).Groups[1].Value;
        }

        private static Regex everythingToLastSlashReplacementRegex = new Regex("^(.*)/");
        public string getName()
        {
            return name;
        }

        public bool exists()
        {
            return client.headHelper(url) == System.Net.HttpStatusCode.OK;
        }

        public DataDirectory create(ReadDataAcl acl = null)
        {
            var aclList = acl?.getAclStrings();
            var resAndData = client.postJsonHelper(
                DataUtilities.getDataUrl(parent), new CreateDataDirectory(name, aclList), null);
            Client.checkResult(resAndData, "Error creating data directory", true);
            return this;
        }

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

        public DataFile file(string child)
        {
            return new DataFile(client, makeChildUrl(child));
        }

        public DataDirectory dir(string child)
        {
            return new DataDirectory(client, makeChildUrl(child));
        }

        public DataDirectory updatePermissions(ReadDataAcl acl)
        {
            var resAndData = client.patchJsonHelper(url, new UpdateDataDirectory(acl.getAclStrings()));
            Client.checkResult(resAndData, "Error setting data directory permissions", true);
            return this;
        }

        public ReadDataAcl getPermissions()
        {
            var resAndData = client.getHelper(url, new Dictionary<string, string> { { "acl", "true" } });
            Client.checkResult(resAndData, "Error getting data directory permissions", true);
            var result = JsonConvert.DeserializeObject<DataResponse>(Client.DEFAULT_ENCODING.GetString(resAndData.result));

            return ReadDataAcl.fromAclStrings(result.acl.read);
        }

        public DataIterator files()
        {
            return new DataIterator(this, true);
        }

        public DataIterator dirs()
        {
            return new DataIterator(this, false);
        }

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