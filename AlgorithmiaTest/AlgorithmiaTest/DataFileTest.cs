using NUnit.Framework;
using System;
using Algorithmia;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Runtime.Serialization;

namespace AlgorithmiaTest
{
	[TestFixture()]
	public class DataFileTest
	{
		private Client client = new Client(AlgorithmTest.ALGORITHMIA_API_KEY);

		[Test()]
		public void invalidPath()
		{
			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.file("");
			});

			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.file("/");
			});

			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.file("a/b/c/");
			});

			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.file("data://");
			});

			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.file("data://blah/file.txt/");
			});
		}

		[Test()]
		public void getName()
		{
			DataFile df = client.file("a");
			Assert.AreEqual(df.getName(), "a");

			df = client.file("really/long/path");
			Assert.AreEqual(df.getName(), "path");

			df = client.file("data://a/b/c/d.txt");
			Assert.AreEqual(df.getName(), "d.txt");
		}


		[Test()]
		public void unfinished_set_attributes()
		{
		}


		[Test()]
		[ExpectedException("Algorithmia.DataApiException", ExpectedMessage = "Delete failed - reason: path not found")]
		public void deleteNonExistantFile()
		{
			DataFile df = client.file("data://.my/doesNotExist/doesNotExist.txt");
			Assert.False(df.exists());
			df.delete();
		}


		[Test()]
		public void fileCreationAndDeletion()
		{
			DataFile df = client.file("data://.my/largeFiles/C_sharp.file");
			Assert.False(df.exists());

			Assert.AreSame(df.put("Hello"), df);
			Assert.True(df.exists());

			Assert.True(df.delete());
			Assert.False(df.exists());
		}

		[Test()]
		public void filePutAndGetString()
		{
			DataFile df = client.file("data://.my/largeFiles/C_sharp_string.txt");

			Assert.AreSame(df.put("Hello"), df);
			Assert.AreEqual("Hello", df.getString());

			Assert.True(df.delete());
		}

		[Test()]
		public void filePutAndGetByteArray()
		{
			DataFile df = client.file("data://.my/largeFiles/C_sharp_byte_array.txt");
			byte[] bytes = { 0, 1, 2, 3, 4 };

			Assert.AreSame(df.put(bytes), df);
			Assert.AreEqual(bytes, df.getBytes());

			Assert.True(df.delete());
		}

		private static Stream generateStreamFromString(string s)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		[Test()]
		public void filePutStreamAndGet()
		{
			DataFile df = client.file("data://.my/largeFiles/C_sharp_byte_stream.txt");

			Assert.AreSame(df.put(generateStreamFromString("Hello Stream")), df);
			Assert.AreEqual("Hello Stream", df.getString());

			Assert.True(df.delete());
		}

		[Test()]
		public void makeAndGetLargeFile()
		{
			int limit = 1000000;
			String fileName = Path.GetTempFileName();
			using (var sw = new StreamWriter(fileName)) {
				for (int i = 0; i < limit; i++)
				{
					sw.WriteLine(i);
				}
			}

			DataFile df = client.file("data://.my/largeFiles/C_sharp_1000000Numbers");
			Assert.AreSame(df.put(File.OpenRead(fileName)), df);

			FileStream downloadedFile = df.getFile();
			StreamReader reader = new StreamReader(downloadedFile);
			int next = 0;
			while (!reader.EndOfStream)
			{
				Assert.AreEqual(next.ToString(), reader.ReadLine());
				next++;
			}
			Assert.AreEqual(next, limit);

			File.Delete(fileName);
			File.Delete(downloadedFile.Name);
		}

	}
}
