
Imports YamlDotNet.Serialization
Imports System.IO

' Class imports translation data from YAML and inserts it into the appropriate tables
Public Class YAMLTranslations
    Inherits YAMLFilesBase

    Public Const trnTranslationColumnsTable As String = "translationColumns"
    Public Const trnTranslationLanguagesTable As String = "translationLanguages"
    Public Const trnTranslationsTable As String = "translations"

    Public Const translationLanguagesFile As String = "translationLanguages.yaml"

    ' Local copies of the translation data tables
    Public TranslationTables As New LocalDatabase ' for final loading and passing to databases for inserts
    Private TranslationTablesDB As SQLiteDB ' for searching
    Private ReadOnly LocalDBPath As String

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
        FileNameErrorTracker = "ImportTranslationLanguages"
        Dim DSB = New DeserializerBuilder()
        If Not TestForSDEChanges Then
            DSB.IgnoreUnmatchedProperties()
        End If
        DSB = DSB.WithNamingConvention(NamingConventions.NullNamingConvention.instance)
        Dim DS As Deserializer = DSB.Build

        Dim YAMLRecords As New Dictionary(Of String, String)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long
        Dim ImportText As String = String.Format("Importing {0}...", translationLanguagesFile)
        Dim IndexFields As List(Of String)

        ' Build columns table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("columnName", FieldType.nvarchar_type, 128, True),
            New DBTableField("masterID", FieldType.nvarchar_type, 128, True),
            New DBTableField("tableName", FieldType.nvarchar_type, 256, True),
            New DBTableField("tcGroupID", FieldType.smallint_type, 0, True),
            New DBTableField("tcID", FieldType.smallint_type, 0, True)
        }

        If ImportTable Then
            Call UpdateDB.CreateTable(trnTranslationColumnsTable, Table)
        End If
        Call TranslationTablesDB.CreateTable(trnTranslationColumnsTable, Table)

        ' Create Indexes
        IndexFields = New List(Of String) From {
            "tableName",
            "masterID",
            "columnName"
        }

        If ImportTable Then
            Call UpdateDB.CreateIndex(trnTranslationColumnsTable, "IDX_" & trnTranslationColumnsTable & "_TN_CN_MID", IndexFields, True)
        End If
        Call TranslationTablesDB.CreateIndex(trnTranslationColumnsTable, "IDX_" & trnTranslationColumnsTable & "_TN_CN_MID", IndexFields, True)

        IndexFields = New List(Of String) From {
            "tableName",
            "tcID"
        }

        If ImportTable Then
            Call UpdateDB.CreateIndex(trnTranslationColumnsTable, "IDX_" & trnTranslationColumnsTable & "_TN_TID", IndexFields, True)
        End If
        Call TranslationTablesDB.CreateIndex(trnTranslationColumnsTable, "IDX_" & trnTranslationColumnsTable & "_TN_TID", IndexFields, True)

        ' Build translations table
        Table = New List(Of DBTableField) From {
            New DBTableField("keyID", FieldType.int_type, 0, True),
            New DBTableField("languageID", FieldType.varchar_type, 50, True),
            New DBTableField("tcID", FieldType.smallint_type, 256, True),
            New DBTableField("text", FieldType.nvarchar_type, -1, True) ' Some are null for some reason
            }

        If ImportTable Then
            Call UpdateDB.CreateTable(trnTranslationsTable, Table)
        End If

        Call TranslationTablesDB.CreateTable(trnTranslationsTable, Table)

        ' Create Index
        IndexFields = New List(Of String) From {
            "tcID",
            "keyID",
            "languageID"
        }

        If ImportTable Then
            Call UpdateDB.CreateIndex(trnTranslationsTable, "IDX_" & trnTranslationsTable & "_TCID_KID_LID", IndexFields, False)
        End If

        Call TranslationTablesDB.CreateIndex(trnTranslationsTable, "IDX_" & trnTranslationsTable & "_TCID_KID_LID", IndexFields, False)

        ' Build language table
        Table = New List(Of DBTableField) From {
            New DBTableField("languageID", FieldType.varchar_type, 5, True),
            New DBTableField("languageName", FieldType.nvarchar_type, 10, True)
        }

        If ImportTable Then
            Call UpdateDB.CreateTable(trnTranslationLanguagesTable, Table)
        End If

        Call TranslationTablesDB.CreateTable(trnTranslationLanguagesTable, Table)

        Try
            ' Parse the input text
            YAMLRecords = DS.Deserialize(Of Dictionary(Of String, String))(New StringReader(File.ReadAllText(YAMLFile & translationLanguagesFile)))
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
                Call InitalizeMainProgressBar(TotalRecords, "Importing " & translationLanguagesFile)
            End If
        End If

        ' Process Data
        For Each DataField In YAMLRecords
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                TranslationTablesDB.BuildDatabaseField("languageID", DataField.Key.ToUpper, FieldType.varchar_type),
                TranslationTablesDB.BuildDatabaseField("languageName", DataField.Value, FieldType.nvarchar_type)
            }

            ' Save locally for searching and import
            Call TranslationTablesDB.InsertRecord(trnTranslationLanguagesTable, DataFields)
            Call TranslationTables.InsertRecord(trnTranslationLanguagesTable, DataFields)

            ' Update progress
            If ShowProgress Then
                If ImportTable Then
                    Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
                Else
                    Call UpdateMainProgressBar(Count, "Importing " & translationLanguagesFile)
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

        YAMLRecords.Clear()

    End Sub

    ''' <summary>
    ''' Saves the records from trnTranslationColumns into the table
    ''' </summary>
    ''' <param name="trnColRecord">Parsed list of trnTranslationColumns from yaml</param>
    ''' <param name="RowLocation">What row the file is in the main grid for updating</param>
    ''' <param name="UpdateGridProgressbar">If we want to update the grid progress bar, then true, else the main progressbar</param>
    ''' <param name="ShowProgress">If we want to show progress at all.</param>
    Private Sub InsertTranslationColumnsRecords(ByVal trnColRecord As List(Of trnTranslationColumn), ByVal RowLocation As Integer,
                                                UpdateGridProgressbar As Boolean, Optional ByVal ShowProgress As Boolean = True)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long
        Dim TempTableName As String

        TotalRecords = trnColRecord.Count

        ' Begin processing
        If ShowProgress Then
            If UpdateGridProgressbar Then
                Call InitGridRow(RowLocation)
            End If
        End If

        ' Process Data
        For Each DataField In trnColRecord
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                TranslationTablesDB.BuildDatabaseField("columnName", DataField.columnName, FieldType.nvarchar_type),
                TranslationTablesDB.BuildDatabaseField("masterID", DataField.masterID, FieldType.nvarchar_type)
            }
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
    ''' Saves the records from trnTranslations into the table
    ''' </summary>
    ''' <param name="trnRecord">Parsed list of trnTranslations from yaml</param>
    ''' <param name="RowLocation">What row the file is in the main grid for updating</param>
    ''' <param name="UpdateGridProgressbar">If we want to update the grid progress bar, then true, else the main progressbar</param>
    ''' <param name="ShowProgress">If we want to show progress at all.</param>
    Private Sub InsertTranslationsRecords(ByVal trnRecord As List(Of trnTranslation), ByVal RowLocation As Integer,
                                          Optional UpdateGridProgressbar As Boolean = True, Optional ByVal ShowProgress As Boolean = True)
        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim TotalRecords As Long

        TotalRecords = trnRecord.Count

        ' Begin processing
        If ShowProgress Then
            If UpdateGridProgressbar Then
                Call InitGridRow(RowLocation)
            End If
        End If

        ' Process Data
        For Each DataField In trnRecord
            ' Build the insert list
            DataFields = New List(Of DBField) From {
                TranslationTablesDB.BuildDatabaseField("keyID", DataField.keyID, FieldType.int_type),
                TranslationTablesDB.BuildDatabaseField("languageID", DataField.languageID.ToUpper, FieldType.varchar_type),
                TranslationTablesDB.BuildDatabaseField("tcID", DataField.tcID, FieldType.smallint_type),
                TranslationTablesDB.BuildDatabaseField("text", DataField.text, FieldType.nvarchar_type)
            }

            Call TranslationTablesDB.InsertRecord(trnTranslationsTable, DataFields)
            Call TranslationTables.InsertRecord(trnTranslationsTable, DataFields)

            ' Update progress
            If ShowProgress Then
                If UpdateGridProgressbar Then
                    Call UpdateGridRowProgress(RowLocation, Count, TotalRecords)
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
    ''' Inserts sent data (single record with all translations) into the temp translation lists for later import.
    ''' </summary>
    ''' <param name="ID">masterID field value (e.g. if masterID is 'typeID' then keyID is the typeID number</param>
    ''' <param name="TranslationMasterID">masterID field name (e.g. typeID)</param>
    ''' <param name="TranslationColumnName">Name of the column/field that will be translated when looked up (e.g. typeName)</param>
    ''' <param name="TranslationTableName">Name of the table with the translation field</param>
    ''' <param name="TranslationDataList">List of translations</param>
    Public Sub InsertTranslationData(ByVal ID As String, ByVal TranslationMasterID As String, ByVal TranslationColumnName As String,
                                     ByVal TranslationTableName As String, ByVal TranslationDataList As List(Of TranslationData))
        Dim TempTranslation As trnTranslation
        Dim TempTranslationColumn As trnTranslationColumn

        Dim TranslationsList As List(Of trnTranslation)
        Dim TranslationColumnsList As List(Of trnTranslationColumn)

        Dim WhereClause As List(Of String)
        Dim SelectClause As New List(Of String)

        Dim TranslationColumnID As Integer
        Dim Records As Boolean = False
        Dim Result As List(Of List(Of Object))

        ' First see if there are any records in the translationscolumns table and set the tcID
        WhereClause = New List(Of String) From {
                "tableName='" & TranslationTableName & "'",
                "masterID ='" & TranslationMasterID & "'",
                "columnName='" & TranslationColumnName & "'"
            }

        SelectClause.Add("tcID")
        SyncLock Lock2
            Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationColumnsTable, WhereClause)

            If Result.Count = 0 Then
                ' Look up the max tcID in the table and add one to it
                WhereClause = New List(Of String)
                SelectClause = New List(Of String) From {
                    "Max(tcID)"
                }

                Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationColumnsTable, WhereClause)

                If IsDBNull(Result(0)(0)) Then
                    ' no records, so set the tcID to 1 and insert
                    TranslationColumnID = 1
                Else
                    ' no records at all in table for this table
                    TranslationColumnID = CInt(Result(0)(0)) + 1
                End If

                ' Since this record didn't exist in the table, insert it now that we have the tcID
                TempTranslationColumn = New trnTranslationColumn With {
                    .tcID = TranslationColumnID,
                    .masterID = TranslationMasterID,
                    .tableName = TranslationTableName,
                    .tcGroupID = -1, ' This isn't used but it seems to be a group number for the translations by table (e.g. invTypes is all group 85) - not setting this now
                    .columnName = TranslationColumnName
                }

                TranslationColumnsList = New List(Of trnTranslationColumn) ' reset but only use for one record
                Call TranslationColumnsList.Add(TempTranslationColumn)
                Call InsertTranslationColumnsRecords(TranslationColumnsList, 0, False, False)
            Else
                ' Record exists, just return the tcID
                TranslationColumnID = CInt(Result(0)(0))
            End If

            ' Insert a translation for each entry present
            For Each entry In TranslationDataList
                Records = False ' reset
                ' See if the record is there in translations
                WhereClause = New List(Of String) From {
                    "tcID =" & TranslationColumnID,
                    "keyID =" & ID,
                    "languageID='" & entry.TranslationCode & "'"
                }

                SelectClause = New List(Of String) From {
                    "tcID"
                }

                Result = TranslationTablesDB.SelectfromTable(SelectClause, trnTranslationsTable, WhereClause, Records)

                If Not Records Then
                    ' No data found, insert
                    TempTranslation = New trnTranslation With {
                        .tcID = TranslationColumnID,
                        .keyID = ID,
                        .text = entry.Translation,
                        .languageID = entry.TranslationCode
                    }

                    TranslationsList = New List(Of trnTranslation) ' reset but only use for one record
                    Call TranslationsList.Add(TempTranslation)
                    Call InsertTranslationsRecords(TranslationsList, 0, False, False)
                End If

                Records = False ' reset

            Next
        End SyncLock

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
        Dim WhereValues As List(Of String)
        Dim ReturnValue As Object
        Dim Result As List(Of List(Of Object))
        Dim SelectClause As New List(Of String)

        If IsNothing(DefaultText) Then
            DefaultText = ""
        End If

        ' Look up tcID first
        WhereValues = New List(Of String) From {
            "tableName='" & tableName & "'",
            "masterID='" & masterID & "'",
            "columnName='" & columnName & "'"
        }

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

        SelectClause = New List(Of String) From {
            "text"
        }

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

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
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