
Imports YamlDotNet.Serialization
Imports System.IO

Public Class YAMLstationServices
    Inherits YAMLFilesBase

    Public Const stationServicesFile As String = "stationServices.yaml"

    Public Sub New(ByVal YAMLFileName As String, ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByRef TranslationRef As YAMLTranslations)
        MyBase.New(YAMLFileName, YAMLFilePath, DatabaseRef, TranslationRef)
    End Sub

    ''' <summary>
    ''' Imports the yaml file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub ImportFile(ByVal Params As ImportParameters)
        FileNameErrorTracker = stationServicesFile
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New Dictionary(Of Long, stationService)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        Dim NameTranslation As New ImportLanguage(Params.ImportLanguageCode)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("serviceID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("serviceName", FieldType.nvarchar_type, 100, True))
        Table.Add(New DBTableField("description", FieldType.nvarchar_type, 1000, True))

        Call UpdateDB.CreateTable(TableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of Long, stationService))(New StringReader(File.ReadAllText(YAMLFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        TotalRecords = YAMLRecords.Count

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("serviceID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("serviceName", NameTranslation.GetLanguageTranslationData(.serviceNameID), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", NameTranslation.GetLanguageTranslationData(.descriptionID), FieldType.nvarchar_type))

                ' Insert the translated data into translation tables
                Call Translator.InsertTranslationData(DataField.Key, "factionID", "factionName", TableName, NameTranslation.GetAllTranslations(.serviceNameID))
                Call Translator.InsertTranslationData(DataField.Key, "factionID", "description", TableName, NameTranslation.GetAllTranslations(.descriptionID))

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

Public Class stationService
    Public Property serviceID As Object
    Public Property serviceNameID As Translations
    Public Property descriptionID As Translations
End Class