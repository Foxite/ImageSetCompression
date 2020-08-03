using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace ImageSetCompression.AndroidApp {
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener {
		private const int PickBaseImage = 1;
		private const int PickSetImages = 2;

		private string m_BaseImagePath;
		private Util.ClipDataList m_SetImages;
		private string m_ResultFolder;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);
			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
			fab.Click += FabOnClick;

			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
			drawer.AddDrawerListener(toggle);
			toggle.SyncState();

			FindViewById<NavigationView>(Resource.Id.nav_view).SetNavigationItemSelectedListener(this);

			FindViewById<Button>(Resource.Id.compress).Click += (o, e) => OnCompressImages();
			FindViewById<Button>(Resource.Id.decompress).Click += (o, e) => OnDecompressImages();
			FindViewById<Button>(Resource.Id.selectBaseImage).Click += (o, e) => OnSelectBaseImage();
			FindViewById<Button>(Resource.Id.selectSetImages).Click += (o, e) => OnSelectSetImages();
		}

		private void OnCompressImages() {
			ImageSetCompressor.CompressSet(m_BaseImagePath, m_SetImages.Select(item => item.Uri.Path), m_ResultFolder, false);
		}
		
		private void OnDecompressImages() {
			ImageSetCompressor.DecompressImageSet(m_BaseImagePath, m_SetImages.Select(item => item.Uri.Path), m_ResultFolder, false);
		}

		private void OnSelectBaseImage() {
			var chooseFile = new Intent(Intent.ActionGetContent);
			chooseFile.AddCategory(Intent.CategoryOpenable);
			chooseFile.SetType("image/*");
			var intent = Intent.CreateChooser(chooseFile, "Select base image");
			StartActivityForResult(intent, PickBaseImage);
		}
		
		private void OnSelectSetImages() {
			var chooseFile = new Intent(Intent.ActionGetContent);
			chooseFile.AddCategory(Intent.CategoryOpenable);
			chooseFile.PutExtra(Intent.ExtraAllowMultiple, true);
			chooseFile.SetType("image/*");
			var intent = Intent.CreateChooser(chooseFile, "Select variant/delta images");
			StartActivityForResult(intent, PickSetImages);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			if (resultCode == Result.Ok) {
				if (requestCode == PickBaseImage) {
					m_BaseImagePath = data.Data.EncodedPath;
					m_ResultFolder = Path.Combine(data.Data.EncodedPath, "result");
				} else if (requestCode == PickSetImages) {
					m_SetImages = data.ClipData.AsList();
				}
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

		public override bool OnCreateOptionsMenu(IMenu menu) {
			MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			int id = item.ItemId;
			if (id == Resource.Id.action_settings) {
				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		private void FabOnClick(object sender, EventArgs eventArgs) {
			View view = (View) sender;
			Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
				.SetAction("Action", (Android.Views.View.IOnClickListener) null).Show();
		}

		public bool OnNavigationItemSelected(IMenuItem item) {
			int id = item.ItemId;

			if (id == Resource.Id.nav_compress) {
				
			} else if (id == Resource.Id.nav_view_files) {

			} else if (id == Resource.Id.nav_settings) {

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

	public static class Util {
		public static ClipDataList AsList(this ClipData clipdata) => new ClipDataList(clipdata);

		public sealed class ClipDataList : IReadOnlyList<ClipData.Item>, IDisposable {
			public ClipDataList(ClipData clipData) {
				ClipData = clipData;
			}

			public ClipData.Item this[int index] => ClipData.GetItemAt(index);

			public int Count => ClipData.ItemCount;

			public ClipData ClipData { get; }

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public IEnumerator<ClipData.Item> GetEnumerator() {
				for (int i = 0; i < ClipData.ItemCount; i++) {
					yield return ClipData.GetItemAt(i);
				}
			}

			public void Dispose() => ClipData.Dispose();
		}
	}
}

