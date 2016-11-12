Namespace My
    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication
        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException

            Call WriteMsgToLog(e.Exception.ToString)

            Dim f2 = New frmError
            frmErrorText = "An Unhandled Exception has occured and the program will now close."
            frmErrorText = frmErrorText & Environment.NewLine & "Copy the data below and send to developer."
            frmErrorText = frmErrorText & Environment.NewLine & Environment.NewLine & "Source: " & e.Exception.Source
            frmErrorText = frmErrorText & Environment.NewLine & "Message: " & e.Exception.Message & vbCrLf
            frmErrorText = frmErrorText & "Raw Error Text: " & e.Exception.ToString

            f2.ShowDialog()

        End Sub
    End Class
End Namespace
