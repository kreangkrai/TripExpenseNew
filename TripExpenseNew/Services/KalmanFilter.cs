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
}
