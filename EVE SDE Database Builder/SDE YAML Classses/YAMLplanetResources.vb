﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLplanetResources
    Inherits YAMLFilesBase

    Public Const planetResourcesFile As String = "planetResources.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = planetResourcesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, planetResources)
        Dim DataFields As List(Of DBField)
        Dim TranslatedField As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("starID", FieldType.bigint_type, 0, True, True),
            New DBTableField("power", FieldType.bigint_type, 0, True),
            New DBTableField("workforce", FieldType.bigint_type, 0, True),
            New DBTableField("cycle_minutes", FieldType.int_type, 0, True),
            New DBTableField("harvest_silo_max", FieldType.bigint_type, 0, True),
            New DBTableField("maturation_cycle_minutes", FieldType.bigint_type, 0, True),
            New DBTableField("maturation_percent", FieldType.int_type, 0, True),
            New DBTableField("mature_silo_max", FieldType.double_type, 0, True),
            New DBTableField("reagent_harvest_amount", FieldType.bigint_type, 0, True),
            New DBTableField("reagent_type_id", FieldType.bigint_type, 0, True)
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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, planetResources))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("starID", DataField.Key, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("power", .power, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("workforce", .workforce, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("cycle_minutes", .cycle_minutes, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("harvest_silo_max", .harvest_silo_max, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maturation_cycle_minutes", .maturation_cycle_minutes, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maturation_percent", .maturation_percent, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("mature_silo_max", .mature_silo_max, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("reagent_harvest_amount", .reagent_harvest_amount, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("reagent_type_id", .reagent_type_id, FieldType.bigint_type))
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

Public Class planetResources
    Public Property power As Long
    Public Property workforce As Long
    Public Property cycle_minutes As Integer
    Public Property harvest_silo_max As Long
    Public Property maturation_cycle_minutes As Long
    Public Property maturation_percent As Integer
    Public Property mature_silo_max As Double
    Public Property reagent_harvest_amount As Long
    Public Property reagent_type_id As Long
End Class