Imports System.Collections.Concurrent

''' <summary>
''' Class to save a local database using System.Data classes
''' </summary>
Public Class LocalDatabase

    Private DataTablesList As New ConcurrentQueue(Of DataTable)

    Public Function GetTables() As List(Of DataTable)
        Return DataTablesList.ToList
    End Function

    ''' <summary>
    ''' Inserts a record into the table name sent. If the table doesn't exist, it creates it.
    ''' </summary>
    ''' <param name="TableNameRef">Table name to insert sent record</param>
    ''' <param name="SentRecord">Record to insert as a DB Field type</param>
    Public Sub InsertRecord(ByVal TableNameRef As String, ByVal SentRecord As List(Of DBField))
        ' Save data in new data table
        Dim DT As New DataTable
        Dim DC As DataColumn
        Dim FT As Type
        Dim TempRecordFieldValue As String = ""
        Dim Data(SentRecord.Count - 1) As Object

        DT = GetDataTable(TableNameRef)

        If DT.TableName = "" Then
            ' Add all the columns to the new table
            For i = 0 To SentRecord.Count - 1
                Select Case SentRecord(i).FieldType
                    Case FieldType.char_type, FieldType.nchar_type, FieldType.ntext_type, FieldType.nvarchar_type, FieldType.text_type, FieldType.varchar_type
                        FT = GetType(String)
                    Case FieldType.double_type, FieldType.float_type, FieldType.real_type
                        FT = GetType(Double)
                    Case FieldType.int_type, FieldType.bigint_type, FieldType.smallint_type, FieldType.tinyint_type
                        FT = GetType(Long)
                    Case FieldType.bit_type
                        FT = GetType(Boolean)
                    Case Else
                        FT = GetType(Object)
                End Select
                DC = New DataColumn(SentRecord(i).FieldName, FT)
                DC.AllowDBNull = True
                DT.Columns.Add(DC)
            Next
            ' Insert the table in the list for reference
            DT.TableName = TableNameRef
            DataTablesList.Enqueue(DT)
        End If

        ' Add a row to the existing table
        For i = 0 To SentRecord.Count - 1
            ' if the record has apostrophes on the first and last character, strip them for local db use
            If SentRecord(i).FieldValue <> "" Then
                If SentRecord(i).FieldValue.Substring(0, 1) = "'" And SentRecord(i).FieldValue.Substring(Len(SentRecord(i).FieldValue) - 1, 1) = "'" Then
                    ' Replace all the apostrophes
                    TempRecordFieldValue = Replace(SentRecord(i).FieldValue.Substring(1, Len(SentRecord(i).FieldValue) - 2), "''", "'")
                Else
                    TempRecordFieldValue = SentRecord(i).FieldValue
                End If
            Else
                TempRecordFieldValue = SentRecord(i).FieldValue
            End If

            If TempRecordFieldValue.ToUpper = "NULL" Or TempRecordFieldValue = "" Then
                Data(i) = DBNull.Value
            ElseIf SentRecord(i).FieldType = FieldType.bit_type Then
                ' Convert to true/false for bits since bulk insert doesn't accept 0/1
                Data(i) = CStr(CBool(TempRecordFieldValue))
            Else
                Data(i) = TempRecordFieldValue
            End If
        Next

        ' Add the new row
        Try
            Dim AddRow As DataRow
            AddRow = DT.NewRow
            AddRow.ItemArray = Data
            SyncLock Lock
                DT.Rows.Add(AddRow)
            End SyncLock
        Catch ex As Exception
            '  MsgBox("An import error occured with the Local Database: " & ex.Message & " Table: " & TableNameRef, vbInformation, "Import Error")
            Application.DoEvents()
        End Try

    End Sub

    ''' <summary>
    ''' Returns a reference to the data table in the class tables list
    ''' </summary>
    ''' <param name="TableNameRef">Table to search</param>
    ''' <returns>Reference to the DataTable for use</returns>
    Public Function GetDataTable(ByVal TableNameRef As String) As DataTable
        Dim ReturnTable As DataTable
        Dim TempList As New List(Of DataTable)

        TempList = GetTables()

        ' Find the table if it is in the list and return a reference to it
        For Each ReturnTable In TempList
            If ReturnTable.TableName = TableNameRef Then
                Return ReturnTable
            End If
        Next

        ReturnTable = New DataTable

        Return ReturnTable

    End Function

End Class
