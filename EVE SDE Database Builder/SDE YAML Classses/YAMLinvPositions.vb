
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLinvPositions
    Inherits YAMLFilesBase

    Public Const invPositionsFile As String = "invPositions.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = invPositionsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New List(Of invPosition)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("itemID", FieldType.bigint_type, 0, False, True),
            New DBTableField("x", FieldType.real_type, 0, False),
            New DBTableField("y", FieldType.real_type, 0, False),
            New DBTableField("z", FieldType.real_type, 0, False),
            New DBTableField("yaw", FieldType.real_type, 0, True),
            New DBTableField("pitch", FieldType.real_type, 0, True),
            New DBTableField("roll", FieldType.real_type, 0, True)
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
            YAMLRecords = DS.Deserialize(Of List(Of invPosition))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("itemID", DataField.itemID, FieldType.bigint_type),
                UpdateDB.BuildDatabaseField("x", DataField.x, FieldType.real_type),
                UpdateDB.BuildDatabaseField("y", DataField.y, FieldType.real_type),
                UpdateDB.BuildDatabaseField("z", DataField.z, FieldType.real_type),
                UpdateDB.BuildDatabaseField("yaw", DataField.yaw, FieldType.real_type),
                UpdateDB.BuildDatabaseField("pitch", DataField.pitch, FieldType.real_type),
                UpdateDB.BuildDatabaseField("roll", DataField.roll, FieldType.real_type)
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

Public Class invPosition
    Public Property itemID As Object
    Public Property x As Object
    Public Property y As Object
    Public Property z As Object
    Public Property yaw As Object
    Public Property pitch As Object
    Public Property roll As Object
End Class