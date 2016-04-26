using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.TestCasesPages {

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1905, "AlertView doesn't scroll when text is to large", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue1905 : ContentPage {
		
		public Issue1905 () {
			var btn = new Button { Text="Show alert" };
			btn.Clicked += async (object sender, EventArgs e) => {
				await DisplayAlert ("Long Message", "Start - kajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfjkajsd;lfkjals;kddfjkl;asdadjsf;lkasjdfl;kasjdf;lkajsd;flkjasd;flkjasd;lkdfjkakls;dfjkl;asdjfkl;asdjfl;kasdjadsfkl;asjdfl;kajadjs;flkjasdl;fklf;jas;dlkjf;alskdjdfakl;sddfjsakls;djfjl;aksdadfjsl;kasdjadsfl;kasdadfjskl;asjdfl;kasdjfl;kasdjfl;kasjdfl;kasjdfkl;asjdfl;kasjdfl;kasjdfl;kasjdfkl;ajsdl;kfkl;jsadkl;fjklasl;dkfjals;kdfjals;kdjfals;kdfalksjdflkajsd;lkfjasl;dkdfjk;alskdjdfjal;skjdfl;askjdfl;aksjdfakl;sdjfkl;asjdfkl;asjdfl;kajsdfl;kajsdfl;kajsdl;ffkl;jasdl;kkl;dfjadls;kdfjkals;kdjkadfsal;ksdjdfal;skddfjal;skdfjal;skdjfal;ksdfjakls;djdfakls;dfjl;askdfjal;skdjfakls;dfj - End", "Ok", "Cancel");
			};

			Content = btn;
		}

	
		
		protected override void OnAppearing () {
			
			base.OnAppearing ();
		}

	}
}
