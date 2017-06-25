namespace Algorithmia
{
    public static class AlgorithmOutputType
    {
        public static readonly OutputType RAW = new RawOutputType();
        public static readonly OutputType VOID = new VoidOutputType();
        public static readonly OutputType DEFAULT = new DefaultOutputType();
    }

    public interface OutputType
    {
        string getOutputType();
    }

    internal class RawOutputType : OutputType
    {
        public string getOutputType()
        {
            return "raw";
        }
    }

    internal class VoidOutputType : OutputType
    {
        public string getOutputType()
        {
            return "void";
        }
    }

    internal class DefaultOutputType : OutputType
    {
        public string getOutputType()
        {
            return "default";
        }
    }
}
