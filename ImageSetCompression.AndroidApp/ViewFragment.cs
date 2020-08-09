using System;
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
	public class ViewFragment : Fragment {
		private const int PickSetImages = 2;
		private const int PickRecentImages = 3;

		private IReadOnlyList<string> m_SetImages;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			return inflater.Inflate(Resource.Layout.fragment_view, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState) {
			Activity.FindViewById<Button>(Resource.Id.viewSelectSetImages).Click += (o, e) => OnSelectSetImages();
			Activity.FindViewById<Button>(Resource.Id.viewSelectRecentSet).Click += (o, e) => OnSelectRecentSet();
		}

		private void OnSelectSetImages() {
			var chooseFile = new Intent(Intent.ActionGetContent);
			chooseFile.AddCategory(Intent.CategoryOpenable);
			chooseFile.PutExtra(Intent.ExtraAllowMultiple, true);
			chooseFile.SetType("image/*");
			var intent = Intent.CreateChooser(chooseFile, "@string/select_delta");
			StartActivityForResult(intent, PickSetImages);
		}

		private void OnSelectRecentSet() {
			StartActivityForResult(new Intent(Activity, typeof(RecentSetsActivity)), PickRecentImages);
		}

		public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
			if (resultCode == (int) Android.App.Result.Ok) {
				if (requestCode == PickSetImages) {
					IReadOnlyList<Android.Net.Uri> uris;
					if (data.Data == null) {
						uris = data.ClipData.AsList().ListSelect(item => item.Uri);
					} else {
						uris = new[] { data.Data };
					}

					m_SetImages = uris.ListSelect(oldPath => Util.GetPathFromUri(Activity.ApplicationContext, oldPath));

					ImageSet.AddRecentSet(Activity.ApplicationContext, m_SetImages);

					if (m_SetImages.Any() && m_SetImages.All(File.Exists)) {
						var adapter = new ImageListAdapter(Algorithm.Delta, this, m_SetImages); // TODO: let user specify algorithm
						Activity.FindViewById<ViewPager2>(Resource.Id.pager).Adapter = adapter;
					}
				} else if (requestCode == PickRecentImages) {
					m_SetImages = data.ClipData.AsList().ListSelect(item => item.Text);
					ImageSet.AddRecentSet(Activity.ApplicationContext, m_SetImages); // This causes the set to be moved to the top of the list
				}
			} else {
				m_SetImages = null;
			}
		}
	}
}
