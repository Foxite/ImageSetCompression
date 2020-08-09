using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageSetCompression {
	// TODO: move this into a solution
	// Document everything, add stuff from other projects, and publish to Myget as Foxite.Common
	// Then add PackageReference to the package
	public static class LinqExtensions {
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T prepend) {
			yield return prepend;
			foreach (T item in source) {
				yield return item;
			}
		}

		/// <summary>
		/// Returns an IReadOnlyCollection wrapping <paramref name="source"/>, which applies <paramref name="selector"/> when enumerating objects.
		/// </summary>
		public static IReadOnlyCollection<TSelect> CollectionSelect<TCollection, TSelect>(this IReadOnlyCollection<TCollection> source, Func<TCollection, TSelect> selector) =>
			new SelectedCollection<TCollection, TSelect>(source, selector);

		private class SelectedCollection<TCollection, TSelect> : IReadOnlyCollection<TSelect> {
			private readonly IReadOnlyCollection<TCollection> m_Source;
			private readonly Func<TCollection, TSelect> m_Selector;

			public SelectedCollection(IReadOnlyCollection<TCollection> source, Func<TCollection, TSelect> selector) {
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
		public static IReadOnlyList<TSelect> ListSelect<TList, TSelect>(this IReadOnlyList<TList> source, Func<TList, TSelect> selector) =>
			new SelectedList<TList, TSelect>(source, selector);

		private class SelectedList<TList, TSelect> : IReadOnlyList<TSelect> {
			private readonly IReadOnlyList<TList> m_Source;
			private readonly Func<TList, TSelect> m_Selector;

			public SelectedList(IReadOnlyList<TList> source, Func<TList, TSelect> selector) {
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

		public static IReadOnlyList<T> ToLazyList<T>(this IEnumerable<T> source, int count) => new LazyList<T>(source, count);

		private sealed class LazyList<T> : IReadOnlyList<T>, IDisposable {
			private readonly IEnumerator<T> m_Source;
			private readonly List<T> m_BackingList = new List<T>();

			public LazyList(IEnumerable<T> source, int count) {
				Count = count;
				m_Source = source.GetEnumerator();
			}

			public T this[int index] {
				get {
					while (index >= m_BackingList.Count && m_Source.MoveNext()) {
						m_BackingList.Add(m_Source.Current);
					}

					return m_BackingList[index];
				}
			}

			public int Count { get; }

			public void Dispose() => m_Source.Dispose();
			public IEnumerator<T> GetEnumerator() => m_BackingList.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => m_BackingList.GetEnumerator();
		}

		public static IAsyncEnumerable<TSelect> Select<TSource, TSelect>(this IAsyncEnumerable<TSource> source, Func<Task<TSource>, TSelect> selector) =>
			new AsyncSelectEnumerable<TSource, TSelect>(source, selector);

		private class AsyncSelectEnumerable<TSource, TSelect> : IAsyncEnumerable<TSelect> {
			private readonly IAsyncEnumerable<TSource> m_Source;
			private readonly Func<Task<TSource>, TSelect> m_Selector;

			public AsyncSelectEnumerable(IAsyncEnumerable<TSource> source, Func<Task<TSource>, TSelect> selector) {
				m_Source = source;
				m_Selector = selector;
			}

			public IAsyncEnumerator<TSelect> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
				new AsyncSelectEnumerator(m_Source, m_Selector);

			private class AsyncSelectEnumerator : IAsyncEnumerator<TSelect> {
				private readonly IAsyncEnumerator<TSource> m_Source;
				private readonly Func<Task<TSource>, TSelect> m_Selector;

				public AsyncSelectEnumerator(IAsyncEnumerable<TSource> source, Func<Task<TSource>, TSelect> selector) {
					m_Source = source.GetAsyncEnumerator();
					m_Selector = selector;
				}

				public TSelect Current { get; private set; }

				public ValueTask DisposeAsync() => m_Source.DisposeAsync();

				public ValueTask<bool> MoveNextAsync() {
					ValueTask<bool> valueTask = m_Source.MoveNextAsync();
					ValueTask<bool> ret = valueTask.Preserve();
					Current = m_Selector(Task.Run(async () => {
						await valueTask;
						return m_Source.Current;
					}));
					return ret;
				}
			}
		}
	}
}
