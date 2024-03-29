﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLcorporationActivities
    Inherits YAMLFilesBase

    Public Const corporationActivitiesFile As String = "corporationActivities.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = corporationActivitiesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, corporationActivity)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("activityID", FieldType.tinyint_type, 0, False, True),
            New DBTableField("activityName", FieldType.nvarchar_type, 100, True)
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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, corporationActivity))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("activityID", DataField.Key, FieldType.tinyint_type),
                UpdateDB.BuildDatabaseField("activityName", NameTranslation.GetLanguageTranslationData(DataField.Value.nameID), FieldType.nvarchar_type)
            }

                ' Insert the translated data into translation tables
            Call Translator.InsertTranslationData(DataField.Key, "activityID", "activityName", TableName, NameTranslation.GetAllTranslations(DataField.Value.nameID))

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

Public Class corporationActivity
    Public Property nameID As Translations
End Class