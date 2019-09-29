
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLdgmTypeAttributes
    Inherits YAMLFilesBase

    Public Const dgmTypeAttributesFile As String = "dgmTypeAttributes.yaml"

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

        Dim YAMLRecords As New List(Of dgmTypeAttribute)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("attributeID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("valueInt", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("valueFloat", FieldType.float_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String)
        IndexFields.Add("typeID")
        IndexFields.Add("attributeID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID_AID", IndexFields)


        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of dgmTypeAttribute))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.typeID, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("attributeID", DataField.attributeID, FieldType.smallint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("valueInt", DataField.valueInt, FieldType.int_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("valueFloat", DataField.valueFloat, FieldType.float_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class dgmTypeAttribute
    Public Property typeID As Object
    Public Property attributeID As Object
    Public Property valueInt As Object
    Public Property valueFloat As Object
End Class