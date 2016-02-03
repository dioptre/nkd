//-----------------------------------------------------------------------------
//
// ADX4 Toolkit
//
// Copyright 2009. Mining Industry Geospatial Consortium.
//
// This file is part of the ADX4 Toolkit.
//
//    The ADX4 toolkit is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published
//    by the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ADX4 toolkit is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public License
//    along with The ADX4 toolkit.  If not, see <http://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace ADX4.Tools
{
    /// <summary>
    /// Property descriptor for a measurement in an assay record.
    /// </summary>
    internal class AssayRecordPropDescriptor : PropertyDescriptor
    {
        #region Constructors
        public AssayRecordPropDescriptor(String displayName,String name,Attribute[] attributes) : base(name,attributes)
        {
            m_displayName = displayName;
        }

        public AssayRecordPropDescriptor(String name, Attribute[] attributes)
            : base(name, attributes)
        {
            m_displayName = name;
        }
        #endregion

        #region Methods
        private String m_displayName;
        public override string DisplayName
        {
            get
            {
                return this.m_displayName;
            }
        }

        public string SetDisplayName
        {
            set
            {
                this.m_displayName = value;
            }
        }

        public override Type ComponentType
        {
            get 
            { 
                return typeof(AssayRecord); 
            }
        }

        public override bool IsReadOnly
        {
            get 
            { 
                return true; 
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override Type PropertyType
        {
            get 
            {
                return typeof(String);
            }
        }

        public override object GetValue(object component)
        {
            if (!(component is AssayRecord))
                return "";

            return (component as AssayRecord).GetValue(this.Name);
        }

        public override void SetValue(object component, object value)
        {
        }
        #endregion
    }
}
