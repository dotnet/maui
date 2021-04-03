using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpDX;
using SharpDX.Direct2D1;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DirectXPanelBase : SwapChainPanel
    {
        private bool _valid;
        private bool _renderOnInvalidate;

        private SwapPanelDeviceManager _deviceManager;
        private bool _initialized;

        public bool RenderOnInvalidate
        {
            set => _renderOnInvalidate = value;
        }

        protected SwapPanelDeviceManager DeviceManager => _deviceManager;

        public void Invalidate()
        {
            if (_valid)
            {
                _valid = false;

                if (_renderOnInvalidate)
                {
                    if (_deviceManager.Initialized)
                    {
                        Render();
                    }
                }
                else if (!InTouchEvent)
                {
                    var dispatcher = Window.Current.CoreWindow.Dispatcher;
                    Task.Run(() => dispatcher.RunAsync(CoreDispatcherPriority.Normal, Render));                    
                }
            }
        }

        protected bool InTouchEvent { get; set; }

        public void ForceRender()
        {
            _valid = false;
            Render();
        }

        public void Render()
        {
            if (_valid)
            {
                return;
            }

            _valid = true;
            var context = _deviceManager.ContextDirect2D;
            context.BeginDraw();
            Draw(context);
            context.EndDraw();
            
            _deviceManager.Present();
        }

        protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);
           
            if (!_initialized)
            {
                var dispatcher = Window.Current.Dispatcher;
                if (dispatcher != null && dispatcher.HasThreadAccess)
                {
                    _initialized = true;

                    _deviceManager = new SwapPanelDeviceManager(this);

                    var displayInformation = DisplayInformation.GetForCurrentView();
                    var logicalDpi = displayInformation.LogicalDpi;
                    _deviceManager.Initialize(logicalDpi);

                    SizeChanged += (sender, args) =>
                    {
                        _deviceManager.OnSizeChange(args.NewSize.Width, args.NewSize.Height);
                        ForceRender();
                    };
                }
            }

            return result;
        }

        protected virtual void Draw(DeviceContext context)
        {
            context.DrawLine(new Vector2(0,0), new Vector2(100,100), new SolidColorBrush(context, Color4.Black) );
        }
    }
}
