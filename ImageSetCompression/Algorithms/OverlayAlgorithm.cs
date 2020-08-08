using SixLabors.ImageSharp.PixelFormats;

namespace ImageSetCompression.Algorithms {
	public class OverlayAlgorithm : PixelAlgorithm {
		public override string Name => "Overlay";

		protected override Argb32 GetDeltaColor(Argb32 baseColor, Argb32 variantColor) => baseColor == variantColor ? new Argb32(0, 0, 0, 0) : variantColor;
		protected override Argb32 GetVariantColor(Argb32 baseColor, Argb32 deltaColor) => deltaColor == new Argb32(0, 0, 0, 0) ? baseColor : deltaColor;
	}
}
