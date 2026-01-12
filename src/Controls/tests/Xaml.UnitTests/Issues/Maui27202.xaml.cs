using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui27202 : ContentPage
{
	public Maui27202() => InitializeComponent();

	public Maui27202(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
		}

		public void Dispose()
		{
			Application.Current = null;
		}

		[Theory]
		[XamlInflatorData]
		internal void DerivedStylesInheritVisualStateManager(XamlInflator inflator)
		{
			var page = new Maui27202(inflator);

			// Verify styles are applied
			Assert.Equal(Colors.Green, page.EnabledLabel1.TextColor);
			
			// Verify VSG exists
			var groups = VisualStateManager.GetVisualStateGroups(page.DisabledLabel1);
			Assert.NotNull(groups);
			Assert.True(groups.Count > 0);
			
			// Check if GoToState succeeds
			var gotoResult = VisualStateManager.GoToState(page.DisabledLabel1, "Disabled");
			
			// Output for debugging
			Console.WriteLine($"GoToState result: {gotoResult}");
			Console.WriteLine($"TextColor after GoToState: {page.DisabledLabel1.TextColor}");
			Console.WriteLine($"Expected: Gray ({Colors.Gray})");
			
			Assert.True(gotoResult, "GoToState should succeed");
			Assert.Equal(Colors.Gray, page.DisabledLabel1.TextColor);
		}
	}
}

// Custom label controls for testing derived styles
public class CustomLabel1 : Label
{
}

public class CustomLabel2 : Label
{
}
