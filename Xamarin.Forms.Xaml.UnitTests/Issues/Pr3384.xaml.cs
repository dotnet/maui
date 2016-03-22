using System;
using System.Collections.Generic;

using Xamarin.Forms;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Pr3384 : ContentPage
	{
		public Pr3384 ()
		{
			InitializeComponent ();
		}

		public Pr3384 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp ()
			{
				Device.OS = TargetPlatform.iOS;
			}

			[TestCase (false)]
			[TestCase (true)]
			public void RecyclingStrategyIsHandled (bool useCompiledXaml)
			{
				var p = new Pr3384 (useCompiledXaml);
				Assert.AreEqual (ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
			}
		}
	}
}