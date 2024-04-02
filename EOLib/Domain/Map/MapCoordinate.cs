using System;

namespace EOLib.Domain.Map
{
    public struct MapCoordinate : IComparable<MapCoordinate>
    {
        public static MapCoordinate Zero { get; } = new MapCoordinate(0, 0);

        public int X { get; }

        public int Y { get; }

        public MapCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static MapCoordinate operator -(MapCoordinate lhs, MapCoordinate rhs)
        {
            return new MapCoordinate(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static bool operator ==(MapCoordinate left, MapCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapCoordinate left, MapCoordinate right)
        {
            return !(left == right);
        }

        public override string ToString() => $"{X}, {Y}";

        public override bool Equals(object obj)
        {
            // Direct comparison for struct type
            if (obj.GetType() != typeof(MapCoordinate))
                return false;

            var other = (MapCoordinate)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            // Simplified hash code calculation
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ X.GetHashCode();
                hash = (hash * 16777619) ^ Y.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(MapCoordinate other)
        {
            // Removed redundant null check for struct and simplified comparison logic
            int compareX = X.CompareTo(other.X);
            if (compareX != 0) return compareX;

            return Y.CompareTo(other.Y);
        }
    }
}