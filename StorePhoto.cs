using System;
using System.Collections.Generic;
using Java.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using Android.Provider;
using Android.Content.PM;
using Android.Graphics;
using Amazon.S3.Transfer;
using System.Threading;
using Java.Lang;

namespace OnlineShop
{

	public static class App {
		public static File _file;
		public static File _dir;
		public static Bitmap bitmap;
	}

    [Activity(Label = "Store Photo")]
    public class StorePhoto : Activity
    {
        private string storeName = "";
        private List<Store> storeList = new List<Store>();

		ImageView _imageView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
 
            SetContentView(Resource.Layout.StorePhoto);

            PopulateStores();


			Button btn = FindViewById<Button>(Resource.Id.btnStorePhotoSelectImage);
			/* btn.Click += delegate {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(
                    Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };*/
			btn.Click += TakePictureClick;

			if (IsThereAnAppToTakePictures ())
			{
				CreateDirectoryForPictures ();

				Button button = FindViewById<Button>(Resource.Id.btnStorePhotoSelectImage);
				_imageView = FindViewById<ImageView>(Resource.Id.imgStorePhotoImage);
				button.Click += TakePictureClick;
			}

			Button btnUpload = FindViewById<Button> (Resource.Id.btnStorePhotoUpload);
			btnUpload.Click += UploadClick;
        }


        private void PopulateStores()
        {
            string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var conn = new SQLiteConnection(System.IO.Path.Combine(folder, "fiveminutes2town.db"));
            if (!MainActivity.TableExists<Store>(conn))
            {
                Toast.MakeText(this, "You need to add a store", ToastLength.Short);
                Task.Delay(2000).Wait();
                Finish();
                return;
            }
            storeList = conn.Query<Store>("SELECT * FROM Store Where UserName='" + User.UserName + "'");

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spnStorePhotoStore);

            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, storeList);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.ItemSelected += Spinner_ItemSelected;
            spinner.Adapter = adapter;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
			base.OnActivityResult (requestCode, resultCode, data);

			// Make it available in the gallery

			Intent mediaScanIntent = new Intent (Intent.ActionMediaScannerScanFile);
			Android.Net.Uri contentUri = Android.Net.Uri.FromFile (App._file);
			mediaScanIntent.SetData (contentUri);
			SendBroadcast (mediaScanIntent);

			// Display in ImageView. We will resize the bitmap to fit the display.
			// Loading the full sized image will consume to much memory
			// and cause the application to crash.

			int height = Resources.DisplayMetrics.HeightPixels;
			int width = _imageView.Width ;
			App.bitmap = App._file.Path.LoadAndResizeBitmap (width, height);
			if (App.bitmap != null) {
				_imageView.SetImageBitmap (App.bitmap);
				App.bitmap = null;
			}

			// Dispose of the Java side bitmap.
			GC.Collect();
        }

		private void CreateDirectoryForPictures ()
		{
			App._dir = new File (
				Android.OS.Environment.GetExternalStoragePublicDirectory (
					Android.OS.Environment.DirectoryPictures), "5Minutes2Town");
			if (!App._dir.Exists ())
			{
				App._dir.Mkdirs( );
			}
		}

		private bool IsThereAnAppToTakePictures ()
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities =
				PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
			Spinner spinner = (Spinner)sender;
			storeName = this.storeList.ElementAt (e.Position).StoreName;
        }

		private void TakePictureClick(object sender, EventArgs e)
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			App._file = new File (App._dir, System.String.Format("{0}_{1}_{2}.jpg",DateTime.Now.ToString("dd_MMM_yyyy_hh_mm_ss"), User.UserName, storeName));
			intent.PutExtra (MediaStore.ExtraOutput, Android.Net.Uri.FromFile (App._file));
			StartActivityForResult (intent, 0);
		}

        private async void UploadClick(object sender, EventArgs e)
        {
            if (App._file == null)
            {
                Toast.MakeText(this, "No file to upload", ToastLength.Short).Show();
                return;
            }
			 
			const string AWS_ACCESS_KEY = "AKIAJLAGR5U2KTXH5QDQ";
			const string AWS_SECRET_KEY = "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";
			AmazonS3Config config = new AmazonS3Config();

			AmazonS3Client client = new AmazonS3Client(AWS_ACCESS_KEY, AWS_SECRET_KEY, RegionEndpoint.EUWest1);
				
			string BUCKET_NAME = "5minutes2town";
			ListBucketsResponse response = await client.ListBucketsAsync();
			bool found = false;
			foreach (S3Bucket bucket in response.Buckets)
			{
				if (bucket.BucketName == BUCKET_NAME)
				{                   
					found = true;
					break;
				}
			}
			if (found)
			{

				PutObjectRequest request = new PutObjectRequest();
				request.BucketName = BUCKET_NAME;
				request.Key = System.IO.Path.GetFileName (App._file.AbsolutePath);
				request.FilePath = App._file.AbsolutePath;
				var x = client.PutObjectAsync(request);
				var response1 = client.PutACLAsync(new PutACLRequest()
				{
					CannedACL = S3CannedACL.PublicRead,
					BucketName = BUCKET_NAME,
					Key = System.IO.Path.GetFileName (App._file.AbsolutePath)
				});

				string preSignedURL = client.GetPreSignedURL(new GetPreSignedUrlRequest()
				{                        
					BucketName = BUCKET_NAME,
					Key = System.IO.Path.GetFileName (App._file.AbsolutePath),
					Expires = System.DateTime.Now.AddMinutes(120)
				});

			}

			Toast.MakeText (this, "File uploaded", ToastLength.Short).Show();
        }
 
    }
}