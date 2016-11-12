
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLstaOperations
    Inherits YAMLFilesBase

    Public Const staOperationsFile As String = "staOperations.yaml"

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

        Dim YAMLRecords As New List(Of staOperation)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("activityID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("operationID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("operationName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("fringe", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("corridor", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("hub", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("border", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("ratio", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("caldariStationTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("minmatarStationTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("amarrStationTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("gallenteStationTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("joveStationTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of staOperation))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("activityID", DataField.activityID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.operationID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("operationName", Translator.TranslateData(TableName, "operationName", "activityID", DataField.activityID, Params.ImportLanguageCode, DataField.operationName), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "activityID", DataField.activityID, Params.ImportLanguageCode, DataField.description), FieldType.nvarchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fringe", DataField.fringe, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("corridor", DataField.corridor, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("hub", DataField.hub, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("border", DataField.border, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("ratio", DataField.ratio, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("caldariStationTypeID", DataField.caldariStationTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("minmatarStationTypeID", DataField.minmatarStationTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("amarrStationTypeID", DataField.amarrStationTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("gallenteStationTypeID", DataField.gallenteStationTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("joveStationTypeID", DataField.joveStationTypeID, FieldType.int_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class staOperation
    Public Property activityID As Object
    Public Property operationID As Object
    Public Property operationName As Object
    Public Property description As Object
    Public Property fringe As Object
    Public Property corridor As Object
    Public Property hub As Object
    Public Property border As Object
    Public Property ratio As Object
    Public Property caldariStationTypeID As Object
    Public Property minmatarStationTypeID As Object
    Public Property amarrStationTypeID As Object
    Public Property gallenteStationTypeID As Object
    Public Property joveStationTypeID As Object
End Class