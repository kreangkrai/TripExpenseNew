using Android.Gms.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.Platforms.Android
{
    public class MyLocationCallback : LocationCallback
    {
        private readonly Action<LocationResult> onLocationResult;

        public MyLocationCallback(Action<LocationResult> onLocationResult)
        {
            this.onLocationResult = onLocationResult;
        }

        public override void OnLocationResult(LocationResult result)
        {
            onLocationResult?.Invoke(result);
        }
    }
}
