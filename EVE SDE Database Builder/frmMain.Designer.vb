<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.btnBuildDatabase = New System.Windows.Forms.Button()
        Me.pnlMain = New System.Windows.Forms.StatusStrip()
        Me.lblStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.pgMain = New System.Windows.Forms.ToolStripProgressBar()
        Me.btnCheckNoGridItems = New System.Windows.Forms.Button()
        Me.btnCheckAllGridItems = New System.Windows.Forms.Button()
        Me.dgMain = New System.Windows.Forms.DataGridView()
        Me.FileSelect = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.FileName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Progress = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.txtServerName = New System.Windows.Forms.TextBox()
        Me.lblFinalDBPath = New System.Windows.Forms.Label()
        Me.btnSelectFinalDBPath = New System.Windows.Forms.Button()
        Me.lblFinalDBFolder = New System.Windows.Forms.Label()
        Me.lblDBName = New System.Windows.Forms.Label()
        Me.txtDBName = New System.Windows.Forms.TextBox()
        Me.btnSaveSettings = New System.Windows.Forms.Button()
        Me.lblSDEPath = New System.Windows.Forms.Label()
        Me.btnSelectSDEPath = New System.Windows.Forms.Button()
        Me.lblMediaFire = New System.Windows.Forms.Label()
        Me.FBDialog = New System.Windows.Forms.FolderBrowserDialog()
        Me.gbSelectDBType = New System.Windows.Forms.GroupBox()
        Me.gbOptions = New System.Windows.Forms.GroupBox()
        Me.lblServerName = New System.Windows.Forms.Label()
        Me.txtPort = New System.Windows.Forms.TextBox()
        Me.lblPort = New System.Windows.Forms.Label()
        Me.rbtnSQLiteDB = New System.Windows.Forms.RadioButton()
        Me.rbtnSQLServer = New System.Windows.Forms.RadioButton()
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.rbtnAccess = New System.Windows.Forms.RadioButton()
        Me.lblUserName = New System.Windows.Forms.Label()
        Me.rbtnCSV = New System.Windows.Forms.RadioButton()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.chkEUFormat = New System.Windows.Forms.CheckBox()
        Me.txtUserName = New System.Windows.Forms.TextBox()
        Me.rbtnMySQL = New System.Windows.Forms.RadioButton()
        Me.rbtnPostgreSQL = New System.Windows.Forms.RadioButton()
        Me.gbLanguage = New System.Windows.Forms.GroupBox()
        Me.rbtnKorean = New System.Windows.Forms.RadioButton()
        Me.rbtnChinese = New System.Windows.Forms.RadioButton()
        Me.rbtnRussian = New System.Windows.Forms.RadioButton()
        Me.rbtnJapanese = New System.Windows.Forms.RadioButton()
        Me.rbtnFrench = New System.Windows.Forms.RadioButton()
        Me.rbtnGerman = New System.Windows.Forms.RadioButton()
        Me.rbtnEnglish = New System.Windows.Forms.RadioButton()
        Me.gbFilePathSelect = New System.Windows.Forms.GroupBox()
        Me.pgBar = New System.Windows.Forms.ProgressBar()
        Me.btnCancelDownload = New System.Windows.Forms.Button()
        Me.btnDownloadSDE = New System.Windows.Forms.Button()
        Me.lblDownload = New System.Windows.Forms.Label()
        Me.lblDownloadFolderPath = New System.Windows.Forms.Label()
        Me.btnSelectDownloadPath = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SetThreadsUsedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CheckForUpdatesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeveloperToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PrepareFilesForUpdateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BuildBinaryToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TestForSDEChangesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.pnlMain.SuspendLayout()
        CType(Me.dgMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbSelectDBType.SuspendLayout()
        Me.gbOptions.SuspendLayout()
        Me.gbLanguage.SuspendLayout()
        Me.gbFilePathSelect.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnBuildDatabase
        '
        Me.btnBuildDatabase.Location = New System.Drawing.Point(81, 621)
        Me.btnBuildDatabase.Name = "btnBuildDatabase"
        Me.btnBuildDatabase.Size = New System.Drawing.Size(90, 30)
        Me.btnBuildDatabase.TabIndex = 2
        Me.btnBuildDatabase.Text = "Build Database"
        Me.btnBuildDatabase.UseVisualStyleBackColor = True
        '
        'pnlMain
        '
        Me.pnlMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblStatus, Me.pgMain})
        Me.pnlMain.Location = New System.Drawing.Point(0, 659)
        Me.pnlMain.Name = "pnlMain"
        Me.pnlMain.Size = New System.Drawing.Size(540, 22)
        Me.pnlMain.TabIndex = 6
        Me.pnlMain.Text = "Status"
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = False
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(300, 17)
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pgMain
        '
        Me.pgMain.Name = "pgMain"
        Me.pgMain.Size = New System.Drawing.Size(220, 16)
        Me.pgMain.Visible = False
        '
        'btnCheckNoGridItems
        '
        Me.btnCheckNoGridItems.Location = New System.Drawing.Point(437, 178)
        Me.btnCheckNoGridItems.Name = "btnCheckNoGridItems"
        Me.btnCheckNoGridItems.Size = New System.Drawing.Size(76, 23)
        Me.btnCheckNoGridItems.TabIndex = 7
        Me.btnCheckNoGridItems.Text = "Check None"
        Me.btnCheckNoGridItems.UseVisualStyleBackColor = True
        '
        'btnCheckAllGridItems
        '
        Me.btnCheckAllGridItems.Location = New System.Drawing.Point(355, 178)
        Me.btnCheckAllGridItems.Name = "btnCheckAllGridItems"
        Me.btnCheckAllGridItems.Size = New System.Drawing.Size(76, 23)
        Me.btnCheckAllGridItems.TabIndex = 6
        Me.btnCheckAllGridItems.Text = "Check All"
        Me.btnCheckAllGridItems.UseVisualStyleBackColor = True
        '
        'dgMain
        '
        Me.dgMain.AllowUserToAddRows = False
        Me.dgMain.AllowUserToDeleteRows = False
        Me.dgMain.AllowUserToResizeColumns = False
        Me.dgMain.AllowUserToResizeRows = False
        Me.dgMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgMain.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.FileSelect, Me.FileName, Me.Progress})
        Me.dgMain.Location = New System.Drawing.Point(8, 395)
        Me.dgMain.Name = "dgMain"
        Me.dgMain.RowHeadersVisible = False
        Me.dgMain.Size = New System.Drawing.Size(526, 220)
        Me.dgMain.TabIndex = 1
        '
        'FileSelect
        '
        Me.FileSelect.HeaderText = ""
        Me.FileSelect.Name = "FileSelect"
        Me.FileSelect.Width = 25
        '
        'FileName
        '
        Me.FileName.HeaderText = "File Name"
        Me.FileName.Name = "FileName"
        Me.FileName.Width = 225
        '
        'Progress
        '
        Me.Progress.HeaderText = "Progress"
        Me.Progress.Name = "Progress"
        Me.Progress.Width = 255
        '
        'txtServerName
        '
        Me.txtServerName.Location = New System.Drawing.Point(95, 92)
        Me.txtServerName.Name = "txtServerName"
        Me.txtServerName.Size = New System.Drawing.Size(100, 20)
        Me.txtServerName.TabIndex = 10
        '
        'lblFinalDBPath
        '
        Me.lblFinalDBPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblFinalDBPath.Location = New System.Drawing.Point(12, 155)
        Me.lblFinalDBPath.Name = "lblFinalDBPath"
        Me.lblFinalDBPath.Size = New System.Drawing.Size(501, 20)
        Me.lblFinalDBPath.TabIndex = 4
        Me.lblFinalDBPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'btnSelectFinalDBPath
        '
        Me.btnSelectFinalDBPath.Location = New System.Drawing.Point(12, 178)
        Me.btnSelectFinalDBPath.Name = "btnSelectFinalDBPath"
        Me.btnSelectFinalDBPath.Size = New System.Drawing.Size(55, 23)
        Me.btnSelectFinalDBPath.TabIndex = 5
        Me.btnSelectFinalDBPath.Text = "Select"
        Me.btnSelectFinalDBPath.UseVisualStyleBackColor = True
        '
        'lblFinalDBFolder
        '
        Me.lblFinalDBFolder.AutoSize = True
        Me.lblFinalDBFolder.Location = New System.Drawing.Point(9, 142)
        Me.lblFinalDBFolder.Name = "lblFinalDBFolder"
        Me.lblFinalDBFolder.Size = New System.Drawing.Size(113, 13)
        Me.lblFinalDBFolder.TabIndex = 3
        Me.lblFinalDBFolder.Text = "Final Database Folder:"
        '
        'lblDBName
        '
        Me.lblDBName.Location = New System.Drawing.Point(6, 72)
        Me.lblDBName.Name = "lblDBName"
        Me.lblDBName.Size = New System.Drawing.Size(87, 13)
        Me.lblDBName.TabIndex = 7
        Me.lblDBName.Text = "Database Name:"
        Me.lblDBName.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtDBName
        '
        Me.txtDBName.Location = New System.Drawing.Point(95, 68)
        Me.txtDBName.Name = "txtDBName"
        Me.txtDBName.Size = New System.Drawing.Size(100, 20)
        Me.txtDBName.TabIndex = 8
        '
        'btnSaveSettings
        '
        Me.btnSaveSettings.Location = New System.Drawing.Point(273, 621)
        Me.btnSaveSettings.Name = "btnSaveSettings"
        Me.btnSaveSettings.Size = New System.Drawing.Size(90, 30)
        Me.btnSaveSettings.TabIndex = 4
        Me.btnSaveSettings.Text = "Save Settings"
        Me.btnSaveSettings.UseVisualStyleBackColor = True
        '
        'lblSDEPath
        '
        Me.lblSDEPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblSDEPath.Location = New System.Drawing.Point(12, 31)
        Me.lblSDEPath.Name = "lblSDEPath"
        Me.lblSDEPath.Size = New System.Drawing.Size(501, 20)
        Me.lblSDEPath.TabIndex = 1
        Me.lblSDEPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'btnSelectSDEPath
        '
        Me.btnSelectSDEPath.Location = New System.Drawing.Point(12, 54)
        Me.btnSelectSDEPath.Name = "btnSelectSDEPath"
        Me.btnSelectSDEPath.Size = New System.Drawing.Size(55, 23)
        Me.btnSelectSDEPath.TabIndex = 2
        Me.btnSelectSDEPath.Text = "Select"
        Me.btnSelectSDEPath.UseVisualStyleBackColor = True
        '
        'lblMediaFire
        '
        Me.lblMediaFire.AutoSize = True
        Me.lblMediaFire.Location = New System.Drawing.Point(9, 18)
        Me.lblMediaFire.Name = "lblMediaFire"
        Me.lblMediaFire.Size = New System.Drawing.Size(83, 13)
        Me.lblMediaFire.TabIndex = 0
        Me.lblMediaFire.Text = "SDE File Folder:"
        '
        'gbSelectDBType
        '
        Me.gbSelectDBType.Controls.Add(Me.gbOptions)
        Me.gbSelectDBType.Controls.Add(Me.gbLanguage)
        Me.gbSelectDBType.Location = New System.Drawing.Point(8, 27)
        Me.gbSelectDBType.Name = "gbSelectDBType"
        Me.gbSelectDBType.Size = New System.Drawing.Size(525, 145)
        Me.gbSelectDBType.TabIndex = 23
        Me.gbSelectDBType.TabStop = False
        Me.gbSelectDBType.Text = "Select Options:"
        '
        'gbOptions
        '
        Me.gbOptions.Controls.Add(Me.lblServerName)
        Me.gbOptions.Controls.Add(Me.txtPort)
        Me.gbOptions.Controls.Add(Me.lblPort)
        Me.gbOptions.Controls.Add(Me.rbtnSQLiteDB)
        Me.gbOptions.Controls.Add(Me.rbtnSQLServer)
        Me.gbOptions.Controls.Add(Me.lblPassword)
        Me.gbOptions.Controls.Add(Me.rbtnAccess)
        Me.gbOptions.Controls.Add(Me.lblUserName)
        Me.gbOptions.Controls.Add(Me.rbtnCSV)
        Me.gbOptions.Controls.Add(Me.txtPassword)
        Me.gbOptions.Controls.Add(Me.chkEUFormat)
        Me.gbOptions.Controls.Add(Me.txtUserName)
        Me.gbOptions.Controls.Add(Me.rbtnMySQL)
        Me.gbOptions.Controls.Add(Me.lblDBName)
        Me.gbOptions.Controls.Add(Me.rbtnPostgreSQL)
        Me.gbOptions.Controls.Add(Me.txtServerName)
        Me.gbOptions.Controls.Add(Me.txtDBName)
        Me.gbOptions.Location = New System.Drawing.Point(6, 15)
        Me.gbOptions.Name = "gbOptions"
        Me.gbOptions.Size = New System.Drawing.Size(406, 124)
        Me.gbOptions.TabIndex = 0
        Me.gbOptions.TabStop = False
        Me.gbOptions.Text = "Database Type:"
        '
        'lblServerName
        '
        Me.lblServerName.Location = New System.Drawing.Point(6, 95)
        Me.lblServerName.Name = "lblServerName"
        Me.lblServerName.Size = New System.Drawing.Size(87, 13)
        Me.lblServerName.TabIndex = 17
        Me.lblServerName.Text = "Server Name:"
        Me.lblServerName.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtPort
        '
        Me.txtPort.Location = New System.Drawing.Point(367, 91)
        Me.txtPort.Name = "txtPort"
        Me.txtPort.Size = New System.Drawing.Size(34, 20)
        Me.txtPort.TabIndex = 16
        Me.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblPort
        '
        Me.lblPort.AutoSize = True
        Me.lblPort.Location = New System.Drawing.Point(367, 71)
        Me.lblPort.Name = "lblPort"
        Me.lblPort.Size = New System.Drawing.Size(29, 13)
        Me.lblPort.TabIndex = 15
        Me.lblPort.Text = "Port:"
        '
        'rbtnSQLiteDB
        '
        Me.rbtnSQLiteDB.AutoSize = True
        Me.rbtnSQLiteDB.Location = New System.Drawing.Point(9, 17)
        Me.rbtnSQLiteDB.Name = "rbtnSQLiteDB"
        Me.rbtnSQLiteDB.Size = New System.Drawing.Size(57, 17)
        Me.rbtnSQLiteDB.TabIndex = 0
        Me.rbtnSQLiteDB.Text = "SQLite"
        Me.rbtnSQLiteDB.UseVisualStyleBackColor = True
        '
        'rbtnSQLServer
        '
        Me.rbtnSQLServer.AutoSize = True
        Me.rbtnSQLServer.Location = New System.Drawing.Point(9, 37)
        Me.rbtnSQLServer.Name = "rbtnSQLServer"
        Me.rbtnSQLServer.Size = New System.Drawing.Size(126, 17)
        Me.rbtnSQLServer.TabIndex = 1
        Me.rbtnSQLServer.Text = "Microsoft SQL Server"
        Me.rbtnSQLServer.UseVisualStyleBackColor = True
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New System.Drawing.Point(203, 95)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(56, 13)
        Me.lblPassword.TabIndex = 13
        Me.lblPassword.Text = "Password:"
        '
        'rbtnAccess
        '
        Me.rbtnAccess.AutoSize = True
        Me.rbtnAccess.Location = New System.Drawing.Point(250, 17)
        Me.rbtnAccess.Name = "rbtnAccess"
        Me.rbtnAccess.Size = New System.Drawing.Size(106, 17)
        Me.rbtnAccess.TabIndex = 4
        Me.rbtnAccess.Text = "Microsoft Access"
        Me.rbtnAccess.UseVisualStyleBackColor = True
        '
        'lblUserName
        '
        Me.lblUserName.AutoSize = True
        Me.lblUserName.Location = New System.Drawing.Point(199, 71)
        Me.lblUserName.Name = "lblUserName"
        Me.lblUserName.Size = New System.Drawing.Size(60, 13)
        Me.lblUserName.TabIndex = 11
        Me.lblUserName.Text = "UserName:"
        '
        'rbtnCSV
        '
        Me.rbtnCSV.AutoSize = True
        Me.rbtnCSV.Location = New System.Drawing.Point(250, 37)
        Me.rbtnCSV.Name = "rbtnCSV"
        Me.rbtnCSV.Size = New System.Drawing.Size(46, 17)
        Me.rbtnCSV.TabIndex = 5
        Me.rbtnCSV.Text = "CSV"
        Me.rbtnCSV.UseVisualStyleBackColor = True
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(261, 91)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(100, 20)
        Me.txtPassword.TabIndex = 14
        '
        'chkEUFormat
        '
        Me.chkEUFormat.AutoSize = True
        Me.chkEUFormat.Location = New System.Drawing.Point(298, 37)
        Me.chkEUFormat.Name = "chkEUFormat"
        Me.chkEUFormat.Size = New System.Drawing.Size(107, 17)
        Me.chkEUFormat.TabIndex = 6
        Me.chkEUFormat.Text = "European Format"
        Me.chkEUFormat.UseVisualStyleBackColor = True
        '
        'txtUserName
        '
        Me.txtUserName.Location = New System.Drawing.Point(261, 67)
        Me.txtUserName.Name = "txtUserName"
        Me.txtUserName.Size = New System.Drawing.Size(100, 20)
        Me.txtUserName.TabIndex = 12
        '
        'rbtnMySQL
        '
        Me.rbtnMySQL.AutoSize = True
        Me.rbtnMySQL.Location = New System.Drawing.Point(148, 16)
        Me.rbtnMySQL.Name = "rbtnMySQL"
        Me.rbtnMySQL.Size = New System.Drawing.Size(60, 17)
        Me.rbtnMySQL.TabIndex = 2
        Me.rbtnMySQL.Text = "MySQL"
        Me.rbtnMySQL.UseVisualStyleBackColor = True
        '
        'rbtnPostgreSQL
        '
        Me.rbtnPostgreSQL.AutoSize = True
        Me.rbtnPostgreSQL.Location = New System.Drawing.Point(148, 36)
        Me.rbtnPostgreSQL.Name = "rbtnPostgreSQL"
        Me.rbtnPostgreSQL.Size = New System.Drawing.Size(82, 17)
        Me.rbtnPostgreSQL.TabIndex = 3
        Me.rbtnPostgreSQL.Text = "PostgreSQL"
        Me.rbtnPostgreSQL.UseVisualStyleBackColor = True
        '
        'gbLanguage
        '
        Me.gbLanguage.Controls.Add(Me.rbtnKorean)
        Me.gbLanguage.Controls.Add(Me.rbtnChinese)
        Me.gbLanguage.Controls.Add(Me.rbtnRussian)
        Me.gbLanguage.Controls.Add(Me.rbtnJapanese)
        Me.gbLanguage.Controls.Add(Me.rbtnFrench)
        Me.gbLanguage.Controls.Add(Me.rbtnGerman)
        Me.gbLanguage.Controls.Add(Me.rbtnEnglish)
        Me.gbLanguage.Location = New System.Drawing.Point(418, 15)
        Me.gbLanguage.Name = "gbLanguage"
        Me.gbLanguage.Size = New System.Drawing.Size(101, 124)
        Me.gbLanguage.TabIndex = 1
        Me.gbLanguage.TabStop = False
        Me.gbLanguage.Text = "Language:"
        '
        'rbtnKorean
        '
        Me.rbtnKorean.AutoSize = True
        Me.rbtnKorean.Location = New System.Drawing.Point(8, 104)
        Me.rbtnKorean.Name = "rbtnKorean"
        Me.rbtnKorean.Size = New System.Drawing.Size(59, 17)
        Me.rbtnKorean.TabIndex = 6
        Me.rbtnKorean.Text = "Korean"
        Me.rbtnKorean.UseVisualStyleBackColor = True
        '
        'rbtnChinese
        '
        Me.rbtnChinese.AutoSize = True
        Me.rbtnChinese.Location = New System.Drawing.Point(8, 89)
        Me.rbtnChinese.Name = "rbtnChinese"
        Me.rbtnChinese.Size = New System.Drawing.Size(63, 17)
        Me.rbtnChinese.TabIndex = 5
        Me.rbtnChinese.Text = "Chinese"
        Me.rbtnChinese.UseVisualStyleBackColor = True
        '
        'rbtnRussian
        '
        Me.rbtnRussian.AutoSize = True
        Me.rbtnRussian.Location = New System.Drawing.Point(8, 74)
        Me.rbtnRussian.Name = "rbtnRussian"
        Me.rbtnRussian.Size = New System.Drawing.Size(63, 17)
        Me.rbtnRussian.TabIndex = 4
        Me.rbtnRussian.Text = "Russian"
        Me.rbtnRussian.UseVisualStyleBackColor = True
        '
        'rbtnJapanese
        '
        Me.rbtnJapanese.AutoSize = True
        Me.rbtnJapanese.Location = New System.Drawing.Point(8, 59)
        Me.rbtnJapanese.Name = "rbtnJapanese"
        Me.rbtnJapanese.Size = New System.Drawing.Size(71, 17)
        Me.rbtnJapanese.TabIndex = 3
        Me.rbtnJapanese.Text = "Japanese"
        Me.rbtnJapanese.UseVisualStyleBackColor = True
        '
        'rbtnFrench
        '
        Me.rbtnFrench.AutoSize = True
        Me.rbtnFrench.Location = New System.Drawing.Point(8, 44)
        Me.rbtnFrench.Name = "rbtnFrench"
        Me.rbtnFrench.Size = New System.Drawing.Size(58, 17)
        Me.rbtnFrench.TabIndex = 2
        Me.rbtnFrench.Text = "French"
        Me.rbtnFrench.UseVisualStyleBackColor = True
        '
        'rbtnGerman
        '
        Me.rbtnGerman.AutoSize = True
        Me.rbtnGerman.Location = New System.Drawing.Point(8, 29)
        Me.rbtnGerman.Name = "rbtnGerman"
        Me.rbtnGerman.Size = New System.Drawing.Size(62, 17)
        Me.rbtnGerman.TabIndex = 1
        Me.rbtnGerman.Text = "German"
        Me.rbtnGerman.UseVisualStyleBackColor = True
        '
        'rbtnEnglish
        '
        Me.rbtnEnglish.AutoSize = True
        Me.rbtnEnglish.Location = New System.Drawing.Point(8, 14)
        Me.rbtnEnglish.Name = "rbtnEnglish"
        Me.rbtnEnglish.Size = New System.Drawing.Size(59, 17)
        Me.rbtnEnglish.TabIndex = 0
        Me.rbtnEnglish.Text = "English"
        Me.rbtnEnglish.UseVisualStyleBackColor = True
        '
        'gbFilePathSelect
        '
        Me.gbFilePathSelect.Controls.Add(Me.pgBar)
        Me.gbFilePathSelect.Controls.Add(Me.btnCancelDownload)
        Me.gbFilePathSelect.Controls.Add(Me.btnDownloadSDE)
        Me.gbFilePathSelect.Controls.Add(Me.lblDownload)
        Me.gbFilePathSelect.Controls.Add(Me.lblDownloadFolderPath)
        Me.gbFilePathSelect.Controls.Add(Me.btnSelectDownloadPath)
        Me.gbFilePathSelect.Controls.Add(Me.btnCheckNoGridItems)
        Me.gbFilePathSelect.Controls.Add(Me.btnCheckAllGridItems)
        Me.gbFilePathSelect.Controls.Add(Me.lblMediaFire)
        Me.gbFilePathSelect.Controls.Add(Me.btnSelectFinalDBPath)
        Me.gbFilePathSelect.Controls.Add(Me.lblSDEPath)
        Me.gbFilePathSelect.Controls.Add(Me.lblFinalDBFolder)
        Me.gbFilePathSelect.Controls.Add(Me.lblFinalDBPath)
        Me.gbFilePathSelect.Controls.Add(Me.btnSelectSDEPath)
        Me.gbFilePathSelect.Location = New System.Drawing.Point(8, 178)
        Me.gbFilePathSelect.Name = "gbFilePathSelect"
        Me.gbFilePathSelect.Size = New System.Drawing.Size(525, 211)
        Me.gbFilePathSelect.TabIndex = 0
        Me.gbFilePathSelect.TabStop = False
        Me.gbFilePathSelect.Text = "Select File Locations:"
        '
        'pgBar
        '
        Me.pgBar.Location = New System.Drawing.Point(216, 116)
        Me.pgBar.Name = "pgBar"
        Me.pgBar.Size = New System.Drawing.Size(297, 23)
        Me.pgBar.TabIndex = 13
        Me.pgBar.Visible = False
        '
        'btnCancelDownload
        '
        Me.btnCancelDownload.Enabled = False
        Me.btnCancelDownload.Location = New System.Drawing.Point(155, 116)
        Me.btnCancelDownload.Name = "btnCancelDownload"
        Me.btnCancelDownload.Size = New System.Drawing.Size(55, 23)
        Me.btnCancelDownload.TabIndex = 12
        Me.btnCancelDownload.Text = "Cancel"
        Me.btnCancelDownload.UseVisualStyleBackColor = True
        '
        'btnDownloadSDE
        '
        Me.btnDownloadSDE.Location = New System.Drawing.Point(73, 116)
        Me.btnDownloadSDE.Name = "btnDownloadSDE"
        Me.btnDownloadSDE.Size = New System.Drawing.Size(76, 23)
        Me.btnDownloadSDE.TabIndex = 11
        Me.btnDownloadSDE.Text = "Download"
        Me.btnDownloadSDE.UseVisualStyleBackColor = True
        '
        'lblDownload
        '
        Me.lblDownload.AutoSize = True
        Me.lblDownload.Location = New System.Drawing.Point(9, 80)
        Me.lblDownload.Name = "lblDownload"
        Me.lblDownload.Size = New System.Drawing.Size(112, 13)
        Me.lblDownload.TabIndex = 9
        Me.lblDownload.Text = "SDE Download Folder"
        '
        'lblDownloadFolderPath
        '
        Me.lblDownloadFolderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblDownloadFolderPath.Location = New System.Drawing.Point(12, 93)
        Me.lblDownloadFolderPath.Name = "lblDownloadFolderPath"
        Me.lblDownloadFolderPath.Size = New System.Drawing.Size(501, 20)
        Me.lblDownloadFolderPath.TabIndex = 10
        Me.lblDownloadFolderPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'btnSelectDownloadPath
        '
        Me.btnSelectDownloadPath.Location = New System.Drawing.Point(12, 116)
        Me.btnSelectDownloadPath.Name = "btnSelectDownloadPath"
        Me.btnSelectDownloadPath.Size = New System.Drawing.Size(55, 23)
        Me.btnSelectDownloadPath.TabIndex = 8
        Me.btnSelectDownloadPath.Text = "Select"
        Me.ToolTip1.SetToolTip(Me.btnSelectDownloadPath, "Checks for a new database version, downloads, and unzips the files to the specifi" &
        "ed folder.")
        Me.btnSelectDownloadPath.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(177, 621)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(90, 30)
        Me.btnCancel.TabIndex = 3
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(369, 621)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(90, 30)
        Me.btnClose.TabIndex = 5
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.AboutToolStripMenuItem, Me.DeveloperToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(540, 24)
        Me.MenuStrip1.TabIndex = 24
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SetThreadsUsedToolStripMenuItem, Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'SetThreadsUsedToolStripMenuItem
        '
        Me.SetThreadsUsedToolStripMenuItem.Name = "SetThreadsUsedToolStripMenuItem"
        Me.SetThreadsUsedToolStripMenuItem.Size = New System.Drawing.Size(163, 22)
        Me.SetThreadsUsedToolStripMenuItem.Text = "Set Threads Used"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(163, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CheckForUpdatesToolStripMenuItem, Me.AboutToolStripMenuItem1})
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(52, 20)
        Me.AboutToolStripMenuItem.Text = "About"
        '
        'CheckForUpdatesToolStripMenuItem
        '
        Me.CheckForUpdatesToolStripMenuItem.Name = "CheckForUpdatesToolStripMenuItem"
        Me.CheckForUpdatesToolStripMenuItem.Size = New System.Drawing.Size(243, 22)
        Me.CheckForUpdatesToolStripMenuItem.Text = "Check for Updates"
        '
        'AboutToolStripMenuItem1
        '
        Me.AboutToolStripMenuItem1.Name = "AboutToolStripMenuItem1"
        Me.AboutToolStripMenuItem1.Size = New System.Drawing.Size(243, 22)
        Me.AboutToolStripMenuItem1.Text = "About EVE SDE Database Builder"
        '
        'DeveloperToolStripMenuItem
        '
        Me.DeveloperToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.PrepareFilesForUpdateToolStripMenuItem, Me.BuildBinaryToolStripMenuItem, Me.TestForSDEChangesToolStripMenuItem})
        Me.DeveloperToolStripMenuItem.Name = "DeveloperToolStripMenuItem"
        Me.DeveloperToolStripMenuItem.Size = New System.Drawing.Size(72, 20)
        Me.DeveloperToolStripMenuItem.Text = "Developer"
        '
        'PrepareFilesForUpdateToolStripMenuItem
        '
        Me.PrepareFilesForUpdateToolStripMenuItem.Name = "PrepareFilesForUpdateToolStripMenuItem"
        Me.PrepareFilesForUpdateToolStripMenuItem.Size = New System.Drawing.Size(199, 22)
        Me.PrepareFilesForUpdateToolStripMenuItem.Text = "Prepare Files for Update"
        '
        'BuildBinaryToolStripMenuItem
        '
        Me.BuildBinaryToolStripMenuItem.Name = "BuildBinaryToolStripMenuItem"
        Me.BuildBinaryToolStripMenuItem.Size = New System.Drawing.Size(199, 22)
        Me.BuildBinaryToolStripMenuItem.Text = "Build Binary"
        '
        'TestForSDEChangesToolStripMenuItem
        '
        Me.TestForSDEChangesToolStripMenuItem.CheckOnClick = True
        Me.TestForSDEChangesToolStripMenuItem.Name = "TestForSDEChangesToolStripMenuItem"
        Me.TestForSDEChangesToolStripMenuItem.Size = New System.Drawing.Size(199, 22)
        Me.TestForSDEChangesToolStripMenuItem.Text = "Test for SDE Changes"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(540, 681)
        Me.Controls.Add(Me.gbSelectDBType)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.gbFilePathSelect)
        Me.Controls.Add(Me.btnBuildDatabase)
        Me.Controls.Add(Me.btnSaveSettings)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Controls.Add(Me.dgMain)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.MaximizeBox = False
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "EVE SDE Database Builder"
        Me.pnlMain.ResumeLayout(False)
        Me.pnlMain.PerformLayout()
        CType(Me.dgMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbSelectDBType.ResumeLayout(False)
        Me.gbOptions.ResumeLayout(False)
        Me.gbOptions.PerformLayout()
        Me.gbLanguage.ResumeLayout(False)
        Me.gbLanguage.PerformLayout()
        Me.gbFilePathSelect.ResumeLayout(False)
        Me.gbFilePathSelect.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnBuildDatabase As Button
    Friend WithEvents pnlMain As StatusStrip
    Friend WithEvents lblStatus As ToolStripStatusLabel
    Friend WithEvents dgMain As DataGridView
    Friend WithEvents btnSaveSettings As Button
    Friend WithEvents lblSDEPath As Label
    Friend WithEvents btnSelectSDEPath As Button
    Friend WithEvents lblMediaFire As Label
    Friend WithEvents lblDBName As Label
    Friend WithEvents txtDBName As TextBox
    Friend WithEvents lblFinalDBPath As Label
    Friend WithEvents btnSelectFinalDBPath As Button
    Friend WithEvents lblFinalDBFolder As Label
    Friend WithEvents txtServerName As TextBox
    Friend WithEvents FBDialog As FolderBrowserDialog
    Friend WithEvents FileSelect As DataGridViewCheckBoxColumn
    Friend WithEvents FileName As DataGridViewTextBoxColumn
    Friend WithEvents Progress As DataGridViewTextBoxColumn
    Friend WithEvents btnCheckNoGridItems As Button
    Friend WithEvents btnCheckAllGridItems As Button
    Friend WithEvents gbSelectDBType As GroupBox
    Friend WithEvents rbtnAccess As RadioButton
    Friend WithEvents rbtnSQLServer As RadioButton
    Friend WithEvents rbtnSQLiteDB As RadioButton
    Friend WithEvents rbtnCSV As RadioButton
    Friend WithEvents gbFilePathSelect As GroupBox
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents pgMain As ToolStripProgressBar
    Friend WithEvents chkEUFormat As CheckBox
    Friend WithEvents rbtnPostgreSQL As RadioButton
    Friend WithEvents rbtnMySQL As RadioButton
    Friend WithEvents gbOptions As GroupBox
    Friend WithEvents gbLanguage As GroupBox
    Friend WithEvents rbtnGerman As RadioButton
    Friend WithEvents rbtnEnglish As RadioButton
    Friend WithEvents lblPassword As Label
    Friend WithEvents lblUserName As Label
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents txtUserName As TextBox
    Friend WithEvents rbtnChinese As RadioButton
    Friend WithEvents rbtnRussian As RadioButton
    Friend WithEvents rbtnJapanese As RadioButton
    Friend WithEvents rbtnFrench As RadioButton
    Friend WithEvents txtPort As TextBox
    Friend WithEvents lblPort As Label
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SetThreadsUsedToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DeveloperToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents PrepareFilesForUpdateToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BuildBinaryToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CheckForUpdatesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents lblServerName As Label
    Friend WithEvents btnSelectDownloadPath As Button
    Friend WithEvents rbtnKorean As RadioButton
    Friend WithEvents TestForSDEChangesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents lblDownload As Label
    Friend WithEvents lblDownloadFolderPath As Label
    Friend WithEvents btnDownloadSDE As Button
    Friend WithEvents btnCancelDownload As Button
    Friend WithEvents pgBar As ProgressBar
End Class
