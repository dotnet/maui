namespace Maui.Controls.Sample.Services;

public class NavigationService
{
	public void NavigateAbsolute(Type pageType)
	{
		var flyoutPage = new AppFlyoutPage();
		
		if (pageType == typeof(Page1))
		{
			flyoutPage.Detail = new NavigationPage(new Page1());
		}
		else if (pageType == typeof(Page2))
		{
			flyoutPage.Detail = new NavigationPage(new Page2());
		}

		var page = (flyoutPage.Detail as NavigationPage)!.CurrentPage;
		
		MemoryTest.Add(flyoutPage);
		MemoryTest.Add(flyoutPage.Detail);
		MemoryTest.Add(page);
		Application.Current!.Windows[0].Page = flyoutPage;
	}

	public void  Navigate(Type pageType)
	{
		var currentPage = Application.Current!.Windows[0].Page;
		
		if (currentPage is FlyoutPage flyoutPage && flyoutPage.Detail is NavigationPage navigationPage)
		{
			Page? targetPage = null;
			
			if (pageType == typeof(Page1))
			{
				targetPage = new Page1();
			}
			else if (pageType == typeof(Page2))
			{
				targetPage = new Page2();
			}
			
			if (targetPage != null)
			{
				flyoutPage.Detail = new NavigationPage(targetPage);
				flyoutPage.IsPresented = false;
			}
			
			
			MemoryTest.Add(flyoutPage);
			MemoryTest.Add(flyoutPage.Detail);
			MemoryTest.Add(targetPage!);
		}
	}
}
static class MemoryTest
{
	static readonly List<WeakReference<object>> weakReferences = [];

	public static int Count => weakReferences.Count;

	public static void Add(object obj)
	{
		weakReferences.Add(new(obj));
	}

	public static async Task IsAliveAsync()
	{
		RunGC();
		for (var i = 0; i < weakReferences.Count; i++)
		{
			var item = weakReferences[i];

			if (item.TryGetTarget(out var target))
			{
				var type = target.GetType();
				var name = type.FullName;

#pragma warning disable CS0618 // Type or member is obsolete
				await App.Current!.MainPage!.DisplayAlert("Memory leak!", $"The {name} object of {type} Type is still alive", "Ok");
#pragma warning restore CS0618 // Type or member is obsolete
			}
			else
			{
				weakReferences.Remove(item);
			}
			RunGC();
		}


#pragma warning disable CS0618 // Type or member is obsolete
				await App.Current!.MainPage!.DisplayAlert("Memory leak!", $"The are {weakReferences.Count} objects still alive", "Ok");
#pragma warning restore CS0618 // Type or member is obsolete
	}

	static void RunGC()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}
}