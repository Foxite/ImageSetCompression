using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageSetCompression.Algorithms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression {
	public abstract class Algorithm {
		public static Algorithm Delta { get; } = new DeltaAlgorithm();
		public static Algorithm Overlay { get; } = new OverlayAlgorithm();

		public static IReadOnlyList<Algorithm> BuiltInAlgorithms { get; } = new[] { Delta, Overlay };

		public abstract string Name { get; }

		public abstract IAsyncEnumerable<Image<Argb32>> CompressAsync(IEnumerable<Image<Argb32>> set);
		public abstract IAsyncEnumerable<Image<Argb32>> DecompressAsync(IEnumerable<Image<Argb32>> set);
		public abstract IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> CompressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> globalProgress);
		public abstract IEnumerable<(Task<Image<Argb32>> Image, Progress<float> ImageProgress)> DecompressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> globalProgress);
	}
}
