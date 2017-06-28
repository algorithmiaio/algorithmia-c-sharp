using System.Collections.Generic;

namespace Algorithmia
{
    /// <summary>
    /// Read data ACLs for data directories.
    /// </summary>
    public class ReadDataAcl
    {
        private const string PUBLIC_PERMISSIONS = "user://*";
        private const string MY_ALGOS_PERMISSIONS = "algo://.my/*";

        /// <summary>
        /// The private ACL - only your algorithms while you call them can read this directory.
        /// </summary>
        public static readonly ReadDataAcl PRIVATE = new ReadDataAcl(new List<string>());

        /// <summary>
        /// The public ACL - any algorithm call can read from this directory.
        /// </summary>
        public static readonly ReadDataAcl PUBLIC = new ReadDataAcl(new List<string>() { PUBLIC_PERMISSIONS });

        /// <summary>
        /// My algorithms ACL - any of my algorithms called by anyone on the platform can read this directory.
        /// </summary>
        public static readonly ReadDataAcl MY_ALGOS = new ReadDataAcl(new List<string>() { MY_ALGOS_PERMISSIONS });


        private readonly List<string> aclStrings;

        internal ReadDataAcl(List<string> acls)
        {
            aclStrings = acls;
        }

        internal List<string> getAclStrings()
        {
            return aclStrings;
        }

        internal static ReadDataAcl fromAclStrings(List<string> aclStrings)
        {
            if (aclStrings == null)
            {
                return null;
            }
            if (aclStrings.Count == 0)
            {
                return PRIVATE;
            }
            if (aclStrings[0].Equals(PUBLIC_PERMISSIONS))
            {
                return PUBLIC;
            }
            if (aclStrings[0].Equals(MY_ALGOS_PERMISSIONS))
            {
                return MY_ALGOS;
            }

            return null;
        }
    }

}
