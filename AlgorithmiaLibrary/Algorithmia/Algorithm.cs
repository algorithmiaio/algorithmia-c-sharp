using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Algorithmia
{
    /// <summary>
    /// Represents an Algorithmia algorithm that can API calls on a user's behalf.
    /// </summary>
    public class Algorithm
    {
        private readonly Client client;
        private readonly string algoUrl;
        private static Regex algoPrefixReplacementRegex = new Regex("^(algo://|/)");
        private static Regex algoReferenceRegex = new Regex("^(\\w+/\\w+)");

        private Dictionary<string, string> queryParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Algorithmia.Algorithm"/> class.
        /// This normally should not be called. Instead use the client's <c>algo</c> method.
        /// </summary>
        /// <param name="client">Client that is configured to talk to the correct Algorithmia API endpoint</param>
        /// <param name="algoRef">The algorithm unique identifier: [author]/[algorithm name]/[optional version]</param>
        public Algorithm(Client client, string algoRef)
        {
            this.client = client;
            algoUrl = getAlgorithmUrl(algoRef);

            queryParameters = new Dictionary<string, string>();
            queryParameters["timeout"] = 300.ToString();
            queryParameters["stdout"] = false.ToString();
            queryParameters["output"] = AlgorithmOutputTypes.DEFAULT.getOutputType();
        }

        /// <summary>
        /// Sets the options for the next algorithm call.
        /// </summary>
        /// <returns>A pointer to <c>this</c>.</returns>
        /// <param name="timeout">The number of seconds we will allow the algorithm to run. Default is 300 seconds (5 minutes)</param>
        /// <param name="stdout">If we want to get the standard output of the algorithm calls. You can only get output for your own algorithms</param>
        /// <param name="outputType">Type of output. Default is the normal response parsing. Raw gets the byte array. Void is for asynchronous calls </param>
        /// <param name="options">Dictionary for options to send to the server. Should normally be null.</param>
        public Algorithm setOptions(int timeout = 300, bool stdout = false, AlgorithmOutputType outputType = AlgorithmOutputType.DEFAULT, Dictionary<string, string> options = null)
        {
            var copy = (options == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(options);

            copy["timeout"] = timeout.ToString();
            copy["stdout"] = stdout.ToString();

            switch (outputType)
            {
                case AlgorithmOutputType.DEFAULT:
                    {
                        copy["output"] = AlgorithmOutputTypes.DEFAULT.getOutputType();
                        break;
                    }
                case AlgorithmOutputType.RAW:
                    {
                        copy["output"] = AlgorithmOutputTypes.RAW.getOutputType();
                        break;
                    }
                case AlgorithmOutputType.VOID:
                    {
                        copy["output"] = AlgorithmOutputTypes.VOID.getOutputType();
                        break;
                    }
            }

            queryParameters = copy;

            return this;
        }

        private string getAlgorithmUrl(string algoRef)
        {
            if (algoRef == null || algoRef.Length == 0)
            {
                throw new ArgumentException("Invalid algorithm URI");
            }

            // Get rid of the starting slash or "algo://"
            var path = algoPrefixReplacementRegex.Replace(algoRef, "");

            if (!algoReferenceRegex.Match(path).Success)
            {
                throw new ArgumentException("Invalid algorithm URI: " + algoRef);
            }

            return "/v1/algo/" + path;
        }

        /// <summary>
        /// Call the algorithm with the <c>input</c>.
        /// </summary>
        /// <returns>An AlgorithmResponse with the result of the call.</returns>
        /// <param name="input">The input which which will be json serialized and sent as the input to the algorithm call.</param>
        /// <typeparam name="T">The type of the <c>result</c> object in the <c>AlgorithmResponse</c>. If the output is Raw, this is ignored and a byte[] array is used instead.</typeparam>
        public AlgorithmResponse pipe<T>(object input)
        {
            var response = client.postJsonHelper(algoUrl, input, queryParameters);
            Client.checkResult(response, "Algorithm call failed", false);

            AlgorithmResponseInternal<T> result = null;
            if (queryParameters["output"] == AlgorithmOutputTypes.RAW.getOutputType())
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
            public IDictionary<string, string> error;
            public byte[] byteResult;

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

            public object getResult()
            {
                if (byteResult != null)
                {
                    return byteResult;
                }

                return result;
            }

            public override string ToString()
            {
                return "result: " + result + " - error - " + error + " - " + metadata;
            }

            public string getErrorMessage()
            {
                if (error == null || error.Count == 0)
                {
                    return null;
                }
                var errorMessage = "";
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
