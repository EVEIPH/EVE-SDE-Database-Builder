
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLchrAncestries
    Inherits YAMLFilesBase

    Public Const chrAncestriesFile As String = "chrAncestries.yaml"

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

        Dim YAMLRecords As New List(Of chrAncestry)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("ancestryID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("ancestryName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("bloodlineID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("description", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("perception", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("willpower", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("charisma", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("memory", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("intelligence", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("shortDescription", FieldType.nvarchar_type, 500, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of chrAncestry))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("ancestryID", DataField.ancestryID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("ancestryName", Translator.TranslateData(TableName, "ancestryName", "ancestryID", DataField.ancestryID, Params.ImportLanguageCode, DataField.ancestryName), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineID", DataField.bloodlineID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "ancestryID", DataField.ancestryID, Params.ImportLanguageCode, DataField.description), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("perception", DataField.perception, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("willpower", DataField.willpower, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("charisma", DataField.charisma, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("memory", DataField.memory, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("intelligence", DataField.intelligence, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", DataField.shortDescription, FieldType.nvarchar_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class chrAncestry
    Public Property ancestryID As Object
    Public Property ancestryName As Object
    Public Property bloodlineID As Object
    Public Property description As Object
    Public Property perception As Object
    Public Property willpower As Object
    Public Property charisma As Object
    Public Property memory As Object
    Public Property intelligence As Object
    Public Property iconID As Object
    Public Property shortDescription As Object
End Class