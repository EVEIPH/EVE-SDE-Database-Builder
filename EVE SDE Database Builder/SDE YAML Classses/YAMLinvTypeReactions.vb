
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLinvTypeReactions
    Inherits YAMLFilesBase

    Public Const invTypeReactionsFile As String = "invTypeReactions.yaml"

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

        Dim YAMLRecords As New List(Of invTypeReaction)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("reactionTypeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("input", FieldType.bit_type, 0, False, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("quantity", FieldType.smallint_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of invTypeReaction))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("reactionTypeID", DataField.reactionTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("input", DataField.input, FieldType.bit_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.typeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("quantity", DataField.quantity, FieldType.smallint_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class invTypeReaction
    Public Property reactionTypeID As Object
    Public Property input As Object
    Public Property typeID As Object
    Public Property quantity As Object
End Class