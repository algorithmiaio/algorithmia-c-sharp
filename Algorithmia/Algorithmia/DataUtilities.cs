﻿using System;
using System.Text.RegularExpressions;

namespace Algorithmia
{
	public static class DataUtilities
	{
		private static Regex dataPrefixReplacementRegex = new Regex("^(data://|/)");
		private static Regex endsWithSlashRegex = new Regex("/$");
		public static String getDataPath(String input, bool isFile)
		{
			String path = dataPrefixReplacementRegex.Replace(input, "");

			if (isFile && path.EndsWith("/", true, null))
			{
				throw new ArgumentException("Invalid file path ending: " + path);
			}

			while (endsWithSlashRegex.Match(path).Success)
			{
				path = path.Substring(0, path.Length - 1);
			}

			if (path.Length == 0)
			{
				throw new ArgumentException("Data path cannot be empty" + input);
			} 


			return path;
		}

		public static String getDataUrl(String validPath)
		{
			return "/v1/data/" + validPath;
		}
	}
}