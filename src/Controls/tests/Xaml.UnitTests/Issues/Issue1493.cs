using System;
using System.Globalization;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1493
	{
		CultureInfo _defaultCulture;
		[SetUp]
		public virtual void Setup()
		{
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public virtual void TearDown()
		{
			Device.PlatformServices = null;
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
		}

		[TestCase("en-US"), TestCase("tr-TR"), TestCase("fr-FR")]
		//mostly happens in european cultures
		public void CultureInvariantNumberParsing(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						<View 
							xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							RelativeLayout.HeightConstraint=""{ConstraintExpression Type=RelativeToParent, Property=Height, Factor=0.25}""
							RelativeLayout.WidthConstraint=""{ConstraintExpression Type=RelativeToParent, Property=Width, Factor=0.6}""/>";
			View view = new View();
			view.LoadFromXaml(xaml);
			Assert.DoesNotThrow(() => view.LoadFromXaml(xaml));
		}
	}
}