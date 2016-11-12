Imports System.IO

Public Class frmThreadSelect

    Public Threads As Integer ' send the number of threads before opening

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub frmThreadSelect_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' Set the threading defaults
        If Threads < 0 Then
            rbtnMaxThreads.Checked = True
        Else
            ' Load what we have
            rbtnNumberofThreads.Checked = True
            lstNumThreads.Text = CStr(Threads)
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Dispose()
    End Sub

    Private Sub rbtnNumberofThreads_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnNumberofThreads.CheckedChanged
        If rbtnNumberofThreads.Checked Then
            lstNumThreads.Enabled = True
        End If
    End Sub

    Private Sub rbtnMaxThreads_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMaxThreads.CheckedChanged
        If rbtnMaxThreads.Checked Then
            lstNumThreads.Enabled = False
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        If lstNumThreads.Text = "" Then
            MsgBox("No number selected", vbExclamation, Application.ProductName)
            lstNumThreads.Focus()
            Exit Sub
        End If

        If rbtnMaxThreads.Checked Then
            Threads = -1
        Else
            Threads = CInt(lstNumThreads.Text)
        End If

        ' Saves the number of threads in a simple text file
        Dim MyStream As StreamWriter
        MyStream = File.CreateText(frmMain.ThreadsFileName)
        MyStream.Write(CStr(Threads))
        MyStream.Flush()
        MyStream.Close()

        ' Save on the main form too
        frmMain.SelectedThreads = Threads

        MsgBox("Settings Saved", vbInformation, Application.ProductName)

    End Sub

End Class