using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
		private const int PickSetImages = 2;

		private IReadOnlyList<string> m_SetImages;
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

				ret.FindViewById<Button>(Resource.Id.compressCompress).Click += (o, e) => AsynchronousOperation("@string/compressing", (progress) =>
					ImageSetCompressor.CompressSet(Algorithm.Delta, m_SetImages, m_ResultFolder, progress)
				);

				ret.FindViewById<Button>(Resource.Id.compressDecompress).Click += (o, e) => AsynchronousOperation("@string/decompressing", (progress) =>
					ImageSetCompressor.DecompressSet(Algorithm.Delta, m_SetImages, m_ResultFolder, progress)
				);

				ret.FindViewById<Button>(Resource.Id.compressSelectSetImages).Click += (o, e) => OnSelectSetImages();

				return ret;
			}
		}

		private void AsynchronousOperation(string label, Func<IProgress<float>, Task> operation) {
			Task.Run(async () => {
				int notificationID = Guid.NewGuid().GetHashCode();
				var manager = Activity.ApplicationContext.GetSystemService<Android.App.NotificationManager>();

				try {
					NotificationCompat.Builder builder = new NotificationCompat.Builder(Activity.ApplicationContext, "ImageSetCompression")
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
					await operation(progress);

					manager.Notify(
						notificationID,
						new NotificationCompat.Builder(Activity.ApplicationContext, "ImageSetCompression")
							.SetSmallIcon(Resource.Mipmap.ic_launcher)
							.SetOngoing(false)
							.SetContentTitle("@string/app_name")
							.SetContentText("@string/completed")
							.Build()
					);
				} catch (Exception e) {
					manager.Notify(
						notificationID,
						new NotificationCompat.Builder(Activity.ApplicationContext, "ImageSetCompression")
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

		private void OnSelectSetImages() {
			var chooseFile = new Intent(Intent.ActionGetContent);
			chooseFile.AddCategory(Intent.CategoryOpenable);
			chooseFile.PutExtra(Intent.ExtraAllowMultiple, true);
			chooseFile.SetType("image/*");
			var intent = Intent.CreateChooser(chooseFile, "@string/select_set");
			StartActivityForResult(intent, PickSetImages);
		}
		
		public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
			if (requestCode == PickSetImages) {
				if (resultCode == (int) Android.App.Result.Ok) {
					IReadOnlyList<Android.Net.Uri> uris;
					if (data.Data == null) {
						uris = data.ClipData.AsList().ListSelect(item => item.Uri);
					} else {
						uris = new[] { data.Data };
					}

					m_SetImages = uris.ListSelect(oldPath => Util.GetPathFromUri(Activity.ApplicationContext, oldPath));

					m_ResultFolder = Path.Combine(Path.GetDirectoryName(m_SetImages[0]), "result"); // TODO localize?
					if (!Directory.Exists(m_ResultFolder)) {
						Directory.CreateDirectory(m_ResultFolder);
					}
				}
			}
		}
	}
}
