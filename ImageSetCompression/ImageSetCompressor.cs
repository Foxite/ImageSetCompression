using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageSetCompression {
	public static class ImageSetCompressor {
		public static void CompressSet(IReadOnlyCollection<string> sourceImages, string resultPath) {
			if (sourceImages is null) {
				throw new ArgumentNullException(nameof(sourceImages));
			}
			if (sourceImages.Count < 2) {
				throw new ArgumentException("A set must consist of at least 2 items.");
			}
			if (sourceImages.Select(path => Path.GetFileName(path)).Distinct().Count() != sourceImages.Count) {
				throw new ArgumentException("The set contains two or more files with the same names. File names must be unique as the result files will be placed in the same folder.");
			}
			/*
			if (sourceImages.Select(image => image.Value.Size).Distinct().Count() > 1) {
				throw new ArgumentException("All images must be the same size.");
			}//*/

			string baseImagePath = sourceImages.First();
			using Bitmap baseImage = new Bitmap(baseImagePath);

			foreach (string itemPath in sourceImages.Skip(1)) {
				using var deltaImage = CompressImageInternal(itemPath, baseImage);

				deltaImage.Save(Path.Combine(resultPath, Path.GetFileName(itemPath)));
			}
		}

		public static Bitmap CompressImage(string baseImagePath, string variantImagePath) {
			using var baseImage = new Bitmap(baseImagePath);
			return CompressImageInternal(variantImagePath, baseImage);
		}

		private static Bitmap CompressImageInternal(string variantImagePath, Bitmap baseImage) {
			using var variantImage = new Bitmap(variantImagePath);
			var deltaImage = new Bitmap(variantImage.Width, variantImage.Height);

			for (int x = 0; x < deltaImage.Width; x++) {
				for (int y = 0; y < deltaImage.Height; y++) {
					deltaImage.SetPixel(x, y, GetColorDelta(baseImage.GetPixel(x, y), variantImage.GetPixel(x, y)));
				}
			}
			return deltaImage;
		}

		public static Bitmap DecompressImageInternal(string deltaImagePath, Bitmap baseImage) {
			using var deltaImage = new Bitmap(deltaImagePath);
			var resultImage = new Bitmap(baseImage.Width, baseImage.Height);

			for (int x = 0; x < deltaImage.Width; x++) {
				for (int y = 0; y < deltaImage.Height; y++) {
					resultImage.SetPixel(x, y, GetVariantColor(baseImage.GetPixel(x, y), deltaImage.GetPixel(x, y)));
				}
			}
			return resultImage;
		}

		public static Bitmap DecompressImage(string baseImagePath, string deltaImagePath) {
			using var baseImage = new Bitmap(baseImagePath);
			return DecompressImageInternal(deltaImagePath, baseImage);
		}

		public static void DecompressImageSet(string baseImagePath, IReadOnlyCollection<string> sourceImages, string resultPath) {
			using Bitmap baseImage = new Bitmap(baseImagePath);

			foreach (string itemPath in sourceImages.Skip(1)) {
				using Bitmap resultImage = DecompressImageInternal(itemPath, baseImage);

				resultImage.Save(Path.Combine(resultPath, Path.GetFileName(itemPath)));
			}
		}

		private static Color GetColorDelta(Color baseColor, Color variantColor) {
			unchecked {
				return Color.FromArgb((byte) (variantColor.A - baseColor.A), (byte) (variantColor.R - baseColor.R), (byte) (variantColor.G - baseColor.G), (byte) (variantColor.B - baseColor.B));
			}
		}

		private static Color GetVariantColor(Color baseColor, Color colorDelta) {
			unchecked {
				return Color.FromArgb((byte) (baseColor.A + colorDelta.A), (byte) (baseColor.R + colorDelta.R), (byte) (baseColor.G + colorDelta.G), (byte) (baseColor.B + colorDelta.B));
			}
		}
	}
}
