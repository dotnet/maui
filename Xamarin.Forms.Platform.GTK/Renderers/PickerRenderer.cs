using Gtk;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class PickerRenderer : ViewRenderer<Picker, ComboBox>
    {
        private bool _disposed;

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    // Use Gtk.ComboBox, a widget used to choose from a list of items.
                    ComboBox comboBox = new ComboBox();
                    CellRendererText text = new CellRendererText();
                    comboBox.PackStart(text, true);
                    comboBox.AddAttribute(text, "text", 0);

                    comboBox.Focused += OnFocused;
                    comboBox.FocusOutEvent += OnFocusOutEvent;
                    comboBox.Changed += OnChanged;

                    ((LockableObservableListWrapper)Element.Items)._list.CollectionChanged += OnCollectionChanged;

                    SetNativeControl(comboBox);
                }

                UpdateItemsSource();
                UpdateSelectedIndex();
                UpdateTextColor();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Picker.TitleProperty.PropertyName)
                UpdatePicker();
            if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
                UpdateSelectedIndex();
            if (e.PropertyName == Picker.ItemsSourceProperty.PropertyName)
                UpdateItemsSource();
            if (e.PropertyName == Picker.TextColorProperty.PropertyName)
                UpdateTextColor();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    if (Control != null)
                    {
                        Control.Focused -= OnFocused;
                        Control.FocusOutEvent -= OnFocusOutEvent;
                        Control.Changed -= OnChanged;
                    }

                    if(Element != null)
                    {
                        ((LockableObservableListWrapper)Element.Items)._list.CollectionChanged -= OnCollectionChanged;
                    }

                }
            }

            base.Dispose(disposing);
        }

        internal override void OnElementFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
        {
            if (Control == null)
                return;

            if (args.Focus)
                args.Result = OpenPicker();
            else
                args.Result = ClosePicker();

            base.OnElementFocusChangeRequested(sender, args);
        }

        private void UpdatePicker()
        {
            if (Control == null || Element == null)
                return;

            var selectedIndex = Element.SelectedIndex;
            var items = Element.Items;

            if (items == null || items.Count == 0 || selectedIndex < 0)
                return;

            UpdateItemsSource();
            UpdateSelectedIndex();
        }

        private void UpdateItemsSource()
        {
            var items = ((LockableObservableListWrapper)Element.Items)._list;
            ListStore listStore = new ListStore(typeof(string));
            Control.Model = listStore;

            foreach (var item in items)
            {
                listStore.AppendValues(item);
            }
        }

        private void UpdateSelectedIndex()
        {
            var selectedIndex = Element.SelectedIndex != -1 ? Element.SelectedIndex : 0;

            Control.Active = selectedIndex;
        }

        private void UpdateTextColor()
        {
            if (Control == null || Element == null)
                return;

            var cellView = Control.Child as CellView;

            if (cellView != null)
            {
                var cellRenderer = cellView.Cells.FirstOrDefault() as CellRendererText;

                if (cellRenderer != null)
                {
                    var textColor = Element.TextColor.ToGtkColor();

                    cellRenderer.ForegroundGdk = Element.TextColor.ToGtkColor();
                }
            }
        }

        private void OnFocused(object o, FocusedArgs args)
        {
            ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
        }

        private void OnFocusOutEvent(object o, FocusOutEventArgs args)
        {
            ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
        }

        private void OnChanged(object sender, System.EventArgs e)
        {
            ElementController?.SetValueFromRenderer(Picker.SelectedIndexProperty, Control.Active);
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateItemsSource();
        }

        private bool OpenPicker()
        {
            if(Control == null)
            {
                return false;
            }

            Control.Popup();

            return true;
        }

        private bool ClosePicker()
        {
            if (Control == null)
            {
                return false;
            }

            Control.Popdown();

            return true;
        }
    }
}