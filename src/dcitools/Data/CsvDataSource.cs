using System.Collections.Generic;
using System.IO;

namespace DCI.Data
{
    public class CsvDataSource
    {
        public CsvDataSource(string path, int index)
        {
            this._path = path;
            this._index = index;
        }

        private string _path;
        private int _index;

        public IEnumerable<double> GetData()
        {
            foreach(var ss in File.ReadLines(this._path))
            {
                var s = ss.Split(',')[this._index];
                var value = 0.0;
                if (double.TryParse(s, out value))
                    yield return value;
                continue;
            }
       
            yield break;
        }
        
    }
}