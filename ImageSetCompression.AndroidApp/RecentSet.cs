using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Java.Lang;

namespace ImageSetCompression.AndroidApp {
	public class RecentSet : Object, IParcelable {
		public string BaseImagePath { get; }
		public IReadOnlyCollection<string> SetImages { get; }

		public static readonly IParcelableCreator CREATOR = new Creator();

		public RecentSet(string baseImagePath, IReadOnlyCollection<string> setImages) {
			BaseImagePath = baseImagePath;
			SetImages = setImages;
		}

		public int DescribeContents() => throw new System.NotImplementedException(); // TODO what is this

		public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags) {
			dest.WriteString(BaseImagePath);
			dest.WriteInt(SetImages.Count);
			dest.WriteStringArray(SetImages.ToArray());
		}

		private class Creator : Object, IParcelableCreator {
			public Object CreateFromParcel(Parcel source) {
				string baseImagePath = source.ReadString();
				string[] setImages = new string[source.ReadInt()];
				source.ReadStringArray(setImages);
				return new RecentSet(baseImagePath, setImages);
			}

			public Object[] NewArray(int size) => throw new System.NotImplementedException(); // TODO what is this
		}
	}
}
