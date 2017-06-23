using System;
namespace Algorithmia
{
    public class AlgorithmiaException : Exception
    {
        public AlgorithmiaException(string message)
            : base(message)
        {
        }
    }

    public class DataApiException : AlgorithmiaException
    {
        public DataApiException(string message)
            : base(message)
        {
        }
    }

    public class AlgorithmException : AlgorithmiaException
    {
        public AlgorithmException(string message)
            : base(message)
        {
        }
    }
}
