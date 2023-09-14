using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSDragAndDropRequestFullSize : ContentPage
	{
		public iOSDragAndDropRequestFullSize()
		{
			InitializeComponent();

			CollectionView1.ItemsSource = new NameObject[]
			{
				new NameObject ("First Item"),
				new NameObject ("Second Item"),
				new NameObject ("Third Item"),
				new NameObject ("Fourth Item"),
				new NameObject ("Fifth Item"),
				new NameObject ("Sixth Item"),
			};

			// Changing the drag preview is not observed working the same as in iOS
#if MACCATALYST
			fullSizedSwitch.IsEnabled = false;
			drawnImageSwitch.IsEnabled = false;
			dotnetBotImageSwitch.IsEnabled = false;
#endif
		}

		void DragGestureRecognizer_DragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
		{
#if IOS || MACCATALYST
			if (drawnImageSwitch.IsToggled)
			{
				Func<UIKit.UIDragPreview> action = () =>
				{
					var previewParameters = new UIKit.UIDragPreviewParameters();
					var funkyPath = new UIKit.UIBezierPath();
					funkyPath.MoveTo(new CoreGraphics.CGPoint(0, 0));
					funkyPath.AddLineTo(new CoreGraphics.CGPoint(300, 0));
					funkyPath.AddLineTo(new CoreGraphics.CGPoint(225, 150));
					funkyPath.AddLineTo(new CoreGraphics.CGPoint(300, 300));
					funkyPath.AddLineTo(new CoreGraphics.CGPoint(0, 300));
					funkyPath.ClosePath();
					previewParameters.VisiblePath = funkyPath;

					return new UIKit.UIDragPreview(e.PlatformArgs.Sender, previewParameters);
				};

				e.PlatformArgs.SetPreviewProvider(action);
			}

			else if (dotnetBotImageSwitch.IsToggled)
			{
				Func<UIKit.UIDragPreview> action = () =>
				{
					var image = UIKit.UIImage.FromFile("dotnet_bot.png");

					UIKit.UIImageView imageView = new UIKit.UIImageView(image);
					imageView.ContentMode = UIKit.UIViewContentMode.Center;
					imageView.Frame = new CoreGraphics.CGRect(0, 0, 250, 250);

					return new UIKit.UIDragPreview(imageView);
				};

				e.PlatformArgs.SetPreviewProvider(action);
			}


			e.PlatformArgs.SetPrefersFullSizePreviews((interaction, session) => { return fullSizedSwitch.IsToggled; });
#endif
		}

		void Drawn_Switch_Toggled(object sender, ToggledEventArgs e)
		{
			if (e.Value)
				dotnetBotImageSwitch.IsToggled = false;
		}

		void DotnetBot_Switch_Toggled(object sender, ToggledEventArgs e)
		{
			if (e.Value)
				drawnImageSwitch.IsToggled = false;
		}

		void DropGestureRecognizer_DragOver(System.Object sender, Microsoft.Maui.Controls.DragEventArgs e)
		{
#if IOS || MACCATALYST
			if (copySwitch.IsToggled)
				e.PlatformArgs.SetDropProposal(new UIKit.UIDropProposal(UIKit.UIDropOperation.Copy));
			else if (moveSwitch.IsToggled)
				e.PlatformArgs.SetDropProposal(new UIKit.UIDropProposal(UIKit.UIDropOperation.Move));
			else if (forbiddenSwitch.IsToggled)
				e.PlatformArgs.SetDropProposal(new UIKit.UIDropProposal(UIKit.UIDropOperation.Forbidden));
#endif
		}

		void FullSized_Switch_Toggled(object sender, ToggledEventArgs e)
		{
		}

		void Copy_Switch_Toggled(object sender, ToggledEventArgs e)
		{
			if (e.Value)
			{
				moveSwitch.IsToggled = false;
				forbiddenSwitch.IsToggled = false;
			}
		}

		void Move_Switch_Toggled(object sender, ToggledEventArgs e)
		{
			if (e.Value)
			{
				copySwitch.IsToggled = false;
				forbiddenSwitch.IsToggled = false;
			}
		}

		void Forbidden_Switch_Toggled(object sender, ToggledEventArgs e)
		{
			if (e.Value)
			{
				moveSwitch.IsToggled = false;
				copySwitch.IsToggled = false;
			}
		}

		public class NameObject
		{
			public NameObject(string name)
			{
				Name = name;
			}

			public string Name { get; set; }
		}
	}



}
