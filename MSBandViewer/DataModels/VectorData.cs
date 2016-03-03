namespace Niuware.MSBandViewer.DataModels
{
    /// <summary>
    /// 3D point T structure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct VectorData3D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }
    }
}