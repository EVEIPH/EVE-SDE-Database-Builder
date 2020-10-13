
Imports YamlDotNet.Serialization
Imports System.IO

' Class imports translation data from YAML and inserts it into the appropriate tables
Public Class YAMLTranslations
    Inherits YAMLFilesBase

    Public Const trnTranslationColumnsTable As String = "trnTranslationColumns"
    Public Const trnTranslationLanguagesTable As String = "trnTranslationLanguages"
    Public Const trnTranslationsTable As String = "trnTranslations"

    Public Const trnTranslationColumnsFile As String = "trnTranslationColumns.yaml"
    Public Const trnTranslationLanguagesFile As String = "trnTranslationLanguages.yaml"
    Public Const trnTranslationsFile As String = "trnTranslations.yaml"

    ' Local copies of the translation data tables
    Public TranslationTables As New LocalDatabase ' for final loading and passing to databases for inserts
    Private TranslationTablesDB As SQLiteDB ' for searching
    Private LocalDBPath As String = ""

    Public Sub New(ByVal YAMLFilePath As String, ByRef DatabaseRef As Object, ByVal DBPath As String)
        ' The file name doesn't matter for transations, they are set below also translations will use a local db and then insert for searching
        MyBase.New("", YAMLFilePath, DatabaseRef, Nothing)
        Dim TempPath = DBPath & "\TranslationTemp\"

        ' Clean up the directory if there
        If Directory.Exists(TempPath) Then
            Call Directory.Delete(TempPath, True)
        End If

        ' Init local db
        TranslationTablesDB = New SQLiteDB(TempPath & "TranslationTemp.sqlite", TempPath, True)
        LocalDBPath = TempPath

    End Sub

    ''' <summary>
    ''' Imports the trnTranslationLanguages table into the database
    ''' </summary>
    ''' <param name="Params">Import parameters</param>
    ''' <param name="ImportTable">If we want to import the table or not. If not, then table imported for local use.</param>
    ''' <param name="ShowProgress">Do we show progress of import or not</param>
    Public Sub ImportTranslationLanguages(ByVal Params As ImportParameters, Optional ImportTable As Boolean = True,
                                          Optional ByVal ShowProgress As Boolean = True)
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of trnTranslationLanguage)
        Dim DataFields As List(Of DBField)
        Dim SQL As String = ""
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0
        Dim ImportText As String = String.Format("Importing {0}...", trnTranslationLanguagesFile)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("numericLanguageID", FieldType.int_type, 0, False, True))
        Table.Add(New DBTableField("languageID", FieldType.varchar_type, 50, True))
        Table.Add(New DBTableField("languageName", FieldType.nvarchar_type, 200, True))

        If ImportTable Then
            Call UpdateDB.CreateTable(trnTranslationLanguagesTable, Table)
        End If

        Call TranslationTablesDB.CreateTable(trnTranslationLanguagesTable, Table)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of trnTranslationLanguage))(New StringReader(File.ReadAllText(YAMLFile & trnTranslationLanguagesFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        TotalRecords = YAMLRecords.Count

        ' Begin processing
        If ShowProgress Then
            If ImportTable Then
                Call InitGridRow(Params.RowLocation)
            Else
                Call InitalizeMainProgressBar(TotalRecords, "Importing " & trnTranslationLanguagesFile)
            End If
        End If

        ' Process Data
        For Each DataField In YAMLRecords
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("numericLanguageID", DataField.numericLanguageID, FieldType.int_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("languageID", DataField.languageID.ToUpper, FieldType.varchar_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("languageName", DataField.languageName, FieldType.nvarchar_type))

            ' Save locally for searching and import
            Call TranslationTablesDB.InsertRecord(trnTranslationLanguagesTable, DataFields)
            Call TranslationTables.InsertRecord(trnTranslationLanguagesTable, DataFields)

            ' Update progress
            If ShowProgress Then
                If ImportTable Then
                    Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
                Else
                    Call UpdateMainProgressBar(Count, "Importing " & trnTranslationLanguagesFile)
                End If
            End If

            Count += 1

            If CancelImport Then
                GoTo Cancel
            End If

        Next

Cancel:

        If ShowProgress Then
            If ImportTable Then
                Call FinalizeGridRow(Params.RowLocation)
            Else
                Call ClearMainProgressBar()
            End If
        End If

    End Sub

    ''' <summary>
    ''' Imports the translations.yaml file and inserts all records into the trnTranslationColumns table
    ''' </summary>
    ''' <param name="Params">Import paramenters</param>
    ''' <param name="ImportTable">If we want to import the table, true, else false. If false, then we are importing the table for local use only</param>
    Public Sub ImportTranslationColumns(ByVal Params As ImportParameters, Optional ImportTable As Boolean = True)
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of trnTranslationColumn)
        Dim SQL As String = ""

        Dim ImportText As String = String.Format("Importing {0}...", trnTranslationColumnsFile)
        Dim IndexFields As List(Of String)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("columnName", FieldType.nvarchar_type, 128, True))
        Table.Add(New DBTableField("masterID", FieldType.nvarchar_type, 128, True))
        Table.Add(New DBTableField("tableName", FieldType.nvarchar_type, 256, True))
        Table.Add(New DBTableField("tcGroupID", FieldType.smallint_type, 0, True))
        Table.Add(New DBTableField("tcID", FieldType.smallint_type, 0, True))

        If ImportTable Then
            Call UpdateDB.CreateTable(trnTranslationColumnsTable, Table)
        End If
        Call TranslationTablesDB.CreateTable(trnTranslationColumnsTable, Table)

        ' Create Index
        IndexFields = New List(Of String)
        IndexFields.Add("tableName")
        IndexFields.Add("columnName")
        IndexFields.Add("masterID")

        If ImportTable Then
            Call UpdateDB.CreateIndex(trnTranslationColumnsTable, "IDX_" & trnTranslationColumnsTable & "_TN_CN_MID", IndexFields, True)
        End If
        Call TranslationTablesDB.CreateIndex(trnTranslationColumnsTable, "IDX_" & trnTranslationColumnsTable & "_TN_CN_MID", IndexFields, True)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of trnTranslationColumn))(New StringReader(File.ReadAllText(YAMLFile & trnTranslationColumnsFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Insert all the records read
        Call InsertTranslationColumnsRecords(YAMLRecords, Params.RowLocation, ImportText, ImportTable)

    End Sub

    ''' <summary>
    ''' Saves the records from trnTranslationColumns into the table
    ''' </summary>
    ''' <param name="trnColRecord">Parsed list of trnTranslationColumns from yaml</param>
    ''' <param name="RowLocation">What row the file is in the main grid for updating</param>
    ''' <param name="ImportText">Text to show our progress on the form</param>
    ''' <param name="UpdateGridProgressbar">If we want to update the grid progress bar, then true, else the main progressbar</param>
    ''' <param name="ShowProgress">If we want to show progress at all.</param>
    Private Sub InsertTranslationColumnsRecords(ByVal trnColRecord As List(Of trnTranslationColumn), ByVal RowLocation As Integer, ByVal ImportText As String,
                                                UpdateGridProgressbar As Boolean, Optional ByVal ShowProgress As Boolean = True)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0
        Dim TempTableName As String = ""

        TotalRecords = trnColRecord.Count

        ' Begin processing
        If ShowProgress Then
            If UpdateGridProgressbar Then
                Call InitGridRow(RowLocation)
            Else
                Call InitalizeMainProgressBar(TotalRecords, "Importing " & trnTranslationColumnsFile)
            End If
        End If

        ' Process Data
        For Each DataField In trnColRecord
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("columnName", DataField.columnName, FieldType.nvarchar_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("masterID", DataField.masterID, FieldType.nvarchar_type))
            ' Format table name and strip the 'dbo.' from it if sent so it's consistent with other db versions
            If DataField.tableName.Contains("dbo.") Then
                TempTableName = DataField.tableName.Substring(InStr(DataField.tableName, "."))
            Else
                TempTableName = DataField.tableName
            End If
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("tableName", TempTableName, FieldType.nvarchar_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("tcGroupID", DataField.tcGroupID, FieldType.smallint_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("tcID", DataField.tcID, FieldType.smallint_type))

            Call TranslationTablesDB.InsertRecord(trnTranslationColumnsTable, DataFields)
            Call TranslationTables.InsertRecord(trnTranslationColumnsTable, DataFields)

            ' Update progress
            If ShowProgress Then
                If UpdateGridProgressbar Then
                    Call UpdateGridRowProgress(RowLocation, Count, TotalRecords)
                Else
                    Call UpdateMainProgressBar(Count, "Importing " & trnTranslationColumnsFile)
                End If
            End If

            Count += 1
            Application.DoEvents()

            If CancelImport Then
                Exit Sub
            End If

        Next

        If ShowProgress Then
            If UpdateGridProgressbar Then
                Call FinalizeGridRow(RowLocation)
            Else
                Call ClearMainProgressBar()
            End If
        End If

    End Sub

    ''' <summary>
    ''' Imports the translations.yaml file and inserts all records into the trnTranslationColumns and trnTranslations tables
    ''' </summary>
    ''' <param name="Params">Import paramenters</param>
    ''' <param name="ImportTable">If we want to import the table, true, else false. If false, then we are importing the table for local use only</param>
    Public Sub ImportTranslations(ByVal Params As ImportParameters, Optional ImportTable As Boolean = True)
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(New NamingConventions.NullNamingConvention)
        Dim DS As New Deserializer
        DS = DSB.Build

        Dim YAMLRecords As New List(Of trnTranslation)
        Dim IndexFields As List(Of String)
        Dim SQL As String = ""
        Dim ImportText As String = String.Format("Importing {0}...", trnTranslationsFile)

        ' Build table
        Dim Table As New List(Of DBTableField)
        Table.Add(New DBTableField("keyID", FieldType.int_type, 0, True))
        Table.Add(New DBTableField("languageID", FieldType.varchar_type, 50, True))
        Table.Add(New DBTableField("tcID", FieldType.smallint_type, 256, True))
        Table.Add(New DBTableField("text", FieldType.nvarchar_type, -1, True)) ' Some are null for some reason

        If ImportTable Then
            Call UpdateDB.CreateTable(trnTranslationsTable, Table)
        End If

        Call TranslationTablesDB.CreateTable(trnTranslationsTable, Table)

        ' Create Index
        IndexFields = New List(Of String)
        IndexFields.Add("tcID")
        IndexFields.Add("keyID")
        IndexFields.Add("languageID")

        If ImportTable Then
            Call UpdateDB.CreateIndex(trnTranslationsTable, "IDX_" & trnTranslationsTable & "_TCID_KID_LID", IndexFields, False)
        End If

        Call TranslationTablesDB.CreateIndex(trnTranslationsTable, "IDX_" & trnTranslationsTable & "_TCID_KID_LID", IndexFields, False)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of List(Of trnTranslation))(New StringReader(File.ReadAllText(YAMLFile & trnTranslationsFile)))
        Catch ex As Exception
            Call ShowErrorMessage(ex)
        End Try

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Insert all the records read
        Call InsertTranslationsRecords(YAMLRecords, Params.RowLocation, ImportText, ImportTable)

    End Sub

    ''' <summary>
    ''' Saves the records from trnTranslations into the table
    ''' </summary>
    ''' <param name="trnRecord">Parsed list of trnTranslations from yaml</param>
    ''' <param name="RowLocation">What row the file is in the main grid for updating</param>
    ''' <param name="ImportText">Text to show our progress on the form</param>
    ''' <param name="UpdateGridProgressbar">If we want to update the grid progress bar, then true, else the main progressbar</param>
    ''' <param name="ShowProgress">If we want to show progress at all.</param>
    Private Sub InsertTranslationsRecords(ByVal trnRecord As List(Of trnTranslation), ByVal RowLocation As Integer, ByVal ImportText As String,
                                          Optional UpdateGridProgressbar As Boolean = True, Optional ByVal ShowProgress As Boolean = True)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long = 0

        TotalRecords = trnRecord.Count

        ' Begin processing
        If ShowProgress Then
            If UpdateGridProgressbar Then
                Call InitGridRow(RowLocation)
            Else
                Call InitalizeMainProgressBar(TotalRecords, "Importing " & trnTranslationsFile)
            End If
        End If

        ' Process Data
        For Each DataField In trnRecord
            DataFields = New List(Of DBField)

            ' Build the insert list
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("keyID", DataField.keyID, FieldType.int_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("languageID", DataField.languageID.ToUpper, FieldType.varchar_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("tcID", DataField.tcID, FieldType.smallint_type))
            DataFields.Add(TranslationTablesDB.BuildDatabaseField("text", DataField.text, FieldType.nvarchar_type))

            Call TranslationTablesDB.InsertRecord(trnTranslationsTable, DataFields)
            Call TranslationTables.InsertRecord(trnTranslationsTable, DataFields)

            ' Update progress
            If ShowProgress Then
                If UpdateGridProgressbar Then
                    Call UpdateGridRowProgress(RowLocation, Count, TotalRecords)
                Else
                    Call UpdateMainProgressBar(Count, "Importing " & trnTranslationsFile)
                End If
            End If

            Count += 1
            Application.DoEvents()

            If CancelImport Then
                Exit Sub
            End If

        Next

        If ShowProgress Then
            If UpdateGridProgressbar Then
                Call FinalizeGridRow(RowLocation)
            Else
                Call ClearMainProgressBar()
            End If
        End If

    End Sub

    ''' <summary>
    ''' Inserts sent data (single record with all translations) into the temp translation lists for later import. For use with typeIDs.yaml, categoryIDs.yaml, and groupIDs.yaml
    ''' </summary>
    ''' <param name="ID">masterID field value (e.g. if masterID is 'typeID' then keyID is the typeID number</param>
    ''' <param name="TranslationMasterID">masterID field name (e.g. typeID)</param>
    ''' <param name="TranslationColumnName">Name of the column/field that will be translated when looked up (e.g. typeName)</param>
    ''' <param name="TranslationTableName">Name of the table with the translation field</param>
    ''' <param name="TranslationDataList">List of translations</param>
    Public Sub InsertTranslationData(ByVal ID As String, ByVal TranslationMasterID As String, ByVal TranslationColumnName As String,
                                     ByVal TranslationTableName As String, ByVal TranslationDataList As List(Of TranslationData))
        Dim TempTranslation As trnTranslation
        Dim TempTranslationColumn As New trnTranslationColumn

        Dim TranslationsList As New List(Of trnTranslation)
        Dim TranslationColumnsList As New List(Of trnTranslationColumn)

        Dim Result As New List(Of List(Of Object))
        Dim WhereClause As List(Of String)
        Dim SelectClause As New List(Of String)

        Dim TranslationColumnID As Integer

        ' Look up id used
        WhereClause = New List(Of String)
        WhereClause.Add("tableName='" & TranslationTableName & "'")
        WhereClause.Add("columnName='" & TranslationColumnName & "'")
        WhereClause.Add("masterID ='" & TranslationMasterID & "'")

        SelectClause.Add("tcID")

        Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationColumnsTable, WhereClause)

        If Result.Count = 0 Then
            ' Look up the max and add one to it
            WhereClause = New List(Of String)
            SelectClause = New List(Of String)
            SelectClause.Add("Max(tcID)")
            TranslationColumnID = CInt(TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationColumnsTable, WhereClause)(0)(0)) + 1
        Else
            TranslationColumnID = CInt(Result(0)(0))
        End If

        For Each entry In TranslationDataList
            ' See if the record is there
            WhereClause = New List(Of String)
            WhereClause.Add("tcID =" & TranslationColumnID)
            WhereClause.Add("keyID =" & ID)
            WhereClause.Add("languageID='" & entry.TranslationCode & "'")

            SelectClause = New List(Of String)
            SelectClause.Add("tcID")

            Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationsTable, WhereClause)

            If Result.Count = 0 Then
                ' No data found, insert
                TempTranslation = New trnTranslation
                TempTranslation.tcID = TranslationColumnID
                TempTranslation.keyID = ID
                TempTranslation.text = entry.Translation
                TempTranslation.languageID = entry.TranslationCode

                TranslationsList = New List(Of trnTranslation) ' reset but only use for one record
                Call TranslationsList.Add(TempTranslation)
                Call InsertTranslationsRecords(TranslationsList, 0, "", False, False)
            End If

            ' See if the record is there
            WhereClause = New List(Of String)
            WhereClause.Add("tableName='" & TranslationTableName & "'")
            WhereClause.Add("tcID =" & TranslationColumnID)

            SelectClause.Add("tcID")

            Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationColumnsTable, WhereClause)

            If Result.Count = 0 Then
                ' No data found, insert
                TempTranslationColumn = New trnTranslationColumn
                TempTranslationColumn.tcID = TranslationColumnID
                TempTranslationColumn.masterID = TranslationMasterID
                TempTranslationColumn.tableName = TranslationTableName
                TempTranslationColumn.tcGroupID = -1 ' This isn't used but it seems to be a group number for the translations by table (e.g. invTypes is all group 85) - not setting this now
                TempTranslationColumn.columnName = TranslationColumnName

                TranslationColumnsList = New List(Of trnTranslationColumn) ' reset but only use for one record
                Call TranslationColumnsList.Add(TempTranslationColumn)
                Call InsertTranslationColumnsRecords(TranslationColumnsList, 0, "", False, False)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Searches the local tables for the correct translation for the data sent in the local database of translation tables (not updated)
    ''' </summary>
    ''' <param name="tableName">Table name to search</param>
    ''' <param name="columnName">Column name in search table</param>
    ''' <param name="masterID">Field name that contains the ID to look up for a translation - the link number</param>
    ''' <param name="keyID">Value of the masterID field (e.g. if masterID is 'typeID' then keyID is the typeID number)</param>
    ''' <param name="LanguageID">Language code we are searching for</param>
    ''' <param name="DefaultText">If not found, what text to return</param>
    ''' <returns></returns>
    Public Function TranslateData(ByVal tableName As String, ByVal columnName As String, ByVal masterID As String, ByVal keyID As Integer,
                                  ByVal LanguageID As LanguageCode, ByVal DefaultText As Object) As String
        Dim WhereValues As New List(Of String)
        Dim ReturnValue As Object
        Dim Result As List(Of List(Of Object))
        Dim SelectClause As New List(Of String)

        If IsNothing(DefaultText) Then
            DefaultText = ""
        End If

        ' Look up tcID first
        WhereValues = New List(Of String)
        WhereValues.Add("tableName='" & tableName & "'")
        WhereValues.Add("columnName='" & columnName & "'")
        WhereValues.Add("masterID='" & masterID & "'")

        SelectClause.Add("tcID")

        Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationColumnsTable, WhereValues)

        If Result.Count = 0 Then
            Return DefaultText
        Else
            ReturnValue = Result(0)(0).ToString()
        End If

        ' Try to look up the text now
        WhereValues = New List(Of String)
        Dim NameTranslation As New ImportLanguage(LanguageID)
        WhereValues.Add("tcID=" & CStr(Result(0)(0)))
        WhereValues.Add("keyID=" & keyID)
        WhereValues.Add("languageID='" & NameTranslation.GetCurrentLanguageID & "'")

        SelectClause = New List(Of String)
        SelectClause.Add("text")

        Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationsTable, WhereValues)

        If Result.Count = 0 Then
            Return DefaultText
        Else
            Return Result(0)(0).ToString()
        End If

    End Function

    ''' <summary>
    ''' Closes local db's and cleans up any new file folders
    ''' </summary>
    Public Sub Close()
        Call TranslationTablesDB.CloseDB()
        TranslationTablesDB = Nothing
        Call Directory.Delete(LocalDBPath, True)
    End Sub

End Class

Public Class trnTranslationLanguage
    Public Property languageID As Object
    Public Property languageName As Object
    Public Property numericLanguageID As Object
End Class

Public Class trnTranslationColumn
    Public Property columnName As Object
    Public Property masterID As Object
    Public Property tableName As Object
    Public Property tcGroupID As Object
    Public Property tcID As Object
End Class

Public Class trnTranslation
    Public Property keyID As Object
    Public Property languageID As Object
    Public Property tcID As Object
    Public Property text As Object
End Class