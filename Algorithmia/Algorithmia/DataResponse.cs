using System;
using System.Collections.Generic;

namespace Algorithmia
{
	class Acl
	{
		public List<String> read;

		public Acl()
		{
			read = null;
		}
	}

	class DataResponse
	{
		public IDictionary<String, String> error;
		public Acl acl;

		public DataResponse()
		{
			error = null;
			acl = null;
		}
	}
}
