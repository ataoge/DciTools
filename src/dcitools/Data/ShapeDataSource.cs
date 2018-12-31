using System.Collections.Generic;

namespace DCI.Data
{
    public class ShapeDataSource
    {
        public ShapeDataSource(string path, int index)
        {
            this._path = path;
            this._index = index;
        }

        private string _path;
        private int _index;

        public IEnumerable<double> GetData()
        {
            var shp = ShapeDataReadIterator.CreateIterator(this._path, this._index, null);
            foreach(var value in shp)
            {
                yield return value;
            }
     
        }
    }
}