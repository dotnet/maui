using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Pages
{
	public class ListDataPageControl : ListView
	{
		public ListDataPageControl()
		{
			SetBinding(ItemTemplateProperty, new TemplateBinding(DataPage.DefaultItemTemplateProperty.PropertyName));
			SetBinding(SelectedItemProperty, new TemplateBinding(ListDataPage.SelectedItemProperty.PropertyName, BindingMode.TwoWay));
			SetBinding(ItemsSourceProperty, new TemplateBinding(DataPage.DataProperty.PropertyName));
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