
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLnpcCorporationDivisions
    Inherits YAMLFilesBase

    Public Const npcCorporationDivisionsFile As String = "npcCorporationDivisions.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, npcCorporationDivision)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("divisionID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("divisionName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("leaderTypeName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("shortDescription", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("longDescription", FieldType.varchar_type, 1000, True))
        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, npcCorporationDivision))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("divisionID", DataField.Key, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("divisionName", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("leaderTypeName", NameTranslation.GetLanguageTranslationData(.leaderTypeNameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", .description, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("longDescription", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))


                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "divisionID", "divisionName", TableName, NameTranslation.GetAllTranslations(.nameID))
                Call Translator.InsertTranslationData(DataField.Key, "divisionID", "leaderTypeName", TableName, NameTranslation.GetAllTranslations(.leaderTypeNameID))
                Call Translator.InsertTranslationData(DataField.Key, "divisionID", "longDescription", TableName, NameTranslation.GetAllTranslations(.descriptionID))

            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class npcCorporationDivision
    Public Property divisionID As Object
    Public Property description As Object
    Public Property descriptionID As Translations
    Public Property leaderTypeNameID As Translations
    Public Property nameID As Translations
End Class