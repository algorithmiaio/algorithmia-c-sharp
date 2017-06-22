using System;
namespace Algorithmia
{
	public static class AlgorithmOutputType {
		public static readonly OutputType RAW = new RawOutputType();
		public static readonly OutputType VOID = new VoidOutputType();
		public static readonly OutputType DEFAULT = new DefaultOutputType();
	}

	public interface OutputType
	{
		String getOutputType();
	}

	public class RawOutputType : OutputType
	{
		public String getOutputType()
		{
			return "raw";
		}
	}

	public class VoidOutputType : OutputType
	{
		public String getOutputType()
		{
			return "void";
		}
	}

	public class DefaultOutputType : OutputType
	{
		public String getOutputType()
		{
			return "default";
		}
	}
}
