
Imports System.IO
Imports Microsoft.Office.Interop.Access.Dao
Imports System.Collections.Concurrent

''' <summary>
''' Class to create a Microsoft Access database and insert data into it.
''' </summary>
Public Class msAccessDB
    Inherits DBFilesBase

    Private DB As Database
    Private DBE As DBEngine

    Private BulkInsertTablesData As ConcurrentQueue(Of BulkInsertData)

    ' For inserting bulk data
    Private Structure BulkInsertData
        Dim TableSQL As String
        Dim SchemaFile As String
    End Structure

    ''' <summary>
    ''' Constructor class for a Microsoft Access database. Class connects to the database
    ''' with the sent file path and password (optional). If the database name sent 
    ''' does not exist, will create the database on the server.
    ''' </summary>
    ''' <param name="DatabaseFileNameandPath">Name and file path of the database to create.</param>
    ''' <param name="DBPassword">Root password access to open the database and create tables, etc.</param>
    ''' <param name="Success">True if the database successfully created.</param>
    Public Sub New(ByVal DatabaseFileNameandPath As String, ByVal DBPassword As String, ByRef Success As Boolean)
        MyBase.New(DatabaseFileNameandPath, DatabaseType.MSAccess)

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        DBE = New DBEngine
        BulkInsertTablesData = New ConcurrentQueue(Of BulkInsertData)
        CSVDirectory = ""

        Dim PasswordString As String = ""
        If Trim(DBPassword) <> "" Then
            PasswordString = String.Format(";pwd={0}", DBPassword)
        End If

        Try
            ' Build a new database and deletes the old DB if it exists
            If Not File.Exists(DatabaseFileNameandPath) Then
                DB = DBE.CreateDatabase(DatabaseFileNameandPath, LanguageConstants.dbLangGeneral & PasswordString, DatabaseTypeEnum.dbVersion40)
                DBE.SetOption(SetOptionEnum.dbMaxLocksPerFile, 100000)
            End If

            ' Open new connection
            If PasswordString = "" Then
                DB = DBE.OpenDatabase(DatabaseFileNameandPath) ', False, False, ";pwd=123")
            Else
                DB = DBE.OpenDatabase(DatabaseFileNameandPath, False, False, PasswordString)
            End If

            Success = True

        Catch ex As Exception
            Call CloseDB()
            Call ShowErrorMessage(ex)
            Success = False
        End Try

    End Sub

    ''' <summary>
    ''' Closes the reference database and finalizes the class.
    ''' </summary>
    Protected Overrides Sub Finalize()
        Call CloseDB()
        MyBase.Finalize()
    End Sub

    ''' <summary>
    ''' Closes and disposes the local database.
    ''' </summary>
    Public Sub CloseDB()
        On Error Resume Next
        DB.Close()
        On Error GoTo 0
    End Sub

    ''' <summary>
    ''' Executes the sent SQL on the main database for the class.
    ''' </summary>
    ''' <param name="SQL">SQL query to execute.</param>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        DB.Execute(SQL)
    End Sub

    ''' <summary>
    ''' Begins an SQL transaction on the server
    ''' </summary>
    Public Sub BeginSQLTransaction()
        DBE.BeginTrans()
    End Sub

    ''' <summary>
    ''' Commits an SQL transaction on the server
    ''' </summary>
    Public Sub CommitSQLTransaction()
        DBE.CommitTrans()
    End Sub

    ''' <summary>
    ''' Rollsback an SQL transaction on the server
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        DBE.Rollback()
    End Sub

    ''' <summary>
    ''' Drops the tablename sent, if it exists on the database.
    ''' </summary>
    ''' <param name="TableName">Table you want to drop</param>
    Private Sub DropTable(TableName As String)
        Dim tblDef As TableDef
        Dim TableFound As Boolean = False

        For Each tblDef In DB.TableDefs
            If tblDef.Name = TableName Then
                TableFound = True
            End If
        Next

        If TableFound Then
            ' Table exists
            Call ExecuteNonQuerySQL("DROP TABLE " & TableName)
        End If

    End Sub

    ''' <summary>
    ''' Creates a table on the server for the sent table name in the structure sent by table structure
    ''' </summary>
    ''' <param name="TableName">Name of the table to create.</param>
    ''' <param name="TableStructure">List of table fields that define the table.</param>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Dim SQL As String = ""
        Dim FieldLength As String = "0" ' For char types
        Dim PKFields As New List(Of String)
        Dim SchemaString As String = ""
        Dim ColumnCount As Integer = 0
        Dim SchemaType As String = ""
        Dim SchemaFieldLength As String = ""

        Dim FieldList As String = ""

        ' Build the header for the schema file
        SchemaString = String.Format("[{0}.csv]", TableName) & Environment.NewLine
        SchemaString &= "ColNameHeader = True" & Environment.NewLine
        SchemaString &= "Format = CSVDelimited" & Environment.NewLine
        SchemaString &= "MaxScanRows = 0" & Environment.NewLine
        SchemaString &= "TextDelimiter=""" & Environment.NewLine

        ' Drop table if it exists
        Call DropTable(TableName)

        ' Get all the PK fields
        For Each F In TableStructure
            If F.IsPrimaryKey Then
                PKFields.Add(F.FieldName)
            End If
        Next

        ' Now build the SQL and execute
        SQL = String.Format("CREATE TABLE {0} (", TableName)

        ' Add fields
        For Each Field In TableStructure
            With Field
                ' Add field name
                SQL &= "[" & .FieldName & "]" & SPACE

                ' Set field length
                If .FieldLength = YAMLFilesBase.MaxFieldLen Then
                    FieldLength = "max"
                Else
                    FieldLength = CStr(.FieldLength)
                End If

                Select Case .FieldType
                    ' Format any strings
                    Case FieldType.nchar_type, FieldType.ntext_type, FieldType.nvarchar_type, FieldType.char_type, FieldType.varchar_type, FieldType.text_type
                        If FieldLength = "max" Or .FieldLength > 255 Then
                            SQL &= "Memo"
                            SchemaType = "Memo"
                        Else
                            SQL &= "Text(" & FieldLength & ")"
                            SchemaFieldLength = FieldLength
                            SchemaType = "Text"
                        End If
                    Case FieldType.double_type, FieldType.float_type, FieldType.real_type
                        SQL &= "Double"
                        SchemaType = "Double"
                    Case FieldType.bit_type
                        SQL &= "YesNo"
                        SchemaType = "Bit"
                    Case FieldType.int_type, FieldType.tinyint_type, FieldType.smallint_type
                        SQL &= "Integer"
                        SchemaType = "Integer"
                    Case FieldType.bigint_type
                        SQL &= "Long"
                        SchemaType = "Long"
                End Select

                If Not .IsNull Or .IsPrimaryKey Then ' All PKs are not null anyway
                    SQL &= SPACE & "NOT NULL" & SPACE
                End If

                ' If there is only one PK, then set it when creating the table
                If PKFields.Count = 1 And .IsPrimaryKey Then
                    SQL &= SPACE & "Primary Key" & COMMA
                Else
                    SQL &= COMMA
                End If

                ColumnCount += 1

                ' Save the field name for bulk
                FieldList = FieldList & "[" & .FieldName & "]" & COMMA

                ' Save the information for the schema file to use with bulk
                SchemaString &= String.Format("Col{0}={1} {2}", CStr(ColumnCount), .FieldName, SchemaType)

                ' Add width of text field if it's set
                If SchemaFieldLength <> "" Then
                    SchemaString &= SPACE & "Width" & SPACE & SchemaFieldLength
                End If

                ' Finalize Schema field
                SchemaString &= Environment.NewLine

                ' Reset
                SchemaFieldLength = ""

            End With
        Next

        ' Strip the final comma
        SQL = StripLastCharacter(SQL)
        SQL &= ")"

        Call ExecuteNonQuerySQL(SQL)

        ' Finally build any unique PK index
        If PKFields.Count > 1 Then
            ' Create a unique index now with multiple PK fields for SQLite
            Call CreateIndex(TableName, "IDX_" & TableName & "_TID_EID", PKFields, True)
        End If

        ' Strip comma
        FieldList = StripLastCharacter(FieldList)

        ' Insert the bulk data insert string for bulk insert later
        Dim TempData As BulkInsertData
        TempData.TableSQL = String.Format("INSERT INTO {0} ({1}) SELECT {1} FROM [Text;FMT=CSVDelimited;HDR=Yes;Database={2}].[{0}.csv]", TableName, FieldList, CSVDirectory)
        TempData.SchemaFile = SchemaString

        BulkInsertTablesData.Enqueue(TempData)

    End Sub

    ''' <summary>
    ''' Creates an index on the table sent using the fields sent.
    ''' </summary>
    ''' <param name="TableName">Table the index will apply to.</param>
    ''' <param name="IndexName">Unique name of the index on the database.</param>
    ''' <param name="IndexFields">Fields that make up the index.</param>
    ''' <param name="Unique">If the index is unique.</param>
    ''' <param name="Clustered">Optional - If the index is clustered or unclustered (not used).</param>
    Public Sub CreateIndex(ByVal TableName As String, ByVal IndexName As String, IndexFields As List(Of String),
                           Optional Unique As Boolean = False, Optional Clustered As Boolean = False)
        Dim SQL As String = ""

        SQL = "CREATE" & SPACE

        ' Unique index
        If Unique Then
            SQL &= "UNIQUE" & SPACE
        End If

        SQL &= String.Format("INDEX {0} ON {1} (", IndexName, TableName)

        ' Build index fields
        For Each Field In IndexFields
            SQL &= "[" & Field & "]" & COMMA
        Next

        ' Strip the last comma
        SQL = StripLastCharacter(SQL) & ")"

        ' Create index
        Call ExecuteNonQuerySQL(SQL)

    End Sub

    ''' <summary>
    ''' Inserts the list of record values (fields) into the table name provided.
    ''' </summary>
    ''' <param name="TableName">Table to insert records.</param>
    ''' <param name="Record">List of table fields that make up the record.</param>
    Public Sub InsertRecord(ByVal TableName As String, Record As List(Of DBField))
        Dim Fields As String = ""
        Dim FieldValues As String = ""

        For Each Field In Record
            Fields = Fields & "[" & Field.FieldName & "]" & COMMA
            FieldValues = FieldValues & Field.FieldValue & COMMA
        Next

        ' Strip the last commas
        Fields = Fields.Substring(0, Len(Fields) - 1)
        FieldValues = FieldValues.Substring(0, Len(FieldValues) - 1)

        Call ExecuteNonQuerySQL(String.Format("INSERT INTO {0} ({1}) VALUES ({2})", TableName, Fields, FieldValues))

    End Sub

    ''' <summary>
    ''' Finalizes the data import. If translation tables were used, they will be imported here.
    ''' </summary>
    ''' <param name="Translator">YAMLTranslations object to get stored tables from.</param>
    ''' <param name="TranslationTableImportList">List of translation tables to import.</param>
    Public Sub FinalizeDataImport(ByRef Translator As YAMLTranslations, ByVal TranslationTableImportList As List(Of String))
        ' Build the schema.ini data for this table to import with text bulk
        Dim TableStream As StreamWriter

        ' Insert each table
        For Each TableData In BulkInsertTablesData
            TableStream = File.CreateText(CSVDirectory & "\Schema.ini")
            TableStream.WriteLine(TableData.SchemaFile)
            TableStream.Flush()
            TableStream.Close()
            TableStream.Dispose()

            ' Bulk insert the data and commit each insert instead of using a transaction
            Call ExecuteNonQuerySQL(TableData.TableSQL)
            Call DBE.Idle()

            ' Remove schema file 
            Call File.Delete(CSVDirectory & "\Schema.ini")

            Application.DoEvents()

        Next

    End Sub

End Class