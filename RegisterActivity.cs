
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

namespace OnlineShop
{
	[Activity (Label = "Register")]			
	public class RegisterActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.Register);
			Button btnSubmit = FindViewById<Button> (Resource.Id.btnRegisterSubmit);
			btnSubmit.Click	+= BtnSubmit_Click;
		}

		private void BtnSubmit_Click (object sender, EventArgs e)
		{
			EditText name = FindViewById<EditText> (Resource.Id.txtRegisterName);
			EditText surname = FindViewById<EditText> (Resource.Id.txtRegisterSurname);
			EditText username = FindViewById<EditText> (Resource.Id.txtRegisterUserName);
			EditText pwd = FindViewById<EditText> (Resource.Id.txtRegisterPassword);
			EditText pwdRE = FindViewById<EditText> (Resource.Id.txtRegisterPasswordReEnter);

			if (name.Length() == 0) {
				Toast.MakeText (this, "Name is required", ToastLength.Long).Show();
				return;
			}

			if (surname.Length() == 0) {
				Toast.MakeText (this, "Surname is required", ToastLength.Long).Show();
				return;
			}

			if (username.Length() == 0) {
				Toast.MakeText (this, "Username is required", ToastLength.Long).Show();
				return;
			}

			if (pwd.Length() == 0) {
				Toast.MakeText (this, "Password is required", ToastLength.Long).Show();
				return;
			}

			if (pwdRE.Length() == 0) {
				Toast.MakeText (this, "Please re-enter password", ToastLength.Long).Show();
				return;
			}

			if (pwd.Text != pwdRE.Text) {
				Toast.MakeText (this, "Passwords do not match", ToastLength.Long).Show();
				return;
			}

			string folder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var conn = new SQLiteConnection (System.IO.Path.Combine (folder, "fiveminutes2town.db"));
			conn.CreateTable<RegisterUser>();

			RegisterUser r = new RegisterUser ();
			r.UserName = username.Text;
			r.Name = name.Text;
			r.Surname = surname.Text;
			r.Password = pwd.Text;
			r.TimeStamp = DateTime.Now;
 			
			try
			{
				conn.Insert (r);

				Toast.MakeText(this, "User successfully added", ToastLength.Long).Show();
				Task.Delay (2000).Wait ();
				Finish ();
			}
			catch(Exception ex)
			{
				if (ex.Message.ToLower().Equals("constraint")){
					Toast.MakeText(this, "Username already exists", ToastLength.Long).Show();
				}
			}
		}
	}
}

