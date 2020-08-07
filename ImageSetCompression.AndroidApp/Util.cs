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

	public static class LinqExtensions {
		/// <summary>
		/// Returns an IReadOnlyCollection wrapping <paramref name="source"/>, which applies <paramref name="selector"/> when enumerating objects.
		/// </summary>
		public static IReadOnlyCollection<TSelect> CollectionSelect<TCollection, TSelect>(this ICollection<TCollection> source, Func<TCollection, TSelect> selector) =>
			new SelectedCollection<TCollection, TSelect>(source, selector);

		private class SelectedCollection<TCollection, TSelect> : IReadOnlyCollection<TSelect> {
			private readonly ICollection<TCollection> m_Source;
			private readonly Func<TCollection, TSelect> m_Selector;

			public SelectedCollection(ICollection<TCollection> source, Func<TCollection, TSelect> selector) {
				m_Source = source;
				m_Selector = selector;
			}

			public int Count => m_Source.Count;

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public IEnumerator<TSelect> GetEnumerator() {
				foreach (TCollection item in m_Source) {
					yield return m_Selector(item);
				}
			}
		}
		
		/// <summary>
		/// Returns an IReadOnlyList wrapping <paramref name="source"/>, which applies <paramref name="selector"/> when enumerating objects and indexing the list.
		/// </summary>
		public static IReadOnlyList<TSelect> ListSelect<TList, TSelect>(this IList<TList> source, Func<TList, TSelect> selector) =>
			new SelectedList<TList, TSelect>(source, selector);

		private class SelectedList<TList, TSelect> : IReadOnlyList<TSelect> {
			private readonly IList<TList> m_Source;
			private readonly Func<TList, TSelect> m_Selector;

			public SelectedList(IList<TList> source, Func<TList, TSelect> selector) {
				m_Source = source;
				m_Selector = selector;
			}

			public int Count => m_Source.Count;

			public TSelect this[int index] => m_Selector(m_Source[index]);

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public IEnumerator<TSelect> GetEnumerator() {
				foreach (TList item in m_Source) {
					yield return m_Selector(item);
				}
			}
		}
	}
}
