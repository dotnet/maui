using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh1346 : ContentPage
	{
		public static string DefaultText = "Gh1346DefaultText";
		public Gh1346()
		{
			InitializeComponent();
		}

		public Gh1346(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{

			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true), TestCase(false)]
			public void xStaticInStyle(bool useCompiledXaml)
			{
				var layout = new Gh1346(useCompiledXaml);
				var style = layout.Resources["TestIconStyle"] as Style;
				var setter = style.Setters[0];
				Assert.That(setter.Property, Is.EqualTo(Gh1346FontIcon.IconProperty));
				Assert.That(setter.Value, Is.TypeOf<Gh1346FontAwesome>());
				Assert.That(layout.fontIcon.Icon.Icon, Is.EqualTo("\uf2dc"));
			}
		}
	}

	public class Gh1346FontIcon : View
	{
		public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(IGh1346FontIcon), typeof(Gh1346FontIcon));

		public IGh1346FontIcon Icon
		{
			get { return (IGh1346FontIcon)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}
	}

	public interface IGh1346FontIcon
	{
		string Icon { get; }
	}

	public sealed class Gh1346FontAwesome : IGh1346FontIcon
	{
		public string Icon { get; }

		Gh1346FontAwesome(char c)
		{
			Icon = c.ToString();
		}

		//public static implicit operator FontIconOptions(FontAwesome @this)
		//{
		//	return new FontIconOptions(@this);
		//}

		public static implicit operator Gh1346FontIconOptions(Gh1346FontAwesome @this)
		{
			return new Gh1346FontIconOptions(@this);
		}

		public static readonly Gh1346FontAwesome SnowflakeO = new Gh1346FontAwesome('\uf2dc');
	}

	public sealed class Gh1346FontIconOptions
	{
		public IGh1346FontIcon FontIcon { get; set; }

		public Color Color { get; set; } = Color.White;

		public Gh1346FontIconOptions() { }

		public Gh1346FontIconOptions(IGh1346FontIcon icon)
		{
			if (icon == null)
				throw new ArgumentNullException(nameof(icon));
			FontIcon = icon;
		}
	}
}