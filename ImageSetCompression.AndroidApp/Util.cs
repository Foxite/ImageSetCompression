using System;
using System.Collections;
using System.Collections.Generic;
using Android.Content;

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
	}
}
