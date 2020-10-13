
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLtypeDogma
    Inherits YAMLFilesBase

    Public Const typeDogmaFile As String = "typeDogma.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, typeDogma)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim AttributesTableName As String = "dogmaTypeAttributes"
        Dim EffectsTableName As String = "dogmaTypeEffects"

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("attributeID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("value", FieldType.real_type, 0, True))

        Call UpdateDB.CreateTable(AttributesTableName, Table)

        ' Build table
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("effectID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("isDefault", FieldType.bit_type, 0, True))

        Call UpdateDB.CreateTable(EffectsTableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, typeDogma))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            With DataField.Value
                ' For each record, insert the attribute values and the effect values
                If .dogmaAttributes.Count > 0 Then
                    For Each attribute In .dogmaAttributes
                        DataFields = New List(Of DBField)
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("attributeID", attribute.attributeID, FieldType.smallint_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("value", attribute.value, FieldType.int_type))
                        Call UpdateDB.InsertRecord(AttributesTableName, DataFields)
                    Next
                End If

                If .dogmaEffects.Count > 0 Then

                    For Each effect In .dogmaEffects
                        DataFields = New List(Of DBField)
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("effectID", effect.effectID, FieldType.smallint_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("isDefault", effect.isDefault, FieldType.bit_type))
                        Call UpdateDB.InsertRecord(EffectsTableName, DataFields)
                    Next
                End If
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class typeDogma
    Public Property dogmaAttributes As List(Of dogmatypeAttribute)
    Public Property dogmaEffects As List(Of dogmatypeEffect)
End Class

Public Class dogmatypeAttribute
    Public Property attributeID As Object
    Public Property value As Object
End Class

Public Class dogmatypeEffect
    Public Property effectID As Object
    Public Property isDefault As Object
End Class