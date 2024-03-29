﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLinvUniqueNames
    Inherits YAMLFilesBase

    Public Const invUniqueNamesFile As String = "invUniqueNames.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = invUniqueNamesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of invUniqueName)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("itemID", FieldType.int_type, 0, False, True),
            New DBTableField("itemName", FieldType.nvarchar_type, 200, True),
            New DBTableField("groupID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String) From {
            "itemName"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_IN", IndexFields, True)

        IndexFields = New List(Of String) From {
            "groupID",
            "itemName"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_GID_IN", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of invUniqueName))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("itemID", DataField.itemID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("itemName", DataField.itemName, FieldType.nvarchar_type),
                UpdateDB.BuildDatabaseField("groupID", DataField.groupID, FieldType.int_type)
            }

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

Public Class invUniqueName
    Public Property itemID As Object
    Public Property itemName As Object
    Public Property groupID As Object
End Class