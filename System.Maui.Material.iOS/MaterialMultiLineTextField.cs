using System;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using MMultilineTextField = MaterialComponents.MultilineTextField;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;
using System.Maui.Internals;

namespace System.Maui.Material.iOS
{
	public class MaterialMultilineTextField : MMultilineTextField, IMaterialTextField
	{
		CGSize _contentSize;

		public MaterialMultilineTextField(IMaterialEntryRenderer element, IFontElement fontElement)
		{
			ContainerScheme = new ContainerScheme();
			MaterialTextManager.Init(element, this, fontElement);
		}

		public ContainerScheme ContainerScheme { get; }
		public SemanticColorScheme ColorScheme { get; set; }
		public TypographyScheme TypographyScheme { get; set; }
		public MTextInputControllerBase ActiveTextInputController { get; set; }
		public ITextInput TextInput => this;

		public override CGRect Frame
		{
			get => base.Frame;
			set
			{
				base.Frame = value;
				UpdateIfTextViewShouldStopExpanding();
			}
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			bool expandTurnedBackOn = UpdateIfTextViewShouldCollapse();
			var result = base.SizeThatFits(size);

			if (nfloat.IsInfinity(result.Width))
				result = SystemLayoutSizeFittingSize(result, (float)UILayoutPriority.FittingSizeLevel, (float)UILayoutPriority.DefaultHigh);

			if (ExpandsOnOverflow)
				_contentSize = result;
			else
				_contentSize = TextView.ContentSize;

			if (!expandTurnedBackOn)
				UpdateIfTextViewShouldStopExpanding();

			return result;
		}

		bool UpdateIfTextViewShouldCollapse()
		{
			if (!ExpandsOnOverflow &&
				!AutoSizeWithChanges &&
				!ShouldRestrainSize())
			{
				ExpandsOnOverflow = true;
				return true;
			}

			return false;
		}

		bool ShouldRestrainSize()
		{
			if (TextView?.Font == null)
				return false;

			return (((NumberOfLines + 1) * TextView.Font.LineHeight) > Frame.Height);
		}

		void UpdateIfTextViewShouldStopExpanding()
		{
			if (!UpdateIfTextViewShouldCollapse() &&
				!AutoSizeWithChanges &&
				ExpandsOnOverflow &&
				ShouldRestrainSize()) 
			{
				ExpandsOnOverflow = false;
			}
		}

		int NumberOfLines
		{
			get
			{
				if (TextView?.ContentSize == null || TextView.Font == null || TextView.Font.LineHeight == 0)
					return 0;

				return (int)(_contentSize.Height / TextView.Font.LineHeight);
			}
		}

		internal bool AutoSizeWithChanges { get; set; } = false;
		internal void ApplyTypographyScheme(IFontElement fontElement) => MaterialTextManager.ApplyTypographyScheme(this, fontElement);
		internal void ApplyTheme(IMaterialEntryRenderer element) => MaterialTextManager.ApplyTheme(this, element);
		internal void UpdatePlaceholder(IMaterialEntryRenderer element) => MaterialTextManager.UpdatePlaceholder(this, element);
		internal void UpdateTextColor(IMaterialEntryRenderer element) => MaterialTextManager.UpdateTextColor(this, element);
	}
}
