using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression.Algorithms {
	public abstract class PixelAlgorithm : Algorithm {
		protected abstract Argb32 GetVariantColor(Argb32 baseColor, Argb32 deltaColor);
		protected abstract Argb32 GetDeltaColor(Argb32 baseColor, Argb32 variantColor);

		#region Without progress
		public async override IAsyncEnumerable<Image<Argb32>> CompressAsync(IEnumerable<Image<Argb32>> set) {
			Image<Argb32> baseImage = null;
			try {
				bool first = true;
				foreach (Image<Argb32> image in set) {
					if (first) {
						baseImage = image.Clone();
						yield return image;
					} else {
						Image<Argb32> overlayImage = null;

						await Task.Run(() => {
							overlayImage = new Image<Argb32>(image.Width, image.Height);
							for (int x = 0; x < overlayImage.Width; x++) {
								for (int y = 0; y < overlayImage.Height; y++) {
									overlayImage[x, y] = GetDeltaColor(baseImage[x, y], image[x, y]);
								}
							}
						});

						yield return overlayImage;
					}
				}
			} finally {
				if (baseImage != null) {
					baseImage.Dispose();
				}
			}
		}

		public async override IAsyncEnumerable<Image<Argb32>> DecompressAsync(IEnumerable<Image<Argb32>> set) {
			Image<Argb32> baseImage = null;
			try {
				bool first = true;
				foreach (Image<Argb32> image in set) {
					if (first) {
						baseImage = image.Clone();
						yield return image;
					} else {
						Image<Argb32> variantImage = null;

						await Task.Run(() => {
							variantImage = new Image<Argb32>(image.Width, image.Height, new Argb32(0, 0, 0, 0));

							for (int x = 0; x < image.Width; x++) {
								for (int y = 0; y < image.Height; y++) {
									if (image[x, y] != new Argb32(0, 0, 0, 0)) {
										variantImage[x, y] = GetVariantColor(baseImage[x, y], image[x, y]);
									}
								}
							}
						});

						yield return variantImage;
					}
				}
			} finally {
				if (baseImage != null) {
					baseImage.Dispose();
				}
			}
		}
		#endregion

		#region With progress
		public override IEnumerable<(Task<Image<Argb32>>, Progress<float>)> CompressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) {
			Image<Argb32> baseImage = null;
			try {
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
							var overlayImage = new Image<Argb32>(image.Width, image.Height);
							for (int x = 0, line = 0, lineProgress = 0; x < overlayImage.Width; x++, line++) {
								for (int y = 0; y < overlayImage.Height; y++) {
									overlayImage[x, y] = GetDeltaColor(baseImage[x, y], image[x, y]);
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
			} finally {
				if (baseImage != null) {
					baseImage.Dispose();
				}
			}
		}

		public override IEnumerable<(Task<Image<Argb32>>, Progress<float>)> DecompressAsync(IReadOnlyCollection<Image<Argb32>> set, IProgress<float> progress) {
			Image<Argb32> baseImage = null;
			try {
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
							Image<Argb32> variantImage = null;
							variantImage = new Image<Argb32>(image.Width, image.Height);
							for (int x = 0, line = 0, lineProgress = 0; x < image.Width; x++, line++) {
								for (int y = 0; y < image.Height; y++) {
									variantImage[x, y] = GetVariantColor(baseImage[x, y], image[x, y]);
								}

								// Report progress 100 times per image
								if (line > image.Width / 100) {
									line = 0;
									((IProgress<float>) imageProgress).Report(++lineProgress / 100f);
								}
							}
							return variantImage;
						}), imageProgress);
					}
					i++;
				}
			} finally {
				if (baseImage != null) {
					baseImage.Dispose();
				}
			}
		}
		#endregion
	}
}
