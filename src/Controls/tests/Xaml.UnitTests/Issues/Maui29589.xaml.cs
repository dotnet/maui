using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui29589Colors : ResourceDictionary
{
	public Maui29589Colors() => InitializeComponent();
}

public partial class Maui29589Overrides : ResourceDictionary
{
	public Maui29589Overrides() => InitializeComponent();
}

public partial class Maui29589Styles : ResourceDictionary
{
	public Maui29589Styles() => InitializeComponent();
}

public partial class Maui29589Library : ResourceDictionary
{
	public Maui29589Library() => InitializeComponent();
}

public partial class Maui29589StylesLocal : ResourceDictionary
{
	public Maui29589StylesLocal() => InitializeComponent();
}

public partial class Maui29589LibraryXc : ResourceDictionary
{
	public Maui29589LibraryXc() => InitializeComponent();
}

public partial class Maui29589LibrarySg : ResourceDictionary
{
	public Maui29589LibrarySg() => InitializeComponent();
}

public partial class Maui29589Local : ContentPage
{
	public Maui29589Local() => InitializeComponent();
}

public partial class Maui29589Content : ContentPage
{
	public Maui29589Content() => InitializeComponent();
}

public partial class Maui29589Override : ContentPage
{
	public Maui29589Override() => InitializeComponent();
}

public partial class Maui29589Nested : ContentPage
{
	public Maui29589Nested() => InitializeComponent();
}

public partial class Maui29589Mixed : ContentPage
{
	public Maui29589Mixed() => InitializeComponent();
}

public partial class Maui29589Reversed : ContentPage
{
	public Maui29589Reversed() => InitializeComponent();
}

public partial class Maui29589App : Application
{
	public Maui29589App() => InitializeComponent();
}

public partial class Maui29589 : ContentPage
{
	public Maui29589() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		static readonly Color Primary = Color.FromArgb("#010203");
		static readonly Color Secondary = Color.FromArgb("#0A0B0C");

		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void TypedMergedDictionaryResolvesStaticResourceFromPrecedingSibling(XamlInflator inflator)
		{
			var page = new Maui29589(inflator);
			Assert.Equal(Primary, page.Button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void DynamicResourceInTypedMergedDictionaryStillResolves(XamlInflator inflator)
		{
			var page = new Maui29589(inflator);
			Assert.Equal(Secondary, page.Label.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void LastPrecedingMergedDictionaryWins(XamlInflator inflator)
		{
			var page = new Maui29589Override(inflator);
			Assert.Equal(Color.FromArgb("#040506"), page.Button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void NestedTypedMergedDictionaries(XamlInflator inflator)
		{
			var page = new Maui29589Nested(inflator);
			Assert.Equal(Primary, page.Button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void TypedMergedDictionaryAfterSourceMergedDictionary(XamlInflator inflator)
		{
			var page = new Maui29589Mixed(inflator);
			Assert.Equal(Primary, page.Button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void ApplicationLevelTypedMergedDictionaries(XamlInflator inflator)
		{
			var app = new Maui29589App(inflator);
			var style = Assert.IsType<Style>(app.Resources[typeof(Button).FullName]);
			Assert.Equal(Primary, Assert.Single(style.Setters).Value);
		}

		[Fact]
		internal void CompiledNestedTypedMergedDictionaries()
		{
			foreach (var library in new ResourceDictionary[] { new Maui29589LibraryXc(), new Maui29589LibrarySg() })
			{
				var style = Assert.IsType<Style>(library[typeof(Button).FullName]);
				Assert.Equal(Primary, Assert.Single(style.Setters).Value);
			}
		}

		[Theory]
		[XamlInflatorData]
		internal void LocalResourceWinsOverMergeTarget(XamlInflator inflator)
		{
			var page = new Maui29589Local(inflator);
			Assert.Equal(Color.FromArgb("#070809"), page.Button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void MergeTargetWinsOverApplicationResources(XamlInflator inflator)
		{
			Application.Current.Resources["Maui29589Primary"] = Colors.Red;
			var page = new Maui29589(inflator);
			Assert.Equal(Primary, page.Button.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void TypedMergedDictionaryWithContent(XamlInflator inflator)
		{
			var page = new Maui29589Content(inflator);
			Assert.Equal(Primary, page.Button.BackgroundColor);
			Assert.Equal(Color.FromArgb("#0D0E0F"), page.Resources["Maui29589Extra"]);
		}

		[Theory]
		[XamlInflatorData]
		internal void StaticResourceDoesNotResolveFromFollowingSibling(XamlInflator inflator)
		{
			var ex = Assert.Throws<XamlParseException>(() => new Maui29589Reversed(inflator));
			Assert.Contains("Maui29589Primary", ex.Message, StringComparison.Ordinal);
		}

		[Theory]
		[XamlInflatorData]
		internal void MergeScopeDoesNotLeakAfterLoading(XamlInflator inflator)
		{
			_ = new Maui29589(inflator);
			Assert.Throws<XamlParseException>(() => new Maui29589Styles());
		}

		[Theory]
		[XamlInflatorData]
		internal void MergeScopeDoesNotLeakAfterFailure(XamlInflator inflator)
		{
			Assert.Throws<XamlParseException>(() => new Maui29589Reversed(inflator));
			Assert.Throws<XamlParseException>(() => new Maui29589Styles());
		}
	}
}
