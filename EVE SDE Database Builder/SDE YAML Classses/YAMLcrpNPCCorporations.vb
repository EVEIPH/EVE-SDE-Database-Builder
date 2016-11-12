
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLcrpNPCCorporations
    Inherits YAMLFilesBase

    Public Const crpNPCCorporationsFile As String = "crpNPCCorporations.yaml"

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

        Dim YAMLRecords As New List(Of crpNPCCorporation)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("corporationID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("size", FieldType.char_type, 1, True))
        Table.Add(New DBTableField("extent", FieldType.char_type, 1, True))
        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("investorID1", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("investorShares1", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("investorID2", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("investorShares2", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("investorID3", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("investorShares3", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("investorID4", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("investorShares4", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("friendID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("enemyID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("publicShares", FieldType.bigint_type, 0, True))
        Table.Add(New DBTableField("initialPrice", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("minSecurity", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("scattered", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("fringe", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("corridor", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("hub", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("border", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("sizeFactor", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("stationCount", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("stationSystemCount", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("description", FieldType.nvarchar_type, 4000, True))
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
            YAMLRecords = DS.Deserialize(Of List(Of crpNPCCorporation))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", DataField.corporationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("size", DataField.size, FieldType.char_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("extent", DataField.extent, FieldType.char_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", DataField.solarSystemID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorID1", DataField.investorID1, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorShares1", DataField.investorShares1, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorID2", DataField.investorID2, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorShares2", DataField.investorShares2, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorID3", DataField.investorID3, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorShares3", DataField.investorShares3, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorID4", DataField.investorID4, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("investorShares4", DataField.investorShares4, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("friendID", DataField.friendID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("enemyID", DataField.enemyID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("publicShares", DataField.publicShares, FieldType.bigint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("initialPrice", DataField.initialPrice, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("minSecurity", DataField.minSecurity, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("scattered", DataField.scattered, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fringe", DataField.fringe, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("corridor", DataField.corridor, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("hub", DataField.hub, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("border", DataField.border, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("factionID", DataField.factionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("sizeFactor", DataField.sizeFactor, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("stationCount", DataField.stationCount, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("stationSystemCount", DataField.stationSystemCount, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "corporationID", DataField.corporationID, Params.ImportLanguageCode, DataField.description), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class crpNPCCorporation
    Public Property corporationID As Object
    Public Property size As Object
    Public Property extent As Object
    Public Property solarSystemID As Object
    Public Property investorID1 As Object
    Public Property investorShares1 As Object
    Public Property investorID2 As Object
    Public Property investorShares2 As Object
    Public Property investorID3 As Object
    Public Property investorShares3 As Object
    Public Property investorID4 As Object
    Public Property investorShares4 As Object
    Public Property friendID As Object
    Public Property enemyID As Object
    Public Property publicShares As Object
    Public Property initialPrice As Object
    Public Property minSecurity As Object
    Public Property scattered As Object
    Public Property fringe As Object
    Public Property corridor As Object
    Public Property hub As Object
    Public Property border As Object
    Public Property factionID As Object
    Public Property sizeFactor As Object
    Public Property stationCount As Object
    Public Property stationSystemCount As Object
    Public Property description As Object
    Public Property iconID As Object
End Class
