using System;
using System.Collections;
using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Uri = Android.Net.Uri;

namespace ImageSetCompression.AndroidApp {
	public static class Util {
		public static T GetSystemService<T>(this Context context) where T : Java.Lang.Object {
			return (T) context.GetSystemService(Java.Lang.Class.FromType(typeof(T)));
		}

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

		// https://stackoverflow.com/a/39388941
		public static string GetPathFromUri(Context context, Uri uri) {
			string getDataColumn(Uri uri, string selection, string[] selectionArgs) {
				using ICursor cursor = context.ContentResolver.Query(uri, new string[] { "_data" }, selection, selectionArgs, null);
				if (cursor != null && cursor.MoveToFirst()) {
					return cursor.GetString(cursor.GetColumnIndexOrThrow("_data"));
				} else {
					return null;
				}
			}
			bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

			if (isKitKat && DocumentsContract.IsDocumentUri(context, uri)) {
				if (uri.Authority == "com.android.externalstorage.documents") {
					string docId = DocumentsContract.GetDocumentId(uri);
					string[] split = docId.Split(":");
					string type = split[0];

					if (type.Equals("primary", StringComparison.InvariantCultureIgnoreCase)) {
						return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1]; // TODO replace with non deprecated alternative
					}
					// TODO non primary volumes
				} else if (uri.Authority == "com.android.providers.downloads.documents") {
					return getDataColumn(ContentUris.WithAppendedId(Uri.Parse("content://downloads/public_downloads"), long.Parse(DocumentsContract.GetDocumentId(uri))), null, null);
				} else if (uri.Authority == "com.android.providers.media.documents") {
					return getDataColumn(MediaStore.Images.Media.ExternalContentUri, "_id=?", new[] { DocumentsContract.GetDocumentId(uri).Split(":")[1] });
				}
			} else if (uri.Scheme.Equals("content", StringComparison.InvariantCultureIgnoreCase)) {
				return uri.Authority == "com.google.android.apps.photos.content"
					? uri.LastPathSegment
					: getDataColumn(uri, null, null);
			} else if (uri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
				return uri.Path;
			}

			return null;
		}
	}
}
