using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34716 : ContentPage
{
	public Maui34716() => InitializeComponent();

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

		// Registers an implicit Button style with VisualStateGroups (like the default MAUI template Styles.xaml)
		void SetupImplicitButtonStyle()
		{
			Application.Current.Resources.Add(new Style(typeof(Button))
			{
				Setters =
				{
					new Setter
					{
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = new VisualStateGroupList
						{
							new VisualStateGroup
							{
								Name = "CommonStates",
								States =
								{
									new VisualState { Name = "Normal" },
									new VisualState { Name = "Pressed" },
									new VisualState { Name = "Disabled" }
								}
							}
						}
					}
				}
			});
		}

		[Theory]
		[XamlInflatorData]
		internal void VisualStateGroupsOnElementShouldNotThrowDuplicateNames(XamlInflator inflator)
		{
			var page = new Maui34716(inflator);
			Assert.NotNull(page);

			var groups = VisualStateManager.GetVisualStateGroups(page.button);
			Assert.Single(groups);
			Assert.Equal("CommonStates", groups[0].Name);
			Assert.Equal(2, groups[0].States.Count);
		}

		[Theory]
		[XamlInflatorData]
		internal void VisualStateGroupsWithImplicitStyleShouldNotThrowDuplicateNames(XamlInflator inflator)
		{
			// This is the real bug scenario: an implicit style already sets VisualStateGroups
			// with "CommonStates", then the XAML also sets VisualStateGroups with "CommonStates".
			// The SG calls GetValue() (returning the style's list) then Add() → duplicate name → crash.
			SetupImplicitButtonStyle();

			var page = new Maui34716(inflator);
			Assert.NotNull(page);

			// The explicit XAML VisualStateGroups should replace the implicit style's groups
			var groups = VisualStateManager.GetVisualStateGroups(page.button);
			Assert.Single(groups);
			Assert.Equal("CommonStates", groups[0].Name);
			Assert.Equal(2, groups[0].States.Count);
		}

		[Theory]
		[XamlInflatorData]
		internal void VisualStateGroupsViaStyleShouldNotThrow(XamlInflator inflator)
		{
			var page = new Maui34716(inflator);
			Assert.NotNull(page);

			var groups = VisualStateManager.GetVisualStateGroups(page.button2);
			Assert.Single(groups);
			Assert.Equal("CommonStates", groups[0].Name);
		}

		[Theory]
		[XamlInflatorData]
		internal void MultipleVisualStateGroupsShouldNotThrow(XamlInflator inflator)
		{
			var page = new Maui34716(inflator);
			Assert.NotNull(page);

			var groups = VisualStateManager.GetVisualStateGroups(page.button3);
			Assert.Equal(2, groups.Count);
			Assert.Equal("CommonStates", groups[0].Name);
			Assert.Equal("FocusStates", groups[1].Name);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExplicitVisualStateGroupListShouldNotThrow(XamlInflator inflator)
		{
			var page = new Maui34716(inflator);
			Assert.NotNull(page);

			var groups = VisualStateManager.GetVisualStateGroups(page.button4);
			Assert.Single(groups);
			Assert.Equal("CommonStates", groups[0].Name);
		}
	}
}
