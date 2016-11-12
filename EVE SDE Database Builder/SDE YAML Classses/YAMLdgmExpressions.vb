
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLdgmExpressions
    Inherits YAMLFilesBase

    Public Const dgmExpressionsFile As String = "dgmExpressions.yaml"

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

        Dim YAMLRecords As New List(Of dgmExpression)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("expressionID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("operandID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("arg1", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("arg2", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("expressionValue", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.varchar_type, 1000, True))
        Table.Add(New DBTableField("expressionName", FieldType.varchar_type, 500, True))
        Table.Add(New DBTableField("expressionTypeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("expressionGroupID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("expressionAttributeID", FieldType.smallint_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of dgmExpression))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("expressionID", DataField.expressionID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("operandID", DataField.operandID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("arg1", DataField.arg1, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("arg2", DataField.arg2, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("expressionValue", DataField.expressionValue, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("description", DataField.description, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("expressionName", DataField.expressionName, FieldType.varchar_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("expressionTypeID", DataField.expressionTypeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("expressionGroupID", DataField.expressionGroupID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("expressionAttributeID", DataField.expressionAttributeID, FieldType.smallint_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class dgmExpression
    Public Property expressionID As Object
    Public Property operandID As Object
    Public Property arg1 As Object
    Public Property arg2 As Object
    Public Property expressionValue As Object
    Public Property description As Object
    Public Property expressionName As Object
    Public Property expressionTypeID As Object
    Public Property expressionGroupID As Object
    Public Property expressionAttributeID As Object
End Class