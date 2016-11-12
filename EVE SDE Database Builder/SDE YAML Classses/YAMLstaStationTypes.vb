
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLstaStationTypes
    Inherits YAMLFilesBase

    Public Const staStationTypesFile As String = "staStationTypes.yaml"

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

        Dim YAMLRecords As New List(Of staStationType)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("stationTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("dockEntryX", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("dockEntryY", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("dockEntryZ", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("dockOrientationX", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("dockOrientationY", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("dockOrientationZ", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("operationID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("officeSlots", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("reprocessingEfficiency", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("conquerable", FieldType.bit_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of staStationType))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("stationTypeID", DataField.stationTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockEntryX", DataField.dockEntryX, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockEntryY", DataField.dockEntryY, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockEntryZ", DataField.dockEntryZ, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockOrientationX", DataField.dockOrientationX, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockOrientationY", DataField.dockOrientationY, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dockOrientationZ", DataField.dockOrientationZ, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.operationID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("officeSlots", DataField.officeSlots, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingEfficiency", DataField.reprocessingEfficiency, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("conquerable", DataField.conquerable, FieldType.bit_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class staStationType
    Public Property stationTypeID As Object
    Public Property dockEntryX As Object
    Public Property dockEntryY As Object
    Public Property dockEntryZ As Object
    Public Property dockOrientationX As Object
    Public Property dockOrientationY As Object
    Public Property dockOrientationZ As Object
    Public Property operationID As Object
    Public Property officeSlots As Object
    Public Property reprocessingEfficiency As Object
    Public Property conquerable As Object

End Class