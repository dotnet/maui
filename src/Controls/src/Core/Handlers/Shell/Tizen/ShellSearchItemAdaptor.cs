using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Internals;
using GColor = Microsoft.Maui.Graphics.Color;
using GColors = Microsoft.Maui.Graphics.Colors;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellSearchItemAdaptor : ItemTemplateAdaptor
	{
		public ShellSearchItemAdaptor(Element element, IEnumerable items, DataTemplate? template) : base(element, items, template ?? new DefaultItemTemplate()) { }

		protected override bool IsSelectable => true;
	}

	class DefaultItemTemplate : DataTemplate
	{
		public static readonly GColor DefaultBackgroundColor = new GColor(0.9f, 0.9f, 0.9f, 1);

		public DefaultItemTemplate() : base(CreateView) { }

		static View CreateView()
		{
			var label = new Label
			{
				TextColor = GColors.Black,
			};
			label.SetBinding(Label.TextProperty, static (object source) => source);

			return new Controls.StackLayout
			{
				BackgroundColor = DefaultBackgroundColor,
				Padding = 5,
				Children =
				{
					label
				}
			};
		}
	}
}
