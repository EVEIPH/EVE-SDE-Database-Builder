﻿
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLmetaGroups
    Inherits YAMLFilesBase

    Public Const metaGroupsFile As String = "metaGroups.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = metaGroupsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, metaGroup)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("metaGroupID", FieldType.smallint_type, 0, False, True),
            New DBTableField("descriptionID", FieldType.nvarchar_type, 1000, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("iconSuffix", FieldType.nvarchar_type, 30, True),
            New DBTableField("nameID", FieldType.nvarchar_type, 100, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Table = New List(Of DBTableField) From {
            New DBTableField("metaGroupID", FieldType.smallint_type, 0, False),
            New DBTableField("colorValue", FieldType.float_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "Colors", Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, metaGroup))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("metaGroupID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("descriptionID", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconSuffix", .iconSuffix, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("nameID", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "marketGroupID", "descriptionID", TableName, NameTranslation.GetAllTranslations(.descriptionID))
                Call Translator.InsertTranslationData(DataField.Key, "marketGroupID", "nameID", TableName, NameTranslation.GetAllTranslations(.nameID))

                Call UpdateDB.InsertRecord(TableName, DataFields)

                If Not IsNothing(.color) Then
                    If .color.Count <> 0 Then
                        ' Add these
                        For i = 0 To .color.Count - 1
                            DataFields = New List(Of DBField) From {
                                    UpdateDB.BuildDatabaseField("metaGroupID", DataField.Key, FieldType.int_type),
                                    UpdateDB.BuildDatabaseField("colorValue", .color(i), FieldType.float_type)
                                }
                            Call UpdateDB.InsertRecord(TableName & "Colors", DataFields)
                        Next
                    End If
                End If

            End With

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

Public Class metaGroup
    Public Property metaGroupID As Object
    Public Property descriptionID As Translations
    Public Property iconID As Object
    Public Property iconSuffix As Object
    Public Property nameID As Translations
    Public Property color As List(Of Double) ' 4 values, no idea what they are
End Class