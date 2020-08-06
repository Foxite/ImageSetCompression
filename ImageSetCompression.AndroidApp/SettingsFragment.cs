using Android.OS;
using AndroidX.Preference;

namespace ImageSetCompression.AndroidApp {
	public class SettingsFragment : PreferenceFragmentCompat {
		public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey) {
			SetPreferencesFromResource(Resource.Xml.preferences, rootKey);
		}
	}
}