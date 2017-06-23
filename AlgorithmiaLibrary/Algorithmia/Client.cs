using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Net.Http;

namespace Algorithmia
{
    public class HttpResponseAndData
    {
        public readonly HttpStatusCode status;
        public readonly byte[] result;
        public HttpResponseAndData(HttpStatusCode s, byte[] r)
        {
            status = s;
            result = r;
        }
    }

    public class Client
    {
        public static readonly System.Text.Encoding DEFAULT_ENCODING = System.Text.Encoding.UTF8;

        private readonly string apiKey;
        public readonly string apiAddress;

        public Client(string key, string address = null)
        {
            apiKey = key;
            apiAddress = getApiAddress(address);
        }

        public Algorithm algo(string algoRef)
        {
            return new Algorithm(this, algoRef);
        }

        public DataFile file(string dataUrl)
        {
            return new DataFile(this, dataUrl);
        }

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
                var query = HttpUtility.ParseQueryString("");
                foreach (var entry in queryParameters)
                {
                    query[entry.Key] = entry.Value;
                }
                url += "?" + query;
            }
            var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("Authorization", apiKey);
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

        public HttpStatusCode headHelper(string url)
        {
            return synchronousHttpCall(HttpMethod.Head, url, null, null, null).status;
        }

        public HttpResponseAndData getHelper(string url, Dictionary<string, string> queryParameters = null)
        {
            return synchronousHttpCall(HttpMethod.Get, url, queryParameters, null, null);
        }

        public HttpResponseAndData deleteHelper(string url, Dictionary<string, string> queryParameters = null)
        {
            return synchronousHttpCall(HttpMethod.Delete, url, queryParameters, null, null);
        }

        public HttpResponseAndData patchJsonHelper(string url, object inputObject)
        {
            using (var stream = new MemoryStream())
            {
                var ser = (inputObject == null) ?
                    new DataContractJsonSerializer(typeof(object)) : new DataContractJsonSerializer(inputObject.GetType());
                ser.WriteObject(stream, inputObject);
                stream.Flush();
                stream.Position = 0;
                return synchronousHttpCall(new HttpMethod("PATCH"), url, null, new StreamContent(stream), "application/json");
            }
        }

        public HttpResponseAndData putHelper(string url, byte[] data)
        {
            return synchronousHttpCall(HttpMethod.Put, url, null, new ByteArrayContent(data), null);
        }

        public HttpResponseAndData putHelper(string url, Stream stream)
        {
            return synchronousHttpCall(HttpMethod.Put, url, null, new StreamContent(stream), null);
        }

        public HttpResponseAndData postJsonHelper(string url, object inputObject, Dictionary<string, string> queryParameters)
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
                    var ser = (inputObject == null) ?
                        new DataContractJsonSerializer(typeof(object)) : new DataContractJsonSerializer(inputObject.GetType());
                    ser.WriteObject(stream, inputObject);
                    stream.Flush();
                    stream.Position = 0;
                    result = synchronousHttpCall(HttpMethod.Post, url, queryParameters, new StreamContent(stream), "application/json");
                }
            }
            return result;
        }

        public static bool checkResult(HttpResponseAndData resAndData, string errorMessage, bool isData)
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
