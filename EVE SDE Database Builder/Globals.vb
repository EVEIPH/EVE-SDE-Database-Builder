
' Common types and variables for the program
Public Module Globals

    Public CancelImport As Boolean = False

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

    ' Initializes the main form grid 
    Private Delegate Sub InitRow(ByVal Position As Integer)
    Public Sub InitGridRow(ByVal Postion As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New InitRow(AddressOf f1.InitGridRow), Postion)

    End Sub

    ' Updates the main form grid
    Private Delegate Sub UpdateRowProgress(ByVal Position As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)
    Public Sub UpdateGridRowProgress(ByVal Postion As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New UpdateRowProgress(AddressOf f1.UpdateGridRowProgress), Postion, Count, TotalRecords)

    End Sub

    ' Finalizes the main form grid
    Private Delegate Sub FinalizeRow(ByVal Position As Integer)
    Public Sub FinalizeGridRow(ByVal Postion As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New FinalizeRow(AddressOf f1.FinalizeGridRow), Postion)

    End Sub

    ' Initializes the progressbar on the main form
    Private Delegate Sub InitMainProgress(MaxCount As Long, UpdateText As String)
    Public Sub InitalizeMainProgressBar(MaxCount As Long, UpdateText As String)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New InitMainProgress(AddressOf f1.InitalizeProgress), MaxCount, UpdateText)

    End Sub

    ' Clears the progress bar and label on the main form
    Private Delegate Sub ClearMainProgress()
    Public Sub ClearMainProgressBar()

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New ClearMainProgress(AddressOf f1.ClearProgress))

    End Sub

    ' Updates the main progress bar and label on the main form
    Private Delegate Sub UpdateMainProgress(ByVal Count As Long, ByVal UpdateText As String)
    Public Sub UpdateMainProgressBar(Count As Long, UpdateText As String)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New UpdateMainProgress(AddressOf f1.UpdateProgress), Count, UpdateText)

    End Sub

End Module
