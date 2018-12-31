using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace DCI.Data
{
    internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
    {
        private int _threadId;
        internal int state;
        internal TSource current;

        public Iterator()
        {
            _threadId = Environment.CurrentManagedThreadId;
        }

        public TSource Current
        {
            get { return current; }
        }

        protected abstract Iterator<TSource> Clone();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            current = default(TSource);
            state = -1;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            if (state == 0 && _threadId == Environment.CurrentManagedThreadId)
            {
                state = 1;
                return this;
            }

            Iterator<TSource> duplicate = Clone();
            duplicate.state = 1;
            return duplicate;
        }

        public abstract bool MoveNext();

        object IEnumerator.Current
        {
            get { return Current; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }


    internal class ReadDataIterator: Iterator<double>
    {
        private readonly string _path;
        //private readonly Encoding _encoding;
        private StreamReader _reader;
        private int _index;

        private ReadDataIterator(string path, int index, StreamReader reader)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.Length > 0);
            //Debug.Assert(encoding != null);
            Debug.Assert(reader != null);

            _path = path;
            //_encoding = encoding;
            _reader = reader;
            _index = index;
        }

        public override bool MoveNext()
        {
            if (this._reader != null)
            {
                var ss = _reader.ReadLine();
                if (ss != null)
                {
                    var s = ss.Split(',')[_index];
                    if (!double.TryParse(s, out this.current))
                        this.current = double.NaN;
                    return true;
                }
                // To maintain 4.0 behavior we Dispose 
                // after reading to the end of the reader.
                Dispose();
            }

            return false;
        }
        protected override Iterator<double> Clone()
        {
            // NOTE: To maintain the same behavior with the previous yield-based
            // iterator in 4.0, we have all the IEnumerator<T> instances share the same 
            // underlying reader. If we have already been disposed, _reader will be null, 
            // which will cause CreateIterator to simply new up a new instance to start up
            // a new iteration. We cannot change this behavior due to compatibility 
            // concerns.
            return CreateIterator(_path, _index, Encoding.Default, _reader);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_reader != null)
                    {
                        _reader.Dispose();
                    }
                }
            }
            finally
            {
                _reader = null;
                base.Dispose(disposing);
            }
        }

        internal static ReadDataIterator CreateIterator(string path, int index)
        {
            return CreateIterator(path, index, Encoding.Default, (StreamReader)null);
        }

        private static ReadDataIterator CreateIterator(string path, int index, Encoding encoding, StreamReader reader)
        {
            return new ReadDataIterator(path, index, reader ?? new StreamReader(path, encoding));
        }

    }

    internal class ShapeDataReadIterator: Iterator<double>
    {
        private readonly string _path;
        private ShapefileDataReader _reader;
        private int _index;

        private ShapeDataReadIterator(string path, int index, ShapefileDataReader reader)
        {
            this._path = path;
            this._index = index;
            this._reader = reader;
        }

        public override bool MoveNext()
        {
            if (this._reader != null)
            {
                if (_reader.Read())
                {
                    this.current = _reader.GetDouble(this._index);
                    return true;
                }
                // To maintain 4.0 behavior we Dispose 
                // after reading to the end of the reader.
                Dispose();
            }

            return false;
        }

        //public void Reset()
        //{
        //    if (this._reader != null)
        //    {
        //        this._reader.Reset();
        //    }
        //}
        protected override Iterator<double> Clone()
        {
           
            return CreateIterator(_path, _index, _reader);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_reader != null)
                    {
                        _reader.Dispose();
                    }
                }
            }
            finally
            {
                _reader = null;
                base.Dispose(disposing);
            }
        }

        internal static ShapeDataReadIterator CreateIterator(string path, int index, Encoding encoding)
        {
            var factory = new GeometryFactory();
            ShapefileDataReader reader;
            if (encoding == null)
                reader = new ShapefileDataReader(path, factory);
            else 
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                reader = new ShapefileDataReader(path, factory,encoding);
            }
            return CreateIterator(path, index, reader);
        }

        private static ShapeDataReadIterator CreateIterator(string path, int index, ShapefileDataReader reader)
        {
            return new ShapeDataReadIterator(path, index, reader);
        }
    }

    internal class DbDataReadIterator : Iterator<double>
    {
        public DbDataReadIterator(int index, DbDataReader dbDataRead)
        {
            this._index = index;
            this._reader = dbDataRead;
        }

        private DbDataReader _reader;
        private int _index;

        public override bool MoveNext()
        {
            if (this._reader != null)
            {
                if (_reader.Read())
                {
                    this.current = _reader.GetDouble(this._index);
                    return true;
                }
                // To maintain 4.0 behavior we Dispose 
                // after reading to the end of the reader.
                Dispose();
            }

            return false;
        }

        protected override Iterator<double> Clone()
        {
           
            return CreateIterator(_index, _reader);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_reader != null)
                    {
                        _reader.Dispose();
                    }
                }
            }
            finally
            {
                _reader = null;
                base.Dispose(disposing);
            }
        }

        internal static DbDataReadIterator CreateIterator(int index, DbDataReader reader)
        {
            return new DbDataReadIterator(index, reader);
        }
    }
}