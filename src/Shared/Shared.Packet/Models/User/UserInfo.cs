using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Packet.Models
{
    [Serializable]
    public class UserInfo : UserSimple
    {
        public ProfileInfo ProfileInfo { get; set; } = new ProfileInfo();
    }

    public class UserInfoComparer : IEqualityComparer<UserInfo>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(UserInfo x, UserInfo y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.UserSeq == y.UserSeq;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(UserInfo userInfo)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(userInfo, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashUserSeq = userInfo.UserSeq.GetHashCode();

            //Get hash code for the Code field.
            int hashNick = userInfo.Nick.GetHashCode();

            //Calculate the hash code for the product.
            return hashUserSeq ^ hashNick;
        }

    }
}
