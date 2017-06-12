using NUnit.Framework;
using System;
using Algorithmia;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;

namespace AlgorithmiaTest
{
	[TestFixture()]
	public class AlgorithmTest
	{
		private static System.Text.Encoding ENCODING = System.Text.Encoding.UTF8;

		public const String ALGORITHMIA_API_KEY = "SET_ME_BEFORE_RUNNING_TEST"; 

		[Test()]
		public void checkAlgorithmPathErrors()
		{
			Client client = new Client("key");
			Assert.Throws(typeof(ArgumentException), delegate {
				client.algo("1");	
			});

			Assert.Throws(typeof(ArgumentException), delegate {
				client.algo("BLAH");
			});
		}

		[Test()]
		public void checkAlgorithmPath()
		{
			Client client = new Client("key");
			Assert.AreEqual(client.algo("a/b").algoUrl, "/v1/algo/a/b");
			Assert.AreEqual(client.algo("/c/d").algoUrl, "/v1/algo/c/d");
			Assert.AreEqual(client.algo("algo://e/f").algoUrl, "/v1/algo/e/f");
		}

		[Test()]
		public void checkAlgorithmResult()
		{ 
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe("1");
			Assert.AreEqual(response.metadata.content_type, "text");
			Assert.Greater(response.metadata.duration, 0);
			Assert.AreEqual(response.result, "Hello 1");
		}

		[Test()]
		public void checkAlgorithmNullInput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe(null);
			Assert.AreEqual(response.metadata.content_type, "text");
			Assert.Greater(response.metadata.duration, 0);
			Assert.AreEqual(response.result, "Hello null");
		}

		[Test()]
		public void checkAlgorithmIntegerInput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe(1);
			Assert.AreEqual(response.result, "Hello 1");
			Assert.Null(response.async);
			Assert.Null(response.request_id);
		}

		[Test()]
		[ExpectedException("Algorithmia.AlgorithmException", ExpectedMessage = "Failed to run algorithm - may have timed-out or hit out-of-memory error")]
		public void checkTimeout()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://testing/Sleep");
			algorithm.setOptions(2);
			AlgorithmResponse response = algorithm.pipe(120);
		}

		[Test()]
		[ExpectedException("Algorithmia.AlgorithmException", ExpectedMessage = "Algorithm call failed - reason: authorization required")]
		public void runHelloWorldUnauthenticated()
		{
			Client client = new Client("");
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe(1);
		}

		[Test()]
		[ExpectedException("Algorithmia.AlgorithmException", ExpectedMessage = "Algorithm call failed - reason: algorithm algo://demo/thisshouldneverexist not found")]
		public void runNonexistantAlgo()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/thisshouldneverexist");
			AlgorithmResponse response = algorithm.pipe(null);
		}

		[Test()]
		public void runWithNULLInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe(null);
			Assert.IsNull(response.result);
		}

		[Test()]
		public void runWithStringInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe("hello");
			Assert.AreEqual(response.result, "hello");
		}

		[Test()]
		public void runWithIntegerInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe(10);
			Assert.AreEqual(response.result, 10);
		}

		[Test()]
		public void runWithRawInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://quality/Python2xEcho");
			byte[] bytes = { 0, 1, 2, 3, 4 };
			AlgorithmResponse response = algorithm.pipe(bytes);
			Assert.AreEqual(response.result, bytes);
		}

		[Test()]
		public void runAsync()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://testing/Sleep");
			algorithm.setOptions(300, false, AlgorithmOutputType.VOID);
			AlgorithmResponse response = algorithm.pipe(10);
			Assert.AreEqual(response.async, "void");
			Assert.NotNull(response.request_id);

			Assert.Null(response.result);
			Assert.Null(response.metadata);
			Assert.Null(response.error);
		}

		[Test()]
		public void runRaw()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://quality/Python2xEcho");
			algorithm.setOptions(300, false, AlgorithmOutputType.RAW);
			AlgorithmResponse response = algorithm.pipe("hello");
			Assert.AreEqual(ENCODING.GetString((Byte[])response.result), "hello");
		}

	}
}
