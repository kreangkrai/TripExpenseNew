using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Models;

namespace TripExpenseNew.Services
{
    public class FindLocationService
    {
        public Tuple<string,bool> FindLocation(List<LocationOtherModel> GetLocationCTL , List<LocationOtherModel> GetLocationOthers , List<LocationCustomerModel> GetLocationCustomers , Location location)
        {
            // Find Customer
            bool isLocation = false;
            string loc = "";
            bool isCustomer = false;
            for (int i = 0; i < GetLocationCTL.Count; i++)
            {
                Location ctl = new Location()
                {
                    Latitude = GetLocationCTL[i].latitude,
                    Longitude = GetLocationCTL[i].longitude
                };
                double distance = CalculateDistance(ctl, location);
                if (distance <= 0.2)
                {
                    loc = GetLocationCTL[i].location;
                    isLocation = true;
                    break;
                }
            }

            if (!isLocation)
            {
                for (int i = 0; i < GetLocationCustomers.Count; i++)
                {
                    Location _loc = new Location()
                    {
                        Latitude = GetLocationCustomers[i].latitude,
                        Longitude = GetLocationCustomers[i].longitude
                    };
                    double distance = CalculateDistance(_loc, location);
                    if (distance <= 0.2)
                    {
                        loc = GetLocationCustomers[i].location;
                        isLocation = true;
                        isCustomer = true;
                        break;
                    }
                }
            }

            if (!isLocation)
            {
                for (int i = 0; i < GetLocationOthers.Count; i++)
                {
                    Location _loc = new Location()
                    {
                        Latitude = GetLocationOthers[i].latitude,
                        Longitude = GetLocationOthers[i].longitude
                    };
                    double distance = CalculateDistance(_loc, location);
                    if (distance <= 0.2)
                    {
                        loc = GetLocationOthers[i].location;
                        isLocation = true;
                        break;
                    }
                }
            }
            return new Tuple<string, bool>(loc,isCustomer);
        }
        private double CalculateDistance(Location loc1, Location loc2)
        {
            double R = 6371; // รัศมีโลก (กิโลเมตร)
            double lat1 = loc1.Latitude * Math.PI / 180;
            double lat2 = loc2.Latitude * Math.PI / 180;
            double deltaLat = (loc2.Latitude - loc1.Latitude) * Math.PI / 180;
            double deltaLon = (loc2.Longitude - loc1.Longitude) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
