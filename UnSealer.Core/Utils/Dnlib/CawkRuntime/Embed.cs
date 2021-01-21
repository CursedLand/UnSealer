using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace UnSealer.Core.Utils.Dnlib.CawkRuntime.ConversionBack
{
	public class EmbeddedDllClass
	{
		private static string tempFolder = "";

		/// <summary>
		/// Extract DLLs from resources to temporary folder
		/// </summary>
		/// <param name="dllName">name of DLL file to create (including dll suffix)</param>
		/// <param name="resourceBytes">The resource name (fully qualified)</param>
		public static void ExtractEmbeddedDlls(string dllName, byte[] resourceBytes)
		{
			Assembly assem = Assembly.GetExecutingAssembly();
			string[] names = assem.GetManifestResourceNames();
			AssemblyName an = assem.GetName();

			// The temporary folder holds one or more of the temporary DLLs
			// It is made "unique" to avoid different versions of the DLL or architectures.
			tempFolder = String.Format("{0}.{1}.{2}", an.Name, an.ProcessorArchitecture, an.Version);

			string dirName = Path.Combine(Path.GetTempPath(), tempFolder);
			if (!Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}

			// Add the temporary dirName to the PATH environment variable (at the head!)
			string path = Environment.GetEnvironmentVariable("PATH");
			string[] pathPieces = path.Split(';');
			bool found = false;
			foreach (string pathPiece in pathPieces)
			{
				if (pathPiece == dirName)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				Environment.SetEnvironmentVariable("PATH", dirName + ";" + path);
			}

			// See if the file exists, avoid rewriting it if not necessary
			string dllPath = Path.Combine(dirName, dllName);
			bool rewrite = true;
			if (File.Exists(dllPath))
			{
				byte[] existing = File.ReadAllBytes(dllPath);
				if (Equality(resourceBytes,existing))
				{
					rewrite = false;
				}
			}
			if (rewrite)
			{
				File.WriteAllBytes(dllPath, resourceBytes);
			}
		}
		public static bool Equality(byte[] a1, byte[] b1)
		{
			int i;
			if (a1.Length == b1.Length)
			{
				i = 0;
				while (i < a1.Length && (a1[i] == b1[i])) //Earlier it was a1[i]!=b1[i]
				{
					i++;
				}
				if (i == a1.Length)
				{
					return true;
				}
			}

			return false;
		}
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibraryEx(string dllToLoad, IntPtr hFile, uint flags);


		/// <summary>
		/// managed wrapper around LoadLibrary
		/// </summary>
		/// <param name="dllName"></param>
		static public IntPtr LoadDll(string dllName)
		{
			if (tempFolder == string.Empty)
			{
				throw new Exception("Please call ExtractEmbeddedDlls before LoadDll");
			}
			IntPtr h = LoadLibraryEx(dllName,IntPtr.Zero,0);
			if (h == IntPtr.Zero)
			{
				Exception e = new Win32Exception();
				throw new DllNotFoundException("Unable to load library: " + dllName + " from " + tempFolder, e);
			}
			return h;
		}

	}
}
