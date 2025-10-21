#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="Type[@FullName='Microsoft.Maui.Controls.DropGestureRecognizer']/Docs/*" />
	public class DropGestureRecognizer : GestureRecognizer
	{
		/// <summary>Bindable property for <see cref="AllowDrop"/>.</summary>
		public static readonly BindableProperty AllowDropProperty = BindableProperty.Create(nameof(AllowDrop), typeof(bool), typeof(DropGestureRecognizer), true);

		/// <summary>Bindable property for <see cref="DragOverCommand"/>.</summary>
		public static readonly BindableProperty DragOverCommandProperty = BindableProperty.Create(nameof(DragOverCommand), typeof(ICommand), typeof(DropGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragOverCommandParameter"/>.</summary>
		public static readonly BindableProperty DragOverCommandParameterProperty = BindableProperty.Create(nameof(DragOverCommandParameter), typeof(object), typeof(DropGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragLeaveCommand"/>.</summary>
		public static readonly BindableProperty DragLeaveCommandProperty = BindableProperty.Create(nameof(DragLeaveCommand), typeof(ICommand), typeof(DropGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DragLeaveCommandParameter"/>.</summary>
		public static readonly BindableProperty DragLeaveCommandParameterProperty = BindableProperty.Create(nameof(DragLeaveCommandParameter), typeof(object), typeof(DropGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DropCommand"/>.</summary>
		public static readonly BindableProperty DropCommandProperty = BindableProperty.Create(nameof(DropCommand), typeof(ICommand), typeof(DragGestureRecognizer), null);

		/// <summary>Bindable property for <see cref="DropCommandParameter"/>.</summary>
		public static readonly BindableProperty DropCommandParameterProperty = BindableProperty.Create(nameof(DropCommandParameter), typeof(object), typeof(DropGestureRecognizer), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DropGestureRecognizer()
		{
		}

		public event EventHandler<DragEventArgs> DragLeave;
		public event EventHandler<DragEventArgs> DragOver;
		public event EventHandler<DropEventArgs> Drop;

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='AllowDrop']/Docs/*" />
		public bool AllowDrop
		{
			get { return (bool)GetValue(AllowDropProperty); }
			set { SetValue(AllowDropProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='DragOverCommand']/Docs/*" />
		public ICommand DragOverCommand
		{
			get { return (ICommand)GetValue(DragOverCommandProperty); }
			set { SetValue(DragOverCommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='DragOverCommandParameter']/Docs/*" />
		public object DragOverCommandParameter
		{
			get { return (object)GetValue(DragOverCommandParameterProperty); }
			set { SetValue(DragOverCommandParameterProperty, value); }
		}
		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='DragLeaveCommand']/Docs/*" />
		public ICommand DragLeaveCommand
		{
			get { return (ICommand)GetValue(DragLeaveCommandProperty); }
			set { SetValue(DragLeaveCommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='DragLeaveCommandParameter']/Docs/*" />
		public object DragLeaveCommandParameter
		{
			get { return (object)GetValue(DragLeaveCommandParameterProperty); }
			set { SetValue(DragLeaveCommandParameterProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='DropCommand']/Docs/*" />
		public ICommand DropCommand
		{
			get { return (ICommand)GetValue(DropCommandProperty); }
			set { SetValue(DropCommandProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropGestureRecognizer.xml" path="//Member[@MemberName='DropCommandParameter']/Docs/*" />
		public object DropCommandParameter
		{
			get { return (object)GetValue(DropCommandParameterProperty); }
			set { SetValue(DropCommandParameterProperty, value); }
		}

		/// <param name="args">The event arguments.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendDragOver(DragEventArgs args)
		{
			DragOverCommand?.Execute(DragOverCommandParameter);
			DragOver?.Invoke(this, args);
		}

		internal void SendDragLeave(DragEventArgs args)
		{
			DragLeaveCommand?.Execute(DragLeaveCommandParameter);
			DragLeave?.Invoke(this, args);
		}

		internal async Task SendDrop(DropEventArgs args)
		{
			if (!AllowDrop)
				return;

			DropCommand?.Execute(DropCommandParameter);
			Drop?.Invoke(this, args);

			if (!args.Handled)
			{
				var dataView = args.Data;
				var internalProperties = dataView.PropertiesInternal;
				IView dragSource = null;
				ImageSource sourceTarget = await dataView.GetImageAsync();
				string text = await dataView.GetTextAsync();

				if (internalProperties.TryGetValue("DragSource", out var property))
				{
					dragSource = (IView)property;
					if (sourceTarget == null && dragSource is IImageElement imageElement)
						sourceTarget = imageElement.Source;

					if (String.IsNullOrWhiteSpace(text))
					{
						text = dragSource.GetStringValue();
					}
				}

				if (Parent is IImageElement && sourceTarget == null)
					sourceTarget = text;

				if (Parent is Image image)
					image.Source = sourceTarget;
				else if (Parent is ImageButton ib)
					ib.Source = sourceTarget;
				else if (Parent is Button b)
					b.ImageSource = sourceTarget;

				Parent?.TrySetValue(text);
			}
		}
	}
}