using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class FrameTests : PlatformTestFixture
	{
		[Test, Category("Frame")]
		public async Task ReusingFrameRendererDoesCauseOverlapWithPreviousContent()
		{
			await Device.InvokeOnMainThreadAsync(() => {


				ContentPage page = new ContentPage();
				Frame frame1 = new Frame()
				{
					Content = new Label()
					{
						Text = "I am frame 1"
					}
				};

				page.Content = frame1;


				using (var pageRenderer = GetRenderer(page))
				using (var renderer = GetRenderer(frame1))
				{
					var frameRenderer = GetRenderer(frame1);

					Frame frame2 = new Frame()
					{
						Content = new Label()
						{
							Text = "I am frame 2"
						}
					};

					frameRenderer.SetElement(frame2);

					Assert.AreEqual(1, frameRenderer.NativeView.Subviews.Length);
					Assert.AreEqual(1, frameRenderer.NativeView.Subviews[0].Subviews.Length);

					LabelRenderer labelRenderer = null;
					var view = frameRenderer.NativeView;
					Assert.AreEqual(1, view.Subviews.Length);

					while (labelRenderer == null)
					{
						view = view.Subviews[0];
						Assert.AreEqual(1, view.Subviews.Length);
						labelRenderer = view as LabelRenderer;
					}

					var uILabel = (UILabel)labelRenderer.NativeView.Subviews[0];
					Assert.AreEqual("I am frame 2", uILabel.Text);

					Frame frameWithButton = new Frame()
					{
						Content = new Button()
						{
							Text = "I am a Button"
						}
					};

					frameRenderer.SetElement(frameWithButton);

					var uiButton = (UIButton)frameRenderer.NativeView.Subviews[0].Subviews[0].Subviews[0];
					Assert.AreEqual("I am a Button", uiButton.Title(UIControlState.Normal));
				}
			});
		}
	}
}