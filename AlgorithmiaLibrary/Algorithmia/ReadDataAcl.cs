using System.Collections.Generic;

namespace Algorithmia
{
    public class ReadDataAcl
    {
        private const string PUBLIC_PERMISSIONS = "user://*";
        private const string MY_ALGOS_PERMISSIONS = "algo://.my/*";

        public static readonly ReadDataAcl PRIVATE = new ReadDataAcl(new List<string>());
        public static readonly ReadDataAcl PUBLIC = new ReadDataAcl(new List<string>() { PUBLIC_PERMISSIONS });
        public static readonly ReadDataAcl MY_ALGOS = new ReadDataAcl(new List<string>() { MY_ALGOS_PERMISSIONS });


        private readonly List<string> aclStrings;

        public ReadDataAcl(List<string> acls)
        {
            aclStrings = acls;
        }

        public List<string> getAclStrings()
        {
            return aclStrings;
        }

        public static ReadDataAcl fromAclStrings(List<string> aclStrings)
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
