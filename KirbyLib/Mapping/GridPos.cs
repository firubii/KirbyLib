using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A representation of an object position within a map.
    /// </summary>
    public struct GridPos
    {
        private uint _value;

        public GridPos(uint rawPos)
        {
            _value = rawPos;
        }

        public GridPos(uint whole, byte _decimal)
        {
            _value = ((uint)(whole << 4) | (uint)(_decimal & 0xF));
        }

        public GridPos(float pos)
        {
            uint wholePart = (uint)Math.Floor(pos);
            uint decimalPart = (uint)Math.Floor((pos - wholePart) * 16f);
            _value = (wholePart << 4) | decimalPart;
        }

        public uint GetWholeNumber()
        {
            return (_value & 0xFFFFFFF0) >> 4;
        }

        public uint GetDecimal()
        {
            return _value & 0xF;
        }

        public uint GetRawValue()
        {
            return _value;
        }

        public float AsFloat()
        {
            return _value / 16f;
        }

        #region Casts

        public static implicit operator uint(GridPos m)
        {
            return m._value;
        }

        public static implicit operator GridPos(uint m)
        {
            return new GridPos(m);
        }

        public static implicit operator float(GridPos m)
        {
            return m.AsFloat();
        }

        public static implicit operator GridPos(float m)
        {
            return new GridPos(m);
        }

        #endregion

        #region Operators

        public static GridPos operator +(GridPos a, GridPos b)
        {
            return new GridPos(a._value + b._value);
        }

        public static GridPos operator -(GridPos a, GridPos b)
        {
            return new GridPos(a._value - b._value);
        }

        public static GridPos operator *(GridPos a, GridPos b)
        {
            return new GridPos(a.AsFloat() * b.AsFloat());
        }

        public static GridPos operator /(GridPos a, GridPos b)
        {
            return new GridPos(a.AsFloat() / b.AsFloat());
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return a._value != b._value;
        }

        public static bool operator >(GridPos a, GridPos b)
        {
            return a._value > b._value;
        }

        public static bool operator <(GridPos a, GridPos b)
        {
            return a._value < b._value;
        }

        public static bool operator >=(GridPos a, GridPos b)
        {
            return a._value >= b._value;
        }

        public static bool operator <=(GridPos a, GridPos b)
        {
            return a._value <= b._value;
        }

        #endregion

        public override string ToString()
        {
            return AsFloat().ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is GridPos)
                return (GridPos)obj == this;

            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
