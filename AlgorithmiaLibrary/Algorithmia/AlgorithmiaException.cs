using System;
namespace Algorithmia
{
	public class AlgorithmiaException : Exception 
	{
		public AlgorithmiaException(String message)
			: base(message) 
		{
		}
	}

	public class DataApiException : AlgorithmiaException
	{
		public DataApiException(String message)
			: base(message)
		{
		}	
	}

	public class AlgorithmException : AlgorithmiaException
	{
		public AlgorithmException(String message)
			: base(message)
		{
		}	
	}
}
