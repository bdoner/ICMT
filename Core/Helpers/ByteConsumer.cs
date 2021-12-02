namespace IMCT.Core.Helpers
{
    public class ByteConsumer
    {
        private IEnumerable<byte> _data;
        public ByteConsumer(IEnumerable<byte> data)
        {
            _data = data;
        }

        public bool SkipTo(IEnumerable<byte> subArr)
        {
            if (_data.Count() < subArr.Count()) throw new ArgumentException("The argument ", nameof(subArr) + " must be containable in the in the remaining bytes of the consumer instance.");

            for (var i = 0; i <= _data.Count() - subArr.Count(); i++)
            {
                if (Enumerable.SequenceEqual(_data.Skip(i).Take(subArr.Count()), subArr))
                {
                    _data = _data.Skip(i);
                    return true;
                }
            }

            return false;
        }

        public bool SkipPast(IEnumerable<byte> subArr)
        {
            if (_data.Count() - subArr.Count() <= subArr.Count()) throw new ArgumentException("The argument ", nameof(subArr) + " must be containable in the in the remaining bytes of the consumer instance.");

            for (var i = 0; i < _data.Count() - subArr.Count(); i++)
            {
                if (Enumerable.SequenceEqual(_data.Skip(i).Take(subArr.Count()), subArr))
                {
                    _data = _data.Skip(i + subArr.Count());
                    return true;
                }
            }

            return false;
        }

        public byte ConsumeSingle()
        {
            return Consume(1)[0];
        }

        public byte[] Consume(int i)
        {
            var consumed = _data.Take(i);
            _data = _data.Skip(i);
            return consumed.ToArray();
        }

        public byte[] Peek(int i)
        {
            return _data.Take(i).ToArray();
        }


    }
}
