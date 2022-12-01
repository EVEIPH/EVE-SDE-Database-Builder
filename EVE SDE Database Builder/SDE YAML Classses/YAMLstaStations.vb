
Imports YamlDotNet.Serialization
Imports System.IO
Imports Newtonsoft.Json

Public Class YAMLstaStations
    Inherits YAMLFilesBase

    Public Const staStationsFile As String = "staStations.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of staStation)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' ESI
        Dim StructureData As String = ""
        Dim StationOutput As ESIStationData

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("stationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("security", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("dockingCostPerVolume", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("maxShipVolumeDockable", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("officeRentalCost", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("operationID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("stationTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("corporationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("constellationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("regionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("stationName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("reprocessingEfficiency", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("reprocessingStationsTake", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("reprocessingHangarFlag", FieldType.tinyint_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("regionID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_RID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("solarSystemID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SSID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("constellationID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("operationID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_OID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("stationTypeID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_STID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("corporationID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CPID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of staStation))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("stationID", DataField.stationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("[security]", DataField.[security], FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockingCostPerVolume", DataField.dockingCostPerVolume, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("maxShipVolumeDockable", DataField.maxShipVolumeDockable, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("officeRentalCost", DataField.officeRentalCost, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.operationID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("stationTypeID", DataField.stationTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", DataField.corporationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", DataField.solarSystemID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("constellationID", DataField.constellationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("regionID", DataField.regionID, FieldType.int_type))
            If DataField.stationName = "ToBeUpdated" Then
                Dim NameCheckESI As New ESI
                StructureData = NameCheckESI.GetPublicESIData("https://esi.evetech.net/latest/universe/stations/" & DataField.stationID & "/?datasource=tranquility", Nothing)
                If IsNothing(StructureData) Then
                    DataField.stationName = "Unknown Station Name"
                Else
                    ' Parse and pull name
                    RetryCall = False
                    StationOutput = JsonConvert.DeserializeObject(Of ESIStationData)(StructureData)
                    DataField.stationName = StationOutput.stationName
                End If
            End If
            DataFields.Add(UpdateDB.BuildDatabaseField("stationName", DataField.stationName, FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x", DataField.x, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", DataField.y, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", DataField.z, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingEfficiency", DataField.reprocessingEfficiency, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingStationsTake", DataField.reprocessingStationsTake, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingHangarFlag", DataField.reprocessingHangarFlag, FieldType.tinyint_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class ESIStationData
    <JsonProperty("max_dockable_ship_volume")> Public max_dockable_ship_volume As Double
    <JsonProperty("name")> Public stationName As String
    <JsonProperty("office_rental_cost")> Public office_rental_cost As Double
    <JsonProperty("owner")> Public owner As Integer
    <JsonProperty("position")> Public position As ESIPosition
    <JsonProperty("race_id")> Public race_id As Integer
    <JsonProperty("reprocessing_efficiency")> Public reprocessing_efficiency As Double
    <JsonProperty("reprocessing_stations_take")> Public reprocessing_stations_take As Double
    <JsonProperty("services")> Public services As List(Of String)
    <JsonProperty("station_id")> Public station_id As Integer
    <JsonProperty("system_id")> Public system_id As Integer
    <JsonProperty("type_id")> Public type_id As Integer
End Class

Public Class ESIPosition
    <JsonProperty("x")> Public x As Double
    <JsonProperty("y")> Public y As Double
    <JsonProperty("z")> Public z As Double
End Class

Public Class staStation
    Public Property stationID As Object
    Public Property security As Object
    Public Property dockingCostPerVolume As Object
    Public Property maxShipVolumeDockable As Object
    Public Property officeRentalCost As Object
    Public Property operationID As Object
    Public Property stationTypeID As Object
    Public Property corporationID As Object
    Public Property solarSystemID As Object
    Public Property constellationID As Object
    Public Property regionID As Object
    Public Property stationName As Object
    Public Property x As Object
    Public Property y As Object
    Public Property z As Object
    Public Property reprocessingEfficiency As Object
    Public Property reprocessingStationsTake As Object
    Public Property reprocessingHangarFlag As Object
End Class
