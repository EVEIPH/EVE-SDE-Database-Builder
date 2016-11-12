
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLchrBloodLines
    Inherits YAMLFilesBase

    Public Const chrBloodlinesFile As String = "chrBloodlines.yaml"

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

        Dim YAMLRecords As New List(Of chrBloodLine)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("bloodlineID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("bloodlineName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("raceID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("description", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("maleDescription", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("femaleDescription", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("shipTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("corporationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("perception", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("willpower", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("charisma", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("memory", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("intelligence", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("shortDescription", FieldType.nvarchar_type, 500, True))
        Table.Add(New DBTableField("shortMaleDescription", FieldType.nvarchar_type, 500, True))
        Table.Add(New DBTableField("shortFemaleDescription", FieldType.nvarchar_type, 500, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of chrBloodLine))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineID", DataField.bloodlineID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineName", Translator.TranslateData(TableName, "bloodlineName", "bloodlineID", DataField.bloodlineID, Params.ImportLanguageCode, DataField.bloodlineName), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("raceID", DataField.raceID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "bloodlineID", DataField.bloodlineID, Params.ImportLanguageCode, DataField.description), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("maleDescription", DataField.maleDescription, FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("femaleDescription", DataField.femaleDescription, FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("shipTypeID", DataField.shipTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", DataField.corporationID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("perception", DataField.perception, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("willpower", DataField.willpower, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("charisma", DataField.charisma, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("memory", DataField.memory, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("intelligence", DataField.intelligence, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", DataField.shortDescription, FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("shortMaleDescription", DataField.shortMaleDescription, FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("shortFemaleDescription", DataField.shortFemaleDescription, FieldType.nvarchar_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class chrBloodLine
    Public Property bloodlineID As Object
    Public Property bloodlineName As Object
    Public Property raceID As Object
    Public Property description As Object
    Public Property maleDescription As Object
    Public Property femaleDescription As Object
    Public Property shipTypeID As Object
    Public Property corporationID As Object

    Public Property perception As Object
    Public Property willpower As Object
    Public Property charisma As Object
    Public Property memory As Object
    Public Property intelligence As Object

    Public Property iconID As Object
    Public Property shortDescription As Object
    Public Property shortMaleDescription As Object
    Public Property shortFemaleDescription As Object
End Class