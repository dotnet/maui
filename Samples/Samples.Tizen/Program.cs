using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace Samples.Tizen
{
    class Program : FormsApplication
    {
        static App formsApp;

        protected override void OnCreate()
        {
            base.OnCreate();

            LoadApplication(formsApp ??= new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app);
            FormsMaterial.Init();
            Xamarin.Essentials.Platform.MapServiceToken = "MAP_SERVICE_KEY";
            app.Run(args);
        }
    }
}
