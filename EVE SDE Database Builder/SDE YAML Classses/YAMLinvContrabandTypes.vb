
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLinvContrabandTypes
    Inherits YAMLFilesBase

    Public Const invContrabandTypesFile As String = "invContrabandTypes.yaml"

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

        Dim YAMLRecords As New List(Of invContrabandType)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("factionID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("standingLoss", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("confiscateMinSec", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("fineByValue", FieldType.float_type, 0, True))
        Table.Add(New DBTableField("attackMinSec", FieldType.float_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String)
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
            YAMLRecords = DS.Deserialize(Of List(Of invContrabandType))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("factionID", DataField.factionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.typeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("standingLoss", DataField.standingLoss, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("confiscateMinSec", DataField.confiscateMinSec, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("fineByValue", DataField.fineByValue, FieldType.float_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("attackMinSec", DataField.attackMinSec, FieldType.float_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class invContrabandType
    Public Property factionID As Object
    Public Property typeID As Object
    Public Property standingLoss As Object
    Public Property confiscateMinSec As Object
    Public Property fineByValue As Object
    Public Property attackMinSec As Object
End Class