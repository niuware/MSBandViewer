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
}