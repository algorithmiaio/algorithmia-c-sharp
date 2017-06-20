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

		[Test()]
		public void dirListFilesSmall()
		{
			DataDirectory dd = client.dir("data://.my/C_sharp_dirListFilesSmall");
			if (dd.exists())
			{
				dd.delete(true);
			}

			dd.create();

			dd.file("one").put("1");
			byte[] bytes = { 0, 1, 2, 3, 4 };
			dd.file("five").put(bytes);

			Boolean seen1 = false;
			Boolean seen5 = false;
			foreach (DataFile df in dd.files())
			{
				// The time now and the current time must differ by at most 2 minutes
				Assert.Greater(DateTime.Now.AddMinutes(2), df.getlastModifiedTime());
				Assert.Greater(df.getlastModifiedTime().AddMinutes(2), DateTime.Now);

				if (df.getName().Equals("one"))
				{
					seen1 = true;
					Assert.AreEqual(df.getSize(), 1);
					Assert.AreEqual("1", df.getString());
				}
				else if (df.getName().Equals("five"))
				{
					seen5 = true;
					Assert.AreEqual(df.getSize(), 5);
					Assert.AreEqual(bytes, df.getBytes());
				}
			}

			Assert.True(seen1);
			Assert.True(seen5);
		}

		[Test()]
		public void dirListDirs()
		{
			DataDirectory dd = client.dir("data://.my/");
			DataDirectory d1 = dd.dir("C_sharp_dirListDirectories_1");
			DataDirectory d2 = dd.dir("C_sharp_dirListDirectories_2");

			if (!d1.exists())
			{
				d1.create();
			}

			if (!d2.exists())
			{
				d2.create();
			}


			Boolean seen1 = false;
			Boolean seen2 = false;

			foreach (DataDirectory dcur in dd.dirs())
			{
				if (dcur.getName().Equals("C_sharp_dirListDirectories_1"))
				{
					seen1 = true;
				}
				else if (dcur.getName().Equals("C_sharp_dirListDirectories_2"))
				{
					seen2 = true;
				}
			}

			Assert.True(seen1);
			Assert.True(seen2);
		}

		[Test()]
		public void dirListFilesLarge()
		{
			DataDirectory dd = client.dir("data://.my/C_sharp_dirListFilesLarge");
			const int NUM_FILES = 2200;

			if (!dd.exists())
			{
				dd.create();
				for (int i = 0; i < NUM_FILES; i++)
				{
					dd.file(i + ".txt").put(i.ToString());
				}
			}

			Boolean[] seen = new Boolean[NUM_FILES];
			foreach (DataFile df in dd.files())
			{
				String index = df.getName().Replace(".txt", "");
				Assert.AreEqual(index.Length, df.getSize());
				Assert.False(seen[long.Parse(index)]);
				seen[long.Parse(index)] = true;
			}

			for (int i = 0; i < NUM_FILES; i++)
			{
				Assert.True(seen[i]);
			}
		}
	}
}
