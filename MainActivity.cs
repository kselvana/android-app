using Android.App;
using Android.Widget;
using Android.OS;
using SQLite;
using System.Collections.Generic;
using Android.Net;
using MySql.Data.MySqlClient;
using System.Threading;
using System;

namespace OnlineShop
{
	[Activity (Label = "Online Shop", MainLauncher = true, Icon = "@drawable/fivemins2town")]
	public class MainActivity : Activity
	{


		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it

			User.UserName = string.Empty;
			Button button = FindViewById<Button> (Resource.Id.btnSignIn);
			Button btnRegister = FindViewById<Button> (Resource.Id.btnRegister);
			btnRegister.Click += delegate  {
				StartActivity(typeof(RegisterActivity));
			};
			
			button.Click += delegate {
				if(Login()){
					StartActivity(typeof(Welcome));
				} else {
					Toast.MakeText(this, "Username or password invalid", ToastLength.Long).Show();
				}
			};

			//AddStoreTypes ();
			//AddProductCategories ();
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<Store>();
			conn.CreateTable<StoreType>();
			conn.CreateTable<Category>();
			conn.CreateTable<Product>();
			conn.CreateTable<StoreProduct> ();		
		}

		private bool Login()
		{
			EditText username = FindViewById<EditText> (Resource.Id.txtMainUserName);
			EditText pwd = FindViewById<EditText> (Resource.Id.txtMainPassword);

			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<RegisterUser>();

			var query = conn.Table<RegisterUser> ()
				.Where (v => v.UserName.ToLower().Equals (username.Text.ToLower()))
				.Where (v => v.Password.Equals (pwd.Text));

			if (query.Count () > 0) {
				User.UserName = username.Text;
				pwd.Text = string.Empty;
				return true;
			} else {
				return false;
			}
		}

		private void AddStoreTypes()
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<StoreType>();

			conn.Execute ("DELETE FROM StoreType");

			 
		}

		private void AddProductCategories()
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<Category>();

			conn.Execute ("DELETE FROM Category");

			InsertCategory (conn, new Category (){ Id = 0, Description = "Cigarettes" });
			InsertCategory (conn, new Category (){ Id = 1, Description = "Pipe Tobacco" });
			InsertCategory (conn, new Category (){ Id = 2, Description = "Cigarette Tobacco" });
			InsertCategory (conn, new Category (){ Id = 3, Description = "Snuff Leaf" });
			InsertCategory (conn, new Category (){ Id = 4, Description = "Snuff" });
			InsertCategory (conn, new Category (){ Id = 5, Description = "Cigarette Papers" });
			InsertCategory (conn, new Category (){ Id = 6, Description = "RYO/MYO Accessories" });
			InsertCategory (conn, new Category (){ Id = 7, Description = "Lighters" });
			InsertCategory (conn, new Category (){ Id = 8, Description = "Matches" });

		}

		public static int InsertStoreType (SQLiteConnection db, StoreType storeType)
		{
			return db.Execute ("INSERT INTO StoreType(Id, Description) SELECT " + storeType.Id + ", '" + storeType.Description + "' " +
			"WHERE NOT EXISTS(SELECT 1 FROM StoreType WHERE Description = '" + storeType.Description + "')");
		}

		public static int InsertCategory (SQLiteConnection db, Category category)
		{
			return db.Execute ("INSERT INTO Category(Id, Description) SELECT " + category.Id + ", '" + category.Description + "' " +
				"WHERE NOT EXISTS(SELECT 1 FROM Category WHERE Description = '" + category.Description + "')");
		}

		public static bool TableExists<T> (SQLiteConnection connection)
		{    
			const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
			var cmd = connection.CreateCommand (cmdText, typeof(T).Name);
			return cmd.ExecuteScalar<string> () != null;
		}


	}
}


