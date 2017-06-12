using System;
using System.Collections.Generic;

namespace Algorithmia
{
	public class ResponseMetadata
	{
		public String content_type;
		public double duration;

		public ResponseMetadata()
		{
		}

		public ResponseMetadata(String ct, double d)
		{
			content_type = ct;
			duration = d;
		}

		public override string ToString()
		{
			return content_type + " - " + duration.ToString();
		}
	}

	public class AlgorithmResponse
	{
		// Only used in async responses
		public string async;
		public string request_id;

		// Used in normal responses
		public ResponseMetadata metadata;
		public Object result;
		public IDictionary<String, String> error;
		public AlgorithmResponse()
		{
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
