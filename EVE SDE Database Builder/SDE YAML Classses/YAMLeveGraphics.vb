
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLeveGrpahics
    Inherits YAMLFilesBase

    Public Const eveGraphicsFile As String = "graphicIDs.yaml"

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
        DSB.IgnoreUnmatchedProperties()
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, eveGrpahic)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("graphicID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("graphicFile", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("iconFolder", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("sofFactionName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("sofHullName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("sofRaceName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.text_type, MaxFieldLen, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, eveGrpahic))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicfile", .graphicfile, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconFolder", .iconFolder, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofFactionName", .sofFactionName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofHullName", .sofHullName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofRaceName", .sofRaceName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.text_type))
            End With
            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class eveGrpahic
    Public Property graphicFile As Object
    Public Property iconFolder As Object
    Public Property sofFactionName As Object
    Public Property sofHullName As Object
    Public Property sofRaceName As Object
    Public Property description As Object
End Class