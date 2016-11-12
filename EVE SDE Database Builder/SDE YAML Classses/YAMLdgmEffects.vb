
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLdgmEffects
    Inherits YAMLFilesBase

    Public Const dgmEffectsFile As String = "dgmEffects.yaml"

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

        Dim YAMLRecords As New List(Of dgmEffect)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("effectID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("effectName", FieldType.varchar_type, 400, True))
        Table.Add(New DBTableField("effectCategory", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("preExpression", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("postExpression", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("description", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("guid", FieldType.varchar_type, 60, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("isOffensive", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("isAssistance", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("durationAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("trackingSpeedAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("dischargeAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("rangeAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("falloffAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("disallowAutoRepeat", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("published", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("displayName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("isWarpSafe", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("rangeChance", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("electronicChance", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("propulsionChance", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("distribution", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("sfxName", FieldType.varchar_type, 20, True))
        Table.Add(New DBTableField("npcUsageChanceAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("npcActivationChanceAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("fittingUsageChanceAttributeID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("modifierInfo", FieldType.varchar_type, -1, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of dgmEffect))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("effectID", DataField.effectID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("effectName", DataField.effectName, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("effectCategory", DataField.effectCategory, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("preExpression", DataField.preExpression, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("postExpression", DataField.postExpression, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", Translator.TranslateData(TableName, "description", "effectID", DataField.effectID, Params.ImportLanguageCode, DataField.description), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("guid", DataField.guid, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("iconID", DataField.iconID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("isOffensive", DataField.isOffensive, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("isAssistance", DataField.isAssistance, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("durationAttributeID", DataField.durationAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("trackingSpeedAttributeID", DataField.trackingSpeedAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("dischargeAttributeID", DataField.dischargeAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("rangeAttributeID", DataField.rangeAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("falloffAttributeID", DataField.falloffAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("disallowAutoRepeat", DataField.disallowAutoRepeat, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("published", DataField.published, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("displayName", Translator.TranslateData(TableName, "displayName", "effectID", DataField.effectID, Params.ImportLanguageCode, DataField.displayName), FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("isWarpSafe", DataField.isWarpSafe, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("rangeChance", DataField.rangeChance, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("electronicChance", DataField.electronicChance, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("propulsionChance", DataField.propulsionChance, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("distribution", DataField.distribution, FieldType.tinyint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("sfxName", DataField.sfxName, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("npcUsageChanceAttributeID", DataField.npcUsageChanceAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("npcActivationChanceAttributeID", DataField.npcActivationChanceAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fittingUsageChanceAttributeID", DataField.fittingUsageChanceAttributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("modifierInfo", DataField.modifierInfo, FieldType.varchar_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class dgmEffect

    Public Property effectID As Object
    Public Property effectName As Object
    Public Property effectCategory As Object
    Public Property preExpression As Object
    Public Property postExpression As Object
    Public Property description As Object
    Public Property guid As Object
    Public Property iconID As Object
    Public Property isOffensive As Object
    Public Property isAssistance As Object
    Public Property durationAttributeID As Object
    Public Property trackingSpeedAttributeID As Object
    Public Property dischargeAttributeID As Object
    Public Property rangeAttributeID As Object
    Public Property falloffAttributeID As Object
    Public Property disallowAutoRepeat As Object
    Public Property published As Object
    Public Property displayName As Object
    Public Property isWarpSafe As Object
    Public Property rangeChance As Object
    Public Property electronicChance As Object
    Public Property propulsionChance As Object
    Public Property distribution As Object
    Public Property sfxName As Object
    Public Property npcUsageChanceAttributeID As Object
    Public Property npcActivationChanceAttributeID As Object
    Public Property fittingUsageChanceAttributeID As Object
    Public Property modifierInfo As Object

End Class