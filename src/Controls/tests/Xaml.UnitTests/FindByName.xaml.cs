// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class FindByName : ContentPage
	{
		public FindByName()
		{
			InitializeComponent();
		}

		public FindByName(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class FindByNameTests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void TestRootName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.AreSame(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
				Assert.AreSame(page, page.FindByName<FindByName>("root"));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.AreSame(page.label0, page.FindByName<Label>("label0"));
			}
		}
	}
}