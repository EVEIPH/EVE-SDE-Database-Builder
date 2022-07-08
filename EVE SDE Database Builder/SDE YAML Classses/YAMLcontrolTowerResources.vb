﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLcontrolTowerResources
    Inherits YAMLFilesBase

    Public Const controlTowerResourcesFile As String = "controlTowerResources.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, controlTowerResources)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("purpose", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("minSecurityLevel", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("quantity", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("resourceTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As New List(Of String)
        IndexFields.Add("typeID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, controlTowerResources))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            With DataField.Value
                If .resources.Count > 0 Then
                    For Each resource In DataField.Value.resources
                        DataFields = New List(Of DBField)
                        ' Build the insert list
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("purpose", resource.purpose, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("factionID", resource.factionID, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("minSecurityLevel", resource.minSecurityLevel, FieldType.real_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("quantity", resource.quantity, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("resourceTypeID", resource.resourceTypeID, FieldType.int_type))

                        Call UpdateDB.InsertRecord(TableName, DataFields)
                    Next
                End If
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class controlTowerResources
    Public Property resources As List(Of controlTowerResource)
End Class

Public Class controlTowerResource
    Public Property purpose As Object
    Public Property factionID As Object
    Public Property minSecurityLevel As Object
    Public Property quantity As Object
    Public Property resourceTypeID As Object
End Class