CREATE TABLE [agents](
  [agentID] INTEGER PRIMARY KEY NOT NULL, 
  [divisionID] TINYINT, 
  [corporationID] INTEGER, 
  [locationID] INTEGER, 
  [level] TINYINT, 
  [agentTypeID] INTEGER, 
  [isLocator] INTEGER);

CREATE TABLE [agentsInSpace](
  [agentID] INTEGER PRIMARY KEY NOT NULL, 
  [dungeonID] INTEGER, 
  [solarSystemID] INTEGER, 
  [spawnPointID] INTEGER, 
  [typeID] INTEGER);

CREATE TABLE [ALL_BLUEPRINTS](
  [BLUEPRINT_ID] INT, 
  [BLUEPRINT_NAME] TEXT, 
  [BLUEPRINT_GROUP_ID] INT, 
  [BLUEPRINT_GROUP] TEXT, 
  [ITEM_ID] INT, 
  [ITEM_NAME] TEXT, 
  [ITEM_GROUP_ID] INT, 
  [ITEM_GROUP] TEXT, 
  [ITEM_CATEGORY_ID] INT, 
  [ITEM_CATEGORY] TEXT, 
  [MARKET_GROUP_ID] INT, 
  [MARKET_GROUP] TEXT, 
  [TECH_LEVEL], 
  [PORTION_SIZE] INT, 
  [BASE_PRODUCTION_TIME] INT, 
  [BASE_RESEARCH_TL_TIME] INT, 
  [BASE_RESEARCH_ML_TIME] INT, 
  [BASE_COPY_TIME] INT, 
  [BASE_INVENTION_TIME] INT, 
  [MAX_PRODUCTION_LIMIT] INT, 
  [ITEM_TYPE] REAL, 
  [RACE_ID] INT, 
  [META_GROUP] INT, 
  [SIZE_GROUP], 
  "IGNORE", 
  [FAVORITE]);

CREATE TABLE [ALL_BLUEPRINT_MATERIALS](
  [BLUEPRINT_ID] INT, 
  [BLUEPRINT_NAME] TEXT, 
  [PRODUCT_ID] INT, 
  [MATERIAL_ID] INT, 
  [MATERIAL] TEXT, 
  [MAT_GROUP_ID] INT, 
  [MATERIAL_GROUP] TEXT, 
  [MAT_CATEGORY_ID] INT, 
  [MATERIAL_CATEGORY] TEXT, 
  [MATERIAL_VOLUME] REAL, 
  [QUANTITY] INT, 
  [ACTIVITY] INT, 
  [CONSUME]);

CREATE TABLE [ancestries](
  [ancestryID] TINYINT PRIMARY KEY NOT NULL, 
  [ancestryName] NVARCHAR(100), 
  [bloodlineID] TINYINT, 
  [description] NVARCHAR(1000), 
  [perception] TINYINT, 
  [willpower] TINYINT, 
  [charisma] TINYINT, 
  [memory] TINYINT, 
  [intelligence] TINYINT, 
  [iconID] INTEGER, 
  [shortDescription] NVARCHAR(500));

CREATE TABLE [bloodlineLastNames](
  [bloodlineID] TINYINT NOT NULL, 
  [lastName] NVARCHAR(100));

CREATE TABLE [bloodlines](
  [bloodlineID] TINYINT PRIMARY KEY NOT NULL, 
  [bloodlineName] NVARCHAR(100), 
  [raceID] TINYINT, 
  [description] NVARCHAR(1000), 
  [shipTypeID] INTEGER, 
  [corporationID] INTEGER, 
  [perception] TINYINT, 
  [willpower] TINYINT, 
  [charisma] TINYINT, 
  [memory] TINYINT, 
  [intelligence] TINYINT, 
  [iconID] INTEGER);

CREATE TABLE [characterAttributes](
  [attributeID] TINYINT PRIMARY KEY NOT NULL, 
  [attributeName] VARCHAR(100), 
  [description] VARCHAR(1000), 
  [iconID] INTEGER, 
  [shortDescription] NVARCHAR(500), 
  [notes] NVARCHAR(500));

CREATE TABLE [contrabandTypes](
  [typeID] INTEGER, 
  [factionID] INTEGER, 
  [attackMinSec] REAL, 
  [confiscateMinSec] REAL, 
  [fineByValue] REAL, 
  [standingLoss] REAL);

CREATE TABLE [controlTowerResources](
  [typeID] INTEGER, 
  [purpose] INTEGER, 
  [factionID] INTEGER, 
  [minSecurityLevel] REAL, 
  [quantity] INTEGER, 
  [resourceTypeID] INTEGER);

CREATE TABLE [corporationActivities](
  [activityID] TINYINT PRIMARY KEY NOT NULL, 
  [activityName] NVARCHAR(100));

CREATE TABLE [corporationAllowedMemberRaces](
  [corporationID] INTEGER NOT NULL, 
  [memberRace] INTEGER);

CREATE TABLE [corporationDivisions](
  [corporationID] INTEGER NOT NULL, 
  [divisionID] INTEGER NOT NULL, 
  [divisionNumber] INTEGER, 
  [leaderID] INTEGER, 
  [size] INTEGER);

CREATE TABLE [corporationExchangeRates](
  [corporationID] INTEGER NOT NULL, 
  [exchangeID] INTEGER, 
  [exchangeRate] REAL);

CREATE TABLE [corporationInvestors](
  [corporationID] INTEGER NOT NULL, 
  [investorID] INTEGER, 
  [shares] REAL);

CREATE TABLE [corporationLPOffers](
  [corporationID] INTEGER NOT NULL, 
  [lpOfferTableID] INTEGER);

CREATE TABLE [corporationTrades](
  [corporationID] INTEGER NOT NULL, 
  [typeID] INTEGER, 
  [value] REAL);

CREATE TABLE [crpNPCCorporationResearchFields](
  [skillID] INTEGER NOT NULL, 
  [corporationID] INTEGER NOT NULL);

CREATE TABLE [crtCertificates](
  [certificateID] INTEGER PRIMARY KEY NOT NULL, 
  [groupID] INTEGER, 
  [name] VARCHAR(100), 
  [description] TEXT);

CREATE TABLE [crtCertificateSkills](
  [certificateID] INTEGER, 
  [masteryLevel] TINYINT, 
  [masteryText] VARCHAR(10), 
  [skillTypeID] INTEGER, 
  [requiredSkillLevel] TINYINT);

CREATE TABLE [crtMasteries](
  [typeID] INTEGER, 
  [masteryLevel] TINYINT, 
  [masteryRecommendedTypeID] INTEGER);

CREATE TABLE [crtRecommendedTypes](
  [certificateID] INTEGER, 
  [typeID] INTEGER);

CREATE TABLE [dogmaAttributeCategories](
  [categoryID] TINYINT PRIMARY KEY NOT NULL, 
  [name] NVARCHAR(50), 
  [description] NVARCHAR(200));

CREATE TABLE [dogmaAttributes](
  [attributeID] SMALLINT PRIMARY KEY NOT NULL, 
  [attributeName] VARCHAR(100), 
  [description] VARCHAR(1000), 
  [displayNameID] VARCHAR(1000), 
  [dataType] VARCHAR(100), 
  [iconID] INTEGER, 
  [defaultValue] REAL, 
  [published] INTEGER, 
  [stackable] INTEGER, 
  [name] VARCHAR(1000), 
  [unitID] TINYINT, 
  [highIsGood] INTEGER, 
  [categoryID] TINYINT, 
  [tooltipDescriptionID] VARCHAR(1000), 
  [tooltipTitleID] VARCHAR(1000), 
  [maxAttributeID] INTEGER, 
  [chargeRechargeTimeID] SMALLINT);

CREATE TABLE [dogmaEffects](
  [effectID] SMALLINT PRIMARY KEY NOT NULL, 
  [descriptionID] VARCHAR(1000), 
  [disallowAutoRepeat] INTEGER, 
  [displayNameID] VARCHAR(1000), 
  [dischargeAttributeID] SMALLINT, 
  [distribution] VARCHAR(100), 
  [durationAttributeID] SMALLINT, 
  [effectCategory] VARCHAR(100), 
  [effectName] VARCHAR(400), 
  [electronicChance] INTEGER, 
  [fittingUsageChanceAttributeID] SMALLINT, 
  [falloffAttributeID] SMALLINT, 
  [guid] VARCHAR(60), 
  [iconID] INTEGER, 
  [isAssistance] INTEGER, 
  [isOffensive] INTEGER, 
  [isWarpSafe] INTEGER, 
  [npcUsageChanceAttributeID] SMALLINT, 
  [npcActivationChanceAttributeID] SMALLINT, 
  [propulsionChance] INTEGER, 
  [published] INTEGER, 
  [rangeAttributeID] SMALLINT, 
  [resistanceAttributeID] SMALLINT, 
  [rangeChance] INTEGER, 
  [sfxName] VARCHAR(20), 
  [trackingSpeedAttributeID] SMALLINT);

CREATE TABLE [dogmaEffectsModifierInfo](
  [effectID] SMALLINT, 
  [domain] VARCHAR(50), 
  [func] VARCHAR(50), 
  [groupID] INTEGER, 
  [modifiedAttributeID] INTEGER, 
  [modifyingAttributeID] INTEGER, 
  [operation] VARCHAR(50), 
  [skillTypeID] INTEGER, 
  [secondaryEffectID] SMALLINT);

CREATE TABLE [dogmaTypeAttributes](
  [typeID] INTEGER NOT NULL, 
  [attributeID] SMALLINT NOT NULL, 
  [value] REAL);

CREATE TABLE [dogmaTypeEffects](
  [typeID] INTEGER NOT NULL, 
  [effectID] SMALLINT NOT NULL, 
  [isDefault] INTEGER);

CREATE TABLE [evegraphicBackgrounds](
  [graphicID] INTEGER, 
  [backgroundProperty] VARCHAR(50));

CREATE TABLE [evegraphicForegrounds](
  [graphicID] INTEGER, 
  [foregroundProperty] VARCHAR(50));

CREATE TABLE [evegraphicIconInfo](
  [graphicID] INTEGER, 
  [folder] VARCHAR(100));

CREATE TABLE [eveGraphics](
  [graphicID] INTEGER PRIMARY KEY NOT NULL, 
  [graphicFile] VARCHAR(100), 
  [description] TEXT, 
  [sofFactionName] VARCHAR(100), 
  [sofHullName] VARCHAR(100), 
  [sofRaceName] VARCHAR(100));

CREATE TABLE [eveIcons](
  [iconID] INTEGER PRIMARY KEY NOT NULL, 
  [iconFile] VARCHAR(500), 
  [description] TEXT, 
  [obsolete] INTEGER);

CREATE TABLE [eveIconsBackgrounds](
  [graphicID] INTEGER, 
  [backgroundProperty] VARCHAR(50));

CREATE TABLE [eveIconsForegrounds](
  [graphicID] INTEGER, 
  [foregroundProperty] VARCHAR(50));

CREATE TABLE [eveUnits](
  [unitID] TINYINT, 
  [unitName] VARCHAR(100), 
  [displayName] VARCHAR(50), 
  [description] VARCHAR(1000));

CREATE TABLE [factions](
  [factionID] INTEGER PRIMARY KEY NOT NULL, 
  [factionName] VARCHAR(100), 
  [description] VARCHAR(1500), 
  [solarSystemID] INTEGER, 
  [corporationID] INTEGER, 
  [sizeFactor] REAL, 
  [militiaCorporationID] INTEGER, 
  [iconID] INTEGER, 
  [uniqueName] INTEGER);

CREATE TABLE [factionsMemberRaces](
  [factionID] INTEGER NOT NULL, 
  [memberRace] INTEGER);

CREATE TABLE [industryActivities](
  [blueprintTypeID] INTEGER NOT NULL, 
  [activityID] TINYINT NOT NULL, 
  [time] INTEGER);

CREATE TABLE [industryActivityMaterials](
  [blueprintTypeID] INTEGER, 
  [activityID] TINYINT, 
  [materialTypeID] INTEGER, 
  [quantity] INTEGER);

CREATE TABLE [industryActivityProducts](
  [blueprintTypeID] INTEGER, 
  [activityID] TINYINT, 
  [productTypeID] INTEGER, 
  [quantity] INTEGER, 
  [probability] REAL);

CREATE TABLE [industryActivitySkills](
  [blueprintTypeID] INTEGER, 
  [activityID] TINYINT, 
  [skillID] INTEGER, 
  [level] TINYINT);

CREATE TABLE [industryBlueprints](
  [blueprintTypeID] INTEGER PRIMARY KEY NOT NULL, 
  [maxProductionLimit] INTEGER);

CREATE TABLE [invCategories](
  [categoryID] INTEGER PRIMARY KEY NOT NULL, 
  [categoryName] NVARCHAR(500), 
  [published] INTEGER, 
  [iconID] INTEGER);

CREATE TABLE [invFlags](
  [flagID] SMALLINT PRIMARY KEY NOT NULL, 
  [flagName] VARCHAR(200), 
  [flagText] VARCHAR(100), 
  [orderID] INTEGER);

CREATE TABLE [invGroups](
  [groupID] INTEGER PRIMARY KEY NOT NULL, 
  [categoryID] INTEGER, 
  [groupName] NVARCHAR(500), 
  [iconID] INTEGER, 
  [useBasePrice] INTEGER, 
  [anchored] INTEGER, 
  [anchorable] INTEGER, 
  [fittableNonSingleton] INTEGER, 
  [published] INTEGER);

CREATE TABLE [invItems](
  [itemID] BIGINT PRIMARY KEY NOT NULL, 
  [typeID] INTEGER, 
  [ownerID] INTEGER, 
  [locationID] BIGINT, 
  [flagID] SMALLINT, 
  [quantity] INTEGER);

CREATE TABLE [invNames](
  [itemID] BIGINT PRIMARY KEY NOT NULL, 
  [itemName] NVARCHAR(200));

CREATE TABLE [invPositions](
  [itemID] BIGINT PRIMARY KEY NOT NULL, 
  [x] REAL NOT NULL, 
  [y] REAL NOT NULL, 
  [z] REAL NOT NULL, 
  [yaw] REAL, 
  [pitch] REAL, 
  [roll] REAL);

CREATE TABLE [invTraits](
  [bonusID] INTEGER, 
  [typeID] INTEGER, 
  [iconID] INTEGER, 
  [skilltypeID] INTEGER, 
  [bonus] REAL, 
  [bonusText] TEXT, 
  [importance] INTEGER, 
  [nameID] INTEGER, 
  [unitID] INTEGER, 
  [isPositive] INTEGER);

CREATE TABLE [invTypeReactions](
  [reactionTypeID] INTEGER NOT NULL, 
  [input] INTEGER NOT NULL, 
  [typeID] INTEGER NOT NULL, 
  [quantity] SMALLINT);

CREATE TABLE [invTypes](
  [typeID] INTEGER PRIMARY KEY NOT NULL, 
  [groupID] INTEGER, 
  [typeName] NVARCHAR(500), 
  [description] TEXT, 
  [mass] REAL, 
  [volume] REAL, 
  [packagedVolume] REAL, 
  [capacity] REAL, 
  [portionSize] INTEGER, 
  [factionID] INTEGER, 
  [raceID] TINYINT, 
  [basePrice] REAL, 
  [published] INTEGER, 
  [marketGroupID] INTEGER, 
  [graphicID] INTEGER, 
  [radius] REAL, 
  [iconID] INTEGER, 
  [soundID] INTEGER, 
  [sofFactionName] NVARCHAR(100), 
  [sofMaterialSetID] INTEGER, 
  [metaGroupID] INTEGER, 
  [variationparentTypeID] INTEGER);

CREATE TABLE [invUniqueNames](
  [itemID] INTEGER PRIMARY KEY NOT NULL, 
  [itemName] NVARCHAR(200), 
  [groupID] INTEGER);

CREATE TABLE [mapCelestialStatistics](
  [celestialID] INTEGER PRIMARY KEY NOT NULL, 
  [temperature] REAL, 
  [spectralClass] VARCHAR(10), 
  [luminosity] REAL, 
  [age] REAL, 
  [life] REAL, 
  [orbitRadius] REAL, 
  [eccentricity] REAL, 
  [massDust] REAL, 
  [massGas] REAL, 
  [fragmented] INTEGER, 
  [density] REAL, 
  [surfaceGravity] REAL, 
  [escapeVelocity] REAL, 
  [orbitPeriod] REAL, 
  [rotationRate] REAL, 
  [locked] INTEGER, 
  [pressure] REAL, 
  [radius] REAL, 
  [heightMap1] INTEGER, 
  [heightMap2] INTEGER, 
  [population] INTEGER, 
  [shaderPreset] INTEGER);

CREATE TABLE [mapConstellationJumps](
  [fromRegionID] INTEGER, 
  [fromConstellationID] INTEGER NOT NULL, 
  [toConstellationID] INTEGER NOT NULL, 
  [toRegionID] INTEGER);

CREATE TABLE [mapConstellations](
  [constellationID] INTEGER PRIMARY KEY NOT NULL, 
  [constellationName] VARCHAR(100), 
  [regionID] INTEGER, 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [x_Min] REAL, 
  [x_Max] REAL, 
  [y_Min] REAL, 
  [y_Max] REAL, 
  [z_Min] REAL, 
  [z_Max] REAL, 
  [radius] REAL, 
  [factionID] INTEGER);

CREATE TABLE [mapDenormalize](
  [itemID] INTEGER PRIMARY KEY NOT NULL, 
  [typeID] INTEGER, 
  [solarSystemID] INTEGER, 
  [constellationID] INTEGER, 
  [regionID] INTEGER, 
  [orbitID] INTEGER, 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [radius] REAL, 
  [nameID] INTEGER, 
  [security] REAL, 
  [celestialIndex] INTEGER, 
  [orbitIndex] INTEGER);

CREATE TABLE [mapDisallowedAnchorCategories](
  [solarSystemID] INTEGER PRIMARY KEY NOT NULL, 
  [categoryID] INTEGER);

CREATE TABLE [mapDisallowedAnchorGroups](
  [solarSystemID] INTEGER PRIMARY KEY NOT NULL, 
  [groupID] INTEGER);

CREATE TABLE [mapItemEffectBeacons](
  [itemID] INTEGER PRIMARY KEY NOT NULL, 
  [effectBeaconTypeID] INTEGER);

CREATE TABLE [mapJumps](
  [stargateID] INTEGER PRIMARY KEY NOT NULL, 
  [destinationID] INTEGER, 
  [typeID] INTEGER, 
  [x] REAL, 
  [y] REAL, 
  [z] REAL);

CREATE TABLE [mapLandmarks](
  [landmarkID] INTEGER PRIMARY KEY NOT NULL, 
  [descriptionID] INTEGER, 
  [description] TEXT, 
  [landmarkNameID] INTEGER, 
  [landmarkName] NVARCHAR(100), 
  [locationID] INTEGER, 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [iconID] INTEGER);

CREATE TABLE [mapLocationScenes](
  [locationID] INTEGER PRIMARY KEY NOT NULL, 
  [graphicID] INTEGER);

CREATE TABLE [mapLocationWormholeClasses](
  [locationID] INTEGER PRIMARY KEY NOT NULL, 
  [wormholeClassID] INTEGER);

CREATE TABLE [mapRegionJumps](
  [fromRegionID] INTEGER NOT NULL, 
  [toRegionID] INTEGER NOT NULL);

CREATE TABLE [mapRegions](
  [regionID] INTEGER PRIMARY KEY NOT NULL, 
  [regionName] VARCHAR(100), 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [x_Min] REAL, 
  [x_Max] REAL, 
  [y_Min] REAL, 
  [y_Max] REAL, 
  [z_Min] REAL, 
  [z_Max] REAL, 
  [factionID] INTEGER, 
  [nameID] INTEGER, 
  [descriptionID] INTEGER);

CREATE TABLE [mapSolarSystemJumps](
  [fromRegionID] INTEGER, 
  [fromConstellationID] INTEGER, 
  [fromSolarSystemID] INTEGER NOT NULL, 
  [toSolarSystemID] INTEGER NOT NULL, 
  [toConstellationID] INTEGER, 
  [toRegionID] INTEGER);

CREATE TABLE [mapSolarSystems](
  [solarSystemID] INTEGER PRIMARY KEY NOT NULL, 
  [solarSystemName] VARCHAR(100), 
  [regionID] INTEGER, 
  [constellationID] INTEGER, 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [x_Min] REAL, 
  [x_Max] REAL, 
  [y_Min] REAL, 
  [y_Max] REAL, 
  [z_Min] REAL, 
  [z_Max] REAL, 
  [luminosity] REAL, 
  [border] INTEGER, 
  [corridor] INTEGER, 
  [fringe] INTEGER, 
  [hub] INTEGER, 
  [international] INTEGER, 
  [regional] INTEGER, 
  [security] REAL, 
  [factionID] INTEGER, 
  [radius] REAL, 
  [sunTypeID] INTEGER, 
  [securityClass] VARCHAR(2), 
  [solarSystemNameID] INTEGER, 
  [visualEffect] VARCHAR(50), 
  [descriptionID] INTEGER);

CREATE TABLE [mapUniverse](
  [universeID] INTEGER PRIMARY KEY NOT NULL, 
  [universeName] VARCHAR(100), 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [x_Min] REAL, 
  [x_Max] REAL, 
  [y_Min] REAL, 
  [y_Max] REAL, 
  [z_Min] REAL, 
  [z_Max] REAL, 
  [radius] REAL);

CREATE TABLE [marketGroups](
  [marketGroupID] INTEGER PRIMARY KEY NOT NULL, 
  [descriptionID] NVARCHAR(300), 
  [hasTypes] INTEGER, 
  [iconID] INTEGER, 
  [nameID] NVARCHAR(100), 
  [parentGroupID] INTEGER);

CREATE TABLE [metaGroups](
  [metaGroupID] SMALLINT PRIMARY KEY NOT NULL, 
  [descriptionID] NVARCHAR(1000), 
  [iconID] INTEGER, 
  [iconSuffix] NVARCHAR(30), 
  [nameID] NVARCHAR(100));

CREATE TABLE [npcCorporationDivisions](
  [divisionID] TINYINT PRIMARY KEY NOT NULL, 
  [divisionName] VARCHAR(100), 
  [leaderTypeName] VARCHAR(100), 
  [shortDescription] VARCHAR(100), 
  [longDescription] VARCHAR(1000));

CREATE TABLE [npcCorporations](
  [corporationID] INTEGER PRIMARY KEY NOT NULL, 
  [tickerName] NVARCHAR(5), 
  [corporationName] NVARCHAR(100), 
  [corporationDescription] NVARCHAR(1000), 
  [uniqueName] INTEGER, 
  [taxRate] REAL, 
  [memberLimit] INTEGER, 
  [hasPlayerPersonnelManager] INTEGER, 
  [factionID] INTEGER, 
  [ceoID] INTEGER, 
  [deleted] INTEGER, 
  [extent] CHARACTER(1), 
  [friendID] INTEGER, 
  [enemyID] INTEGER, 
  [solarSystemID] INTEGER, 
  [stationID] INTEGER, 
  [minSecurity] REAL, 
  [minimumJoinStanding] REAL, 
  [publicShares] BIGINT, 
  [shares] BIGINT, 
  [initialPrice] REAL, 
  [mainActivityID] INTEGER, 
  [secondaryActivityID] INTEGER, 
  [size] CHARACTER(1), 
  [sizeFactor] REAL, 
  [raceID] INTEGER, 
  [sendCharTerminationMessage] INTEGER, 
  [url] NVARCHAR(1000), 
  [iconID] INTEGER);

CREATE TABLE [planetSchematics](
  [schematicID] SMALLINT PRIMARY KEY NOT NULL, 
  [schematicName] NVARCHAR(255), 
  [cycleTime] INTEGER);

CREATE TABLE [planetSchematicsPinMap](
  [schematicID] SMALLINT, 
  [pinTypeID] INTEGER);

CREATE TABLE [planetSchematicsTypeMap](
  [schematicID] SMALLINT, 
  [typeID] INTEGER, 
  [quantity] SMALLINT, 
  [isInput] INTEGER);

CREATE TABLE [races](
  [raceID] TINYINT, 
  [raceName] VARCHAR(100), 
  [raceDescription] VARCHAR(1000), 
  [iconID] INTEGER);

CREATE TABLE [raceSkills](
  [raceID] INTEGER NOT NULL, 
  [skillTypeID] INTEGER, 
  [level] INTEGER);

CREATE TABLE [ramActivities](
  [activityID] TINYINT PRIMARY KEY NOT NULL, 
  [activityName] NVARCHAR(100), 
  [iconNo] VARCHAR(5), 
  [description] NVARCHAR(1000), 
  [published] INTEGER);

CREATE TABLE [ramAssemblyLineStations](
  [stationID] INTEGER NOT NULL, 
  [assemblyLineTypeID] TINYINT NOT NULL, 
  [quantity] TINYINT, 
  [stationTypeID] INTEGER, 
  [ownerID] INTEGER, 
  [solarSystemID] INTEGER, 
  [regionID] INTEGER);

CREATE TABLE [ramAssemblyLineTypeDetailPerCategory](
  [assemblyLineTypeID] TINYINT NOT NULL, 
  [categoryID] INTEGER NOT NULL, 
  [timeMultiplier] REAL, 
  [materialMultiplier] REAL, 
  [costMultiplier] REAL);

CREATE TABLE [ramAssemblyLineTypeDetailPerGroup](
  [assemblyLineTypeID] TINYINT NOT NULL, 
  [groupID] INTEGER NOT NULL, 
  [timeMultiplier] REAL, 
  [materialMultiplier] REAL, 
  [costMultiplier] REAL);

CREATE TABLE [ramAssemblyLineTypes](
  [assemblyLineTypeID] TINYINT PRIMARY KEY NOT NULL, 
  [assemblyLineTypeName] NVARCHAR(100), 
  [description] NVARCHAR(1000), 
  [baseTimeMultiplier] REAL, 
  [baseMaterialMultiplier] REAL, 
  [baseCostMultiplier] REAL, 
  [volume] REAL, 
  [activityID] TINYINT, 
  [minCostPerHour] REAL);

CREATE TABLE [ramInstallationTypeContents](
  [installationTypeID] INTEGER NOT NULL, 
  [assemblyLineTypeID] TINYINT NOT NULL, 
  [quantity] TINYINT);

CREATE TABLE [researchAgents](
  [agentID] INTEGER NOT NULL, 
  [typeID] INTEGER NOT NULL);

CREATE TABLE [skinLicenses](
  [licenseTypeID] INTEGER PRIMARY KEY NOT NULL, 
  [duration] INTEGER, 
  [isSingleUse] INTEGER, 
  [skinID] INTEGER);

CREATE TABLE [skinMaterials](
  [skinMaterialID] INTEGER PRIMARY KEY NOT NULL, 
  [displayNameID] INTEGER, 
  [materialSetID] INTEGER);

CREATE TABLE [skins](
  [skinID] INTEGER, 
  [skinDescription] TEXT, 
  [internalName] VARCHAR(100), 
  [skinMaterialID] INTEGER, 
  [isStructureSkin] INTEGER, 
  [typeID] INTEGER, 
  [allowCCPDevs] INTEGER, 
  [visibleSerenity] INTEGER, 
  [visibleTranquility] INTEGER);

CREATE TABLE [staStations](
  [stationID] INTEGER PRIMARY KEY NOT NULL, 
  [security] REAL, 
  [dockingCostPerVolume] REAL, 
  [maxShipVolumeDockable] REAL, 
  [officeRentalCost] INTEGER, 
  [operationID] TINYINT, 
  [stationTypeID] INTEGER, 
  [corporationID] INTEGER, 
  [solarSystemID] INTEGER, 
  [constellationID] INTEGER, 
  [regionID] INTEGER, 
  [stationName] NVARCHAR(100), 
  [x] REAL, 
  [y] REAL, 
  [z] REAL, 
  [reprocessingEfficiency] REAL, 
  [reprocessingStationsTake] REAL, 
  [reprocessingHangarFlag] TINYINT);

CREATE TABLE [staStationTypes](
  [stationTypeID] INTEGER, 
  [dockEntryX] REAL, 
  [dockEntryY] REAL, 
  [dockEntryZ] REAL, 
  [dockOrientationX] REAL, 
  [dockOrientationY] REAL, 
  [dockOrientationZ] REAL, 
  [operationID] TINYINT, 
  [officeSlots] TINYINT, 
  [reprocessingEfficiency] REAL, 
  [conquerable] INTEGER);

CREATE TABLE [stationOperations](
  [operationID] TINYINT PRIMARY KEY NOT NULL, 
  [operationName] NVARCHAR(1000), 
  [activityID] TINYINT, 
  [border] REAL, 
  [corridor] REAL, 
  [description] NVARCHAR(1000), 
  [fringe] REAL, 
  [hub] REAL, 
  [manufacturingFactor] REAL, 
  [ratio] REAL, 
  [researchFactor] REAL);

CREATE TABLE [stationOperationServices](
  [operationID] INTEGER NOT NULL, 
  [serviceID] INTEGER);

CREATE TABLE [stationOperationTypes](
  [operationID] INTEGER NOT NULL, 
  [raceID] INTEGER, 
  [stationTypeID] INTEGER);

CREATE TABLE [stationServices](
  [serviceID] INTEGER PRIMARY KEY NOT NULL, 
  [serviceName] NVARCHAR(100), 
  [description] NVARCHAR(1000));

CREATE TABLE [tntTournamentBannedGroups](
  [ruleSetID] VARCHAR(100), 
  [groupID] INTEGER);

CREATE TABLE [tntTournamentBannedTypes](
  [ruleSetID] VARCHAR(100), 
  [typeID] INTEGER);

CREATE TABLE [tntTournamentGroupPoints](
  [ruleSetID] VARCHAR(100), 
  [groupID] INTEGER, 
  [points] INTEGER);

CREATE TABLE [tntTournaments](
  [ruleSetID] VARCHAR(100) PRIMARY KEY NOT NULL, 
  [ruleSetName] VARCHAR(100), 
  [maximumPointsMatch] INTEGER, 
  [maximumPilotsMatch] INTEGER);

CREATE TABLE [tntTournamentTypePoints](
  [ruleSetID] VARCHAR(100), 
  [typeID] INTEGER, 
  [points] INTEGER);

CREATE TABLE [trnTranslationColumns](
  [columnName] NVARCHAR(128), 
  [masterID] NVARCHAR(128), 
  [tableName] NVARCHAR(256), 
  [tcGroupID] SMALLINT, 
  [tcID] SMALLINT);

CREATE TABLE [trnTranslationLanguages](
  [numericLanguageID] INTEGER PRIMARY KEY NOT NULL, 
  [languageID] VARCHAR(50), 
  [languageName] NVARCHAR(200));

CREATE TABLE [trnTranslations](
  [keyID] INTEGER, 
  [languageID] VARCHAR(50), 
  [tcID] SMALLINT, 
  [text] TEXT);

CREATE TABLE [typeMaterials](
  [typeID] INTEGER NOT NULL, 
  [materialTypeID] INTEGER NOT NULL, 
  [quantity] INTEGER);

CREATE TABLE [warCombatZones](
  [combatZoneID] INTEGER PRIMARY KEY NOT NULL, 
  [combatZoneName] NVARCHAR(100), 
  [factionID] INTEGER, 
  [centerSystemID] INTEGER, 
  [description] NVARCHAR(500));

CREATE TABLE [warCombatZoneSystems](
  [solarSystemID] INTEGER PRIMARY KEY NOT NULL, 
  [combatZoneID] INTEGER);

CREATE VIEW [MY_INDUSTRY_MATERIALS]
AS
SELECT 
       [blueprintTypeID], 
       [activityID], 
       [materialTypeID], 
       [quantity], 
       1 AS [consume]
FROM   [industryActivityMaterials]
UNION
SELECT 
       [blueprintTypeID], 
       [activityID], 
       [skillID] AS [materialTypeID], 
       [level] AS [quantity], 
       0 AS [consume]
FROM   [industryActivitySkills];

CREATE VIEW [PRICES_BUILD]
AS
SELECT 
       [ALL_BLUEPRINTS].[ITEM_ID], 
       [ALL_BLUEPRINTS].[ITEM_NAME], 
       [ALL_BLUEPRINTS].[TECH_LEVEL], 
       0 AS [PRICE], 
       [ALL_BLUEPRINTS].[ITEM_CATEGORY_ID], 
       [ALL_BLUEPRINTS].[ITEM_CATEGORY], 
       [ALL_BLUEPRINTS].[ITEM_GROUP_ID], 
       [ALL_BLUEPRINTS].[ITEM_GROUP], 
       1 AS [MANUFACTURE], 
       [ALL_BLUEPRINTS].[ITEM_TYPE], 
       'None' AS [PRICE_TYPE]
FROM   [ALL_BLUEPRINTS]
WHERE  [ITEM_ID] <> 33195;

CREATE VIEW [PRICES_NOBUILD]
AS
SELECT *
FROM   (SELECT DISTINCT 
                        [MATERIAL_ID], 
                        [MATERIAL], 
                        0 AS [TECH_LEVEL], 
                        0 AS [PRICE], 
                        [MAT_CATEGORY_ID], 
                        [MATERIAL_CATEGORY], 
                        [MAT_GROUP_ID], 
                        [MATERIAL_GROUP], 
                        0 AS [MANUFACTURE], 
                        0 AS [ITEM_TYPE], 
                        'None' AS [PRICE_TYPE]
        FROM   [ALL_BLUEPRINT_MATERIALS]
        WHERE  [MATERIAL_ID] NOT IN (SELECT [ITEM_ID]
               FROM   [ALL_BLUEPRINTS])
                 AND [MATERIAL_CATEGORY] <> 'Skill'
        UNION
        SELECT DISTINCT 
                        [typeID] AS [MATERIAL_ID], 
                        [typeName] AS [MATERIAL], 
                        0 AS [TECH_LEVEL], 
                        0 AS [PRICE], 
                        [invCategories].[categoryID] AS [MAT_CATEGORY_ID], 
                        [categoryName] AS [MATERIAL_CATEGORY], 
                        [invGroups].[groupID] AS [MAT_GROUP_ID], 
                        [groupName] AS [MATERIAL_GROUP], 
                        0 AS [MANUFACTURE], 
                        0 AS [ITEM_TYPE], 
                        'None' AS [PRICE_TYPE]
        FROM   [invTypes],
               [invGroups],
               [invCategories]
        WHERE  [invTypes].[groupID] = [invGroups].[groupID]
                 AND [invGroups].[categoryID] = [invCategories].[categoryID]
                 AND [invTypes].[published] <> 0
                 AND [invGroups].[published] <> 0
                 AND [invCategories].[published] <> 0
                 AND [invTypes].[marketGroupID] IS NOT NULL
                 AND ([categoryName] IN ('Asteroid', 'Decryptors', 'Planetary Commodities', 'Planetary Resources')
                 OR [groupName] IN ('Moon Materials', 'Ice Product', 'Harvestable Cloud', 'Intermediate Materials')
                 OR [typeID] IN (41, 3699, 3773, 9850, 33195))) AS X
WHERE  [MATERIAL_ID] NOT IN (SELECT [ITEM_ID]
       FROM   [PRICES_BUILD]);

CREATE INDEX [IDX_agentsInSpace_AID] ON [agentsInSpace]([agentID]);

CREATE INDEX [IDX_agentsInSpace_SSID] ON [agentsInSpace]([solarSystemID]);

CREATE INDEX [IDX_agents_CID] ON [agents]([corporationID]);

CREATE INDEX [IDX_agents_LID] ON [agents]([locationID]);

CREATE INDEX [IDX_contrabandTypes_TID] ON [contrabandTypes]([typeID]);

CREATE INDEX [IDX_controlTowerResources_TID]
ON [controlTowerResources]([typeID]);

CREATE UNIQUE INDEX [IDX_crpNPCCorporationResearchFields_PK]
ON [crpNPCCorporationResearchFields](
  [skillID], 
  [corporationID]);

CREATE INDEX [IDX_crtCertificateSkills_CID]
ON [crtCertificateSkills]([certificateID]);

CREATE INDEX [IDX_crtCertificateSkills_SID]
ON [crtCertificateSkills]([skillTypeID]);

CREATE INDEX [IDX_crtMasteries_TID] ON [crtMasteries]([typeID]);

CREATE INDEX [IDX_crtRecommendedTypes_TID_CID]
ON [crtRecommendedTypes](
  [typeID], 
  [certificateID]);

CREATE UNIQUE INDEX [IDX_dogmaEffectsModifierInfo_EID]
ON [dogmaEffectsModifierInfo](
  [effectID], 
  [domain], 
  [func], 
  [groupID], 
  [modifiedAttributeID], 
  [modifyingAttributeID], 
  [operation], 
  [skillTypeID]);

CREATE UNIQUE INDEX [IDX_dogmaTypeAttributes_PK]
ON [dogmaTypeAttributes](
  [typeID], 
  [attributeID]);

CREATE UNIQUE INDEX [IDX_dogmaTypeEffects_PK]
ON [dogmaTypeEffects](
  [typeID], 
  [effectID]);

CREATE INDEX [IDX_evegraphicBackgrounds_GID]
ON [evegraphicBackgrounds]([graphicID]);

CREATE INDEX [IDX_evegraphicForegrounds_GID]
ON [evegraphicForegrounds]([graphicID]);

CREATE INDEX [IDX_evegraphicIconInfo_GID] ON [evegraphicIconInfo]([graphicID]);

CREATE INDEX [IDX_eveIconsBackgrounds_GID] ON [eveIconsBackgrounds]([graphicID]);

CREATE INDEX [IDX_eveIconsForegrounds_GID] ON [eveIconsForegrounds]([graphicID]);

CREATE INDEX [IDX_industryActivities_AID] ON [industryActivities]([activityID]);

CREATE UNIQUE INDEX [IDX_industryActivities_PK]
ON [industryActivities](
  [blueprintTypeID], 
  [activityID]);

CREATE INDEX [IDX_industryActivityMaterials_TID_AID]
ON [industryActivityMaterials](
  [blueprintTypeID], 
  [activityID]);

CREATE INDEX [IDX_industryActivityProducts_PTID]
ON [industryActivityProducts]([productTypeID]);

CREATE INDEX [IDX_industryActivityProducts_TID_AID]
ON [industryActivityProducts](
  [blueprintTypeID], 
  [activityID]);

CREATE INDEX [IDX_industryActivitySkills_TID_AID]
ON [industryActivitySkills](
  [blueprintTypeID], 
  [activityID]);

CREATE INDEX [IDX_industryBlueprints_BPID]
ON [industryBlueprints]([blueprintTypeID]);

CREATE INDEX [IDX_invCategories_CID] ON [invCategories]([categoryID]);

CREATE INDEX [IDX_invGroups_CID] ON [invGroups]([categoryID]);

CREATE INDEX [IDX_invGroups_GID] ON [invGroups]([groupID]);

CREATE INDEX [IDX_invItems_LID] ON [invItems]([locationID]);

CREATE INDEX [IDX_invItems_OID_LID]
ON [invItems](
  [ownerID], 
  [locationID]);

CREATE INDEX [IDX_invTraits_BID] ON [invTraits]([bonusID]);

CREATE INDEX [IDX_invTraits_TID] ON [invTraits]([typeID]);

CREATE UNIQUE INDEX [IDX_invTypeReactions_PK]
ON [invTypeReactions](
  [reactionTypeID], 
  [input], 
  [typeID]);

CREATE INDEX [IDX_invTypes_GID] ON [invTypes]([groupID]);

CREATE INDEX [IDX_invTypes_MGID] ON [invTypes]([marketGroupID]);

CREATE INDEX [IDX_invTypes_TID] ON [invTypes]([typeID]);

CREATE INDEX [IDX_invUniqueNames_GID_IN]
ON [invUniqueNames](
  [groupID], 
  [itemName]);

CREATE UNIQUE INDEX [IDX_invUniqueNames_IN] ON [invUniqueNames]([itemName]);

CREATE UNIQUE INDEX [IDX_mapConstellationJumps_PK]
ON [mapConstellationJumps](
  [fromConstellationID], 
  [toConstellationID]);

CREATE INDEX [IDX_mapConstellations_RID] ON [mapConstellations]([regionID]);

CREATE INDEX [IDX_mapDenormalize_CID] ON [mapDenormalize]([constellationID]);

CREATE INDEX [IDX_mapDenormalize_OID] ON [mapDenormalize]([orbitID]);

CREATE INDEX [IDX_mapDenormalize_RID] ON [mapDenormalize]([regionID]);

CREATE INDEX [IDX_mapDenormalize_SSID] ON [mapDenormalize]([solarSystemID]);

CREATE UNIQUE INDEX [IDX_mapRegionJumps_PK]
ON [mapRegionJumps](
  [fromRegionID], 
  [toRegionID]);

CREATE INDEX [IDX_mapRegions_RID] ON [mapRegions]([regionID]);

CREATE UNIQUE INDEX [IDX_mapSolarSystemJumps_PK]
ON [mapSolarSystemJumps](
  [fromSolarSystemID], 
  [toSolarSystemID]);

CREATE INDEX [IDX_mapSolarSystems_CID] ON [mapSolarSystems]([constellationID]);

CREATE INDEX [IDX_mapSolarSystems_RID] ON [mapSolarSystems]([regionID]);

CREATE INDEX [IDX_mapSolarSystems_SEC] ON [mapSolarSystems]([security]);

CREATE INDEX [IDX_marketGroups_MGID] ON [marketGroups]([marketGroupID]);

CREATE INDEX [IDX_ramAssemblyLineStations_OID]
ON [ramAssemblyLineStations]([ownerID]);

CREATE UNIQUE INDEX [IDX_ramAssemblyLineStations_PK]
ON [ramAssemblyLineStations](
  [stationID], 
  [assemblyLineTypeID]);

CREATE INDEX [IDX_ramAssemblyLineStations_RID]
ON [ramAssemblyLineStations]([regionID]);

CREATE UNIQUE INDEX [IDX_ramAssemblyLineTypeDetailPerCategory_PK]
ON [ramAssemblyLineTypeDetailPerCategory](
  [assemblyLineTypeID], 
  [categoryID]);

CREATE UNIQUE INDEX [IDX_ramAssemblyLineTypeDetailPerGroup_PK]
ON [ramAssemblyLineTypeDetailPerGroup](
  [assemblyLineTypeID], 
  [groupID]);

CREATE UNIQUE INDEX [IDX_ramInstallationTypeContents_PK]
ON [ramInstallationTypeContents](
  [installationTypeID], 
  [assemblyLineTypeID]);

CREATE UNIQUE INDEX [IDX_researchAgents_PK]
ON [researchAgents](
  [agentID], 
  [typeID]);

CREATE INDEX [IDX_researchAgents_TID] ON [researchAgents]([typeID]);

CREATE INDEX [IDX_skins_SID] ON [skins]([skinID]);

CREATE INDEX [IDX_staStations_CID] ON [staStations]([constellationID]);

CREATE INDEX [IDX_staStations_CPID] ON [staStations]([corporationID]);

CREATE INDEX [IDX_staStations_OID] ON [staStations]([operationID]);

CREATE INDEX [IDX_staStations_RID] ON [staStations]([regionID]);

CREATE INDEX [IDX_staStations_SSID] ON [staStations]([solarSystemID]);

CREATE INDEX [IDX_staStations_STID] ON [staStations]([stationTypeID]);

CREATE INDEX [IDX_tntTournamentBannedGroups_RSID]
ON [tntTournamentBannedGroups]([ruleSetID]);

CREATE INDEX [IDX_tntTournamentBannedTypes_RSID]
ON [tntTournamentBannedTypes]([ruleSetID]);

CREATE INDEX [IDX_tntTournamentGroupPoints_RSID]
ON [tntTournamentGroupPoints]([ruleSetID]);

CREATE INDEX [IDX_tntTournamentTypePoints_RSID]
ON [tntTournamentTypePoints]([ruleSetID]);

CREATE UNIQUE INDEX [IDX_trnTranslationColumns_TN_CN_MID]
ON [trnTranslationColumns](
  [tableName], 
  [columnName], 
  [masterID]);

CREATE INDEX [IDX_trnTranslations_TCID_KID_LID]
ON [trnTranslations](
  [tcID], 
  [keyID], 
  [languageID]);

CREATE UNIQUE INDEX [IDX_typeMaterials_PK]
ON [typeMaterials](
  [typeID], 
  [materialTypeID]);

