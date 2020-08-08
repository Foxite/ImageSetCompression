using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression {
	public static class ImageSetCompressor {
		#region Compress without progress
		public static IAsyncEnumerable<Image<Argb32>> CompressSet(Algorithm algorithm, IEnumerable<Image<Argb32>> set) {
			return algorithm.CompressAsync(set);
		}

		public static async Task CompressSet(Algorithm algorithm, IEnumerable<string> set, string resultPath) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			await foreach (Image<Argb32> image in CompressSet(algorithm, set.SelectDisposable(Image.Load<Argb32>))) {
				setPathEnumerator.MoveNext();
				await image.SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
			}
		}

		public static IAsyncEnumerable<Image<Argb32>> CompressSet(Algorithm algorithm, IEnumerable<string> set) {
			return CompressSet(algorithm, set.SelectDisposable(Image.Load<Argb32>));
		}
		#endregion

		#region Compress with progress
		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> CompressSet(Algorithm algorithm, IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) {
			return algorithm.CompressAsync(set, progress);
		}

		public static async Task CompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, string resultPath, IProgress<float> progress) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			foreach ((Task<Image<Argb32>> Image, Progress<float> ImageProgress) in CompressSet(algorithm, set.SelectDisposableCollection(Image.Load<Argb32>), progress)) {
				setPathEnumerator.MoveNext();
				await (await Image).SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
			}
		}

		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> CompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, IProgress<float> progress) {
			return CompressSet(algorithm, set.SelectDisposableCollection(Image.Load<Argb32>), progress);
		}
		#endregion

		#region Decompress without progress
		public static IAsyncEnumerable<Image<Argb32>> DecompressSet(Algorithm algorithm, IEnumerable<Image<Argb32>> set) {
			return algorithm.DecompressAsync(set);
		}

		public static async Task DecompressSet(Algorithm algorithm, IEnumerable<string> set, string resultPath) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			await foreach (Image<Argb32> image in DecompressSet(algorithm, set.SelectDisposable(Image.Load<Argb32>))) {
				setPathEnumerator.MoveNext();
				await image.SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
			}
		}

		public static IAsyncEnumerable<Image<Argb32>> DecompressSet(Algorithm algorithm, IEnumerable<string> set) {
			return DecompressSet(algorithm, set.SelectDisposable(Image.Load<Argb32>));
		}
		#endregion

		#region Compress with progress
		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> DecompressSet(Algorithm algorithm, IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) {
			return algorithm.DecompressAsync(set, progress);
		}

		public static async Task DecompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, string resultPath, IProgress<float> progress) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			foreach ((Task<Image<Argb32>> Image, Progress<float> ImageProgress) in DecompressSet(algorithm, set.SelectDisposableCollection(Image.Load<Argb32>), progress)) {
				setPathEnumerator.MoveNext();
				await (await Image).SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
			}
		}

		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> DecompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, IProgress<float> progress) {
			return DecompressSet(algorithm, set.SelectDisposableCollection(Image.Load<Argb32>), progress);
		}
		#endregion
	}
}
