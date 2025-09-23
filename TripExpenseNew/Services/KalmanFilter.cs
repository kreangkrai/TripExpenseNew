using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Maui.Devices.Sensors;

namespace TripExpenseNew.Services
{
    public class KalmanFilter
    {
        private Matrix<double> A, H, Q, R, P;
        private Vector<double> x;
        private double dt;

        public KalmanFilter(double dt, double processNoise, double measurementNoise)
        {
            this.dt = dt;
            A = DenseMatrix.OfArray(new double[,] {
                { 1, 0, dt, 0 },
                { 0, 1, 0, dt },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            });
            H = DenseMatrix.OfArray(new double[,] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 }
            });
            Q = processNoise * DenseMatrix.CreateIdentity(4);
            R = measurementNoise * DenseMatrix.CreateIdentity(2);
            x = DenseVector.OfArray(new[] { 0.0, 0.0, 0.0, 0.0 });
            P = DenseMatrix.CreateIdentity(4);
        }

        public void Predict()
        {
            x = A.Multiply(x);
            P = A.Multiply(P).Multiply(A.Transpose()) + Q;
        }

        public void Update(Vector<double> z, double accuracy)
        {
            R = DenseMatrix.CreateDiagonal(2, 2, accuracy * accuracy);
            var y = z - H.Multiply(x);
            var S = H.Multiply(P).Multiply(H.Transpose()) + R;
            var K = P.Multiply(H.Transpose()).Multiply(S.Inverse());
            x = x + K.Multiply(y);
            var I = DenseMatrix.CreateIdentity(4);
            P = (I - K.Multiply(H)).Multiply(P);
        }

        public Vector<double> GetState() => x;

        public double GetFilteredAccuracy()
        {
            return Math.Sqrt(P[0, 0] + P[1, 1]);
        }
    }

    public static class CoordinateConverter
    {
        private const double EarthRadius = 6378137.0;

        public static (double x, double y) LatLonToXY(double lat, double lon, double latRef, double lonRef)
        {
            double latRad = lat * Math.PI / 180.0;
            double lonRad = lon * Math.PI / 180.0;
            double latRefRad = latRef * Math.PI / 180.0;
            double lonRefRad = lonRef * Math.PI / 180.0;
            double x = EarthRadius * (lonRad - lonRefRad) * Math.Cos(latRefRad);
            double y = EarthRadius * (latRad - latRefRad);
            return (x, y);
        }

        public static (double lat, double lon) XYToLatLon(double x, double y, double latRef, double lonRef)
        {
            double latRefRad = latRef * Math.PI / 180.0;
            double lonRefRad = lonRef * Math.PI / 180.0;
            double lat = latRef + (y / EarthRadius) * 180.0 / Math.PI;
            double lon = lonRef + (x / (EarthRadius * Math.Cos(latRefRad))) * 180.0 / Math.PI;
            return (lat, lon);
        }
    }

    public class CalculateKalman
    {
        private Location curr_location;
        private Location prev_location;

        public CalculateKalman(Location _curr_location, Location _prev_location)
        {
            curr_location = _curr_location;
            prev_location = _prev_location;
        }

        public Location Calculate()
        {
            if (curr_location == null || prev_location == null)
                return curr_location ?? prev_location ?? new Location();

            double first_speed = curr_location.Speed.HasValue ? curr_location.Speed.Value * 3.6 : 0;
            double accuracy_curr = curr_location.Accuracy.HasValue ? curr_location.Accuracy.Value : 10.0;
            double dt = (curr_location.Timestamp - prev_location.Timestamp).TotalSeconds;
            //double dt = 1.0;
            if (dt <= 0) dt = 1.0;

            double processNoise = first_speed switch
            {
                <= 10.0 => 0.005,
                <= 60.0 => 0.03,
                <= 120.0 => 0.07,
                _ => 0.1
            };

            double measurementNoise = accuracy_curr * accuracy_curr;
            double latRef = prev_location.Latitude;
            double lonRef = prev_location.Longitude;

            //var (initialX, initialY) = CoordinateConverter.LatLonToXY(prev_location.Latitude, prev_location.Longitude, latRef, lonRef);
            var kf = new KalmanFilter(dt: dt, processNoise: processNoise, measurementNoise: measurementNoise);

            if (accuracy_curr > 100)
            {
                kf.Predict();
            }
            else
            {
                var (x, y) = CoordinateConverter.LatLonToXY(curr_location.Latitude, curr_location.Longitude, latRef, lonRef);
                var z = DenseVector.OfArray(new[] { x, y });
                kf.Predict();
                kf.Update(z, accuracy_curr);
            }

            var state = kf.GetState();
            var (filteredLat, filteredLon) = CoordinateConverter.XYToLatLon(state[0], state[1], latRef, lonRef);
            double filteredAccuracy = kf.GetFilteredAccuracy();

            // Calculate the new speed (magnitude of velocity vector) in km/h
            //double vx = state[2]; // x-velocity in meters per second
            //double vy = state[3]; // y-velocity in meters per second
            //double speedMs = Math.Sqrt(vx * vx + vy * vy); // Speed in meters per second
            //double filteredSpeed = speedMs * 3.6; // Convert to km/h

            var filteredLocation = new Location
            {
                Latitude = filteredLat,
                Longitude = filteredLon,
                Accuracy = filteredAccuracy,  
                //Speed = filteredSpeed,
                Timestamp = curr_location.Timestamp
            };

            return filteredLocation;
        }
    }
}