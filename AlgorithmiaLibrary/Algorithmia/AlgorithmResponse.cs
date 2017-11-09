using System.Collections.Generic;

namespace Algorithmia
{
    /// <summary>
    /// Response metadata for default (the standard) algorithm requests.
    /// </summary>
    public class ResponseMetadata
    {
        /// <summary>
        /// The content type of the result.
        /// </summary>
        public string content_type;


        /// <summary>
        /// The execution duration.
        /// </summary>
        public double duration;


        /// <summary>
        /// If this algorithm was called with the timing option enabled, this stores that timing data.
        /// </summary>
        public IDictionary<string, double> timing;

        internal ResponseMetadata()
        {
        }
    }

    /// <summary>
    /// Algorithm response object.
    /// </summary>
    public class AlgorithmResponse
    {
        /// <summary>
        /// Async type used when making the call. Only used in asynchronous calls.
        /// </summary>
        public readonly string async;

        /// <summary>
        /// The request identifier. Only used in asynchronous calls.
        /// </summary>
        public readonly string request_id;

        /// <summary>
        /// Metadata for the request. Only used in synchronous calls.
        /// </summary>
        public readonly ResponseMetadata metadata;

        /// <summary>
        /// The result cast to object. If this was called with <c>AlgorithmOutputType.RAW</c> this is a byte array.
        /// Otherwise it is the type passed into the <c>pipe</c> method that created the object. Only used in synchronous calls.
        /// </summary>
        public readonly object result;

        /// <summary>
        /// If this algorithm call failed, this holds the error messages. Only used in synchronous calls.
        /// </summary>
        public readonly IDictionary<string, string> error;

        internal AlgorithmResponse(string async, string request_id, ResponseMetadata metadata, object result, IDictionary<string, string> error)
        {
            this.async = async;
            this.request_id = request_id;

            this.metadata = metadata;
            this.result = result;
            this.error = error;
        }
    }
}
