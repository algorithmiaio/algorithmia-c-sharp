using System;
using System.Collections.Generic;

namespace Algorithmia
{
	public class DataAclType
	{
		public const String PUBLIC_PERMISSIONS = "user://*";
		public const String MY_ALGOS_PERMISSIONS = "algo://.my/*";

		public static readonly DataAclType PRIVATE = new DataAclType(new List<String>());
		public static readonly DataAclType PUBLIC = new DataAclType(new List<String>() { PUBLIC_PERMISSIONS });
		public static readonly DataAclType MY_ALGOS = new DataAclType(new List<String>() { MY_ALGOS_PERMISSIONS });


		private readonly List<String> aclStrings;

		public DataAclType(List<String> acls)
		{
			this.aclStrings = acls;
		}

		public List<String> getAclStrings()
		{
			return aclStrings;
		}

		public static DataAclType fromAclStrings(List<String> aclStrings)
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
