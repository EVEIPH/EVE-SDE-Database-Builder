
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLinvNames
    Inherits YAMLFilesBase

    Public Const invNamesFile As String = "invNames.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' Returns a list of invName of all the data for use
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Function ImportFile(ByVal Params As ImportParameters) As List(Of invName)
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of invName)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table unless we just want the data
        If Not Params.ReturnList Then
            Dim Table As New List(Of DBTableField)
            Table.Add(New DBTableField("itemID", FieldType.bigint_type, 0, False, True))
            Table.Add(New DBTableField("itemName", FieldType.nvarchar_type, 200, True))

            Call UpdateDB.CreateTable(TableName, Table)

            ' See if we only want to build the table and indexes
            If Not Params.InsertRecords Then
                Return Nothing
            End If

            ' Start processing
            Call InitGridRow(Params.RowLocation)
        End If

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of invName))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        If Params.ReturnList Then
            Return YAMLRecords
        End If

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(UpdateDB.BuildDatabaseField("itemID", DataField.itemID, FieldType.bigint_type))
            DataFields.Add(UpdateDB.BuildDatabaseField("itemName", DataField.itemName, FieldType.nvarchar_type))

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

        YAMLRecords.Clear()
        YAMLRecords = Nothing

        Return YAMLRecords

    End Function

End Class

Public Class invName
    Public Property itemID As Object
    Public Property itemName As Object
End Class
