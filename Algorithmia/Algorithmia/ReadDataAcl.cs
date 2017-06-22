using System;
using System.Collections.Generic;

namespace Algorithmia
{
	public class ReadDataAcl
	{
		private const String PUBLIC_PERMISSIONS = "user://*";
		private const String MY_ALGOS_PERMISSIONS = "algo://.my/*";

		public static readonly ReadDataAcl PRIVATE = new ReadDataAcl(new List<String>());
		public static readonly ReadDataAcl PUBLIC = new ReadDataAcl(new List<String>() { PUBLIC_PERMISSIONS });
		public static readonly ReadDataAcl MY_ALGOS = new ReadDataAcl(new List<String>() { MY_ALGOS_PERMISSIONS });


		private readonly List<String> aclStrings;

		public ReadDataAcl(List<String> acls)
		{
			this.aclStrings = acls;
		}

		public List<String> getAclStrings()
		{
			return aclStrings;
		}

		public static ReadDataAcl fromAclStrings(List<String> aclStrings)
		{
			if (aclStrings == null)
			{
				return null;
			}
			else if (aclStrings.Count == 0)
			{
				return PRIVATE;
			}
			else if (aclStrings[0].Equals(PUBLIC_PERMISSIONS))
			{
				return PUBLIC;
			}
			else if (aclStrings[0].Equals(MY_ALGOS_PERMISSIONS))
			{
				return MY_ALGOS;
			}

			return null;
		}
	}

}
