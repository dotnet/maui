using System;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class BoxView1501 : BoxView
	{
		public bool Fired { get; set; }

		public void OnBoxViewTapped(object sender, EventArgs e)
		{
			Fired = true;
		}
	}

	[TestFixture]
	public class Issue1501
	{
		[Test]
		public void ConnectEventsInGestureRecognizers()
		{
			var xaml = @"
				<BoxView 
					xmlns=""http://xamarin.com/schemas/2014/forms""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Xamarin.Forms.Xaml.UnitTests.BoxView1501"" >
				    <BoxView.GestureRecognizers>
				      <TapGestureRecognizer Tapped=""OnBoxViewTapped"" />
				    </BoxView.GestureRecognizers>
				</BoxView>";

			BoxView1501 layout = null;
			Assert.DoesNotThrow(() => { layout = new BoxView1501().LoadFromXaml(xaml); });

			Assert.False(layout.Fired);
			var tgr = layout.GestureRecognizers[0] as TapGestureRecognizer;
			tgr.SendTapped(layout);
			Assert.True(layout.Fired);
		}
	}
}