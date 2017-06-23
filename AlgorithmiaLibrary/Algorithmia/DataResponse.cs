using System.Collections.Generic;

namespace Algorithmia
{
    class Acl
    {
        public List<string> read;

        public Acl()
        {
            read = null;
        }
    }

    class DataResponse
    {
        public IDictionary<string, string> error;
        public Acl acl;

        public DataResponse()
        {
            error = null;
            acl = null;
        }
    }
}
