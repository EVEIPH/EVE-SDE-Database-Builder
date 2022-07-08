
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLeveIcons
    Inherits YAMLFilesBase

    Public Const eveIconsFile As String = "iconIDs.yaml"
    Private Const eveIconsBackgroundsTableName As String = "eveIconsBackgrounds"
    Private Const eveIconsForegroundsTableName As String = "eveIconsForegrounds"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
        ' Reset table name
        TableName = "eveIcons"
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

        Dim YAMLRecords As New Dictionary(Of Long, eveIcon)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("iconFile", FieldType.varchar_type, 500, True))
        Table.Add(New DBTableField("description", FieldType.text_type, MaxFieldLen, True))
        Table.Add(New DBTableField("obsolete", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        Call BuildIconInfoTables()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, eveIcon))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconFile", .iconFile, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.text_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("obsolete", .obsolete, FieldType.int_type))
            End With
            Call UpdateDB.InsertRecord(TableName, DataFields)

            Call InsertBackForeInfo(DataField.Key, "backgrounds", DataField.Value.backgrounds)
            Call InsertBackForeInfo(DataField.Key, "foregrounds", DataField.Value.foregrounds)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub InsertBackForeInfo(ByVal graphicID As Integer, ByVal iconDataType As String, ByVal iconData As List(Of Object))
        Dim DataFields As List(Of DBField)

        If Not IsNothing(iconData) Then
            DataFields = New List(Of DBField)

            ' Build the insert list for backfore types
            If iconDataType = "backgrounds" Then
                For Each BG In iconData
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", graphicID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("backgroundProperty", BG, FieldType.varchar_type))
                    Call UpdateDB.InsertRecord(eveIconsBackgroundsTableName, DataFields)
                Next
            ElseIf iconDataType = "foregrounds" Then
                For Each FG In iconData
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", graphicID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("foregroundProperty", FG, FieldType.varchar_type))
                    Call UpdateDB.InsertRecord(eveIconsForegroundsTableName, DataFields)
                Next
            End If

        End If
    End Sub

    Private Sub BuildIconInfoTables()

        Dim Table As New List(Of DBTableField)
        Dim IndexFields As List(Of String)

        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("backgroundProperty", FieldType.varchar_type, 50, True))

        Call UpdateDB.CreateTable(eveIconsBackgroundsTableName, Table)

        IndexFields = New List(Of String)
        IndexFields.Add("graphicID")
        Call UpdateDB.CreateIndex(eveIconsBackgroundsTableName, "IDX_" & eveIconsBackgroundsTableName & "_GID", IndexFields)

        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("foregroundProperty", FieldType.varchar_type, 50, True))

        Call UpdateDB.CreateTable(eveIconsForegroundsTableName, Table)

        IndexFields = New List(Of String)
        IndexFields.Add("graphicID")
        Call UpdateDB.CreateIndex(eveIconsForegroundsTableName, "IDX_" & eveIconsForegroundsTableName & "_GID", IndexFields)

    End Sub

End Class

Public Class eveIcon
    Public Property description As Object
    Public Property iconFile As Object
    Public Property obsolete As Object
    Public Property backgrounds As List(Of Object)
    Public Property foregrounds As List(Of Object)
End Class