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

namespace ADX4.Tools.Validation
{
    /// <summary>
    /// Delagate for validation callbacks.
    /// </summary>
    public delegate void ValidationEventCallback(ValidationResult result);

    /// <summary>
    /// Indicates the level of validation to perform.
    /// </summary>
    public enum ValidationLevel 
    {
        /// <summary>
        /// Perform no validation.
        /// </summary>
        None,
        /// <summary>
        /// Validate the ADX schema.
        /// </summary>
        SchemaOnly,
        /// <summary>
        /// Validate the ADX document against the rules for a laboratory export.
        /// </summary>
        LaboratoryExport 
    };

    /// <summary>
    /// Indicates a validation error code.
    /// </summary>
    public enum ErrorCodes
    {
        /// <summary>
        /// No error.
        /// </summary>
        None,
        /// <summary>
        /// XML Format error (according to the schema).
        /// </summary>
        XMLFormat,
        /// <summary>
        /// ADX Header error.
        /// </summary>
        Header,
        /// <summary>
        /// ADX Address Book error.
        /// </summary>
        AddressBook,
        /// <summary>
        /// ADX Chain of Custody error.
        /// </summary>
        ChainOfCustody,
        /// <summary>
        /// ADX Protocols error.
        /// </summary>
        Protocols,
        /// <summary>
        /// ADX Samples error.
        /// </summary>
        Samples,
        /// <summary>
        /// ADX Reference Material error.
        /// </summary>
        ReferenceMaterial,
        /// <summary>
        /// ADX Results error.
        /// </summary>
        Results,
        /// <summary>
        /// ADX Results error.
        /// </summary>
        SampleProcessingHistory,
    };

    /// <summary>
    /// Indicates the severity of the error.
    /// </summary>
    public enum ErrorLevel
    {
        /// <summary>
        /// It is considered an error.
        /// </summary>
        Error,
        /// <summary>
        /// It is a warning only. An alternative behaviour is preferred.
        /// </summary>
        Warning,
        /// <summary>
        /// It is only information regarding the contents of the ADX document.
        /// </summary>
        Information,
    };

    /// <summary>
    /// This defines a validation error.
    /// </summary>
    public class ValidationResult
    {
        public ValidationResult(ErrorCodes c, ErrorLevel e, int l, String m)
        {
            code = c;
            level = e;
            message = m;
            line = l;
        }

        public ValidationResult(ErrorCodes c, ErrorLevel e, String m)
        {
            code = c;
            level = e;
            message = m;
            line = 0;
        }

        public ErrorCodes code;
        public ErrorLevel level;
        public String message;
        public int line;
    }

    /// <summary>
    /// This performs validation on an ADX document.
    /// </summary>
    public class DocumentValidation : List<ValidationResult>
    {
        #region Static Methods
        public static List<ValidationResult> Validate(string fileName, ValidationLevel level, string schemasDirectory)
        {
            DocumentValidation validator = new DocumentValidation();

            validator.Check(fileName, level, schemasDirectory);

            return validator;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentValidation"/> class.
        /// </summary>
        public DocumentValidation()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks the specified ADX file for errors.
        /// </summary>
        /// <param name="fileName">Name of the ADX file.</param>
        /// <param name="level">The validation level.</param>
        /// <param name="schemasDirectory">The directory for the ADX schemas.</param>
        public void Check(String fileName, ValidationLevel level,String schemasDirectory)
        {
            // No validation required ?
            if (level == ValidationLevel.None)
                return;

            // Validate according to the ADX schema
            ValidateSchema validateSchema = new ValidateSchema();
            validateSchema.Check(fileName, schemasDirectory);
            this.AddRange(validateSchema);

            if (level == ValidationLevel.SchemaOnly)
                return;

            // Validate a laboratory assay export
            if (level == ValidationLevel.LaboratoryExport)
            {
                ValidateLaboratoryExport validateLab = new ValidateLaboratoryExport();

                validateLab.Check(fileName);

                this.AddRange(validateLab); // Report any results from this validation
            }
        }
        #endregion
    }
}
