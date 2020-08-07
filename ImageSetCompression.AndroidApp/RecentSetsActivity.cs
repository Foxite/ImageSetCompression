using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using AndroidX.RecyclerView.Widget;

namespace ImageSetCompression.AndroidApp {
	[Activity(Label = "RecentSetsActivity")]
	public class RecentSetsActivity : Activity {
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_recent_sets);

			var prefs = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

			var recyclerView = FindViewById<RecyclerView>(Resource.Id.listRecentSets);
			recyclerView.HasFixedSize = true;
			recyclerView.SetLayoutManager(new LinearLayoutManager(this));
			recyclerView.SetAdapter(new RecentSetsListAdapter(new List<RecentSet>()));
		}

		private class RecentSetsListAdapter : RecyclerView.Adapter {
			private readonly IList<RecentSet> m_RecentSets;

			public RecentSetsListAdapter(IList<RecentSet> recentSets) {
				m_RecentSets = recentSets;
			}

			public override int ItemCount => m_RecentSets.Count;

			public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
				return new ViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_recent_sets_item, parent, false));
			}

			public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
				var vh = (ViewHolder) holder;
				vh.Image.SetImageBitmap(BitmapFactory.DecodeFile(m_RecentSets[position].BaseImagePath));
				vh.Name.Text = System.IO.Path.GetFileNameWithoutExtension(m_RecentSets[position].BaseImagePath);
			}

			private class ViewHolder : RecyclerView.ViewHolder {
				public ImageView Image { get; }
				public TextView Name { get; }

				public ViewHolder(View root) : base(root) {
					Image = root.FindViewById<ImageView>(Resource.Id.recentSetItem_Image);
					Name = root.FindViewById<TextView>(Resource.Id.recentSetItem_Name);
				}
			}
		}
	}
}