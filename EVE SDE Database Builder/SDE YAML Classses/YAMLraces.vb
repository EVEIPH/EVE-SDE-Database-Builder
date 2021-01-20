
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLraces
    Inherits YAMLFilesBase

    Public Const racesFile As String = "races.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, race)
        Dim DataFields As List(Of DBField)
        Dim TranslatedField As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("raceID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("raceName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("raceDescription", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("iconID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("shipTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        Dim RaceSkillsTableName As String = "raceSkills"
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("raceID", FieldType.int_type, 0, False))
        Table.Add(New DBTableField("skillTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("level", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(RaceSkillsTableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, race))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", DataField.Key, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceName", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceDescription", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shipTypeID", .shipTypeID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            If Not IsNothing(DataField.Value.skills) Then
                For Each Skill In DataField.Value.skills
                    DataFields = New List(Of DBField)
                    DataFields.Add(UpdateDB.BuildDatabaseField("raceID", DataField.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skillTypeID", Skill.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("level", Skill.Value, FieldType.int_type))
                    Call UpdateDB.InsertRecord(RaceSkillsTableName, DataFields)
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class race
    Public Property raceID As Object
    Public Property nameID As Translations
    Public Property descriptionID As Translations
    Public Property iconID As Object
    Public Property shipTypeID As Integer
    Public Property skills As Dictionary(Of Long, Integer) ' typeID and level
End Class