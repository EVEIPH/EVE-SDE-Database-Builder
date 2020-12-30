
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLplanetSchematics
    Inherits YAMLFilesBase

    Public Const planetSchematicsFile As String = "planetSchematics.yaml"

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

        Dim YAMLRecords As New Dictionary(Of Long, planetSchematic)
        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim DataFields3 As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim planetSchematicsTypeMapTable As String = "planetSchematicsTypeMap"
        Dim planetSchematicsPinMapTable As String = "planetSchematicsPinMap"

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table - planetSchematics
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("schematicID", FieldType.smallint_type, 0, False, True))
        Table.Add(New DBTableField("schematicName", FieldType.nvarchar_type, 255, True))
        Table.Add(New DBTableField("cycleTime", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' Build table - planetSchematicsPinMap
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("schematicID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("pinTypeID", FieldType.int_type, 0, True))

        Call UpdateDB.CreateTable(planetSchematicsPinMaptable, Table)

        ' Build table - planetSchematicsTypeMap
        Table = New List(Of DBTableField)
        Table.Add(New DBTableField("schematicID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("typeID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("quantity", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("isInput", FieldType.bit_type, 0, True))

        Call UpdateDB.CreateTable(planetSchematicsTypeMapTable, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, planetSchematic))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            With DataField.Value
                DataFields = New List(Of DBField)
                DataFields.Add(UpdateDB.BuildDatabaseField("schematicID", DataField.Key, FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("schematicName", NameTranslation.GetLanguageTranslationData(.nameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("cycleTime", .cycleTime, FieldType.int_type))

                ' insert into pin map table
                For i = 0 To .pins.Count - 1
                    DataFields2 = New List(Of DBField)
                    DataFields2.Add(UpdateDB.BuildDatabaseField("schematicID", DataField.Key, FieldType.smallint_type))
                    DataFields2.Add(UpdateDB.BuildDatabaseField("pinTypeID", .pins(i), FieldType.int_type))
                    Call UpdateDB.InsertRecord(planetSchematicsPinMapTable, DataFields2)
                Next

                ' Insert all data into type map table
                For Each pinType In .types
                    DataFields3 = New List(Of DBField)
                    DataFields3.Add(UpdateDB.BuildDatabaseField("schematicID", DataField.Key, FieldType.smallint_type))
                    DataFields3.Add(UpdateDB.BuildDatabaseField("typeID", pinType.Key, FieldType.int_type))
                    DataFields3.Add(UpdateDB.BuildDatabaseField("quantity", pinType.Value.quantity, FieldType.smallint_type))
                    DataFields3.Add(UpdateDB.BuildDatabaseField("isInput", pinType.Value.isInput, FieldType.bit_type))
                    Call UpdateDB.InsertRecord(planetSchematicsTypeMapTable, DataFields3)
                Next

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "schematicID", "schematicName", TableName, NameTranslation.GetAllTranslations(.nameID))

            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

End Class

Public Class planetSchematic
    Public Property cycleTime As Object
    Public Property nameID As Translations
    Public Property pins As List(Of Object)
    Public Property types As Dictionary(Of Long, planetSchematicTypeMapValues)
End Class

Public Class planetSchematicTypeMapValues
    Public Property quantity As Object
    Public Property isInput As Object
End Class