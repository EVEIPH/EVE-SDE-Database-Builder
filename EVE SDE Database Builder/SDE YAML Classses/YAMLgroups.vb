
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLgroups
    Inherits YAMLFilesBase

    Public Const groupsFile As String = "groups.yaml"

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
        FileNameErrorTracker = groupsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, groupID)
        Dim DataFields As List(Of DBField)
        Dim CategoryName As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("groupID", FieldType.int_type, 0, False, True),
            New DBTableField("categoryID", FieldType.int_type, 0, True),
            New DBTableField("groupName", FieldType.nvarchar_type, 500, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("useBasePrice", FieldType.bit_type, -1, True),
            New DBTableField("anchored", FieldType.bit_type, -1, True),
            New DBTableField("anchorable", FieldType.bit_type, -1, True),
            New DBTableField("fittableNonSingleton", FieldType.bit_type, -1, True),
            New DBTableField("published", FieldType.bit_type, -1, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {"groupID"}
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_GID", IndexFields)

        IndexFields = New List(Of String) From {"categoryID"}
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("useBasePrice", .useBasePrice, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anchored", .anchored, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anchorable", .anchorable, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fittableNonSingleton", .fittableNonSingleton, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", .published, FieldType.bit_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "groupID", "groupName", TableName, NameTranslation.GetAllTranslations(.name))
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