using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

namespace ImageSetCompression.AndroidApp {
	public class ViewFragment : Fragment {
		private const int PickBaseImage = 1;
		private const int PickSetImages = 2;

		private string m_BaseImagePath;
		private IList<string> m_SetImages;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			var ret = inflater.Inflate(Resource.Layout.fragment_view, container, false);

			ret.FindViewById<Button>(Resource.Id.viewSelectBaseImage).Click += (o, e) => OnSelectBaseImage();
			ret.FindViewById<Button>(Resource.Id.viewSelectSetImages).Click += (o, e) => OnSelectSetImages();
			ret.FindViewById<Button>(Resource.Id.viewPrevImage).Click += (o, e) => MoveImage(-1);
			ret.FindViewById<Button>(Resource.Id.viewNextImage).Click += (o, e) => MoveImage(1);
			
			return ret;
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

		private int m_ImageIndex = 0;

		private void MoveImage(int direction) {
			m_ImageIndex += direction;
			SetImage(m_ImageIndex == 0 ? m_BaseImagePath : m_SetImages[m_ImageIndex - 1]);
		}
		
		public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
			if (resultCode == (int) Android.App.Result.Ok) {
				if (requestCode == PickBaseImage) {
					m_BaseImagePath = data.Data.Path;
				} else if (requestCode == PickSetImages) {
					m_SetImages = new[] { data.Data.Path }; //data.ClipData.AsList();
				}

				if (m_BaseImagePath != null && File.Exists(m_BaseImagePath) && m_SetImages != null && m_SetImages.Any() && m_SetImages.All(File.Exists)) {
					m_ImageIndex = 0;
					SetImage(m_BaseImagePath);
				}
			}
		}

		private void SetImage(string path) {
			Bitmap bmp;
			if (path == m_BaseImagePath) {
				bmp = BitmapFactory.DecodeFile(path);
			} else {
				using var ms = new MemoryStream();
				using (SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Argb32> slBitmap = ImageSetCompressor.DecompressImage(m_BaseImagePath, path)) {
					slBitmap.Save(ms, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
				}
				bmp = BitmapFactory.DecodeStream(ms);
			}
			Activity.FindViewById<ImageView>(Resource.Id.viewImageView).SetImageBitmap(bmp);
			Activity.FindViewById<TextView>(Resource.Id.viewImageTitle).SetText(System.IO.Path.GetFileName(path), TextView.BufferType.Normal);
		}
	}
}
