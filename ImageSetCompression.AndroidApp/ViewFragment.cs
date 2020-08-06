using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;

namespace ImageSetCompression.AndroidApp {
	// TODO: move common code between this and CompressFragment into abstract class
	public class ViewFragment : Fragment {
		private const int PickBaseImage = 1;
		private const int PickSetImages = 2;

		private string m_BaseImagePath;
		private IReadOnlyList<string> m_SetImages;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_view, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState) {
			Activity.FindViewById<Button>(Resource.Id.viewSelectBaseImage).Click += (o, e) => OnSelectBaseImage();
			Activity.FindViewById<Button>(Resource.Id.viewSelectSetImages).Click += (o, e) => OnSelectSetImages();
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
			var intent = Intent.CreateChooser(chooseFile, "Select delta images");
			StartActivityForResult(intent, PickSetImages);
		}

		public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
			if (resultCode == (int) Android.App.Result.Ok) {
				if (requestCode == PickBaseImage) {
					m_BaseImagePath = data.Data.Path;
				} else if (requestCode == PickSetImages) {
					// TODO fix this for multiple files (also in CompressFragment)
					// TODO fix this for /document/... uris, this is returned by the Android file chooser (which is also the only picker on my phone that can actually select multiple files)
					m_SetImages = new[] { data.Data.Path };
				}

				if (m_BaseImagePath != null && File.Exists(m_BaseImagePath) && m_SetImages != null && m_SetImages.Any() && m_SetImages.All(File.Exists)) {
					var adapter = new ImageListAdapter(this, m_BaseImagePath, m_SetImages);
					Activity.FindViewById<ViewPager2>(Resource.Id.pager).Adapter = adapter;
				}
			}
		}
	}
}
