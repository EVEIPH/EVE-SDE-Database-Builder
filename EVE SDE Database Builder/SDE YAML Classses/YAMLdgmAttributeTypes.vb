

Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLdgmAttributeTypes
    Inherits YAMLFilesBase

    Public Const dgmAttributeTypesFile As String = "dgmAttributeTypes.yaml"

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

        Dim YAMLRecords As New List(Of dgmAttributeType)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("attributeID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("attributeName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("defaultValue", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("published", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("displayName", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("unitID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("stackable", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("highIsGood", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("categoryID", FieldType.tinyint_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of dgmAttributeType))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("attributeID", DataField.attributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("attributeName", DataField.attributeName, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", DataField.description, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("defaultValue", DataField.defaultValue, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("published", DataField.published, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("displayName", Translator.TranslateData(TableName, "displayName", "attributeID", DataField.attributeID, Params.ImportLanguageCode, DataField.displayName), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("unitID", DataField.unitID, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("stackable", DataField.stackable, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("highIsGood", DataField.highIsGood, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("categoryID", DataField.categoryID, FieldType.tinyint_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class dgmAttributeType
    Public Property attributeID As Object
    Public Property attributeName As Object
    Public Property description As Object
    Public Property iconID As Object
    Public Property defaultValue As Object
    Public Property published As Object
    Public Property displayName As Object
    Public Property unitID As Object
    Public Property stackable As Object
    Public Property highIsGood As Object
    Public Property categoryID As Object

End Class