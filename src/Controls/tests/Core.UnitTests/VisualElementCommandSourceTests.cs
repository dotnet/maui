using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public abstract class VisualElementCommandSourceTests<T> : CommandSourceTests<T>
		where T : VisualElement
	{
		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void EnabledLayoutDoesNotAffectChildIsEnabled(Type TLayout)
		{
			var layout = CreateLayout(TLayout);

			var child = CreateSource();
			layout.Add(child);

			Assert.True(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void DisabledLayoutDisablesChild(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			layout.Add(child);

			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void DisabledLayoutDisablesChildWithCommand(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			child.SetValue(CommandProperty, new Command(() => { }, () => true));
			layout.Add(child);

			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void DisabledLayoutDisablesNestedChild(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			var childLayout = CreateLayout(TLayout);
			childLayout.Add(child);

			layout.Add(childLayout);

			Assert.False(childLayout.IsEnabled);
			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void EnablingChildDoesNotEnableChildOfDisabledLayout(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			child.IsEnabled = false;

			layout.Add(child);

			Assert.False(layout.IsEnabled);
			Assert.False(child.IsEnabled);

			child.IsEnabled = true;

			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void EnablingChildWithCommandDoesNotEnableChildOfDisabledLayout(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			var isEnabled = false;
			var command = new Command(() => { }, () => isEnabled);
			child.SetValue(CommandProperty, command);

			layout.Add(child);

			Assert.False(layout.IsEnabled);
			Assert.False(child.IsEnabled);

			isEnabled = true;
			command.ChangeCanExecute();

			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void DisablingLayoutDisablesChild(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			var child = CreateSource();
			layout.Add(child);

			layout.IsEnabled = false;

			Assert.False(layout.IsEnabled);
			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void DisablingLayoutDisablesChildWithCommand(Type TLayout)
		{
			var layout = CreateLayout(TLayout);

			var child = CreateSource();
			var command = new Command(() => { }, () => true);
			child.SetValue(CommandProperty, command);

			layout.Add(child);

			layout.IsEnabled = false;

			Assert.False(layout.IsEnabled);
			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void DisablingLayoutDisablesChildWithBinding(Type TLayout)
		{
			var layout = CreateLayout(TLayout);

			var child = CreateSource();
			var context = new { Enabled = true };
			child.BindingContext = context;
			child.SetBinding(IsEnabledProperty, "Enabled");

			layout.Add(child);

			layout.IsEnabled = false;

			Assert.False(layout.IsEnabled);
			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void EnablingLayoutWithDisabledChildWithCommandDoesNotEnableChild(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			var command = new Command(() => { }, () => false);
			child.SetValue(CommandProperty, command);

			layout.Add(child);

			layout.IsEnabled = true;

			Assert.True(layout.IsEnabled);
			Assert.False(child.IsEnabled);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void EnablingLayoutWithDisabledChildWithBindingDoesNotEnableChild(Type TLayout)
		{
			var layout = CreateLayout(TLayout);
			layout.IsEnabled = false;

			var child = CreateSource();
			var context = new { Enabled = false };
			child.BindingContext = context;
			child.SetBinding(IsEnabledProperty, "Enabled");

			layout.Add(child);

			layout.IsEnabled = true;

			Assert.True(layout.IsEnabled);
			Assert.False(child.IsEnabled);
		}

		static Layout CreateLayout(Type TLayout)
		{
			var layout = (Layout)Activator.CreateInstance(TLayout)!;
			return layout;
		}
	}
}
