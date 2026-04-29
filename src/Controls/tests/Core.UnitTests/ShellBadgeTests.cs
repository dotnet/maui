using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ShellBadgeTests : ShellTestBase
	{
		[Fact]
		public void BadgeTextDefaultsToNull()
		{
			var shellSection = new ShellSection();
			Assert.Null(shellSection.BadgeText);
		}

		[Fact]
		public void BadgeColorDefaultsToNull()
		{
			var shellSection = new ShellSection();
			Assert.Null(shellSection.BadgeColor);
		}

		[Fact]
		public void BadgeTextCanBeSet()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeText = "5";
			Assert.Equal("5", shellSection.BadgeText);
		}

		[Fact]
		public void BadgeTextCanBeSetToEmptyString()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeText = "5";
			shellSection.BadgeText = "";
			Assert.Equal("", shellSection.BadgeText);
		}

		[Fact]
		public void BadgeTextCanBeCleared()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeText = "5";
			shellSection.BadgeText = null;
			Assert.Null(shellSection.BadgeText);
		}

		[Fact]
		public void BadgeColorCanBeSet()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeColor = Colors.Red;
			Assert.Equal(Colors.Red, shellSection.BadgeColor);
		}

		[Fact]
		public void BadgeColorCanBeCleared()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeColor = Colors.Red;
			shellSection.BadgeColor = null;
			Assert.Null(shellSection.BadgeColor);
		}

		[Fact]
		public void BadgeTextPropertyChangeFires()
		{
			var shellSection = new ShellSection();
			string changedProperty = null;
			shellSection.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

			shellSection.BadgeText = "3";
			Assert.Equal(nameof(BaseShellItem.BadgeText), changedProperty);
		}

		[Fact]
		public void BadgeColorPropertyChangeFires()
		{
			var shellSection = new ShellSection();
			string changedProperty = null;
			shellSection.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

			shellSection.BadgeColor = Colors.Blue;
			Assert.Equal(nameof(BaseShellItem.BadgeColor), changedProperty);
		}

		[Fact]
		public void BadgeTextWorksOnShellContent()
		{
			var shellContent = new ShellContent();
			shellContent.BadgeText = "99+";
			Assert.Equal("99+", shellContent.BadgeText);
		}

		[Fact]
		public void BadgeTextWorksOnShellItem()
		{
			var shellItem = new ShellItem();
			shellItem.BadgeText = "New";
			Assert.Equal("New", shellItem.BadgeText);
		}

		[Fact]
		public void BadgeTextWorksOnFlyoutItem()
		{
			var flyoutItem = new FlyoutItem();
			flyoutItem.BadgeText = "1";
			Assert.Equal("1", flyoutItem.BadgeText);
		}

		[Fact]
		public void BadgeTextWorksOnTabBar()
		{
			var tabBar = new TabBar();
			tabBar.BadgeText = "2";
			Assert.Equal("2", tabBar.BadgeText);
		}

		[Fact]
		public void BadgePropertiesWorkWithShellTabs()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();

			shell.Items.Add(shellItem);

			var section = shell.CurrentItem.CurrentItem;
			section.BadgeText = "5";
			section.BadgeColor = Colors.Red;

			Assert.Equal("5", section.BadgeText);
			Assert.Equal(Colors.Red, section.BadgeColor);
		}

		[Fact]
		public void BadgeTextBindablePropertyHasCorrectMetadata()
		{
			Assert.Equal(nameof(BaseShellItem.BadgeText), BaseShellItem.BadgeTextProperty.PropertyName);
			Assert.Equal(typeof(string), BaseShellItem.BadgeTextProperty.ReturnType);
			Assert.Equal(typeof(BaseShellItem), BaseShellItem.BadgeTextProperty.DeclaringType);
			Assert.Null(BaseShellItem.BadgeTextProperty.DefaultValue);
		}

		[Fact]
		public void BadgeColorBindablePropertyHasCorrectMetadata()
		{
			Assert.Equal(nameof(BaseShellItem.BadgeColor), BaseShellItem.BadgeColorProperty.PropertyName);
			Assert.Equal(typeof(Color), BaseShellItem.BadgeColorProperty.ReturnType);
			Assert.Equal(typeof(BaseShellItem), BaseShellItem.BadgeColorProperty.DeclaringType);
			Assert.Null(BaseShellItem.BadgeColorProperty.DefaultValue);
		}

		[Fact]
		public void BadgeTextSupportsDataBinding()
		{
			var shellSection = new ShellSection();
			var binding = new Binding("BadgeCount");
			shellSection.SetBinding(BaseShellItem.BadgeTextProperty, binding);

			shellSection.BindingContext = new { BadgeCount = "42" };
			Assert.Equal("42", shellSection.BadgeText);
		}

		[Fact]
		public void BadgeColorSupportsDataBinding()
		{
			var shellSection = new ShellSection();
			var binding = new Binding("AlertColor");
			shellSection.SetBinding(BaseShellItem.BadgeColorProperty, binding);

			shellSection.BindingContext = new { AlertColor = Colors.Orange };
			Assert.Equal(Colors.Orange, shellSection.BadgeColor);
		}

		[Fact]
		public void MultipleSectionsCanHaveDifferentBadges()
		{
			var shell = new Shell();
			var section1 = CreateShellSection();
			var section2 = CreateShellSection();
			var shellItem = new ShellItem();
			shellItem.Items.Add(section1);
			shellItem.Items.Add(section2);
			shell.Items.Add(shellItem);

			section1.BadgeText = "1";
			section1.BadgeColor = Colors.Red;
			section2.BadgeText = "99";
			section2.BadgeColor = Colors.Blue;

			Assert.Equal("1", section1.BadgeText);
			Assert.Equal(Colors.Red, section1.BadgeColor);
			Assert.Equal("99", section2.BadgeText);
			Assert.Equal(Colors.Blue, section2.BadgeColor);
		}

		[Fact]
		public void BadgeTextColorDefaultsToNull()
		{
			var shellSection = new ShellSection();
			Assert.Null(shellSection.BadgeTextColor);
		}

		[Fact]
		public void BadgeTextColorCanBeSet()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeTextColor = Colors.White;
			Assert.Equal(Colors.White, shellSection.BadgeTextColor);
		}

		[Fact]
		public void BadgeTextColorCanBeCleared()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeTextColor = Colors.Black;
			shellSection.BadgeTextColor = null;
			Assert.Null(shellSection.BadgeTextColor);
		}

		[Fact]
		public void BadgeTextColorPropertyChangedFires()
		{
			var shellSection = new ShellSection();
			bool fired = false;
			shellSection.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(BaseShellItem.BadgeTextColor))
					fired = true;
			};
			shellSection.BadgeTextColor = Colors.Yellow;
			Assert.True(fired);
		}

		[Fact]
		public void BadgeTextColorBindablePropertyHasCorrectMetadata()
		{
			Assert.Equal(nameof(BaseShellItem.BadgeTextColor), BaseShellItem.BadgeTextColorProperty.PropertyName);
			Assert.Equal(typeof(Color), BaseShellItem.BadgeTextColorProperty.ReturnType);
			Assert.Equal(typeof(BaseShellItem), BaseShellItem.BadgeTextColorProperty.DeclaringType);
			Assert.Null(BaseShellItem.BadgeTextColorProperty.DefaultValue);
		}

		[Fact]
		public void BadgeTextColorSupportsDataBinding()
		{
			var shellSection = new ShellSection();
			var binding = new Binding("TextColor");
			shellSection.SetBinding(BaseShellItem.BadgeTextColorProperty, binding);

			shellSection.BindingContext = new { TextColor = Colors.Cyan };
			Assert.Equal(Colors.Cyan, shellSection.BadgeTextColor);
		}

		[Fact]
		public void SetBadgeTextEmptyStringForDotBadge()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeText = "";
			Assert.Equal("", shellSection.BadgeText);
		}

		[Fact]
		public void EmptyStringBadgeIsDistinctFromNull()
		{
			var shellSection = new ShellSection();
			shellSection.BadgeText = "";
			Assert.NotNull(shellSection.BadgeText);
			shellSection.BadgeText = null;
			Assert.Null(shellSection.BadgeText);
		}

		[Fact]
		public void EmptyStringBadgeFiresPropertyChanged()
		{
			var shellSection = new ShellSection();
			bool fired = false;
			shellSection.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(BaseShellItem.BadgeText))
					fired = true;
			};
			shellSection.BadgeText = "";
			Assert.True(fired);
		}
	}
}
