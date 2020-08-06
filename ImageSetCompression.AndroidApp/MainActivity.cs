using Android;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.Navigation;

namespace ImageSetCompression.AndroidApp {
	[Android.App.Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener {
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);
			
			Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
			drawer.AddDrawerListener(toggle);
			toggle.SyncState();

			FindViewById<NavigationView>(Resource.Id.nav_view).SetNavigationItemSelectedListener(this);

			RequestPermissions(new[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 1);

			if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Android.Content.PM.Permission.Granted) {
				if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted) {
					SwitchFragment<CompressFragment>();
				} else {
					SwitchFragment<ViewFragment>();
				}
			} else {
				new AlertDialog.Builder(ApplicationContext)
					.SetTitle("Permission is required")
					.SetMessage("We need permission to read files in order to do anything. Additionally, permission to write files is required to compress images.")
					.SetPositiveButton("OK", (o, e) => Finish())
					.Create();
			}
		}

		public override void OnBackPressed() {
			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			if (drawer.IsDrawerOpen(GravityCompat.Start)) {
				drawer.CloseDrawer(GravityCompat.Start);
			} else {
				base.OnBackPressed();
			}
		}

		internal void SwitchFragment<T>() where T : Fragment, new() =>
			SupportFragmentManager.BeginTransaction()
				.Replace(Resource.Id.fragment_container, new T())
				.Commit();

		public bool OnNavigationItemSelected(IMenuItem item) {
			int id = item.ItemId;

			if (id == Resource.Id.nav_compress) {
				SwitchFragment<CompressFragment>();
			} else if (id == Resource.Id.nav_view_files) {
				SwitchFragment<ViewFragment>();
			} else if (id == Resource.Id.nav_settings) {
				SwitchFragment<SettingsFragment>();
			} else if (id == Resource.Id.nav_share) {

			} else if (id == Resource.Id.nav_send) {

			}

			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			drawer.CloseDrawer(GravityCompat.Start);
			return true;
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}
