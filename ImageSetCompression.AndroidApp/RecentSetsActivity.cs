using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace ImageSetCompression.AndroidApp {
	[Activity(Label = "Recent sets")]
	public class RecentSetsActivity : Activity {
		private RecyclerView RecyclerView { get; set; }

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_recent_sets);

			var recyclerView = FindViewById<RecyclerView>(Resource.Id.listRecentSets);
			recyclerView.HasFixedSize = true;
			recyclerView.SetLayoutManager(new LinearLayoutManager(this));
			recyclerView.SetAdapter(new RecentSetsListAdapter(() => ImageSet.GetRecentSets(ApplicationContext), this));
		}

		private class RecentSetsListAdapter : RecyclerView.Adapter {
			private IList<IReadOnlyCollection<string>> m_RecentSets;
			private readonly Func<IList<IReadOnlyCollection<string>>> m_GetRecentSets;
			private readonly RecentSetsActivity m_Activity;

			public RecentSetsListAdapter(Func<IList<IReadOnlyCollection<string>>> getRecentSets, RecentSetsActivity activity) {
				m_RecentSets = getRecentSets();
				m_GetRecentSets = getRecentSets;
				m_Activity = activity;
			}

			public override int ItemCount => m_RecentSets.Count;

			public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
				return new ViewHolder(m_Activity, LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_recent_sets_item, parent, false));
			}

			public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
				((ViewHolder) holder).Bind(m_RecentSets[position]);
			}

			private void UpdateSet() {
				m_RecentSets = m_GetRecentSets();
			}

			private class ViewHolder : RecyclerView.ViewHolder {
				public ImageView Image { get; }
				public TextView Name { get; }
				public ImageButton DeleteButton { get; }
				public IReadOnlyCollection<string> Set { get; private set; }

				public ViewHolder(RecentSetsActivity activity, View root) : base(root) {
					Image = root.FindViewById<ImageView>(Resource.Id.recentSetItem_Image);
					Name = root.FindViewById<TextView>(Resource.Id.recentSetItem_Name);
					DeleteButton = root.FindViewById<ImageButton>(Resource.Id.recentSetItem_Delete);

					DeleteButton.Click += (o, e) => {
						ImageSet.RemoveRecentSet(activity.ApplicationContext, Set);
						((RecentSetsListAdapter) activity.RecyclerView.GetAdapter()).UpdateSet();
						activity.RecyclerView.GetAdapter().NotifyItemRemoved(AdapterPosition);
					};

					root.Click += (o, e) => {
						var data = new Intent() {
							ClipData = Set.Select(item => new ClipData.Item(item)).ToClipData("Recent set", "text/plain")
						};
						activity.SetResult(Result.Ok, data);
						activity.Finish();
					};
				}

				public void Bind(IReadOnlyCollection<string> set) {
					Set = set;
					Image.SetImageBitmap(BitmapFactory.DecodeFile(set.First()));
					Name.Text = System.IO.Path.GetFileNameWithoutExtension(set.First());
				}
			}
		}
	}
}
