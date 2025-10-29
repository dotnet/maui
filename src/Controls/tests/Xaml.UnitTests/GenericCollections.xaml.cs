using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class AttachedBP
	{
		public static readonly BindableProperty AttachedBPProperty = BindableProperty.CreateAttached(
			"AttachedBP",
			typeof(GenericCollection),
			typeof(AttachedBP),
			null);

		public static GenericCollection GetAttachedBP(BindableObject bindable)
		{
			throw new NotImplementedException();
		}
	}

	public class GenericCollection : ObservableCollection<object>
	{
	}

	public partial class GenericCollections : ContentPage
	{
		public GenericCollections()
		{
			InitializeComponent();
		}

		public GenericCollections(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SupportsCrookedGenericScenarios(bool useCompiledXaml)
		{
			var p = new GenericCollections(useCompiledXaml);
			Assert.Equal("Foo", (p.label0.GetValue(AttachedBP.AttachedBPProperty) as GenericCollection)[0]);
		}
	}
}