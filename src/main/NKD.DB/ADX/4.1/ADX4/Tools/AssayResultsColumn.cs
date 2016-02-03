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
    /// This describes an assay column for an <see cref="AssayMeasurement"/>.
    /// </summary>
    public class AssayResultsColumn 
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AssayResultsColumn"/> class.
        /// </summary>
        /// <param name="procedure">The procedure describing this assay column.</param>
        public AssayResultsColumn(Procedure procedure)
        {
            m_name = procedure.Id;
            m_procedure = procedure;
        }
        #endregion

        #region Properties
        private Procedure m_procedure;
        /// <summary>
        /// Gets the procedure that generated values for this assay column. 
        /// </summary>
        /// <value>The procedure.</value>
        public Procedure Procedure
        {
            get
            {
                return m_procedure;
            }
        }

        private String m_name;
        /// <summary>
        /// Gets the name of this assay column. 
        /// </summary>
        /// <value>The assay name.</value>
        public String Name
        {
            get
            {
                return m_name;
            }
        }
        #endregion
    }
}
