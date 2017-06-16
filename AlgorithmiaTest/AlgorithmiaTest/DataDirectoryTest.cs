using NUnit.Framework;
using System;
using Algorithmia;
namespace AlgorithmiaTest
{
	[TestFixture()]
	public class DataDirectoryTest
	{
		private Client client = new Client(AlgorithmTest.ALGORITHMIA_API_KEY);

		[Test()]
		public void invalidPath()
		{
			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.dir("");
			});

			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.dir("/");
			});

			Assert.Throws(typeof(ArgumentException), delegate
			{
				client.dir("data://");
			});
		}

		[Test()]
		public void getName()
		{
			Assert.AreEqual(client.dir("data://what").getName(), "what");
			Assert.AreEqual(client.dir("data://a/b/c/").getName(), "c");
			Assert.AreEqual(client.dir("/a/b/c/d").getName(), "d");
		}

		[Test()]
		public void existsNotThere()
		{
			DataDirectory dd = client.dir("data://.my/neverShouldExist");
			Assert.False(dd.exists());
		}

		[Test()]
		public void createAndDelete()
		{
			DataDirectory dd = client.dir("data://.my/C_sharp_createAndDelete");
			Assert.False(dd.exists());

			Assert.AreSame(dd.create(), dd);
			Assert.True(dd.exists());

			Assert.AreSame(dd.delete(), dd);
			Assert.False(dd.exists());
		}

		[Test()]
		public void deleteForce()
		{
			DataDirectory dd = client.dir("data://.my/C_sharp_deleteForce");
			Assert.False(dd.exists());

			Assert.AreSame(dd.create(), dd);
			DataFile df = dd.file("file");
			Assert.False(df.exists());
			Assert.AreSame(df.put("C# delete force test"), df);

			Assert.Throws(typeof(DataApiException), delegate
			{
				dd.delete();
			});

			Assert.AreSame(dd.delete(true), dd);
			Assert.False(dd.exists());
		}

		[Test()]
		public void childDirectory()
		{
			DataDirectory dd = client.dir("data://.my/");
			Assert.True(dd.exists());

			DataDirectory childDir = dd.dir("C_sharp_dir");
			Assert.False(childDir.exists());

			childDir.create();
			childDir.file("inner").put(1.ToString());
			Assert.True(childDir.file("inner").exists());

			Assert.AreSame(childDir.delete(true), childDir);
			Assert.False(childDir.file("inner").exists());
			Assert.False(childDir.exists());
			Assert.True(dd.exists());
		}

		private void checkCreationWithAcl(DataDirectory dd, ReadDataAcl acl)
		{
			dd.create(acl);
			Assert.AreEqual(dd.getPermissions(), acl);
			dd.delete();
		}

		[Test()]
		public void directoryCreateWithACLandGet()
		{
			DataDirectory dd = client.dir("data://.my/C_sharp_directoryACLs");
			if (dd.exists())
			{
				dd.delete(true);
			}

			dd.create();
			Assert.AreEqual(dd.getPermissions(), ReadDataAcl.MY_ALGOS);
			dd.delete();

            checkCreationWithAcl(dd, ReadDataAcl.MY_ALGOS);
			checkCreationWithAcl(dd, ReadDataAcl.PRIVATE);
			checkCreationWithAcl(dd, ReadDataAcl.PUBLIC);
		}

		private void checkUpdateWithAcl(DataDirectory dd, ReadDataAcl acl)
		{
			dd.updatePermissions(acl);
			Assert.AreEqual(dd.getPermissions(), acl);
		}

		[Test()]
		public void directoryUpdateACLs()
		{
			DataDirectory dd = client.dir("data://.my/C_sharp_directoryUpdateACLs");
			if (!dd.exists())
			{
				dd.create();
			}

			checkUpdateWithAcl(dd, ReadDataAcl.PRIVATE);
			checkUpdateWithAcl(dd, ReadDataAcl.MY_ALGOS);
			checkUpdateWithAcl(dd, ReadDataAcl.PUBLIC);
		}

	}
}
