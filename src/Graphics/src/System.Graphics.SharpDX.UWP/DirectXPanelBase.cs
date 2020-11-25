using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpDX;
using SharpDX.Direct2D1;

namespace System.Graphics.SharpDX
{
    public class DirectXPanelBase : SwapChainPanel
    {
        private bool valid;
        private bool renderOnInvalidate;

        private SwapPanelDeviceManager deviceManager;
        private bool initialized;

        public bool RenderOnInvalidate
        {
            set => renderOnInvalidate = value;
        }

        protected SwapPanelDeviceManager DeviceManager => deviceManager;

        public void Invalidate()
        {
            if (valid)
            {
                valid = false;

                if (renderOnInvalidate)
                {
                    if (deviceManager.Initialized)
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
            valid = false;
            Render();
        }

        public void Render()
        {
            if (valid)
            {
                return;
            }

            valid = true;
            var context = deviceManager.ContextDirect2D;
            context.BeginDraw();
            Draw(context);
            context.EndDraw();
            
            deviceManager.Present();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);
           
            if (!initialized)
            {
                var dispatcher = Window.Current.Dispatcher;
                if (dispatcher != null && dispatcher.HasThreadAccess)
                {
                    initialized = true;

                    deviceManager = new SwapPanelDeviceManager(this);

                    var displayInformation = DisplayInformation.GetForCurrentView();
                    var logicalDpi = displayInformation.LogicalDpi;
                    deviceManager.Initialize(logicalDpi);

                    SizeChanged += (sender, args) =>
                    {
                        deviceManager.OnSizeChange(args.NewSize.Width, args.NewSize.Height);
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
