using Mirror2MegaNZ.V2.DomainModel;
using System.Collections.Generic;

namespace Mirror2MegaNZ.V2.Logic
{
    internal class ItemComparer : IEqualityComparer<IItem>
    {
        public bool Equals(IItem x, IItem y)
        {
            if( x == null && y == null )
            {
                return true;
            }

            if(( x == null && y != null ) || (x != null && y == null ))
            {
                return false;
            }

            var isEqual = x.Type == y.Type &&
                   x.Name == y.Name &&
                   x.Path == y.Path &&
                   x.Size == y.Size &&
                   x.LastModified.HasValue == y.LastModified.HasValue;

            if( x.LastModified.HasValue && y.LastModified.HasValue )
            {
                // Let's compare also the Last modified datetime
                isEqual &= x.LastModified.Value.Year == y.LastModified.Value.Year &&
                           x.LastModified.Value.Month == y.LastModified.Value.Month &&
                           x.LastModified.Value.Day == y.LastModified.Value.Day &&
                           x.LastModified.Value.Hour == y.LastModified.Value.Hour &&
                           x.LastModified.Value.Minute == y.LastModified.Value.Minute &&
                           x.LastModified.Value.Second == y.LastModified.Value.Second;
            }

            return isEqual;
        }

        public int GetHashCode(IItem obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ obj.Name.ToLower().GetHashCode();
                hash = (hash * 16777619) ^ obj.Size.GetHashCode();
                hash = (hash * 16777619) ^ obj.Type.GetHashCode();
                if( obj.LastModified.HasValue)
                {
                    hash = (hash * 16777619) ^ obj.LastModified.Value.Year.GetHashCode();
                    hash = (hash * 16777619) ^ obj.LastModified.Value.Month.GetHashCode();
                    hash = (hash * 16777619) ^ obj.LastModified.Value.Day.GetHashCode();
                    hash = (hash * 16777619) ^ obj.LastModified.Value.Hour.GetHashCode();
                    hash = (hash * 16777619) ^ obj.LastModified.Value.Minute.GetHashCode();
                    hash = (hash * 16777619) ^ obj.LastModified.Value.Second.GetHashCode();
                }
                
                return hash;
            }
        }
    }
}
