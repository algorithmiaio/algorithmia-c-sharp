using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Algorithmia
{
	public class Algorithm
	{
		public readonly Client client;
		public readonly String algoUrl;
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

		public AlgorithmResponse pipe(Object input)
		{
			HttpResponseAndData response = client.postJsonHelper(algoUrl, input, queryParameters);
			Client.checkResult(response, "Algorithm call failed", false);

			AlgorithmResponse result = null;
			if (queryParameters["output"] == AlgorithmOutputType.RAW.getOutputType())
			{
				result = new AlgorithmResponse();
				result.result = response.result;
			}
			else
			{
				result = JsonConvert.DeserializeObject<AlgorithmResponse>(Client.DEFAULT_ENCODING.GetString(response.result));
			}

			if (result.error != null)
			{
				throw new AlgorithmException(result.getErrorMessage());
			}

			if (result.metadata != null && result.metadata.content_type == "binary")
			{
				result.result = Convert.FromBase64String((String)result.result);
			}

			if (result == null)
			{
				throw new AlgorithmiaException("Could not decode result from the API server");
			}

			return result;
		}
	}
}
