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
using System.Text;
using System.Xml.Schema;
using System.Xml;

namespace ADX4.Tools.Validation
{
    /// <summary>
    /// This validates the ADX document against its XML schema.
    /// </summary>
    internal class ValidateSchema : List<ValidationResult>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateSchema"/> class.
        /// </summary>
        public ValidateSchema()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks the specified ADX file against the ADX Schema.
        /// </summary>
        /// <param name="fileName">Name of the ADX file.</param>
        /// <param name="schemasDirectory">The ADX schemas directory.</param>
        public void Check(String fileName, String schemasDirectory)
        {

            XmlReader reader = null;
            try
            {
                // Compile the ADX schema
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(ADX.c_ADX4_NameSpace, String.Concat(schemasDirectory, "\\", ADX.c_ADX4_Xsd));
                schemas.Add(ADX.c_ADX4Dictionary_NameSpace, String.Concat(schemasDirectory, "\\", ADX.c_ADX4Dictionary_Xsd));
                schemas.Compile();

                // Read the ADX document and report any errors via the validation events
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas = schemas;
                settings.XmlResolver = new XmlUrlResolver();
                settings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings | System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationEventHandler += this.SchemaValidationResults;

                reader = XmlReader.Create(fileName, settings);
  
                while (reader.Read());
                reader.Close();
                reader = null;
            }
            catch (System.Exception exc)
            {
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

        }

        /// <summary>
        /// Reports a schema validation error.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="eventArgs">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        private void SchemaValidationResults(System.Object obj, ValidationEventArgs eventArgs)
        {
            StringBuilder message = new StringBuilder();
            message.AppendFormat("{0} : {1}", eventArgs.Severity.ToString(), eventArgs.Message);
            this.Add(new ValidationResult(ErrorCodes.XMLFormat, ErrorLevel.Error, eventArgs.Exception.LineNumber,message.ToString()));
        }
        #endregion
    }
}
