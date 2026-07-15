Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Namespace RestaurantPOS

    Public Class SessionManager
        Public Shared Property UserID As Integer = 0
        Public Shared Property Username As String = ""
        Public Shared Property FullName As String = ""
        Public Shared Property Role As String = ""
        Public Shared Property LoginTime As DateTime = DateTime.Now
        Public Shared Function IsAdmin() As Boolean
            Return Role = "Admin"
        End Function
        Public Shared Function IsManagerOrAdmin() As Boolean
            Return Role = "Admin" OrElse Role = "Manager"
        End Function
        Public Shared Sub ClearSession()
            UserID = 0 : Username = "" : FullName = "" : Role = ""
        End Sub
    End Class

    Public Class AppTheme
        Public Shared ReadOnly Primary      As Color = Color.FromArgb(183, 55, 35)
        Public Shared ReadOnly PrimaryDark  As Color = Color.FromArgb(110, 22, 8)
        Public Shared ReadOnly Green        As Color = Color.FromArgb(39, 174, 96)
        Public Shared ReadOnly Orange       As Color = Color.FromArgb(230, 126, 34)
        Public Shared ReadOnly Red          As Color = Color.FromArgb(192, 57, 43)
        Public Shared ReadOnly Blue         As Color = Color.FromArgb(41, 128, 185)
        Public Shared ReadOnly Purple       As Color = Color.FromArgb(142, 68, 173)
        Public Shared ReadOnly SidebarBg    As Color = Color.FromArgb(22, 12, 6)
        Public Shared ReadOnly ContentBg    As Color = Color.FromArgb(245, 241, 237)
        Public Shared ReadOnly White        As Color = Color.White
        Public Shared ReadOnly TextDark     As Color = Color.FromArgb(28, 18, 8)
        Public Shared ReadOnly TextMed      As Color = Color.FromArgb(110, 88, 68)
        Public Shared ReadOnly TextLight    As Color = Color.FromArgb(160, 140, 120)
        Public Shared ReadOnly BorderColor  As Color = Color.FromArgb(220, 210, 200)
        Public Shared ReadOnly SidebarText  As Color = Color.FromArgb(185, 165, 148)
        Public Shared ReadOnly InputBg      As Color = Color.FromArgb(252, 250, 248)

        Public Shared ReadOnly F8B  As New System.Drawing.Font("Segoe UI", 8, FontStyle.Bold)
        Public Shared ReadOnly F9   As New System.Drawing.Font("Segoe UI", 9)
        Public Shared ReadOnly F9B  As New System.Drawing.Font("Segoe UI", 9, FontStyle.Bold)
        Public Shared ReadOnly F10  As New System.Drawing.Font("Segoe UI", 10)
        Public Shared ReadOnly F10B As New System.Drawing.Font("Segoe UI", 10, FontStyle.Bold)
        Public Shared ReadOnly F11B As New System.Drawing.Font("Segoe UI", 11, FontStyle.Bold)
        Public Shared ReadOnly F12B As New System.Drawing.Font("Segoe UI", 12, FontStyle.Bold)

        Public Shared Function StyleBtn(btn As Button, style As String) As Button
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 0
            btn.Cursor = Cursors.Hand
            btn.Font = F10B
            btn.TextAlign = ContentAlignment.MiddleCenter
            Select Case style
                Case "primary"   : btn.BackColor = Primary   : btn.ForeColor = White : btn.FlatAppearance.MouseOverBackColor = PrimaryDark
                Case "success"   : btn.BackColor = Green     : btn.ForeColor = White : btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30,150,80)
                Case "danger"    : btn.BackColor = Red       : btn.ForeColor = White : btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(155,40,25)
                Case "warning"   : btn.BackColor = Orange    : btn.ForeColor = White
                Case "info"      : btn.BackColor = Blue      : btn.ForeColor = White
                Case "secondary" : btn.BackColor = Color.FromArgb(108,117,125) : btn.ForeColor = White
                Case "purple"    : btn.BackColor = Purple    : btn.ForeColor = White
            End Select
            Return btn
        End Function

        Public Shared Function MakeGrid(dgv As DataGridView) As DataGridView
            dgv.BorderStyle = BorderStyle.None
            dgv.BackgroundColor = White
            dgv.GridColor = BorderColor
            dgv.DefaultCellStyle.Font = F10
            dgv.DefaultCellStyle.ForeColor = TextDark
            dgv.DefaultCellStyle.BackColor = White
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 215, 195)
            dgv.DefaultCellStyle.SelectionForeColor = TextDark
            dgv.DefaultCellStyle.Padding = New Padding(6, 3, 6, 3)
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 249, 246)
            dgv.ColumnHeadersDefaultCellStyle.Font = F9B
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = White
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Primary
            dgv.ColumnHeadersDefaultCellStyle.Padding = New Padding(8, 5, 8, 5)
            dgv.ColumnHeadersHeight = 36
            dgv.RowTemplate.Height = 30
            dgv.EnableHeadersVisualStyles = False
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            dgv.RowHeadersVisible = False
            dgv.AllowUserToAddRows = False
            dgv.AllowUserToResizeRows = False
            Return dgv
        End Function

        Public Shared Function MakeToolbar() As Panel
            Dim p As New Panel()
            p.Dock = DockStyle.Top : p.Height = 52 : p.BackColor = White : p.Padding = New Padding(10, 10, 10, 10)
            AddHandler p.Paint, Sub(s, e)
                Dim pnl As Panel = DirectCast(s, Panel)
                e.Graphics.DrawLine(New Pen(BorderColor), 0, pnl.Height - 1, pnl.Width, pnl.Height - 1)
            End Sub
            Return p
        End Function

        Public Shared Function MakeSplit(leftPct As Single, rightPx As Integer) As TableLayoutPanel
            Dim tbl As New TableLayoutPanel()
            tbl.Dock = DockStyle.Fill : tbl.ColumnCount = 2 : tbl.RowCount = 1
            tbl.Padding = New Padding(0) : tbl.Margin = New Padding(0)
            tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, leftPct))
            tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, rightPx))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Return tbl
        End Function

        Public Shared Function AccentBar() As Panel
            Dim p As New Panel() : p.Dock = DockStyle.Top : p.Height = 4 : p.BackColor = Primary : Return p
        End Function

        Public Shared Function FormTitle(text As String) As Label
            Dim l As New Label() : l.Text = text : l.Font = F11B : l.ForeColor = Primary
            l.Dock = DockStyle.Top : l.Height = 40 : l.Padding = New Padding(14, 10, 0, 0)
            Return l
        End Function

    End Class
End Namespace
