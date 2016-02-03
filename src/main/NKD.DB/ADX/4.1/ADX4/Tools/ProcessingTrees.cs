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
using ADX4.Tools.Validation;

namespace ADX4.Tools
{
    /// <summary>
    /// This is a list of sample processing trees generated from an ADX document. 
    /// Each processing tree represents all processing performed on a single sample or sample composite.
    /// </summary>
    public class ProcessingTrees : List<ProcessingTree>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingTrees"/> class.
        /// </summary>
        /// <param name="document">The ADX document.</param>
        public ProcessingTrees(ADX document,ValidationEventCallback onValidationEvent)
        {
            // Add the validation callback
            this.OnValidationEvent += this.OnValidationEventSink;
            if (onValidationEvent != null)
                this.OnValidationEvent += onValidationEvent;

            m_document = document;

            Build(); // Build the processing trees
        }

        public ProcessingTrees(ADX document)
            : this(document, null)
        {
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

        #region Properties
        private ADX m_document;
        /// <summary>
        /// Gets the ADX document associated with these processing trees. 
        /// </summary>
        /// <value>The ADX document.</value>
        internal ADX Document
        {
            get
            {
                return m_document;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds all the procesing trees described by the ADX document.
        /// </summary>
        private void Build()
        {
            // Remove any previous tree
            this.Clear();

            // Anything to process ?
            if (this.Document == null || this.Document.Results == null || this.Document.Results.ProcessingGroup == null)
                return;

            // Add each processing group to the processing tree
            Boolean isFirstprocessingGroup = true;
            foreach (ADX4.ProcessingGroup processingGroup in this.Document.Results.ProcessingGroup)
            {
                if (processingGroup.Result == null)
                    continue;

                // Add each result's analysis records
                foreach (ADX4.Result result in processingGroup.Result)
                {
                    // No sample references for this result ?
                    if (result.Sample == null || result.Sample.Length == 0 || String.IsNullOrEmpty(result.Sample[0].IdRef))
                        continue;

                    // Find the sample's processing associated with this result
                    ProcessingTree processingTree = this[result.Sample];
                    if (processingTree == null)
                    {
                        // If the sample has not yet been found then add a new sample
                        processingTree = new ProcessingTree(this.Document, result.Sample,this.OnValidationEvent);
                        this.Add(processingTree);
                    }
                    
                    // No parent processing path is defined
                    if (result.ParentProcessingPath == null && !isFirstprocessingGroup && this.Document.Results.ProcessingGroup.Length > 1)
                        this.OnValidationEvent(new ValidationResult(ErrorCodes.SampleProcessingHistory, ErrorLevel.Warning, String.Format(Languages.Strings.valResultNoParentProcessingPath, result.Sample[0].IdRef, (String.IsNullOrEmpty(processingGroup.Id) ? "?" : processingGroup.Id))));

                    // No processing history on result
                    if (result.ProcessingHistory == null)
                        this.OnValidationEvent(new ValidationResult(ErrorCodes.SampleProcessingHistory, ErrorLevel.Warning, String.Format(Languages.Strings.valResultNoProcessingHistoryDefined, result.Sample[0].IdRef)));

                    // Append this result's processing history to the matching sample
                    processingTree.AppendProcessingHistory(result.ParentProcessingPath, result.ProcessingHistory);

                    // Are there any analyses to be reported?
                    if (result.Analysis == null)
                        continue;

                    // Add each analysis record in this sample result
                    foreach (ADX4.AnalysisRecord analysis in result.Analysis)
                    {
                        // Is there an analysis already for this processing path ?
                        AssayRecord assayRecord = processingTree[analysis.FullProcessingPath];

                        // If not then add it otherwise merge the current analysis's measurements with its previous matching measurements
                        if (assayRecord == null)
                        {
                            // Create the analysis record
                            assayRecord = new AssayRecord(processingTree, analysis.FullProcessingPath);
                            processingTree.Add(assayRecord);
                        }
                        else
                            assayRecord.ProcessingHistory = processingTree.GetProcessingHistory(analysis.FullProcessingPath); // Update the processing history provided by this analysis

                        // Add (or merge) each measurement in this analysis to the matching analysis record
                        foreach (Measurement measurement in analysis.Measurement)
                        {
                            // TODO must merge previous measurements with same analyte (this happens if measurements are made from the same sample+processpath but reported in a different analysis record)

                            // Does this analysis have an AnalysisProcedure with the analytical parameters ?
                            Procedure searchProcedure  = null;
                            if (assayRecord.ProcessingHistory != null)
                                searchProcedure = assayRecord.ProcessingHistory.FindProcedure(measurement.ProcedureRef);

                            if (searchProcedure == null)
                            {
                                AnalysisProcedure procedure = new AnalysisProcedure();
                                procedure.Id = (measurement.ProcedureRef == null ? measurement.Property.ToString() : measurement.ProcedureRef);
                                procedure.Property = measurement.Property;
                                procedure.UOM = (measurement.Value == null ? Enum.GetName(typeof(PropertyTypeStrict), UOMStrict.Unspecified) : measurement.Value.UOM);
                                searchProcedure = procedure;
                            }
                            AssayMeasurement analyteMeasurement = new AssayMeasurement(measurement, searchProcedure);

                            // Add the measurement to the analysis
                            assayRecord.Add(analyteMeasurement);
                        }
                    }
                    isFirstprocessingGroup = false;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ADX4.Tools.ProcessingTree"/> with the specified sample identifiers.
        /// </summary>
        /// <value>A single processing tree</value>
        public ProcessingTree this[SampleReference[] refs]
        {
            get
            {
                // Search the processing tree for a particular branch corresponding to the specified samples
                foreach (ProcessingTree processingTree in this)
                    if (processingTree.Equals(refs))
                        return processingTree;

                return null;
            }
        }
        #endregion
    }
}
