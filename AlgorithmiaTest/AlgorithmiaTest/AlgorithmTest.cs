using NUnit.Framework;
using System;
using Algorithmia;
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
		public void checkAlgorithmResult()
		{ 
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe<String>("1");
			Assert.AreEqual(response.metadata.content_type, "text");
			Assert.Greater(response.metadata.duration, 0);
			Assert.AreEqual(response.result, "Hello 1");
		}

		[Test()]
		public void checkAlgorithmNullInput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe<String>(null);
			Assert.AreEqual(response.metadata.content_type, "text");
			Assert.Greater(response.metadata.duration, 0);
			Assert.AreEqual(response.result, "Hello null");
		}

		[Test()]
		public void checkAlgorithmIntegerInput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe<String>(1);
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
			AlgorithmResponse response = algorithm.pipe<Object>(120);
		}

		[Test()]
		[ExpectedException("Algorithmia.AlgorithmException", ExpectedMessage = "Algorithm call failed - reason: authorization required")]
		public void runHelloWorldUnauthenticated()
		{
			Client client = new Client("");
			Algorithm algorithm = client.algo("algo://demo/hello");
			AlgorithmResponse response = algorithm.pipe<Object>(1);
		}

		[Test()]
		[ExpectedException("Algorithmia.AlgorithmException", ExpectedMessage = "Algorithm call failed - reason: algorithm algo://demo/thisshouldneverexist not found")]
		public void runNonexistantAlgo()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://demo/thisshouldneverexist");
			AlgorithmResponse response = algorithm.pipe<Object>(null);
		}

		[Test()]
		public void runWithNULLInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe<Object>(null);
			Assert.IsNull(response.result);
		}

		[Test()]
		public void runWithStringInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe<String>("hello");
			Assert.AreEqual(response.result, "hello");
		}

		[Test()]
		public void runWithStringInputAsObjectOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe<Object>("hello");
			Assert.AreEqual(response.result, "hello");
		}

		[Test()]
		public void runWithList()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			List<String> list = new List<String> { "a", "1" };
			AlgorithmResponse response = algorithm.pipe<List<String>>(list);
			Assert.AreEqual(response.result, list);
		}

		public class TestObject
		{
			public String word;
			public int number;
			public TestObject()
			{
			}
			public TestObject(String w, int n)
			{
				word = w;
				number = n;
			}

			public override bool Equals(Object o)
			{
				TestObject other = (TestObject)o;
				return other.word.Equals(this.word) && other.number == this.number;
			}

			// Just here for the warning that comes from overriding Equals
			public override int GetHashCode()
			{
				return 7;
			}
		}

		[Test()]
		public void runWithObject()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			TestObject input = new TestObject("a", 1);
			AlgorithmResponse response = algorithm.pipe<TestObject>(input);
			Assert.AreEqual(response.result, input);
		}

		[Test()]
		public void runWithIntegerInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe<int>(10);
			Assert.AreEqual(response.result, 10);
		}

		[Test()]
		public void runWithIntegerInputAsObjectOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://util/Echo");
			AlgorithmResponse response = algorithm.pipe<Object>(10);
			Assert.AreEqual(response.result, 10);
		}

		[Test()]
		public void runWithRawInputOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://quality/Python2xEcho");
			byte[] bytes = { 0, 1, 2, 3, 4 };
			AlgorithmResponse response = algorithm.pipe<byte[]>(bytes);
			Assert.AreEqual(response.result, bytes);
		}

		[Test()]
		public void runWithRawInputStringOutput()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://quality/Python2xEcho");
			byte[] bytes = { 0, 1, 2, 3, 4 };
			AlgorithmResponse response = algorithm.pipe<String>(bytes);
			Assert.AreEqual(response.result, Convert.ToBase64String(bytes));
		}

		[Test()]
		public void runAsync()
		{
			Client client = new Client(ALGORITHMIA_API_KEY);
			Algorithm algorithm = client.algo("algo://testing/Sleep");
			algorithm.setOptions(300, false, AlgorithmOutputType.VOID);
			AlgorithmResponse response = algorithm.pipe<Object>(10);
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
			AlgorithmResponse response = algorithm.pipe<byte[]>("hello");
			Assert.AreEqual(ENCODING.GetString((Byte[])response.result), "hello");
		}

	}
}
