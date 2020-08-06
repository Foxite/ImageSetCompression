using System;

namespace ImageSetCompression {
	public static class ProgressUtil {
		public static Progress<float> GetSubProgress(this IProgress<float> parent, int i, int count, float factor) =>
			new Progress<float>(p => parent.Report((float) i / count + p / count * factor));
	}
}
