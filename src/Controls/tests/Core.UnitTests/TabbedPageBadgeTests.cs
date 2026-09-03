using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TabbedPageBadgeTests
	{
		[Fact]
		public void BadgePropertiesDefaultToNull()
		{
			var page = new ContentPage();

			Assert.Null(TabbedPage.GetBadgeText(page));
			Assert.Null(TabbedPage.GetBadgeColor(page));
			Assert.Null(TabbedPage.GetBadgeTextColor(page));
		}

		[Fact]
		public void BadgePropertiesCanBeSetAndCleared()
		{
			var page = new ContentPage();

			TabbedPage.SetBadgeText(page, "7");
			TabbedPage.SetBadgeColor(page, Colors.Red);
			TabbedPage.SetBadgeTextColor(page, Colors.White);

			Assert.Equal("7", TabbedPage.GetBadgeText(page));
			Assert.Equal(Colors.Red, TabbedPage.GetBadgeColor(page));
			Assert.Equal(Colors.White, TabbedPage.GetBadgeTextColor(page));

			TabbedPage.SetBadgeText(page, null);
			TabbedPage.SetBadgeColor(page, null);
			TabbedPage.SetBadgeTextColor(page, null);

			Assert.Null(TabbedPage.GetBadgeText(page));
			Assert.Null(TabbedPage.GetBadgeColor(page));
			Assert.Null(TabbedPage.GetBadgeTextColor(page));
		}

		[Fact]
		public void EmptyBadgeTextIsDistinctFromNull()
		{
			var page = new ContentPage();

			TabbedPage.SetBadgeText(page, "");

			Assert.Equal("", TabbedPage.GetBadgeText(page));
			Assert.NotNull(TabbedPage.GetBadgeText(page));
		}

		[Theory]
		[InlineData("BadgeText")]
		[InlineData("BadgeColor")]
		[InlineData("BadgeTextColor")]
		public void BadgePropertyChangeFires(string propertyName)
		{
			var page = new ContentPage();
			string changedProperty = null;
			page.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

			switch (propertyName)
			{
				case "BadgeText":
					TabbedPage.SetBadgeText(page, "3");
					break;
				case "BadgeColor":
					TabbedPage.SetBadgeColor(page, Colors.Blue);
					break;
				case "BadgeTextColor":
					TabbedPage.SetBadgeTextColor(page, Colors.Yellow);
					break;
			}

			Assert.Equal(propertyName, changedProperty);
		}

		[Fact]
		public void BadgePropertiesHaveCorrectMetadata()
		{
			Assert.Equal("BadgeText", TabbedPage.BadgeTextProperty.PropertyName);
			Assert.Equal(typeof(string), TabbedPage.BadgeTextProperty.ReturnType);
			Assert.Equal(typeof(TabbedPage), TabbedPage.BadgeTextProperty.DeclaringType);
			Assert.Null(TabbedPage.BadgeTextProperty.DefaultValue);

			Assert.Equal("BadgeColor", TabbedPage.BadgeColorProperty.PropertyName);
			Assert.Equal(typeof(Color), TabbedPage.BadgeColorProperty.ReturnType);
			Assert.Equal(typeof(TabbedPage), TabbedPage.BadgeColorProperty.DeclaringType);
			Assert.Null(TabbedPage.BadgeColorProperty.DefaultValue);

			Assert.Equal("BadgeTextColor", TabbedPage.BadgeTextColorProperty.PropertyName);
			Assert.Equal(typeof(Color), TabbedPage.BadgeTextColorProperty.ReturnType);
			Assert.Equal(typeof(TabbedPage), TabbedPage.BadgeTextColorProperty.DeclaringType);
			Assert.Null(TabbedPage.BadgeTextColorProperty.DefaultValue);
		}

		[Fact]
		public void BadgePropertiesSupportDataBinding()
		{
			var page = new ContentPage();
			page.SetBinding(TabbedPage.BadgeTextProperty, new Binding("Count"));
			page.SetBinding(TabbedPage.BadgeColorProperty, new Binding("Background"));
			page.SetBinding(TabbedPage.BadgeTextColorProperty, new Binding("Foreground"));

			page.BindingContext = new
			{
				Count = "42",
				Background = Colors.Orange,
				Foreground = Colors.Black
			};

			Assert.Equal("42", TabbedPage.GetBadgeText(page));
			Assert.Equal(Colors.Orange, TabbedPage.GetBadgeColor(page));
			Assert.Equal(Colors.Black, TabbedPage.GetBadgeTextColor(page));
		}

		[Fact]
		public void PagesCanHaveIndependentBadges()
		{
			var first = new ContentPage();
			var second = new ContentPage();

			TabbedPage.SetBadgeText(first, "1");
			TabbedPage.SetBadgeColor(first, Colors.Red);
			TabbedPage.SetBadgeText(second, "99+");
			TabbedPage.SetBadgeColor(second, Colors.Blue);

			Assert.Equal("1", TabbedPage.GetBadgeText(first));
			Assert.Equal(Colors.Red, TabbedPage.GetBadgeColor(first));
			Assert.Equal("99+", TabbedPage.GetBadgeText(second));
			Assert.Equal(Colors.Blue, TabbedPage.GetBadgeColor(second));
		}
	}
}
