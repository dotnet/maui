using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsDragAndDropCustomization : ContentPage
	{
		public WindowsDragAndDropCustomization()
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
		}

		void DragGestureRecognizer_DragStarting(object? sender, DragStartingEventArgs e)
		{
#if WINDOWS
			if (MoveSwitch.IsToggled)
			{
				e.PlatformArgs!.DragStartingEventArgs.AllowedOperations =
					global::Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy |
					global::Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
				e.PlatformArgs.Handled = true;
			}
#endif
		}

		void DropGestureRecognizer_DragOver(System.Object? sender, Microsoft.Maui.Controls.DragEventArgs e)
		{
			e.AcceptedOperation = MoveSwitch.IsToggled ? DataPackageOperation.Move : DataPackageOperation.Copy;
#if WINDOWS
			var dragUI = e.PlatformArgs!.DragEventArgs.DragUIOverride;
			dragUI.IsCaptionVisible = ShowCaptionSwitch.IsToggled;
			dragUI.IsGlyphVisible = ShowGlyphSwitch.IsToggled;
			dragUI.IsContentVisible = ShowContentSwitch.IsToggled;

			dragUI.Caption = string.IsNullOrEmpty (CustomCaptionEntry.Text) ? e.AcceptedOperation.ToString() : CustomCaptionEntry.Text;
#endif
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
