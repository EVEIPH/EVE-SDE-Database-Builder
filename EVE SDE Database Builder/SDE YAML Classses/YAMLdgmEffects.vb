
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLdgmEffects
    Inherits YAMLFilesBase

    Public Const dgmEffectsFile As String = "dgmEffects.yaml"
    Public Const dgmEffectsModifierInfoTable As String = "dgmEffectsModifierInfo" ' Special table to store modifierInfo string, which is in yaml

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

        Dim TempModInfoDB As New LocalDatabase

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

        Call UpdateDB.CreateTable(TableName, Table)

        Table = New List(Of DBTableField)

        Table.Add(New DBTableField("effectID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("func", FieldType.varchar_type, 50, True))
        Table.Add(New DBTableField("modifiedAttributeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("modifyingAttributeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("operator", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(dgmEffectsModifierInfoTable, Table)

        ' Create unique index
        Dim IndexFields As New List(Of String)
        IndexFields.Add("effectID")
        IndexFields.Add("func")
        IndexFields.Add("modifiedAttributeID")
        IndexFields.Add("modifyingAttributeID")
        IndexFields.Add("operator")
        Call UpdateDB.CreateIndex(dgmEffectsModifierInfoTable, "IDX_" & dgmEffectsModifierInfoTable & "_EID", IndexFields, True)

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

            Call UpdateDB.InsertRecord(TableName, DataFields)

            If Not IsNothing(DataField.modifierInfo) Then
                ' Store modifier string data into the new table
                Dim DataFields2 As New List(Of DBField)
                Dim ModifierRecord As New List(Of dgmEffectModifierInfo)

                ' Parse out the data first
                ModifierRecord = DS.Deserialize(Of List(Of dgmEffectModifierInfo))(DataField.modifierInfo)

                If Not IsNothing(ModifierRecord) Then
                    For Each Record In ModifierRecord
                        DataFields2 = New List(Of DBField)

                        DataFields2.Add(UpdateDB.BuildDatabaseField("effectID", DataField.effectID, FieldType.smallint_type))
                        DataFields2.Add(UpdateDB.BuildDatabaseField("func", Record.func, FieldType.varchar_type))
                        DataFields2.Add(UpdateDB.BuildDatabaseField("modifiedAttributeID", Record.modifiedAttributeID, FieldType.int_type))
                        DataFields2.Add(UpdateDB.BuildDatabaseField("modifyingAttributeID", Record.modifyingAttributeID, FieldType.int_type))
                        DataFields2.Add(UpdateDB.BuildDatabaseField("operator", Record.operator, FieldType.int_type))

                        ' Check for duplicates
                        If Not RecordinModinfoTable(TempModInfoDB.GetDataTable(dgmEffectsModifierInfoTable), DataFields2) Then
                            Call TempModInfoDB.InsertRecord(dgmEffectsModifierInfoTable, DataFields2)
                            Call UpdateDB.InsertRecord(dgmEffectsModifierInfoTable, DataFields2)
                        End If

                    Next
                End If
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Function RecordinModinfoTable(ByRef ModInfoTableRef As DataTable, ByVal Record As List(Of DBField)) As Boolean
        Dim SearchRow() As DataRow
        Dim SearchString As String = ""
        Dim TempFieldValue As String = ""

        If ModInfoTableRef.Rows.Count > 0 Then

            For i = 0 To Record.Count - 1
                If Record(i).FieldValue <> NullValue And Record(i).FieldValue <> "" Then
                    ' If the search string is formatted for Excel input, then strip the second set of quotes
                    If Record(i).FieldValue.Substring(0, 1) = """" Then
                        TempFieldValue = "'" & Record(i).FieldValue.Substring(1, Len(Record(i).FieldValue) - 2) & "'"
                    Else
                        TempFieldValue = Record(i).FieldValue
                    End If
                    SearchString &= String.Format("{0} = {1}", Record(i).FieldName, TempFieldValue)
                    SearchString &= " AND "
                End If
            Next

            ' Strip last AND
            SearchString = SearchString.Substring(0, Len(SearchString) - 5)

            SearchRow = ModInfoTableRef.Select(SearchString)

            If SearchRow.Count > 0 Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function

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

Public Class dgmEffectModifierInfo
    Public Property domain As Object
    Public Property func As Object
    Public Property modifiedAttributeID As Object
    Public Property modifyingAttributeID As Object
    Public Property [operator] As Object
End Class
