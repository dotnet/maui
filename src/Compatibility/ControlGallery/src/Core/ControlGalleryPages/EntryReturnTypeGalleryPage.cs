using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class EntryReturnTypeGalleryPage : ContentPage
	{
		Picker picker;
		Entry returnTypeEntry;
		Label lblCompleted;
		Entry nextEntry;

		public EntryReturnTypeGalleryPage()
		{
			BackgroundColor = Colors.LightBlue;
			var layout = new StackLayout
			{
				VerticalOptions = LayoutOptions.StartAndExpand
			};
			lblCompleted = new Label
			{
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			picker = new Picker
			{
				HorizontalOptions = LayoutOptions.FillAndExpand
			};
			picker.Items.Add(ReturnType.Done.ToString());
			picker.Items.Add(ReturnType.Go.ToString());
			picker.Items.Add(ReturnType.Next.ToString());
			picker.Items.Add(ReturnType.Search.ToString());
			picker.Items.Add(ReturnType.Send.ToString());
			picker.Items.Add(ReturnType.Default.ToString());

			returnTypeEntry = new Entry
			{
				HorizontalOptions = LayoutOptions.Fill,
				Placeholder = $"Entry with {ReturnType.Go}",
				ReturnCommand = new Command<string>(obj =>
				{
					lblCompleted.Text = "Completed Fired";
				}),
				ReturnCommandParameter = "hello titles",
				AutomationId = "returnTypeEntry"
			};

			returnTypeEntry.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == Entry.ReturnTypeProperty.PropertyName)
				{
					returnTypeEntry.Placeholder = $"Entry with {returnTypeEntry.ReturnType}";
					lblCompleted.Text = null;
				}
			};

			picker.SelectedIndexChanged += (s, e) =>
			{
				if (picker.SelectedItem.ToString() == ReturnType.Done.ToString())
				{
					returnTypeEntry.ReturnType = ReturnType.Done;
				}
				if (picker.SelectedItem.ToString() == ReturnType.Go.ToString())
				{
					returnTypeEntry.ReturnType = ReturnType.Go;
				}
				if (picker.SelectedItem.ToString() == ReturnType.Next.ToString())
				{
					returnTypeEntry.ReturnType = ReturnType.Next;
				}
				if (picker.SelectedItem.ToString() == ReturnType.Search.ToString())
				{
					returnTypeEntry.ReturnType = ReturnType.Search;
				}
				if (picker.SelectedItem.ToString() == ReturnType.Send.ToString())
				{
					returnTypeEntry.ReturnType = ReturnType.Send;
				}
				if (picker.SelectedItem.ToString() == ReturnType.Default.ToString())
				{
					returnTypeEntry.ReturnType = ReturnType.Default;
				}
			};

			nextEntry = new Entry
			{
				Placeholder = "Next Entry to Focus",
				ReturnType = ReturnType.Next,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			layout.Children.Add(picker);
			layout.Children.Add(returnTypeEntry);
			layout.Children.Add(lblCompleted);
			layout.Children.Add(nextEntry);
			picker.SelectedIndex = 0;

			Content = layout;
		}
	}
}
