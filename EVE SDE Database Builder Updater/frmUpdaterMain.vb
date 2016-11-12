
Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.ComponentModel
Imports System.Globalization ' For culture info
Imports System.Threading

Delegate Sub UpdateStatusSafe(ByVal pgBarVisible As Boolean, ByVal lblText As String)
Delegate Sub UpdatePGBarSafe(ByVal pgBarValue As Integer)

Public Class frmUpdaterMain

    Public Structure FileEntry
        Dim Name As String
        Dim Version As String
        Dim URL As String
        Dim MD5 As String
    End Structure

    Private Enum SettingTypes
        TypeInteger = 1
        TypeDouble = 2
        TypeString = 3
        TypeBoolean = 4
        TypeLong = 5
    End Enum

#Region "Delegate Functions"

    Public Sub UpdateStatus(ByVal pgBarVisible As Boolean, ByVal lblText As String)
        pgUpdate.Visible = pgBarVisible
        If lblText <> "" Then
            lblUpdateMain.Text = lblText
        End If
    End Sub

    ' Updates the value in the progressbar for a smooth progress (slows procesing a little) - total hack from this: http://stackoverflow.com/questions/977278/how-can-i-make-the-progress-bar-update-fast-enough/1214147#1214147
    Public Sub UpdateProgressBar(inValue As Integer)
        If inValue <= pgUpdate.Maximum - 1 And inValue <> 0 Then
            pgUpdate.Value = inValue
            pgUpdate.Value = pgUpdate.Value - 1
            pgUpdate.Value = inValue
        Else
            pgUpdate.Value = inValue
        End If
    End Sub

#End Region

    ' Worker
    Public worker As BackgroundWorker
    Public LocalXMLFileName As String

    Public UpdateFileList As New List(Of FileEntry) ' List of files that need updating, will download and rename all at the same time

    Public Const XMLLatestVersionFileName As String = "LatestVersionESDEDB.xml"
    Public Const UpdaterFileName As String = "ESDEDB Updater.exe"
    Public Const UpdatePath As String = "Updates\"
    Public UserWorkingFolder As String = "" ' Where the DB and updater and anything that changes files will be

    ' File Path
    Public Const XMLUpdateFileURL = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/LatestVersionESDEDB.xml"

    ' For tracking an error
    Public ProgramErrorLocation As String
    Public SQL As String ' Keep global so I can put in error log
    Public ThrownError As String

    Public UPDATES_FOLDER As String ' Where Updates will take place 
    Public ROOT_FOLDER As String ' Where the root folder is located
    Public SHELL_PATH As String ' Where to shell back to

    Public Const MainEXE As String = "EVE SDE Database Builder.exe" ' For Shelling

    Public Const NO_LOCAL_XML_FILE As String = "NO LOCAL XML FILE"

    Public Const OLD_PREFIX As String = "OLD_"

    Public LocalCulture As New CultureInfo("en-US")

    Public Sub New()
        Dim UserPath As String = ""

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Update folder path
        UPDATES_FOLDER = UpdatePath
        ROOT_FOLDER = ""

        SHELL_PATH = ROOT_FOLDER & MainEXE

        ' Set the version of the XML file we will use
        LocalXMLFileName = XMLLatestVersionFileName

        ' Create the updates folder
        If Directory.Exists(UPDATES_FOLDER) Then
            ' Delete what is there and replace
            Dim ImageDir As New DirectoryInfo(UPDATES_FOLDER)
            ImageDir.Delete(True)
        End If

        ' Create the new folder
        Directory.CreateDirectory(UPDATES_FOLDER)

        BGWorker.WorkerReportsProgress = True
        BGWorker.WorkerSupportsCancellation = True

        pgUpdate.Value = 0
        pgUpdate.Visible = False
        pgUpdate.Maximum = 100

        ProgramErrorLocation = ""
        ThrownError = ""

        Me.Focus()

    End Sub

    ' This event handler is where the time-consuming work is done.
    Private Sub BGWorker_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BGWorker.DoWork
        worker = CType(sender, BackgroundWorker)
        Dim ProgressCounter As Integer

        Dim m_xmld As New XmlDocument
        Dim m_nodelist As XmlNodeList
        Dim m_node As XmlNode

        Dim LocalFileMD5 As String = ""
        Dim EVEImagesNewLocalFolderName As String = "" ' This is the name of the folder we are going to unzip to
        Dim EVEDBLocalFileVersion As String = "" ' Local DB version

        Dim TempFile As FileEntry
        Dim ServerFileList As New List(Of FileEntry)
        Dim LocalFileList As New List(Of FileEntry)

        Dim i, j As Integer
        Dim RecordCount As Integer
        Dim CheckFile As String ' For checking if the file downloads or not
        Dim UpdateComplete As Boolean = False

        ' XML Temp file path for server file
        Dim ServerXMLLastUpdatePath As String

        ' Delegate for updating status
        Dim UpdateStatusDelegate As UpdateStatusSafe

        Dim VersionNumber As Double = 0

        Dim TempAccessMask As String
        Dim TempExpDate As Date
        Dim TempAccountType As String

        Dim HavePrecentiles As Boolean = False
        Dim HaveNewAPIFields As Boolean = False
        Dim HaveNewEVEIPHFields As Boolean = False
        Dim HasOldOutpostDataField As Boolean = False
        Dim HaveNewIndustryJobsTable As Boolean = False
        Dim HaveNewItemPricesFields As Boolean = False
        Dim HaveNewOwnedBPTable As Boolean = False

        'Create a variable tracking times
        Dim Count As Long = 0
        Dim Counter As Long = 0

        '================================================
        On Error GoTo 0

        Application.UseWaitCursor = True

        ' Sets the CurrentCulture 
        Thread.CurrentThread.CurrentCulture = LocalCulture

        UpdateStatusDelegate = New UpdateStatusSafe(AddressOf UpdateStatus)
        Me.Invoke(UpdateStatusDelegate, False, "Checking for Updates...")

        ' Get the newest update file from server
        ServerXMLLastUpdatePath = DownloadFileFromServer(XMLUpdateFileURL, UPDATES_FOLDER & LocalXMLFileName)

        If ServerXMLLastUpdatePath <> "" Then
            ' Load the server xml file to check for updates 
            m_xmld.Load(ServerXMLLastUpdatePath)

            m_nodelist = m_xmld.SelectNodes("/EVEIPH/result/rowset/row")

            ' Loop through the nodes 
            For Each m_node In m_nodelist
                ' Load all except updater
                If m_node.Attributes.GetNamedItem("Name").Value <> UpdaterFileName Then
                    TempFile.Name = m_node.Attributes.GetNamedItem("Name").Value
                    TempFile.Version = m_node.Attributes.GetNamedItem("Version").Value
                    TempFile.MD5 = m_node.Attributes.GetNamedItem("MD5").Value
                    TempFile.URL = m_node.Attributes.GetNamedItem("URL").Value
                    ' Insert the file
                    ServerFileList.Add(TempFile)
                End If
            Next
        Else
            ' Didn't download properly
            GoTo RevertToOldFileVersions
        End If

        If File.Exists(ROOT_FOLDER & LocalXMLFileName) Then
            ' Load the local xml file to check for updates for the DB and images
            m_xmld.Load(ROOT_FOLDER & LocalXMLFileName)
            m_nodelist = m_xmld.SelectNodes("/EVEIPH/result/rowset/row")

            ' Loop through the nodes 
            For Each m_node In m_nodelist
                ' Load all except updater
                If m_node.Attributes.GetNamedItem("Name").Value <> UpdaterFileName Then
                    TempFile.Name = m_node.Attributes.GetNamedItem("Name").Value
                    TempFile.Version = m_node.Attributes.GetNamedItem("Version").Value
                    TempFile.MD5 = m_node.Attributes.GetNamedItem("MD5").Value
                    TempFile.URL = m_node.Attributes.GetNamedItem("URL").Value
                    ' Insert the file
                    LocalFileList.Add(TempFile)
                End If
            Next
        End If

        ' Done with these
        m_xmld = Nothing
        m_nodelist = Nothing
        m_node = Nothing

        Me.Invoke(UpdateStatusDelegate, False, "Downloading Updates...")
        Application.DoEvents()

        ' Now download all in the list if the server has newer versions
        RecordCount = ServerFileList.Count - 1
        For i = 0 To RecordCount

            If (worker.CancellationPending = True) Then
                e.Cancel = True
                Exit Sub
            End If

            ' All files other than the DB and Updater are run from the Root folder - Images and db a special exception above
            LocalFileMD5 = MD5CalcFile(ROOT_FOLDER & ServerFileList(i).Name)

            ' Compare the MD5's and see if we download the new file
            If LocalFileMD5 <> ServerFileList(i).MD5 Then

                ' Need to update, download to updates folder for later update
                Me.Invoke(UpdateStatusDelegate, True, "")
                CheckFile = DownloadFileFromServer(ServerFileList(i).URL, UPDATES_FOLDER & ServerFileList(i).Name)

                If (worker.CancellationPending = True) Then
                    e.Cancel = True
                    Exit Sub
                End If

                If CheckFile = "" Then
                    ' Some error in downloading
                    ProgramErrorLocation = "Download Failed."
                    Exit Sub
                Else
                    ' Check the file MD5 to make sure we got a good download. If not, try one more time
                    ' If they don't have a local file (which will have a blank MD5) then just go with what they got
                    If ServerFileList(i).MD5 <> NO_LOCAL_XML_FILE Then
                        ' Get the file size to check
                        Dim infoReader As System.IO.FileInfo
                        infoReader = My.Computer.FileSystem.GetFileInfo(CheckFile)
                        ' Still bad MD5 or the file is 0 bytes
                        If MD5CalcFile(CheckFile) <> ServerFileList(i).MD5 Or infoReader.Length = 0 Then
                            CheckFile = DownloadFileFromServer(ServerFileList(i).URL, UPDATES_FOLDER & ServerFileList(i).Name)

                            If (worker.CancellationPending = True) Then
                                e.Cancel = True
                                Exit Sub
                            End If

                            If MD5CalcFile(CheckFile) <> ServerFileList(i).MD5 Or CheckFile = "" Then
                                ProgramErrorLocation = "Download Corrupted."
                                Exit Sub
                            End If
                        End If
                    End If
                End If
                ' Record the file we are upating
                UpdateFileList.Add(ServerFileList(i))
            End If

            Me.Invoke(UpdateStatusDelegate, False, "")
        Next

        ' Leave if nothing to update
        If IsNothing(UpdateFileList) Then
            Exit Sub
        End If

        ProgramErrorLocation = ""
        SQL = ""
        Application.DoEvents()

        ' If we screw up after this, we have to revert to anything we changed if possible
        On Error GoTo RevertToOldFileVersions

        ' Rename all files/folders with OLD and copy over new files/folders
        RecordCount = UpdateFileList.Count - 1

        For i = 0 To RecordCount
            Me.Invoke(UpdateStatusDelegate, False, "Copying Files...")
            If (worker.CancellationPending = True) Then
                e.Cancel = True
                Exit Sub
            Else
                ' Report progress.
                If RecordCount > 0 Then
                    worker.ReportProgress((i / RecordCount) + 1 * 10)
                End If
            End If

            Me.Invoke(UpdateStatusDelegate, False, "Updating " & UpdateFileList(i).Name & "...")

            ' If an OLD file exists, delete it
            If File.Exists(ROOT_FOLDER & OLD_PREFIX & UpdateFileList(i).Name) Then
                ProgramErrorLocation = "Error Deleting Old " & UpdateFileList(i).Name & "file"
                File.Delete(ROOT_FOLDER & OLD_PREFIX & UpdateFileList(i).Name)
                Application.DoEvents()
            End If

            ' Rename old file if it exists to old prefix
            If File.Exists(ROOT_FOLDER & UpdateFileList(i).Name) Then
                ProgramErrorLocation = "Error Moving Old " & UpdateFileList(i).Name & "file"
                File.Move(ROOT_FOLDER & UpdateFileList(i).Name, ROOT_FOLDER & OLD_PREFIX & UpdateFileList(i).Name)
                Application.DoEvents()
            End If

            ' Move new file
            ProgramErrorLocation = "Error Moving New " & UpdateFileList(i).Name & "file"
            File.Move(UPDATES_FOLDER & UpdateFileList(i).Name, ROOT_FOLDER & UpdateFileList(i).Name)
            Application.DoEvents()

        Next

        ProgramErrorLocation = ""
        Me.Invoke(UpdateStatusDelegate, False, "Cleaning up Temp Files...")

        Exit Sub

RevertToOldFileVersions:

        ' Output error first
        Call WriteMsgToLog(Err.Description)

        On Error Resume Next

        ' If we get here, try to delete everything we downloaded and rename any files saved as "Old" to original names
        ProgramErrorLocation = ProgramErrorLocation & " - Reverted to Old file versions"
        ' Save the error
        ThrownError = Err.Description

        ' Delete the updates folder
        If Directory.Exists(UPDATES_FOLDER) Then
            ' Delete what is there and replace
            Directory.Delete(UPDATES_FOLDER, True)
            Application.DoEvents()
        End If

        ' Rename all files/folders 
        If Not IsNothing(UpdateFileList) Then
            For i = 0 To UpdateFileList.Count - 1
                ' Only rename if old version exists
                If File.Exists(ROOT_FOLDER & OLD_PREFIX & UpdateFileList(i).Name) Then
                    ' Delete the new file
                    File.Delete(ROOT_FOLDER & UpdateFileList(i).Name)
                    Application.DoEvents()
                    ' Rename old file back 
                    File.Move(ROOT_FOLDER & OLD_PREFIX & UpdateFileList(i).Name, ROOT_FOLDER & UpdateFileList(i).Name)
                    Application.DoEvents()
                End If
            Next
        End If

        Exit Sub

    End Sub

    ' This event handler updates the progress.
    Private Sub BGWorker_ProgressChanged(ByVal sender As System.Object, ByVal e As ProgressChangedEventArgs) Handles BGWorker.ProgressChanged

        Dim safedelegate As New UpdatePGBarSafe(AddressOf UpdateProgressBar)
        Me.Invoke(safedelegate, e.ProgressPercentage) 'Invoke the TreadsafeDelegate

    End Sub

    ' Shows message box with message sent
    Private Sub ShowNotifyBox(LabelText As String)
        Dim f1 As New frmNotify
        f1.lblNotify.Text = LabelText
        f1.ShowDialog()
    End Sub

    ' This event handler deals with the results of the background operation.
    Private Sub BGWorker_RunWorkerCompleted(ByVal sender As System.Object, ByVal e As RunWorkerCompletedEventArgs) Handles BGWorker.RunWorkerCompleted
        Dim ErrorText As String = ""

        On Error Resume Next

        ' Allow the messagebox to pop up over the form now
        Me.TopMost = False

        ErrorText = e.Error.ToString

        ' Clean up all OLD files and folders that might be left around
        Call CleanUpOLDFiles()

        Application.UseWaitCursor = False

        If e.Cancelled = True Then
            lblUpdateMain.Text = "Update Canceled"
            Call ShowNotifyBox("Update Canceled")
        ElseIf (e.Error IsNot Nothing) Or (ProgramErrorLocation <> "") Then

            lblUpdateMain.Text = "Update Failed."

            ' Write sql and error to log
            If SQL <> "" Then
                ErrorText = ErrorText & " SQL: " & SQL
            End If

            Call WriteMsgToLog(ErrorText)

            Dim MainMessage As String = "There was an error in the update. Program not updated."

            If ThrownError <> "" And ProgramErrorLocation <> "" Then
                MsgBox(MainMessage & vbCrLf & ProgramErrorLocation & vbCrLf & "Error: " & ThrownError, vbCritical, Application.ProductName)
            ElseIf ProgramErrorLocation <> "" Then
                MsgBox(MainMessage & vbCrLf & ProgramErrorLocation, vbCritical, Application.ProductName)
            ElseIf ThrownError <> "" Then
                MsgBox(MainMessage & vbCrLf & "Error: " & ThrownError, vbCritical, Application.ProductName)
            Else
                Call ShowNotifyBox(MainMessage)
            End If

        Else
            Me.Hide()
            lblUpdateMain.Text = "Update Complete."
            ' We have completed the update
            ' Copy over the old XML file and delete the old
            If File.Exists(ROOT_FOLDER & LocalXMLFileName) Then
                File.Delete(ROOT_FOLDER & LocalXMLFileName)
            End If

            File.Move(UPDATES_FOLDER & LocalXMLFileName, ROOT_FOLDER & LocalXMLFileName)

            ' Finally delete the updates folder
            Directory.Delete(UPDATES_FOLDER, True)

            ' Wait for a second before running - might solve the problem with incorrectly suggesting an update
            Thread.Sleep(1000)

            Call ShowNotifyBox("Update Complete!")
        End If

        ' Shell to program
        Dim ProcInfo As New ProcessStartInfo
        ProcInfo.FileName = SHELL_PATH
        ProcInfo.UseShellExecute = True
        ProcInfo.WindowStyle = ProcessWindowStyle.Normal

        Process.Start(SHELL_PATH)

        On Error Resume Next
        ' Done
        End

    End Sub

    Private Sub CleanUpOLDFiles()
        Dim ImageDir As DirectoryInfo

        On Error Resume Next

        For i = 0 To UpdateFileList.Count - 1
            ' Delete old file
            File.Delete(ROOT_FOLDER & OLD_PREFIX & UpdateFileList(i).Name)
        Next
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If BGWorker.WorkerSupportsCancellation = True Then
            ' Cancel the asynchronous operation.
            BGWorker.CancelAsync()
        End If
    End Sub

    ' When the form is shown, run the updates
    Private Sub frmUpdaterMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown

        Me.Refresh()
        Application.DoEvents()
        Application.UseWaitCursor = True

        If BGWorker.IsBusy <> True Then
            ' Start the asynchronous operation.
            BGWorker.RunWorkerAsync()
        End If

        Me.Refresh()
        Application.DoEvents()
        Application.UseWaitCursor = False

    End Sub

    ' Downloads the sent file from server and saves it to the root directory as the sent file name
    Public Function DownloadFileFromServer(ByVal DownloadURL As String, ByVal FileName As String) As String
        'Creating the request and getting the response
        Dim Response As HttpWebResponse
        Dim Request As HttpWebRequest

        ' File sizes for progress bar
        Dim FileSize As Double

        ' For reading in chunks of data
        Dim readBytes(4095) As Byte
        ' Save in root directory
        Dim writeStream As New FileStream(FileName, FileMode.Create)
        Dim bytesread As Integer

        'Replacement for Stream.Position (webResponse stream doesn't support seek)
        Dim nRead As Long

        worker.ReportProgress(0)

        Try 'Checks if the file exist
            Request = DirectCast(HttpWebRequest.Create(DownloadURL), HttpWebRequest)
            'Request.Proxy = GetProxyData()
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

            If worker.CancellationPending Then 'If user abort download
                Exit Do
            End If

            bytesread = Response.GetResponseStream.Read(readBytes, 0, 4096)

            ' No more bytes to read
            If bytesread = 0 Then Exit Do

            nRead = nRead + bytesread
            ' Update progress 
            worker.ReportProgress((nRead * 100) / FileSize)

            writeStream.Write(readBytes, 0, bytesread)
        Loop

        'Close the streams
        Response.GetResponseStream.Close()
        writeStream.Close()

        Return FileName

    End Function

    ' MD5 Hash - specify the path to a file and this routine will calculate your hash
    Public Function MD5CalcFile(ByVal filepath As String) As String

        ' open file (as read-only)
        If File.Exists(filepath) Then
            Using reader As New System.IO.FileStream(filepath, IO.FileMode.Open, IO.FileAccess.Read)
                Using md5 As New System.Security.Cryptography.MD5CryptoServiceProvider

                    ' hash contents of this stream
                    Dim hash() As Byte = md5.ComputeHash(reader)

                    ' return formatted hash
                    Return ByteArrayToString(hash)

                End Using
            End Using
        End If

        ' Something went wrong
        Return ""

    End Function

    ' MD5 Hash - utility function to convert a byte array into a hex string
    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String

        Dim sb As New System.Text.StringBuilder(arrInput.Length * 2)

        For i As Integer = 0 To arrInput.Length - 1
            sb.Append(arrInput(i).ToString("X2"))
        Next

        Return sb.ToString().ToLower

    End Function

    ' Writes a sent message to a log file
    Public Sub WriteMsgToLog(ByVal ErrorMsg As String)
        Dim FilePath As String = "EVEIPH.log"
        Dim AllText() As String

        ' Only write to log if there is an error to write
        If Trim(ErrorMsg) <> "" Then
            If Not IO.File.Exists(FilePath) Then
                Dim sw As IO.StreamWriter = IO.File.CreateText(FilePath)
                sw.Close()
            End If

            ' This is an easier way to get all of the strings in the file.
            AllText = IO.File.ReadAllLines(FilePath)
            ' This will append the string to the end of the file.
            My.Computer.FileSystem.WriteAllText(FilePath, CStr(Now) & " - " & ErrorMsg & vbCrLf, True)
        End If

    End Sub

End Class