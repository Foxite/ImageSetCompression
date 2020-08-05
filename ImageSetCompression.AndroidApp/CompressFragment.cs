using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace ImageSetCompression.AndroidApp {
	public class CompressFragment : Fragment {
		private const int PickBaseImage = 1;
		private const int PickSetImages = 2;

		private string m_BaseImagePath;
		private IEnumerable<string> m_SetImages;
		private string m_ResultFolder;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			var ret = inflater.Inflate(Resource.Layout.fragment_compress, container);

			ret.FindViewById<Button>(Resource.Id.compressCompress).Click += (o, e) => AsynchronousOperation("Compressing images...", () => {
				File.Copy(m_BaseImagePath, Path.Combine(m_ResultFolder, Path.GetFileName(m_BaseImagePath)));
				ImageSetCompressor.CompressSet(m_BaseImagePath, m_SetImages.Select(item => item), m_ResultFolder);
			});

			ret.FindViewById<Button>(Resource.Id.compressDecompress).Click += (o, e) => AsynchronousOperation("Decompresing images...", () => {
				File.Copy(m_BaseImagePath, Path.Combine(m_ResultFolder, Path.GetFileName(m_BaseImagePath)));
				ImageSetCompressor.DecompressImageSet(m_BaseImagePath, m_SetImages.Select(item => item), m_ResultFolder);
			});

			ret.FindViewById<Button>(Resource.Id.compressSelectBaseImage).Click += (o, e) => OnSelectBaseImage();
			ret.FindViewById<Button>(Resource.Id.compressSelectSetImages).Click += (o, e) => OnSelectSetImages();

			return ret;
		}

		private void AsynchronousOperation(string label, Action operation) {
			Task.Run(() => {
				int notificationID = Guid.NewGuid().GetHashCode();
				var manager = Activity.ApplicationContext.GetSystemService<Android.App.NotificationManager>();

				try {
					var notifBuilder = new NotificationCompat.Builder(Activity.ApplicationContext, "ImageSetCompressor")
						.SetSmallIcon(Resource.Mipmap.ic_launcher)
						.SetContentTitle("ImageSetCompressor")
						.SetContentText(label)
						.SetProgress(0, 0, true);

					manager.Notify(notificationID, notifBuilder.Build());

					operation();

					manager.Notify(
						notificationID,
						new NotificationCompat.Builder(Activity.ApplicationContext, "ImageSetCompressor")
							.SetSmallIcon(Resource.Mipmap.ic_launcher)
							.SetContentTitle("ImageSetCompressor")
							.SetOngoing(false)
							.SetContentText("Operation completed")
							.Build()
					);
				} catch (Exception e) {
					manager.Notify(
						notificationID,
						new NotificationCompat.Builder(Activity.ApplicationContext, "ImageSetCompressor")
							.SetSmallIcon(Resource.Mipmap.ic_launcher)
							.SetOngoing(false)
							.SetContentTitle("ImageSetCompressor")
							.SetContentText("Operation failed")
							.SetSubText(e.ToStringDemystified())
							.Build()
					);
					throw;
				}
			});
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
		
		public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
			if (resultCode == (int) Android.App.Result.Ok) {
				if (requestCode == PickBaseImage) {
					m_BaseImagePath = data.Data.Path;
					m_ResultFolder = Path.Combine(Path.GetDirectoryName(data.Data.Path), "result");
					if (!Directory.Exists(m_ResultFolder)) {
						Directory.CreateDirectory(m_ResultFolder);
					}
				} else if (requestCode == PickSetImages) {
					m_SetImages = new[] { data.Data.Path }; //data.ClipData.AsList();
				}
			}
		}

	}
}