
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
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Android.Net;
using System.Threading;

namespace OnlineShop
{
	[Activity (Label = "Welcome")]			
	public class Welcome : Activity
	{
		private string mysqlHost = "fiveminutes2town.c3hloudbtc3w.eu-west-1.rds.amazonaws.com";
		private string mysqlUser = "Minutes2Town";
		private string mysqlPassword = "9ZpwUvLNd7Rzzt";
		private uint mysqlPort = 3306;
		private string mysqlDatabase = "FiveMinutesToTown";

 		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView(Resource.Layout.Welcome);

			Sync ();

			Button button = FindViewById<Button> (Resource.Id.btnNewStore);
			Button btnOrderExisting = FindViewById<Button> (Resource.Id.btnOrderExisting);
			btnOrderExisting.Click += BtnOrderExisting_Click;;
			button.Click += delegate {
				StartActivity(typeof(AddStore));
			};

			Button btnViewAllOrders = FindViewById<Button> (Resource.Id.btnWelcomeViewAllOrders);
			btnViewAllOrders.Click += BtnViewAllOrders_Click;

            Button btnAbout = FindViewById<Button>(Resource.Id.btnWelcomeAbout);
            btnAbout.Click += BtnAbout_Click;

            Button btnAddStorePhoto = FindViewById<Button>(Resource.Id.btnWelcomeAddStorePhoto);
            btnAddStorePhoto.Click += BtnAddStorePhoto_Click;


		}

		private void Sync ()
		{
			ConnectivityManager connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
			NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
			bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

			if (isOnline) {
 
				MySqlConnection sqlconn;
				new I18N.West.CP1250 ();
				MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
				sb.Server = mysqlHost;
				sb.Port = mysqlPort;
				sb.Database = mysqlDatabase;
				sb.UserID = mysqlUser;
				sb.Password = mysqlPassword;
				sb.ConnectionTimeout = 600;
				sb.DefaultCommandTimeout = 600;
				string connsqlstring = sb.GetConnectionString(true);
				sqlconn = new MySqlConnection(connsqlstring);

				var progressDialog = ProgressDialog.Show(this, "Please wait...", "Downloading data...", true);

				new Thread(new ThreadStart(delegate
					{
						RunOnUiThread(() =>
							{
								try
								{
									sqlconn.Open();
 
									UpdateFromServerGetData(sqlconn);
 
									sqlconn.Close();
								}
								catch (MySqlException ex)
								{
									Toast.MakeText(this, "Unable to download data. Please try again later :" + ex.Message, ToastLength.Long).Show();
									sqlconn.Close();
								}
							}
						);
						RunOnUiThread(() => progressDialog.Hide());
					})).Start();
			} else {
				Toast.MakeText (this, "You need to be connected to the internet in order to sync", ToastLength.Long);
			}
			base.OnDestroy ();
		}

		public void UpdateFromServerGetData(MySqlConnection sqlconn)
		{
			//Todo: Test for internet connection
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<CaseConfiguration>();
			conn.CreateTable<Store>();
			conn.CreateTable<StoreType>();
			conn.CreateTable<Category>();
			conn.CreateTable<Product>();
			conn.CreateTable<StoreProduct> ();		

			try
			{

				conn.Execute ("DELETE FROM StoreType");
				//StoreTypes
				string queryStringStoreType = "select * from StoreType";
				MySqlCommand sqlcmd = new MySqlCommand(queryStringStoreType, sqlconn);
				MySqlDataReader rdrStoreType = sqlcmd.ExecuteReader();
				while (rdrStoreType.Read()) 
				{
					StoreType st = new StoreType();
					st.Id = rdrStoreType.GetInt32("Id");
					st.Description = rdrStoreType.GetString("Description");

					conn.Insert(st);
				}
				rdrStoreType.Close();
				Toast.MakeText (this, "Updated Store Types", ToastLength.Short).Show();

				conn.Execute("DELETE FROM CaseConfiguration");
				//Category
				string queryStringCC = "select * from CaseConfiguration";
				sqlcmd = new MySqlCommand(queryStringCC, sqlconn);
				MySqlDataReader rdrCC = sqlcmd.ExecuteReader();
				while (rdrCC.Read())
				{
					CaseConfiguration c = new CaseConfiguration();
					c.Id = rdrCC.GetInt32("Id");
					c.Description = rdrCC.GetString("Description");

					conn.Insert(c);
				}
				rdrCC.Close();
				Toast.MakeText(this, "Updated Case Configurations", ToastLength.Short).Show();
 
				conn.Execute ("DELETE FROM Category");
				//Category
				string queryStringCat = "select * from Category";
				sqlcmd = new MySqlCommand(queryStringCat, sqlconn);
				MySqlDataReader rdrCat = sqlcmd.ExecuteReader();
				while (rdrCat.Read()) 
				{
					Category c = new Category();
					c.Id = rdrCat.GetInt32("Id");
					c.Description = rdrCat.GetString("Description");

					conn.Insert(c);
				}
				rdrCat.Close();
				Toast.MakeText (this, "Updated Categories", ToastLength.Short).Show();

				conn.Execute ("DELETE FROM Product");
				//Products
				string queryStringProd = "select * from Product";
				sqlcmd = new MySqlCommand(queryStringProd, sqlconn);
				MySqlDataReader rdrProd = sqlcmd.ExecuteReader();
				while (rdrProd.Read()) 
				{
					Product p = new Product();
					p.Id = rdrProd.GetInt32("Id");
					p.Description = rdrProd.GetString("Description");
					p.CategoryId = rdrProd.GetInt32("CategoryId");
					conn.Insert(p);
				}
				rdrProd.Close();
 
				Toast.MakeText (this, "Updated Products", ToastLength.Short).Show();

				conn.Execute ("DELETE FROM Store");
				//Store
				string queryStringStore = "select * from Store";
				sqlcmd = new MySqlCommand(queryStringStore, sqlconn);
				MySqlDataReader rdrStore = sqlcmd.ExecuteReader();
				while (rdrStore.Read()) 
				{
					Store st = new Store();
					st.Id = rdrStore.GetInt32("Id");
					st.StoreName = rdrStore.GetString("StoreName");
					st.StoreOwner = rdrStore.GetString("StoreOwner");
					st.PhoneNumber = rdrStore.GetString("PhoneNumber");
					st.StoreTypeId = rdrStore.GetInt32("StoreTypeId");
					st.UserName = rdrStore.GetString("UserName");
					st.StoreAddress = rdrStore.GetString("StoreAddress");
					st.StoreCoordinates = rdrStore.GetString("StoreCoordinates");
					st.TimeStamp = rdrStore.GetDateTime("TimeStamp");
					st.UserName = rdrStore.GetString("UserName");

					conn.Insert(st);
				}
				rdrStore.Close();
				Toast.MakeText (this, "Updated Stores", ToastLength.Short).Show();


				//Toast.MakeText (this, "All data updated", ToastLength.Long).Show();
				new AlertDialog.Builder(this)
					.SetMessage("All data updated")
					.SetTitle("Info")
					.Show();
				//sqlconn.Close();
			} catch (Exception ex)
			{
				Toast.MakeText (this, "An error occured : " + ex.Message, ToastLength.Long).Show();
			}
		}

        private void BtnAddStorePhoto_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(StorePhoto));
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(AboutActivity));
        }

		private void BtnViewAllOrders_Click (object sender, EventArgs e)
		{
			StartActivity (typeof(OrdersViewActivity));
		}

		private void BtnOrderExisting_Click (object sender, EventArgs e)
		{
			StartActivity (typeof(AddProduct));
		}

	}
}

