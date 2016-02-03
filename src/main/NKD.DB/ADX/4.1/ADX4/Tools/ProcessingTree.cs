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
using System.Diagnostics;
using System.ComponentModel;
using ADX4.Tools.Validation;

namespace ADX4.Tools
{
    /// <summary>
    /// This builds and manages a single processing tree.
    /// </summary>
    public class ProcessingTree : List<AssayRecord>
    {
        #region Internal Classes
        /// <summary>
        /// This is a carrier class to pass information inside a recursive method.
        /// </summary>
        private class SearchForTargetDetails
        {
            public SearchForTargetDetails(MaterialDestination destination, Int32 procedureIndex)
            {
                this.destination = destination;
                this.procedureIndex = procedureIndex;
            }

            public SearchForTargetDetails(Protocol protocol, Process process, MaterialDestination destination, Int32 procedureIndex)
            {
                this.protocol = protocol;
                this.process = process;
                this.destination = destination;
                this.procedureIndex = procedureIndex;
            }

            public Protocol protocol;
            public Process process;
            public MaterialDestination destination;
            public Int32 procedureIndex;
        }

        private class ProcessingTarget : MaterialTarget
        {
            #region Constructors

            public ProcessingTarget()
            {
            }

            public ProcessingTarget(MaterialTarget target, Protocol defaultProtocol, Process defaultProcess)
                : base(target)
            {
                m_defaultProtocol = defaultProtocol;
                m_defaultProcess = defaultProcess;
            }

            public ProcessingTarget(ProcessingTarget target)
                : base(target)
            {
                m_defaultProtocol = target.DefaultProtocol;
                m_defaultProcess = target.DefaultProcess;
            }
            #endregion

            #region Properties
            private Protocol m_defaultProtocol;
            public Protocol DefaultProtocol
            {
                get
                {
                    return m_defaultProtocol;
                }
                set
                {
                    m_defaultProtocol = value;
                }
            }

            private Process m_defaultProcess;
            public Process DefaultProcess
            {
                get
                {
                    return m_defaultProcess;
                }
                set
                {
                    m_defaultProcess = value;
                }
            }
            #endregion
        }

        #endregion

        #region Structures
        private class SearchForPathDetails
        {
            public SearchForPathDetails(ProcessingPath path,Int32 tagLevel, MaterialDestination destination,List<Procedure> procedures)
            {
                this.tagLevel = tagLevel;
                this.destination = destination;
                this.path = path;
                this.procedures = procedures;
                this.endOfBranch = false;
            }

            public ProcessingPath path;
            public Int32 tagLevel;
            public MaterialDestination destination;
            public List<Procedure> procedures;
            public Boolean endOfBranch;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a processing tree for a collection of samples.
        /// </summary>
        /// <param name="ADX">The ADX document.</param>
        /// <param name="refs">The list of sample references.</param>
        public ProcessingTree(ADX document, SampleReference[] refs, Validation.ValidationEventCallback onValidationEvent)
        {
            // Add the validation callback
            if (onValidationEvent != null)
                this.OnValidationEvent += onValidationEvent;

            m_document = document; // Set the ADX document

            // Copy the sample references 
            foreach (SampleReference sampleRef in refs)
            {
                SampleReference copyRef = new SampleReference();
                copyRef.IdRef = sampleRef.IdRef;
                m_sampleIds.Add(copyRef);
            }
        }

        public ProcessingTree(ADX document, SampleReference[] refs) : this(document,refs,null)
        {
        }
        #endregion

        #region Browsable Properties
        private StringBuilder m_fullIdBuilder = new StringBuilder();
        /// <summary>
        /// Converts the processing tree into a text string containing the sample identifiers.
        /// </summary>
        /// <returns>
        /// Sample identiiers of the processing tree as a text string.
        /// </returns>
        public override String ToString()
        {
            m_fullIdBuilder.Length = 0;
            for (int i = 0; i < m_sampleIds.Count; i++)
            {
                m_fullIdBuilder.Append(m_sampleIds[i].IdRef);
                if (i < m_sampleIds.Count - 2)
                    m_fullIdBuilder.Append(",");
            }

            return m_fullIdBuilder.ToString();
        }
        #endregion

        #region Properties
        private ADX m_document;
        /// <summary>
        /// Gets the ADX document. 
        /// </summary>
        /// <value>The ADX document.</value>
        public ADX Document
        {
            get
            {
                return m_document;
            }
        }

        private List<SampleReference> m_sampleIds = new List<SampleReference>();
        /// <summary>
        /// Gets the sample references for the processing tree. 
        /// </summary>
        /// <value>The sample references.</value>
        [Browsable(false)]
        public List<SampleReference> SampleRefs
        {
            get
            {
                return m_sampleIds;
            }
        }

        private MaterialDestination m_rootDestination;
        /// <summary>
        /// Gets or sets the root material destination at the top of the processing tree. 
        /// </summary>
        /// <value>The root material destination.</value>
        [Browsable(false)]
        public MaterialDestination Root
        {
            get
            {
                return m_rootDestination;
            }
            private set
            {
                m_rootDestination = value;
            }
        }
        #endregion

        #region Variables
        private ValidationEventCallback OnValidationEvent;
        #endregion

        #region Methods
        /// <summary>
        /// This checks to see if this processing tree matches a list of sample references.
        /// </summary>
        /// <param name="refs">The refs.</param>
        /// <returns>True if the tree uses the same sample references, otherwise false.</returns>
        internal Boolean Equals(SampleReference[] refs)
        {
            if (refs == null)
            {
                if (this.SampleRefs == null || this.SampleRefs.Count == 0)
                    return true;
                else
                    return false;
            }

            if (refs.Length != this.SampleRefs.Count)
                return false;

            // Is each sample reference the same ?
            for (int i = 0; i < refs.Length; i++)
                if (String.Compare(refs[i].IdRef, this.SampleRefs[i].IdRef) != 0)
                    return false;

            return true;
        }

        /// <summary>
        /// Appends the processing history onto the end of the specified processing path.
        /// </summary>
        /// <param name="parentPath">The parent processing path to append onto.</param>
        /// <param name="processing">The processing history.</param>
        internal void AppendProcessingHistory(ProcessingPath parentPath, ADX4.Process processing)
        {
            // Any processing history to append ?
            if (processing == null || processing.Destination == null)
                return;

            // Expand the processing history into a procedures (i.e. replaces ProcessRef, ProcedureRef, etc... for the actual procedures).
            MaterialDestination clonedDestination = processing.Destination.Clone();
            Debug.Assert(clonedDestination != null);

            List<Procedure> expandedProcedures = ExpandToProcedures(clonedDestination, 0, new ProcessingTarget());
            Debug.Assert(expandedProcedures != null);

            // Any procedures to append ?
            if (expandedProcedures.Count == 0)
                return;

            // Do we have a root destination at the top of the process tree ?
            MaterialDestination leafDestination;
            if (this.Root == null)
            {
                this.Root = new MaterialDestination();
                leafDestination = this.Root;
            }
            // Or do we trace the processing path through the current tree.
            else
            {
                leafDestination = SearchForDestination(parentPath); // Find the destination at the end of this processing path !
                if (leafDestination == null)
                {
                    this.OnValidationEvent(new ValidationResult(ErrorCodes.SampleProcessingHistory, ErrorLevel.Error, String.Format(Languages.Strings.valResultPathNotFoundInProcessingHistory, parentPath.ToString(), this.ToString())));
                    return;
                }
            }
            Debug.Assert(leafDestination != null);

            // Add a target to the destination at the end of the processing path (if not one there already)
            if (leafDestination.Target == null)
                leafDestination.Target = new MaterialTarget();

            // Add the procedures onto the target (if no procedures have been set on the target yet!)
            if (leafDestination.Target.Procedure == null)
                leafDestination.Target.Procedure = expandedProcedures.ToArray();
            
            // Otherwise extend the target's procedure list and append the processing history's procedures
            else
            {
                Procedure[] curProcedures = leafDestination.Target.Procedure;
                leafDestination.Target.Procedure = new Procedure[curProcedures.Length + expandedProcedures.Count];

                for (int i = 0; i < curProcedures.Length; i++)
                    leafDestination.Target.Procedure[i] = curProcedures[i];

                for (int i = 0; i < expandedProcedures.Count; i++)
                    leafDestination.Target.Procedure[curProcedures.Length + i] = expandedProcedures[i];
            }
        }

        /// <summary>
        /// Gets the processing history associated with a processing path.
        /// </summary>
        /// <param name="processingPath">The processing path.</param>
        /// <returns></returns>
        internal ADX4.Process GetProcessingHistory(ProcessingPath processingPath)
        {
            // Does this processing tree have a root ? if not then there is not history to return.
            if (this.Root == null)
                return null;

            // If the processing path is null then create an empty processing path to trace.
            if (processingPath == null)
            {
                processingPath = new ProcessingPath();
                processingPath.Tag = new ProcessingTag[0];
            }

            // Trace the processing path and build the list of procedures on it
            SearchForPathDetails finalSearch = SearchForPath(new SearchForPathDetails(processingPath, 0, this.Root, new List<Procedure>()));
            if (finalSearch == null || (finalSearch.tagLevel < processingPath.Tag.Length))
            {
                this.OnValidationEvent(new ValidationResult(ErrorCodes.SampleProcessingHistory,ErrorLevel.Error,String.Format(Languages.Strings.valResultPathNotFoundInProcessingHistory,processingPath.ToString(),this.SampleRefs[0].IdRef)));
                return null;
            }

            // Return the list of procedures for this processing path
            Process history = new Process();
            history.Destination = new MaterialDestination();
            history.Destination.Target = new MaterialTarget();
            history.Destination.Target.Procedure = finalSearch.procedures.ToArray();
            return history;
        }

        /// <summary>
        /// Gets the <see cref="ADX4.Tools.AssayRecord"/> associated with the specified processing path.
        /// </summary>
        /// <value>The requested assay record.</value>
        public AssayRecord this[ProcessingPath path]
        {
            get
            {
                // Search the Assay Records for the record with the requested processing path
                foreach (AssayRecord assayRecord in this)
                {
                    if (assayRecord.ProcessingPath == null)
                    {
                        if (path == null)
                            return assayRecord;
                    }
                    else if (assayRecord.ProcessingPath.Equals(path))
                        return assayRecord;
                }

                return null;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Expands all the processing (i.e. ProcessRefs, ProcedureRef, etc...) under this destination into procedures. By converting all processing into procedures, searching and tracing is considerably easier.
        /// </summary>
        /// <param name="parentDestination">The parent destination to convert into procedures.</param>
        /// <param name="startProcedureIndex">The start procedure within the material target on the parent destination from which to begin converting.</param>
        /// <param name="defaultTarget">The default target describing the current procotol and process.</param>
        /// <returns>The expanded list of procedures</returns>
        private List<Procedure> ExpandToProcedures(MaterialDestination parentDestination,Int32 startProcedureIndex,ProcessingTarget defaultTarget)
        {
            List<Procedure> procedures = new List<Procedure>();

            // Does this destination have a target to expand ? if not then just return an empty procedure list
            if (parentDestination == null || !parentDestination.HasTarget)
                return procedures;
            
            // Invalid target ?
            if (!parentDestination.IsValidTarget)
                this.OnValidationEvent(new ValidationResult(ErrorCodes.SampleProcessingHistory,ErrorLevel.Error,String.Format(Languages.Strings.valResultInvalidMaterialTarget,parentDestination.Target == null ? "?" : parentDestination.Target.ToString())));

            // Does this destination have a list of procedures as its target ? if so then expand each procedure
            if (parentDestination.Target.HasProcedure())
            {
                // Add each procedure in the target and expand the procedure's destinations
                for (Int32 i = startProcedureIndex; i < parentDestination.Target.Procedure.Length; i++)
                {
                    procedures.Add(parentDestination.Target.Procedure[i]);

                    // Expand each destination into procedures
                    List<MaterialDestination> destinations = parentDestination.Target.Procedure[i].GetDestinations();
                    foreach (MaterialDestination destination in destinations)
                    {
                        MaterialTarget target = new MaterialTarget();
                        target.Procedure = this.ExpandToProcedures(destination, 0, defaultTarget).ToArray();
                        destination.Target = target;
                    }
                }
            }

            // Does the destination have references to a protocol, process or procedure name instead? 
            // if so then set the default target to the current protocol etc... and expand the reference
            else if (parentDestination.Target.IsSpecified)
            {
                ProcessingTarget searchTarget = new ProcessingTarget(defaultTarget);

                // Is there a protocol reference ?
                if (parentDestination.Target.HasProtocol())
                    searchTarget.ProtocolRef = parentDestination.Target.ProtocolRef;

                // Is there a process reference ?
                if (parentDestination.Target.HasProcessName())
                    searchTarget.ProcessRef = parentDestination.Target.ProcessRef;

                // Is there a procedure reference ?
                if (parentDestination.Target.HasProcedureName())
                    searchTarget.ProcedureRef = parentDestination.Target.ProcedureRef;

                // Is there a material source reference ?
                if (parentDestination.Target.HasMaterialSourceName())
                    searchTarget.MaterialSourceRef = parentDestination.Target.MaterialSourceRef;

                // Is the current target's references valid (i.e. is there a protocol or process that matches the specified protocol or process, etc...).
                SearchForTargetDetails match = this.SearchForTarget(searchTarget);

                // If the target is valid and points to another position in the ADX document then jump to that place and continue expanding into procedures
                if (match != null && match.destination != null)
                {
                    searchTarget.DefaultProcess = match.process;
                    searchTarget.DefaultProtocol = match.protocol;
                    procedures.AddRange(this.ExpandToProcedures(match.destination.Clone(), match.procedureIndex, searchTarget));
                }
            }

            return procedures;
        }

        /// <summary>
        /// Searches for the destination at the end of the processing path.
        /// </summary>
        /// <param name="path">The processing path to trace.</param>
        /// <returns>Material destination</returns>
        private MaterialDestination SearchForDestination(ProcessingPath path)
        {
            // Does the processing trace have any history to trace ?
            if (this.Root == null)
                return null;

            // Is there a processing path to trace ? if not then create an empty path to trace.
            if (path == null)
                path = new ProcessingPath();
            if (path.Tag == null)
                path.Tag = new ProcessingTag[0];

            // Search the processing tree for the destination associated with the processing path
            SearchForPathDetails finalSearch = SearchForPath(new SearchForPathDetails(path, 0, this.Root, new List<Procedure>()));
            //Debug.Assert(finalSearch != null);
            //Debug.Assert(path.Tag != null);
            return finalSearch == null || finalSearch.tagLevel != path.Tag.Length ? null : finalSearch.destination;
        }

        /// <summary>
        /// Searches for the end of the processing path.
        /// </summary>
        /// <param name="searchLevel">The search level within the processing path.</param>
        /// <returns>Position at the end of the processing path</returns>
        private SearchForPathDetails SearchForPath(SearchForPathDetails searchLevel)
        {
            // Has the search passed the end of the length of the processing path ? i.e. the path has 5 tags so we only need to search 5 tags into the processing tree
            Debug.Assert(searchLevel != null);
            Debug.Assert(searchLevel.path != null);
            Debug.Assert(searchLevel.path.Tag != null);
            if (searchLevel.tagLevel > searchLevel.path.Tag.Length)
                return searchLevel;

            // Does the current search point have a target? or have we reached the end of a branch (and must stop searching whether we have compleed the processing path or not)
            if (searchLevel.destination == null ||
                searchLevel.destination.Target == null ||
                searchLevel.destination.Target.Procedure == null ||
                searchLevel.destination.Target.Procedure.Length == 0)
            {
                searchLevel.endOfBranch = true;
                return searchLevel;
            }

            // Add the current search destination's procedures to the cummulative list of  procedures
            for (Int32 i = 0; i < searchLevel.destination.Target.Procedure.Length; i++)
                searchLevel.procedures.Add(searchLevel.destination.Target.Procedure[i]);

            // Get the destinations on the last procedure (we will continue searching along one of these destinations).
            List<MaterialDestination> destinations = searchLevel.destination.Target.Procedure[searchLevel.destination.Target.Procedure.Length - 1].GetDestinations();

            // If we have not reached the end of the processing path then find the destination that matches the path and continue searching down that one.
            if (searchLevel.tagLevel < searchLevel.path.Tag.Length)
                foreach (MaterialDestination destination in destinations)
                    if (searchLevel.path.Tag[searchLevel.tagLevel].Equals(destination.Name, destination.Type))
                        return SearchForPath(new SearchForPathDetails(searchLevel.path, searchLevel.tagLevel + 1, destination, searchLevel.procedures));

            // If there are multiple destinations and no destination was found to match the processing path then follow the first destination with an empty processing tag
            if (destinations.Count > 0)
                // Is the tag empty ?
                for (int i = 0; i < destinations.Count; i++)
                {
                    if (String.IsNullOrEmpty(destinations[i].Name) && String.IsNullOrEmpty(destinations[i].Type))
                    {
                        // Does this destination have a target to search ? if so then search down it.
                        if (destinations[i].HasTarget || destinations.Count > 1)
                        {
                            return SearchForPath(new SearchForPathDetails(searchLevel.path, searchLevel.tagLevel, destinations[i], searchLevel.procedures));
                        }

                        // If not then we have reached the end of a branch and must stop searching
                        else
                        {
                            searchLevel.endOfBranch = true;
                            return searchLevel;
                        }
                    }
                }

            return null;
        }


        /// <summary>
        /// Searches for the protocol, process ,etc... that the target jumps to.
        /// </summary>
        /// <param name="searchTarget">The search target.</param>
        /// <returns>Details describing the target to jump to.</returns>
        private SearchForTargetDetails SearchForTarget(ProcessingTarget searchTarget)
        {
            // Are there any protocols, processes etc... to search ?
            if (searchTarget == null || this.Document == null || this.Document.Protocols == null || this.Document.Protocols.Protocol == null)
                return null;

            // Does the seacrh target jump to a protcol ?
            Protocol searchProtocol = null;
            if (searchTarget.HasProtocol())
            {
                // If so then find the protocol 
                foreach (Protocol protocol in this.Document.Protocols.Protocol)
                    if (String.Equals(protocol.Id, searchTarget.ProtocolRef))
                    {
                        searchProtocol = protocol;
                        break;
                    }
            }
            // If not then use the first protocol in the ADX document
            else
            {
                searchProtocol = searchTarget.DefaultProtocol;
                if (searchProtocol == null && this.Document.Protocols.Protocol != null)
                    if (this.Document.Protocols.Protocol.Length > 0)
                        searchProtocol = this.Document.Protocols.Protocol[0];
            }

            // Do we have a protocol to search ? if not then we must stop.
            if (searchProtocol == null)
                return null;

            // Does the search target jump to a process within the current protocol
            Process searchProcess = null;
            if (searchTarget.HasProcessName())
            {
                // Search for the specified process
                foreach (Process process in searchProtocol.Process)
                    if (String.Equals(process.Id, searchTarget.ProcessRef))
                    {
                        searchProcess = process;
                        break;
                    }
            }
            // If not then use the first process in the first protocol
            else
            {
                searchProcess = searchTarget.DefaultProcess;
                if (searchProcess == null)
                    if (this.Document.Protocols.Protocol.Length > 0 && this.Document.Protocols.Protocol[0].Process != null)
                        if (this.Document.Protocols.Protocol[0].Process.Length > 0)
                            searchProcess = this.Document.Protocols.Protocol[0].Process[0];
            }

            // Do we have a process to search ? if not then stop searching.
            if (searchProcess == null)
                return null;

            // Do we start from the top of the current process (i.e. no ProcedureName was specified to jump to).
            if (!searchTarget.HasProcedureName())
                return new SearchForTargetDetails(searchProtocol, searchProcess, searchProcess.Destination, 0);

            // Find the specified procedure in the current process and jump to that procedure
            SearchForTargetDetails match = SearchForTargetProcedure(searchTarget, searchProcess.Destination);
            if (match != null)
            {
                match.protocol = searchProtocol;
                match.process = searchProcess;
            }
            return match;
        }

        /// <summary>
        /// Searches for target procedure below a specified material destination.
        /// </summary>
        /// <param name="searchTarget">The search target's procedure.</param>
        /// <param name="parentDestination">The material destination to begin searching from.</param>
        /// <returns>Details describing the requested procedure</returns>
        private SearchForTargetDetails SearchForTargetProcedure(ProcessingTarget searchTarget, MaterialDestination parentDestination)
        {
            // Is there a destination to search ?
            if (parentDestination == null || parentDestination.Target == null || !parentDestination.Target.HasProcedure())
                return null;

            // Search each procedure for a match 
            for (Int32 i = 0; i < parentDestination.Target.Procedure.Length; i++)
            {
                // If this procedure matches then return it as the jump point
                if (String.Equals(parentDestination.Target.Procedure[i].Id, searchTarget.ProcedureRef))
                    return new SearchForTargetDetails(parentDestination, i);

                // Otherwise recursively search for the requested procedure
                List<MaterialDestination> destinations = parentDestination.Target.Procedure[i].GetDestinations();
                foreach (MaterialDestination destination in destinations)
                {
                    SearchForTargetDetails match = SearchForTargetProcedure(searchTarget, destination);
                    if (match != null)
                        return match;
                }
            }

            return null;
        }
        #endregion
    }
}
