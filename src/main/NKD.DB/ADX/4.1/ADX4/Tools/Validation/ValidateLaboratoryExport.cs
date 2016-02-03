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
    /// This validates an ADX file produced as a laboratory export.
    /// </summary>
    internal class ValidateLaboratoryExport : List<ValidationResult>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateLaboratoryExport"/> class.
        /// </summary>
        public ValidateLaboratoryExport()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks the ADX file against the rules for a valid laboratory export.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Check(String fileName)
        {
            try
            {
                ADX document = ADX.Load(fileName);

                CheckHeader(document);

                CheckChainOfCustody(document);

                CheckSamples(document);

                CheckResults(document);

                AssayResults assayResults = new AssayResults(document, OnValidationEvent);
            }
            catch (System.Exception exc)
            {
            }
        }

        /// <summary>
        /// Called when a validation event occurs inside support class.
        /// </summary>
        /// <param name="result">The result.</param>
        public void OnValidationEvent(ValidationResult result)
        {
            this.Add(result);
        }

        /// <summary>
        /// Checks the ADX header contents.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        private void CheckHeader(ADX document)
        {
            // 
            // Samples Owner
            //

            // Is there a header section defined ?
            if (document.Header == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeader));
                return;
            }

            // Is there a samples owner section defined ?
            if (document.Header.SamplesOwner == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderLaboratory));
            }
            else
            {
                // Is a samples owner location reference defined ?
                if (String.IsNullOrEmpty(document.Header.SamplesOwner.LocationRef))
                    this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderSamplesOwnerLocationRef));

                // Is a project defined ?
                if (document.Header.SamplesOwner.Project == null)
                    this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Warning, Languages.Strings.valNoHeaderSamplesOwnerProject));
                else
                {
                    if (String.IsNullOrEmpty(document.Header.SamplesOwner.Project.Id))
                        this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Warning, Languages.Strings.valNoHeaderSamplesOwnerProject));
                }
            }

            // 
            // Despatch
            //

            // Is there a despatch section defined ?
            if (document.Header.Despatch == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Warning, Languages.Strings.valNoHeaderDespatch));
            }
            else
            {
                // Is a despatch number specified ?
                if (String.IsNullOrEmpty(document.Header.Despatch.DespatchNo))
                    this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Warning, Languages.Strings.valNoHeaderDespatchSpecified));
            }

            //
            // Laboratory
            //

            // Is there a laboratory defined ?
            if (document.Header.Laboratory == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderLaboratory));
                return;
            }
            else
            {
                // Is a laboratory location reference defined ?
                if (String.IsNullOrEmpty(document.Header.Laboratory.LocationRef))
                    this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderLaboratoryLocationRef));
            }

            //
            // Analytical Report
            //

            // Is there an analytical report defined ?
            if (document.Header.AnalyticalReport == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderAnalyticalReport));
                return;
            }
            else
            {
                // Is a laboratory job no defined ?
                if (String.IsNullOrEmpty(document.Header.AnalyticalReport.LabJob))
                    this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderAnalyticalReportLabJobNo));

                // Unspecified analytical status ?
                if (document.Header.AnalyticalReport.Status == AnalyticalReportStatus.Unspecified)
                    this.Add(new ValidationResult(ErrorCodes.Header, ErrorLevel.Error, Languages.Strings.valNoHeaderAnalyticalReportStatus));
            }

        }

        /// <summary>
        /// Checks the ADX chain of custody.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        private void CheckChainOfCustody(ADX document)
        {
            // Is there a chain of custody section defined ?
            if (document.ChainOfCustody == null)
            {
                this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, Languages.Strings.valNoChainOfCustody));
                return;
            }

            // Is there a chain of custody batch section defined ?
            if (document.ChainOfCustody.Batch == null)
            {
                this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, Languages.Strings.valNoChainOfCustodyBatch));
                return;
            }

            //
            // Is there a ReceiveProcedure for this lab ?
            //

            ChainOfCustodyBatch receiveBatch = null;
            Procedure receiveProcedure = null;
            if (document.Header != null && document.Header.Laboratory != null && document.Header.Laboratory.LocationRef != null)
            {
                // Check each batch for the lab's ReceiveProcedure
                String labRef = document.Header.Laboratory.LocationRef;
                foreach (ChainOfCustodyBatch batch in document.ChainOfCustody.Batch)
                {
                    // Are there any procedures in this batch's history ?
                    if (batch.History == null || batch.History == null || batch.History.Destination == null || batch.History.Destination.Target == null || batch.History.Destination.Target.Procedure == null)
                        continue;

                    foreach (Procedure procedure in batch.History.Destination.Target.Procedure)
                    {
                        // If this is a ReceiveProcedure then see if it is for the document's laboratory
                        if (procedure is ReceiveProcedure)
                        {
                            ReceiveProcedure cofcRecieve = procedure as ReceiveProcedure;
                            if (String.IsNullOrEmpty(cofcRecieve.LocationRef))
                                continue;

                            // Is this for this laboratory ?
                            if (String.Equals(cofcRecieve.LocationRef, labRef))
                            {
                                receiveProcedure = cofcRecieve;
                                receiveBatch = batch;
                                break;
                            }
                        }
                    }

                    if (receiveProcedure != null)
                        break;
                }
            }

            if (receiveProcedure == null)
            {
                this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, Languages.Strings.valNoChainOfCustodyLabReceiveProcedure));
                return;
            }

            //
            // Are there any samples listed in the Chain Of Custody batch ?
            //
            
            if (receiveBatch.SampleMaterial == null)
            {
                this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, Languages.Strings.valNoChainOfCustodyLabReceiveSamples));
                return;
            }

            //
            // Check that every chain of custody sample has a valid sample reference
            //
            
            foreach (ChainOfCustodyBatch batch in document.ChainOfCustody.Batch)
            {
                if (batch.SampleMaterial == null)
                    return;

                // Check that each sample is valid in this batch 
                foreach (ChainOfCustodySample cofcSample in batch.SampleMaterial)
                {
                    if (cofcSample.Sample == null || cofcSample.Sample.Length == 0)
                    {
                        this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, Languages.Strings.valChainOfCustodySampleHasNoRef));
                    }
                    else
                    {
                        // Are there any samples listed in the document ?
                        if (document.Samples == null || document.Samples.Sample.Length == 0)
                        {
                            this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, String.Format(Languages.Strings.valChainOfCustodySampleRefDoesNotExist, "*")));
                        }
                        else
                        {
                            // Make sure each sampleRef is listed in the Samples section
                            foreach (SampleReference sampleRef in cofcSample.Sample)
                                if (document.Samples.Find(sampleRef) == null)
                                    this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, String.Format(Languages.Strings.valChainOfCustodySampleRefDoesNotExist, sampleRef.IdRef)));
                        }
                    }
                }
            }

            //
            // Are all the samples listed in the Chain Of Custody ReceiveProcedure
            //

            if (document.Samples.Sample != null)
            {
                foreach (Sample sample in document.Samples.Sample)
                {
                    if (!(sample is ReferenceMaterial))
                        if (receiveBatch.Find(new SampleReference(sample.Id)) == null)
                            this.Add(new ValidationResult(ErrorCodes.ChainOfCustody, ErrorLevel.Error, String.Format(Languages.Strings.valSampleNotReceivedByLaboratory, sample.Id)));
                }
            }
        }

        /// <summary>
        /// Checks the ADX samples.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        private void CheckSamples(ADX document)
        {
            // Is there a samples section defined ?
            if (document.Samples == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Error, Languages.Strings.valNoSamples));
                return;
            }

            // Are there samples in the samples section ?
            if (document.Samples.Sample == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Error, Languages.Strings.valNoSamplesListed));
                return;
            }

            //
            // Check if each sample is defined correctly
            //

            foreach (Sample sample in document.Samples.Sample)
            {
                // Is the ID defined ?
                if (String.IsNullOrEmpty(sample.Id))
                    this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Error, Languages.Strings.valSampleExistsWithNoId));

                else
                {
                    // Is the Type defined ?
                    if (String.IsNullOrEmpty(sample.Type) && !(sample is ReferenceMaterial))
                        this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Warning, String.Format(Languages.Strings.valSampleExistsWithNoType, sample.Id)));

                    // Is the Status defined ?
                    if (sample.Status == SampleStatus.Unspecified)
                        this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Error, String.Format(Languages.Strings.valSampleHasAnUnspecifiedStatus, sample.Id)));

                    // Is this a standard or blank ?
                    if (sample is ReferenceMaterial)
                    {
                        ReferenceMaterial rm = sample as ReferenceMaterial;
                        if (rm.Category == ReferenceMaterialCategory.Unspecified)
                            this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Error, String.Format(Languages.Strings.valRefMatHasAnUnspecifiedCategory, sample.Id)));
                    }
                    // Check if a QA/QC sample has been hidden as a normal sample...
                    else
                    {
                        String compressedSampleId = sample.Id.ToUpper().Replace(" ", "");
                        if (
                            compressedSampleId.EndsWith("D") ||
                            compressedSampleId.Contains("-DUPLICATE") ||
                            compressedSampleId.Contains("-DUP") ||
                            compressedSampleId.Contains("-D") ||
                            compressedSampleId.Contains("DUP") ||
                            compressedSampleId.EndsWith("R") ||
                            compressedSampleId.Contains("-REP") ||
                            compressedSampleId.Contains("-REPEAT") ||
                            compressedSampleId.Contains("-R") ||
                            compressedSampleId.Contains("DUP") ||
                            compressedSampleId.EndsWith("CRD") ||
                            compressedSampleId.Contains("/") ||
                            compressedSampleId.Contains("BLK") ||
                            compressedSampleId.Contains("BLANK") ||
                            compressedSampleId.Contains("STD")
                            )
                            this.Add(new ValidationResult(ErrorCodes.Samples, ErrorLevel.Warning, String.Format(Languages.Strings.valSamplePotentialQAQCListedAsSample, sample.Id)));
                    }
                }
            }
        }

        /// <summary>
        /// Checks the ADX results.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        private void CheckResults(ADX document)
        {
            // Is there a results section defined ?
            if (document.Results == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, Languages.Strings.valNoResults));
                return;
            }

            // Are there processing groups in the results section ?
            if (document.Results.ProcessingGroup == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, Languages.Strings.valNoResultsProcessingGroups));
                return;
            }
            
            // Does each processing group contain results ?
            foreach (ProcessingGroup processingGroup in document.Results.ProcessingGroup)
            {
                // Does it have an ID ?
                if (String.IsNullOrEmpty(processingGroup.Id))
                    this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Warning, Languages.Strings.valNoIdForProcessingGroup));

                // Any results ?
                if (processingGroup.Result == null)
                    this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoResultsResult, processingGroup.ToString())));
                else
                {
                    // Check each result
                    foreach (Result result in processingGroup.Result)
                        CheckResult(document, result);
                }
            }
        }

        /// <summary>
        /// Checks the single ADX result.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        /// <param name="result">The ADX result.</param>
        private void CheckResult(ADX document, Result result)
        {
            String sampleIdentifier = result.ToString();

            // No processing history defined ?
            if (result.ProcessingHistory == null)
                this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoResultProcessingHistory, sampleIdentifier)));

            // Any sample references ?
            if (result.Sample == null || result.Sample.Length == 0)
                this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoSampleRefDefinedForResult, sampleIdentifier)));

            // Does each sample reference refer to a sample ?
            else if (document.Samples != null)
            {
                foreach (SampleReference sampleRef in result.Sample)
                    if (document.Samples.Find(sampleRef) == null)
                        this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valUnknownResultSampleRef, sampleRef.IdRef)));
            }

            // Check each result analysis record
            if (result.Analysis != null)
                foreach (AnalysisRecord analysisRecord in result.Analysis)
                    CheckAnalysisRecord(document, result, analysisRecord);
        }

        /// <summary>
        /// Checks a single analysis record.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        /// <param name="result">The ADX result.</param>
        /// <param name="analysisRecord">The ADX analysis record.</param>
        private void CheckAnalysisRecord(ADX document, Result result, AnalysisRecord analysisRecord)
        {
            // No measurements specified for this analysis record
            if (analysisRecord.Measurement == null)
            {
                this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoAnalysisRecordMeasurementsSpecified, result.ToString())));
                return;
            }
            
            // Check the measurements
            foreach (Measurement measurement in analysisRecord.Measurement)
            {
                // Is there an anlyte specified ?
                if (String.IsNullOrEmpty(measurement.Property))
                {
                    this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoAnalyteSpecified, result.ToString())));
                }
                // Is there a reccommended analyte specified ?
                else
                {
                    try
                    {
                        PropertyTypeStrict strictAnalyte = (PropertyTypeStrict)Enum.Parse(typeof(PropertyTypeStrict), measurement.Property);
                    }
                    catch (System.Exception exc)
                    {
                        this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Warning, String.Format(Languages.Strings.valNoStrictAnalyteSpecified, measurement.Property)));
                    }
                }

                // Is the Status defined ?
                if (measurement.Status == MeasurementStatus.Unspecified)
                    this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valMeasurementHasAnUnspecifiedStatus, measurement.ToString(), result.ToString())));

                // Is the Status defined ?
                if (String.IsNullOrEmpty(measurement.ProcedureRef))
                    this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Warning, String.Format(Languages.Strings.valNoMeasurementProcedureRef, measurement.ToString(), result.ToString())));

                // Is there a measurement value ?
                if (measurement.Value == null)
                {
                    this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoMeasurementValue, measurement.ToString(), result.ToString())));
                }
                else
                {
                    if (String.IsNullOrEmpty(measurement.Value.UOM))
                        this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoMeasurementValueUOM, measurement.ToString(), result.ToString())));
                    else
                    {
                        try
                        {
                            UOMStrict strictUOM = (UOMStrict)Enum.Parse(typeof(UOMStrict), measurement.Value.UOM);
                        }
                        catch (System.Exception exc)
                        {
                            this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Warning, String.Format(Languages.Strings.valNoStrictUOMSpecified, measurement.Value.UOM)));
                        }
                    }

                    // Is the measurement a number ?
                    if (String.IsNullOrEmpty(measurement.Value.Value))
                        this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Error, String.Format(Languages.Strings.valNoMeasurementValue, measurement.ToString(), result.ToString())));
                    else
                    {
                        try
                        {
                            Double v = Double.Parse(measurement.Value.Value);
                        }
                        catch (System.Exception exc)
                        {
                            this.Add(new ValidationResult(ErrorCodes.Results, ErrorLevel.Warning, String.Format(Languages.Strings.valNoMeasurementNumberValue, measurement.Property, measurement.Value.Value, result.ToString())));
                        }
                    }
                }
            }            
        }
        #endregion
    }
}
