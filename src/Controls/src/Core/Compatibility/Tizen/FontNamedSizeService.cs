#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.Common;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Tizen.FontNamedSizeService))]
#pragma warning disable CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	class FontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			if (DeviceInfo.IsTV || DeviceInfo.IsIoT)
			{
				return size switch
				{
					NamedSize.Default => 22,
					NamedSize.Micro => 18,
					NamedSize.Small => 20,
					NamedSize.Medium => 22,
					NamedSize.Large => 26,
					NamedSize.Body => 24,
					NamedSize.Caption => 20,
					NamedSize.Header => 42,
					NamedSize.Subtitle => 24,
					NamedSize.Title => 32,
					_ => throw new ArgumentOutOfRangeException(nameof(size)),
				};
			}
			else
			{
				return size switch
				{
					NamedSize.Default => 14,
					NamedSize.Micro => 10,
					NamedSize.Small => 12,
					NamedSize.Medium => 14,
					NamedSize.Large => 18,
					NamedSize.Body => 16,
					NamedSize.Caption => 12,
					NamedSize.Header => 14,
					NamedSize.Subtitle => 16,
					NamedSize.Title => 24,
					_ => throw new ArgumentOutOfRangeException(nameof(size)),
				};
			}
		}
	}
}