﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLskinLicenses
    Inherits YAMLFilesBase

    Public Const skinLicensesFile As String = "skinLicenses.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = skinLicensesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, skinLicense)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("licenseTypeID", FieldType.int_type, 0, False, True),
            New DBTableField("duration", FieldType.int_type, 0, True),
            New DBTableField("isSingleUse", FieldType.int_type, 0, True),
            New DBTableField("skinID", FieldType.int_type, 0, True)
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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, skinLicense))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("licenseTypeID", .licenseTypeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("duration", .duration, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("skinID", .skinID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("isSingleUse", .isSingleUse, FieldType.int_type))
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

Public Class skinLicense
    Public Property licenseTypeID As Object
    Public Property duration As Object
    Public Property isSingleUse As Object
    Public Property skinID As Object
End Class