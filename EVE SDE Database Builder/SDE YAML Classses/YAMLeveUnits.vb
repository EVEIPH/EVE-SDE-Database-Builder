
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLeveUnits
    Inherits YAMLFilesBase

    Public Const eveUnitsFile As String = "eveUnits.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = eveUnitsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of eveUnit)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("unitID", FieldType.tinyint_type, 0, True),
            New DBTableField("unitName", FieldType.varchar_type, 100, True),
            New DBTableField("displayName", FieldType.varchar_type, 50, True),
            New DBTableField("description", FieldType.varchar_type, 1000, True)
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
            YAMLRecords = DS.Deserialize(Of List(Of eveUnit))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("unitID", DataField.unitID, FieldType.tinyint_type),
                UpdateDB.BuildDatabaseField("unitName", DataField.unitName, FieldType.varchar_type),
                UpdateDB.BuildDatabaseField("displayName", Translator.TranslateData(TableName, "displayName", "unitID", DataField.unitID, Params.ImportLanguageCode, DataField.displayName), FieldType.varchar_type),
                UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "unitID", DataField.unitID, Params.ImportLanguageCode, DataField.description), FieldType.varchar_type)
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

Public Class eveUnit
    Public Property unitID As Object
    Public Property unitName As Object
    Public Property displayName As Object
    Public Property description As Object
End Class