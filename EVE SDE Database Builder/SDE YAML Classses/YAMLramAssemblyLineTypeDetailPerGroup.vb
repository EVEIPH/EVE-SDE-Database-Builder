
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLramAssemblyLineTypeDetailPerGroup
    Inherits YAMLFilesBase

    Public Const ramAssemblyLineTypeDetailPerGroupFile As String = "ramAssemblyLineTypeDetailPerGroup.yaml"

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

        Dim YAMLRecords As New List(Of ramAssemblyLineTypeDetailPerGroup)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("assemblyLineTypeID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("timeMultiplier", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("materialMultiplier", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("costMultiplier", FieldType.real_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of ramAssemblyLineTypeDetailPerGroup))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("assemblyLineTypeID", DataField.assemblyLineTypeID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("groupID", DataField.groupID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("timeMultiplier", DataField.timeMultiplier, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("materialMultiplier", DataField.materialMultiplier, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("costMultiplier", DataField.costMultiplier, FieldType.real_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class ramAssemblyLineTypeDetailPerGroup
    Public Property assemblyLineTypeID As Object
    Public Property groupID As Object
    Public Property timeMultiplier As Object
    Public Property materialMultiplier As Object
    Public Property costMultiplier As Object
End Class