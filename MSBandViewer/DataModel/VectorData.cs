using System;

namespace Niuware.MSBandViewer.DataModel
{
    public struct VectorData2D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
    }

    public struct VectorData3D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }
    }

    public struct SensorData
    {
        //public DateTime Timestamp { get; set; }

        public VectorData3D<double> gyroscopeAngVel;
        public int heartRateBpm;

        public override string ToString()
        {
            return heartRateBpm.ToString() + "," + gyroscopeAngVel.X + "," + gyroscopeAngVel.Y + "," + gyroscopeAngVel.Z;
        }

        public bool IsEmpty()
        {
            if (heartRateBpm == 0)
                return true;

            if (gyroscopeAngVel.X == 0.0 && gyroscopeAngVel.Y == 0.0 && gyroscopeAngVel.Z == 0.0)
                return true;

            return false;
        }
    }

    public struct VectorDataTime3D<T>
    {
        public string Label { get; set; }
        public DateTime Timestamp { get; set; }

        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        public override string ToString()
        {
            return Timestamp.ToString("HH:mm:ss:fff") + "," + X.ToString() + "," + Y.ToString() + "," + Z.ToString();
        }
    }

    public enum SensorType
    {
        HEARTRATE,
        GYROSCOPE
    }
}