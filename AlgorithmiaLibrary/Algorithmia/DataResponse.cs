using System;
using System.Collections.Generic;

namespace Algorithmia
{
	/*
	public class Message
	{
		public IDictionary<String, String> a;
		public Message()
		{
		}
	}
	*/

	public class Acl
	{
		public List<String> read;
		public Acl()
		{
		}
	}
	public class DataResponse
	{
		public IDictionary<String, String> error;
		//public Dictionary<String, Dictionary<String, List<String>>> acl;
		public Acl acl;
		public DataResponse()
		{
		}
	}
}
