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

namespace ADX4.Tools
{
    /// <summary>
    /// AssayMeasurement is a proxy class for an ADX4 measurement.
    /// </summary>
    public class AssayMeasurement
    {
        #region Constructors
        /// <summary>
        /// Constructor for AssayMeasurement.
        /// </summary>
        /// <param name="measurement"></param>
        public AssayMeasurement(ADX4.Measurement measurement,ADX4.Procedure procedure)
        {
            m_measurement = measurement;
            m_procedure = procedure;
        }
        #endregion

        #region Properties
        /// <summary>
        /// This provides access to the ADX4 measurement.
        /// </summary>
        /// <value>Actual measurement results.</value>
        private ADX4.Measurement m_measurement;
        public ADX4.Measurement Measurement
        {
            get
            {
                return m_measurement;
            }
        }

        /// <summary>
        /// This provides access to a procedure in the processing history associated with the measurement.
        /// </summary>
        /// <value>A procedure that generated the associated measurement.</value>
        private ADX4.Procedure m_procedure;
        public ADX4.Procedure Procedure
        {
            get
            {
                return m_procedure;
            }
        }

        /// <summary>
        /// Converts the measurement into a text readable representation based on its value, UOM and procedure name.
        /// </summary>
        /// <returns>Measurement as text.</returns>
        public override String ToString()
        {
            // If there is nothing to convert then return a question mark ?
            if (m_measurement == null && m_procedure == null)
                return "?";

            // If there is no value or valid UOM then just return the procedure name.
            if (m_measurement.Value == null || m_measurement.Value.UOM == Enum.GetName(typeof(PropertyTypeStrict),UOMStrict.Unspecified))
            {
                if (m_measurement.ProcedureRef == null)
                    return m_measurement.Property.ToString();
                else
                    return m_measurement.ProcedureRef;
            }
            
            // Otherwise build a string based on value, UOM and procedure name.
            else
                return String.Format("{0} {1}", (m_measurement.ProcedureRef == null ? m_measurement.Property.ToString() : m_measurement.ProcedureRef), m_measurement.Value.UOM);
        }
        #endregion
    }
}
