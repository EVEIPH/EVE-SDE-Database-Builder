﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLramAssemblyLineTypes
    Inherits YAMLFilesBase

    Public Const ramAssemblyLineTypesFile As String = "ramAssemblyLineTypes.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = ramAssemblyLineTypesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of ramAssemblyLineType)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("assemblyLineTypeID", FieldType.tinyint_type, 0, False, True),
            New DBTableField("assemblyLineTypeName", FieldType.nvarchar_type, 100, True),
            New DBTableField("description", FieldType.nvarchar_type, 1000, True),
            New DBTableField("baseTimeMultiplier", FieldType.real_type, 0, True),
            New DBTableField("baseMaterialMultiplier", FieldType.real_type, 0, True),
            New DBTableField("baseCostMultiplier", FieldType.real_type, 0, True),
            New DBTableField("volume", FieldType.real_type, 0, True),
            New DBTableField("activityID", FieldType.tinyint_type, 0, True),
            New DBTableField("minCostPerHour", FieldType.real_type, 0, True)
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
            YAMLRecords = DS.Deserialize(Of List(Of ramAssemblyLineType))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("assemblyLineTypeID", DataField.assemblyLineTypeID, FieldType.tinyint_type),
                UpdateDB.BuildDatabaseField("assemblyLineTypeName", DataField.assemblyLineTypeName, FieldType.nvarchar_type),
                UpdateDB.BuildDatabaseField("description", DataField.description, FieldType.nvarchar_type),
                UpdateDB.BuildDatabaseField("baseTimeMultiplier", DataField.baseTimeMultiplier, FieldType.real_type),
                UpdateDB.BuildDatabaseField("baseMaterialMultiplier", DataField.baseMaterialMultiplier, FieldType.real_type),
                UpdateDB.BuildDatabaseField("baseCostMultiplier", DataField.baseCostMultiplier, FieldType.real_type),
                UpdateDB.BuildDatabaseField("volume", DataField.volume, FieldType.real_type),
                UpdateDB.BuildDatabaseField("activityID", DataField.activityID, FieldType.tinyint_type),
                UpdateDB.BuildDatabaseField("minCostPerHour", DataField.minCostPerHour, FieldType.real_type)
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

Public Class ramAssemblyLineType
    Public Property assemblyLineTypeID As Object
    Public Property assemblyLineTypeName As Object
    Public Property description As Object
    Public Property baseTimeMultiplier As Object
    Public Property baseMaterialMultiplier As Object
    Public Property baseCostMultiplier As Object
    Public Property volume As Object
    Public Property activityID As Object
    Public Property minCostPerHour As Object
End Class