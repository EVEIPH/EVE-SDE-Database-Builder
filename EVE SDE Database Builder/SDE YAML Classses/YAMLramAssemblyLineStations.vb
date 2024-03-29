﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLramAssemblyLineStations
    Inherits YAMLFilesBase

    Public Const ramAssemblyLineStationsFile As String = "ramAssemblyLineStations.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = ramAssemblyLineStationsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of ramAssemblyLineStation)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("stationID", FieldType.int_type, 0, False, True),
            New DBTableField("assemblyLineTypeID", FieldType.tinyint_type, 0, False, True),
            New DBTableField("quantity", FieldType.tinyint_type, 0, True),
            New DBTableField("stationTypeID", FieldType.int_type, 0, True),
            New DBTableField("ownerID", FieldType.int_type, 0, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("regionID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String) From {
            "regionID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_RID", IndexFields)

        IndexFields = New List(Of String) From {
            "ownerID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_OID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of ramAssemblyLineStation))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("stationID", DataField.stationID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("assemblyLineTypeID", DataField.assemblyLineTypeID, FieldType.tinyint_type),
                UpdateDB.BuildDatabaseField("quantity", DataField.quantity, FieldType.tinyint_type),
                UpdateDB.BuildDatabaseField("stationTypeID", DataField.stationTypeID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("ownerID", DataField.ownerID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("solarSystemID", DataField.solarSystemID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("regionID", DataField.regionID, FieldType.int_type)
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

Public Class ramAssemblyLineStation
    Public Property stationID As Object
    Public Property assemblyLineTypeID As Object
    Public Property quantity As Object
    Public Property stationTypeID As Object
    Public Property ownerID As Object
    Public Property solarSystemID As Object
    Public Property regionID As Object
End Class