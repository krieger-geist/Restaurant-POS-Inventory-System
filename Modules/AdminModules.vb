Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient
Imports System.IO
Imports System.Linq
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports NPOI.SS.UserModel
Imports NPOI.XSSF.UserModel
Imports BCrypt.Net

Namespace RestaurantPOS

    '═══════════════════════════════════════════════
    ' INVENTORY MODULE
    '═══════════════════════════════════════════════
    Public Class InventoryModule
        Inherits Panel
        Private dgvInv As DataGridView
        Private lblFT As Label
        Private txtName, txtCat, txtUnit, txtStock, txtMin, txtCost, txtSupplier As TextBox
        Private currentID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadInv()
        End Sub

        Private Sub Build()
            Dim pBar As Panel = AppTheme.MakeToolbar()
            Me.Controls.Add(pBar)
            Dim btnAdd As New Button() : btnAdd.Text = "Add Item" : btnAdd.Size = New Size(110, 32) : btnAdd.Location = New Point(10, 10)
            AppTheme.StyleBtn(btnAdd, "success") : AddHandler btnAdd.Click, AddressOf NewItem : pBar.Controls.Add(btnAdd)
            Dim btnLow As New Button() : btnLow.Text = "Low Stock" : btnLow.Size = New Size(110, 32) : btnLow.Location = New Point(128, 10)
            AppTheme.StyleBtn(btnLow, "warning")
            AddHandler btnLow.Click, Sub(s, e) LoadLow()
            pBar.Controls.Add(btnLow)
            Dim btnAll As New Button() : btnAll.Text = "Show All" : btnAll.Size = New Size(100, 32) : btnAll.Location = New Point(246, 10)
            AppTheme.StyleBtn(btnAll, "secondary")
            AddHandler btnAll.Click, Sub(s, e) LoadInv()
            pBar.Controls.Add(btnAll)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 350)
            Me.Controls.Add(split)

            dgvInv = New DataGridView() : dgvInv.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvInv) : dgvInv.Margin = New Padding(0, 0, 4, 0)
            AddHandler dgvInv.SelectionChanged, Sub(s, e)
                                                    If dgvInv.CurrentRow IsNot Nothing AndAlso dgvInv.CurrentRow.Index >= 0 Then
                                                        Try : LoadForm(CInt(dgvInv.CurrentRow.Cells("InventoryID").Value)) : Catch : End Try
                                                    End If
                                                End Sub
            split.Controls.Add(dgvInv, 0, 0)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(4, 0, 0, 0)
            split.Controls.Add(pForm, 1, 0)
            pForm.Controls.Add(AppTheme.AccentBar())
            lblFT = AppTheme.FormTitle("Inventory Item") : pForm.Controls.Add(lblFT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 6, 14, 10) : pF.BackColor = Color.White : pForm.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 48
            AddL(pF, "Item Name *", y) : txtName = NTB(pF, y + 18) : y += sp
            AddL(pF, "Category", y) : txtCat = NTB(pF, y + 18) : y += sp
            AddL(pF, "Unit (Kg/Liter/Piece)", y) : txtUnit = NTB(pF, y + 18) : y += sp
            AddL(pF, "Current Stock", y) : txtStock = NTB(pF, y + 18) : txtStock.Text = "0" : y += sp
            AddL(pF, "Minimum Stock Level", y) : txtMin = NTB(pF, y + 18) : txtMin.Text = "5" : y += sp
            AddL(pF, "Cost Per Unit (Rs.)", y) : txtCost = NTB(pF, y + 18) : txtCost.Text = "0" : y += sp
            AddL(pF, "Supplier Name", y) : txtSupplier = NTB(pF, y + 18) : y += sp

            Dim btnS As New Button() : btnS.Text = "Save" : btnS.Size = New Size(94, 34) : btnS.Location = New Point(0, y) : AppTheme.StyleBtn(btnS, "success") : AddHandler btnS.Click, AddressOf BtnSave : pF.Controls.Add(btnS)
            Dim btnN As New Button() : btnN.Text = "New" : btnN.Size = New Size(94, 34) : btnN.Location = New Point(102, y) : AppTheme.StyleBtn(btnN, "secondary") : AddHandler btnN.Click, AddressOf NewItem : pF.Controls.Add(btnN)
            Dim btnD As New Button() : btnD.Text = "Delete" : btnD.Size = New Size(94, 34) : btnD.Location = New Point(204, y) : AppTheme.StyleBtn(btnD, "danger") : AddHandler btnD.Click, AddressOf BtnDelete : pF.Controls.Add(btnD)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(318, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function

        Private Sub LoadInv()
            Try
                dgvInv.DataSource = DatabaseManager.GetDataTable("SELECT InventoryID,Name,Category,Unit,CurrentStock,MinStock,CostPerUnit,Supplier FROM Inventory ORDER BY Name")
                If dgvInv.Columns.Contains("InventoryID") Then dgvInv.Columns("InventoryID").Visible = False
                HighlightLow()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadLow()
            Try
                dgvInv.DataSource = DatabaseManager.GetDataTable("SELECT InventoryID,Name,Category,Unit,CurrentStock,MinStock,CostPerUnit,Supplier FROM Inventory WHERE CurrentStock<=MinStock ORDER BY CurrentStock ASC")
                If dgvInv.Columns.Contains("InventoryID") Then dgvInv.Columns("InventoryID").Visible = False
                HighlightLow()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub HighlightLow()
            For Each row As DataGridViewRow In dgvInv.Rows
                If row.Cells("CurrentStock").Value IsNot Nothing AndAlso row.Cells("MinStock").Value IsNot Nothing Then
                    If CDec(row.Cells("CurrentStock").Value) <= CDec(row.Cells("MinStock").Value) Then
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 232, 228) : row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 35, 15)
                    End If
                End If
            Next
        End Sub

        Private Sub LoadForm(id As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT * FROM Inventory WHERE InventoryID=@id", New SqlParameter("@id", id))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentID = id : lblFT.Text = "Edit: " & row("Name").ToString()
                txtName.Text = row("Name").ToString() : txtCat.Text = row("Category").ToString() : txtUnit.Text = row("Unit").ToString()
                txtStock.Text = row("CurrentStock").ToString() : txtMin.Text = row("MinStock").ToString() : txtCost.Text = row("CostPerUnit").ToString() : txtSupplier.Text = row("Supplier").ToString()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub NewItem(sender As Object, e As EventArgs)
            currentID = 0 : lblFT.Text = "New Item" : txtName.Text = "" : txtCat.Text = "" : txtUnit.Text = "" : txtStock.Text = "0" : txtMin.Text = "5" : txtCost.Text = "0" : txtSupplier.Text = "" : txtName.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtName.Text.Trim()) Then
                MessageBox.Show("Name required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim stock As Decimal = 0 : Decimal.TryParse(txtStock.Text, stock) : Dim minS As Decimal = 5 : Decimal.TryParse(txtMin.Text, minS) : Dim cost As Decimal = 0 : Decimal.TryParse(txtCost.Text, cost)
            Try
                If currentID = 0 Then
                    DatabaseManager.ExecuteNonQuery("INSERT INTO Inventory(Name,Category,Unit,CurrentStock,MinStock,CostPerUnit,Supplier,LastUpdated) VALUES(@n,@c,@u,@s,@ms,@cp,@sup,GETDATE())", New SqlParameter("@n", txtName.Text.Trim()), New SqlParameter("@c", txtCat.Text.Trim()), New SqlParameter("@u", txtUnit.Text.Trim()), New SqlParameter("@s", stock), New SqlParameter("@ms", minS), New SqlParameter("@cp", cost), New SqlParameter("@sup", txtSupplier.Text.Trim()))
                    MessageBox.Show("Item added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE Inventory SET Name=@n,Category=@c,Unit=@u,CurrentStock=@s,MinStock=@ms,CostPerUnit=@cp,Supplier=@sup,LastUpdated=GETDATE() WHERE InventoryID=@id", New SqlParameter("@n", txtName.Text.Trim()), New SqlParameter("@c", txtCat.Text.Trim()), New SqlParameter("@u", txtUnit.Text.Trim()), New SqlParameter("@s", stock), New SqlParameter("@ms", minS), New SqlParameter("@cp", cost), New SqlParameter("@sup", txtSupplier.Text.Trim()), New SqlParameter("@id", currentID))
                    MessageBox.Show("Item updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                LoadInv()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnDelete(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            If MessageBox.Show("Delete '" & txtName.Text & "'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("DELETE FROM Inventory WHERE InventoryID=@id", New SqlParameter("@id", currentID))
                LoadInv() : NewItem(Nothing, Nothing)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class

    '═══════════════════════════════════════════════
    ' EXPENSES MODULE
    '═══════════════════════════════════════════════
    Public Class ExpensesModule
        Inherits Panel
        Private dgvExp As DataGridView
        Private txtCat, txtDesc, txtAmount, txtPaidTo As TextBox
        Private dtpExp As DateTimePicker
        Private lblMonth As Label

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadExp()
        End Sub

        Private Sub Build()
            ' 3 rows: stats | main content
            Dim tbl As New TableLayoutPanel()
            tbl.Dock = DockStyle.Fill : tbl.ColumnCount = 1 : tbl.RowCount = 2
            tbl.Padding = New Padding(0) : tbl.Margin = New Padding(0)
            tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Me.Controls.Add(tbl)

            Dim pHdr As New Panel() : pHdr.Dock = DockStyle.Fill : pHdr.BackColor = Color.White : pHdr.Padding = New Padding(14, 8, 14, 8)
            AddHandler pHdr.Paint, Sub(s, e) e.Graphics.DrawLine(New Pen(AppTheme.BorderColor), 0, DirectCast(s, Panel).Height - 1, DirectCast(s, Panel).Width, DirectCast(s, Panel).Height - 1)
            tbl.Controls.Add(pHdr, 0, 0)
            lblMonth = New Label() : lblMonth.Text = "This Month: Rs. 0" : lblMonth.Font = New System.Drawing.Font("Segoe UI", 13, FontStyle.Bold) : lblMonth.ForeColor = AppTheme.Primary : lblMonth.AutoSize = True : lblMonth.Location = New Point(14, 10) : pHdr.Controls.Add(lblMonth)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 340)
            tbl.Controls.Add(split, 0, 1)

            dgvExp = New DataGridView() : dgvExp.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvExp) : dgvExp.Margin = New Padding(0, 0, 4, 0)
            split.Controls.Add(dgvExp, 0, 0)

            Dim pAdd As New Panel() : pAdd.Dock = DockStyle.Fill : pAdd.BackColor = Color.White : pAdd.Margin = New Padding(4, 0, 0, 0)
            split.Controls.Add(pAdd, 1, 0)
            pAdd.Controls.Add(AppTheme.AccentBar())
            pAdd.Controls.Add(AppTheme.FormTitle("Add New Expense"))

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 6, 14, 10) : pF.BackColor = Color.White : pAdd.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 50
            AddL(pF, "Category", y) : txtCat = NTB(pF, y + 18) : y += sp
            AddL(pF, "Description", y) : txtDesc = NTB(pF, y + 18) : y += sp
            AddL(pF, "Amount (Rs.) *", y) : txtAmount = NTB(pF, y + 18) : y += sp
            AddL(pF, "Paid To", y) : txtPaidTo = NTB(pF, y + 18) : y += sp
            AddL(pF, "Expense Date", y) : dtpExp = New DateTimePicker() : dtpExp.Location = New Point(0, y + 18) : dtpExp.Size = New Size(308, 26) : dtpExp.Font = AppTheme.F10 : dtpExp.Format = DateTimePickerFormat.Short : dtpExp.Value = DateTime.Today : pF.Controls.Add(dtpExp) : y += sp

            Dim btnAdd As New Button() : btnAdd.Text = "Add Expense" : btnAdd.Size = New Size(308, 40) : btnAdd.Location = New Point(0, y)
            AppTheme.StyleBtn(btnAdd, "primary") : btnAdd.Font = AppTheme.F11B
            AddHandler btnAdd.Click, Sub(s, e)
                                         If String.IsNullOrEmpty(txtAmount.Text) Then
                                             MessageBox.Show("Amount required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                             Return
                                         End If
                                         Dim amt As Decimal = 0 : Decimal.TryParse(txtAmount.Text, amt)
                                         Try
                                             DatabaseManager.ExecuteNonQuery("INSERT INTO Expenses(Category,Description,Amount,ExpenseDate,PaidTo,CreatedBy) VALUES(@c,@d,@a,@dt,@p,@cb)", New SqlParameter("@c", txtCat.Text.Trim()), New SqlParameter("@d", txtDesc.Text.Trim()), New SqlParameter("@a", amt), New SqlParameter("@dt", dtpExp.Value.Date), New SqlParameter("@p", txtPaidTo.Text.Trim()), New SqlParameter("@cb", SessionManager.UserID))
                                             MessageBox.Show("Expense recorded!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                             txtCat.Text = "" : txtDesc.Text = "" : txtAmount.Text = "" : txtPaidTo.Text = "" : LoadExp()
                                         Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
                                     End Sub
            pF.Controls.Add(btnAdd)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(308, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function

        Private Sub LoadExp()
            Try
                dgvExp.DataSource = DatabaseManager.GetDataTable("SELECT ExpenseID,Category,Description,Amount,ExpenseDate,PaidTo FROM Expenses ORDER BY ExpenseDate DESC")
                If dgvExp.Columns.Contains("ExpenseID") Then dgvExp.Columns("ExpenseID").Visible = False
                Dim total As Object = DatabaseManager.ExecuteScalar("SELECT ISNULL(SUM(Amount),0) FROM Expenses WHERE MONTH(ExpenseDate)=MONTH(GETDATE()) AND YEAR(ExpenseDate)=YEAR(GETDATE())")
                lblMonth.Text = "This Month: Rs. " & CDec(total).ToString("N2")
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class

    '═══════════════════════════════════════════════
    ' REPORTS MODULE
    '═══════════════════════════════════════════════
    Public Class ReportsModule
        Inherits Panel
        Private tabCtrl As TabControl
        Private dgvSales, dgvPerf As DataGridView
        Private pnlChart As Panel
        Private dtpFrom, dtpTo As DateTimePicker
        Private lblRev, lblOrders, lblAvg As Label
        Private chartData As New Dictionary(Of String, Decimal)()

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadThisMonth()
        End Sub

        Private Sub Build()
            Dim root As New TableLayoutPanel()
            root.Dock = DockStyle.Fill : root.ColumnCount = 1 : root.RowCount = 3
            root.Padding = New Padding(0) : root.Margin = New Padding(0)
            root.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 52))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 88))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Me.Controls.Add(root)

            ' Toolbar
            Dim pBar As New Panel() : pBar.Dock = DockStyle.Fill : pBar.BackColor = Color.White : pBar.Padding = New Padding(10, 10, 10, 10)
            AddHandler pBar.Paint, Sub(s, e) e.Graphics.DrawLine(New Pen(AppTheme.BorderColor), 0, DirectCast(s, Panel).Height - 1, DirectCast(s, Panel).Width, DirectCast(s, Panel).Height - 1)
            root.Controls.Add(pBar, 0, 0)

            Dim x As Integer = 10
            AddL(pBar, "From:", x, 17) : x += 42
            dtpFrom = New DateTimePicker() : dtpFrom.Location = New Point(x, 11) : dtpFrom.Size = New Size(118, 28) : dtpFrom.Font = AppTheme.F10 : dtpFrom.Format = DateTimePickerFormat.Short : dtpFrom.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : pBar.Controls.Add(dtpFrom) : x += 128
            AddL(pBar, "To:", x, 17) : x += 28
            dtpTo = New DateTimePicker() : dtpTo.Location = New Point(x, 11) : dtpTo.Size = New Size(118, 28) : dtpTo.Font = AppTheme.F10 : dtpTo.Format = DateTimePickerFormat.Short : dtpTo.Value = DateTime.Now : pBar.Controls.Add(dtpTo) : x += 128
            Dim btnLoad As New Button() : btnLoad.Text = "Load" : btnLoad.Size = New Size(80, 30) : btnLoad.Location = New Point(x, 11) : AppTheme.StyleBtn(btnLoad, "primary")
            AddHandler btnLoad.Click, Sub(s, e) LoadReport()
            pBar.Controls.Add(btnLoad) : x += 88
            Dim btnToday As New Button() : btnToday.Text = "Today" : btnToday.Size = New Size(80, 30) : btnToday.Location = New Point(x, 11) : AppTheme.StyleBtn(btnToday, "secondary")
            AddHandler btnToday.Click, Sub(s, e)
                                           dtpFrom.Value = DateTime.Today
                                           dtpTo.Value = DateTime.Today
                                           LoadReport()
                                       End Sub
            pBar.Controls.Add(btnToday) : x += 88
            Dim btnMonth As New Button() : btnMonth.Text = "This Month" : btnMonth.Size = New Size(108, 30) : btnMonth.Location = New Point(x, 11) : AppTheme.StyleBtn(btnMonth, "secondary")
            AddHandler btnMonth.Click, Sub(s, e) LoadThisMonth()
            pBar.Controls.Add(btnMonth) : x += 116
            Dim btnXLS As New Button() : btnXLS.Text = "Export Excel" : btnXLS.Size = New Size(118, 30) : btnXLS.Location = New Point(x, 11) : AppTheme.StyleBtn(btnXLS, "success")
            AddHandler btnXLS.Click, Sub(s, e) ExportExcel()
            pBar.Controls.Add(btnXLS) : x += 126
            Dim btnPDF As New Button() : btnPDF.Text = "Export PDF" : btnPDF.Size = New Size(108, 30) : btnPDF.Location = New Point(x, 11) : AppTheme.StyleBtn(btnPDF, "info")
            AddHandler btnPDF.Click, Sub(s, e) ExportPDF()
            pBar.Controls.Add(btnPDF)

            ' Stats cards
            Dim pStats As New Panel() : pStats.Dock = DockStyle.Fill : pStats.BackColor = Color.FromArgb(248, 244, 240) : pStats.Padding = New Padding(14, 12, 14, 12)
            root.Controls.Add(pStats, 0, 1)
            Dim flow As New FlowLayoutPanel() : flow.Dock = DockStyle.Fill : flow.FlowDirection = FlowDirection.LeftToRight : flow.WrapContents = False : flow.BackColor = Color.Transparent : pStats.Controls.Add(flow)
            lblRev = New Label() : lblOrders = New Label() : lblAvg = New Label()
            Dim c1 As Panel = MkCard("Revenue", "Rs. 0", AppTheme.Green, lblRev) : Dim c2 As Panel = MkCard("Orders", "0", AppTheme.Primary, lblOrders) : Dim c3 As Panel = MkCard("Avg Order", "Rs. 0", AppTheme.Blue, lblAvg)
            For Each cp As Panel In {c1, c2, c3} : cp.Margin = New Padding(0, 0, 14, 0) : flow.Controls.Add(cp) : Next

            ' Tabs
            tabCtrl = New TabControl() : tabCtrl.Dock = DockStyle.Fill : tabCtrl.Font = AppTheme.F10
            root.Controls.Add(tabCtrl, 0, 2)
            Dim t1 As New TabPage("  Order Summary  ") : t1.BackColor = Color.White : tabCtrl.TabPages.Add(t1)
            dgvSales = New DataGridView() : dgvSales.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvSales) : t1.Controls.Add(dgvSales)
            Dim t2 As New TabPage("  Menu Performance  ") : t2.BackColor = Color.White : tabCtrl.TabPages.Add(t2)
            dgvPerf = New DataGridView() : dgvPerf.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvPerf) : t2.Controls.Add(dgvPerf)
            Dim t3 As New TabPage("  Revenue Chart  ") : t3.BackColor = Color.White : tabCtrl.TabPages.Add(t3)
            pnlChart = New Panel() : pnlChart.Dock = DockStyle.Fill : pnlChart.BackColor = Color.White : AddHandler pnlChart.Paint, AddressOf DrawChart : t3.Controls.Add(pnlChart)
        End Sub

        Private Sub AddL(p As Control, t As String, x As Integer, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.AutoSize = True : l.Location = New Point(x, y) : p.Controls.Add(l)
        End Sub

        Private Function MkCard(title As String, value As String, color As Color, ByRef vl As Label) As Panel
            Dim card As New Panel() : card.Size = New Size(200, 62) : card.BackColor = color.White
            Dim bar As New Panel() : bar.Height = 4 : bar.Dock = DockStyle.Top : bar.BackColor = color : card.Controls.Add(bar)
            vl = New Label() : vl.Text = value : vl.Font = New System.Drawing.Font("Segoe UI", 13, FontStyle.Bold) : vl.ForeColor = AppTheme.TextDark : vl.Location = New Point(10, 10) : vl.AutoSize = True : card.Controls.Add(vl)
            Dim lt As New Label() : lt.Text = title : lt.Font = AppTheme.F8B : lt.ForeColor = AppTheme.TextLight : lt.Location = New Point(10, 40) : lt.AutoSize = True : card.Controls.Add(lt) : Return card
        End Function

        Private Sub LoadThisMonth()
            dtpFrom.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : dtpTo.Value = DateTime.Now : LoadReport()
        End Sub

        Private Sub LoadReport()
            Dim f As DateTime = dtpFrom.Value.Date : Dim t As DateTime = dtpTo.Value.Date.AddDays(1).AddSeconds(-1)
            Try
                dgvSales.DataSource = DatabaseManager.GetDataTable("SELECT OrderID,CustomerName,OrderType,TotalAmount,PaymentMode,CONVERT(VARCHAR,OrderDate,120) AS [Date] FROM Orders WHERE Status='Closed' AND ClosedDate BETWEEN @f AND @t ORDER BY ClosedDate DESC", New SqlParameter("@f", f), New SqlParameter("@t", t))
                If dgvSales.Columns.Contains("OrderID") Then dgvSales.Columns("OrderID").Visible = False
                Dim st As DataTable = DatabaseManager.GetDataTable("SELECT ISNULL(SUM(TotalAmount),0) AS Rev,COUNT(*) AS Cnt,ISNULL(AVG(TotalAmount),0) AS Avg FROM Orders WHERE Status='Closed' AND ClosedDate BETWEEN @f AND @t", New SqlParameter("@f", f), New SqlParameter("@t", t))
                If st.Rows.Count > 0 Then
                    lblRev.Text = "Rs. " & CDec(st.Rows(0)("Rev")).ToString("N0")
                    lblOrders.Text = st.Rows(0)("Cnt").ToString()
                    lblAvg.Text = "Rs. " & CDec(st.Rows(0)("Avg")).ToString("N0")
                End If
                dgvPerf.DataSource = DatabaseManager.GetDataTable("SELECT oi.ItemName AS Item,SUM(oi.Quantity) AS [Total Sold],SUM(oi.TotalPrice) AS Revenue FROM OrderItems oi INNER JOIN Orders o ON oi.OrderID=o.OrderID WHERE o.Status='Closed' AND o.ClosedDate BETWEEN @f AND @t GROUP BY oi.ItemName ORDER BY Revenue DESC", New SqlParameter("@f", f), New SqlParameter("@t", t))
                Dim daily As DataTable = DatabaseManager.GetDataTable("SELECT CONVERT(VARCHAR,CAST(ClosedDate AS DATE),105) AS Day,ISNULL(SUM(TotalAmount),0) AS Rev FROM Orders WHERE Status='Closed' AND ClosedDate BETWEEN @f AND @t GROUP BY CAST(ClosedDate AS DATE) ORDER BY CAST(ClosedDate AS DATE)", New SqlParameter("@f", f), New SqlParameter("@t", t))
                chartData.Clear() : For Each row As DataRow In daily.Rows : chartData(row("Day").ToString()) = CDec(row("Rev")) : Next
                pnlChart.Invalidate()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub DrawChart(sender As Object, e As PaintEventArgs)
            Dim g As Graphics = e.Graphics : g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            Dim w As Integer = pnlChart.Width : Dim h As Integer = pnlChart.Height : g.Clear(Color.White)
            If chartData.Count = 0 Then
                g.DrawString("No data — click Load Report", New System.Drawing.Font("Segoe UI", 12), New SolidBrush(Color.FromArgb(160, 140, 120)), CSng(w / 2 - 160), CSng(h / 2 - 10)) : Return
            End If
            Dim pad As Integer = 70 : Dim cw As Integer = w - pad * 2 : Dim ch As Integer = h - pad * 2 - 30
            Dim maxV As Decimal = chartData.Values.Max() : If maxV = 0 Then maxV = 1
            g.DrawString("Daily Revenue Chart", New System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), New SolidBrush(AppTheme.Primary), pad, 14)
            For i As Integer = 0 To 4
                Dim yp As Integer = pad + CInt(ch * i / 4) : Dim v As Decimal = maxV * (4 - i) / 4
                g.DrawLine(New Pen(Color.FromArgb(232, 225, 215)), pad, yp, w - pad, yp)
                g.DrawString("Rs." & (v / 1000).ToString("N0") & "K", New System.Drawing.Font("Segoe UI", 7), New SolidBrush(AppTheme.TextLight), 2, yp - 8)
            Next
            Dim keys As List(Of String) = chartData.Keys.ToList()
            Dim bw As Integer = Math.Max(6, CInt(cw / keys.Count) - 6) : Dim bs As Integer = CInt(cw / keys.Count)
            For i As Integer = 0 To keys.Count - 1
                Dim v As Decimal = chartData(keys(i)) : Dim bh As Integer = CInt(ch * (v / maxV))
                Dim xp As Integer = pad + i * bs + (bs - bw) / 2 : Dim yp As Integer = pad + ch - bh
                If bh > 0 Then
                    Dim barRect As New System.Drawing.Rectangle(xp, yp, bw, bh)
                    Using brush As New Drawing2D.LinearGradientBrush(barRect, Color.FromArgb(220, 90, 65), AppTheme.Primary, 90)
                        g.FillRectangle(brush, barRect)
                    End Using
                    If bh > 18 Then g.DrawString("Rs." & (v / 1000).ToString("N1") & "K", New System.Drawing.Font("Segoe UI", 7, FontStyle.Bold), Brushes.White, xp + 2, yp + 3)
                End If
                If keys.Count <= 20 Then
                    Dim parts() As String = keys(i).Split("-"c) : Dim xLbl As String = If(parts.Length = 3, parts(0) & "/" & parts(1), keys(i).Substring(0, Math.Min(6, keys(i).Length)))
                    g.DrawString(xLbl, New System.Drawing.Font("Segoe UI", 7), New SolidBrush(AppTheme.TextMed), xp - 2, pad + ch + 6)
                End If
            Next
            g.DrawLine(New Pen(AppTheme.TextMed, 1.5F), pad, pad, pad, pad + ch) : g.DrawLine(New Pen(AppTheme.TextMed, 1.5F), pad, pad + ch, w - pad, pad + ch)
        End Sub

        Private Sub ExportExcel()
            If dgvSales.Rows.Count = 0 Then
                MessageBox.Show("No data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim sfd As New SaveFileDialog() : sfd.Filter = "Excel|*.xlsx" : sfd.FileName = "Report_" & dtpFrom.Value.ToString("yyyyMMdd")
            If sfd.ShowDialog() <> DialogResult.OK Then Return
            Try
                Dim wb As New XSSFWorkbook() : Dim sh As ISheet = wb.CreateSheet("Orders")
                Dim hStyle As ICellStyle = wb.CreateCellStyle() : Dim hFont As NPOI.SS.UserModel.IFont = wb.CreateFont() : hFont.IsBold = True : hStyle.SetFont(hFont) : hStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Orange.Index : hStyle.FillPattern = FillPattern.SolidForeground
                Dim hRow As IRow = sh.CreateRow(0)
                For c As Integer = 0 To dgvSales.Columns.Count - 1 : Dim cell As ICell = hRow.CreateCell(c) : cell.SetCellValue(dgvSales.Columns(c).HeaderText) : cell.CellStyle = hStyle : Next
                For r As Integer = 0 To dgvSales.Rows.Count - 1
                    Dim dRow As IRow = sh.CreateRow(r + 1)
                    For c As Integer = 0 To dgvSales.Columns.Count - 1
                        Dim v As Object = dgvSales.Rows(r).Cells(c).Value
                        If v IsNot Nothing Then
                            Dim cell As ICell = dRow.CreateCell(c)
                            If TypeOf v Is Decimal OrElse TypeOf v Is Integer OrElse TypeOf v Is Double Then cell.SetCellValue(CDbl(v)) Else cell.SetCellValue(v.ToString())
                        End If
                    Next
                Next
                For c As Integer = 0 To dgvSales.Columns.Count - 1 : sh.AutoSizeColumn(c) : Next
                Using fs As New FileStream(sfd.FileName, FileMode.Create, FileAccess.Write) : wb.Write(fs) : End Using
                MessageBox.Show("Excel exported!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Dim psi As New ProcessStartInfo(sfd.FileName)
                psi.UseShellExecute = True
                Process.Start(psi)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub ExportPDF()
            If dgvSales.Rows.Count = 0 Then
                MessageBox.Show("No data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim sfd As New SaveFileDialog() : sfd.Filter = "PDF|*.pdf" : sfd.FileName = "Report_" & dtpFrom.Value.ToString("yyyyMMdd")
            If sfd.ShowDialog() <> DialogResult.OK Then Return
            Try
                Dim doc As New iTextSharp.text.Document(PageSize.A4.Rotate(), 30, 30, 40, 30) : PdfWriter.GetInstance(doc, New FileStream(sfd.FileName, FileMode.Create)) : doc.Open()
                Dim tF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, New BaseColor(183, 55, 35))
                Dim nF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA, 9)
                Dim wF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)
                Dim pTitle As New Paragraph("Restaurant POS — Sales Report", tF)
                pTitle.Alignment = Element.ALIGN_CENTER
                doc.Add(pTitle)
                Dim periodPara As New Paragraph("Period: " & dtpFrom.Value.ToString("dd/MM/yyyy") & " to " & dtpTo.Value.ToString("dd/MM/yyyy") & "   Revenue: " & lblRev.Text & "   Orders: " & lblOrders.Text, FontFactory.GetFont(FontFactory.HELVETICA, 9, New BaseColor(110, 90, 70)))
                periodPara.Alignment = Element.ALIGN_CENTER
                doc.Add(periodPara)
                doc.Add(New Paragraph(" "))
                Dim cnt As Integer = dgvSales.Columns.Count : Dim tbl2 As New PdfPTable(cnt) : tbl2.WidthPercentage = 100
                For c As Integer = 0 To cnt - 1 : Dim hc As New PdfPCell(New Phrase(dgvSales.Columns(c).HeaderText, wF)) : hc.BackgroundColor = New BaseColor(183, 55, 35) : hc.Padding = 6 : hc.HorizontalAlignment = Element.ALIGN_CENTER : tbl2.AddCell(hc) : Next
                Dim alt As New BaseColor(252, 248, 244)
                For r As Integer = 0 To dgvSales.Rows.Count - 1
                    Dim bg As BaseColor = If(r Mod 2 = 0, BaseColor.WHITE, alt)
                    For c As Integer = 0 To cnt - 1
                        Dim vv As String = If(dgvSales.Rows(r).Cells(c).Value IsNot Nothing, dgvSales.Rows(r).Cells(c).Value.ToString(), "")
                        Dim cell As New PdfPCell(New Phrase(vv, nF)) : cell.BackgroundColor = bg : cell.Padding = 5 : tbl2.AddCell(cell)
                    Next
                Next
                doc.Add(tbl2) : doc.Close()
                MessageBox.Show("PDF exported!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Dim psi As New ProcessStartInfo(sfd.FileName)
                psi.UseShellExecute = True
                Process.Start(psi)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class

    '═══════════════════════════════════════════════
    ' USER MODULE
    '═══════════════════════════════════════════════
    Public Class UserModule
        Inherits Panel
        Private dgvUsers, dgvLog As DataGridView
        Private tabCtrl As TabControl
        Private lblFT As Label
        Private txtUser, txtFull, txtEmail, txtPass As TextBox
        Private cmbRole As ComboBox
        Private chkActive As CheckBox
        Private currentUID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadUsers() : LoadLog()
        End Sub

        Private Sub Build()
            Dim pBar As Panel = AppTheme.MakeToolbar()
            Me.Controls.Add(pBar)
            Dim btnNew As New Button() : btnNew.Text = "New User" : btnNew.Size = New Size(110, 32) : btnNew.Location = New Point(10, 10)
            AppTheme.StyleBtn(btnNew, "success") : AddHandler btnNew.Click, AddressOf NewUser : pBar.Controls.Add(btnNew)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 352)
            Me.Controls.Add(split)

            tabCtrl = New TabControl() : tabCtrl.Dock = DockStyle.Fill : tabCtrl.Font = AppTheme.F10 : tabCtrl.Margin = New Padding(0, 0, 4, 0)
            split.Controls.Add(tabCtrl, 0, 0)

            Dim tU As New TabPage("  Users  ") : tU.BackColor = Color.White : tabCtrl.TabPages.Add(tU)
            dgvUsers = New DataGridView() : dgvUsers.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvUsers)
            AddHandler dgvUsers.SelectionChanged, Sub(s, e)
                                                      If dgvUsers.CurrentRow IsNot Nothing AndAlso dgvUsers.CurrentRow.Index >= 0 Then
                                                          Try : LoadForm(CInt(dgvUsers.CurrentRow.Cells("UserID").Value)) : Catch : End Try
                                                      End If
                                                  End Sub
            tU.Controls.Add(dgvUsers)

            Dim tL As New TabPage("  Activity Log  ") : tL.BackColor = Color.White : tabCtrl.TabPages.Add(tL)
            Dim tLtbl As New TableLayoutPanel()
            tLtbl.Dock = DockStyle.Fill : tLtbl.ColumnCount = 1 : tLtbl.RowCount = 2
            tLtbl.Padding = New Padding(0) : tLtbl.Margin = New Padding(0)
            tLtbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            tLtbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            tLtbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
            tLtbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            tL.Controls.Add(tLtbl)

            Dim btnRef As New Button() : btnRef.Text = "Refresh" : btnRef.Dock = DockStyle.Fill
            AppTheme.StyleBtn(btnRef, "secondary")
            AddHandler btnRef.Click, Sub(s, e) LoadLog()
            tLtbl.Controls.Add(btnRef, 0, 0)
            dgvLog = New DataGridView() : dgvLog.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvLog) : tLtbl.Controls.Add(dgvLog, 0, 1)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(4, 0, 0, 0)
            split.Controls.Add(pForm, 1, 0)
            pForm.Controls.Add(AppTheme.AccentBar())
            lblFT = AppTheme.FormTitle("User Account") : pForm.Controls.Add(lblFT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 6, 14, 10) : pF.BackColor = Color.White : pForm.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 50
            AddL(pF, "Username *", y) : txtUser = NTB(pF, y + 18) : y += sp
            AddL(pF, "Full Name", y) : txtFull = NTB(pF, y + 18) : y += sp
            AddL(pF, "Email", y) : txtEmail = NTB(pF, y + 18) : y += sp
            AddL(pF, "Role", y) : cmbRole = New ComboBox() : cmbRole.Location = New Point(0, y + 18) : cmbRole.Size = New Size(318, 26) : cmbRole.Font = AppTheme.F10 : cmbRole.DropDownStyle = ComboBoxStyle.DropDownList : cmbRole.Items.AddRange({"Admin", "Manager", "Staff"}) : cmbRole.SelectedIndex = 2 : pF.Controls.Add(cmbRole) : y += sp
            AddL(pF, "Password (blank = keep existing)", y) : txtPass = NTB(pF, y + 18) : txtPass.UseSystemPasswordChar = True : y += sp
            chkActive = New CheckBox() : chkActive.Text = "Account Active" : chkActive.Font = AppTheme.F10 : chkActive.ForeColor = AppTheme.TextDark : chkActive.Checked = True : chkActive.Location = New Point(0, y) : chkActive.AutoSize = True : pF.Controls.Add(chkActive) : y += 36
            Dim btnS As New Button() : btnS.Text = "Save" : btnS.Size = New Size(94, 34) : btnS.Location = New Point(0, y) : AppTheme.StyleBtn(btnS, "success") : AddHandler btnS.Click, AddressOf BtnSave : pF.Controls.Add(btnS)
            Dim btnD As New Button() : btnD.Text = "Deactivate" : btnD.Size = New Size(110, 34) : btnD.Location = New Point(102, y) : AppTheme.StyleBtn(btnD, "danger") : AddHandler btnD.Click, AddressOf BtnDeactivate : pF.Controls.Add(btnD)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(318, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function

        Private Sub LoadUsers()
            Try
                dgvUsers.DataSource = DatabaseManager.GetDataTable("SELECT UserID,Username,FullName,Role,IsActive,CONVERT(VARCHAR,LastLogin,120) AS LastLogin FROM Users ORDER BY Username")
                If dgvUsers.Columns.Contains("UserID") Then dgvUsers.Columns("UserID").Visible = False
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadLog()
            Try
                dgvLog.DataSource = DatabaseManager.GetDataTable("SELECT TOP 200 Username,Action,Details,CONVERT(VARCHAR,LogDate,120) AS [DateTime] FROM ActivityLog ORDER BY LogDate DESC")
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadForm(uid As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT * FROM Users WHERE UserID=@id", New SqlParameter("@id", uid))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentUID = uid : lblFT.Text = "Edit: " & row("Username").ToString()
                txtUser.Text = row("Username").ToString() : txtFull.Text = row("FullName").ToString() : txtEmail.Text = row("Email").ToString() : txtPass.Text = ""
                Dim ri As Integer = cmbRole.Items.IndexOf(row("Role").ToString()) : If ri >= 0 Then cmbRole.SelectedIndex = ri
                chkActive.Checked = CBool(row("IsActive"))
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub NewUser(sender As Object, e As EventArgs)
            currentUID = 0 : lblFT.Text = "New User" : txtUser.Text = "" : txtFull.Text = "" : txtEmail.Text = "" : txtPass.Text = "" : cmbRole.SelectedIndex = 2 : chkActive.Checked = True : txtUser.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtUser.Text.Trim()) Then
                MessageBox.Show("Username required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If currentUID = 0 AndAlso String.IsNullOrEmpty(txtPass.Text) Then
                MessageBox.Show("Password required for new user.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Try
                If currentUID = 0 Then
                    Dim h As String = BCrypt.Net.BCrypt.HashPassword(txtPass.Text)
                    DatabaseManager.ExecuteNonQuery("INSERT INTO Users(Username,PasswordHash,Role,FullName,Email,IsActive) VALUES(@u,@h,@r,@fn,@e,@a)", New SqlParameter("@u", txtUser.Text.Trim()), New SqlParameter("@h", h), New SqlParameter("@r", cmbRole.Text), New SqlParameter("@fn", txtFull.Text.Trim()), New SqlParameter("@e", txtEmail.Text.Trim()), New SqlParameter("@a", If(chkActive.Checked, 1, 0)))
                    MessageBox.Show("User created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    Dim sql As String = "UPDATE Users SET Username=@u,Role=@r,FullName=@fn,Email=@e,IsActive=@a"
                    Dim prms As New List(Of SqlParameter) From {New SqlParameter("@u", txtUser.Text.Trim()), New SqlParameter("@r", cmbRole.Text), New SqlParameter("@fn", txtFull.Text.Trim()), New SqlParameter("@e", txtEmail.Text.Trim()), New SqlParameter("@a", If(chkActive.Checked, 1, 0))}
                    If Not String.IsNullOrEmpty(txtPass.Text) Then sql &= ",PasswordHash=@h" : prms.Add(New SqlParameter("@h", BCrypt.Net.BCrypt.HashPassword(txtPass.Text)))
                    sql &= " WHERE UserID=@id" : prms.Add(New SqlParameter("@id", currentUID))
                    DatabaseManager.ExecuteNonQuery(sql, prms.ToArray())
                    MessageBox.Show("User updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                DatabaseManager.Log(SessionManager.UserID, SessionManager.Username, "USER_SAVE", txtUser.Text) : LoadUsers()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnDeactivate(sender As Object, e As EventArgs)
            If currentUID = 0 Then Return
            If currentUID = SessionManager.UserID Then
                MessageBox.Show("Cannot deactivate your own account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            If MessageBox.Show("Deactivate this user?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("UPDATE Users SET IsActive=0 WHERE UserID=@id", New SqlParameter("@id", currentUID))
                LoadUsers() : NewUser(Nothing, Nothing)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class

    '═══════════════════════════════════════════════
    ' SETTINGS MODULE
    '═══════════════════════════════════════════════
    Public Class SettingsModule
        Inherits Panel
        Private txtConn As TextBox
        Private lblStatus As Label

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build()
        End Sub

        Private Sub Build()
            Dim scroll As New Panel() : scroll.Dock = DockStyle.Fill : scroll.AutoScroll = True : scroll.BackColor = Color.White : scroll.Padding = New Padding(24, 18, 24, 18) : Me.Controls.Add(scroll)
            Dim y As Integer = 0

            AddSec(scroll, "Application Information", y) : y += 38
            Dim info() As String = {
                "Application:   Restaurant POS v1.0",
                "Framework:     .NET 10 (net10.0-windows) — Windows Forms",
                "IDE:           Visual Studio 2026 (v18.6.2)",
                "Logged In As:  " & SessionManager.FullName & "  [" & SessionManager.Role & "]",
                "Session Start: " & SessionManager.LoginTime.ToString("dd/MM/yyyy HH:mm:ss"),
                "Machine:       " & Environment.MachineName
            }
            For Each line As String In info
                Dim l As New Label() : l.Text = line : l.Font = AppTheme.F10 : l.ForeColor = AppTheme.TextDark : l.AutoSize = True : l.Location = New Point(0, y) : scroll.Controls.Add(l) : y += 28
            Next
            y += 10

            AddSec(scroll, "Database Connection", y) : y += 42
            Dim lC As New Label() : lC.Text = "Connection String:" : lC.Font = AppTheme.F8B : lC.ForeColor = AppTheme.TextMed : lC.AutoSize = True : lC.Location = New Point(0, y) : scroll.Controls.Add(lC)
            txtConn = New TextBox() : txtConn.Location = New Point(0, y + 18) : txtConn.Size = New Size(740, 28) : txtConn.Font = AppTheme.F9 : txtConn.BorderStyle = BorderStyle.FixedSingle : txtConn.BackColor = AppTheme.InputBg : txtConn.Text = DatabaseManager.ConnectionString : scroll.Controls.Add(txtConn) : y += 62

            Dim btnTest As New Button() : btnTest.Text = "Test Connection" : btnTest.Size = New Size(156, 34) : btnTest.Location = New Point(0, y) : AppTheme.StyleBtn(btnTest, "success")
            AddHandler btnTest.Click, Sub(s, e)
                                          Try
                                              Using conn As New SqlConnection(txtConn.Text) : conn.Open() : SetStatus("Connected to: " & conn.DataSource, AppTheme.Green) : End Using
                                          Catch ex As Exception : SetStatus("Failed: " & ex.Message.Substring(0, Math.Min(80, ex.Message.Length)), AppTheme.Red) : End Try
                                      End Sub
            scroll.Controls.Add(btnTest)
            Dim btnUpd As New Button() : btnUpd.Text = "Update" : btnUpd.Size = New Size(110, 34) : btnUpd.Location = New Point(164, y) : AppTheme.StyleBtn(btnUpd, "info")
            AddHandler btnUpd.Click, Sub(s, e)
                                         DatabaseManager.ConnectionString = txtConn.Text
                                         SetStatus("Connection string updated.", AppTheme.Blue)
                                     End Sub
            scroll.Controls.Add(btnUpd) : y += 50

            AddSec(scroll, "Backup & Security", y) : y += 42
            Dim btnBak As New Button() : btnBak.Text = "Backup Database" : btnBak.Size = New Size(176, 34) : btnBak.Location = New Point(0, y) : AppTheme.StyleBtn(btnBak, "primary")
            AddHandler btnBak.Click, Sub(s, e)
                                         Dim sfd As New SaveFileDialog() : sfd.Filter = "Backup|*.bak" : sfd.FileName = "RestaurantPOS_" & DateTime.Now.ToString("yyyyMMdd_HHmm")
                                         If sfd.ShowDialog() <> DialogResult.OK Then Return
                                         Try
                                             Using conn As New SqlConnection(DatabaseManager.ConnectionString) : conn.Open() : Dim cmd As New SqlCommand("BACKUP DATABASE RestaurantPOSDB TO DISK='" & sfd.FileName & "' WITH FORMAT,INIT", conn) : cmd.CommandTimeout = 120 : cmd.ExecuteNonQuery() : End Using
                                             SetStatus("Backup saved: " & sfd.FileName, AppTheme.Green) : MessageBox.Show("Backup completed!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                         Catch ex As Exception : SetStatus("Backup failed: " & ex.Message, AppTheme.Red) : End Try
                                     End Sub
            scroll.Controls.Add(btnBak)

            Dim btnPwd As New Button() : btnPwd.Text = "Change Password" : btnPwd.Size = New Size(166, 34) : btnPwd.Location = New Point(184, y) : AppTheme.StyleBtn(btnPwd, "warning") : AddHandler btnPwd.Click, AddressOf ChangePwd : scroll.Controls.Add(btnPwd) : y += 52

            lblStatus = New Label() : lblStatus.Text = "" : lblStatus.Font = AppTheme.F10B : lblStatus.ForeColor = AppTheme.Green : lblStatus.AutoSize = True : lblStatus.Location = New Point(0, y) : scroll.Controls.Add(lblStatus)
        End Sub

        Private Sub AddSec(p As Control, title As String, y As Integer)
            Dim pnl As New Panel() : pnl.Location = New Point(0, y) : pnl.Size = New Size(900, 32) : pnl.BackColor = Color.FromArgb(238, 230, 222)
            Dim l As New Label() : l.Text = "  " & title : l.Font = AppTheme.F10B : l.ForeColor = AppTheme.Primary : l.Dock = DockStyle.Fill : l.TextAlign = ContentAlignment.MiddleLeft : pnl.Controls.Add(l) : p.Controls.Add(pnl)
        End Sub

        Private Sub SetStatus(msg As String, color As Color)
            lblStatus.Text = msg : lblStatus.ForeColor = color
        End Sub

        Private Sub ChangePwd(sender As Object, e As EventArgs)
            Dim frm As New Form() : frm.Text = "Change Password" : frm.Size = New Size(380, 260) : frm.StartPosition = FormStartPosition.CenterParent : frm.BackColor = Color.White : frm.FormBorderStyle = FormBorderStyle.FixedDialog : frm.MaximizeBox = False
            Dim AddF As Action(Of String, Integer, Control) = Sub(t, y2, ctrl)
                                                                  Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(20, y2) : l.AutoSize = True : frm.Controls.Add(l)
                                                                  ctrl.Location = New Point(20, y2 + 18) : frm.Controls.Add(ctrl)
                                                              End Sub
            Dim tbC As New TextBox() : tbC.Size = New Size(330, 28) : tbC.Font = AppTheme.F10 : tbC.BorderStyle = BorderStyle.FixedSingle : tbC.UseSystemPasswordChar = True : AddF("Current Password:", 20, tbC)
            Dim tbN As New TextBox() : tbN.Size = New Size(330, 28) : tbN.Font = AppTheme.F10 : tbN.BorderStyle = BorderStyle.FixedSingle : tbN.UseSystemPasswordChar = True : AddF("New Password:", 78, tbN)
            Dim tbCf As New TextBox() : tbCf.Size = New Size(330, 28) : tbCf.Font = AppTheme.F10 : tbCf.BorderStyle = BorderStyle.FixedSingle : tbCf.UseSystemPasswordChar = True : AddF("Confirm New Password:", 136, tbCf)
            Dim btnChg As New Button() : btnChg.Text = "Change" : btnChg.Size = New Size(150, 36) : btnChg.Location = New Point(20, 192) : AppTheme.StyleBtn(btnChg, "primary") : frm.Controls.Add(btnChg)
            AddHandler btnChg.Click, Sub(s, ev)
                                         If tbN.Text <> tbCf.Text Then
                                             MessageBox.Show("Passwords do not match.")
                                             Return
                                         End If
                                         If tbN.Text.Length < 6 Then
                                             MessageBox.Show("Minimum 6 characters.")
                                             Return
                                         End If
                                         Try
                                             Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT PasswordHash FROM Users WHERE UserID=@id", New SqlParameter("@id", SessionManager.UserID))
                                             If dt.Rows.Count > 0 AndAlso BCrypt.Net.BCrypt.Verify(tbC.Text, dt.Rows(0)("PasswordHash").ToString()) Then
                                                 DatabaseManager.ExecuteNonQuery("UPDATE Users SET PasswordHash=@h WHERE UserID=@id", New SqlParameter("@h", BCrypt.Net.BCrypt.HashPassword(tbN.Text)), New SqlParameter("@id", SessionManager.UserID))
                                                 MessageBox.Show("Password changed!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information) : frm.Close()
                                             Else
                                                 MessageBox.Show("Current password incorrect.")
                                             End If
                                         Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
                                     End Sub
            frm.ShowDialog(Me.FindForm())
        End Sub
    End Class

End Namespace