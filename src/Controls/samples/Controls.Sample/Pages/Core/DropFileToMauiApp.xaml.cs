using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif

#if IOS || MACCATALYST
using UIKit;
using Foundation;
#endif

namespace Maui.Controls.Sample.Pages
{
	public partial class DropFileToMauiApp
	{

		public DropFileToMauiApp()
		{
			InitializeComponent();
		}

		void DropGestureDragLeave(object? sender, DragEventArgs e)
		{

		}

		async void DropGestureDrop(object? sender, DropEventArgs e)
		{
			var filePaths = new List<string>();

#if WINDOWS
			if (e.PlatformArgs is not null && e.PlatformArgs.DragEventArgs.DataView.Contains(StandardDataFormats.StorageItems))
			{
				var items = await e.PlatformArgs.DragEventArgs.DataView.GetStorageItemsAsync();
				if (items.Any())
				{
					foreach (var item in items)
					{
						if (item is StorageFile file)
						{
							filePaths.Add(item.Path);
						}
					}

				}
			}
#elif MACCATALYST

			var session = e.PlatformArgs?.DropSession;
			if (session == null)
			{
				return;
			}
			foreach (UIDragItem item in session.Items)
			{
				var result = await LoadItemAsync(item.ItemProvider, item.ItemProvider.RegisteredTypeIdentifiers.ToList());
				if (result is not null)
				{
					filePaths.Add(result.FileUrl?.Path!);
				}
			}
			foreach (var item in filePaths)
			{
				Debug.WriteLine($"Path: {item}");
			}

			static async Task<LoadInPlaceResult?> LoadItemAsync(NSItemProvider itemProvider, List<string> typeIdentifiers)
			{
				if (typeIdentifiers is null || typeIdentifiers.Count == 0)
				{
					return null;
				}

				var typeIdent = typeIdentifiers.First();

				if (itemProvider.HasItemConformingTo(typeIdent))
				{
					return await itemProvider.LoadInPlaceFileRepresentationAsync(typeIdent);
				}

				typeIdentifiers.Remove(typeIdent);

				return await LoadItemAsync(itemProvider, typeIdentifiers);
			}
#else
			await Task.CompletedTask;
#endif

			lblPath.Text = filePaths.FirstOrDefault();
		}

		void DropGestureDragOver(object? sender, DragEventArgs e)
		{
			Debug.WriteLine($"Dragging {e.Data?.Text}, {e.Data?.Image}");
		}

	}
}