using System.Maui.Internals;

namespace System.Maui.Pages
{
	public class ListDataPageControl : ListView
	{
		public ListDataPageControl()
		{
			SetBinding(ItemTemplateProperty, new Binding(DataPage.DefaultItemTemplateProperty.PropertyName, source: RelativeBindingSource.TemplatedParent));
			SetBinding(SelectedItemProperty, new Binding(ListDataPage.SelectedItemProperty.PropertyName, BindingMode.TwoWay, source: RelativeBindingSource.TemplatedParent));
			SetBinding(ItemsSourceProperty, new Binding(DataPage.DataProperty.PropertyName, source: RelativeBindingSource.TemplatedParent));
		}
	}

	public class ListDataPage : DataPage
	{
		public static readonly BindableProperty DetailTemplateProperty = BindableProperty.Create(nameof(DetailTemplate), typeof(DataTemplate), typeof(ListDataPage), null);

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(ListDataPage), null, BindingMode.TwoWay,
			propertyChanged: OnSelectedItemChanged);

		public DataTemplate DetailTemplate
		{
			get { return (DataTemplate)GetValue(DetailTemplateProperty); }
			set { SetValue(DetailTemplateProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		static async void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (ListDataPage)bindable;
			DataTemplate template = self.DetailTemplate;
			if (newValue == null)
				return;

			Page detailPage;
			if (template == null)
			{
				detailPage = new DataPage();
			}
			else
			{
				detailPage = (Page)template.CreateContent(newValue, self);
			}

			detailPage.BindingContext = newValue;
			await self.Navigation.PushAsync(detailPage);

			self.SelectedItem = null;
		}
	}
}