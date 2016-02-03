USE NKDADX
;



IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalysisMethodEquipment') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalysisMethodEquipment
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('AnalysisMethodTechnique') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE AnalysisMethodTechnique
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('CompositingMethodType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE CompositingMethodType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DecompositionMethodType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DecompositionMethodType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('DryingMethodType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE DryingMethodType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('MaterialSourceType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE MaterialSourceType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ParticleSizeReductionMethodType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ParticleSizeReductionMethodType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ProcessingGroupType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ProcessingGroupType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ProcessingTagType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ProcessingTagType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('PropertyType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE PropertyType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('SampleType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE SampleType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('ScreenSeriesType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE ScreenSeriesType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('SplittingMethodType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE SplittingMethodType
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('UOM') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE UOM
;

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id('WeighingMethodType') AND  OBJECTPROPERTY(id, 'IsUserTable') = 1)
DROP TABLE WeighingMethodType
;


CREATE TABLE AnalysisMethodEquipment ( 
	analysisMethodEquipmentID int NOT NULL
)
;

CREATE TABLE AnalysisMethodTechnique ( 
	analysisMethodTechniqueID int NOT NULL
)
;

CREATE TABLE CompositingMethodType ( 
	compositingMethodTypeID int NOT NULL
)
;

CREATE TABLE DecompositionMethodType ( 
	decompositionMethodTypeID int NOT NULL
)
;

CREATE TABLE DryingMethodType ( 
	dryingMethodTypeID int NOT NULL
)
;

CREATE TABLE MaterialSourceType ( 
	materialSourceTypeID int NOT NULL
)
;

CREATE TABLE ParticleSizeReductionMethodType ( 
	particleSizeReductionMethodTypeID int NOT NULL
)
;

CREATE TABLE ProcessingGroupType ( 
	processingGroupTypeID int NOT NULL
)
;

CREATE TABLE ProcessingTagType ( 
	processingTagTypeID int NOT NULL
)
;

CREATE TABLE PropertyType ( 
	propertyTypeID int NOT NULL
)
;

CREATE TABLE SampleType ( 
	sampleTypeID int NOT NULL
)
;

CREATE TABLE ScreenSeriesType ( 
	screenSeriesTypeID int NOT NULL
)
;

CREATE TABLE SplittingMethodType ( 
	splittingMethodTypeID int NOT NULL
)
;

CREATE TABLE UOM ( 
	uOMID int NOT NULL
)
;

CREATE TABLE WeighingMethodType ( 
	weighingMethodTypeID int NOT NULL
)
;


ALTER TABLE AnalysisMethodEquipment ADD CONSTRAINT PK_AnalysisMethodEquipment 
	PRIMARY KEY CLUSTERED (analysisMethodEquipmentID)
;

ALTER TABLE AnalysisMethodTechnique ADD CONSTRAINT PK_AnalysisMethodTechnique 
	PRIMARY KEY CLUSTERED (analysisMethodTechniqueID)
;

ALTER TABLE CompositingMethodType ADD CONSTRAINT PK_CompositingMethodType 
	PRIMARY KEY CLUSTERED (compositingMethodTypeID)
;

ALTER TABLE DecompositionMethodType ADD CONSTRAINT PK_DecompositionMethodType 
	PRIMARY KEY CLUSTERED (decompositionMethodTypeID)
;

ALTER TABLE DryingMethodType ADD CONSTRAINT PK_DryingMethodType 
	PRIMARY KEY CLUSTERED (dryingMethodTypeID)
;

ALTER TABLE MaterialSourceType ADD CONSTRAINT PK_MaterialSourceType 
	PRIMARY KEY CLUSTERED (materialSourceTypeID)
;

ALTER TABLE ParticleSizeReductionMethodType ADD CONSTRAINT PK_ParticleSizeReductionMethodType 
	PRIMARY KEY CLUSTERED (particleSizeReductionMethodTypeID)
;

ALTER TABLE ProcessingGroupType ADD CONSTRAINT PK_ProcessingGroupType 
	PRIMARY KEY CLUSTERED (processingGroupTypeID)
;

ALTER TABLE ProcessingTagType ADD CONSTRAINT PK_ProcessingTagType 
	PRIMARY KEY CLUSTERED (processingTagTypeID)
;

ALTER TABLE PropertyType ADD CONSTRAINT PK_PropertyType 
	PRIMARY KEY CLUSTERED (propertyTypeID)
;

ALTER TABLE SampleType ADD CONSTRAINT PK_SampleType 
	PRIMARY KEY CLUSTERED (sampleTypeID)
;

ALTER TABLE ScreenSeriesType ADD CONSTRAINT PK_ScreenSeriesType 
	PRIMARY KEY CLUSTERED (screenSeriesTypeID)
;

ALTER TABLE SplittingMethodType ADD CONSTRAINT PK_SplittingMethodType 
	PRIMARY KEY CLUSTERED (splittingMethodTypeID)
;

ALTER TABLE UOM ADD CONSTRAINT PK_UOM 
	PRIMARY KEY CLUSTERED (uOMID)
;

ALTER TABLE WeighingMethodType ADD CONSTRAINT PK_WeighingMethodType 
	PRIMARY KEY CLUSTERED (weighingMethodTypeID)
;
