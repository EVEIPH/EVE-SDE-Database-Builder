
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLancestries
    Inherits YAMLFilesBase

    Public Const ancestriesFile As String = "ancestries.yaml"

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
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, chrAncestry)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, chrAncestry))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("ancestryID", DataField.Key, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ancestryName", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineID", .bloodlineID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("perception", .perception, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("willpower", .willpower, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("charisma", .charisma, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("memory", .memory, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("intelligence", .intelligence, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", .shortDescription, FieldType.nvarchar_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "ancestryID", "ancestryName", TableName, NameTranslation.GetAllTranslations(.nameID))
                Call Translator.InsertTranslationData(DataField.Key, "ancestryID", "description", TableName, NameTranslation.GetAllTranslations(.descriptionID))

            End With
            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class chrAncestry
    Public Property nameID As Translations
    Public Property bloodlineID As Object
    Public Property descriptionID As Translations
    Public Property perception As Object
    Public Property willpower As Object
    Public Property charisma As Object
    Public Property memory As Object
    Public Property intelligence As Object
    Public Property iconID As Object
    Public Property shortDescription As Object
End Class