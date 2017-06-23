using System.Collections.Generic;

namespace Algorithmia
{
    public class ResponseMetadata
    {
        public string content_type;
        public double duration;

        public ResponseMetadata()
        {
        }

        public ResponseMetadata(string ct, double d)
        {
            content_type = ct;
            duration = d;
        }

        public override string ToString()
        {
            return content_type + " - " + duration;
        }
    }

    public class AlgorithmResponse
    {
        // Only used in async responses
        public readonly string async;
        public readonly string request_id;

        // Used in normal responses
        public readonly ResponseMetadata metadata;
        public readonly object result;
        public readonly IDictionary<string, string> error;

        public AlgorithmResponse(string async, string request_id, ResponseMetadata metadata, object result, IDictionary<string, string> error)
        {
            this.async = async;
            this.request_id = request_id;

            this.metadata = metadata;
            this.result = result;
            this.error = error;
        }
    }
}
