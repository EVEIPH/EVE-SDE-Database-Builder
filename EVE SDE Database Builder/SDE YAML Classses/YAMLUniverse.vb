
Imports YamlDotNet.Serialization
Imports System.IO
Imports System.Threading

Public Class YAMLUniverse
    Inherits YAMLFilesBase

    Private Const mapCelestialStatistics_Table As String = "mapCelestialStatistics"
    Private Const mapConstellationJumps_Table As String = "mapConstellationJumps"
    Private Const mapConstellations_Table As String = "mapConstellations"
    Private Const mapDenormalize_Table As String = "mapDenormalize"
    Private Const mapJumps_Table As String = "mapJumps"
    Private Const mapLocationScenes_Table As String = "mapLocationScenes"
    Private Const mapLocationWormholeClasses_Table As String = "mapLocationWormholeClasses"
    Private Const mapRegionJumps_Table As String = "mapRegionJumps"
    Private Const mapRegions_Table As String = "mapRegions"
    Private Const mapSolarSystemJumps_Table As String = "mapSolarSystemJumps"
    Private Const mapSolarSystems_Table As String = "mapSolarSystems"
    Private Const mapDisallowedAnchorCategories_Table As String = "mapDisallowedAnchorCategories"
    Private Const mapDisallowedAnchorGroups_Table As String = "mapDisallowedAnchorGroups"
    Private Const mapItemEffectBeacons_Table As String = "mapItemEffectBeacons"

    Public Const regionFile As String = "region.staticdata"
    Public Const constellationFile As String = "constellation.staticdata"
    Public Const solarsystemFile As String = "solarsystem.staticdata"

    Public Const UniverseFiles As String = "Universe Data (multiple files)"

    Private Const invNamesTableName As String = "invNames"

    Private FilePath As String = ""
    Private RecordCount As Integer = 0
    Private ThreadsArray As List(Of Thread) = New List(Of Thread)

    Private invNamesLDB As SQLiteDB ' for searching
    Private MapJumpsData As SQLiteDB = Nothing ' for building the mapJump tables

    Private LocalDBsLocation As String = ""
    Private BaseYAMLPath As String = ""

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
        FilePath = YAMLFilePath

        ' Set the local db location 
        LocalDBsLocation = YAMLFilePath & "UniverseTemp\"

        ' Clean up the directory if there
        If Directory.Exists(LocalDBsLocation) Then
            Call Directory.Delete(LocalDBsLocation, True)
        End If

        BaseYAMLPath = YAMLFilePath

        ' walk back directory until it's the "sde" for later use
        Do Until BaseYAMLPath.Substring(Len(BaseYAMLPath) - 3) = "sde"
            BaseYAMLPath = Directory.GetParent(BaseYAMLPath).FullName
        Loop

        ' Delete anything there
        On Error Resume Next
        Call Directory.Delete(LocalDBsLocation, True)

    End Sub

    ''' <summary>
    ''' Imports the yaml files into the database maps tables 
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = invNamesTableName
        Dim UniverseDirs As New List(Of String)
        Dim RegionDirs As New List(Of String)
        Dim ConstellationDirs As New List(Of String)
        Dim SystemDirs As New List(Of String)

        Dim RegionID As Integer
        Dim ConstellationID As Integer

        ' Build all the tables to insert map data into. This includes the following tables:
        ' - mapCelestialStatistics
        ' - mapConstellationJumps
        ' - mapConstellations
        ' - mapDenormalize
        ' - mapJumps
        ' - mapLocationScenes
        ' - mapLocationWormholeClasses
        ' - mapRegionJumps
        ' - mapRegions
        ' - mapSolarSystemJumps
        ' - mapSolarSystems
        ' - mapDisallowedAnchorCategories * new
        ' - mapDisallowedAnchorGroups * new

        Call CreatemapRegionsTable()
        Call CreatemapConstellationsTable()
        Call CreatemapSolarSystemsTable()

        Call CreatemapCelestialStatisticsTable()
        Call CreatemapDenormalizeTable(UpdateDB)

        Call CreatemapLocationWormholeClasses()
        Call CreatemapLocationScenesTable()

        Call CreatemapJumpsTable(UpdateDB)

        Call CreatemapDisallowedAnchorCategories()
        Call CreatemapDisallowedAnchorGroups()
        Call CreatemapItemEffectBeacons()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Load up invNames in a local database for searching the names of items in the universe files
        Call LoadinvNames(Params.ImportLanguageCode)

        ' Build the maps database
        Call CreateMapDatabase()

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        ' Get total count of files as the total count for updating the progress
        Dim TotalRecords As Integer = Directory.GetFiles(FilePath, "*.staticdata", SearchOption.AllDirectories).Count

        ' Begin looping through the universe folders (eve and wormholes) for regions
        UniverseDirs = New List(Of String)(Directory.EnumerateDirectories(FilePath))

        For Each UniverseFolder In UniverseDirs
            ' For this universe folder, get the regions
            RegionDirs = New List(Of String)(Directory.EnumerateDirectories(UniverseFolder))

            For Each RegionFolder In RegionDirs
                ' Import the region file from this region folder first
                RegionID = ImportRegion(RegionFolder)

                ' Now load the constellation yaml data from this region folder
                ConstellationDirs = New List(Of String)(Directory.EnumerateDirectories(RegionFolder))
                For Each ConstellationFolder In ConstellationDirs
                    ' Import the constellation data
                    ConstellationID = ImportConstellation(ConstellationFolder, RegionID)

                    SystemDirs = New List(Of String)(Directory.EnumerateDirectories(ConstellationFolder))
                    For Each SystemFolder In SystemDirs
                        ' Import the system data, which includes all the planets, moons, celestial statistics, and mapdenormalize data
                        Call ImportSolarSystem(SystemFolder, RegionID, ConstellationID)

                        ' Update grid progress
                        Call UpdateGridRowProgress(Params.RowLocation, RecordCount, TotalRecords)
                        RecordCount += 1 ' Count after so we never get to 100 until finished
                    Next
                    RecordCount += 1
                Next
                RecordCount += 1
            Next
        Next

        ' Build the mapjump tables now that we have all the data imported
        Call CreateMapJumpTables()

        ' Finally, set the indexes (this will speed up inserts)
        Call CreateUniverseIndexes()

        ' Close local dbs
        Call invNamesLDB.CloseDB()
        Call MapJumpsData.CloseDB()

        ' Cleanup local db files
        Call Directory.Delete(LocalDBsLocation, True)

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    ''' <summary>
    ''' Creates the local map database for building the jumps tables after all data is imported
    ''' </summary>
    Private Sub CreateMapDatabase()

        ' Open the database
        MapJumpsData = New SQLiteDB(LocalDBsLocation & "MapJumpsData.sqlite", LocalDBsLocation, True)

        ' Build the tables we need on this db
        Call CreatemapDenormalizeTable(MapJumpsData)
        Call CreatemapJumpsTable(MapJumpsData)

    End Sub

    ''' <summary>
    ''' Creates the mapJumpsTables (mapSolarSystemJumps, mapConstellationJumps, mapRegionJumps)
    ''' </summary>
    Private Sub CreateMapJumpTables()
        Dim WhereClause As New List(Of String)
        Dim Result As New List(Of List(Of Object))
        Dim SelectClause As New List(Of String)
        Dim DataFields As New List(Of DBField)

        ' Create the tables on the new dB
        Call CreatemapRegionJumpsTable()
        Call CreatemapConstellationJumpsTable()
        Call CreatemapSolarSystemJumpsTable()

        Call MapJumpsData.BeginSQLiteTransaction()

        Dim SQL = "CREATE TABLE " & mapSolarSystemJumps_Table & " AS SELECT MD1.regionID AS fromRegionID, MD1.constellationID AS fromConstellationID, MD1.solarSystemID AS fromSolarSystemID, "
        SQL &= "MD2.solarSystemID As toSolarSystemID, MD2.constellationID As toConstellationID, MD2.regionID As toRegionID "
        SQL &= "FROM mapDenormalize AS MD1, mapDenormalize AS MD2, mapJumps WHERE mapJumps.stargateID = MD1.itemID AND MD2.itemID = mapJumps.destinationID"

        ' Do an insert via SQL for each table, the first will be the base to query others
        MapJumpsData.ExecuteNonQuerySQL(SQL)

        SQL = "CREATE TABLE " & mapConstellationJumps_Table & " AS SELECT fromRegionID, fromConstellationID, toConstellationID, toRegionID FROM mapSolarSystemJumps "
        SQL &= "GROUP BY fromConstellationID, toConstellationID, fromRegionID, toRegionID"

        MapJumpsData.ExecuteNonQuerySQL(SQL)

        SQL = "CREATE TABLE " & mapRegionJumps_Table & " AS SELECT fromRegionID, toRegionID FROM mapSolarSystemJumps GROUP BY fromRegionID, toRegionID"
        MapJumpsData.ExecuteNonQuerySQL(SQL)

        ' Now select the data from the three new tables and insert into the current DB
        ' mapSolarSystemJumps
        SelectClause = New List(Of String)
        WhereClause = New List(Of String)
        SelectClause.Add("fromRegionID")
        SelectClause.Add("fromConstellationID")
        SelectClause.Add("fromSolarSystemID")
        SelectClause.Add("toSolarSystemID")
        SelectClause.Add("toConstellationID")
        SelectClause.Add("toRegionID")

        Result = MapJumpsData.SelectfromTable(SelectClause, mapSolarSystemJumps_Table, WhereClause)

        ' Insert each row
        For i = 0 To Result.Count - 1
            DataFields = New List(Of DBField)
            ' Build each record
            DataFields.Add(UpdateDB.BuildDatabaseField("fromRegionID", Result(i)(0), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fromConstellationID", Result(i)(1), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fromSolarSystemID", Result(i)(2), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("toSolarSystemID", Result(i)(3), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("toConstellationID", Result(i)(4), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("toRegionID", Result(i)(5), FieldType.int_type))

            Call UpdateDB.InsertRecord(mapSolarSystemJumps_Table, DataFields)

        Next

        ' mapConstellationJumps
        SelectClause = New List(Of String)
        WhereClause = New List(Of String)
        SelectClause.Add("fromRegionID")
        SelectClause.Add("fromConstellationID")
        SelectClause.Add("toConstellationID")
        SelectClause.Add("toRegionID")

        Result = MapJumpsData.SelectfromTable(SelectClause, mapConstellationJumps_Table, WhereClause)

        ' Insert each row
        For i = 0 To Result.Count - 1
            DataFields = New List(Of DBField)
            ' Build each record
            DataFields.Add(UpdateDB.BuildDatabaseField("fromRegionID", Result(i)(0), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fromConstellationID", Result(i)(1), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("toConstellationID", Result(i)(2), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("toRegionID", Result(i)(3), FieldType.int_type))

            Call UpdateDB.InsertRecord(mapConstellationJumps_Table, DataFields)

        Next

        ' mapRegionJumps
        SelectClause = New List(Of String)
        WhereClause = New List(Of String)
        SelectClause.Add("fromRegionID")
        SelectClause.Add("toRegionID")

        Result = MapJumpsData.SelectfromTable(SelectClause, mapRegionJumps_Table, WhereClause)

        ' Insert each row
        For i = 0 To Result.Count - 1
            DataFields = New List(Of DBField)
            ' Build each record
            DataFields.Add(UpdateDB.BuildDatabaseField("fromRegionID", Result(i)(0), FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("toRegionID", Result(i)(1), FieldType.int_type))

            Call UpdateDB.InsertRecord(mapRegionJumps_Table, DataFields)

        Next

        Call MapJumpsData.CommitSQLiteTransaction()

    End Sub

    ''' <summary>
    ''' Import the region file and return region ID to use for later inserts
    ''' </summary>
    ''' <param name="DirectoryPath">Directory path with the Region file</param>
    ''' <returns>Region ID as integer</returns>
    Private Function ImportRegion(ByVal DirectoryPath As String) As Integer
        FileNameErrorTracker = "ImportRegion"
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If

        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        Dim NameLookup As New ESI
        DS = DSB.Build

        Dim YAMLRecord As New region
        Dim DataFields As New List(Of DBField)

        Try
            ' Parse the input text
            YAMLRecord = DS.Deserialize(Of region)(New StringReader(File.ReadAllText(DirectoryPath & "\" & regionFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        Dim RegionName As String = New DirectoryInfo(DirectoryPath).Name

        ' Insert the region
        With YAMLRecord
            DataFields.Add(UpdateDB.BuildDatabaseField("regionID", .regionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("regionName", GetItemName(.regionID, RegionName), FieldType.varchar_type)) ' Get the name of the region from the folder
            DataFields.Add(UpdateDB.BuildDatabaseField("x", .center(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", .center(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", .center(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Min", .min(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Max", .max(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Min", .min(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Max", .max(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Min", .min(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Max", .max(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("nameID", .nameID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("descriptionID", .descriptionID, FieldType.int_type))

            If Not IsNothing(.nebula) Then
                Call SavemapLocationScene(.regionID, .nebula)
            End If

            If Not IsNothing(.wormholeClassID) Then
                Call SavemapLocationWormholeClass(.regionID, .wormholeClassID)
            End If

        End With

        Call UpdateDB.InsertRecord(mapRegions_Table, DataFields)

        Return YAMLRecord.regionID

    End Function

    ''' <summary>
    ''' Import the constellation file and return constellation ID to use for later inserts
    ''' </summary>
    ''' <param name="DirectoryPath">Directory path with the Region file</param>
    ''' <param name="ConstellationRegionID">RegionID for this constellation</param>
    ''' <returns>Contellation ID as integer</returns>
    Private Function ImportConstellation(ByVal DirectoryPath As String, ConstellationRegionID As Integer) As Integer
        FileNameErrorTracker = "ImportConstellation"
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build
        Dim NameLookup As New ESI

        Dim YAMLRecord As New constellation
        Dim DataFields As New List(Of DBField)

        Try
            ' Parse the input text
            YAMLRecord = DS.Deserialize(Of constellation)(New StringReader(File.ReadAllText(DirectoryPath & "\" & constellationFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        Dim ConstellationName As String = New DirectoryInfo(DirectoryPath).Name

        ' Insert the constellation
        With YAMLRecord
            DataFields.Add(UpdateDB.BuildDatabaseField("constellationID", .constellationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("constellationName", GetItemName(.constellationID, ConstellationName), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("regionID", ConstellationRegionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x", .center(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", .center(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", .center(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Min", .min(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Max", .max(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Min", .min(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Max", .max(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Min", .min(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Max", .max(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))

            If Not IsNothing(.wormholeClassID) Then
                Call SavemapLocationWormholeClass(.constellationID, .wormholeClassID)
            End If

        End With

        Call UpdateDB.InsertRecord(mapConstellations_Table, DataFields)

        Return YAMLRecord.constellationID

    End Function

    ''' <summary>
    ''' Import the Solar System file and return Solary System ID to use for later inserts
    ''' </summary>
    ''' <param name="DirectoryPath">Directory path with the Region file</param>
    ''' <param name="SystemRegionID">Region ID for this System</param>
    ''' <param name="SystemConstellationID">Contellation ID for this System</param>
    Private Sub ImportSolarSystem(ByVal DirectoryPath As String, ByVal SystemRegionID As Integer, ByVal SystemConstellationID As Integer)
        FileNameErrorTracker = "ImportSolarSystem"
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build
        Dim NameLookup As New ESI

        Dim YAMLRecord As New solarSystem
        Dim DataFields As New List(Of DBField)
        Dim i As Integer = 0

        Try
            ' Parse the input text
            YAMLRecord = DS.Deserialize(Of solarSystem)(New StringReader(File.ReadAllText(DirectoryPath & "\" & solarsystemFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        Dim SolarSystemName As String = New DirectoryInfo(DirectoryPath).Name

        ' Insert the system
        With YAMLRecord
            DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemName", GetItemName(.solarSystemID, SolarSystemName), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("regionID", SystemRegionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("constellationID", SystemConstellationID, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x", .center(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", .center(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", .center(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Min", .min(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Max", .max(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Min", .min(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Max", .max(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Min", .min(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Max", .max(2), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("luminosity", .luminosity, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("border", .border, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fringe", .fringe, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("corridor", .corridor, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("hub", .hub, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("international", .international, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("regional", .regional, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("security", .security, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("sunTypeID", .sunTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("securityClass", .securityClass, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemNameID", .solarSystemNameID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("visualEffect", .visualEffect, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("descriptionID", .descriptionID, FieldType.int_type))

            Call UpdateDB.InsertRecord(mapSolarSystems_Table, DataFields)

            If Not IsNothing(.wormholeClassID) Then
                Call SavemapLocationWormholeClass(.solarSystemID, .wormholeClassID)
            End If

            ' Insert jumps data for stargates
            If Not IsNothing(.stargates) Then
                For Each Gate In .stargates
                    DataFields = New List(Of DBField)

                    ' Simple insert into mapJumps
                    DataFields.Add(UpdateDB.BuildDatabaseField("stargateID", Gate.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("destinationID", Gate.Value.destination, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("typeID", Gate.Value.typeID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("x", Gate.Value.position(0), FieldType.real_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("y", Gate.Value.position(1), FieldType.real_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("z", Gate.Value.position(2), FieldType.real_type))

                    Call UpdateDB.InsertRecord(mapJumps_Table, DataFields)

                    ' Also add to the local db too
                    If Not IsNothing(MapJumpsData) Then
                        Call MapJumpsData.InsertRecord(mapJumps_Table, DataFields)
                    End If

                    ' Add the others stargate data to map denormalize
                    Call SavemapDenormalizeItem(Gate.Key, Gate.Value.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                                Nothing, Gate.Value.position, Nothing, Nothing, YAMLRecord.security, Nothing, Nothing)

                Next
            End If

            ' Save the star data and map data from it
            If Not IsNothing(.star) Then
                ' All positions are at 0,0,0 for stars
                Dim TempPostion As New List(Of Object)
                TempPostion.Add(0)
                TempPostion.Add(0)
                TempPostion.Add(0)
                Call SavemapDenormalizeItem(.star.id, .star.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                                Nothing, TempPostion, .star.radius, Nothing, YAMLRecord.security, Nothing, Nothing)

                Call SaveCelestialStatistics(.star.id, .star.statistics, Nothing)

            End If

            If Not IsNothing(.secondarySun) Then

                Call SavemapDenormalizeItem(.secondarySun.itemID, .secondarySun.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                                Nothing, Nothing, Nothing, Nothing, YAMLRecord.security, Nothing, Nothing)

                ' Save the effectBeaconTypeID in new table
                DataFields = New List(Of DBField)
                DataFields.Add(UpdateDB.BuildDatabaseField("itemID", .secondarySun.itemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("effectBeaconTypeID", .secondarySun.effectBeaconTypeID, FieldType.int_type))

                Call UpdateDB.InsertRecord(mapItemEffectBeacons_Table, DataFields)

            End If

            ' Import all the planets, moons, asteroid belts, and stations
            For Each ssPlanet In .planets

                Call SavemapDenormalizeItem(ssPlanet.Key, ssPlanet.Value.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                            .star.id, ssPlanet.Value.position, ssPlanet.Value.radius, ssPlanet.Value.planetNameID,
                                            YAMLRecord.security, ssPlanet.Value.celestialIndex, Nothing)

                Call SaveCelestialStatistics(ssPlanet.Key, ssPlanet.Value.statistics, ssPlanet.Value.planetAttributes)

                If Not IsNothing(ssPlanet.Value.moons) Then
                    i = 1
                    For Each planetMoon In ssPlanet.Value.moons

                        Call SavemapDenormalizeItem(planetMoon.Key, planetMoon.Value.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                            ssPlanet.Key, planetMoon.Value.position, planetMoon.Value.radius, planetMoon.Value.moonNameID,
                                            YAMLRecord.security, ssPlanet.Value.celestialIndex, i)

                        Call SaveCelestialStatistics(planetMoon.Key, planetMoon.Value.statistics, planetMoon.Value.planetAttributes)

                        ' Insert station location data on a moon (should be duplicated in staStations as well though)
                        If Not IsNothing(planetMoon.Value.npcStations) Then
                            For Each Station In planetMoon.Value.npcStations

                                Call SavemapDenormalizeItem(Station.Key, Station.Value.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                    planetMoon.Key, Station.Value.position, Nothing, Nothing,
                                    YAMLRecord.security, ssPlanet.Value.celestialIndex, Nothing)
                            Next
                        End If
                        i += 1
                    Next
                End If

                If Not IsNothing(ssPlanet.Value.asteroidBelts) Then
                    i = 1
                    For Each Belt In ssPlanet.Value.asteroidBelts

                        Call SavemapDenormalizeItem(Belt.Key, Belt.Value.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                                ssPlanet.Key, Belt.Value.position, Nothing, Belt.Value.asteroidBeltNameID,
                                                YAMLRecord.security, ssPlanet.Value.celestialIndex, i)

                        Call SaveCelestialStatistics(Belt.Key, Belt.Value.statistics, Nothing)
                        i += 1
                    Next
                End If

                ' Insert station location data on a planet (should be duplicated in staStations as well though)
                If Not IsNothing(ssPlanet.Value.npcStations) Then
                    For Each Station In ssPlanet.Value.npcStations

                        Call SavemapDenormalizeItem(Station.Key, Station.Value.typeID, YAMLRecord.solarSystemID, SystemConstellationID, SystemRegionID,
                                                ssPlanet.Key, Station.Value.position, Nothing, Nothing,
                                                YAMLRecord.security, ssPlanet.Value.celestialIndex, Nothing)
                    Next
                End If
            Next

            If Not IsNothing(.disallowedAnchorCategories) Then
                For Each CategoryID In .disallowedAnchorCategories
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", YAMLRecord.solarSystemID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("categoryID", CategoryID, FieldType.int_type))

                    Call UpdateDB.InsertRecord(mapDisallowedAnchorCategories_Table, DataFields)
                Next
            End If

            If Not IsNothing(.disallowedAnchorGroups) Then
                For Each GroupID In .disallowedAnchorCategories
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", YAMLRecord.solarSystemID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("groupID", GroupID, FieldType.int_type))

                    Call UpdateDB.InsertRecord(mapDisallowedAnchorGroups_Table, DataFields)
                Next
            End If

        End With

    End Sub

    ''' <summary>
    ''' Saves the sent Celestial Statistics to the mapCelestialStatistics table
    ''' </summary>
    ''' <param name="CelestialID">ID of the Celestial object</param>
    ''' <param name="Stats">Celestial statistics to insert</param>
    ''' <param name="Attributes">Attributes of the planet object - can be nothing</param>
    Private Sub SaveCelestialStatistics(ByVal CelestialID As Integer, ByVal Stats As solarSystem.celestialStatistics, ByVal Attributes As solarSystem.planet.objectAttributes)
        Dim DataFields As New List(Of DBField)

        If IsNothing(Attributes) Then
            Attributes = New solarSystem.planet.objectAttributes
        End If

        ' Build the insert list

        If Not IsNothing(Stats) And Not IsNothing(Attributes) Then
            With Stats
                ' Must have this field and either stats or attributes to insert a record
                DataFields.Add(UpdateDB.BuildDatabaseField("celestialID", CelestialID, FieldType.int_type))
                If Not IsNothing(Stats) Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("temperature", .temperature, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("spectralClass", .spectralClass, FieldType.varchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("luminosity", .luminosity, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("age", .age, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("life", .life, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("orbitRadius", .orbitRadius, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("eccentricity", .eccentricity, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("massDust", .massDust, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("massGas", .massGas, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("fragmented", .fragmented, FieldType.bit_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("density", .density, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("surfaceGravity", .surfaceGravity, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("escapeVelocity", .escapeVelocity, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("orbitPeriod", .orbitPeriod, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("rotationRate", .rotationRate, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("locked", .locked, FieldType.bit_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("pressure", .pressure, FieldType.float_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.float_type))
                Else
                    DataFields.Add(New DBField("temperature", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("spectralClass", NullValue, FieldType.varchar_type))
                    DataFields.Add(New DBField("luminosity", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("age", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("life", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("orbitRadius", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("eccentricity", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("massDust", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("massGas", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("fragmented", NullValue, FieldType.bit_type))
                    DataFields.Add(New DBField("density", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("surfaceGravity", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("escapeVelocity", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("orbitPeriod", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("rotationRate", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("locked", NullValue, FieldType.bit_type))
                    DataFields.Add(New DBField("pressure", NullValue, FieldType.float_type))
                    DataFields.Add(New DBField("radius", NullValue, FieldType.float_type))
                End If
                If Not IsNothing(Attributes) Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("heightMap1", Attributes.heightMap1, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("heightMap2", Attributes.heightMap2, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("population", Attributes.population, FieldType.bit_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("shaderPreset", Attributes.shaderPreset, FieldType.int_type))
                Else
                    DataFields.Add(New DBField("heightMap1", NullValue, FieldType.int_type))
                    DataFields.Add(New DBField("heightMap2", NullValue, FieldType.int_type))
                    DataFields.Add(New DBField("population", NullValue, FieldType.int_type))
                    DataFields.Add(New DBField("shaderPreset", NullValue, FieldType.int_type))
                End If
            End With

            Call UpdateDB.InsertRecord(mapCelestialStatistics_Table, DataFields)

        End If

    End Sub

    ''' <summary>
    ''' Saves the location scene information in the mapLocationScenes Table
    ''' </summary>
    ''' <param name="LocationID"></param>
    ''' <param name="NebulaID"></param>
    Private Sub SavemapLocationScene(ByVal LocationID As Integer, NebulaID As Integer)
        Dim DataFields As New List(Of DBField)

        ' Build the insert list
        DataFields.Add(UpdateDB.BuildDatabaseField("locationID", LocationID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", NebulaID, FieldType.int_type))

        Call UpdateDB.InsertRecord(mapLocationScenes_Table, DataFields)

    End Sub

    ''' <summary>
    ''' Saves the location data for a wormhole class in the mapLocationWormholeClasses Table
    ''' </summary>
    ''' <param name="LocationID"></param>
    ''' <param name="WormholeClassID"></param>
    Private Sub SavemapLocationWormholeClass(ByVal LocationID As Integer, WormholeClassID As Integer)
        Dim DataFields As New List(Of DBField)

        ' Build the insert list
        DataFields.Add(UpdateDB.BuildDatabaseField("locationID", LocationID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("wormholeClassID", WormholeClassID, FieldType.int_type))

        Call UpdateDB.InsertRecord(mapLocationWormholeClasses_Table, DataFields)

    End Sub

    ''' <summary>
    ''' Saves the sent item in the mapDenormalize table
    ''' </summary>
    ''' <param name="itemID"></param>
    ''' <param name="typeID"></param>
    ''' <param name="solarSystemID"></param>
    ''' <param name="constellationID"></param>
    ''' <param name="regionID"></param>
    ''' <param name="orbitID"></param>
    ''' <param name="position"></param>
    ''' <param name="radius"></param>
    ''' <param name="nameID"></param>
    ''' <param name="security"></param>
    ''' <param name="celestialIndex"></param>
    ''' <param name="orbitIndex"></param>
    Private Sub SavemapDenormalizeItem(ByVal itemID As Object, ByVal typeID As Object, ByVal solarSystemID As Object, ByVal constellationID As Object, ByVal regionID As Object,
                                      ByVal orbitID As Object, ByVal position As List(Of Object), ByVal radius As Object, ByVal nameID As Object,
                                      ByVal security As Object, ByVal celestialIndex As Object, ByVal orbitIndex As Object)
        Dim DataFields As New List(Of DBField)
        Dim NameLookup As New ESI

        DataFields.Add(UpdateDB.BuildDatabaseField("itemID", itemID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", typeID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", solarSystemID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("constellationID", constellationID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("regionID", regionID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("orbitID", orbitID, FieldType.int_type))
        If Not IsNothing(position) Then
            DataFields.Add(UpdateDB.BuildDatabaseField("x", position(0), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", position(1), FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", position(2), FieldType.real_type))
        Else
            DataFields.Add(UpdateDB.BuildDatabaseField("x", Nothing, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", Nothing, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", Nothing, FieldType.real_type))
        End If
        DataFields.Add(UpdateDB.BuildDatabaseField("radius", radius, FieldType.real_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("nameID", nameID, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("security", security, FieldType.real_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("celestialIndex", celestialIndex, FieldType.int_type))
        DataFields.Add(UpdateDB.BuildDatabaseField("orbitIndex", orbitIndex, FieldType.int_type))

        Call UpdateDB.InsertRecord(mapDenormalize_Table, DataFields)

        ' Also add to the local db too
        If Not IsNothing(MapJumpsData) Then
            Call MapJumpsData.InsertRecord(mapDenormalize_Table, DataFields)
        End If

    End Sub

    ''' <summary>
    ''' Creates the mapCelestialStatistics Table
    ''' </summary>
    Private Sub CreatemapCelestialStatisticsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("celestialID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("temperature", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("spectralClass", FieldType.varchar_type, 10, True))
        Table.Add(New DBTableField("luminosity", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("age", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("life", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("orbitRadius", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("eccentricity", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("massDust", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("massGas", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("fragmented", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("density", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("surfaceGravity", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("escapeVelocity", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("orbitPeriod", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("rotationRate", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("locked", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("pressure", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("radius", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("heightMap1", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("heightMap2", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("population", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("shaderPreset", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapCelestialStatistics_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapConstellationJumps Table
    ''' </summary>
    Private Sub CreatemapConstellationJumpsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("fromRegionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("fromConstellationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("toConstellationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("toRegionID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapConstellationJumps_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapConstellations Table
    ''' </summary>
    Private Sub CreatemapConstellationsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("constellationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("constellationName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("regionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("x_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("x_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("radius", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapConstellations_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapDenormalize Table on the referenced DB
    ''' <param name="ReferenceDB">DB to create the table on</param>
    ''' </summary>
    Private Sub CreatemapDenormalizeTable(ByRef ReferenceDB As Object)
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("itemID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        'Table.Add(New DBTableField("groupID", FieldType.int_type, 0, True)) ' Too hard to update from YAML (not in SS file) - just leave blank, people can get this from invGroups if they want it
        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("constellationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("regionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("orbitID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("radius", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("nameID", FieldType.int_type, 0, True))
        'Table.Add(New DBTableField("itemName", FieldType.varchar_type, 100, True)) ' people can link to invNames if they want this, it kills efficiency and it's not in the yaml file data
        Table.Add(New DBTableField("security", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("celestialIndex", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("orbitIndex", FieldType.int_type, 0, True))

        Call ReferenceDB.CreateTable(mapDenormalize_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapDisallowedAnchorCategories Table
    ''' </summary>
    Private Sub CreatemapDisallowedAnchorCategories()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("categoryID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapDisallowedAnchorCategories_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapDisallowedAnchorGroups Table
    ''' </summary>
    Private Sub CreatemapDisallowedAnchorGroups()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapDisallowedAnchorGroups_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapItemEffectBeacons Table
    ''' </summary>
    Private Sub CreatemapItemEffectBeacons()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("itemID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("effectBeaconTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapItemEffectBeacons_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapJumps Table
    ''' <param name="ReferenceDB">DB to create the table on</param>
    ''' </summary>
    Private Sub CreatemapJumpsTable(ByRef ReferenceDB As Object)
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("stargateID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("destinationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))

        Call ReferenceDB.CreateTable(mapJumps_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapLocationWormholeClasses Table
    ''' </summary>
    Private Sub CreatemapLocationWormholeClasses()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("locationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("wormholeClassID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapLocationWormholeClasses_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapLocationScenes Table
    ''' </summary>
    Private Sub CreatemapLocationScenesTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("locationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True)) ' Nebula field in the yaml

        Call UpdateDB.CreateTable(mapLocationScenes_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapRegionJumps Table
    ''' </summary>
    Private Sub CreatemapRegionJumpsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("fromRegionID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("toRegionID", FieldType.int_type, 0, False, True))

        Call UpdateDB.CreateTable(mapRegionJumps_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapRegions Table
    ''' </summary>
    Private Sub CreatemapRegionsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("regionID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("regionName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("x_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("x_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("nameID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("descriptionID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapRegions_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapSolarSystemJumps Table
    ''' </summary>
    Private Sub CreatemapSolarSystemJumpsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("fromRegionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("fromConstellationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("fromSolarSystemID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("toSolarSystemID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("toConstellationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("toRegionID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapSolarSystemJumps_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates the mapSolarSystems Table
    ''' </summary>
    Private Sub CreatemapSolarSystemsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("solarSystemName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("regionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("constellationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("x_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("x_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z_Min", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z_Max", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("luminosity", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("border", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("corridor", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("fringe", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("hub", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("international", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("regional", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("security", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("radius", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("sunTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("securityClass", FieldType.varchar_type, 2, True))
        Table.Add(New DBTableField("solarSystemNameID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("visualEffect", FieldType.varchar_type, 50, True))
        Table.Add(New DBTableField("descriptionID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(mapSolarSystems_Table, Table)

    End Sub

    ''' <summary>
    ''' Creates indexes for the following tables
    ''' mapRegions
    ''' mapConstellations
    ''' mapSolarSystems
    ''' mapDenormalize
    ''' </summary>
    Private Sub CreateUniverseIndexes()
        Dim IndexFields As New List(Of String)

        ' Regions
        IndexFields = New List(Of String)
        IndexFields.Add("regionID")
        Call UpdateDB.CreateIndex(mapRegions_Table, "IDX_" & mapRegions_Table & "_RID", IndexFields)

        ' Constellations
        IndexFields = New List(Of String)
        IndexFields.Add("regionID")
        Call UpdateDB.CreateIndex(mapConstellations_Table, "IDX_" & mapConstellations_Table & "_RID", IndexFields)

        ' Solar Systems
        IndexFields = New List(Of String)
        IndexFields.Add("regionID")
        Call UpdateDB.CreateIndex(mapSolarSystems_Table, "IDX_" & mapSolarSystems_Table & "_RID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("constellationID")
        Call UpdateDB.CreateIndex(mapSolarSystems_Table, "IDX_" & mapSolarSystems_Table & "_CID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("security")
        Call UpdateDB.CreateIndex(mapSolarSystems_Table, "IDX_" & mapSolarSystems_Table & "_SEC", IndexFields)

        ' Map Denormalize
        IndexFields = New List(Of String)
        IndexFields.Add("orbitID")
        Call UpdateDB.CreateIndex(mapDenormalize_Table, "IDX_" & mapDenormalize_Table & "_OID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("solarSystemID")
        Call UpdateDB.CreateIndex(mapDenormalize_Table, "IDX_" & mapDenormalize_Table & "_SSID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("constellationID")
        Call UpdateDB.CreateIndex(mapDenormalize_Table, "IDX_" & mapDenormalize_Table & "_CID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("regionID")
        Call UpdateDB.CreateIndex(mapDenormalize_Table, "IDX_" & mapDenormalize_Table & "_RID", IndexFields)

    End Sub

    Public Sub Close()
        Call invNamesLDB.CloseDB()
        invNamesLDB = Nothing
        Call MapJumpsData.CloseDB()
        MapJumpsData = Nothing
        Call Directory.Delete(LocalDBsLocation, True)
    End Sub

#Region "Local Database"

    ''' <summary>
    ''' Loads a local database with the invNames data for searching item names in the Universe files
    ''' </summary>
    ''' <param name="LangCode">Language to import the data in (not used for invNames)</param>
    Private Sub LoadinvNames(ByVal LangCode As String)
        Dim invNamesData As New List(Of invName)
        Dim InvNames As New YAMLinvNames(YAMLinvNames.invNamesFile, BaseYAMLPath & "\bsd\", Nothing, Nothing)
        Dim ImportParams As ImportParameters
        Dim DataFields As List(Of DBField)

        ImportParams.ImportLanguageCode = LangCode
        ImportParams.InsertRecords = False
        ImportParams.ReturnList = True
        ImportParams.RowLocation = 0

        invNamesData = InvNames.ImportFile(ImportParams)
        invNamesLDB = New SQLiteDB(LocalDBsLocation & "invNames.sqlite", LocalDBsLocation, True)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("itemID", FieldType.bigint_type, 0, False, True))
        Table.Add(New DBTableField("itemName", FieldType.nvarchar_type, 200, True))

        Call invNamesLDB.CreateTable(invNamesTableName, Table)

        invNamesLDB.BeginSQLiteTransaction()

        For Each DataRecord In invNamesData
            DataFields = New List(Of DBField)

            ' Simple insert into local table
            DataFields.Add(invNamesLDB.BuildDatabaseField("itemID", DataRecord.itemID, FieldType.bigint_type))
            DataFields.Add(invNamesLDB.BuildDatabaseField("itemName", DataRecord.itemName, FieldType.nvarchar_type))

            Call invNamesLDB.InsertRecord(invNamesTableName, DataFields)
        Next

        invNamesLDB.CommitSQLiteTransaction()

        Application.DoEvents()

    End Sub

    ''' <summary>
    ''' Returns the string name for the itemID sent
    ''' </summary>
    ''' <param name="ItemID">integer of ID to look up name in invNames table</param>
    ''' <returns></returns>
    Private Function GetItemName(ByVal ItemID As Integer, ByVal FolderName As String) As String
        Dim WhereClause As List(Of String)
        Dim Result As New List(Of List(Of Object))
        Dim TempResult As String = ""

        ' See if the record is there
        WhereClause = New List(Of String)
        WhereClause.Add("itemID =" & ItemID)
        Dim SelectClause As New List(Of String)
        SelectClause.Add("itemName")

        Result = invNamesLDB.SelectfromTable(SelectClause, invNamesTableName, WhereClause)

        If Result.Count = 0 Then
            ' Run an ESI check
            Dim ItemNameESICheck As New ESI
            Dim ItemName = ItemNameESICheck.GetItemName(ItemID)

            If ItemName = "" Then
                ' Return the folder name as a last resort
                ItemName = FolderName
            End If
            Return ItemName
        Else
            Return CStr(Result(0)(0))
        End If

    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
#End Region

End Class

Public Class region
    Public Property regionID As Object
    Public Property center As List(Of Object)
    Public Property max As List(Of Object)
    Public Property min As List(Of Object)
    Public Property factionID As Object
    Public Property nameID As Object
    Public Property descriptionID As Object
    Public Property nebula As Object
    Public Property wormholeClassID As Object
End Class

Public Class constellation
    Public Property constellationID As Object
    Public Property center As List(Of Object)
    Public Property max As List(Of Object)
    Public Property min As List(Of Object)
    Public Property nameID As Object
    Public Property factionID As Object
    Public Property radius As Object
    Public Property wormholeClassID As Object
End Class

Public Class solarSystem
    Public Property solarSystemID As Object
    Public Property center As List(Of Object)
    Public Property max As List(Of Object)
    Public Property min As List(Of Object)
    Public Property luminosity As Object
    Public Property border As Object
    Public Property corridor As Object
    Public Property fringe As Object
    Public Property hub As Object
    Public Property international As Object
    Public Property regional As Object
    Public Property security As Object
    Public Property factionID As Object
    Public Property radius As Object
    Public Property sunTypeID As Object
    Public Property securityClass As Object

    Public Property solarSystemNameID As Object
    Public Property visualEffect As Object
    Public Property descriptionID As Object
    Public Property wormholeClassID As Object

    Public Property star As systemStar
    Public Property secondarySun As secondaryStar
    Public Property stargates As Dictionary(Of Long, stargate)
    Public Property planets As Dictionary(Of Long, planet)
    Public Property disallowedAnchorCategories As List(Of Object)
    Public Property disallowedAnchorGroups As List(Of Object)

    Public Class planet
        Public Property asteroidBelts As Dictionary(Of Long, asteroidBelt)
        Public Property celestialIndex As Object
        Public Property moons As Dictionary(Of Long, moon)
        Public Property npcStations As Dictionary(Of Long, NPCStation)
        Public Property planetAttributes As objectAttributes
        Public Property planetNameID As Object
        Public Property position As List(Of Object)
        Public Property statistics As celestialStatistics
        Public Property radius As Object
        Public Property typeID As Object

        Public Class asteroidBelt
            Public Property position As List(Of Object)
            Public Property typeID As Object
            Public Property statistics As celestialStatistics
            Public Property asteroidBeltNameID As Object
        End Class

        Public Class moon
            Public Property planetAttributes As objectAttributes
            Public Property position As List(Of Object)
            Public Property npcStations As Dictionary(Of Long, NPCStation)
            Public Property radius As Object
            Public Property statistics As celestialStatistics
            Public Property typeID As Object
            Public Property moonNameID As Object

        End Class

        Public Class objectAttributes
            Public Property heightMap1 As Object
            Public Property heightMap2 As Object
            Public Property population As Object
            Public Property shaderPreset As Object
        End Class

        ' Not used?
        Public Class NPCStation
            Public Property graphicID As Object
            Public Property isConquerable As Object
            Public Property operationID As Object
            Public Property ownerID As Object
            Public Property position As List(Of Object)
            Public Property reprocessingEfficiency As Object
            Public Property reprocessingHangarFlag As Object
            Public Property reprocessingStationsTake As Object
            Public Property typeID As Object
            Public Property useOperationName As Object
        End Class

    End Class

    Public Class systemStar
        Public Property id As Object
        Public Property radius As Object
        Public Property statistics As celestialStatistics
        Public Property typeID As Object
    End Class

    Public Class secondaryStar
        Public Property effectBeaconTypeID As Object ' Add to dgmTypeEffects
        Public Property itemID As Object
        Public Property position As List(Of Object)
        Public Property typeID As Object
    End Class

    Public Class stargate
        Public Property destination As Object
        Public Property position As List(Of Object)
        Public Property typeID As Object
    End Class

    ' For stars, asteroids, moons, and planets
    Public Class celestialStatistics
        Public Property age As Object
        Public Property density As Object
        Public Property eccentricity As Object
        Public Property escapeVelocity As Object
        Public Property fragmented As Object
        Public Property life As Object
        Public Property locked As Object
        Public Property luminosity As Object
        Public Property massDust As Object
        Public Property massGas As Object
        Public Property orbitPeriod As Object
        Public Property orbitRadius As Object
        Public Property pressure As Object
        Public Property radius As Object
        Public Property rotationRate As Object
        Public Property spectralClass As Object
        Public Property surfaceGravity As Object
        Public Property temperature As Object
    End Class

End Class