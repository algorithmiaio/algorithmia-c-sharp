using NUnit.Framework;
using System;
using Algorithmia;
namespace AlgorithmiaTest
{
	[TestFixture()]
	public class ClientTest
	{
		public static String DEFAULT_ALGORITHMIA_API_ADDRESS = "https://api.algorithmia.com";

		[Test()]
		public void checkApiAddressDefault()
		{
			Client client = new Client("key");
			Assert.AreEqual(client.apiAddress, DEFAULT_ALGORITHMIA_API_ADDRESS);
		}

		[Test()]
		public void checkApiAddressFromConstructor()
		{
			String address = "address";
			Client client = new Client("key", address);
			Assert.AreEqual(client.apiAddress, address);
		}

		[Test()]
		public void checkApiAddressFromEnvironmentVariable()
		{
			String address = "environment";
			System.Environment.SetEnvironmentVariable("ALGORITHMIA_API", address);
			Client client = new Client("key");
			Assert.AreEqual(client.apiAddress, address);
			System.Environment.SetEnvironmentVariable("ALGORITHMIA_API", null);
		}

		[Test()]
		public void checkGetAlgorithm()
		{
			Client client = new Client("key");
			Algorithm algo = client.algo("algo://test/algo");
			Assert.AreSame(algo.client, client);
		}

		[Test()]
		public void checkGetDataFile()
		{
			Client client = new Client("key");
			DataFile file = client.file("test/path.txt");
			Assert.AreSame(file.client, client);
		}

		[Test()]
		public void checkGetDataDirectory()
		{
			Client client = new Client("key");
			DataDirectory dir = client.dir("test/path");
			Assert.AreSame(dir.client, client);
		}

	}
}
