Imports System.Net
Imports System.IO

' Common types and variables for the program
Public Module Globals

    Public CancelImport As Boolean = False
    Public CancelDownload As Boolean = False
    Public frmErrorText As String = ""
    Public Lock As New Object
    Public AllSettings As New ProgramSettings
    Public UserApplicationSettings As ApplicationSettings

    Public TestForSDEChanges As Boolean

    Public RetryCall As Boolean = False

    ''' <summary>
    ''' Displays error message from Try/Catch Exceptions
    ''' </summary>
    ''' <param name="ex">Exepction variable for displaying exception text</param>
    Public Sub ShowErrorMessage(ex As Exception)
        Dim msg As String = ex.Message
        If Not CancelImport Then
            If Not IsNothing(ex.InnerException) Then
                msg &= vbCrLf & vbCrLf & "Inner Exception: " & ex.InnerException.ToString
            End If
            Call MsgBox(msg, vbExclamation, Application.ProductName)
        End If
    End Sub

    ''' <summary>
    ''' Writes a sent message to the Errors.log file
    ''' </summary>
    ''' <param name="ErrorMsg">Message to write to log file</param>
    Public Sub WriteMsgToErrorLog(ByVal ErrorMsg As String)
        Call OutputToFile("Errors.log", ErrorMsg)
    End Sub

    ''' <summary>
    ''' Writes a sent message to the OutputLog.log file
    ''' </summary>
    ''' <param name="OutputMessage">Message to write to log file</param>
    Public Sub OutputMsgtoLog(ByVal OutputMessage As String)
        Call OutputToFile("OutputLog.log", OutputMessage)
    End Sub

    ''' <summary>
    ''' Outputs sent output text to the file path
    ''' </summary>
    ''' <param name="FilePath">Path of outputfile with name</param>
    ''' <param name="OutputText">Text to output to file</param>
    Private Sub OutputToFile(FilePath As String, OutputText As String)
        Dim AllText() As String

        If Not IO.File.Exists(FilePath) Then
            Dim sw As IO.StreamWriter = IO.File.CreateText(FilePath)
            sw.Close()
        End If

        ' This is an easier way to get all of the strings in the file.
        AllText = IO.File.ReadAllLines(FilePath)
        ' This will append the string to the end of the file.
        My.Computer.FileSystem.WriteAllText(FilePath, CStr(Now) & ", " & OutputText & Environment.NewLine, True)

    End Sub

    ' Initializes the main form grid 
    Private Delegate Sub InitRow(ByVal Position As Integer)
    Public Sub InitGridRow(ByVal Postion As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New InitRow(AddressOf f1.InitGridRow), Postion)
        Application.DoEvents()
    End Sub

    ' Updates the main form grid
    Private Delegate Sub UpdateRowProgress(ByVal Position As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)
    Public Sub UpdateGridRowProgress(ByVal Postion As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New UpdateRowProgress(AddressOf f1.UpdateGridRowProgress), Postion, Count, TotalRecords)
        Application.DoEvents()
    End Sub
    ''' <summary>
    ''' Downloads the sent file from server and saves it to the root directory as the sent file name
    ''' </summary>
    ''' <param name="DownloadURL">URL to download the file</param>
    ''' <param name="FileName">File name of downloaded file</param>
    ''' <returns>File Name of where the downloaded file was saved.</returns>
    Public Function DownloadFileFromServer(ByVal DownloadURL As String, ByVal FileName As String, Optional PGBar As ProgressBar = Nothing) As String
        'Creating the request and getting the response
        Dim Response As HttpWebResponse
        Dim Request As HttpWebRequest

        ' File sizes for progress bar
        Dim FileSize As Double

        ' For reading in chunks of data
        Dim readBytes(4095) As Byte
        ' Create directory if it doesn't exist already
        If Not Directory.Exists(Path.GetDirectoryName(FileName)) Then
            Directory.CreateDirectory(Path.GetDirectoryName(FileName))
        End If
        Dim writeStream As New FileStream(FileName, FileMode.Create)
        Dim bytesread As Integer

        'Replacement for Stream.Position (webResponse stream doesn't support seek)
        Dim nRead As Long

        If Not IsNothing(PGBar) Then
            PGBar.Minimum = 0
            PGBar.Value = 0
            PGBar.Visible = True
            Application.DoEvents()
        End If

        Try 'Checks if the file exist
            Request = DirectCast(HttpWebRequest.Create(DownloadURL), HttpWebRequest)
            Request.Proxy = Nothing
            Request.Credentials = CredentialCache.DefaultCredentials ' Added 9/27 to attempt to fix error: (407) Proxy Authentication Required.
            Request.Timeout = 50000
            Response = CType(Request.GetResponse, HttpWebResponse)
        Catch ex As Exception
            ' Set as empty and return
            writeStream.Close()
            Return ""
        End Try

        ' Get size
        FileSize = Response.ContentLength()

        ' Loop through and get the file in chunks, save out
        Do
            Application.DoEvents()

            If CancelDownload Then 'If user abort download
                Exit Do
            End If

            bytesread = Response.GetResponseStream.Read(readBytes, 0, 4096)

            ' No more bytes to read
            If bytesread = 0 Then Exit Do

            nRead = nRead + bytesread
            ' Update progress 
            If Not IsNothing(PGBar) Then
                PGBar.Value = (nRead * 100) / FileSize
            End If

            writeStream.Write(readBytes, 0, bytesread)
        Loop

        'Close the streams
        Response.GetResponseStream.Close()
        writeStream.Close()

        If Not IsNothing(PGBar) Then
            PGBar.Value = 0
            PGBar.Visible = False
        End If

        Return FileName

    End Function

    ' Finalizes the main form grid
    Private Delegate Sub FinalizeRow(ByVal Position As Integer)
    Public Sub FinalizeGridRow(ByVal Postion As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New FinalizeRow(AddressOf f1.FinalizeGridRow), Postion)
        Application.DoEvents()
    End Sub

    ' Initializes the progressbar on the main form
    Private Delegate Sub InitMainProgress(MaxCount As Long, UpdateText As String)
    Public Sub InitalizeMainProgressBar(MaxCount As Long, UpdateText As String)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New InitMainProgress(AddressOf f1.InitalizeProgress), MaxCount, UpdateText)
        Application.DoEvents()
    End Sub

    ' Clears the progress bar and label on the main form
    Private Delegate Sub ClearMainProgress()
    Public Sub ClearMainProgressBar()

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New ClearMainProgress(AddressOf f1.ClearProgress))
        Application.DoEvents()
    End Sub

    ' Updates the main progress bar and label on the main form
    Private Delegate Sub UpdateMainProgress(ByVal Count As Long, ByVal UpdateText As String)
    Public Sub UpdateMainProgressBar(Count As Long, UpdateText As String)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New UpdateMainProgress(AddressOf f1.UpdateProgress), Count, UpdateText)
        Application.DoEvents()
    End Sub

End Module
