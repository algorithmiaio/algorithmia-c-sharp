using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Algorithmia
{
	public class Algorithm
	{
		private readonly Client client;
		private readonly String algoUrl;
		private static Regex algoPrefixReplacementRegex = new Regex("^(algo://|/)");
		private static Regex algoReferenceRegex = new Regex("^(\\w+/\\w+)$");

		private Dictionary<String, String> queryParameters;

		public Algorithm(Client client, String algoRef)
		{
			this.client = client;
			this.algoUrl = getAlgorithmUrl(algoRef);

			queryParameters = new Dictionary<String, String>();
			queryParameters["timeout"] = 300.ToString();
			queryParameters["stdout"] = false.ToString();
			queryParameters["output"] = AlgorithmOutputType.DEFAULT.getOutputType();
		}

		public Algorithm setOptions(int timeout=300, bool stdout=false, OutputType output=null, Dictionary<String, String> options=null)
		{
			if (output == null)
			{
				output = AlgorithmOutputType.DEFAULT;
			}

			Dictionary<String, String> copy = (options == null) ? new Dictionary<String, String>() : new Dictionary<String, String>(options);

			copy["timeout"] = timeout.ToString();
			copy["stdout"] = stdout.ToString();
			copy["output"] = output.getOutputType();

			queryParameters = copy;

			return this;
		}

		private String getAlgorithmUrl(String algoRef)
		{
			if (algoRef == null || algoRef.Length == 0)
			{
				throw new ArgumentException("Invalid algorithm URI");
			}

			// Get rid of the starting slash or "algo://"
			String path = algoPrefixReplacementRegex.Replace(algoRef, "");

			if (!algoReferenceRegex.Match(path).Success)
			{
				throw new ArgumentException("Invalid algorithm URI: " + algoRef);
			}

			return "/v1/algo/" + path;
		}

		public AlgorithmResponse pipe<T>(Object input)
		{
			HttpResponseAndData response = client.postJsonHelper(algoUrl, input, queryParameters);
			Client.checkResult(response, "Algorithm call failed", false);

			AlgorithmResponseInternal<T> result = null;
			if (queryParameters["output"] == AlgorithmOutputType.RAW.getOutputType())
			{
				result = new AlgorithmResponseInternal<T>();
				result.byteResult = response.result;
			}
			else
			{
				result = JsonConvert.DeserializeObject<AlgorithmResponseInternal<T>>(Client.DEFAULT_ENCODING.GetString(response.result));
			}

			if (result.error != null)
			{
				throw new AlgorithmException(result.getErrorMessage());
			}

			if (result == null)
			{
				throw new AlgorithmiaException("Could not decode result from the API server");
			}

			return result.getAlgorithmResponse();
		}

		private class AlgorithmResponseInternal<T>
		{
			// Only used in async responses
			public string async;
			public string request_id;

			// Used in normal responses
			public ResponseMetadata metadata;
			public T result;
			public IDictionary<String, String> error;
			public Byte[] byteResult;

			public AlgorithmResponseInternal()
			{
				async = null;
				request_id = null;
				metadata = null;
				result = default(T);
				error = null;
				byteResult = null;
			}

			public AlgorithmResponse getAlgorithmResponse()
			{
				return new AlgorithmResponse(async, request_id, metadata, getResult(), error);
			}

			public Object getResult()
			{
				if (byteResult != null)
				{
					return byteResult;
				}

				return result;
			}

			public override String ToString()
			{
				return "result: " + result + " - error - " + error + " - " + metadata.ToString();
			}

			public String getErrorMessage()
			{
				if (error == null || error.Count == 0)
				{
					return null;
				}
				String errorMessage = "";
				if (error.ContainsKey("message"))
				{
					errorMessage = error["message"];
				}

				if (error.ContainsKey("stacktrace"))
				{
					if (errorMessage.Length > 0)
					{
						errorMessage += "\n";
					}
					errorMessage += "stacktrace: " + error["stacktrace"];
				}
				return errorMessage;
			}
		}
	}
}
