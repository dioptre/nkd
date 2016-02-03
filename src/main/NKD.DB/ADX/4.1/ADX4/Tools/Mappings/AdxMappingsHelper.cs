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
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ADX4.Tools.Mappings
{
    /// <summary>
    /// This uses a lookup table to convert values inside an ADX document into alternative values.
    /// </summary>
    public partial class Mappings
    {
        #region Constants
        private String c_mappingSeparator = "|"; // Used to separate processing tags when converting a processing path into text.
        #endregion

        #region Properties
        /// <summary>
        /// Indciates if the lookup table has assay mappings. 
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has assay mappings; otherwise, <c>false</c>.
        /// </value>
        public Boolean HasAssayMappings
        {
            get
            {
                return this.Assays != null && this.Assays.Length > 1;
            }
        }
        #endregion

        #region Events
        static public event XmlElementEventHandler OnLoadUnknownElements;
        static public event XmlAttributeEventHandler OnLoadUnknownAttributes;
        static public event XmlNodeEventHandler OnLoadUnknownNode;
        static public event UnreferencedObjectEventHandler OnLoadUnreferencedObjects;
        #endregion

        /// <summary>
        /// Loads mappings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the mappings file.</param>
        /// <returns></returns>
        static public Mappings Load(String fileName)
        {
            if (fileName == null)
                return new Mappings();

            // Deserialize the ADX mappings from an XML file
            Mappings mappings = new Mappings();
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Mappings));

                FileStream adxStream = new FileStream(fileName, FileMode.Open);
                XmlReader xmlReader = XmlReader.Create(adxStream);

                // Ignore mal-formed XML for now
                XmlDeserializationEvents events = new XmlDeserializationEvents();
                events.OnUnknownElement = Mappings.OnLoadUnknownElements;
                events.OnUnknownAttribute = Mappings.OnLoadUnknownAttributes;
                events.OnUnknownNode = Mappings.OnLoadUnknownNode;
                events.OnUnreferencedObject = Mappings.OnLoadUnreferencedObjects;

                // Get the ADX mappings
                mappings = xmlSerializer.Deserialize(xmlReader, events) as Mappings;

                adxStream.Close();
            }

            // Trap and throw the any exceptions higher up
            catch (System.Exception exc)
            {
                throw exc;
            }

            return mappings;
        }

        /// <summary>
        /// Substitutes the analyte name for an alternative analyte name.
        /// </summary>
        /// <param name="analyte">The new analyte.</param>
        /// <returns></returns>
        public String MapAnalyte(String analyte)
        {
            if (this.Analyte == null)
                return analyte;

            foreach (Substitution substitution in this.Analyte)
                if (String.Equals(analyte, substitution.Name))
                    return substitution.With;

            return analyte;
        }

        /// <summary>
        /// Substitutes the UOM for an alternative UOM.
        /// </summary>
        /// <param name="UOM">The new UOM.</param>
        /// <returns></returns>
        public String MapUOM(String UOM)
        {
            if (this.UOM == null)
                return UOM;

            foreach (Substitution substitution in this.UOM)
                if (String.Equals(UOM, substitution.Name))
                    return substitution.With;

            return UOM;
        }

        /// <summary>
        /// Substitutes the processing path with an alternative processing path.
        /// </summary>
        /// <param name="processingPath">The new processing path.</param>
        /// <returns></returns>
        public ProcessingPath MapProcessingPath(ProcessingPath processingPath)
        {
            // Any processing path lookups ? 
            if (this.ProcessingPath == null || processingPath == null || processingPath.Tag == null)
                return processingPath;

            // Map each processing tag
            List<ProcessingTag> mappedTags = new List<ProcessingTag>();
            for (Int32 i = 0; i < processingPath.Tag.Length; i++)
            {
                ProcessingTag mappedTag = new ProcessingTag();
                mappedTag.Type = processingPath.Tag[i].Type;
                mappedTag.Name = processingPath.Tag[i].Name;

                // Find the longest matching mapping
                Boolean mapped = false;
                for (Int32 j = processingPath.Tag.Length; j > i && !mapped; j--)
                {
                    // Append these processing tags together
                    StringBuilder appendedTags = new StringBuilder();
                    for (Int32 k = i; k < j; k++)
                    {
                        if (appendedTags.Length > 0)
                            appendedTags.Append(c_mappingSeparator);
                        if (!String.IsNullOrEmpty(processingPath.Tag[k].Type))
                            appendedTags.Append(processingPath.Tag[k].Type);
                    }

                    // Does this appended list of tags types match any mapping ?
                    foreach (Substitution substitution in this.ProcessingPath)
                        if (!String.IsNullOrEmpty(substitution.Name))
                        {
                            if (String.Equals(appendedTags.ToString(), substitution.Name))
                            {
                                mappedTag.Type = substitution.With;
                                mappedTag.Name = processingPath.Tag[j - 1].Name;
                                mapped = true;
                                i = j;
                                break;
                            }
                        }
                }

                mappedTags.Add(mappedTag);
            }

            // Return the mapped processing path
            ProcessingPath mappedPath = new ProcessingPath();
            mappedPath.Id = processingPath.Id;
            mappedPath.Tag = mappedTags.ToArray();
            return mappedPath;
        }

        /// <summary>
        /// Searchs the object for properties that can be mapped and replaces those properties with the new values.
        /// </summary>
        /// <param name="parentObj">The mapping object.</param>
        /// <returns></returns>
        public System.Object Map(System.Object parentObj)
        {
            // Any object to map ?
            if (parentObj == null || parentObj.GetType().GetProperties() == null || parentObj.GetType().IsValueType || !String.Equals(parentObj.GetType().Namespace, "ADX4"))
                return null;

            // Recursive search the object's properties
            foreach (PropertyInfo propertyInfo in parentObj.GetType().GetProperties())
            {
                try
                {
                    // Get the property's value
                    System.Object propValue = propertyInfo.GetValue(parentObj, null);
                    if (propValue == null)
                        continue;

                    // Is this a Analyte property ?
                    if (propertyInfo.PropertyType == typeof(String) && String.Equals(propertyInfo.Name, "Analyte"))
                        propertyInfo.SetValue(parentObj, this.MapAnalyte(propValue.ToString()), null);

                    // Is this a UOM property ?
                    else if (propertyInfo.PropertyType == typeof(String) && String.Equals(propertyInfo.Name, "UOM"))
                        propertyInfo.SetValue(parentObj, this.MapUOM(propValue.ToString()), null);

                    // Is this a Processing Path property ?
                    else if (propertyInfo.PropertyType == typeof(ProcessingPath))
                        propertyInfo.SetValue(parentObj, this.MapProcessingPath(propValue as ProcessingPath), null);

                    // Map each chold object if it is an array of objects
                    else if (propertyInfo.PropertyType.IsArray)
                    {
                        foreach (System.Object childObj in propValue as Array)
                                Map(childObj);
                    }
                    // Otherwise map the propertie's on this object
                    else
                        Map(propValue);
                }
                catch (System.Exception exc)
                {
                    throw exc;
                }
            }

            return parentObj;
        }

        /// <summary>
        /// Maps the specified procedure code, analye and UOM to a standard name.
        /// </summary>
        /// <param name="procedureCode">The procedure code.</param>
        /// <param name="analyteName">Name of the analyte.</param>
        /// <param name="uomName">Name of the uom.</param>
        /// <returns>Name of assay column</returns>
        public String Map(String id,String procedureCode, String analyteName, String uomName)
        {
            if (this.Assays == null)
                return null;

            // Search all the assay identifications
            foreach (AssayIdentification assayIdent in this.Assays)
            {
                // Anything to map ?
                if (String.IsNullOrEmpty(assayIdent.Analyte) || String.IsNullOrEmpty(assayIdent.UOM) || String.IsNullOrEmpty(assayIdent.AssayType))
                    continue;

                // If this identification matches the procedureCode, analyteName and UOM then return the alternative assay column name
                if (!String.IsNullOrEmpty(assayIdent.Id))
                {
                    if (String.Equals(analyteName, assayIdent.Analyte) && String.Equals(uomName, assayIdent.UOM) && String.Equals(id, assayIdent.Id))
                        return assayIdent.AssayType;
                }
                else if (String.IsNullOrEmpty(assayIdent.ProcedureCode))
                {
                    if (String.Equals(analyteName, assayIdent.Analyte) && String.Equals(uomName, assayIdent.UOM) && String.IsNullOrEmpty(procedureCode))
                        return assayIdent.AssayType;
                }
                else
                {
                    if (String.Equals(analyteName, assayIdent.Analyte) && String.Equals(uomName, assayIdent.UOM) && String.Equals(procedureCode, assayIdent.ProcedureCode))
                        return assayIdent.AssayType;
                }
            }

            return null;
        }
    }
}
