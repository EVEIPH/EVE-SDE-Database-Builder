
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLmetaGroups
    Inherits YAMLFilesBase

    Public Const metaGroupsFile As String = "metaGroups.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, metaGroup)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("metaGroupID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("descriptionID", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("iconSuffix", FieldType.nvarchar_type, 30, True))
        Table.Add(New DBTableField("nameID", FieldType.nvarchar_type, 100, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, metaGroup))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("metaGroupID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("descriptionID", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconSuffix", .iconSuffix, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("nameID", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "marketGroupID", "descriptionID", TableName, NameTranslation.GetAllTranslations(.descriptionID))
                Call Translator.InsertTranslationData(DataField.Key, "marketGroupID", "nameID", TableName, NameTranslation.GetAllTranslations(.nameID))

            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class metaGroup
    Public Property metaGroupID As Object
    Public Property descriptionID As Translations
    Public Property iconID As Object
    Public Property iconSuffix As Object
    Public Property nameID As Translations
End Class