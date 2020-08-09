using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			IEnumerable<Image<Argb32>> images = set.Select(Image.Load<Argb32>);
			try {
				await foreach (Image<Argb32> image in CompressSet(algorithm, images)) {
					setPathEnumerator.MoveNext();
					await image.SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
				}
			} finally {
				foreach (Image<Argb32> image in images) {
					image.Dispose();
				}
			}
		}

		public async static IAsyncEnumerable<Image<Argb32>> CompressSet(Algorithm algorithm, IEnumerable<string> set) {
			IEnumerable<Image<Argb32>> images = set.Select(Image.Load<Argb32>);
			try {
				await foreach (Image<Argb32> image in CompressSet(algorithm, images)) {
					yield return image;
				}
			} finally {
				foreach (Image<Argb32> image in images) {
					image.Dispose();
				}
			}
		}
		#endregion

		#region Compress with progress
		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> CompressSet(Algorithm algorithm, IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) {
			return algorithm.CompressAsync(set, progress);
		}

		public static async Task CompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, string resultPath, IProgress<float> progress) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			IReadOnlyCollection<Image<Argb32>> images = set.CollectionSelect(Image.Load<Argb32>);

			try {
				foreach ((Task<Image<Argb32>> Image, Progress<float> ImageProgress) in CompressSet(algorithm, images, progress)) {
					setPathEnumerator.MoveNext();
					await (await Image).SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
				}
			} finally {
				foreach (var image in images) {
					image.Dispose();
				}
			}
		}

		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> CompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, IProgress<float> progress) {
			IReadOnlyCollection<Image<Argb32>> images = set.CollectionSelect(Image.Load<Argb32>);
			try {
				foreach (var image in CompressSet(algorithm, images, progress)) {
					yield return image;
				}
			} finally {
				foreach (Image<Argb32> image in images) {
					image.Dispose();
				}
			}
		}
		#endregion

		#region Decompress without progress
		public static IAsyncEnumerable<Image<Argb32>> DecompressSet(Algorithm algorithm, IEnumerable<Image<Argb32>> set) {
			return algorithm.DecompressAsync(set);
		}

		public static async Task DecompressSet(Algorithm algorithm, IEnumerable<string> set, string resultPath) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			IEnumerable<Image<Argb32>> images = set.Select(Image.Load<Argb32>);
			try {
				await foreach (Image<Argb32> image in DecompressSet(algorithm, images)) {
					setPathEnumerator.MoveNext();
					await image.SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
				}
			} finally {
				foreach (Image<Argb32> image in images) {
					image.Dispose();
				}
			}
		}

		public async static IAsyncEnumerable<Image<Argb32>> DecompressSet(Algorithm algorithm, IEnumerable<string> set) {
			IEnumerable<Image<Argb32>> images = set.Select(Image.Load<Argb32>);
			try {
				await foreach (Image<Argb32> image in DecompressSet(algorithm, images)) {
					yield return image;
				}
			} finally {
				foreach (Image<Argb32> image in images) {
					image.Dispose();
				}
			}
		}
		#endregion

		#region Decompress with progress
		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> DecompressSet(Algorithm algorithm, IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) {
			return algorithm.DecompressAsync(set, progress);
		}

		public static async Task DecompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, string resultPath, IProgress<float> progress) {
			using IEnumerator<string> setPathEnumerator = set.GetEnumerator();

			IReadOnlyCollection<Image<Argb32>> images = set.CollectionSelect(Image.Load<Argb32>);

			try {
				foreach ((Task<Image<Argb32>> Image, Progress<float> ImageProgress) in DecompressSet(algorithm, images, progress)) {
					setPathEnumerator.MoveNext();
					await (await Image).SaveAsync(Path.Combine(resultPath, Path.GetFileName(setPathEnumerator.Current)));
				}
			} finally {
				foreach (var image in images) {
					image.Dispose();
				}
			}
		}

		public static IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> DecompressSet(Algorithm algorithm, IReadOnlyCollection<string> set, IProgress<float> progress) {
			IReadOnlyCollection<Image<Argb32>> images = set.CollectionSelect(Image.Load<Argb32>);
			try {
				foreach (var image in DecompressSet(algorithm, images, progress)) {
					yield return image;
				}
			} finally {
				foreach (Image<Argb32> image in images) {
					image.Dispose();
				}
			}
		}
		#endregion
	}
}
