
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLfactions
    Inherits YAMLFilesBase

    Public Const factionsFile As String = "factions.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = factionsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, faction)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("factionID", FieldType.int_type, 0, False, True),
            New DBTableField("factionName", FieldType.varchar_type, 100, True),
            New DBTableField("description", FieldType.varchar_type, 1500, True),
            New DBTableField("shortDescriptionID", FieldType.varchar_type, 500, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("corporationID", FieldType.int_type, 0, True),
            New DBTableField("sizeFactor", FieldType.real_type, 0, True),
            New DBTableField("militiaCorporationID", FieldType.int_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("uniqueName", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim MemberRacesTableName As String = "factionsMemberRaces"
        Table = New List(Of DBTableField) From {
            New DBTableField("factionID", FieldType.int_type, 0, False),
            New DBTableField("memberRace", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(MemberRacesTableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, faction))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionName", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shortDescriptionID", NameTranslation.GetLanguageTranslationData(.shortDescriptionID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", .corporationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sizeFactor", .sizeFactor, FieldType.real_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("militiaCorporationID", .militiaCorporationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", .uniqueName, FieldType.int_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "factionID", "factionName", TableName, NameTranslation.GetAllTranslations(.nameID))
                Call Translator.InsertTranslationData(DataField.Key, "factionID", "description", TableName, NameTranslation.GetAllTranslations(.descriptionID))
                Call Translator.InsertTranslationData(DataField.Key, "factionID", "shortDescriptionID", TableName, NameTranslation.GetAllTranslations(.shortDescriptionID))

            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            For Each MR In DataField.Value.memberRaces
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("factionID", DataField.Key, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("memberRace", MR, FieldType.int_type)
                }
                Call UpdateDB.InsertRecord(MemberRacesTableName, DataFields)
            Next

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        YAMLRecords.Clear()

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class faction
    Public Property corporationID As Object
    Public Property descriptionID As Translations
    Public Property iconID As Object
    Public Property memberRaces As List(Of Object)
    Public Property militiaCorporationID As Object
    Public Property nameID As Translations
    Public Property shortDescriptionID As Translations
    Public Property sizeFactor As Object
    Public Property solarSystemID As Object
    Public Property uniqueName As Object
End Class