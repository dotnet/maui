using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	public partial class CollectionViewMemoryBenchmark : ContentPage
	{
		int _id;

		public CollectionViewMemoryBenchmark()
		{
			InitializeComponent();
		}

		async void Button_Clicked_1(object sender, EventArgs e)
		{
			CollectionView1.ItemsSource = GetNewData();
			await UpdateMemoryAsync(1);
		}

		async void Button_Clicked_2(object sender, EventArgs e)
		{
			CollectionView2.ItemsSource = GetNewData();
			await UpdateMemoryAsync(2);
		}

		async void Button_Clicked_3(object sender, EventArgs e)
		{
			CollectionView3.ItemsSource = GetNewData();
			await UpdateMemoryAsync(3);
		}

		List<DataModel> GetNewData()
		{
			List<DataModel> data = new();

			for (int i = 0; i < 5; i++)
			{
				_id += 1;
				data.Add(new DataModel() { ID = _id, Name = $"Name{_id}", Data1 = "data1", Data2 = "data2", Data3 = "data3" });
			}

			return data;
		}

		async Task UpdateMemoryAsync(int testid)
		{
			await Task.Delay(500);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			LabelMemory.Text = $"Memory {GC.GetTotalMemory(true):N0} after test {testid}";
			LabelAlive.Text = $"Objects alive {References.GetAliveCount()}";

			References.Print();
		}
	}

	public class DataModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Data1 { get; set; }
		public string Data2 { get; set; }
		public string Data3 { get; set; }
	}

	public static class References
	{
		static readonly List<WeakReference> Refs = new();

		public static readonly BindableProperty IsWatchedProperty =
			BindableProperty.CreateAttached("IsWatched", typeof(bool), typeof(References), false, propertyChanged: OnIsWatchedChanged);

		public static bool GetIsWatched(BindableObject obj)
		{
			return (bool)obj.GetValue(IsWatchedProperty);
		}

		public static void SetIsWatched(BindableObject obj, bool value)
		{
			obj.SetValue(IsWatchedProperty, value);
		}

		static void OnIsWatchedChanged(BindableObject bindable, object oldValue, object newValue)
		{
			AddRef(bindable);
		}

		public static void AddRef(object p)
		{
			Refs.Add(new WeakReference(p));
		}

		public static void Print()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Refs.RemoveAll(a => !a.IsAlive);
			foreach (WeakReference weakReference in Refs)
				Debug.WriteLine("IsAlive: " + weakReference.Target?.GetType().Name);

			Debug.WriteLine($"Total Refs still alive: {Refs.Count}");
			Debug.WriteLine("---------------");
		}

		public static int GetAliveCount()
		{
			return Refs.Count(weakReference => weakReference.IsAlive);
		}
	}
}