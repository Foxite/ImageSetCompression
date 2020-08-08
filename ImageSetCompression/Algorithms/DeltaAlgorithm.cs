using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression.Algorithms {
	public class DeltaAlgorithm : PixelAlgorithm {
		public override string Name => "Delta";

		protected override Argb32 GetDeltaColor(Argb32 baseColor, Argb32 variantColor) {
			unchecked {
				return new Argb32((byte) (variantColor.R - baseColor.R), (byte) (variantColor.G - baseColor.G), (byte) (variantColor.B - baseColor.B), (byte) (variantColor.A - baseColor.A));
			}
		}

		protected override Argb32 GetVariantColor(Argb32 baseColor, Argb32 deltaColor) {
			unchecked {
				return new Argb32((byte) (baseColor.R + deltaColor.R), (byte) (baseColor.G + deltaColor.G), (byte) (baseColor.B + deltaColor.B), (byte) (baseColor.A + deltaColor.A));
			}
		}
	}
}
