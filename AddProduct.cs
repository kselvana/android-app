
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
using System.Threading.Tasks;
using Android.Net;
using MySql.Data.MySqlClient;
using System.Threading;

namespace OnlineShop
{
	[Activity (Label = "Add Product")]			
	public class AddProduct : Activity
	{
		private string mysqlHost = "fiveminutes2town.c3hloudbtc3w.eu-west-1.rds.amazonaws.com";
		private string mysqlUser = "Minutes2Town";
		private string mysqlPassword = "9ZpwUvLNd7Rzzt";
		private uint mysqlPort = 3306;
		private string mysqlDatabase = "FiveMinutesToTown";

		private int storeId = 0;
		private int productId = 0;
		private int categoryId = 0;
		private int caseConfigurationId = 0;
		private List<Store> storeList = new List<Store> ();
		private List<Category> categoryList = new List<Category> ();
		private List<Product> productList = new List<Product> ();
		private List<CaseConfiguration> caseConfigurationList = new List<CaseConfiguration> ();

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView(Resource.Layout.AddProduct);

			Button btnSubmit = FindViewById<Button> (Resource.Id.btnProductSubmit);
			btnSubmit.Click += BtnSubmit_Click;

			Button btnAddAnother = FindViewById<Button> (Resource.Id.btnProductSubmitAndAddAnother);
			btnAddAnother.Click += BtnAddAnother_Click;

			Button btnExit = FindViewById<Button> (Resource.Id.btnProductExit);
			btnExit.Click += BtnExit_Click;

			PopulateStores ();
			PopulateCategories ();
			PopulateCaseConfigurations ();
		}

		private void BtnAddAnother_Click (object sender, EventArgs e)
		{
			Save (true);	
		}

		void BtnExit_Click (object sender, EventArgs e)
		{
			Sync ();
			Finish ();
		}

		private void PopulateCaseConfigurations()
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));	 
			caseConfigurationList = conn.Query<CaseConfiguration> ("SELECT * FROM CaseConfiguration");

			Spinner spinner = FindViewById<Spinner> (Resource.Id.spnProductCaseConfiguration);

			var adapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleSpinnerItem, caseConfigurationList);
			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.ItemSelected += Spinner_ItemSelectedCaseConfiguration;
            spinner.Adapter = adapter;
		}

		private void Spinner_ItemSelectedCaseConfiguration (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;
			caseConfigurationId = this.caseConfigurationList.ElementAt (e.Position).Id;
		}

		private void PopulateStores()
		{			
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));	 
			if (!MainActivity.TableExists<Store> (conn)) {
				Toast.MakeText (this, "You need to add a store", ToastLength.Short);
				Task.Delay (2000).Wait ();
				Finish ();
				return;
			}
			storeList = conn.Query<Store> ("SELECT * FROM Store Where UserName='"+User.UserName+"'");
 
			Spinner spinner = FindViewById<Spinner> (Resource.Id.spnProductStore);

			var adapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleSpinnerItem, storeList);
			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.ItemSelected += Spinner_ItemSelected;
			spinner.Adapter = adapter;
		}

		private void Spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;
			storeId = this.storeList.ElementAt (e.Position).Id;
		}

		private void PopulateCategories()
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));	 
			categoryList = conn.Query<Category> ("SELECT * FROM Category");
 
			Spinner spinner = FindViewById<Spinner> (Resource.Id.spnProductCategory);

			var adapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleSpinnerItem, categoryList);
			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.ItemSelected += Spinner_ItemSelectedCategory;
			spinner.Adapter = adapter;
		}

		private void Spinner_ItemSelectedCategory (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;
			categoryId = this.categoryList.ElementAt (e.Position).Id;

			PopulateProducts (categoryId);
		}

		private void BtnSubmit_Click (object sender, EventArgs e)
		{
			Save (false);
		}

		private void PopulateProducts(int categoryId)
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));	 
			productList = conn.Query<Product> ("SELECT * FROM Product WHERE CategoryId="+categoryId.ToString());

			Spinner spinner = FindViewById<Spinner> (Resource.Id.spnProductProduct);

			var adapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleSpinnerItem, productList);
			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.ItemSelected += Spinner_ItemSelectedProduct;
			spinner.Adapter = adapter;
		}

		private void Spinner_ItemSelectedProduct (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;
			productId = this.productList.ElementAt (e.Position).Id;
		}

		private void Save(bool addAnother)
		{
			EditText amount = FindViewById<EditText> (Resource.Id.txtProductAmount);
			Spinner spinnerStore = FindViewById<Spinner> (Resource.Id.spnProductStore);
			Spinner spinnerCategory = FindViewById<Spinner> (Resource.Id.spnProductCategory);
			Spinner spinnerProduct = FindViewById<Spinner> (Resource.Id.spnProductProduct);
            Spinner spinnerCaseConfig = FindViewById<Spinner>(Resource.Id.spnProductCaseConfiguration);

            string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<StoreProduct>();
		

			StoreProduct sp = new StoreProduct ();
			sp.UserName = User.UserName;
			int i = 0;
			Int32.TryParse (amount.Text,out i);

			sp.Amount = i;
			sp.StoreId = storeId;
			sp.CategoryId = categoryId;
			sp.ProductId = productId;
			sp.TimeStamp = DateTime.Now;
			sp.StoreName = spinnerStore.SelectedItem.ToString ();
            sp.CaseConfigurationId = caseConfigurationId;

			try
			{
				conn.Insert (sp);

				Toast.MakeText(this, "Product for this store successfully added", ToastLength.Long).Show();
				Task.Delay (2000).Wait ();
				if(addAnother)
				{
					amount.Text = string.Empty;
					spinnerStore.SetSelection(0);
					spinnerCategory.SetSelection(0);
					spinnerProduct.SetSelection(0);
                    spinnerCaseConfig.SetSelection(0);
				} else {
					Sync ();
					Finish ();
				}
			}
			catch(Exception ex)
			{
				if (ex.Message.ToLower().Equals("constraint")){
					Toast.MakeText(this, "Product for this store already exists", ToastLength.Long).Show();
				}
			}
		}

		private void Sync ()
		{
			ConnectivityManager connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
			NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
			bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

			if (isOnline) {
				Toast.MakeText (this, "Starting sync", ToastLength.Short).Show ();
				MySqlConnection sqlconn;
				new I18N.West.CP1250 ();
				MySqlConnectionStringBuilder sb= new MySqlConnectionStringBuilder();
				sb.Server = mysqlHost;
				sb.Port = mysqlPort;
				sb.Database = mysqlDatabase;
				sb.UserID = mysqlUser;
				sb.Password = mysqlPassword;
				sb.ConnectionTimeout = 600;
				sb.DefaultCommandTimeout = 600;
				string connsqlstring = sb.GetConnectionString(true);
				sqlconn = new MySqlConnection(connsqlstring);

				var progressDialog = ProgressDialog.Show(this, "Please wait...", "Syncing data...", true);

				new Thread(new ThreadStart(delegate
					{
						RunOnUiThread(() =>
							{
								try
								{
									sqlconn.Open();

									if (UpdateToServerPushData(sqlconn))
									{
										UpdateFromServerGetData(sqlconn);
									}
									sqlconn.Close();
								}
								catch (MySqlException ex)
								{
									Toast.MakeText(this, "Unable to upload data. Please try again later :" + ex.Message, ToastLength.Long).Show();
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
				conn.Execute ("DELETE FROM StoreProduct");
				Toast.MakeText (this, "Updated Products", ToastLength.Short).Show();

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

		private bool UpdateToServerPushData (MySqlConnection sqlconn)
		{

			//SQlite connection
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));

			InsertUser (sqlconn, conn);
			InsertStore (sqlconn, conn);
			InsertOrders (sqlconn, conn);

			conn.Close ();

			Toast.MakeText (this, "Done uploading data to cloud", ToastLength.Long).Show();
			return true;
		}

		private bool InsertUser(MySqlConnection mySqlCon, SQLiteConnection sqliteCon)
		{
			string sql = "INSERT INTO RegisterUser (UserName, Name, Surname, Password, TimeStamp)" +
				"VALUES (@UserName, @Name, @Surname, @Password, @TimeStamp) " +
				"ON DUPLICATE KEY UPDATE " +
				"UserName = VALUES(UserName), " +
				"Name = VALUES(Name), " +
				"Surname = VALUES(Surname), " +
				"Password = VALUES(Password), " +
				"TimeStamp = VALUES(TimeStamp); ";
			sqliteCon.CreateTable<RegisterUser> ();
			var query = sqliteCon.Table<RegisterUser> ();

			if (query.Count() <= 0)
				return true;

			MySqlTransaction tr = null;
			//tr = mySqlCon.BeginTransaction();
			try
			{
				foreach(var user in query)
				{
					MySqlCommand cmd = new MySqlCommand();
					cmd.Connection = mySqlCon;
					//cmd.Transaction = tr;
					cmd.CommandText = sql;
					cmd.Prepare();

					cmd.Parameters.AddWithValue("@UserName", user.UserName);
					cmd.Parameters.AddWithValue("@Name", user.Name);
					cmd.Parameters.AddWithValue("@Surname", user.Surname);
					cmd.Parameters.AddWithValue("@Password", user.Password);
					cmd.Parameters.AddWithValue("@TimeStamp", user.TimeStamp);

					cmd.ExecuteNonQuery();
				}

				//tr.Commit();

				Toast.MakeText(this, "All registered users information sent to cloud", ToastLength.Short).Show();
			}
			catch(MySqlException ex)
			{
				//	tr.Rollback();
				Toast.MakeText (this, "An error occured : " + ex.Message, ToastLength.Long).Show();
			}
			finally
			{

			}
			return true;
		}

		private bool InsertStore(MySqlConnection mySqlCon, SQLiteConnection sqliteCon)
		{
			string sql = "INSERT INTO Store (StoreName, StoreOwner, PhoneNumber, StoreTypeId, UserName, StoreAddress, StoreCoordinates, TimeStamp)" +
				"VALUES (@StoreName, @StoreOwner, @PhoneNumber, @StoreTypeId, @UserName, @StoreAddress, @StoreCoordinates, @TimeStamp) " +
				"ON DUPLICATE KEY UPDATE " +
				"StoreName = VALUES(StoreName), " +
				"StoreOwner = VALUES(StoreOwner), " +
				"PhoneNumber = VALUES(PhoneNumber), " +
				"StoreTypeId = VALUES(StoreTypeId), " +
				"UserName = VALUES(UserName), "+
				"StoreAddress = VALUES(StoreAddress), " +
				"StoreCoordinates = VALUES(StoreCoordinates), " +
				"TimeStamp = VALUES(TimeStamp); ";
			sqliteCon.CreateTable<Store> ();
			var query = sqliteCon.Table<Store> ();

			if (query.Count() <= 0)
				return true;

			MySqlTransaction tr = null;
			//	tr = mySqlCon.BeginTransaction();
			try
			{
				foreach(var store in query)
				{
					MySqlCommand cmd = new MySqlCommand();
					cmd.Connection = mySqlCon;
					cmd.CommandText = sql;
					//	cmd.Transaction = tr;
					cmd.Prepare();

					cmd.Parameters.AddWithValue("@StoreName", store.StoreName);
					cmd.Parameters.AddWithValue("@StoreOwner", store.StoreOwner);
					cmd.Parameters.AddWithValue("@PhoneNumber", store.PhoneNumber);
					cmd.Parameters.AddWithValue("@StoreTypeId", store.StoreTypeId);
					cmd.Parameters.AddWithValue("@UserName", store.UserName);
					cmd.Parameters.AddWithValue("@StoreAddress", store.StoreAddress);
					cmd.Parameters.AddWithValue("@StoreCoordinates", store.StoreCoordinates);
					cmd.Parameters.AddWithValue("@TimeStamp", store.TimeStamp);

					cmd.ExecuteNonQuery();
				}

				//	tr.Commit();

				Toast.MakeText(this, "All stores information sent to cloud", ToastLength.Short).Show();
			}
			catch(MySqlException ex)
			{
				//	tr.Rollback();
				Toast.MakeText (this, "An error occured : " + ex.Message, ToastLength.Long).Show();
			}
			finally
			{

			}
			return true;
		}

		private bool InsertOrders(MySqlConnection mySqlCon, SQLiteConnection sqliteCon)
		{
			string sql = "INSERT INTO Orders (UserName, StoreId, CategoryId, ProductId, Amount, TimeStamp, StoreName, CaseConfigurationId) " +
				"VALUES (@UserName, @StoreId, @CategoryId,@ProductId, @Amount, @TimeStamp, @StoreName, @CaseConfigurationId); ";

			sqliteCon.CreateTable<StoreProduct> ();
			var query = sqliteCon.Table<StoreProduct> ();

			if (query.Count() <= 0)
				return true;

			MySqlTransaction tr = null;
			//tr = mySqlCon.BeginTransaction();
			try
			{
				foreach(var order in query)
				{
					MySqlCommand cmd = new MySqlCommand();
					cmd.Connection = mySqlCon;
					//cmd.Transaction = tr;
					cmd.CommandText = sql;
					cmd.Prepare();

					cmd.Parameters.AddWithValue("@UserName", order.UserName);
					cmd.Parameters.AddWithValue("@StoreId", order.StoreId);
					cmd.Parameters.AddWithValue("@CategoryId", order.CategoryId);
					cmd.Parameters.AddWithValue("@ProductId", order.ProductId);
					cmd.Parameters.AddWithValue("@Amount", order.Amount);
					cmd.Parameters.AddWithValue("@TimeStamp", order.TimeStamp);
					cmd.Parameters.AddWithValue("@StoreName", order.StoreName);
					cmd.Parameters.AddWithValue("@CaseConfigurationId", order.CaseConfigurationId);

					cmd.ExecuteNonQuery();
				}

				//tr.Commit();

				Toast.MakeText(this, "All orders information sent to cloud", ToastLength.Short).Show();
			}
			catch(MySqlException ex)
			{
				//	tr.Rollback();
				Toast.MakeText (this, "An error occurred : " + ex.Message, ToastLength.Long).Show();
			}
			finally
			{

			}
			return true;
		}
	}
}

