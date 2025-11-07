using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class BoxView1501 : BoxView
	{
		public bool Fired { get; set; }

		public void OnBoxViewTapped(object sender, EventArgs e)
		{
			Fired = true;
		}
	}


	public class Issue1501
	{
		[Fact]
		public void ConnectEventsInGestureRecognizers()
		{
			var xaml = @"
				<BoxView 
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.BoxView1501"" >
				    <BoxView.GestureRecognizers>
				      <TapGestureRecognizer Tapped=""OnBoxViewTapped"" />
				    </BoxView.GestureRecognizers>
				</BoxView>";

			BoxView1501 layout = new BoxView1501().LoadFromXaml(xaml);
			;

			Assert.False(layout.Fired);
			var tgr = layout.GestureRecognizers[0] as TapGestureRecognizer;
			tgr.SendTapped(layout);
			Assert.True(layout.Fired);
		}
	}
}