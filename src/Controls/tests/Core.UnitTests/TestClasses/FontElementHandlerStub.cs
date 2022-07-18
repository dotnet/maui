using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class FontElementHandlerStub : ViewHandler<IView, object>
	{
		static readonly IPropertyMapper<IView, FontElementHandlerStub> Mapper =
			new PropertyMapper<IView, FontElementHandlerStub>
			{
				[nameof(IFontElement.FontAttributes)] = (h, v) => h.MapProperty(nameof(IFontElement.FontAttributes)),
				[nameof(IFontElement.FontAutoScalingEnabled)] = (h, v) => h.MapProperty(nameof(IFontElement.FontAutoScalingEnabled)),
				[nameof(IFontElement.FontFamily)] = (h, v) => h.MapProperty(nameof(IFontElement.FontFamily)),
				[nameof(IFontElement.FontSize)] = (h, v) => h.MapProperty(nameof(IFontElement.FontSize)),
				[nameof(ITextStyle.Font)] = (h, v) => h.MapProperty(nameof(ITextStyle.Font)),
			};

		public FontElementHandlerStub()
			: base(Mapper)
		{
		}

		public List<string> Updates { get; set; } = new List<string>();

		protected override object CreatePlatformView() => new object();

		void MapProperty(string propertyName) =>
			Updates.Add(propertyName);
	}
}