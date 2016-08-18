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
using Amazon;
using System.Net;

namespace OnlineShop
{
    class Constants
    {
        // You should replace these values with your own
        public const string COGNITO_POOL_ID = "FiveMinutesUser";


        // Note, the bucket will be created in all lower case letters
        // If you don't enter an all lower case title, any references you add
        // will need to be sanitized
        public const string BUCKET_NAME = "5minutes2town";

        public static RegionEndpoint REGION = RegionEndpoint.EUWest1;

        public const HttpStatusCode NO_SUCH_BUCKET_STATUS_CODE = HttpStatusCode.NotFound;
        public const HttpStatusCode BUCKET_ACCESS_FORBIDDEN_STATUS_CODE = HttpStatusCode.Forbidden;
        public const HttpStatusCode BUCKET_REDIRECT_STATUS_CODE = HttpStatusCode.Redirect;
    }
}