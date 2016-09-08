using System;

namespace Xamarin.Forms.Xaml
{
	internal static class XmlnsHelper
	{
		public static bool IsCustom(string ns)
		{
			switch (ns)
			{
				case "":
				case "http://xamarin.com/schemas/2014/forms":
					return false;
			}
			return true;
		}

		public static string ParseNamespaceFromXmlns(string xmlns)
		{
			string typeName;
			string ns;
			string asm;
			string targetPlatform;

			ParseXmlns(xmlns, out typeName, out ns, out asm, out targetPlatform);

			return ns;
		}

		public static void ParseXmlns(string xmlns, out string typeName, out string ns, out string asm, out string targetPlatform)
		{
			typeName = ns = asm = targetPlatform = null;

			foreach (var decl in xmlns.Split(';'))
			{
				if (decl.StartsWith("clr-namespace:", StringComparison.Ordinal))
				{
					ns = decl.Substring(14, decl.Length - 14);
					continue;
				}
				if (decl.StartsWith("assembly=", StringComparison.Ordinal))
				{
					asm = decl.Substring(9, decl.Length - 9);
					continue;
				}
				if (decl.StartsWith("targetPlatform=", StringComparison.Ordinal)) {
					targetPlatform = decl.Substring(15, decl.Length - 15);
					continue;
				}
				var nsind = decl.LastIndexOf(".", StringComparison.Ordinal);
				if (nsind > 0)
				{
					ns = decl.Substring(0, nsind);
					typeName = decl.Substring(nsind + 1, decl.Length - nsind - 1);
				}
				else
					typeName = decl;
			}
		}
	}
}