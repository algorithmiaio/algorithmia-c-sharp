using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Algorithmia
{
	public class HttpResponseAndData
	{
		public readonly HttpStatusCode status;
		public readonly Byte[] result;
		public HttpResponseAndData(HttpStatusCode s, Byte[] r)
		{
			status = s;
			result = r;
		}
	}
	public class Client
	{
		public static readonly System.Text.Encoding DEFAULT_ENCODING = System.Text.Encoding.UTF8;
		
		private readonly String apiKey;
		public readonly String apiAddress;

		public Client(String key, String address=null)
		{
			this.apiKey = key;
			this.apiAddress = getApiAddress(address);
		}

		public Algorithm algo(String algoRef)
		{
			return new Algorithm(this, algoRef);
		}

		public DataFile file(String dataUrl)
		{
			return new DataFile(this, dataUrl);
		}

		public DataDirectory dir(String path)
		{
			return new DataDirectory(this, path);
		}

		private String getApiAddress(String address)
		{
			if (address != null)
			{
				return address;
			}

			String envApiAddress = System.Environment.GetEnvironmentVariable("ALGORITHMIA_API");
			if (envApiAddress != null)
			{
				return envApiAddress;
			}
			return "https://api.algorithmia.com";
		}

		private HttpResponseAndData synchronousHttpCall(HttpMethod method, String url, Dictionary<String, String> queryParameters, 
		                                                HttpContent content, String contentType)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(this.apiAddress);

			if (queryParameters != null && queryParameters.Count > 0)
			{
				var query = HttpUtility.ParseQueryString("");
				foreach (KeyValuePair<string, string> entry in queryParameters)
				{
					query[entry.Key] = entry.Value;
				}
				url += "?" + query.ToString();
			}
			HttpRequestMessage request = new HttpRequestMessage(method, url);

			if (this.apiKey != null && this.apiKey.Length > 0)
			{
				request.Headers.Add("Authorization", this.apiKey);
			}

			if (content != null)
			{
				request.Content = content;
				if (contentType != null)
				{
					request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
				}
			}
			Task<HttpResponseMessage> x = client.SendAsync(request);
			x.Wait();
			HttpResponseMessage result = x.Result;
			//Console.Write(result);
			Task<byte[]> bytes = result.Content.ReadAsByteArrayAsync();
			bytes.Wait();

			return new HttpResponseAndData(result.StatusCode, bytes.Result);
		}

		public HttpStatusCode headHelper(String url)
		{
			return synchronousHttpCall(HttpMethod.Head, url, null, null, null).status;
		}

		public HttpResponseAndData getHelper(String url, Dictionary<String, String> queryParameters=null)
		{
			return synchronousHttpCall(HttpMethod.Get, url, queryParameters, null, null);
		}

		public HttpResponseAndData deleteHelper(String url, Dictionary<String, String> queryParameters=null)
		{
			return synchronousHttpCall(HttpMethod.Delete, url, queryParameters, null, null);
		}

		public HttpResponseAndData patchJsonHelper(String url, Object inputObject)
		{
			using (var stream = new MemoryStream())
			{
				DataContractJsonSerializer ser = (inputObject == null) ?
					new DataContractJsonSerializer(typeof(Object)) : new DataContractJsonSerializer(inputObject.GetType());
				ser.WriteObject(stream, inputObject);
				stream.Flush();
				stream.Position = 0;
				return synchronousHttpCall(new HttpMethod("PATCH"), url, null, new StreamContent(stream), "application/json");
			}
		}

		public HttpResponseAndData putHelper(String url, Byte[] data)
		{
			return synchronousHttpCall(HttpMethod.Put, url, null, new ByteArrayContent(data), null);
		}

		public HttpResponseAndData putHelper(String url, Stream stream)
		{
			return synchronousHttpCall(HttpMethod.Put, url, null, new StreamContent(stream), null);
		}

		public HttpResponseAndData postJsonHelper(String url, Object inputObject, Dictionary<String, String> queryParameters)
		{
			Boolean isByteArray = inputObject != null && inputObject.GetType().Name.ToLower() == "byte[]";

			HttpResponseAndData result = null;
			if (isByteArray)
			{
				result = synchronousHttpCall(HttpMethod.Post, url, queryParameters, new ByteArrayContent((Byte[])inputObject), "application/octet-stream");
			}
			else
			{
				using (var stream = new MemoryStream())
				{
					DataContractJsonSerializer ser = (inputObject == null) ?
						new DataContractJsonSerializer(typeof(Object)) : new DataContractJsonSerializer(inputObject.GetType());
					ser.WriteObject(stream, inputObject);
					stream.Flush();
					stream.Position = 0;
					result = synchronousHttpCall(HttpMethod.Post, url, queryParameters, new StreamContent(stream), "application/json");
				}
			}
			return result;
		}

		public static Boolean checkResult(HttpResponseAndData resAndData, String errorMessage, Boolean isData)
		{
			if (resAndData.status == System.Net.HttpStatusCode.OK)
			{
				return true;
			}
			try
			{
				DataResponse dr = JsonConvert.DeserializeObject<DataResponse>(DEFAULT_ENCODING.GetString(resAndData.result));
				if (dr.error.ContainsKey("message"))
				{
					String exceptionMessage = errorMessage + " - reason: " + dr.error["message"];
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
