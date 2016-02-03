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
    /// AssayRecord represents an analysis at the end of a processing path through a sample's processing tree.
    /// </summary>
    public class AssayRecord : BindingList<AssayMeasurement>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AssayRecord"/> class.
        /// </summary>
        /// <param name="processingTree">Sample processing tree</param>
        /// <param name="processingPath">Path through the processing tree to the assay analysis</param>
        public AssayRecord(ProcessingTree processingTree,ProcessingPath processingPath)
        {
            m_processingTree = processingTree;
            m_fullProcessingPath = processingPath;
            m_processingHistory = processingTree.GetProcessingHistory(processingPath);
        }
        #endregion

        #region Properties
        private ProcessingTree m_processingTree;

        /// <summary>
        /// Gets the sample processing tree. 
        /// </summary>
        /// <value>The processing tree.</value>
        [Browsable(false)]
        public ProcessingTree ProcessingTree
        {
            get
            {
                return m_processingTree;
            }
        }

        private ProcessingPath m_fullProcessingPath;

        /// <summary>
        /// Gets the processing path through the processing tree for this analysis. 
        /// </summary>
        /// <value>The pprocessing path.</value>
        [Browsable(false)]
        public ProcessingPath ProcessingPath
        {
            get
            {
                return m_fullProcessingPath;
            }
        }

        private Process m_processingHistory;

        /// <summary>
        /// Gets or sets the processing history associated with the processing path. The processing history is a sequence of procedures. 
        /// </summary>
        /// <value>The processing history.</value>
        [Browsable(false)]
        public Process ProcessingHistory
        {
            get
            {
                return m_processingHistory;
            }
            set
            {
                m_processingHistory = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the value of a measurement on this assay analysis.
        /// </summary>
        /// <param name="columnName">Name of the assay column.</param>
        /// <returns>Assay measurement</returns>
        public object GetValue(String columnName)
        {
            // If the column is the ID key then return the sample ID.
            if (String.Equals(ADX4.Tools.Languages.Strings.fldSampleIdKey, columnName))
                return m_processingTree.ToString();

            // If the column is the processing path key then return the processing path.
            if (String.Equals(ADX4.Tools.Languages.Strings.fldProcessingPathKey, columnName))
                return (this.m_fullProcessingPath == null ? "" : this.m_fullProcessingPath.ToString());

            // Otherwise find the requested assay column
            foreach (AssayMeasurement measurement in this)
                if (String.Equals(measurement.Procedure.Id, columnName))
                {
                    // Return the assay measurement's value for this column
                    if (measurement.Measurement == null)
                        return "";
                    if (measurement.Measurement.Value == null)
                        return ADX4.Tools.Languages.Strings.defNA;
                    
                    if (measurement.Measurement.Status == MeasurementStatus.Detected)
                        return measurement.Measurement.Value.Value;

                    if (measurement.Measurement.Status == MeasurementStatus.BelowDetection)
                        return String.Concat(ADX4.Tools.Languages.Strings.defBelowDetection,measurement.Measurement.Value.Value);

                    if (measurement.Measurement.Status == MeasurementStatus.OverRange)
                        return String.Concat(ADX4.Tools.Languages.Strings.defOverRange,measurement.Measurement.Value.Value);
                }

            return "";
        }
        #endregion
    }
}
