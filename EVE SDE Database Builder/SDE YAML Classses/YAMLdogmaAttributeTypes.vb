

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
        FileNameErrorTracker = dogmaAttributeTypesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        Dim YAMLRecords As New Dictionary(Of Long, dogmaAttributeType)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("attributeID", FieldType.smallint_type, 0, False), ' Dupes ok in uprising expansion
            New DBTableField("attributeName", FieldType.varchar_type, 100, True),
            New DBTableField("description", FieldType.varchar_type, 1000, True),
            New DBTableField("displayNameID", FieldType.varchar_type, 1000, True),
            New DBTableField("dataType", FieldType.varchar_type, 100, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("defaultValue", FieldType.real_type, 0, True),
            New DBTableField("published", FieldType.bit_type, 0, True),
            New DBTableField("stackable", FieldType.bit_type, 0, True),
            New DBTableField("name", FieldType.varchar_type, 1000, True),
            New DBTableField("unitID", FieldType.tinyint_type, 0, True),
            New DBTableField("highIsGood", FieldType.bit_type, 0, True),
            New DBTableField("categoryID", FieldType.tinyint_type, 0, True),
            New DBTableField("tooltipDescriptionID", FieldType.varchar_type, 1000, True),
            New DBTableField("tooltipTitleID", FieldType.varchar_type, 1000, True),
            New DBTableField("maxAttributeID", FieldType.int_type, 0, True),
            New DBTableField("chargeRechargeTimeID", FieldType.smallint_type, 0, True),
            New DBTableField("displayWhenZero", FieldType.bit_type, 0, True),
            New DBTableField("minAttributeID", FieldType.int_type, 0, True)
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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, dogmaAttributeType))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            With DataField.Value
                ' Build the insert list
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("attributeID", .attributeID, FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("attributeName", .name, FieldType.varchar_type),
                    UpdateDB.BuildDatabaseField("description", .description, FieldType.varchar_type),
                    UpdateDB.BuildDatabaseField("displayNameID", NameTranslation.GetLanguageTranslationData(.displayNameID), FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("dataType", .dataType, FieldType.varchar_type),
                    UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("defaultValue", .defaultValue, FieldType.real_type),
                    UpdateDB.BuildDatabaseField("published", .published, FieldType.bit_type),
                    UpdateDB.BuildDatabaseField("stackable", .stackable, FieldType.bit_type),
                    UpdateDB.BuildDatabaseField("name", .name, FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("unitID", .unitID, FieldType.tinyint_type),
                    UpdateDB.BuildDatabaseField("highIsGood", .highIsGood, FieldType.bit_type),
                    UpdateDB.BuildDatabaseField("categoryID", .categoryID, FieldType.tinyint_type),
                    UpdateDB.BuildDatabaseField("tooltipDescriptionID", NameTranslation.GetLanguageTranslationData(.tooltipDescriptionID), FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("tooltipTitleID", NameTranslation.GetLanguageTranslationData(.tooltipTitleID), FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("maxAttributeID", .maxAttributeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("chargeRechargeTimeID", .chargeRechargeTimeID, FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("displayWhenZero", .displayWhenZero, FieldType.bit_type),
                    UpdateDB.BuildDatabaseField("minAttributeID", .minAttributeID, FieldType.int_type)
                }

                Call UpdateDB.InsertRecord(TableName, DataFields)

                ' Update grid progress
                Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
                Count += 1
            End With
        Next

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "attributeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_AID", IndexFields)

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
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
    Public Property displayWhenZero As Object
    Public Property minAttributeID As Object
End Class