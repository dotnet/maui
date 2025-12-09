namespace Maui.Controls.Sample
{
	public static class GarbageCollectionHelper
	{
		public static void Collect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public static async Task WaitForGC(params WeakReference[] references) => await WaitForGC(5000, references);

		public static async Task WaitForGC(int timeout, params WeakReference[] references)
		{
			bool referencesCollected()
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();

				foreach (var reference in references)
				{
					if (reference.IsAlive)
					{
						return false;
					}
				}

				return true;
			}

			await AssertEventually(referencesCollected, timeout);
		}

		public static async Task AssertEventually(this Func<bool> assertion, int timeout = 1000, int interval = 100, string message = "Assertion timed out")
		{
			do
			{
				if (assertion())
				{
					return;
				}

				await Task.Delay(interval);
				timeout -= interval;

			}
			while (timeout >= 0);

			if (!assertion())
			{
				throw new Exception(message);
			}
		}

		public static Task WaitUntilLoaded(this Image image, int timeout = 1000) =>
					AssertEventually(() => !image.IsLoading, timeout, message: $"Timed out loading image {image}");

		public static void RunMemoryTest(this INavigation navigationPage, Func<VisualElement> elementToTest)
		{
			ContentPage rootPage = new ContentPage { Title = "Page 1" };
			navigationPage.PushAsync(rootPage);
			rootPage.Content = new VerticalStackLayout()
			{
				new Label
				{
					Text = "If you don't see a success label this test has failed"
				}
			};

			rootPage.Loaded += OnPageLoaded;

			async void OnPageLoaded(object sender, EventArgs e)
			{
				var references = new List<WeakReference>();
				rootPage.Loaded -= OnPageLoaded;

				{
					var element = elementToTest();
					var page = new ContentPage
					{
						Content = new VerticalStackLayout { element }
					};

					await navigationPage.PushAsync(page);
					if (element is Image image)
					{
						await image.WaitUntilLoaded();
					}
					else
					{
						await Task.Delay(500); // give the View time to load
					}

					references.Add(new(element));
					references.Add(new(element.Handler));
					references.Add(new(element.Handler.PlatformView));

					await navigationPage.PopAsync();
				}

				try
				{
					rootPage.Content = new VerticalStackLayout()
					{
						new Label
						{
							Text = "Waiting for resources to cleanup",
							AutomationId = "Waiting"

						}
					};

					await WaitForGC(references.ToArray());
					rootPage.Content = new VerticalStackLayout()
					{
						new Label
						{
							Text = "Success, everything has been cleaned up",
							AutomationId = "Success"
						}
					};
				}
				catch
				{
					var stillAlive = references.Where(x => x.IsAlive).Select(x => x.Target).ToList();
					rootPage.Content = new VerticalStackLayout()
					{
						new Label
						{
							Text = "Failed to cleanup: " + string.Join(", ", stillAlive),
							AutomationId = "Failed"
						}
					};
				}
			}
		}
	}
}
