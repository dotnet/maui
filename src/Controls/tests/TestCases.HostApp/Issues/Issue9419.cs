namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 9419, "Crash when toolbar item removed then page changed", PlatformAffected.Android)]
	public class Issue9419 : TestFlyoutPage
	{
		const string OkResult = "Ok";

		protected override async void Init()
		{
			Flyout = new ContentPage { Title = "Title" };
			Detail = new NavigationPage(new Issue9419Page());

			await Task.Delay(TimeSpan.FromSeconds(3));

			Detail = new NavigationPage(new Issue9419Page());

			await Task.Delay(TimeSpan.FromSeconds(3));

			GarbageCollectionHelper.Collect();

			Detail = new NavigationPage(new ContentPage { Content = new Label { AutomationId = OkResult, Text = OkResult } });
		}

		class ConditionalToolbarItem : ToolbarItem
		{
			public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(ConditionalToolbarItem), true, propertyChanged: IsVisiblePropertyChanged);
			public static readonly BindableProperty IndexProperty = BindableProperty.Create(nameof(Index), typeof(int), typeof(ConditionalToolbarItem), 0);

			public bool IsVisible
			{
				get { return (bool)GetValue(IsVisibleProperty); }
				set { SetValue(IsVisibleProperty, value); }
			}

			public int Index
			{
				get { return (int)GetValue(IndexProperty); }
				set { SetValue(IndexProperty, value); }
			}

			static void IsVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
			{
				var item = (ConditionalToolbarItem)bindable;

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				Device.BeginInvokeOnMainThread(() =>
				{
					IList<ToolbarItem> items = ((Page)item.Parent)?.ToolbarItems;

					if (items == null)
						return;

					bool setValue = item.IsVisible;

					if (setValue && !items.Contains(item))
					{
						int index = items.Count;

						for (int i = 0; i < items.Count; i++)
						{
							if (((ConditionalToolbarItem)items[i]).Index > item.Index)
							{
								index = i;
								break;
							}
						}

						items.Insert(index, item);
					}
					else if (!setValue && items.Contains(item))
					{
						items.Remove(item);
					}
				});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			}
		}

		class Issue9419Page : TestContentPage
		{
			ConditionalToolbarItem _conditionalToolbarItem;

			bool _isVisible;

			protected override void Init()
			{
				_conditionalToolbarItem = new ConditionalToolbarItem { Text = "Test" };

				ToolbarItems.Add(_conditionalToolbarItem);
			}

			protected override async void OnAppearing()
			{
				base.OnAppearing();

				_isVisible = true;

				while (true)
				{
					_conditionalToolbarItem.IsVisible = !_conditionalToolbarItem.IsVisible;

					await Task.Delay(TimeSpan.FromSeconds(1));

					if (!_isVisible)
					{
						_conditionalToolbarItem.IsVisible = !_conditionalToolbarItem.IsVisible;

						return;
					}
				}
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();

				_isVisible = false;
			}
		}
	}
}