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
using System.Collections.Generic;
using System.ComponentModel;
using ADX4.Tools.Validation;

namespace ADX4.Tools
{
    /// <summary>
    /// This flattens an ADX document's analytical results into a simple list where the list's entries represent the analytical results as records and the list's properties are assay columns.
    /// </summary>
    public class AssayResults : BindingList<AssayRecord>, ITypedList
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AssayResults"/> class.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        public AssayResults(ADX document,ValidationEventCallback onValidationEvent)
        {
            // Add the validation callback
            this.OnValidationEvent += this.OnValidationEventSink;
            if (onValidationEvent != null)
                this.OnValidationEvent += onValidationEvent;

            m_document = document;

            // Build the processing trees for this ADX document.
            m_processingTrees = new ProcessingTrees(this.Document, onValidationEvent);

            // Bind the analyses and measurements to the assay results list.
            this.Bind();
        }

        public AssayResults(ADX document) : this(document,null)
        {
        }
        #endregion

        #region Variables
         /// <summary>
         /// List of property descriptors for the assay columns.
         /// </summary>
        private PropertyDescriptorCollection m_propertyDescriptors;
        #endregion
         
        #region Properties
        private ADX m_document;
        /// <summary>
        /// Gets the ADX document associated with these assay results. 
        /// </summary>
        /// <value>The ADX document.</value>
        public ADX Document
        {
            get
            {
                return m_document;
            }
        }

        private ProcessingTrees m_processingTrees;
        /// <summary>
        /// Gets the processing trees for all samples listed in the ADX document. 
        /// </summary>
        /// <value>The processing trees.</value>
        public ProcessingTrees ProcessingTrees
        {
            get
            {
                return m_processingTrees;
            }
        }

        private List<AssayResultsColumn> m_columns = new List<AssayResultsColumn>();
        /// <summary>
        /// Gets the assay columns for measurements recorded in this ADX document. The columns will include the ID and Processing Path columns also.
        /// </summary>
        /// <value>The assay columns.</value>
        public List<AssayResultsColumn> Columns
        {
            get
            {
                return m_columns;
            }
        }
        #endregion

        #region Variables
        private ValidationEventCallback OnValidationEvent;
        #endregion

        #region Events
        /// <summary>
        /// Called when a validation event occurs inside support class.
        /// </summary>
        /// <param name="result">The result.</param>
        public void OnValidationEventSink(ValidationResult result)
        {
        }
        #endregion

        #region ITypedList
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (listAccessors == null)
                return m_propertyDescriptors;

            return null;
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return typeof(AssayRecord).Name;
        }
        #endregion

        #region Methods
        private void Bind()
        {
            // Create the list of properties for the assay values, ids, processing paths, etc...
            m_propertyDescriptors = new PropertyDescriptorCollection(null);
            m_propertyDescriptors.Add(new AssayRecordPropDescriptor(ADX4.Tools.Languages.Strings.fldSampleIdKey, null));
            m_propertyDescriptors.Add(new AssayRecordPropDescriptor(ADX4.Tools.Languages.Strings.fldProcessingPathKey, null));

            // Scan each processing tree in the ADX document
            foreach (ProcessingTree processingTree in m_processingTrees)
            {
                // Scan each analysis in each processing tree
                foreach (AssayRecord assayRecord in processingTree)
                {
                    // Scan each measurement in the analysis
                    foreach (AssayMeasurement measurement in assayRecord)
                    {
                        // Check if there is property for this measurement
                        Boolean found = false;
                        foreach (PropertyDescriptor propertyDescriptor in m_propertyDescriptors)
                        {
                            if (String.Equals(propertyDescriptor.Name, measurement.Procedure.Id)) // The measurement's procedure ID identifies the assay column
                            {
                                found = true;
                                break;
                            }
                        }

                        // If no found then add a new property to expose this measurement.
                        if (!found)
                        {
                            m_propertyDescriptors.Add(new AssayRecordPropDescriptor(measurement.ToString(), measurement.Procedure.Id, null));
                            m_columns.Add(new AssayResultsColumn(measurement.Procedure));
                        }
                    }
                }
            }

            // Build a simple list of analytical results based on the processing trees' analyses
            this.Clear();
            foreach (ProcessingTree processingTree in m_processingTrees)
                foreach (AssayRecord assayRecord in processingTree)
                    this.Add(assayRecord);
        }

        /// <summary>
        /// Sets the display name for an assay column's property.
        /// </summary>
        /// <param name="procedure">The assay column's procedure.</param>
        /// <param name="displayName">The property's display name.</param>
        public void SetDisplayName(Procedure procedure, String displayName)
        {
            foreach(PropertyDescriptor descriptor in this.m_propertyDescriptors)
                if (String.Equals(descriptor.Name, procedure.Id))
                {
                    (descriptor as AssayRecordPropDescriptor).SetDisplayName = displayName;
                }

        }

        /// <summary>
        /// Gets the display name for an assay column's property.
        /// </summary>
        /// <param name="procedureName">Name of the assay column's procedure.</param>
        /// <returns></returns>
        public String GetDisplayName(String procedureName)
        {
            foreach (PropertyDescriptor descriptor in this.m_propertyDescriptors)
                if (String.Equals(descriptor.Name, procedureName))
                {
                    return descriptor.DisplayName;
                }
            return "";
        }
        #endregion
    }
}
