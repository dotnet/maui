using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz27968Page : ContentPage
	{
	}

	public partial class Bz27968 : Bz27968Page
	{
		public Bz27968 ()
		{
			InitializeComponent ();
		}

		public Bz27968 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void BaseClassIdentifiersAreValidForResources (bool useCompiledXaml)
			{
				var layout = new Bz27968 (useCompiledXaml);
				Assert.That (layout.Resources ["listView"], Is.TypeOf<ListView> ());
			}
		}
	}
}
