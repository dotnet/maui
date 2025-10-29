using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	abstract class Gh1497BaseValidationBehavior<TBindable, TModel> : Behavior<TBindable> where TBindable : BindableObject
	{
	}

	sealed class Gh1497EntryValidationBehavior<TModel> : Gh1497BaseValidationBehavior<Entry, TModel>
	{
	}

	public partial class Gh1497 : ContentPage
	{
		public Gh1497()
		{
			InitializeComponent();
		}

		public Gh1497(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true), InlineData(false)]
			public void GenericsIssue(bool useCompiledXaml)
			{
				var layout = new Gh1497(useCompiledXaml);
				Assert.True(layout.entry.Behaviors[0], Is.TypeOf(typeof(Gh1497EntryValidationBehavior<Entry>)));
			}
		}
	}
}
