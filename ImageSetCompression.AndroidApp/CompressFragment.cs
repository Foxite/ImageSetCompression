using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Fragment.App;

namespace ImageSetCompression.AndroidApp {
	public class CompressFragment : Fragment {
		private const int PickBaseImage = 1;
		private const int PickSetImages = 2;

		private string m_BaseImagePath;
		private IList<string> m_SetImages;
		private string m_ResultFolder;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			if (Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Denied) {
				new AlertDialog.Builder(Activity.ApplicationContext)
					.SetTitle("@string/permission_required")
					.SetMessage("@string/permission_required_write")
					.SetPositiveButton("@string/ok", (o, e) => ((MainActivity) Activity).SwitchFragment<ViewFragment>())
					.Create();

				return new View(Activity.ApplicationContext);
			} else {
				var ret = inflater.Inflate(Resource.Layout.fragment_compress, container, false);

				ret.FindViewById<Button>(Resource.Id.compressCompress).Click += (o, e) => AsynchronousOperation("@string/compressing", (progress) => {
					File.Copy(m_BaseImagePath, Path.Combine(m_ResultFolder, Path.GetFileName(m_BaseImagePath)));
					ImageSetCompressor.CompressSet(m_BaseImagePath, m_SetImages.ListSelect(item => item), m_ResultFolder, progress);
				});

				ret.FindViewById<Button>(Resource.Id.compressDecompress).Click += (o, e) => AsynchronousOperation("@string/decompressing", (progress) => {
					File.Copy(m_BaseImagePath, Path.Combine(m_ResultFolder, Path.GetFileName(m_BaseImagePath)));
					ImageSetCompressor.DecompressSet(m_BaseImagePath, m_SetImages.ListSelect(item => item), m_ResultFolder, progress);
				});

				ret.FindViewById<Button>(Resource.Id.compressSelectBaseImage).Click += (o, e) => OnSelectBaseImage();
				ret.FindViewById<Button>(Resource.Id.compressSelectSetImages).Click += (o, e) => OnSelectSetImages();

				return ret;
			}
		}

		private void AsynchronousOperation(string label, Action<IProgress<float>> operation) {
			Task.Run(() => {
				int notificationID = Guid.NewGuid().GetHashCode();
				var manager = Activity.ApplicationContext.GetSystemService<Android.App.NotificationManager>();

				try {
					NotificationCompat.Builder builder = new NotificationCompat.Builder(Activity.ApplicationContext, "@string/app_name")
						.SetSmallIcon(Resource.Mipmap.ic_launcher)
						.SetContentTitle("@string/app_name")
						.SetContentText(label)
						.SetProgress(100, 0, false);

					manager.Notify(notificationID, builder.Build());

					var progress = new Progress<float>();
					progress.ProgressChanged += (o, p) => {
						manager.Notify(
							notificationID,
							builder
								.SetProgress(100, (int) (p * 100), false)
								.Build()
						);
					};
					operation(progress);

					manager.Notify(
						notificationID,
						new NotificationCompat.Builder(Activity.ApplicationContext, "@string/app_name")
							.SetSmallIcon(Resource.Mipmap.ic_launcher)
							.SetContentTitle("@string/app_name")
							.SetOngoing(false)
							.SetContentText("@string/completed")
							.Build()
					);
				} catch (Exception e) {
					manager.Notify(
						notificationID,
						new NotificationCompat.Builder(Activity.ApplicationContext, "@string/app_name")
							.SetSmallIcon(Resource.Mipmap.ic_launcher)
							.SetOngoing(false)
							.SetContentTitle("@string/app_name")
							.SetContentText("@string/failed")
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
			var intent = Intent.CreateChooser(chooseFile, "@string/select_base");
			StartActivityForResult(intent, PickBaseImage);
		}
		
		private void OnSelectSetImages() {
			var chooseFile = new Intent(Intent.ActionGetContent);
			chooseFile.AddCategory(Intent.CategoryOpenable);
			chooseFile.PutExtra(Intent.ExtraAllowMultiple, true);
			chooseFile.SetType("image/*");
			var intent = Intent.CreateChooser(chooseFile, "@string/select_set");
			StartActivityForResult(intent, PickSetImages);
		}
		
		public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
			if (resultCode == (int) Android.App.Result.Ok) {
				if (requestCode == PickBaseImage) {
					m_BaseImagePath = data.Data.Path;
					m_ResultFolder = Path.Combine(Path.GetDirectoryName(data.Data.Path), "result"); // TODO localize?
					if (!Directory.Exists(m_ResultFolder)) {
						Directory.CreateDirectory(m_ResultFolder);
					}
				} else if (requestCode == PickSetImages) {
					// TODO fix this (also in ViewFragment)
					m_SetImages = new[] { data.Data.Path };
				}
			}
		}
	}
}
