using System;

namespace Algorithmia
{
    /// <summary>
    /// Base exception for all exceptions thrown during algorithm or data API calls.
    /// </summary>
    public class AlgorithmiaException : Exception
    {
        internal AlgorithmiaException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception that is used for data-related errors.
    /// </summary>
    public class DataApiException : AlgorithmiaException
    {
        internal DataApiException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception that is used for algorithm-related errors.
    /// </summary>
    public class AlgorithmException : AlgorithmiaException
    {
        internal AlgorithmException(string message)
            : base(message)
        {
        }
    }
}
