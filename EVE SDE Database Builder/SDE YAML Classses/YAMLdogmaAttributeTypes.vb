

Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLdogmaAttributeTypes
    Inherits YAMLFilesBase

    Public Const dogmaAttributeTypesFile As String = "dogmaAttributes.yaml"

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

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        Dim YAMLRecords As New Dictionary(Of Long, dogmaAttributeType)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("attributeID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("attributeName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("displayNameID", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("dataType", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("defaultValue", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("published", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("stackable", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("name", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("unitID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("highIsGood", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("categoryID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("tooltipDescriptionID", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("tooltipTitleID", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("maxAttributeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("chargeRechargeTimeID", FieldType.smallint_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, dogmaAttributeType))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            With DataField.Value
                DataFields = New List(Of DBField)

                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("attributeID", .attributeID, FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("attributeName", .name, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("displayNameID", NameTranslation.GetLanguageTranslationData(.displayNameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("dataType", .dataType, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("defaultValue", .defaultValue, FieldType.float_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", .published, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("stackable", .stackable, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", .name, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("unitID", .unitID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("highIsGood", .highIsGood, FieldType.bit_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("categoryID", .categoryID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("tooltipDescriptionID", NameTranslation.GetLanguageTranslationData(.tooltipDescriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("tooltipTitleID", NameTranslation.GetLanguageTranslationData(.tooltipTitleID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maxAttributeID", .maxAttributeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("chargeRechargeTimeID", .chargeRechargeTimeID, FieldType.smallint_type))

                Call UpdateDB.InsertRecord(TableName, DataFields)

                ' Update grid progress
                Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
                Count += 1
            End With
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class dogmaAttributeType
    Public Property attributeID As Object
    Public Property categoryID As Object
    Public Property chargeRechargeTimeID As Object
    Public Property dataType As Object
    Public Property defaultValue As Object
    Public Property description As Object
    Public Property displayNameID As Translations
    Public Property highIsGood As Object
    Public Property iconID As Object
    Public Property maxAttributeID As Object
    Public Property name As Object
    Public Property published As Object
    Public Property stackable As Object
    Public Property tooltipDescriptionID As Translations
    Public Property tooltipTitleID As Translations
    Public Property unitID As Object
End Class