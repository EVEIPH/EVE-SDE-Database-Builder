
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLlandmarks
    Inherits YAMLFilesBase

    Public Const landmarksFile As String = "landmarks.staticdata"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
        ' Reset Table Name
        TableName = "mapLandmarks"
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

        Dim YAMLRecords As New Dictionary(Of Long, landmark)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("landmarkID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("descriptionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("description", FieldType.text_type, -1, True)) ' Field doesn't exist in YAML, but will get from translation table
        Table.Add(New DBTableField("landmarkNameID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("landmarkName", FieldType.nvarchar_type, 100, True)) ' Field doesn't exist in YAML, but will get from translation table
        Table.Add(New DBTableField("locationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("x", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("y", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("z", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, landmark))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("landmarkID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("descriptionID", .descriptionID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "landmarkID", DataField.Key, Params.ImportLanguageCode, ""), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("landmarkNameID", .landmarkNameID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("landmarkName", Translator.TranslateData(TableName, "landmarkName", "landmarkID", DataField.Key, Params.ImportLanguageCode, ""), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("locationID", .locationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position(0), FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position(1), FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position(2), FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class landmark
    Public Property descriptionID As Object
    Public Property iconID As Object
    Public Property landmarkNameID As Object
    Public Property locationID As Object
    Public Property position As List(Of Object)
End Class