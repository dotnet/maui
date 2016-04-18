using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class SharedResourceDictionary : ResourceDictionary
	{
		public SharedResourceDictionary ()
		{
			InitializeComponent ();
		}

		public SharedResourceDictionary (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void ResourcesDirectoriesCanBeXamlRoots (bool useCompiledXaml)
			{
				var layout = new SharedResourceDictionary (useCompiledXaml);
				Assert.AreEqual (5, layout.Count);
			}
		}
	}
}