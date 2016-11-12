
Public Class frmError

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        End
    End Sub

    Private Sub frmError_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        txtError.Text = frmErrorText
        Me.Activate()
    End Sub

End Class