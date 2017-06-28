namespace Algorithmia
{
    /// <summary>
    /// The valid algorithm output types.
    /// </summary>
    public enum AlgorithmOutputType
    {
        /// <summary>
        /// The default output type. Deserializes the response as a json object with metadata.
        /// </summary>
        DEFAULT,

        /// <summary>
        /// The raw output type. Deserializes the response as a byte array without the metadata.
        /// </summary>
        RAW,

        /// <summary>
        /// The void output type. This makes the request in an asynchronous manner.
        /// </summary>
        VOID
    };

    internal static class AlgorithmOutputTypes
    {
        public static readonly OutputTypeInterface RAW = new RawOutputType();
        public static readonly OutputTypeInterface VOID = new VoidOutputType();
        public static readonly OutputTypeInterface DEFAULT = new DefaultOutputType();
    }

    internal interface OutputTypeInterface
    {
        string getOutputType();
    }

    internal class RawOutputType : OutputTypeInterface
    {
        public string getOutputType()
        {
            return "raw";
        }
    }

    internal class VoidOutputType : OutputTypeInterface
    {
        public string getOutputType()
        {
            return "void";
        }
    }

    internal class DefaultOutputType : OutputTypeInterface
    {
        public string getOutputType()
        {
            return "default";
        }
    }
}
