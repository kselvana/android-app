
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace OnlineShop
{
	[Activity (Label = "Orders History")]			
	public class OrdersViewActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.ViewOrdersListActivity);

			ListView listView = FindViewById<ListView> (Resource.Id.lvViewOrdersList);
			//listView.ItemClick += OnListItemClick;

			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			List<ViewOrder> listData = new List<ViewOrder> ();
			if (MainActivity.TableExists<StoreProduct>(conn)){
			  listData = conn.Query<ViewOrder> ("SELECT s.StoreName AS 'StoreName', p.Description AS 'ProductName', " +
                "sp.Amount AS 'Amount', sp.TimeStamp AS 'TimeStamp', c.Description AS 'CategoryName', cc.Description AS 'CaseConfigurationName' " +
				"FROM StoreProduct sp " +
				"LEFT JOIN Store s ON sp.StoreId = s.Id " +
				"LEFT JOIN Category c ON sp.CategoryId = c.Id " +
				"LEFT JOIN Product p ON sp.ProductId = p.Id " +
                "LEFT JOIN CaseConfiguration cc ON sp.CaseConfigurationId = cc.Id " +
				"WHERE sp.UserName = '"+User.UserName+"' ORDER BY 'TimeStamp' ASC ");
			}
			listView.Adapter = new CustomOrderListAdapter(this, listData);
		}
	}
}

