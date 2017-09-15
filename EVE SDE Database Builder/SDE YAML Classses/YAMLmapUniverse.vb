
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLmapUniverse
    Inherits YAMLFilesBase

    Public Const mapUniverseFile As String = "mapUniverse.yaml"

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

        Dim YAMLRecords As New List(Of mapUniverse)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("universeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("universeName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("x", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("x_Min", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("x_Max", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("y_Min", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("y_Max", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("z_Min", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("z_Max", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("radius", FieldType.float_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of mapUniverse))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("universeID", DataField.universeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("universeName", DataField.universeName, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x", DataField.x, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y", DataField.y, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z", DataField.z, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Min", DataField.xMin, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("x_Max", DataField.xMax, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Min", DataField.yMin, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("y_Max", DataField.yMax, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Min", DataField.zMin, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("z_Max", DataField.zMax, FieldType.double_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("radius", DataField.radius, FieldType.double_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class mapUniverse
    Public Property universeID As Object
    Public Property universeName As Object
    Public Property x As Object
    Public Property y As Object
    Public Property z As Object
    Public Property xMin As Object
    Public Property xMax As Object
    Public Property yMin As Object
    Public Property yMax As Object
    Public Property zMin As Object
    Public Property zMax As Object
    Public Property radius As Object
End Class