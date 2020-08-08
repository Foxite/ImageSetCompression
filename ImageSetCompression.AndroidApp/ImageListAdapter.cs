using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression.AndroidApp {
	// TODO: preload images
	// Should be relatively easy now by enumerating the LazyList
	public class ImageListAdapter : FragmentStateAdapter {
		private readonly IReadOnlyList<(Task<Image<Argb32>> Image, Progress<float> Progress, string Path)> m_SetImages;

		public ImageListAdapter(Algorithm algorithm, ViewFragment fragment, IReadOnlyList<string> setImages) : base(fragment) {
			m_SetImages = ImageSetCompressor.DecompressSet(algorithm, setImages.SelectDisposableCollection(Image.Load<Argb32>), new Progress<float>())
				.Select((tuple, idx) => (tuple.Image, tuple.ImageProgress, Path: setImages[idx])).ToLazyList();
		}

		public override int ItemCount => m_SetImages.Count;

		public override Fragment CreateFragment(int index) {
			(Task<Image<Argb32>> Image, Progress<float> Progress, string Path) item = m_SetImages[index];
			return new ImageViewFragment(item.Path, item.Image, item.Progress);
		}

		private class ImageViewFragment : Fragment {
			private readonly string m_Path;
			private readonly Task<Image<Argb32>> m_Image;
			private readonly Progress<float> m_Progress;

			public ImageViewFragment(string filename, Task<Image<Argb32>> image, Progress<float> progress) {
				m_Path = filename;
				m_Image = image;
				m_Progress = progress;
			}

			public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
				return inflater.Inflate(Resource.Layout.fragment_view_image, container, false);
			}

			public override void OnViewCreated(View view, Bundle savedInstanceState) {
				view.FindViewById<TextView>(Resource.Id.viewImageTitle).SetText(System.IO.Path.GetFileNameWithoutExtension(m_Path), TextView.BufferType.Normal);
				Task.Run(() => LoadImage(view));
			}

			private async Task LoadImage(View view) {
				try {
					ProgressBar progressBar = view.FindViewById<ProgressBar>(Resource.Id.viewFragmentProgress);
					m_Progress.ProgressChanged += (o, p) => {
						// Oddly enough, you can do this off the UI thread.
						progressBar.Progress = (int) (p * 100);
					};

					using var ms = new MemoryStream();
					Image<Argb32> decompressedImage = await m_Image;
					decompressedImage.Save(ms, SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance);
					ms.Seek(0, SeekOrigin.Begin);
					Bitmap bmp = BitmapFactory.DecodeStream(ms);

					Activity.RunOnUiThread(() => {
						((ViewGroup) view).RemoveView(progressBar);
						view.FindViewById<ImageView>(Resource.Id.viewImageView).SetImageBitmap(bmp);
					});
				} catch (Exception e) {
					Activity.RunOnUiThread(() => throw new FileLoadException("Exception thrown when loading image", m_Path, e.Demystify()));
				}
			}
		}
	}
}
