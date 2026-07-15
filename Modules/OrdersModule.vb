Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient

Namespace RestaurantPOS
    Public Class OrdersModule
        Inherits Panel
        Private dgvOrders,dgvItems As DataGridView
        Private dtpFrom,dtpTo As DateTimePicker
        Private cmbStatus,cmbType As ComboBox
        Private txtSearch As TextBox
        Private lblStats As Label

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadOrders()
        End Sub

        Private Sub Build()
            ' 3-row TableLayoutPanel: toolbar | stats | main
            Dim tbl As New TableLayoutPanel()
            tbl.Dock = DockStyle.Fill : tbl.ColumnCount = 1 : tbl.RowCount = 3
            tbl.Padding = New Padding(0) : tbl.Margin = New Padding(0)
            tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent,100))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute,52))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute,38))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent,100))
            Me.Controls.Add(tbl)

            ' TOOLBAR
            Dim pBar As New Panel() : pBar.Dock = DockStyle.Fill : pBar.BackColor = Color.White : pBar.Padding = New Padding(10,10,10,10)
            AddHandler pBar.Paint, Sub(s,e) e.Graphics.DrawLine(New Pen(AppTheme.BorderColor),0,DirectCast(s,Panel).Height-1,DirectCast(s,Panel).Width,DirectCast(s,Panel).Height-1)
            tbl.Controls.Add(pBar,0,0)

            Dim x As Integer = 10
            AddL(pBar,"From:",x,17) : x += 42
            dtpFrom = New DateTimePicker() : dtpFrom.Location = New Point(x,12) : dtpFrom.Size = New Size(118,28) : dtpFrom.Font = AppTheme.F10 : dtpFrom.Format = DateTimePickerFormat.Short : dtpFrom.Value = New DateTime(DateTime.Now.Year,DateTime.Now.Month,1) : pBar.Controls.Add(dtpFrom) : x += 128
            AddL(pBar,"To:",x,17) : x += 28
            dtpTo = New DateTimePicker() : dtpTo.Location = New Point(x,12) : dtpTo.Size = New Size(118,28) : dtpTo.Font = AppTheme.F10 : dtpTo.Format = DateTimePickerFormat.Short : dtpTo.Value = DateTime.Now : pBar.Controls.Add(dtpTo) : x += 128
            AddL(pBar,"Status:",x,17) : x += 52
            cmbStatus = NewCmb(pBar,x,12,108) : cmbStatus.Items.AddRange({"All","Open","Closed","Cancelled"}) : cmbStatus.SelectedIndex = 0 : x += 118
            AddL(pBar,"Type:",x,17) : x += 42
            cmbType = NewCmb(pBar,x,12,118) : cmbType.Items.AddRange({"All Types","Dine-In","Takeaway","Delivery","Counter"}) : cmbType.SelectedIndex = 0 : x += 128
            txtSearch = New TextBox() : txtSearch.Location = New Point(x,12) : txtSearch.Size = New Size(165,28) : txtSearch.Font = AppTheme.F10 : txtSearch.BorderStyle = BorderStyle.FixedSingle : txtSearch.PlaceholderText = "Search customer..." : txtSearch.BackColor = AppTheme.InputBg : pBar.Controls.Add(txtSearch) : x += 175
            Dim btnLoad As New Button() : btnLoad.Text = "Load" : btnLoad.Size = New Size(76,30) : btnLoad.Location = New Point(x,11) : AppTheme.StyleBtn(btnLoad,"primary")
            AddHandler btnLoad.Click, Sub(s,e) LoadOrders()
            pBar.Controls.Add(btnLoad) : x += 84
            Dim btnToday As New Button() : btnToday.Text = "Today" : btnToday.Size = New Size(76,30) : btnToday.Location = New Point(x,11) : AppTheme.StyleBtn(btnToday,"secondary")
            AddHandler btnToday.Click, Sub(s,e)
                dtpFrom.Value = DateTime.Today
                dtpTo.Value = DateTime.Today
                LoadOrders()
            End Sub
            pBar.Controls.Add(btnToday)

            ' STATS BAR
            Dim pStats As New Panel() : pStats.Dock = DockStyle.Fill : pStats.BackColor = Color.FromArgb(240,235,228) : pStats.Padding = New Padding(14,8,14,8)
            tbl.Controls.Add(pStats,0,1)
            lblStats = New Label() : lblStats.Text = "Select filters and click Load" : lblStats.Font = AppTheme.F9B : lblStats.ForeColor = AppTheme.Primary : lblStats.AutoSize = True : lblStats.Location = New Point(14,9) : pStats.Controls.Add(lblStats)

            ' MAIN: orders grid top, items grid bottom
            Dim pMain As New Panel() : pMain.Dock = DockStyle.Fill : pMain.BackColor = Color.Transparent : pMain.Padding = New Padding(0)
            tbl.Controls.Add(pMain,0,2)

            Dim mainTbl As New TableLayoutPanel()
            mainTbl.Dock = DockStyle.Fill : mainTbl.ColumnCount = 1 : mainTbl.RowCount = 3
            mainTbl.Padding = New Padding(0) : mainTbl.Margin = New Padding(0)
            mainTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            mainTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent,100))
            mainTbl.RowStyles.Add(New RowStyle(SizeType.Percent,58))
            mainTbl.RowStyles.Add(New RowStyle(SizeType.Absolute,32))
            mainTbl.RowStyles.Add(New RowStyle(SizeType.Percent,42))
            pMain.Controls.Add(mainTbl)

            dgvOrders = New DataGridView() : dgvOrders.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvOrders)
            AddHandler dgvOrders.SelectionChanged, Sub(s,e)
                If dgvOrders.CurrentRow IsNot Nothing AndAlso dgvOrders.CurrentRow.Index >= 0 Then
                    Try
                        Dim oid As Integer = CInt(dgvOrders.CurrentRow.Cells("OrderID").Value)
                        dgvItems.DataSource = DatabaseManager.GetDataTable("SELECT ItemName AS [Item Name],Quantity AS Qty,UnitPrice AS [Unit Price],TaxPercent AS [Tax%],TotalPrice AS Total FROM OrderItems WHERE OrderID=@id",New SqlParameter("@id",oid))
                    Catch
                    End Try
                End If
            End Sub
            Dim ctxM As New ContextMenuStrip()
            Dim mClose As New ToolStripMenuItem("Mark as Closed") : Dim mCancel As New ToolStripMenuItem("Cancel Order")
            AddHandler mClose.Click, Sub(s,e)
                If dgvOrders.CurrentRow Is Nothing Then Return
                Try
                    Dim oid As Integer = CInt(dgvOrders.CurrentRow.Cells("OrderID").Value)
                    DatabaseManager.ExecuteNonQuery("UPDATE Orders SET Status='Closed',ClosedDate=GETDATE() WHERE OrderID=@id",New SqlParameter("@id",oid))
                    MessageBox.Show("Order closed.","Done",MessageBoxButtons.OK,MessageBoxIcon.Information) : LoadOrders()
                Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
            End Sub
            AddHandler mCancel.Click, Sub(s,e)
                If dgvOrders.CurrentRow Is Nothing Then Return
                If MessageBox.Show("Cancel?","Confirm",MessageBoxButtons.YesNo,MessageBoxIcon.Warning) = DialogResult.No Then Return
                Try
                    Dim oid As Integer = CInt(dgvOrders.CurrentRow.Cells("OrderID").Value)
                    DatabaseManager.ExecuteNonQuery("UPDATE Orders SET Status='Cancelled' WHERE OrderID=@id",New SqlParameter("@id",oid))
                    LoadOrders()
                Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
            End Sub
            ctxM.Items.Add(mClose) : ctxM.Items.Add(mCancel) : dgvOrders.ContextMenuStrip = ctxM
            mainTbl.Controls.Add(dgvOrders,0,0)

            Dim lIH As New Label() : lIH.Text = "  Order Items  (click an order above to see details)"
            lIH.Font = AppTheme.F9B : lIH.ForeColor = AppTheme.TextDark : lIH.Dock = DockStyle.Fill : lIH.BackColor = Color.FromArgb(248,244,240) : lIH.TextAlign = ContentAlignment.MiddleLeft
            mainTbl.Controls.Add(lIH,0,1)

            dgvItems = New DataGridView() : dgvItems.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvItems)
            mainTbl.Controls.Add(dgvItems,0,2)
        End Sub

        Private Sub AddL(p As Control, t As String, x As Integer, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.AutoSize = True : l.Location = New Point(x,y) : p.Controls.Add(l)
        End Sub
        Private Function NewCmb(p As Control, x As Integer, y As Integer, w As Integer) As ComboBox
            Dim cb As New ComboBox() : cb.Location = New Point(x,y) : cb.Size = New Size(w,28) : cb.Font = AppTheme.F10 : cb.DropDownStyle = ComboBoxStyle.DropDownList : p.Controls.Add(cb) : Return cb
        End Function

        Private Sub LoadOrders()
            Try
                Dim sql As String = "SELECT OrderID,CustomerName,OrderType,Status,SubTotal,TaxAmount,DiscountAmount,TotalAmount,PaymentMode,CONVERT(VARCHAR,OrderDate,120) AS [OrderDate] FROM Orders WHERE OrderDate BETWEEN @from AND @to"
                Dim prms As New List(Of SqlParameter)()
                prms.Add(New SqlParameter("@from",dtpFrom.Value.Date))
                prms.Add(New SqlParameter("@to",dtpTo.Value.Date.AddDays(1).AddSeconds(-1)))
                If cmbStatus.SelectedIndex > 0 Then sql &= " AND Status=@st" : prms.Add(New SqlParameter("@st",cmbStatus.Text))
                If cmbType.SelectedIndex > 0 Then sql &= " AND OrderType=@ot" : prms.Add(New SqlParameter("@ot",cmbType.Text))
                If Not String.IsNullOrEmpty(txtSearch.Text) Then sql &= " AND CustomerName LIKE @s" : prms.Add(New SqlParameter("@s","%" & txtSearch.Text & "%"))
                sql &= " ORDER BY OrderID DESC"
                Dim dt As DataTable = DatabaseManager.GetDataTable(sql,prms.ToArray())
                dgvOrders.DataSource = dt
                If dgvOrders.Columns.Contains("OrderID") Then dgvOrders.Columns("OrderID").Visible = False
                For Each row As DataGridViewRow In dgvOrders.Rows
                    Dim st As String = If(row.Cells("Status").Value IsNot Nothing,row.Cells("Status").Value.ToString(),"")
                    Select Case st
                        Case "Open"      : row.DefaultCellStyle.BackColor = Color.FromArgb(255,252,230)
                        Case "Closed"    : row.DefaultCellStyle.BackColor = Color.FromArgb(235,255,240)
                        Case "Cancelled" : row.DefaultCellStyle.BackColor = Color.FromArgb(250,240,240)
                    End Select
                Next
                Dim total As Decimal = 0
                For Each row As DataRow In dt.Rows : total += CDec(row("TotalAmount")) : Next
                lblStats.Text = $"Orders: {dt.Rows.Count}   |   Total Revenue: Rs. {total:N2}   |   Period: {dtpFrom.Value:dd/MM/yyyy} to {dtpTo.Value:dd/MM/yyyy}"
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class
End Namespace
