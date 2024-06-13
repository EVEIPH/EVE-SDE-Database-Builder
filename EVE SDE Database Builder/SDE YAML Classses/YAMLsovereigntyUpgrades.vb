
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLsovereigntyUpgrades
    Inherits YAMLFilesBase

    Public Const sovereigntyUpgradesFile As String = "sovereigntyUpgrades.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = sovereigntyUpgradesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.Instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, sovereigntyUpgrades)
        Dim DataFields As List(Of DBField)
        Dim TranslatedField As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.bigint_type, 0, True, True),
            New DBTableField("fuel_hourly_upkeep", FieldType.bigint_type, 0, True),
            New DBTableField("fuel_startup_cost", FieldType.bigint_type, 0, True),
            New DBTableField("fuel_type_id", FieldType.bigint_type, 0, True),
            New DBTableField("mutually_exclusive_group", FieldType.varchar_type, 100, True),
            New DBTableField("power_allocation", FieldType.int_type, 0, True),
            New DBTableField("workforce_allocation", FieldType.int_type, 0, True)
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
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, sovereigntyUpgrades))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fuel_hourly_upkeep", .fuel_hourly_upkeep, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fuel_startup_cost", .fuel_startup_cost, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fuel_type_id", .fuel_type_id, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("mutually_exclusive_group", .mutually_exclusive_group, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("power_allocation", .power_allocation, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("workforce_allocation", .workforce_allocation, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, DataFields)

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

Public Class sovereigntyUpgrades
    Public Property fuel_hourly_upkeep As Long
    Public Property fuel_startup_cost As Long
    Public Property fuel_type_id As Long
    Public Property mutually_exclusive_group As String
    Public Property power_allocation As Integer
    Public Property workforce_allocation As Integer

End Class