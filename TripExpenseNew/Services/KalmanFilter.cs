using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
namespace TripExpenseNew.Services
{
    public class KalmanFilter
    {
        private Matrix<double> A, H, Q, R, P;
        private Vector<double> x; // เปลี่ยนเป็น Vector<double> ชัดเจน
        private double dt;

        public KalmanFilter(double dt, double processNoise, double measurementNoise)
        {
            this.dt = dt;

            // State transition matrix A
            A = DenseMatrix.OfArray(new double[,] {
            { 1, 0, dt, 0 },
            { 0, 1, 0, dt },
            { 0, 0, 1, 0 },
            { 0, 0, 0, 1 }
        });

            // Measurement matrix H
            H = DenseMatrix.OfArray(new double[,] {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 }
        });

            // Process noise covariance Q
            Q = processNoise * DenseMatrix.CreateIdentity(4);

            // Measurement noise covariance R
            R = measurementNoise * DenseMatrix.CreateIdentity(2);

            // Initial state [x, y, vx, vy]
            x = DenseVector.Create(4, 0); // ใช้ Vector<double>

            // Initial covariance
            P = DenseMatrix.CreateIdentity(4);
        }

        public void Predict()
        {
            // Predict state: x = A * x
            x = A.Multiply(x); // ถูกต้อง: A (Matrix) * x (Vector) = Vector
                               // Predict covariance: P = A * P * A^T + Q
            P = A.Multiply(P).Multiply(A.Transpose()) + Q;
        }

        public void Update(Vector<double> z, double accuracy)
        {
            // Update R based on GPS accuracy
            R = DenseMatrix.CreateDiagonal(2, 2, accuracy * accuracy);

            // Innovation: y = z - H * x
            var y = z - H.Multiply(x); // ถูกต้อง: H (Matrix) * x (Vector) = Vector
                                       // Innovation covariance: S = H * P * H^T + R
            var S = H.Multiply(P).Multiply(H.Transpose()) + R;
            // Kalman gain: K = P * H^T * S^-1
            var K = P.Multiply(H.Transpose()).Multiply(S.Inverse());
            // Update state: x = x + K * y
            x = x + K.Multiply(y); // ถูกต้อง: K (Matrix) * y (Vector) = Vector
                                   // Update covariance: P = (I - K * H) * P
            var I = DenseMatrix.CreateIdentity(4);
            P = (I - K.Multiply(H)).Multiply(P);
        }

        public Vector<double> GetState()
        {
            return x; // คืน Vector<double>
        }
    }

    public static class CoordinateConverter
    {
        public static (double x, double y) LatLonToXY(double lat, double lon, double latRef, double lonRef)
        {
            const double mPhi = 111132.0;
            double mLambda = 111132.0 * Math.Cos(latRef * Math.PI / 180.0);
            double x = mLambda * (lon - lonRef);
            double y = mPhi * (lat - latRef);
            return (x, y);
        }

        public static (double lat, double lon) XYToLatLon(double x, double y, double latRef, double lonRef)
        {
            const double mPhi = 111132.0;
            double mLambda = 111132.0 * Math.Cos(latRef * Math.PI / 180.0);
            double lat = latRef + (y / mPhi);
            double lon = lonRef + (x / mLambda);
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
            double first_speed = curr_location.Speed.HasValue ? curr_location.Speed.Value * 3.6 : 0;
            double accuracy_prev = prev_location.Accuracy != null ? prev_location.Accuracy.Value : 0.5;
            double accuracy_curr = curr_location.Accuracy != null ? curr_location.Accuracy.Value : 0.5;
            double dt = 1.0;
            double processNoise = 0.01;
            double measurementNoise = accuracy_curr * accuracy_curr;

            if (first_speed <= 10.0)
            {
                dt = 1.0;
                processNoise = 0.005;
            }
            else if (first_speed > 10.0 && first_speed <= 60.0)
            {
                dt = 1.0;
                processNoise = 0.03;
            }
            else if (first_speed > 60.0 && first_speed <= 120.0)
            {
                dt = 1.0;
                processNoise = 0.07;
            }
            else if (first_speed > 120.0)
            {
                dt = 1.0;
                processNoise = 0.1;
            }

            var data = new (double lat, double lon, double acc)[]
                {
                    (prev_location.Latitude, prev_location.Longitude, accuracy_prev),
                    (curr_location.Latitude, curr_location.Longitude, accuracy_curr)
                };

            double latRef = data[0].lat;
            double lonRef = data[0].lon;

            var kf = new KalmanFilter(dt: dt, processNoise: processNoise, measurementNoise: measurementNoise);

            foreach (var (lat, lon, acc) in data)
            {
                var (x, y) = CoordinateConverter.LatLonToXY(lat, lon, latRef, lonRef);
                var z = DenseVector.OfArray(new[] { x, y });

                kf.Predict();
                kf.Update(z, acc);

                var state = kf.GetState();

                var (filteredLat, filteredLon) = CoordinateConverter.XYToLatLon(state[0], state[1], latRef, lonRef);
                curr_location.Latitude = filteredLat;
                curr_location.Longitude = filteredLon;
            }
            return curr_location;
        }
    }
}
