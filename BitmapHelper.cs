using System;
using Android.Graphics;

namespace OnlineShop
{
	public static class BitmapHelper
	{
 
			public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
			{
				BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
				BitmapFactory.DecodeFile(fileName, options);

				int outHeight = options.OutHeight;
				int outWidth = options.OutWidth;
				int inSampleSize = 1;

				if (outHeight > height || outWidth > width)
				{
					inSampleSize = outWidth > outHeight
						? outHeight / height
						: outWidth / width;
				}

				options.InSampleSize = inSampleSize;
				options.InJustDecodeBounds = false;
				Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

				return resizedBitmap;
			}
 
	}
}

