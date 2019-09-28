
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLgroupIDs
    Inherits YAMLFilesBase

    Public Const groupIDsFile As String = "groupIDs.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
        ' Reset table name
        TableName = "invGroups"
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

        Dim YAMLRecords As New Dictionary(Of Long, groupID)
        Dim DataFields As List(Of DBField)
        Dim CategoryName As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("categoryID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("groupName", FieldType.nvarchar_type, 500, True))
        Table.Add(New DBTableField("published", FieldType.bit_type, -1, True))
        Table.Add(New DBTableField("anchorable", FieldType.bit_type, -1, True))
        Table.Add(New DBTableField("anchored", FieldType.bit_type, -1, True))
        Table.Add(New DBTableField("fittableNonSingleton", FieldType.bit_type, -1, True))
        Table.Add(New DBTableField("useBasePrice", FieldType.bit_type, -1, True))
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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, groupID))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("groupID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("categoryID", .categoryID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("groupName", NameTranslation.GetLanguageTranslationData(.name), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anchorable", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anchored", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fittableNonSingleton", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("useBasePrice", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "groupID", "groupName", TableName, NameTranslation.GetAllTranslations(.name))
            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class groupID
    Public Property name As Translations
    Public Property categoryID As Object
    Public Property published As Object
    Public Property anchorable As Object
    Public Property anchored As Object
    Public Property fittableNonSingleton As Object
    Public Property useBasePrice As Object
    Public Property iconID As Object
End Class