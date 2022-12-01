
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLeveGrpahics
    Inherits YAMLFilesBase

    Public Const eveGraphicsFile As String = "graphicIDs.yaml"
    Private Const eveGraphicsIconInfoTableName As String = "evegraphicIconInfo"
    Private Const eveGraphicsIconBackgroundsTableName As String = "evegraphicBackgrounds"
    Private Const eveGraphicsIconForegroundsTableName As String = "evegraphicForegrounds"
    Private Const eveGraphicssofLayouts As String = "graphicsofLayouts"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
        ' Reset table name
        TableName = "eveGraphics"
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

        Dim YAMLRecords As New Dictionary(Of Long, eveGraphic)
        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build main table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("graphicFile", FieldType.varchar_type, 150, True))
        Table.Add(New DBTableField("description", FieldType.text_type, MaxFieldLen, True))
        Table.Add(New DBTableField("sofFactionName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("sofHullName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("sofRaceName", FieldType.varchar_type, 100, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Add minor table for sofLayout change in uprising
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("sofLayout", FieldType.varchar_type, 100, True))

        Call UpdateDB.CreateTable(eveGraphicssofLayouts, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("graphicID")
        Call UpdateDB.CreateIndex(eveGraphicssofLayouts, "IDX_" & eveGraphicssofLayouts & "_GID", IndexFields)

        ' Set up the tables for icon info
        Call BuildIconInfoTables()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, eveGraphic))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)
            DataFields2 = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicFile", .graphicFile, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofFactionName", .sofFactionName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofHullName", .sofHullName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofRaceName", .sofRaceName, FieldType.varchar_type))

                If Not IsNothing(.sofLayout) Then
                    For Each sofLayout In .sofLayout
                        DataFields2.Add(UpdateDB.BuildDatabaseField("graphicID", DataField.Key, FieldType.int_type))
                        DataFields2.Add(UpdateDB.BuildDatabaseField("sofLayout", sofLayout, FieldType.varchar_type))
                        Call UpdateDB.InsertRecord(eveGraphicssofLayouts, DataFields2)
                    Next
                End If

                ' Insert the icon info as well
                Call InsertIconInfo(DataField.Key, .iconInfo)

            End With
            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub InsertIconInfo(ByVal graphicID As Integer, ByVal TypeList As iconInfoClass)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(TypeList) Then
            DataFields = New List(Of DBField)

            ' Build the insert list for recomended types
            DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", graphicID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("folder", TypeList.folder, FieldType.varchar_type))
            Call UpdateDB.InsertRecord(eveGraphicsIconInfoTableName, DataFields)

            If Not IsNothing(TypeList.backgrounds) Then
                For Each BG In TypeList.backgrounds
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", graphicID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("backgroundProperty", BG, FieldType.varchar_type))
                    Call UpdateDB.InsertRecord(eveGraphicsIconBackgroundsTableName, DataFields)
                Next
            End If

            If Not IsNothing(TypeList.foregrounds) Then
                For Each FG In TypeList.foregrounds
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", graphicID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("foregroundProperty", FG, FieldType.varchar_type))
                    Call UpdateDB.InsertRecord(eveGraphicsIconForegroundsTableName, DataFields)
                Next
            End If

        End If
    End Sub

    Private Sub BuildIconInfoTables()

        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("folder", FieldType.varchar_type, 100, True))

        Call UpdateDB.CreateTable(eveGraphicsIconInfoTableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("graphicID")
        Call UpdateDB.CreateIndex(eveGraphicsIconInfoTableName, "IDX_" & eveGraphicsIconInfoTableName & "_GID", IndexFields)

        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("backgroundProperty", FieldType.varchar_type, 50, True))

        Call UpdateDB.CreateTable(eveGraphicsIconBackgroundsTableName, Table)

        IndexFields = New List(Of String)
        IndexFields.Add("graphicID")
        Call UpdateDB.CreateIndex(eveGraphicsIconBackgroundsTableName, "IDX_" & eveGraphicsIconBackgroundsTableName & "_GID", IndexFields)

        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("foregroundProperty", FieldType.varchar_type, 50, True))

        Call UpdateDB.CreateTable(eveGraphicsIconForegroundsTableName, Table)

        IndexFields = New List(Of String)
        IndexFields.Add("graphicID")
        Call UpdateDB.CreateIndex(eveGraphicsIconForegroundsTableName, "IDX_" & eveGraphicsIconForegroundsTableName & "_GID", IndexFields)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class eveGraphic
    Public Property description As Object
    Public Property graphicFile As Object
    Public Property iconInfo As iconInfoClass
    Public Property sofFactionName As Object
    Public Property sofHullName As Object
    Public Property sofRaceName As Object
    Public Property sofLayout As List(Of String)
End Class

Public Class iconInfoClass
    Public Property folder As Object
    Public Property backgrounds As List(Of Object)
    Public Property foregrounds As List(Of Object)
End Class