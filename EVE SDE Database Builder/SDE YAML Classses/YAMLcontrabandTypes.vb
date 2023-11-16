
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLcontrabandTypes
    Inherits YAMLFilesBase

    Public Const contrabandTypesFile As String = "contrabandTypes.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = contrabandTypesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, contrabandData)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("attackMinSec", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("confiscateMinSec", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("fineByValue", FieldType.real_type, 0, True))
        Table.Add(New DBTableField("standingLoss", FieldType.real_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As New List(Of String)
        IndexFields.Add("typeID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, contrabandData))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            With DataField.Value
                If .factions.Count > 0 Then
                    For Each resource In DataField.Value.factions
                        DataFields = New List(Of DBField)
                        ' Build the insert list
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("factionID", resource.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("attackMinSec", resource.Value.attackMinSec, FieldType.real_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("confiscateMinSec", resource.Value.confiscateMinSec, FieldType.real_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("fineByValue", resource.Value.fineByValue, FieldType.real_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("standingLoss", resource.Value.standingLoss, FieldType.real_type))

                        Call UpdateDB.InsertRecord(TableName, DataFields)
                    Next
                End If
            End With

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

Public Class contrabandData
    Public Property factions As New Dictionary(Of Long, contrabandEffectData)
End Class

Public Class contrabandEffectData
    Public Property attackMinSec As Object
    Public Property confiscateMinSec As Object
    Public Property fineByValue As Object
    Public Property standingLoss As Object
End Class