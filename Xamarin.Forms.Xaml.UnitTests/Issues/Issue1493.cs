using System;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1493
	{
		[Test]
		//mostly happens in european cultures
		[SetCulture ("fr-FR")]
		public void CultureInvariantNumberParsing ()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						<View 
							xmlns=""http://xamarin.com/schemas/2014/forms"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							RelativeLayout.HeightConstraint=""{ConstraintExpression Type=RelativeToParent, Property=Height, Factor=0.25}""
							RelativeLayout.WidthConstraint=""{ConstraintExpression Type=RelativeToParent, Property=Width, Factor=0.6}""/>";
			View view = new View ();
			view.LoadFromXaml (xaml);
			Assert.DoesNotThrow (() => view.LoadFromXaml (xaml));
		}
	}
}

