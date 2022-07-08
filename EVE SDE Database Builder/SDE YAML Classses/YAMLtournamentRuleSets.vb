
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLtournamentRuleSets
    Inherits YAMLFilesBase

    Public Const tournamentRuleSetsFile As String = "tournamentRuleSets.yaml"

    Private Const tntTournaments_Table As String = "tntTournaments"
    Private Const tntTournamentTypePoints_Table As String = "tntTournamentTypePoints"
    Private Const tntTournamentGroupPoints_Table As String = "tntTournamentGroupPoints"
    Private Const tntTournamentBannedTypes_Table As String = "tntTournamentBannedTypes"
    Private Const tntTournamentBannedGroups_Table As String = "tntTournamentBannedGroups"

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
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of tournamentRuleSet)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build all the tables to insert tournament rule set data into. This includes the following tables:
        ' - tntTournaments
        ' - tntTournamentTypePoints
        ' - tntTournamentGroupPoints
        ' - tntTournamentBannedTypes
        ' - tntTournamentBannedGroups
        Call BuildTournamentsTable()
        Call BuildTournamentTypePointsTable()
        Call BuildTournamentGroupPointsTable()
        Call BuildTournamentBannedTypesTable()
        Call BuildTournamentBannedGroupsTable()

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of tournamentRuleSet))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            With DataField
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("ruleSetID", .ruleSetID, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ruleSetName", .ruleSetName, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maximumPointsMatch", .maximumPointsMatch, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maximumPilotsMatch", .maximumPilotsMatch, FieldType.int_type))

                Call UpdateDB.InsertRecord(tntTournaments_Table, DataFields)

                ' Add the type and group data for points and banned
                Call InsertTournamentGroupTypePoints(.ruleSetID, .points)
                Call InsertTournamentBannedGroupsTypes(.ruleSetID, .banned)

            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub BuildTournamentsTable()
        Dim Table As New List(Of DBTableField)

        Table.Add(New DBTableField("ruleSetID", FieldType.varchar_type, 100, False, True))
        Table.Add(New DBTableField("ruleSetName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("maximumPointsMatch", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("maximumPilotsMatch", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(tntTournaments_Table, Table)

    End Sub

    Private Sub BuildTournamentTypePointsTable()
        Dim Table As New List(Of DBTableField)
        Dim IndexFields As List(Of String)

        Table.Add(New DBTableField("ruleSetID", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("points", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(tntTournamentTypePoints_Table, Table)

        ' Create index
        IndexFields = New List(Of String)
        IndexFields.Add("ruleSetID")
        Call UpdateDB.CreateIndex(tntTournamentTypePoints_Table, "IDX_" & tntTournamentTypePoints_Table & "_RSID", IndexFields)

    End Sub

    Private Sub BuildTournamentGroupPointsTable()
        Dim Table As New List(Of DBTableField)
        Dim IndexFields As List(Of String)

        Table.Add(New DBTableField("ruleSetID", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("points", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(tntTournamentGroupPoints_Table, Table)

        ' Create index
        IndexFields = New List(Of String)
        IndexFields.Add("ruleSetID")
        Call UpdateDB.CreateIndex(tntTournamentGroupPoints_Table, "IDX_" & tntTournamentGroupPoints_Table & "_RSID", IndexFields)

    End Sub

    Private Sub BuildTournamentBannedTypesTable()
        Dim Table As New List(Of DBTableField)
        Dim IndexFields As List(Of String)

        Table.Add(New DBTableField("ruleSetID", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(tntTournamentBannedTypes_Table, Table)

        ' Create index
        IndexFields = New List(Of String)
        IndexFields.Add("ruleSetID")
        Call UpdateDB.CreateIndex(tntTournamentBannedTypes_Table, "IDX_" & tntTournamentBannedTypes_Table & "_RSID", IndexFields)

    End Sub

    Private Sub BuildTournamentBannedGroupsTable()
        Dim Table As New List(Of DBTableField)
        Dim IndexFields As List(Of String)

        Table.Add(New DBTableField("ruleSetID", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("groupID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(tntTournamentBannedGroups_Table, Table)

        ' Create index
        IndexFields = New List(Of String)
        IndexFields.Add("ruleSetID")
        Call UpdateDB.CreateIndex(tntTournamentBannedGroups_Table, "IDX_" & tntTournamentBannedGroups_Table & "_RSID", IndexFields)

    End Sub

    Private Sub InsertTournamentGroupTypePoints(ByVal RuleSetID As String, ByVal TypePoints As tournamentRuleSet.tournamentPoints)
        Dim DataFields As New List(Of DBField)

        ' Insert groups first
        For Each record In TypePoints.groups
            DataFields = New List(Of DBField)
            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("ruleSetID", RuleSetID, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("groupID", record.groupID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("points", record.points, FieldType.int_type))

            Call UpdateDB.InsertRecord(tntTournamentGroupPoints_Table, DataFields)
        Next

        ' Now types
        For Each record In TypePoints.types
            DataFields = New List(Of DBField)
            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("ruleSetID", RuleSetID, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("typeID", record.typeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("points", record.points, FieldType.int_type))

            Call UpdateDB.InsertRecord(tntTournamentTypePoints_Table, DataFields)
        Next
    End Sub

    Private Sub InsertTournamentBannedGroupsTypes(ByVal RuleSetID As String, ByVal BannedGTs As tournamentRuleSet.bannedGroupsTypes)
        Dim DataFields As New List(Of DBField)

        ' Insert groups first
        For Each GroupID In BannedGTs.groups
            DataFields = New List(Of DBField)
            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("ruleSetID", RuleSetID, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("groupID", GroupID, FieldType.int_type))

            Call UpdateDB.InsertRecord(tntTournamentBannedGroups_Table, DataFields)
        Next

        ' Now types
        For Each TypeID In BannedGTs.types
            DataFields = New List(Of DBField)
            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("ruleSetID", RuleSetID, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("typeID", TypeID, FieldType.int_type))

            Call UpdateDB.InsertRecord(tntTournamentBannedTypes_Table, DataFields)
        Next

    End Sub

End Class

Public Class tournamentRuleSet
    Public Property ruleSetID As Object
    Public Property ruleSetName As Object
    Public Property points As tournamentPoints
    Public Property maximumPointsMatch As Object
    Public Property maximumPilotsMatch As Object
    Public Property banned As bannedGroupsTypes

    Public Class tournamentPoints
        Public Property types As List(Of tournamentTypePoint)
        Public Property groups As List(Of tournamentGroupPoint)
    End Class

    Public Class tournamentTypePoint
        Public Property points As Object
        Public Property typeID As Object
    End Class

    Public Class tournamentGroupPoint
        Public Property points As Object
        Public Property groupID As Object
    End Class

    Public Class bannedGroupsTypes
        Public Property types As List(Of Object)
        Public Property groups As List(Of Object)
    End Class

End Class

