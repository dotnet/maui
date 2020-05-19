using System;
using System.Collections.Generic;

using System.Maui;
using NUnit.Framework;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Bz37524 : ContentPage
	{
		public Bz37524 ()
		{
			InitializeComponent ();
		}

		public Bz37524 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void MultiTriggerConditionNotApplied (bool useCompiledXaml)
			{
				var layout = new Bz37524 (useCompiledXaml);
				Assert.AreEqual (false, layout.TheButton.IsEnabled);
			}
		}
	}
}