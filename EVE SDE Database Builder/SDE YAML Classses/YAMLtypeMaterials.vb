
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLtypeMaterials
    Inherits YAMLFilesBase

    Public Const typeMaterialsFile As String = "typeMaterials.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = typeMaterialsFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, typeMaterials)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False, True),
            New DBTableField("materialTypeID", FieldType.int_type, 0, False, True),
            New DBTableField("quantity", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, typeMaterials))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            Dim typeID As Long = DataField.Key
            For Each record In DataField.Value.materials
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("typeID", typeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("materialTypeID", record.materialTypeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("quantity", record.quantity, FieldType.int_type)
                }

                Call UpdateDB.InsertRecord(TableName, DataFields)
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

Public Class typeMaterials
    Public Property materials As List(Of materialPair)
End Class

Public Class materialPair
    Public Property materialTypeID As Object
    Public Property quantity As Object
End Class