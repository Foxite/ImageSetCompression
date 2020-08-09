using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression.Algorithms {
	public abstract class PixelAlgorithm : Algorithm {
		protected abstract Argb32 GetVariantColor(Argb32 baseColor, Argb32 deltaColor);
		protected abstract Argb32 GetDeltaColor(Argb32 baseColor, Argb32 variantColor);

		#region Without progress
		public override IAsyncEnumerable<Image<Argb32>> CompressAsync(IEnumerable<Image<Argb32>> set) => ProcessWithoutProgressAsync(set, GetDeltaColor);
		public override IAsyncEnumerable<Image<Argb32>> DecompressAsync(IEnumerable<Image<Argb32>> set) => ProcessWithoutProgressAsync(set, GetVariantColor);
		private async IAsyncEnumerable<Image<Argb32>> ProcessWithoutProgressAsync(IEnumerable<Image<Argb32>> set, Func<Argb32, Argb32, Argb32> compare) {
			Image<Argb32> baseImage = null;
			bool first = true;
			foreach (Image<Argb32> image in set) {
				if (first) {
					baseImage = image;
					yield return image;
				} else {
					Image<Argb32> overlayImage = null;

					await Task.Run(() => {
						overlayImage = new Image<Argb32>(image.Width, image.Height);
						for (int x = 0; x < overlayImage.Width; x++) {
							for (int y = 0; y < overlayImage.Height; y++) {
								overlayImage[x, y] = compare(baseImage[x, y], image[x, y]);
							}
						}
					});

					yield return overlayImage;
				}
			}
		}
		#endregion

		#region With progress
		public override IEnumerable<(Task<Image<Argb32>>, Progress<float>)> CompressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) => ProcessWithProgressAsync(set, progress, GetDeltaColor);
		public override IEnumerable<(Task<Image<Argb32>>, Progress<float>)> DecompressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) => ProcessWithProgressAsync(set, progress, GetVariantColor);
		private IEnumerable<(Task<Image<Argb32>>, Progress<float>)> ProcessWithProgressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress, Func<Argb32, Argb32, Argb32> compare) {
			Image<Argb32> baseImage = null;
			int i = 0;
			foreach (Image<Argb32> image in set) {
				if (i == 0) {
					baseImage = image.Clone();
					progress.Report(1.0f / set.Count);
					var fullProgress = new Progress<float>();
					((IProgress<float>) fullProgress).Report(1);
					yield return (Task.FromResult(image), fullProgress);
				} else {
					Progress<float> imageProgress = progress.GetSubProgress(i, set.Count, 1);

					yield return (Task.Run(() => {
						Image<Argb32> overlayImage;
						overlayImage = new Image<Argb32>(image.Width, image.Height);
						for (int x = 0, line = 0, lineProgress = 0; x < overlayImage.Width; x++, line++) {
							for (int y = 0; y < overlayImage.Height; y++) {
								overlayImage[x, y] = compare(baseImage[x, y], image[x, y]);
							}

							if (line > overlayImage.Width / 100) {
								line = 0;
								((IProgress<float>) imageProgress).Report(++lineProgress / 100f);
							}
						}
						return overlayImage;
					}), imageProgress);
				}
				i++;
			}
		}
		#endregion
	}
}
