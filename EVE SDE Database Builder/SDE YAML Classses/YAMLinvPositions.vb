
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
        Dim DSB = New DeserializerBuilder()
        DSB.IgnoreUnmatchedProperties()
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of invPosition)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("itemID", FieldType.bigint_type, 0, False, True))
        Table.Add(New DBTableField("x", FieldType.float_type, 0, False))
        Table.Add(New DBTableField("y", FieldType.float_type, 0, False))
        Table.Add(New DBTableField("z", FieldType.float_type, 0, False))
        Table.Add(New DBTableField("yaw", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("pitch", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("roll", FieldType.real_type, 0, True))

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
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("itemID", DataField.itemID, FieldType.bigint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x", DataField.x, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", DataField.y, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", DataField.z, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("yaw", DataField.yaw, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("pitch", DataField.pitch, FieldType.real_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("roll", DataField.roll, FieldType.real_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

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