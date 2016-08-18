
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using SQLite;
using System.Threading.Tasks;
using Android.Locations;

namespace OnlineShop
{
	[Activity (Label = "Add Store")]			
	public class AddStore : Activity, ILocationListener 
	{
		public static readonly int PickImageId = 1000;
		private ImageView _imageView;
		private int storeTypeId = 0;
		private List<StoreType> storeTypeList = new List<StoreType> ();
 
		TextView _addressText;
		Location _currentLocation;
		LocationManager _locationManager;

		string _locationProvider;
		TextView _locationText;
		bool gpsEnabled = false;

		protected override void OnCreate (Bundle savedInstanceState)
		{
 			base.OnCreate (savedInstanceState);

			SetContentView(Resource.Layout.StoreDetails);

			Button buttonAddress = FindViewById<Button> (Resource.Id.btnGetAddress);
			buttonAddress.Visibility = ViewStates.Gone;
			buttonAddress.Click += ButtonAddress_Click;

			Button btnSubmit = FindViewById<Button> (Resource.Id.btnStoreSubmit);
			btnSubmit.Click += BtnSubmit_Click;

			PopulateStoreTypes ();

			_addressText = FindViewById<TextView>(Resource.Id.txtStoreAddress);
			_locationText = FindViewById<TextView>(Resource.Id.txtStoreCoordinates);

			LocationManager mlocManager = (LocationManager) GetSystemService(LocationService);;  
			gpsEnabled = mlocManager.IsProviderEnabled(LocationManager.GpsProvider);  

			InitializeLocationManager();
		}

		private void BtnSubmit_Click (object sender, EventArgs e)
		{
			EditText storeName = FindViewById<EditText> (Resource.Id.txtStoreName);
			EditText storeOwner = FindViewById<EditText> (Resource.Id.txtStoreOwnerName);
			EditText storePhoneNumber = FindViewById<EditText> (Resource.Id.txtStorePhoneNumber);
			EditText storeAddress = FindViewById<EditText> (Resource.Id.txtStoreAddress);
			EditText storeCoordinates = FindViewById<EditText> (Resource.Id.txtStoreCoordinates);

			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<Store>();

			//Check if store exists for the current user
			var query = conn.Table<Store> ()
				.Where (st => st.UserName == User.UserName)
				.Where (st => st.StoreName == storeName.Text);

			if (query.Count() > 0) {
				Toast.MakeText(this, "Store already exists for this user", ToastLength.Long).Show();
				return;
			}

			Store s = new Store ();
			s.Id = GetNewStoreId ();
			s.UserName = User.UserName;
			s.StoreName = storeName.Text;
			s.StoreOwner = storeOwner.Text;
			s.PhoneNumber = storePhoneNumber.Text;
			s.StoreTypeId = storeTypeId;
			s.StoreAddress = storeAddress.Text;
			s.StoreCoordinates = storeCoordinates.Text;
			s.TimeStamp = DateTime.Now;

			try
			{
				conn.Insert (s);

				Toast.MakeText(this, "Store successfully added", ToastLength.Long).Show();
				Task.Delay (2000).Wait ();
				Finish ();
			}
			catch(Exception ex)
			{
				if (ex.Message.ToLower().Equals("constraint")){
					Toast.MakeText(this, "Store already exists", ToastLength.Long).Show();
				}
			}				
		}

		private int GetNewStoreId()
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<Store>();

			//Check if store exists for the current user
			var highest = conn.Table<Store>()
				.OrderByDescending(rs => rs.Id)
				.FirstOrDefault();

			if (highest != null) {			
				return highest.Id + 1;
			}else {
				return 1;
			}
		}

		private async void ButtonAddress_Click (object sender, EventArgs e)
		{
			try
			{
				if (_currentLocation == null)
				{
					_addressText.Text = "Can't determine the current address. Try again in a few minutes.";
					return;
				}

				//Address address = ReverseGeocodeCurrentLocation();
				//DisplayAddress(address);
			}

			catch(Exception ex) {
				Toast.MakeText (this, "ButtonAddress_Click " + ex.Message, ToastLength.Long).Show ();
			}

			/*
			if (!string.IsNullOrEmpty (address.Text)) {
				var geoUri = Android.Net.Uri.Parse ("geo:0,0?q=" + WebUtility.UrlEncode(address.Text));
				var mapIntent = new Intent (Intent.ActionView, geoUri);
				StartActivity (mapIntent);
			} else {
				 
			}*/
		}

		public async void OnLocationChanged(Location location)
		{
			try
			{
				_currentLocation = location;
				if (_currentLocation == null)
				{
					_locationText.Text = "Unable to determine your location. Try again in a short while.";
				}
				else
				{
					_locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
					//Address address = ReverseGeocodeCurrentLocation();
					//DisplayAddress(address);
				}
			}
			catch(Exception ex) {
				Toast.MakeText (this, "OnLocationChanged " + ex.Message, ToastLength.Long).Show ();
			}
		}

		private Address ReverseGeocodeCurrentLocation()
		{
			try
			{
				Geocoder geocoder = new Geocoder(this);
				IList<Address> addressList =
					 geocoder.GetFromLocation(_currentLocation.Latitude, _currentLocation.Longitude, 10);

				Address address = addressList.FirstOrDefault();
				return address;
			}
			catch(Exception ex) {
					Toast.MakeText (this, "ReverseGeocodeCurrentLocation " + ex.Message, ToastLength.Long).Show ();
                return null;
			}
		}

		void DisplayAddress(Address address)
		{
			try
			{
				if (address != null)
				{
					StringBuilder deviceAddress = new StringBuilder();
					for (int i = 0; i < address.MaxAddressLineIndex; i++)
					{
						deviceAddress.AppendLine(address.GetAddressLine(i));
					}
					// Remove the last comma from the end of the address.
					_addressText.Text = deviceAddress.ToString();
				}
				else
				{
					_addressText.Text = "Unable to determine the address. Try again in a few minutes.";
				}
			}
			catch(Exception ex) {
				Toast.MakeText (this, "DisplayAddress " + ex.Message, ToastLength.Long).Show ();
			}
		}

		private void ButtonOnClick(object sender, EventArgs eventArgs)
		{
			Intent = new Intent();
			Intent.SetType("image/*");
			Intent.SetAction(Intent.ActionGetContent);
			StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
			{
				Android.Net.Uri uri = data.Data;
				_imageView.SetImageURI(uri);
			}
		}

		private void PopulateStoreTypes()
		{
			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));	 
			storeTypeList = conn.Query<StoreType> ("SELECT * FROM StoreType");
 
			Spinner spinner = FindViewById<Spinner> (Resource.Id.spnStoreTypeOfStore);
	 
			var adapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleSpinnerItem, storeTypeList);
			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.ItemSelected += Spinner_ItemSelected;
			spinner.Adapter = adapter;
		}

		void Spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;
			storeTypeId = this.storeTypeList.ElementAt (e.Position).Id;
		}

		void InitializeLocationManager()
		{
			_locationManager = (LocationManager) GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = string.Empty;
			}
		} 

		public void OnProviderDisabled(string provider) {}

		public void OnProviderEnabled(string provider) {}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}

		protected override void OnResume()
		{
			try 
			{
				base.OnResume();
				if (gpsEnabled) {
					_locationManager.RequestLocationUpdates (_locationProvider, 0, 0, this);
				}
			}
			catch(Exception ex) {
				Toast.MakeText (this, "OnResume " + ex.Message, ToastLength.Long).Show ();
			}
		}

		protected override void OnPause()
		{
			try
			{
				base.OnPause();
				if (gpsEnabled) {
					_locationManager.RemoveUpdates (this);
				}
			}
			catch(Exception ex) {
				Toast.MakeText (this, "OnPause " + ex.Message, ToastLength.Long).Show ();
			}
		}
	}
}

