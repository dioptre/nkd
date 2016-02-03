using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallery.Core.Enums
{
    public sealed class HashEncodingType
    {
        public static readonly HashEncodingType Hex = new HashEncodingType("Hex");
        public static readonly HashEncodingType Base64 = new HashEncodingType("Base64");

        private readonly string _name;

        private HashEncodingType(string name)
        {
            _name = name;
        }

        private static readonly List<HashEncodingType> _hashEncodingTypes = new List<HashEncodingType> { Hex, Base64 };
        public static HashEncodingType FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            return _hashEncodingTypes.Single(a => string.Equals(a._name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string ToString()
        {
            return _name;
        }

    }
}