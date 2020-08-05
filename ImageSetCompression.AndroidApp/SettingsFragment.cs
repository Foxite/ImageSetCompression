using Android.OS;
using Android.Preferences;

namespace ImageSetCompression.AndroidApp {
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CS0618:Type or member is obsolete", Justification = "Replaced by AndroidX which is not supported by Xamarin (I think)")]
	public class SettingsFragment : PreferenceFragment {
		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			AddPreferencesFromResource(Resource.Xml.preferences);
		}
	}
}