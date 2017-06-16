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
		public readonly string async;
		public readonly string request_id;

		// Used in normal responses
		public readonly ResponseMetadata metadata;
		public readonly Object result;
		public readonly IDictionary<String, String> error;

		public AlgorithmResponse(string async, string request_id, ResponseMetadata metadata, Object result, IDictionary<String, String> error)
		{
			this.async = async;
			this.request_id = request_id;

			this.metadata = metadata;
			this.result = result;
			this.error = error;
		}
	}
}
