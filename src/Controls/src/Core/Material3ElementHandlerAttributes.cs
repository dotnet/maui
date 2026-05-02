using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls;

// Keep these manual attributes small; https://github.com/dotnet/maui/issues/35285 tracks source-generating them.
#if ANDROID
public partial class ActivityIndicator
{
	internal sealed class ActivityIndicatorHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(ActivityIndicatorHandler2);

			return typeof(ActivityIndicatorHandler);
		}
	}
}

public partial class DatePicker
{
	internal sealed class DatePickerHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(DatePickerHandler2);

			return typeof(DatePickerHandler);
		}
	}
}

public partial class Editor
{
	internal sealed class EditorHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(EditorHandler2);

			return typeof(EditorHandler);
		}
	}
}

public partial class Entry
{
	internal sealed class EntryHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(EntryHandler2);

			return typeof(EntryHandler);
		}
	}
}

public partial class Image
{
	internal sealed class ImageHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(ImageHandler2);

			return typeof(ImageHandler);
		}
	}
}

public partial class Label
{
	internal sealed class LabelHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(LabelHandler2);

			return typeof(LabelHandler);
		}
	}
}

public partial class Picker
{
	internal sealed class PickerHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(PickerHandler2);

			return typeof(PickerHandler);
		}
	}
}

public partial class ProgressBar
{
	internal sealed class ProgressBarHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(ProgressBarHandler2);

			return typeof(ProgressBarHandler);
		}
	}
}

public partial class RadioButton
{
	internal sealed class RadioButtonHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(RadioButtonHandler2);

			return typeof(RadioButtonHandler);
		}
	}
}

public partial class SearchBar
{
	internal sealed class SearchBarHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(SearchBarHandler2);

			return typeof(SearchBarHandler);
		}
	}
}

public partial class Slider
{
	internal sealed class SliderHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(SliderHandler2);

			return typeof(SliderHandler);
		}
	}
}

public partial class Switch
{
	internal sealed class SwitchHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(SwitchHandler2);

			return typeof(SwitchHandler);
		}
	}
}

public partial class TimePicker
{
	internal sealed class TimePickerHandlerAttribute : ElementHandlerAttribute
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public override Type GetHandlerType()
		{
			if (RuntimeFeature.IsMaterial3Enabled)
				return typeof(TimePickerHandler2);

			return typeof(TimePickerHandler);
		}
	}
}
#endif
