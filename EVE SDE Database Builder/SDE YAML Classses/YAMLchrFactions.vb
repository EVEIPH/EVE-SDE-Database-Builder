
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLchrFactions
    Inherits YAMLFilesBase

    Public Const chrFactionsFile As String = "chrFactions.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        Dim DSB = New DeserializerBuilder()
        DSB.IgnoreUnmatchedProperties()
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of chrFaction)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("factionName", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("description", FieldType.varchar_type, 1500, True))
        Table.Add(New DBTableField("raceIDs", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("corporationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("sizeFactor", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("stationCount", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("stationSystemCount", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("militiaCorporationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of chrFaction))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("factionID", DataField.factionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("factionName", Translator.TranslateData(TableName, "factionName", "factionID", DataField.factionID, Params.ImportLanguageCode, DataField.factionName), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "factionID", DataField.factionID, Params.ImportLanguageCode, DataField.description), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("raceIDs", DataField.raceIDs, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", DataField.solarSystemID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", DataField.corporationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("sizeFactor", DataField.sizeFactor, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("stationCount", DataField.stationCount, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("stationSystemCount", DataField.stationSystemCount, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("militiaCorporationID", DataField.militiaCorporationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class chrFaction
    Public Property factionID As Object
    Public Property factionName As Object
    Public Property description As Object
    Public Property raceIDs As Object
    Public Property solarSystemID As Object
    Public Property corporationID As Object
    Public Property sizeFactor As Object
    Public Property stationCount As Object
    Public Property stationSystemCount As Object
    Public Property militiaCorporationID As Object
    Public Property iconID As Object
End Class