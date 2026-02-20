#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Recognizes drop gestures and handles data transfer during drag and drop operations.
	/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="DropGestureRecognizer"/> class.
		/// </summary>
		public DropGestureRecognizer()
		{
		}

		/// <summary>
		/// Occurs when a dragged element leaves the drop target.
		/// </summary>
		public event EventHandler<DragEventArgs> DragLeave;

		/// <summary>
		/// Occurs when a dragged element is over the drop target.
		/// </summary>
		public event EventHandler<DragEventArgs> DragOver;

		/// <summary>
		/// Occurs when an element is dropped on the drop target.
		/// </summary>
		public event EventHandler<DropEventArgs> Drop;

		/// <summary>
		/// Gets or sets a value indicating whether the element can accept dropped data. This is a bindable property.
		/// </summary>
		public bool AllowDrop
		{
			get { return (bool)GetValue(AllowDropProperty); }
			set { SetValue(AllowDropProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to invoke when a dragged element is over the drop target. This is a bindable property.
		/// </summary>
		public ICommand DragOverCommand
		{
			get { return (ICommand)GetValue(DragOverCommandProperty); }
			set { SetValue(DragOverCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to <see cref="DragOverCommand"/>. This is a bindable property.
		/// </summary>
		public object DragOverCommandParameter
		{
			get { return (object)GetValue(DragOverCommandParameterProperty); }
			set { SetValue(DragOverCommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to invoke when a dragged element leaves the drop target. This is a bindable property.
		/// </summary>
		public ICommand DragLeaveCommand
		{
			get { return (ICommand)GetValue(DragLeaveCommandProperty); }
			set { SetValue(DragLeaveCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to <see cref="DragLeaveCommand"/>. This is a bindable property.
		/// </summary>
		public object DragLeaveCommandParameter
		{
			get { return (object)GetValue(DragLeaveCommandParameterProperty); }
			set { SetValue(DragLeaveCommandParameterProperty, value); }
		}

		/// <summary>
		/// Gets or sets the command to invoke when an element is dropped. This is a bindable property.
		/// </summary>
		public ICommand DropCommand
		{
			get { return (ICommand)GetValue(DropCommandProperty); }
			set { SetValue(DropCommandProperty, value); }
		}

		/// <summary>
		/// Gets or sets the parameter to pass to <see cref="DropCommand"/>. This is a bindable property.
		/// </summary>
		public object DropCommandParameter
		{
			get { return (object)GetValue(DropCommandParameterProperty); }
			set { SetValue(DropCommandParameterProperty, value); }
		}

		/// <summary>
		/// Raises the <see cref="DragOver"/> event.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendDragOver(DragEventArgs args)
		{
			DragOverCommand?.Execute(DragOverCommandParameter);
			DragOver?.Invoke(Parent ?? this, args);
		}

		internal void SendDragLeave(DragEventArgs args)
		{
			DragLeaveCommand?.Execute(DragLeaveCommandParameter);
			DragLeave?.Invoke(Parent ?? this, args);
		}

		internal async Task SendDrop(DropEventArgs args)
		{
			if (!AllowDrop)
				return;

			DropCommand?.Execute(DropCommandParameter);
			Drop?.Invoke(Parent ?? this, args);

			if (!args.Handled)
			{
				var dataView = args.Data;
				var internalProperties = dataView.PropertiesInternal;
				IView dragSource = null;
				ImageSource sourceTarget = await dataView.GetImageAsync();
				string text = await dataView.GetTextAsync();

				if (internalProperties.ContainsKey("DragSource"))
				{
					dragSource = (IView)internalProperties["DragSource"];
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