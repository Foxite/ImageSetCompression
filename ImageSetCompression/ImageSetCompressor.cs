using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression {
	public static class ImageSetCompressor {
		public static void CompressSet(string baseImagePath, IEnumerable<string> setImages, string resultPath) {
			using var baseImage = Image.Load<Argb32>(baseImagePath);
			foreach (string itemPath in setImages) {
				using var deltaImage = CompressImageInternal(itemPath, baseImage);

				deltaImage.Save(Path.Combine(resultPath, Path.GetFileName(itemPath)));
			}
		}

		public static Image<Argb32> CompressImage(string baseImagePath, string variantImagePath) {
			using var baseImage = Image.Load<Argb32>(baseImagePath);
			return CompressImageInternal(variantImagePath, baseImage);
		}

		private static Image<Argb32> CompressImageInternal(string variantImagePath, Image<Argb32> baseImage) {
			using var variantImage = Image.Load<Argb32>(variantImagePath);
			var deltaImage = new Image<Argb32>(variantImage.Width, variantImage.Height);

			for (int x = 0; x < deltaImage.Width; x++) {
				for (int y = 0; y < deltaImage.Height; y++) {
					deltaImage[x, y] = GetColorDelta(baseImage[x, y], variantImage[x, y]);
				}
			}
			return deltaImage;
		}

		public static Image<Argb32> DecompressImageInternal(string deltaImagePath, Image<Argb32> baseImage) {
			using var deltaImage = Image.Load<Argb32>(deltaImagePath);
			var resultImage = new Image<Argb32>(baseImage.Width, baseImage.Height);

			for (int x = 0; x < deltaImage.Width; x++) {
				for (int y = 0; y < deltaImage.Height; y++) {
					resultImage[x, y] = GetVariantColor(baseImage[x, y], deltaImage[x, y]);
				}
			}
			return resultImage;
		}

		public static Image<Argb32> DecompressImage(string baseImagePath, string deltaImagePath) {
			using var baseImage = Image.Load<Argb32>(baseImagePath);
			return DecompressImageInternal(deltaImagePath, baseImage);
		}

		public static void DecompressImageSet(string baseImagePath, IEnumerable<string> setImages, string resultPath) {
			using var baseImage = Image.Load<Argb32>(baseImagePath);
			foreach (string itemPath in setImages) {
				using Image<Argb32> resultImage = DecompressImageInternal(itemPath, baseImage);

				resultImage.Save(Path.Combine(resultPath, Path.GetFileName(itemPath)));
			}
		}

		private static Argb32 GetColorDelta(Argb32 baseColor, Argb32 variantColor) {
			unchecked {
				return new Argb32((byte) (variantColor.R - baseColor.R), (byte) (variantColor.G - baseColor.G), (byte) (variantColor.B - baseColor.B), (byte) (variantColor.A - baseColor.A));
			}
		}

		private static Argb32 GetVariantColor(Argb32 baseColor, Argb32 colorDelta) {
			unchecked {
				return new Argb32((byte) (baseColor.R + colorDelta.R), (byte) (baseColor.G + colorDelta.G), (byte) (baseColor.B + colorDelta.B), (byte) (baseColor.A + colorDelta.A));
			}
		}
	}
}
