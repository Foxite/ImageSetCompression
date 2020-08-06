using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;

namespace ImageSetCompression.AndroidApp {
	// TODO: preload images
	public class ImageListAdapter : FragmentStateAdapter {
		private readonly string m_BaseImagePath;
		private readonly IReadOnlyList<string> m_SetImages;

		public ImageListAdapter(ViewFragment fragment, string baseImagePath, IReadOnlyList<string> setImages) : base(fragment) {
			m_BaseImagePath = baseImagePath;
			m_SetImages = setImages;
		}

		public override int ItemCount => m_SetImages.Count + 1;

		public override Fragment CreateFragment(int index) {
			var ret = new ImageViewFragment();
			ret.Arguments = new Bundle();
			ret.Arguments.PutString(ImageViewFragment.BaseImagePathKey, m_BaseImagePath);
			ret.Arguments.PutString(ImageViewFragment.DeltaImagePathKey, index == 0 ? m_BaseImagePath : m_SetImages[index - 1]);
			return ret;
		}

		private class ImageViewFragment : Fragment {
			public const string BaseImagePathKey = nameof(m_BaseImagePath);
			public const string DeltaImagePathKey = nameof(m_DeltaImagePath);

			private string m_BaseImagePath;
			private string m_DeltaImagePath;

			public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
				return inflater.Inflate(Resource.Layout.fragment_view_image, container, false);
			}

			public override void OnViewCreated(View view, Bundle savedInstanceState) {
				m_BaseImagePath = Arguments.GetString(BaseImagePathKey);
				m_DeltaImagePath = Arguments.GetString(DeltaImagePathKey);

				// TODO: linear progress bar when loading image
				view.FindViewById<TextView>(Resource.Id.viewImageTitle).SetText(System.IO.Path.GetFileName(m_DeltaImagePath), TextView.BufferType.Normal);
				Task.Run(() => {
					Bitmap bmp;
					if (m_DeltaImagePath == m_BaseImagePath) {
						bmp = BitmapFactory.DecodeFile(m_BaseImagePath);
					} else {
						// TODO: in memory decompression, instead of saving it to FS and loading it right back
						string tempFile = System.IO.Path.GetTempFileName() + ".jpg";
						using (SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Argb32> slBitmap = ImageSetCompressor.DecompressImage(m_BaseImagePath, m_DeltaImagePath)) {
							SixLabors.ImageSharp.ImageExtensions.Save(slBitmap, tempFile, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
						}
						bmp = BitmapFactory.DecodeFile(tempFile);
					}
					Activity.FindViewById<ImageView>(Resource.Id.viewImageView).SetImageBitmap(bmp);
				});
			}
		}
	}
}
