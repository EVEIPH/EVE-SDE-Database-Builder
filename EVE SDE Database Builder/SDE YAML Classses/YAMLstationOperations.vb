
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLstationOperations
    Inherits YAMLFilesBase

    Public Const stationOperationsFile As String = "stationOperations.yaml"

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
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, stationOperation)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("operationID", FieldType.tinyint_type, 0, False, True))
        Table.Add(New DBTableField("operationName", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("activityID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("border", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("corridor", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("description", FieldType.nvarchar_type, 1000, True))
        Table.Add(New DBTableField("fringe", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("hub", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("manufacturingFactor", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("ratio", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("researchFactor", FieldType.real_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        Dim StationOperationServicesTableName As String = "stationOperationServices"
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("operationID", FieldType.int_type, 0, False))
        Table.Add(New DBTableField("serviceID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(StationOperationServicesTableName, Table)

        Dim StationOperationTypesTableName As String = "stationOperationTypes"
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("operationID", FieldType.int_type, 0, False))
        Table.Add(New DBTableField("raceID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("stationTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(StationOperationTypesTableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, stationOperation))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.Key, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("operationName", NameTranslation.GetLanguageTranslationData(.operationNameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("activityID", .activityID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("border", .border, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corridor", .corridor, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fringe", .fringe, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("hub", .hub, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("manufacturingFactor", .manufacturingFactor, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ratio", .ratio, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("researchFactor", .researchFactor, FieldType.real_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "operationID", "operationName", TableName, NameTranslation.GetAllTranslations(.operationNameID))
                Call Translator.InsertTranslationData(DataField.Key, "operationID", "description", TableName, NameTranslation.GetAllTranslations(.descriptionID))

            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            If Not IsNothing(DataField.Value.services) Then
                For Each Service In DataField.Value.services
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("serviceID", Service, FieldType.int_type))
                    Call UpdateDB.InsertRecord(StationOperationServicesTableName, DataFields)
                Next
            End If

            If Not IsNothing(DataField.Value.stationTypes) Then
                For Each StationType In DataField.Value.stationTypes
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("raceID", StationType.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("stationTypeID", StationType.Value, FieldType.int_type))
                    Call UpdateDB.InsertRecord(StationOperationTypesTableName, DataFields)
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class stationOperation
    Public Property activityID As Object
    Public Property border As Object
    Public Property corridor As Object
    Public Property descriptionID As Translations
    Public Property fringe As Object
    Public Property hub As Object
    Public Property manufacturingFactor As Object
    Public Property operationNameID As Translations
    Public Property ratio As Object
    Public Property researchFactor As Object
    Public Property services As List(Of Integer) ' new table
    Public Property stationTypes As Dictionary(Of Long, Long) ' new table
End Class