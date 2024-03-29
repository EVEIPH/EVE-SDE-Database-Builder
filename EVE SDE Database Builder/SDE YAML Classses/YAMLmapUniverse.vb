﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLmapUniverse
    Inherits YAMLFilesBase

    Public Const mapUniverseFile As String = "mapUniverse.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = mapUniverseFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of mapUniverse)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("universeID", FieldType.int_type, 0, False, True),
            New DBTableField("universeName", FieldType.varchar_type, 100, True),
            New DBTableField("x", FieldType.real_type, 0, True),
            New DBTableField("y", FieldType.real_type, 0, True),
            New DBTableField("z", FieldType.real_type, 0, True),
            New DBTableField("x_Min", FieldType.real_type, 0, True),
            New DBTableField("x_Max", FieldType.real_type, 0, True),
            New DBTableField("y_Min", FieldType.real_type, 0, True),
            New DBTableField("y_Max", FieldType.real_type, 0, True),
            New DBTableField("z_Min", FieldType.real_type, 0, True),
            New DBTableField("z_Max", FieldType.real_type, 0, True),
            New DBTableField("radius", FieldType.real_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of mapUniverse))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("universeID", DataField.universeID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("universeName", DataField.universeName, FieldType.varchar_type),
                UpdateDB.BuildDatabaseField("x", DataField.x, FieldType.real_type),
                UpdateDB.BuildDatabaseField("y", DataField.y, FieldType.real_type),
                UpdateDB.BuildDatabaseField("z", DataField.z, FieldType.real_type),
                UpdateDB.BuildDatabaseField("x_Min", DataField.xMin, FieldType.real_type),
                UpdateDB.BuildDatabaseField("x_Max", DataField.xMax, FieldType.real_type),
                UpdateDB.BuildDatabaseField("y_Min", DataField.yMin, FieldType.real_type),
                UpdateDB.BuildDatabaseField("y_Max", DataField.yMax, FieldType.real_type),
                UpdateDB.BuildDatabaseField("z_Min", DataField.zMin, FieldType.real_type),
                UpdateDB.BuildDatabaseField("z_Max", DataField.zMax, FieldType.real_type),
                UpdateDB.BuildDatabaseField("radius", DataField.radius, FieldType.real_type)
            }

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

Public Class mapUniverse
    Public Property universeID As Object
    Public Property universeName As Object
    Public Property x As Object
    Public Property y As Object
    Public Property z As Object
    Public Property xMin As Object
    Public Property xMax As Object
    Public Property yMin As Object
    Public Property yMax As Object
    Public Property zMin As Object
    Public Property zMax As Object
    Public Property radius As Object
End Class