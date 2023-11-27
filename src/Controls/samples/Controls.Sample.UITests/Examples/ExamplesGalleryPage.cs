using System;
using ExampleFramework.Tooling;
using Microsoft.Maui.Controls.Internals;
using VisualTestUtils;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class ExamplesGalleryPage : ContentViewGalleryPage
	{
		public ExamplesGalleryPage(UIComponent uiComponent)
		{
			foreach (UIExample example in uiComponent.Examples)
			{
				Add(new ExampleView(example));
			}
		}

		public ExamplesGalleryPage(UIExample example)
		{
			Add(new ExampleView(example));
			SetSelection(0);
		}

		/// <summary>
		/// Get the snapshot for the currently selected example. If no example is currently
		/// selected, this is considered an error and an exception is thrown.
		/// </summary>
		/// <returns>PNG snapshot for current examaple</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public async Task<ImageSnapshot> GetExampleSnapshotAsync()
		{
			ContentView contentView = GetSelectedContentView();
			if (contentView == null)
			{
				throw new InvalidOperationException("No example is currently selected");
			}

			byte[] data = await Microsoft.Maui.VisualDiagnostics.CaptureAsPngAsync(contentView);
			if (data == null)
			{
				throw new InvalidOperationException("VisualDiagnostics.CaptureAsPngAsync failed");
			}

			return new ImageSnapshot(data, ImageSnapshotFormat.PNG);
		}
	}
}