
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLagentsinSpace
    Inherits YAMLFilesBase

    Public Const agentsinSpaceFile As String = "agentsInSpace.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, agentInSpace)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("agentID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("dungeonID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("solarSystemID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("spawnPointID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("agentID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_AID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("solarSystemID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SSID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, agentInSpace))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("agentID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("dungeonID", .dungeonID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("spawnPointID", .spawnPointID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class agentInSpace
    Public Property dungeonID As Object
    Public Property solarSystemID As Object
    Public Property spawnPointID As Object
    Public Property typeID As Object
End Class