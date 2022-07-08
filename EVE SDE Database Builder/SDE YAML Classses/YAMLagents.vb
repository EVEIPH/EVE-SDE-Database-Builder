
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLagents
    Inherits YAMLFilesBase

    Public Const agentsFile As String = "agents.yaml"

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
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, agtAgent)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("agentID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("divisionID", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("corporationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("locationID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("level", FieldType.tinyint_type, 0, True))
        Table.Add(New DBTableField("agentTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("isLocator", FieldType.bit_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("corporationID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        IndexFields = New List(Of String)
        IndexFields.Add("locationID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_LID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, agtAgent))(New StringReader(File.ReadAllText(YAMLFile)))
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
                DataFields.Add(UpdateDB.BuildDatabaseField("divisionID", .divisionID, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", .corporationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("locationID", .locationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("level", .level, FieldType.tinyint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("agentTypeID", .agentTypeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("isLocator", .isLocator, FieldType.bit_type))
            End With
            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class agtAgent
    Public Property divisionID As Object
    Public Property corporationID As Object
    Public Property locationID As Object
    Public Property level As Object
    Public Property quality As Object
    Public Property agentTypeID As Object
    Public Property isLocator As Object
End Class