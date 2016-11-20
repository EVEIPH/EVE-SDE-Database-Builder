<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmThreadSelect
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmThreadSelect))
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lblNotify = New System.Windows.Forms.Label()
        Me.rbtnMaxThreads = New System.Windows.Forms.RadioButton()
        Me.rbtnNumberofThreads = New System.Windows.Forms.RadioButton()
        Me.lstNumThreads = New System.Windows.Forms.ListBox()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(15, 150)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(85, 28)
        Me.btnSave.TabIndex = 2
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(115, 150)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(85, 28)
        Me.btnClose.TabIndex = 3
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'lblNotify
        '
        Me.lblNotify.Location = New System.Drawing.Point(15, 9)
        Me.lblNotify.Name = "lblNotify"
        Me.lblNotify.Size = New System.Drawing.Size(185, 62)
        Me.lblNotify.TabIndex = 4
        Me.lblNotify.Text = "Select the number of Threads you want to use to run the import. Select Max Thread" &
    "s for no limit or set a number from 1 to 24."
        Me.lblNotify.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'rbtnMaxThreads
        '
        Me.rbtnMaxThreads.AutoSize = True
        Me.rbtnMaxThreads.Location = New System.Drawing.Point(30, 74)
        Me.rbtnMaxThreads.Name = "rbtnMaxThreads"
        Me.rbtnMaxThreads.Size = New System.Drawing.Size(87, 17)
        Me.rbtnMaxThreads.TabIndex = 5
        Me.rbtnMaxThreads.TabStop = True
        Me.rbtnMaxThreads.Text = "Max Threads"
        Me.rbtnMaxThreads.UseVisualStyleBackColor = True
        '
        'rbtnNumberofThreads
        '
        Me.rbtnNumberofThreads.AutoSize = True
        Me.rbtnNumberofThreads.Location = New System.Drawing.Point(30, 97)
        Me.rbtnNumberofThreads.Name = "rbtnNumberofThreads"
        Me.rbtnNumberofThreads.Size = New System.Drawing.Size(119, 17)
        Me.rbtnNumberofThreads.TabIndex = 6
        Me.rbtnNumberofThreads.TabStop = True
        Me.rbtnNumberofThreads.Text = "Number of Threads:"
        Me.rbtnNumberofThreads.UseVisualStyleBackColor = True
        '
        'lstNumThreads
        '
        Me.lstNumThreads.FormattingEnabled = True
        Me.lstNumThreads.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24"})
        Me.lstNumThreads.Location = New System.Drawing.Point(155, 97)
        Me.lstNumThreads.Name = "lstNumThreads"
        Me.lstNumThreads.Size = New System.Drawing.Size(37, 30)
        Me.lstNumThreads.TabIndex = 7
        '
        'frmThreadSelect
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(215, 190)
        Me.Controls.Add(Me.lstNumThreads)
        Me.Controls.Add(Me.rbtnNumberofThreads)
        Me.Controls.Add(Me.rbtnMaxThreads)
        Me.Controls.Add(Me.lblNotify)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmThreadSelect"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Select Threads"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents lblNotify As Label
    Friend WithEvents rbtnMaxThreads As RadioButton
    Friend WithEvents rbtnNumberofThreads As RadioButton
    Friend WithEvents lstNumThreads As ListBox
End Class
