
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLskins
    Inherits YAMLFilesBase

    Public Const skinsFile As String = "skins.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, skin)
        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("skinID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("skinDescription", FieldType.text_type, MaxFieldLen, True))
        Table.Add(New DBTableField("internalName", FieldType.varchar_type, 100, True))
        Table.Add(New DBTableField("skinMaterialID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("isStructureSkin", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("allowCCPDevs", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("visibleSerenity", FieldType.bit_type, 0, True))
        Table.Add(New DBTableField("visibleTranquility", FieldType.bit_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields.Add("skinID")
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, skin))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            For Each DF In DataField.Value.types
                DataFields = New List(Of DBField)

                With DataField.Value
                    ' Build the insert list
                    DataFields.Add(UpdateDB.BuildDatabaseField("skinID", .skinID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skinDescription", .skinDescription, FieldType.text_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("internalName", .internalName, FieldType.varchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skinMaterialID", .skinMaterialID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("isStructureSkin", .isStructureSkin, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DF, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("allowCCPDevs", .allowCCPDevs, FieldType.bit_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("visibleSerenity", .visibleSerenity, FieldType.bit_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("visibleTranquility", .visibleTranquility, FieldType.bit_type))
                End With

                Call UpdateDB.InsertRecord(TableName, DataFields)
            Next

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class skin
    Public Property skinID As Object
    Public Property skinDescription As Object
    Public Property allowCCPDevs As Object
    Public Property internalName As Object
    Public Property skinMaterialID As Object
    Public Property isStructureSkin As Object
    Public Property types As List(Of Object)
    Public Property visibleSerenity As Object
    Public Property visibleTranquility As Object
End Class