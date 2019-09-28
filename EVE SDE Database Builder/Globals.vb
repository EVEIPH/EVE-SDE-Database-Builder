Imports System.Net
Imports System.Text

' Common types and variables for the program
Public Module Globals

    Public CancelImport As Boolean = False
    Public frmErrorText As String = ""

    Public AllSettings As New ProgramSettings
    Public UserApplicationSettings As ApplicationSettings

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
    Public Sub WriteMsgToLog(ByVal ErrorMsg As String)
        Dim FilePath As String = "Errors.log"
        Dim AllText() As String

        If Not IO.File.Exists(FilePath) Then
            Dim sw As IO.StreamWriter = IO.File.CreateText(FilePath)
            sw.Close()
        End If

        ' This is an easier way to get all of the strings in the file.
        AllText = IO.File.ReadAllLines(FilePath)
        ' This will append the string to the end of the file.
        My.Computer.FileSystem.WriteAllText(FilePath, CStr(Now) & ", " & ErrorMsg & Environment.NewLine, True)


    End Sub

    ''' <summary>
    ''' Queries the server for public data for the URL sent. If not found, returns nothing
    ''' </summary>
    ''' <param name="URL">Full public data URL as a string</param>
    ''' <returns>Byte Array of response or nothing if call fails</returns>
    Public Function GetPublicESIData(ByVal URL As String, ByRef CacheDate As Date, Optional BodyData As String = "") As String
        Dim Response As String = ""
        Dim WC As New WebClient
        Dim myWebHeaderCollection As New WebHeaderCollection
        Dim Expires As String = Nothing
        Dim Pages As Integer = Nothing

        Try

            WC.Proxy = Nothing

            If BodyData <> "" Then
                Response = Encoding.UTF8.GetString(WC.UploadData(URL, Encoding.UTF8.GetBytes(BodyData)))
            Else
                Response = WC.DownloadString(URL)
            End If

            ' Get the expiration date for the cache date
            myWebHeaderCollection = WC.ResponseHeaders
            Expires = myWebHeaderCollection.Item("Expires")
            Pages = CInt(myWebHeaderCollection.Item("X-Pages"))

            If Not IsNothing(Expires) Then
                CacheDate = CDate(Expires.Replace("GMT", "").Substring(InStr(Expires, ",") + 1)) ' Expiration date is in GMT
            Else
                CacheDate = #1/1/2200#
            End If

            If Not IsNothing(Pages) Then
                If Pages > 1 Then
                    Dim TempResponse As String = ""
                    For i = 2 To Pages
                        TempResponse = WC.DownloadString(URL & "&page=" & CStr(i))
                        ' Combine with the original response - strip the end and leading brackets
                        Response = Response.Substring(0, Len(Response) - 1) & "," & TempResponse.Substring(1)
                    Next
                End If
            End If

            Return Response

        Catch ex As WebException

            ' Retry call
            If CType(ex.Response, HttpWebResponse).StatusCode >= 500 And Not RetryCall Then
                RetryCall = True
                ' Try this call again after waiting a second
                Threading.Thread.Sleep(1000)
                Return GetPublicESIData(URL, CacheDate, BodyData)
            End If

        Catch ex As Exception
            MsgBox("The request failed to get public data. " & ex.Message, vbInformation, Application.ProductName)
        End Try

        If Not IsNothing(Response) Then
            If Response <> "" Then
                Return Response
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If

    End Function

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
