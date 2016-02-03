USE NKDADX
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_AnalysisMethod_AnalysisProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE AnalysisMethod DROP CONSTRAINT FK_AnalysisMethod_AnalysisProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_AnalysisProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE AnalysisProcedure DROP CONSTRAINT FK_AnalysisProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_AnalysisRecord_Result') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE AnalysisRecord DROP CONSTRAINT FK_AnalysisRecord_Result
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_AnalysisReference_ReferenceMaterial') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE AnalysisReference DROP CONSTRAINT FK_AnalysisReference_ReferenceMaterial
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_AnalyticalReportDetails_Header') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE AnalyticalReportDetails DROP CONSTRAINT FK_AnalyticalReportDetails_Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_BillingDetails_Header') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE BillingDetails DROP CONSTRAINT FK_BillingDetails_Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ChainOfCustodyBatch_ChainOfCustody') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ChainOfCustodyBatch DROP CONSTRAINT FK_ChainOfCustodyBatch_ChainOfCustody
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ChainOfCustodySample_ChainOfCustodyBatch') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ChainOfCustodySample DROP CONSTRAINT FK_ChainOfCustodySample_ChainOfCustodyBatch
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_CompositingProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE CompositingProcedure DROP CONSTRAINT FK_CompositingProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ContactDetails_AddressBook') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ContactDetails DROP CONSTRAINT FK_ContactDetails_AddressBook
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_DecompositionProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE DecompositionProcedure DROP CONSTRAINT FK_DecompositionProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_DespatchDetails_Header') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE DespatchDetails DROP CONSTRAINT FK_DespatchDetails_Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_DisposeProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE DisposeProcedure DROP CONSTRAINT FK_DisposeProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_DocumentAuthorDetails_Header') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE DocumentAuthorDetails DROP CONSTRAINT FK_DocumentAuthorDetails_Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_DryingProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE DryingProcedure DROP CONSTRAINT FK_DryingProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_GenericProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE GenericProcedure DROP CONSTRAINT FK_GenericProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_LaboratoryDetails_Header') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE LaboratoryDetails DROP CONSTRAINT FK_LaboratoryDetails_Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_LocationDetails_AddressBook') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE LocationDetails DROP CONSTRAINT FK_LocationDetails_AddressBook
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_AnalysisProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_AnalysisProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_CompositingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_CompositingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_DecompositionProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_DecompositionProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_DryingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_DryingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_GenericProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_GenericProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_ScreeningProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_ScreeningProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_ParticleSizeReductionProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_ParticleSizeReductionProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_StorageProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_StorageProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_SplittingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_SplittingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_Process') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_Process
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_WeighingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_WeighingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_SendProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_SendProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_ReceiveProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_ReceiveProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialDestination_RackPlacementProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialDestination DROP CONSTRAINT FK_MaterialDestination_RackPlacementProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialSource_CompositingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialSource DROP CONSTRAINT FK_MaterialSource_CompositingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_MaterialTarget_MaterialDestination') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE MaterialTarget DROP CONSTRAINT FK_MaterialTarget_MaterialDestination
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Measurement_AnalysisRecord') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Measurement DROP CONSTRAINT FK_Measurement_AnalysisRecord
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ParticleSizeReductionProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ParticleSizeReductionProcedure DROP CONSTRAINT FK_ParticleSizeReductionProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Procedure_MaterialTarget') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Procedure DROP CONSTRAINT FK_Procedure_MaterialTarget
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Process_ChainOfCustodyBatch') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Process DROP CONSTRAINT FK_Process_ChainOfCustodyBatch
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Process_DespatchDetails') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Process DROP CONSTRAINT FK_Process_DespatchDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Process_Protocol') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Process DROP CONSTRAINT FK_Process_Protocol
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Process_Result') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Process DROP CONSTRAINT FK_Process_Result
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProcessingGroup_ReferenceMaterials') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProcessingGroup DROP CONSTRAINT FK_ProcessingGroup_ReferenceMaterials
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProcessingGroup_Results') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProcessingGroup DROP CONSTRAINT FK_ProcessingGroup_Results
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProcessingPath_AnalysisRecord') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProcessingPath DROP CONSTRAINT FK_ProcessingPath_AnalysisRecord
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProcessingPath_ChainOfCustodySample') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProcessingPath DROP CONSTRAINT FK_ProcessingPath_ChainOfCustodySample
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProcessingPath_Result') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProcessingPath DROP CONSTRAINT FK_ProcessingPath_Result
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProcessingTag_ProcessingPath') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProcessingTag DROP CONSTRAINT FK_ProcessingTag_ProcessingPath
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ProjectDetails_SamplesOwnerDetails') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ProjectDetails DROP CONSTRAINT FK_ProjectDetails_SamplesOwnerDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Protocol_Protocols') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Protocol DROP CONSTRAINT FK_Protocol_Protocols
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_AnalysisProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_AnalysisProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_DecompositionProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_DecompositionProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_DryingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_DryingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_MaterialDestination') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_MaterialDestination
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_Measurement') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_Measurement
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_string') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_string
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Quantity_WeighingProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Quantity DROP CONSTRAINT FK_Quantity_WeighingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_RackPlacementProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE RackPlacementProcedure DROP CONSTRAINT FK_RackPlacementProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ReceiveProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ReceiveProcedure DROP CONSTRAINT FK_ReceiveProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ReferenceMaterial_Sample') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ReferenceMaterial DROP CONSTRAINT FK_ReferenceMaterial_Sample
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Result_ProcessingGroup') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Result DROP CONSTRAINT FK_Result_ProcessingGroup
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Sample_Samples') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Sample DROP CONSTRAINT FK_Sample_Samples
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_SampleReference_ChainOfCustodySample') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE SampleReference DROP CONSTRAINT FK_SampleReference_ChainOfCustodySample
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_SampleReference_Result') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE SampleReference DROP CONSTRAINT FK_SampleReference_Result
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_SamplesOwnerDetails_Header') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE SamplesOwnerDetails DROP CONSTRAINT FK_SamplesOwnerDetails_Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Screen_decimal') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Screen DROP CONSTRAINT FK_Screen_decimal
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Screen_ParticleSizeReductionProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Screen DROP CONSTRAINT FK_Screen_ParticleSizeReductionProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_Screen_ScreeningProcedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE Screen DROP CONSTRAINT FK_Screen_ScreeningProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_ScreeningProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE ScreeningProcedure DROP CONSTRAINT FK_ScreeningProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_SendProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE SendProcedure DROP CONSTRAINT FK_SendProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_SplittingProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE SplittingProcedure DROP CONSTRAINT FK_SplittingProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_StorageProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE StorageProcedure DROP CONSTRAINT FK_StorageProcedure_Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('FK_WeighingProcedure_Procedure') AND OBJECTPROPERTY(id, 'IsForeignKey') = 1)
ALTER TABLE WeighingProcedure DROP CONSTRAINT FK_WeighingProcedure_Procedure
;



IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AddressBook') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AddressBook
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ADX') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ADX
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ADXDictionaryStrict') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ADXDictionaryStrict
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalysisMethod') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalysisMethod
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalysisProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalysisProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalysisRecord') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalysisRecord
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalysisReference') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalysisReference
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalyticalReportDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalyticalReportDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('BillingDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE BillingDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ChainOfCustody') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ChainOfCustody
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ChainOfCustodyBatch') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ChainOfCustodyBatch
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ChainOfCustodySample') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ChainOfCustodySample
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('CompositingProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE CompositingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ContactDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ContactDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('decimal') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE decimal
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DecompositionProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DecompositionProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DespatchDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DespatchDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DisposeProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DisposeProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DocumentAuthorDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DocumentAuthorDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DryingProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DryingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('GenericProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE GenericProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Header') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Header
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('LaboratoryDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE LaboratoryDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('LocationDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE LocationDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('MaterialDestination') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE MaterialDestination
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('MaterialSource') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE MaterialSource
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('MaterialTarget') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE MaterialTarget
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Measurement') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Measurement
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ParticleSizeReductionProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ParticleSizeReductionProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Procedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Procedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Process') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Process
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ProcessingGroup') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ProcessingGroup
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ProcessingPath') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ProcessingPath
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ProcessingTag') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ProcessingTag
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ProjectDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ProjectDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Protocol') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Protocol
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Protocols') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Protocols
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Quantity') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Quantity
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('RackPlacementProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE RackPlacementProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ReceiveProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ReceiveProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ReferenceMaterial') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ReferenceMaterial
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ReferenceMaterials') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ReferenceMaterials
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Result') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Result
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Results') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Results
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Sample') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Sample
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('SampleReference') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE SampleReference
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Samples') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Samples
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('SamplesOwnerDetails') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE SamplesOwnerDetails
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('Screen') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE Screen
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ScreeningProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ScreeningProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('SendProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE SendProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('SplittingProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE SplittingProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('StorageProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE StorageProcedure
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('string') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE string
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('WeighingProcedure') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE WeighingProcedure
;


CREATE TABLE AddressBook ( 
	Comments varchar(50) NULL,
	addressBookID int NOT NULL
)
;

CREATE TABLE ADX ( 
	aDXID int NOT NULL
)
;

CREATE TABLE ADXDictionaryStrict ( 
	aDXDictionaryStrictID int NOT NULL
)
;

CREATE TABLE AnalysisMethod ( 
	Comments varchar(50) NULL,
	Equipment AnalysisMethodEquipment NULL,
	Technique AnalysisMethodTechnique NULL,
	analysisMethodID int NOT NULL,
	analysisProcedureID int NULL
)
;

CREATE TABLE AnalysisProcedure ( 
	Property PropertyType NULL,
	UOM UOM NULL,
	analysisProcedureID int NOT NULL
)
;

CREATE TABLE AnalysisRecord ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	analysisRecordID int NOT NULL,
	resultID int NULL
)
;

CREATE TABLE AnalysisReference ( 
	IdRef varchar(50) NULL,
	analysisReferenceID int NOT NULL,
	referenceMaterialID int NULL
)
;

CREATE TABLE AnalyticalReportDetails ( 
	Comments varchar(50) NULL,
	LabInvoiceNo varchar(50) NULL,
	LabJob varchar(50) NULL,
	ReferenceLabJob varchar(50) NULL,
	Status AnalyticalReportStatus NULL,
	StatusDate datetime2(7) NULL,
	analyticalReportDetailsID int NOT NULL,
	headerID int NULL
)
;

CREATE TABLE BillingDetails ( 
	Code varchar(50) NULL,
	Comments varchar(50) NULL,
	billingDetailsID int NOT NULL,
	headerID int NULL
)
;

CREATE TABLE ChainOfCustody ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	chainOfCustodyID int NOT NULL
)
;

CREATE TABLE ChainOfCustodyBatch ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	chainOfCustodyBatchID int NOT NULL,
	chainOfCustodyID int NULL
)
;

CREATE TABLE ChainOfCustodySample ( 
	Comments varchar(50) NULL,
	chainOfCustodySampleID int NOT NULL,
	chainOfCustodyBatchID int NULL
)
;

CREATE TABLE CompositingProcedure ( 
	Equipment varchar(50) NULL,
	Method CompositingMethodType NULL,
	compositingProcedureID int NOT NULL
)
;

CREATE TABLE ContactDetails ( 
	Comments varchar(50) NULL,
	ContactName varchar(50) NULL,
	Email varchar(50) NULL,
	Id varchar(50) NULL,
	LocationRef varchar(50) NULL,
	Role varchar(50) NULL,
	Telephone varchar(50) NULL,
	contactDetailsID int NOT NULL,
	addressBookID int NULL
)
;

CREATE TABLE decimal ( 
	decimalID int NOT NULL
)
;

CREATE TABLE DecompositionProcedure ( 
	Agitation varchar(50) NULL,
	Chemicals varchar(50) NULL,
	Duration varchar(50) NULL,
	Method DecompositionMethodType NULL,
	decompositionProcedureID int NOT NULL
)
;

CREATE TABLE DespatchDetails ( 
	Comments varchar(50) NULL,
	DespatchNo varchar(50) NULL,
	despatchDetailsID int NOT NULL,
	headerID int NULL
)
;

CREATE TABLE DisposeProcedure ( 
	disposeProcedureID int NOT NULL
)
;

CREATE TABLE DocumentAuthorDetails ( 
	AuthorRef varchar(50) NULL,
	Comments varchar(50) NULL,
	CreatedOn datetime2(7) NULL,
	Id varchar(50) NULL,
	documentAuthorDetailsID int NOT NULL,
	headerID int NULL
)
;

CREATE TABLE DryingProcedure ( 
	Duration varchar(50) NULL,
	Equipment varchar(50) NULL,
	Method DryingMethodType NULL,
	dryingProcedureID int NOT NULL
)
;

CREATE TABLE GenericProcedure ( 
	genericProcedureID int NOT NULL
)
;

CREATE TABLE Header ( 
	Comments varchar(50) NULL,
	headerID int NOT NULL
)
;

CREATE TABLE LaboratoryDetails ( 
	Comments varchar(50) NULL,
	ContactRef varchar(50) NULL,
	LocationRef varchar(50) NULL,
	laboratoryDetailsID int NOT NULL,
	headerID int NULL
)
;

CREATE TABLE LocationDetails ( 
	Address varchar(50) NULL,
	City varchar(50) NULL,
	Comments varchar(50) NULL,
	Company varchar(50) NULL,
	ContactRef varchar(50) NULL,
	Country varchar(50) NULL,
	Description varchar(50) NULL,
	Email varchar(50) NULL,
	Id varchar(50) NULL,
	Office varchar(50) NULL,
	PostalCode varchar(50) NULL,
	State varchar(50) NULL,
	Telephone varchar(50) NULL,
	locationDetailsID int NOT NULL,
	addressBookID int NULL
)
;

CREATE TABLE MaterialDestination ( 
	Comments varchar(50) NULL,
	Description varchar(50) NULL,
	Name varchar(50) NULL,
	Type varchar(50) NULL,
	materialDestinationID int NOT NULL,
	analysisProcedureID int NULL,
	compositingProcedureID int NULL,
	decompositionProcedureID int NULL,
	dryingProcedureID int NULL,
	genericProcedureID int NULL,
	screeningProcedureID int NULL,
	particleSizeReductionProcedureID int NULL,
	storageProcedureID int NULL,
	splittingProcedureID int NULL,
	processID int NULL,
	weighingProcedureID int NULL,
	sendProcedureID int NULL,
	receiveProcedureID int NULL,
	rackPlacementProcedureID int NULL
)
;

CREATE TABLE MaterialSource ( 
	Id varchar(50) NULL,
	Type MaterialSourceType NULL,
	materialSourceID int NOT NULL,
	compositingProcedureID int NULL
)
;

CREATE TABLE MaterialTarget ( 
	MaterialSourceRef varchar(50) NULL,
	ProcedureRef varchar(50) NULL,
	ProcessRef varchar(50) NULL,
	ProtocolRef varchar(50) NULL,
	materialTargetID int NOT NULL,
	materialDestinationID int NULL
)
;

CREATE TABLE Measurement ( 
	Comments varchar(50) NULL,
	LocationRef varchar(50) NULL,
	ProcedureRef varchar(50) NULL,
	Property PropertyType NULL,
	Status MeasurementStatus NULL,
	measurementID int NOT NULL,
	analysisRecordID int NULL
)
;

CREATE TABLE ParticleSizeReductionProcedure ( 
	Equipment varchar(50) NULL,
	Method ParticleSizeReductionMethodType NULL,
	Percent decimal(10,2) NULL,
	particleSizeReductionProcedureID int NOT NULL
)
;

CREATE TABLE Procedure ( 
	Code varchar(50) NULL,
	Comments varchar(50) NULL,
	ContactRef varchar(50) NULL,
	Description varchar(50) NULL,
	Id varchar(50) NULL,
	LocationRef varchar(50) NULL,
	TimeStamp datetime2(7) NULL,
	procedureID int NOT NULL,
	materialTargetID int NULL
)
;

CREATE TABLE Process ( 
	Code varchar(50) NULL,
	Comments varchar(50) NULL,
	Description varchar(50) NULL,
	Id varchar(50) NULL,
	processID int NOT NULL,
	chainOfCustodyBatchID int NULL,
	despatchDetailsID int NULL,
	protocolID int NULL,
	resultID int NULL
)
;

CREATE TABLE ProcessingGroup ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	Type ProcessingGroupType NULL,
	processingGroupID int NOT NULL,
	referenceMaterialsID int NULL,
	resultsID int NULL
)
;

CREATE TABLE ProcessingPath ( 
	Id varchar(50) NULL,
	processingPathID int NOT NULL,
	analysisRecordID int NULL,
	chainOfCustodySampleID int NULL,
	resultID int NULL
)
;

CREATE TABLE ProcessingTag ( 
	Name varchar(50) NULL,
	Type ProcessingTagType NULL,
	processingTagID int NOT NULL,
	processingPathID int NULL
)
;

CREATE TABLE ProjectDetails ( 
	Comments varchar(50) NULL,
	Description varchar(50) NULL,
	Id varchar(50) NULL,
	projectDetailsID int NOT NULL,
	samplesOwnerDetailsID int NULL
)
;

CREATE TABLE Protocol ( 
	Comments varchar(50) NULL,
	Description varchar(50) NULL,
	Id varchar(50) NULL,
	protocolID int NOT NULL,
	protocolsID int NULL
)
;

CREATE TABLE Protocols ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	protocolsID int NOT NULL
)
;

CREATE TABLE Quantity ( 
	UOM UOM NULL,
	quantityID int NOT NULL,
	analysisProcedureID int NULL,
	decompositionProcedureID int NULL,
	dryingProcedureID int NULL,
	materialDestinationID int NULL,
	measurementID int NULL,
	weighingProcedureID int NULL
)
;

CREATE TABLE RackPlacementProcedure ( 
	Block varchar(50) NULL,
	Position varchar(50) NULL,
	rackPlacementProcedureID int NOT NULL
)
;

CREATE TABLE ReceiveProcedure ( 
	receiveProcedureID int NOT NULL
)
;

CREATE TABLE ReferenceMaterial ( 
	Category ReferenceMaterialCategory NULL,
	referenceMaterialID int NOT NULL
)
;

CREATE TABLE ReferenceMaterials ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	referenceMaterialsID int NOT NULL
)
;

CREATE TABLE Result ( 
	Comments varchar(50) NULL,
	resultID int NOT NULL,
	processingGroupID int NULL
)
;

CREATE TABLE Results ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	resultsID int NOT NULL
)
;

CREATE TABLE Sample ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	Status SampleStatus NULL,
	Type SampleType NULL,
	sampleID int NOT NULL,
	samplesID int NULL
)
;

CREATE TABLE SampleReference ( 
	IdRef varchar(50) NULL,
	sampleReferenceID int NOT NULL,
	chainOfCustodySampleID int NULL,
	resultID int NULL
)
;

CREATE TABLE Samples ( 
	Comments varchar(50) NULL,
	Id varchar(50) NULL,
	samplesID int NOT NULL
)
;

CREATE TABLE SamplesOwnerDetails ( 
	Comments varchar(50) NULL,
	ContactRef varchar(50) NULL,
	LocationRef varchar(50) NULL,
	samplesOwnerDetailsID int NOT NULL,
	headerID int NULL
)
;

CREATE TABLE Screen ( 
	Series ScreenSeriesType NULL,
	UOM UOM NULL,
	screenID int NOT NULL,
	particleSizeReductionProcedureID int NULL,
	screeningProcedureID int NULL
)
;

CREATE TABLE ScreeningProcedure ( 
	Condition ScreenCondition NULL,
	screeningProcedureID int NOT NULL
)
;

CREATE TABLE SendProcedure ( 
	sendProcedureID int NOT NULL
)
;

CREATE TABLE SplittingProcedure ( 
	Equipment varchar(50) NULL,
	Method SplittingMethodType NULL,
	splittingProcedureID int NOT NULL
)
;

CREATE TABLE StorageProcedure ( 
	storageProcedureID int NOT NULL
)
;

CREATE TABLE string ( 
	stringID int NOT NULL
)
;

CREATE TABLE WeighingProcedure ( 
	Equipment varchar(50) NULL,
	Method WeighingMethodType NULL,
	weighingProcedureID int NOT NULL
)
;


ALTER TABLE AddressBook ADD CONSTRAINT PK_AddressBook 
	PRIMARY KEY CLUSTERED (addressBookID)
;

ALTER TABLE ADX ADD CONSTRAINT PK_ADX 
	PRIMARY KEY CLUSTERED (aDXID)
;

ALTER TABLE ADXDictionaryStrict ADD CONSTRAINT PK_ADXDictionaryStrict 
	PRIMARY KEY CLUSTERED (aDXDictionaryStrictID)
;

ALTER TABLE AnalysisMethod ADD CONSTRAINT PK_AnalysisMethod 
	PRIMARY KEY CLUSTERED (analysisMethodID)
;

ALTER TABLE AnalysisProcedure ADD CONSTRAINT PK_AnalysisProcedure 
	PRIMARY KEY CLUSTERED (analysisProcedureID)
;

ALTER TABLE AnalysisRecord ADD CONSTRAINT PK_AnalysisRecord 
	PRIMARY KEY CLUSTERED (analysisRecordID)
;

ALTER TABLE AnalysisReference ADD CONSTRAINT PK_AnalysisReference 
	PRIMARY KEY CLUSTERED (analysisReferenceID)
;

ALTER TABLE AnalyticalReportDetails ADD CONSTRAINT PK_AnalyticalReportDetails 
	PRIMARY KEY CLUSTERED (analyticalReportDetailsID)
;

ALTER TABLE BillingDetails ADD CONSTRAINT PK_BillingDetails 
	PRIMARY KEY CLUSTERED (billingDetailsID)
;

ALTER TABLE ChainOfCustody ADD CONSTRAINT PK_ChainOfCustody 
	PRIMARY KEY CLUSTERED (chainOfCustodyID)
;

ALTER TABLE ChainOfCustodyBatch ADD CONSTRAINT PK_ChainOfCustodyBatch 
	PRIMARY KEY CLUSTERED (chainOfCustodyBatchID)
;

ALTER TABLE ChainOfCustodySample ADD CONSTRAINT PK_ChainOfCustodySample 
	PRIMARY KEY CLUSTERED (chainOfCustodySampleID)
;

ALTER TABLE CompositingProcedure ADD CONSTRAINT PK_CompositingProcedure 
	PRIMARY KEY CLUSTERED (compositingProcedureID)
;

ALTER TABLE ContactDetails ADD CONSTRAINT PK_ContactDetails 
	PRIMARY KEY CLUSTERED (contactDetailsID)
;

ALTER TABLE decimal ADD CONSTRAINT PK_decimal 
	PRIMARY KEY CLUSTERED (decimalID)
;

ALTER TABLE DecompositionProcedure ADD CONSTRAINT PK_DecompositionProcedure 
	PRIMARY KEY CLUSTERED (decompositionProcedureID)
;

ALTER TABLE DespatchDetails ADD CONSTRAINT PK_DespatchDetails 
	PRIMARY KEY CLUSTERED (despatchDetailsID)
;

ALTER TABLE DisposeProcedure ADD CONSTRAINT PK_DisposeProcedure 
	PRIMARY KEY CLUSTERED (disposeProcedureID)
;

ALTER TABLE DocumentAuthorDetails ADD CONSTRAINT PK_DocumentAuthorDetails 
	PRIMARY KEY CLUSTERED (documentAuthorDetailsID)
;

ALTER TABLE DryingProcedure ADD CONSTRAINT PK_DryingProcedure 
	PRIMARY KEY CLUSTERED (dryingProcedureID)
;

ALTER TABLE GenericProcedure ADD CONSTRAINT PK_GenericProcedure 
	PRIMARY KEY CLUSTERED (genericProcedureID)
;

ALTER TABLE Header ADD CONSTRAINT PK_Header 
	PRIMARY KEY CLUSTERED (headerID)
;

ALTER TABLE LaboratoryDetails ADD CONSTRAINT PK_LaboratoryDetails 
	PRIMARY KEY CLUSTERED (laboratoryDetailsID)
;

ALTER TABLE LocationDetails ADD CONSTRAINT PK_LocationDetails 
	PRIMARY KEY CLUSTERED (locationDetailsID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT PK_MaterialDestination 
	PRIMARY KEY CLUSTERED (materialDestinationID)
;

ALTER TABLE MaterialSource ADD CONSTRAINT PK_MaterialSource 
	PRIMARY KEY CLUSTERED (materialSourceID)
;

ALTER TABLE MaterialTarget ADD CONSTRAINT PK_MaterialTarget 
	PRIMARY KEY CLUSTERED (materialTargetID)
;

ALTER TABLE Measurement ADD CONSTRAINT PK_Measurement 
	PRIMARY KEY CLUSTERED (measurementID)
;

ALTER TABLE ParticleSizeReductionProcedure ADD CONSTRAINT PK_ParticleSizeReductionProcedure 
	PRIMARY KEY CLUSTERED (particleSizeReductionProcedureID)
;

ALTER TABLE Procedure ADD CONSTRAINT PK_Procedure 
	PRIMARY KEY CLUSTERED (procedureID)
;

ALTER TABLE Process ADD CONSTRAINT PK_Process 
	PRIMARY KEY CLUSTERED (processID)
;

ALTER TABLE ProcessingGroup ADD CONSTRAINT PK_ProcessingGroup 
	PRIMARY KEY CLUSTERED (processingGroupID)
;

ALTER TABLE ProcessingPath ADD CONSTRAINT PK_ProcessingPath 
	PRIMARY KEY CLUSTERED (processingPathID)
;

ALTER TABLE ProcessingTag ADD CONSTRAINT PK_ProcessingTag 
	PRIMARY KEY CLUSTERED (processingTagID)
;

ALTER TABLE ProjectDetails ADD CONSTRAINT PK_ProjectDetails 
	PRIMARY KEY CLUSTERED (projectDetailsID)
;

ALTER TABLE Protocol ADD CONSTRAINT PK_Protocol 
	PRIMARY KEY CLUSTERED (protocolID)
;

ALTER TABLE Protocols ADD CONSTRAINT PK_Protocols 
	PRIMARY KEY CLUSTERED (protocolsID)
;

ALTER TABLE Quantity ADD CONSTRAINT PK_Quantity 
	PRIMARY KEY CLUSTERED (quantityID)
;

ALTER TABLE RackPlacementProcedure ADD CONSTRAINT PK_RackPlacementProcedure 
	PRIMARY KEY CLUSTERED (rackPlacementProcedureID)
;

ALTER TABLE ReceiveProcedure ADD CONSTRAINT PK_ReceiveProcedure 
	PRIMARY KEY CLUSTERED (receiveProcedureID)
;

ALTER TABLE ReferenceMaterial ADD CONSTRAINT PK_ReferenceMaterial 
	PRIMARY KEY CLUSTERED (referenceMaterialID)
;

ALTER TABLE ReferenceMaterials ADD CONSTRAINT PK_ReferenceMaterials 
	PRIMARY KEY CLUSTERED (referenceMaterialsID)
;

ALTER TABLE Result ADD CONSTRAINT PK_Result 
	PRIMARY KEY CLUSTERED (resultID)
;

ALTER TABLE Results ADD CONSTRAINT PK_Results 
	PRIMARY KEY CLUSTERED (resultsID)
;

ALTER TABLE Sample ADD CONSTRAINT PK_Sample 
	PRIMARY KEY CLUSTERED (sampleID)
;

ALTER TABLE SampleReference ADD CONSTRAINT PK_SampleReference 
	PRIMARY KEY CLUSTERED (sampleReferenceID)
;

ALTER TABLE Samples ADD CONSTRAINT PK_Samples 
	PRIMARY KEY CLUSTERED (samplesID)
;

ALTER TABLE SamplesOwnerDetails ADD CONSTRAINT PK_SamplesOwnerDetails 
	PRIMARY KEY CLUSTERED (samplesOwnerDetailsID)
;

ALTER TABLE Screen ADD CONSTRAINT PK_Screen 
	PRIMARY KEY CLUSTERED (screenID)
;

ALTER TABLE ScreeningProcedure ADD CONSTRAINT PK_ScreeningProcedure 
	PRIMARY KEY CLUSTERED (screeningProcedureID)
;

ALTER TABLE SendProcedure ADD CONSTRAINT PK_SendProcedure 
	PRIMARY KEY CLUSTERED (sendProcedureID)
;

ALTER TABLE SplittingProcedure ADD CONSTRAINT PK_SplittingProcedure 
	PRIMARY KEY CLUSTERED (splittingProcedureID)
;

ALTER TABLE StorageProcedure ADD CONSTRAINT PK_StorageProcedure 
	PRIMARY KEY CLUSTERED (storageProcedureID)
;

ALTER TABLE string ADD CONSTRAINT PK_string 
	PRIMARY KEY CLUSTERED (stringID)
;

ALTER TABLE WeighingProcedure ADD CONSTRAINT PK_WeighingProcedure 
	PRIMARY KEY CLUSTERED (weighingProcedureID)
;



ALTER TABLE AnalysisMethod ADD CONSTRAINT FK_AnalysisMethod_AnalysisProcedure 
	FOREIGN KEY (analysisProcedureID) REFERENCES AnalysisProcedure (analysisProcedureID)
;

ALTER TABLE AnalysisProcedure ADD CONSTRAINT FK_AnalysisProcedure_Procedure 
	FOREIGN KEY (analysisProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE AnalysisRecord ADD CONSTRAINT FK_AnalysisRecord_Result 
	FOREIGN KEY (resultID) REFERENCES Result (resultID)
;

ALTER TABLE AnalysisReference ADD CONSTRAINT FK_AnalysisReference_ReferenceMaterial 
	FOREIGN KEY (referenceMaterialID) REFERENCES ReferenceMaterial (referenceMaterialID)
;

ALTER TABLE AnalyticalReportDetails ADD CONSTRAINT FK_AnalyticalReportDetails_Header 
	FOREIGN KEY (headerID) REFERENCES Header (headerID)
;

ALTER TABLE BillingDetails ADD CONSTRAINT FK_BillingDetails_Header 
	FOREIGN KEY (headerID) REFERENCES Header (headerID)
;

ALTER TABLE ChainOfCustodyBatch ADD CONSTRAINT FK_ChainOfCustodyBatch_ChainOfCustody 
	FOREIGN KEY (chainOfCustodyID) REFERENCES ChainOfCustody (chainOfCustodyID)
;

ALTER TABLE ChainOfCustodySample ADD CONSTRAINT FK_ChainOfCustodySample_ChainOfCustodyBatch 
	FOREIGN KEY (chainOfCustodyBatchID) REFERENCES ChainOfCustodyBatch (chainOfCustodyBatchID)
;

ALTER TABLE CompositingProcedure ADD CONSTRAINT FK_CompositingProcedure_Procedure 
	FOREIGN KEY (compositingProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE ContactDetails ADD CONSTRAINT FK_ContactDetails_AddressBook 
	FOREIGN KEY (addressBookID) REFERENCES AddressBook (addressBookID)
;

ALTER TABLE DecompositionProcedure ADD CONSTRAINT FK_DecompositionProcedure_Procedure 
	FOREIGN KEY (decompositionProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE DespatchDetails ADD CONSTRAINT FK_DespatchDetails_Header 
	FOREIGN KEY (headerID) REFERENCES Header (headerID)
;

ALTER TABLE DisposeProcedure ADD CONSTRAINT FK_DisposeProcedure_Procedure 
	FOREIGN KEY (disposeProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE DocumentAuthorDetails ADD CONSTRAINT FK_DocumentAuthorDetails_Header 
	FOREIGN KEY (headerID) REFERENCES Header (headerID)
;

ALTER TABLE DryingProcedure ADD CONSTRAINT FK_DryingProcedure_Procedure 
	FOREIGN KEY (dryingProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE GenericProcedure ADD CONSTRAINT FK_GenericProcedure_Procedure 
	FOREIGN KEY (genericProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE LaboratoryDetails ADD CONSTRAINT FK_LaboratoryDetails_Header 
	FOREIGN KEY (headerID) REFERENCES Header (headerID)
;

ALTER TABLE LocationDetails ADD CONSTRAINT FK_LocationDetails_AddressBook 
	FOREIGN KEY (addressBookID) REFERENCES AddressBook (addressBookID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_AnalysisProcedure 
	FOREIGN KEY (analysisProcedureID) REFERENCES AnalysisProcedure (analysisProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_CompositingProcedure 
	FOREIGN KEY (compositingProcedureID) REFERENCES CompositingProcedure (compositingProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_DecompositionProcedure 
	FOREIGN KEY (decompositionProcedureID) REFERENCES DecompositionProcedure (decompositionProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_DryingProcedure 
	FOREIGN KEY (dryingProcedureID) REFERENCES DryingProcedure (dryingProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_GenericProcedure 
	FOREIGN KEY (genericProcedureID) REFERENCES GenericProcedure (genericProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_ScreeningProcedure 
	FOREIGN KEY (screeningProcedureID) REFERENCES ScreeningProcedure (screeningProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_ParticleSizeReductionProcedure 
	FOREIGN KEY (particleSizeReductionProcedureID) REFERENCES ParticleSizeReductionProcedure (particleSizeReductionProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_StorageProcedure 
	FOREIGN KEY (storageProcedureID) REFERENCES StorageProcedure (storageProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_SplittingProcedure 
	FOREIGN KEY (splittingProcedureID) REFERENCES SplittingProcedure (splittingProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_Process 
	FOREIGN KEY (processID) REFERENCES Process (processID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_WeighingProcedure 
	FOREIGN KEY (weighingProcedureID) REFERENCES WeighingProcedure (weighingProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_SendProcedure 
	FOREIGN KEY (sendProcedureID) REFERENCES SendProcedure (sendProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_ReceiveProcedure 
	FOREIGN KEY (receiveProcedureID) REFERENCES ReceiveProcedure (receiveProcedureID)
;

ALTER TABLE MaterialDestination ADD CONSTRAINT FK_MaterialDestination_RackPlacementProcedure 
	FOREIGN KEY (rackPlacementProcedureID) REFERENCES RackPlacementProcedure (rackPlacementProcedureID)
;

ALTER TABLE MaterialSource ADD CONSTRAINT FK_MaterialSource_CompositingProcedure 
	FOREIGN KEY (compositingProcedureID) REFERENCES CompositingProcedure (compositingProcedureID)
;

ALTER TABLE MaterialTarget ADD CONSTRAINT FK_MaterialTarget_MaterialDestination 
	FOREIGN KEY (materialDestinationID) REFERENCES MaterialDestination (materialDestinationID)
;

ALTER TABLE Measurement ADD CONSTRAINT FK_Measurement_AnalysisRecord 
	FOREIGN KEY (analysisRecordID) REFERENCES AnalysisRecord (analysisRecordID)
;

ALTER TABLE ParticleSizeReductionProcedure ADD CONSTRAINT FK_ParticleSizeReductionProcedure_Procedure 
	FOREIGN KEY (particleSizeReductionProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE Procedure ADD CONSTRAINT FK_Procedure_MaterialTarget 
	FOREIGN KEY (materialTargetID) REFERENCES MaterialTarget (materialTargetID)
;

ALTER TABLE Process ADD CONSTRAINT FK_Process_ChainOfCustodyBatch 
	FOREIGN KEY (chainOfCustodyBatchID) REFERENCES ChainOfCustodyBatch (chainOfCustodyBatchID)
;

ALTER TABLE Process ADD CONSTRAINT FK_Process_DespatchDetails 
	FOREIGN KEY (despatchDetailsID) REFERENCES DespatchDetails (despatchDetailsID)
;

ALTER TABLE Process ADD CONSTRAINT FK_Process_Protocol 
	FOREIGN KEY (protocolID) REFERENCES Protocol (protocolID)
;

ALTER TABLE Process ADD CONSTRAINT FK_Process_Result 
	FOREIGN KEY (resultID) REFERENCES Result (resultID)
;

ALTER TABLE ProcessingGroup ADD CONSTRAINT FK_ProcessingGroup_ReferenceMaterials 
	FOREIGN KEY (referenceMaterialsID) REFERENCES ReferenceMaterials (referenceMaterialsID)
;

ALTER TABLE ProcessingGroup ADD CONSTRAINT FK_ProcessingGroup_Results 
	FOREIGN KEY (resultsID) REFERENCES Results (resultsID)
;

ALTER TABLE ProcessingPath ADD CONSTRAINT FK_ProcessingPath_AnalysisRecord 
	FOREIGN KEY (analysisRecordID) REFERENCES AnalysisRecord (analysisRecordID)
;

ALTER TABLE ProcessingPath ADD CONSTRAINT FK_ProcessingPath_ChainOfCustodySample 
	FOREIGN KEY (chainOfCustodySampleID) REFERENCES ChainOfCustodySample (chainOfCustodySampleID)
;

ALTER TABLE ProcessingPath ADD CONSTRAINT FK_ProcessingPath_Result 
	FOREIGN KEY (resultID) REFERENCES Result (resultID)
;

ALTER TABLE ProcessingTag ADD CONSTRAINT FK_ProcessingTag_ProcessingPath 
	FOREIGN KEY (processingPathID) REFERENCES ProcessingPath (processingPathID)
;

ALTER TABLE ProjectDetails ADD CONSTRAINT FK_ProjectDetails_SamplesOwnerDetails 
	FOREIGN KEY (samplesOwnerDetailsID) REFERENCES SamplesOwnerDetails (samplesOwnerDetailsID)
;

ALTER TABLE Protocol ADD CONSTRAINT FK_Protocol_Protocols 
	FOREIGN KEY (protocolsID) REFERENCES Protocols (protocolsID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_AnalysisProcedure 
	FOREIGN KEY (analysisProcedureID) REFERENCES AnalysisProcedure (analysisProcedureID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_DecompositionProcedure 
	FOREIGN KEY (decompositionProcedureID) REFERENCES DecompositionProcedure (decompositionProcedureID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_DryingProcedure 
	FOREIGN KEY (dryingProcedureID) REFERENCES DryingProcedure (dryingProcedureID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_MaterialDestination 
	FOREIGN KEY (materialDestinationID) REFERENCES MaterialDestination (materialDestinationID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_Measurement 
	FOREIGN KEY (measurementID) REFERENCES Measurement (measurementID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_string 
	FOREIGN KEY (quantityID) REFERENCES string (stringID)
;

ALTER TABLE Quantity ADD CONSTRAINT FK_Quantity_WeighingProcedure 
	FOREIGN KEY (weighingProcedureID) REFERENCES WeighingProcedure (weighingProcedureID)
;

ALTER TABLE RackPlacementProcedure ADD CONSTRAINT FK_RackPlacementProcedure_Procedure 
	FOREIGN KEY (rackPlacementProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE ReceiveProcedure ADD CONSTRAINT FK_ReceiveProcedure_Procedure 
	FOREIGN KEY (receiveProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE ReferenceMaterial ADD CONSTRAINT FK_ReferenceMaterial_Sample 
	FOREIGN KEY (referenceMaterialID) REFERENCES Sample (sampleID)
;

ALTER TABLE Result ADD CONSTRAINT FK_Result_ProcessingGroup 
	FOREIGN KEY (processingGroupID) REFERENCES ProcessingGroup (processingGroupID)
;

ALTER TABLE Sample ADD CONSTRAINT FK_Sample_Samples 
	FOREIGN KEY (samplesID) REFERENCES Samples (samplesID)
;

ALTER TABLE SampleReference ADD CONSTRAINT FK_SampleReference_ChainOfCustodySample 
	FOREIGN KEY (chainOfCustodySampleID) REFERENCES ChainOfCustodySample (chainOfCustodySampleID)
;

ALTER TABLE SampleReference ADD CONSTRAINT FK_SampleReference_Result 
	FOREIGN KEY (resultID) REFERENCES Result (resultID)
;

ALTER TABLE SamplesOwnerDetails ADD CONSTRAINT FK_SamplesOwnerDetails_Header 
	FOREIGN KEY (headerID) REFERENCES Header (headerID)
;

ALTER TABLE Screen ADD CONSTRAINT FK_Screen_decimal 
	FOREIGN KEY (screenID) REFERENCES decimal (decimalID)
;

ALTER TABLE Screen ADD CONSTRAINT FK_Screen_ParticleSizeReductionProcedure 
	FOREIGN KEY (particleSizeReductionProcedureID) REFERENCES ParticleSizeReductionProcedure (particleSizeReductionProcedureID)
;

ALTER TABLE Screen ADD CONSTRAINT FK_Screen_ScreeningProcedure 
	FOREIGN KEY (screeningProcedureID) REFERENCES ScreeningProcedure (screeningProcedureID)
;

ALTER TABLE ScreeningProcedure ADD CONSTRAINT FK_ScreeningProcedure_Procedure 
	FOREIGN KEY (screeningProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE SendProcedure ADD CONSTRAINT FK_SendProcedure_Procedure 
	FOREIGN KEY (sendProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE SplittingProcedure ADD CONSTRAINT FK_SplittingProcedure_Procedure 
	FOREIGN KEY (splittingProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE StorageProcedure ADD CONSTRAINT FK_StorageProcedure_Procedure 
	FOREIGN KEY (storageProcedureID) REFERENCES Procedure (procedureID)
;

ALTER TABLE WeighingProcedure ADD CONSTRAINT FK_WeighingProcedure_Procedure 
	FOREIGN KEY (weighingProcedureID) REFERENCES Procedure (procedureID)
;
