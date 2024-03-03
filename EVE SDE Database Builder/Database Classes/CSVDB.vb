
Imports System.IO
Imports System.Security.AccessControl

''' <summary>
''' Class to create a CSV "database" and insert data into it.
''' </summary>
Public Class CSVDB
    Inherits DBFilesBase

    Private ReadOnly DB As String ' Folder Path for all 'tables' as files

    Private ReadOnly DELIMITER As String = ""
    Private Const CSVExtention As String = ".csv"
    Private ReadOnly UseNullforBlanks As Boolean = False

    ''' <summary>
    ''' Constructor class for a CSV "database". 
    ''' </summary>
    ''' <param name="DatabaseFileNameandPath">Name of the database to create.</param>
    ''' <param name="Success">True if the database successfully created.</param>
    ''' <param name="AllowDirectoryFullAccess">Optional boolean to allow access to the full directory to allow other DB classes access to the folder</param>
    ''' <param name="ExportasSSV">Export the data in Semi-colon separated values for EU users</param>
    Public Sub New(ByVal DatabaseFileNameandPath As String, ByRef Success As Boolean, ByVal InsertNullforBlankValues As Boolean,
                   Optional ByVal AllowDirectoryFullAccess As Boolean = False, Optional ByVal ExportasSSV As Boolean = False)
        MyBase.New(DatabaseFileNameandPath, DatabaseType.CSV)

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try
            ' Build a new folder for the 'database' 
            If Not Directory.Exists(DatabaseFileNameandPath) Then
                Directory.CreateDirectory(DatabaseFileNameandPath)
                Application.DoEvents()
            End If

            ' Set the folder access if needed (mainly for postgresql bulk import)
            If AllowDirectoryFullAccess Then
                Dim UserAccount As String = "EVERYONE" 'Specify the user here, to allow stuff like postgresql to have access to the folder
                Dim FolderInfo As New DirectoryInfo(DatabaseFileNameandPath)
                Dim FolderSecurity As New DirectorySecurity
                FolderSecurity.AddAccessRule(New FileSystemAccessRule(UserAccount, FileSystemRights.Read, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
                FolderInfo.SetAccessControl(FolderSecurity)
            End If

            DB = DatabaseFileNameandPath & "\"

            If ExportasSSV Then
                DELIMITER = SEMICOLON
            Else
                DELIMITER = COMMA
            End If

            UseNullforBlanks = InsertNullforBlankValues

            Success = True

        Catch ex As Exception
            Call ShowErrorMessage(ex)
            Success = False
        End Try

    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub CloseDB()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub BeginSQLTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub CommitSQLTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Creates a table on the server for the sent table name in the structure sent by table structure
    ''' </summary>
    ''' <param name="TableName">Name of the table to create.</param>
    ''' <param name="TableStructure">List of table fields that define the table.</param>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Dim TableStream As StreamWriter
        Dim OutputText As String = ""

        ' "drop table" if it exists
        If File.Exists(DB & TableName & CSVExtention) Then
            File.Delete(DB & TableName & CSVExtention)
        End If

        ' Build the file
        Try
            TableStream = File.CreateText(DB & TableName & CSVExtention)
        Catch ex As Exception
            ShowErrorMessage(ex)
            Exit Sub
        End Try

        ' Add fields
        For Each Field In TableStructure
            With Field
                OutputText = OutputText & Field.FieldName & DELIMITER
            End With
        Next

        ' Strip last DELIMITER
        OutputText = StripLastCharacter(OutputText)

        TableStream.WriteLine(OutputText)
        TableStream.Flush()
        TableStream.Close()
        TableStream.Dispose()

    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub CreateIndex(ByVal TableName As String, ByVal IndexName As String, IndexFields As List(Of String),
                           Optional Unique As Boolean = False, Optional Clustered As Boolean = False)
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Converts a US formatted number (10,000.00) to a EU formatted number (10.000,00)
    ''' </summary>
    ''' <param name="USFormattedValue">The US formatted number</param>
    ''' <returns></returns>
    Private Function ConvertUStoEUDecimal(ByVal USFormattedValue As String) As String
        Dim TempString As String

        TempString = USFormattedValue

        ' First replace any periods with pipes
        TempString = TempString.Replace(".", "|")

        ' Now change the commas to periods
        TempString = TempString.Replace(",", ".")

        ' Now change the pipes to commas
        TempString = TempString.Replace("|", ",")

        ' Last update, re-set the names for R.A.M.s and R.Dbs back
        TempString = TempString.Replace("R,A,M,", "R.A.M.")
        TempString = TempString.Replace("R,Db", "R.Db")

        Return TempString

    End Function

    ''' <summary>
    ''' Inserts the list of record values (fields) into the table name provided.
    ''' </summary>
    ''' <param name="TableName">Table to insert records.</param>
    ''' <param name="Record">List of table fields that make up the record.</param>
    Public Sub InsertRecord(ByVal TableName As String, Record As List(Of DBField))
        Dim TableStream As StreamWriter
        Dim OutputText As String = ""

        ' Open the file
        Try
            TableStream = File.AppendText(DB & TableName & CSVExtention)
        Catch ex As Exception
            ShowErrorMessage(ex)
            Exit Sub
        End Try

        For Each Field In Record
            If (Field.FieldType = FieldType.double_type Or Field.FieldType = FieldType.float_type Or Field.FieldType = FieldType.real_type) And DELIMITER = SEMICOLON Then
                ' Need to format for comma as a decimal
                Field.FieldValue = ConvertUStoEUDecimal(Field.FieldValue)
            End If

            If UseNullforBlanks And Field.FieldValue = "" Then
                Field.FieldValue = NULL
            End If

            OutputText = OutputText & Field.FieldValue & DELIMITER
        Next

        ' Strip last seperator
        OutputText = StripLastCharacter(OutputText)

        TableStream.WriteLine(OutputText)
        TableStream.Flush()
        TableStream.Close()
        TableStream.Dispose()

    End Sub

    ''' <summary>
    ''' Finalizes the data import. If translation tables were used, they will be imported here.
    ''' </summary>
    ''' <param name="Translator">YAMLTranslations object to get stored tables from.</param>
    ''' <param name="TranslationTableImportList">List of translation tables to import.</param>
    Public Sub FinalizeDataImport(ByRef Translator As YAMLTranslations, ByVal TranslationTableImportList As List(Of String))
        Dim Tables As List(Of DataTable) = Translator.TranslationTables.GetTables
        Dim i As Integer

        Call InitalizeMainProgressBar(Tables.Count, "Importing Translation data...")

        ' Import the translation tables only if they were selected - otherwise skip
        For Each TTable In Tables
            i = 0
            If TranslationTableImportList.Contains(TTable.TableName) Then

                Call UpdateMainProgressBar(i, "Importing " & TTable.TableName)

                For Each row As DataRow In TTable.Rows
                    Dim DataFields As New List(Of DBField)

                    If TTable.TableName = YAMLTranslations.trnTranslationColumnsTable Then
                        DataFields.Add(BuildDatabaseField("columnName", CType(row.Item(0), Object), FieldType.nvarchar_type))
                        DataFields.Add(BuildDatabaseField("masterID", CType(row.Item(1), Object), FieldType.nvarchar_type))
                        DataFields.Add(BuildDatabaseField("tableName", CType(row.Item(2), Object), FieldType.nvarchar_type))
                        DataFields.Add(BuildDatabaseField("tcGroupID", CType(row.Item(3), Object), FieldType.smallint_type))
                        DataFields.Add(BuildDatabaseField("tcID", CType(row.Item(4), Object), FieldType.smallint_type))

                    ElseIf TTable.TableName = YAMLTranslations.trnTranslationLanguagesTable Then
                        DataFields.Add(BuildDatabaseField("languageID", CType(row.Item(0), Object), FieldType.varchar_type))
                        DataFields.Add(BuildDatabaseField("languageName", CType(row.Item(1), Object), FieldType.nvarchar_type))

                    ElseIf TTable.TableName = YAMLTranslations.trnTranslationsTable Then
                        DataFields.Add(BuildDatabaseField("keyID", CType(row.Item(0), Object), FieldType.int_type))
                        DataFields.Add(BuildDatabaseField("languageID", CType(row.Item(1), Object), FieldType.varchar_type))
                        DataFields.Add(BuildDatabaseField("tcID", CType(row.Item(2), Object), FieldType.smallint_type))
                        DataFields.Add(BuildDatabaseField("text", CType(row.Item(3), Object), FieldType.nvarchar_type))

                    End If

                    Call InsertRecord(TTable.TableName, DataFields)

                Next

            Else
                ' "drop table" if it exists
                If File.Exists(DB & TTable.TableName & CSVExtention) Then
                    File.Delete(DB & TTable.TableName & CSVExtention)
                End If
            End If
        Next

        Call ClearMainProgressBar()

    End Sub

End Class