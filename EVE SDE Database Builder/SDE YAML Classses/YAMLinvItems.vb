﻿
Imports YamlDotNet.Serialization
Imports System.IO
Imports System.Data.SqlClient

Public Class YAMLinvItems
    Inherits YAMLFilesBase

    Public Const invItemsFile As String = "invItems.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = invItemsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of invItem)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("itemID", FieldType.bigint_type, 0, False, True),
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("ownerID", FieldType.int_type, 0, True),
            New DBTableField("locationID", FieldType.bigint_type, 0, True),
            New DBTableField("flagID", FieldType.smallint_type, 0, True),
            New DBTableField("quantity", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String) From {
            "locationID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_LID", IndexFields)

        IndexFields = New List(Of String) From {
            "ownerID",
            "locationID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_OID_LID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of invItem))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("itemID", DataField.itemID, FieldType.bigint_type),
                UpdateDB.BuildDatabaseField("typeID", DataField.typeID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("ownerID", DataField.ownerID, FieldType.int_type),
                UpdateDB.BuildDatabaseField("locationID", DataField.locationID, FieldType.bigint_type),
                UpdateDB.BuildDatabaseField("flagID", DataField.flagID, FieldType.smallint_type),
                UpdateDB.BuildDatabaseField("quantity", DataField.quantity, FieldType.int_type)
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

Public Class invItem
    Public Property itemID As Object
    Public Property typeID As Object
    Public Property ownerID As Object
    Public Property locationID As Object
    Public Property flagID As Object
    Public Property quantity As Object
End Class