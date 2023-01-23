
Imports System.IO
Imports Npgsql
Imports System.Collections.Concurrent

''' <summary>
''' Class to create a postgreSQLDB and insert data into it.
''' </summary>
Public Class postgreSQLDB
    Inherits DBFilesBase

    Private BulkInsertTablesData As ConcurrentQueue(Of BulkInsertData)

    ' Save the database information for later connections
    Private DBServerName As String
    Private DBUserName As String
    Private DBUserPassword As String
    Private DBPort As String

    ' For inserting bulk data imports
    Private Structure BulkInsertData
        Dim BulkImportSQL As String ' The sql to do the bulk insert
        Dim TableName As String ' The table name that the data will insert into
    End Structure

    ''' <summary>
    ''' Constructor class for a postgreSQL database. Class connects to the default
    ''' postgres server with root UserName and password. If the database name sent 
    ''' does not exist, will create the database on the server.
    ''' </summary>
    ''' <param name="DatabaseName">Name of the database to create.</param>
    ''' <param name="ServerName">Name of the postgreSQL server (e.g. localhost')</param>
    ''' <param name="UserName">Root user name or user name with account access to create databases, tables, etc.</param>
    ''' <param name="Password">Root password or password of account with access to create databases, tables, etc.</param>
    ''' <param name="PortNum">Port Number for postgreSQL server</param>
    ''' <param name="Success">True if the database successfully created.</param>
    Public Sub New(ByVal DatabaseName As String, ByVal ServerName As String, ByVal UserName As String,
                   ByVal Password As String, ByVal PortNum As String, ByRef Success As Boolean)
        MyBase.New(DatabaseName, DatabaseType.PostgreSQL)

        Dim DB As New NpgsqlConnection

        BulkInsertTablesData = New ConcurrentQueue(Of BulkInsertData)
        CSVDirectory = ""

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try

            ' Set the DB Instance and Name data for later use
            MainDatabase = DatabaseName
            DBServerName = ServerName
            DBUserName = UserName
            DBUserPassword = Password
            DBPort = PortNum

            ' Open a default connection first to create the database if needed
            DB = DBConnectionRef("postgres")

            ' If the database doesn't exist, then create it
            Dim SQL As String = String.Format("SELECT 1 FROM pg_database WHERE datname = '{0}'", MainDatabase)
            Dim Command As New NpgsqlCommand(SQL, DB)

            If IsNothing(Command.ExecuteScalar) Then
                SQL = String.Format("CREATE DATABASE ""{0}"" WITH OWNER=postgres ENCODING='UTF8' CONNECTION LIMIT=-1", MainDatabase)
                Command = New NpgsqlCommand(SQL, DB)
                Command.ExecuteNonQuery()
                Command.Dispose()
            End If

            Call CloseDB(DB)

            Success = True

        Catch ex As Exception
            Call CloseDB(DB)
            Call ShowErrorMessage(ex)
            Success = False
        End Try

        Call ClearMainProgressBar()

    End Sub

    ''' <summary>
    ''' Opens a database connection to a postgreSQL server for
    ''' the database name given for use with the class.
    ''' </summary>
    ''' <param name="DatabaseName">The name of the database on the server to open a connection to.</param>
    ''' <returns>Returns a NgpsqlConnetion for use.</returns>
    Private Function DBConnectionRef(ByVal DatabaseName As String) As NpgsqlConnection

        ' Open the connection for reference
        Dim DBRef As New NpgsqlConnection(String.Format("Server={0};Port={1};Userid={2};password={3};Database={4};Timeout=30;CommandTimeout=30",
                                                        DBServerName, DBPort, DBUserName, DBUserPassword, DatabaseName))
        DBRef.Open()

        Return DBRef

    End Function

    ''' <summary>
    ''' Closes and disposes the referenced database.
    ''' </summary>
    ''' <param name="RefDB">The database to close.</param>
    Public Sub CloseDB(ByRef RefDB As NpgsqlConnection)
        On Error Resume Next
        RefDB.Close()
        RefDB.Dispose()
        On Error GoTo 0
    End Sub

    ''' <summary>
    ''' Executes the sent SQL on the main database for the class.
    ''' </summary>
    ''' <param name="SQL">SQL query to execute.</param>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        Dim DBRef As New NpgsqlConnection
        DBRef = DBConnectionRef(MainDatabase)

        Dim Command As New NpgsqlCommand(SQL, DBRef)
        Command.ExecuteNonQuery()
        Command.Dispose()
        Call CloseDB(DBRef)

    End Sub

    ''' <summary>
    ''' Begins an SQL transaction on the server
    ''' </summary>
    Public Sub BeginSQLTransaction()
        Call ExecuteNonQuerySQL("BEGIN;")
    End Sub

    ''' <summary>
    ''' Commits an SQL transaction on the server
    ''' </summary>
    Public Sub CommitSQLTransaction()
        Call ExecuteNonQuerySQL("COMMIT;")
    End Sub

    ''' <summary>
    ''' Rollsback an SQL transaction on the server
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Call ExecuteNonQuerySQL("ROLLBACK;")
    End Sub

    ''' <summary>
    ''' Drops the tablename sent, if it exists on the database.
    ''' </summary>
    ''' <param name="TableName">Table you want to drop</param>
    Private Sub DropTable(TableName As String)
        Call ExecuteNonQuerySQL(String.Format("DROP TABLE IF EXISTS public.""{0}""", TableName))
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

        Dim FieldList As String = ""

        ' Drop table first
        Call DropTable(TableName)

        ' Get all the PK fields
        For Each F In TableStructure
            If F.IsPrimaryKey Then
                PKFields.Add(F.FieldName)
            End If
        Next

        ' Now build the SQL and execute
        SQL = String.Format("CREATE TABLE ""{0}"" (", TableName)

        ' Add fields
        For Each Field In TableStructure
            With Field
                ' Add field name
                SQL &= .FieldName & SPACE

                ' Set field length
                If .FieldLength <> -1 Then
                    FieldLength = CStr(.FieldLength)
                Else
                    .FieldType = FieldType.text_type
                End If

                Select Case .FieldType
                    ' Format any strings
                    Case FieldType.char_type, FieldType.nchar_type
                        SQL &= "char(" & FieldLength & ")"
                    Case FieldType.varchar_type, FieldType.nvarchar_type
                        SQL &= "varchar(" & FieldLength & ")"
                    Case FieldType.text_type, FieldType.ntext_type
                        SQL &= "text"
                    Case FieldType.double_type, FieldType.float_type
                        SQL &= "double precision"
                    Case FieldType.real_type
                        SQL &= "real"
                    Case FieldType.smallint_type, FieldType.tinyint_type, FieldType.bit_type
                        SQL &= "smallint"
                    Case FieldType.int_type
                        SQL &= "integer"
                    Case FieldType.bigint_type
                        SQL &= "bigint"
                End Select

                ' If there is only one PK, then set it when creating the table
                If PKFields.Count = 1 And .IsPrimaryKey Then
                    SQL &= SPACE & "PRIMARY KEY" & SPACE
                End If

                If Not .IsNull Or .IsPrimaryKey Then ' All PKs are not null anyway
                    SQL &= SPACE & "NOT NULL" & COMMA
                Else
                    SQL &= COMMA
                End If

                FieldList = FieldList & .FieldName & COMMA

            End With
        Next

        If PKFields.Count > 1 Then
            ' Create PK constraint here
            SQL &= "PRIMARY KEY ("
            For Each PK In PKFields
                SQL &= PK & COMMA
            Next
            ' Strip the final comma and close
            SQL = StripLastCharacter(SQL) & ")"
        Else
            ' Strip the final comma
            SQL = StripLastCharacter(SQL)
        End If

        SQL &= ")"

        Call ExecuteNonQuerySQL(SQL)

        ' Strip comma
        FieldList = "(" & StripLastCharacter(FieldList) & ")"

        ' Insert the bulk data insert string for bulk insert later
        Dim TempData As BulkInsertData
        TempData.BulkImportSQL = String.Format("COPY ""{0}"" FROM '{1}/{0}.csv'  DELIMITER ',' HEADER QUOTE '""' CSV;", TableName, CSVDirectory.Replace("\", "/"))
        TempData.TableName = TableName

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

        SQL &= String.Format("INDEX {0} ON ""{1}"" (", IndexName, TableName)

        ' Build index fields
        For Each Field In IndexFields
            SQL &= Field & COMMA
        Next

        ' Strip the last comman
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
        Dim SQL As String = ""
        Dim Fields As String = ""
        Dim FieldValues As String = ""

        For Each Field In Record
            Fields = Fields & Field.FieldName & COMMA
            FieldValues = FieldValues & Field.FieldValue & COMMA
        Next

        ' Strip the last commas
        Fields = Fields.Substring(0, Len(Fields) - 1)
        FieldValues = FieldValues.Substring(0, Len(FieldValues) - 1)

        SQL = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", TableName, Fields, FieldValues)

        Call ExecuteNonQuerySQL(SQL)

    End Sub

    ''' <summary>
    ''' Finalizes the data import. If translation tables were used, they will be imported here.
    ''' </summary>
    ''' <param name="Translator">YAMLTranslations object to get stored tables from.</param>
    ''' <param name="TranslationTableImportList">List of translation tables to import.</param>
    Public Sub FinalizeDataImport(ByRef Translator As YAMLTranslations, ByVal TranslationTableImportList As List(Of String))
        Dim i As Integer = 0

        Call InitalizeMainProgressBar(BulkInsertTablesData.Count, "Importing Bulk Data...")
        Try
            For i = 0 To BulkInsertTablesData.Count - 1
                Call UpdateMainProgressBar(i, "Bulk Loading " & BulkInsertTablesData(i).TableName & "...")
                Debug.Print(BulkInsertTablesData(i).TableName)
                Application.DoEvents()

                Call BeginSQLTransaction()
                Call ExecuteNonQuerySQL(BulkInsertTablesData(i).BulkImportSQL)
                Call CommitSQLTransaction()

                ' Since we are done, delete the csv file we just imported
                Call File.Delete(CSVDirectory & BulkInsertTablesData(i).TableName & ".csv")

            Next
        Catch ex As Exception
            MsgBox("Table: " & BulkInsertTablesData(i).TableName & " did not import. Error: " & ex.Message, vbExclamation, Application.ProductName)
        End Try

        Call ClearMainProgressBar()

    End Sub

End Class