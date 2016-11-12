
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLchrRaces
    Inherits YAMLFilesBase

    Public Const chrRacesFile As String = "chrRaces.yaml"

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

        Dim YAMLRecords As New List(Of chrRace)
        Dim DataFields As List(Of DBField)
        Dim TranslatedField As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("raceID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("raceName", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("description", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("shortDescription", FieldType.varchar_type, 500, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of chrRace))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("raceID", DataField.raceID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("raceName", Translator.TranslateData(TableName, "raceName", "raceID", DataField.raceID, Params.ImportLanguageCode, DataField.raceName), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "raceID", DataField.raceID, Params.ImportLanguageCode, DataField.description), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", DataField.shortDescription, FieldType.varchar_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class chrRace
    Public Property raceID As Object
    Public Property raceName As Object
    Public Property description As Object
    Public Property iconID As Object
    Public Property shortDescription As Object
End Class