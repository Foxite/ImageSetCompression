using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using AndroidX.Preference;
using Newtonsoft.Json;

namespace ImageSetCompression.AndroidApp {
	internal static class ImageSet {
		private const string PreferenceKey = "recentsets";

		public static IList<IReadOnlyCollection<string>> GetRecentSets(Context ctx) =>
			JsonConvert.DeserializeObject<IList<IReadOnlyCollection<string>>>(PreferenceManager.GetDefaultSharedPreferences(ctx).GetString(PreferenceKey, "[]"));

		public static void AddRecentSet(Context ctx, IReadOnlyCollection<string> set) {
			ModifyRecentSets(ctx, oldSets => {
				int index = oldSets.IndexOf(otherSet => otherSet.SequenceEqual(set));
				if (index != -1) {
					// Move it to the top of the list
					oldSets.RemoveAt(index);
				}
				oldSets.Add(set);
			});
		}

		public static void RemoveRecentSet(Context ctx, IReadOnlyCollection<string> set) {
			ModifyRecentSets(ctx, oldSets => oldSets.RemoveAt(oldSets.IndexOf(otherSet => otherSet.SequenceEqual(set))));
		}

		private static void ModifyRecentSets(Context ctx, Action<IList<IReadOnlyCollection<string>>> func) {
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(ctx);

			IList<IReadOnlyCollection<string>> sets = JsonConvert.DeserializeObject<IList<IReadOnlyCollection<string>>>(prefs.GetString(PreferenceKey, "[]"));
			func(sets);

			prefs.Edit()
				.PutString(PreferenceKey, JsonConvert.SerializeObject(sets))
				.Commit();
		}
	}
}
