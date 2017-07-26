using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

namespace Algorithmia
{
    internal class HttpResponseAndData
    {
        public readonly HttpStatusCode status;
        public readonly byte[] result;
        public HttpResponseAndData(HttpStatusCode s, byte[] r)
        {
            status = s;
            result = r;
        }
    }

    /// <summary>
    /// The Algorithmia Client which is used to create Algorithm, DataFile, and DataDirectory objects.
    /// This class contains a majority of the REST calls we use to interact with the Algorithmia platform.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// The default encoding.
        /// </summary>
        public static readonly System.Text.Encoding DEFAULT_ENCODING = System.Text.Encoding.UTF8;

        private readonly string apiKey;

        /// <summary>
        /// The API address. Defaults to "https://api.algorithmia.com"
        /// </summary>
        public readonly string apiAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Algorithmia.Client"/> class.
        /// </summary>
        /// <param name="key">The API key for the user calling the algorithm.</param>
        /// <param name="address">Optional API address. When it's not specified, we use the public marketplace endpoint.</param>
        public Client(string key, string address = null)
        {
            apiKey = key;
            apiAddress = getApiAddress(address);
        }

        /// <summary>
        /// Creates the Algorithm object given the reference to an algorithm.
        /// </summary>
        /// <returns>The Algorithm object.</returns>
        /// <param name="algoRef">Algorithm reference which has the format: [author name]/[algorithm name]/[optional version]</param>
        public Algorithm algo(string algoRef)
        {
            return new Algorithm(this, algoRef);
        }

        /// <summary>
        /// Creates a DataFile object given the path to the file.
        /// </summary>
        /// <returns>The DataFile object.</returns>
        /// <param name="dataUrl">Path to the data file.</param>
        public DataFile file(string dataUrl)
        {
            return new DataFile(this, dataUrl);
        }

        /// <summary>
        /// Creates a DataDirectory object given the path to the directory.
        /// </summary>
        /// <returns>The DataDirectory object.</returns>
        /// <param name="path">Path for the directory.</param>
        public DataDirectory dir(string path)
        {
            return new DataDirectory(this, path);
        }

        private string getApiAddress(string address)
        {
            if (address != null)
            {
                return address;
            }

            var envApiAddress = Environment.GetEnvironmentVariable("ALGORITHMIA_API");
            return envApiAddress ?? "https://api.algorithmia.com";
        }

        private HttpResponseAndData synchronousHttpCall(HttpMethod method, string url, Dictionary<string, string> queryParameters,
                                                        HttpContent content, string contentType)
        {
            var client = new HttpClient { BaseAddress = new Uri(apiAddress) };

            if (queryParameters != null && queryParameters.Count > 0)
            {
                Boolean first = true;
                foreach (var entry in queryParameters)
                {
                    String symbol = "&";
                    if (first)
                    {
                        symbol = "?";
                        first = false;
                    }

                    url += symbol + entry.Key + "=" + WebUtility.UrlEncode(entry.Value);
                }
            }
            var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);
            }

            if (content != null)
            {
                request.Content = content;
                if (contentType != null)
                {
                    request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                }
            }
            var x = client.SendAsync(request);
            x.Wait();
            var result = x.Result;
            var bytes = result.Content.ReadAsByteArrayAsync();
            bytes.Wait();

            return new HttpResponseAndData(result.StatusCode, bytes.Result);
        }

        internal HttpStatusCode headHelper(string url)
        {
            return synchronousHttpCall(HttpMethod.Head, url, null, null, null).status;
        }

        internal HttpResponseAndData getHelper(string url, Dictionary<string, string> queryParameters = null)
        {
            return synchronousHttpCall(HttpMethod.Get, url, queryParameters, null, null);
        }

        internal HttpResponseAndData deleteHelper(string url, Dictionary<string, string> queryParameters = null)
        {
            return synchronousHttpCall(HttpMethod.Delete, url, queryParameters, null, null);
        }

        internal HttpResponseAndData patchJsonHelper(string url, object inputObject)
        {
            using (var stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(JsonConvert.SerializeObject(inputObject));
                writer.Flush();
                stream.Flush();
                stream.Position = 0;
                return synchronousHttpCall(new HttpMethod("PATCH"), url, null, new StreamContent(stream), "application/json");
            }
        }

        internal HttpResponseAndData putHelper(string url, byte[] data)
        {
            return synchronousHttpCall(HttpMethod.Put, url, null, new ByteArrayContent(data), null);
        }

        internal HttpResponseAndData putHelper(string url, Stream stream)
        {
            return synchronousHttpCall(HttpMethod.Put, url, null, new StreamContent(stream), null);
        }

        internal HttpResponseAndData postJsonHelper(string url, object inputObject, Dictionary<string, string> queryParameters)
        {
            var isByteArray = inputObject != null && inputObject.GetType().Name.ToLower() == "byte[]";

            HttpResponseAndData result = null;
            if (isByteArray)
            {
                result = synchronousHttpCall(HttpMethod.Post, url, queryParameters, new ByteArrayContent((byte[])inputObject), "application/octet-stream");
            }
            else
            {
                using (var stream = new MemoryStream())
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(JsonConvert.SerializeObject(inputObject));
                    writer.Flush();
                    stream.Flush();
                    stream.Position = 0;
                    result = synchronousHttpCall(HttpMethod.Post, url, queryParameters, new StreamContent(stream), "application/json");
                }
            }
            return result;
        }

        internal static bool checkResult(HttpResponseAndData resAndData, string errorMessage, bool isData)
        {
            if (resAndData.status == HttpStatusCode.OK)
            {
                return true;
            }
            try
            {
                var dr = JsonConvert.DeserializeObject<DataResponse>(DEFAULT_ENCODING.GetString(resAndData.result));
                if (dr.error.ContainsKey("message"))
                {
                    var exceptionMessage = errorMessage + " - reason: " + dr.error["message"];
                    if (isData)
                    {
                        throw new DataApiException(exceptionMessage);
                    }
                    else
                    {
                        throw new AlgorithmException(exceptionMessage);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is AlgorithmiaException)
                {
                    throw e;
                }
            }
            throw new AlgorithmiaException(errorMessage);
        }
    }
}
