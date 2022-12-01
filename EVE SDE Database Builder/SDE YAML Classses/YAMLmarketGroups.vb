
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLmarketGroups
    Inherits YAMLFilesBase

    Public Const marketGroupsFile As String = "marketGroups.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, marketGroup)
        Dim IndexFields As List(Of String)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("marketGroupID ", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("descriptionID", FieldType.nvarchar_type, 300, True))
        Table.Add(New DBTableField("hasTypes ", FieldType.bit_type, -1, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("nameID", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("parentGroupID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        IndexFields = New List(Of String)
        IndexFields.Add("marketGroupID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_MGID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, marketGroup))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("marketGroupID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("descriptionID", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("hasTypes", .hasTypes, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("nameID", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("parentGroupID", .parentGroupID, FieldType.int_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "marketGroupID", "descriptionID", TableName, NameTranslation.GetAllTranslations(.descriptionID))
                Call Translator.InsertTranslationData(DataField.Key, "marketGroupID", "nameID", TableName, NameTranslation.GetAllTranslations(.nameID))

            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class marketGroup
    Public Property marketGroupID As Object
    Public Property descriptionID As Translations
    Public Property hasTypes As Object
    Public Property iconID As Object
    Public Property nameID As Translations
    Public Property parentGroupID As Object
End Class