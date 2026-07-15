Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient
Imports System.Linq
Imports System.IO
Imports iTextSharp.text
Imports iTextSharp.text.pdf

Namespace RestaurantPOS
    Public Class BillingModule
        Inherits Panel
        Private cmbTable, cmbType, cmbPayment As ComboBox
        Private txtCustomer, txtPhone, txtNotes, txtDiscount, txtPaid, txtSearch As TextBox
        Private cmbCategory As ComboBox
        Private dgvMenu, dgvOrder As DataGridView
        Private lblSub, lblTax, lblDisc, lblTotal, lblChange As Label
        Private menuData As New DataTable()
        Private orderItems As New DataTable()
        Private currentOrderID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            InitTable() : Build() : LoadTables() : LoadCats() : LoadMenu() : LoadOpenOrders()
        End Sub

        Private Sub InitTable()
            orderItems.Columns.Add("ItemID", GetType(Integer)) : orderItems.Columns.Add("Item", GetType(String))
            orderItems.Columns.Add("Price", GetType(Decimal)) : orderItems.Columns.Add("Tax%", GetType(Decimal))
            orderItems.Columns.Add("Qty", GetType(Integer)) : orderItems.Columns.Add("Total", GetType(Decimal))
        End Sub

        Private Sub Build()
            ' ROOT: TableLayoutPanel with 2 rows - top info bar (fixed 60px) | body (fill)
            Dim rootTbl As New TableLayoutPanel()
            rootTbl.Dock = DockStyle.Fill : rootTbl.ColumnCount = 1 : rootTbl.RowCount = 2
            rootTbl.Padding = New Padding(0) : rootTbl.Margin = New Padding(0)
            rootTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rootTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rootTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 64))
            rootTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Me.Controls.Add(rootTbl)

            ' TOP BAR
            Dim pTop As New Panel()
            pTop.Dock = DockStyle.Fill : pTop.BackColor = Color.White
            AddHandler pTop.Paint, Sub(s, e) e.Graphics.DrawLine(New Pen(AppTheme.BorderColor), 0, DirectCast(s, Panel).Height - 1, DirectCast(s, Panel).Width, DirectCast(s, Panel).Height - 1)
            rootTbl.Controls.Add(pTop, 0, 0)
            Dim tf As New FlowLayoutPanel()
            tf.Dock = DockStyle.Fill : tf.FlowDirection = FlowDirection.LeftToRight : tf.WrapContents = False
            tf.Padding = New Padding(8, 8, 8, 8) : tf.BackColor = Color.Transparent : pTop.Controls.Add(tf)
            cmbTable = AddTC(tf, "Table:", 110) : cmbType = AddTC(tf, "Type:", 118)
            cmbType.Items.AddRange({"Dine-In", "Takeaway", "Delivery", "Counter"}) : cmbType.SelectedIndex = 0
            txtCustomer = AddTT(tf, "Customer:", 175) : txtCustomer.Text = "Walk-in Guest"
            txtPhone = AddTT(tf, "Phone:", 130) : txtNotes = AddTT(tf, "Notes:", 205)

            ' BODY 3 cols: 38% menu | 36% order | 26% right
            Dim body As New TableLayoutPanel()
            body.Dock = DockStyle.Fill : body.ColumnCount = 3 : body.RowCount = 1
            body.Padding = New Padding(8) : body.Margin = New Padding(0) : body.BackColor = AppTheme.ContentBg
            body.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38))
            body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 36))
            body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 26))
            body.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            rootTbl.Controls.Add(body, 0, 1)

            ' --- MENU PANEL ---
            Dim pM As New Panel() : pM.Dock = DockStyle.Fill : pM.BackColor = Color.White : pM.Margin = New Padding(0, 0, 4, 0)
            body.Controls.Add(pM, 0, 0)

            Dim mTbl As New TableLayoutPanel()
            mTbl.Dock = DockStyle.Fill : mTbl.ColumnCount = 1 : mTbl.RowCount = 2
            mTbl.Padding = New Padding(0) : mTbl.Margin = New Padding(0)
            mTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            mTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            mTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
            mTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            pM.Controls.Add(mTbl)

            Dim pMbar As New TableLayoutPanel()
            pMbar.Dock = DockStyle.Fill : pMbar.ColumnCount = 2 : pMbar.RowCount = 1
            pMbar.Padding = New Padding(6, 7, 6, 7) : pMbar.BackColor = Color.White
            pMbar.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            pMbar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52))
            pMbar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 48))
            mTbl.Controls.Add(pMbar, 0, 0)
            cmbCategory = New ComboBox() : cmbCategory.Dock = DockStyle.Fill : cmbCategory.Font = AppTheme.F9
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList : cmbCategory.Margin = New Padding(0, 0, 3, 0)
            AddHandler cmbCategory.SelectedIndexChanged, Sub(s, e) LoadMenu()
            pMbar.Controls.Add(cmbCategory, 0, 0)
            txtSearch = New TextBox() : txtSearch.Dock = DockStyle.Fill : txtSearch.Font = AppTheme.F9
            txtSearch.BorderStyle = BorderStyle.FixedSingle : txtSearch.PlaceholderText = "Search..."
            AddHandler txtSearch.TextChanged, Sub(s, e) LoadMenu(txtSearch.Text)
            pMbar.Controls.Add(txtSearch, 1, 0)
            dgvMenu = New DataGridView() : dgvMenu.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvMenu)
            AddHandler dgvMenu.DataBindingComplete, Sub(s, e)
                                                        If dgvMenu.Columns.Contains("ItemID") Then dgvMenu.Columns("ItemID").Visible = False
                                                    End Sub
            AddHandler dgvMenu.CellDoubleClick, Sub(s, e)
                                                    If e.RowIndex >= 0 Then AddItem(e.RowIndex)
                                                End Sub
            mTbl.Controls.Add(dgvMenu, 0, 1)

            ' --- ORDER PANEL ---
            Dim pO As New Panel() : pO.Dock = DockStyle.Fill : pO.BackColor = Color.White : pO.Margin = New Padding(4, 0, 4, 0)
            body.Controls.Add(pO, 1, 0)

            Dim oTbl As New TableLayoutPanel()
            oTbl.Dock = DockStyle.Fill : oTbl.ColumnCount = 1 : oTbl.RowCount = 3
            oTbl.Padding = New Padding(0) : oTbl.Margin = New Padding(0)
            oTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            oTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            oTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
            oTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            oTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
            pO.Controls.Add(oTbl)

            Dim pOhdr As New Panel() : pOhdr.Dock = DockStyle.Fill : pOhdr.BackColor = AppTheme.PrimaryDark
            oTbl.Controls.Add(pOhdr, 0, 0)
            Dim lOH As New Label() : lOH.Text = "  CURRENT ORDER  (double-click menu item to add)"
            lOH.Dock = DockStyle.Fill : lOH.Font = AppTheme.F10B : lOH.ForeColor = Color.White : lOH.TextAlign = ContentAlignment.MiddleLeft
            pOhdr.Controls.Add(lOH)

            dgvOrder = New DataGridView() : dgvOrder.DataSource = orderItems : dgvOrder.Dock = DockStyle.Fill
            AppTheme.MakeGrid(dgvOrder) : dgvOrder.AllowUserToDeleteRows = False
            AddHandler dgvOrder.DataBindingComplete, Sub(s, e)
                                                         If dgvOrder.Columns.Contains("ItemID") Then dgvOrder.Columns("ItemID").Visible = False
                                                     End Sub
            If dgvOrder.Columns.Contains("ItemID") Then dgvOrder.Columns("ItemID").Visible = False
            oTbl.Controls.Add(dgvOrder, 0, 1)

            Dim btnRem As New Button() : btnRem.Text = "Remove Selected" : btnRem.Dock = DockStyle.Fill
            AppTheme.StyleBtn(btnRem, "danger") : btnRem.Font = AppTheme.F9B
            AddHandler btnRem.Click, Sub(s, e)
                                         If dgvOrder.CurrentRow IsNot Nothing AndAlso dgvOrder.CurrentRow.Index >= 0 Then
                                             orderItems.Rows.RemoveAt(dgvOrder.CurrentRow.Index) : Recalc()
                                         End If
                                     End Sub
            oTbl.Controls.Add(btnRem, 0, 2)

            ' --- RIGHT PANEL ---
            Dim pR As New Panel() : pR.Dock = DockStyle.Fill : pR.BackColor = Color.White : pR.Margin = New Padding(4, 0, 0, 0)
            body.Controls.Add(pR, 2, 0)

            Dim tbl As New TableLayoutPanel()
            tbl.Dock = DockStyle.Fill : tbl.ColumnCount = 1 : tbl.Padding = New Padding(12, 10, 12, 8)
            tbl.BackColor = Color.White : tbl.AutoSize = False

            ' Totals
            AddTRow(tbl, "Sub Total:", lblSub) : AddTRow(tbl, "Tax Amount:", lblTax) : AddTRow(tbl, "Discount:", lblDisc)

            ' Grand total
            Dim pG As New Panel() : pG.Height = 60 : pG.BackColor = AppTheme.Primary : pG.Dock = DockStyle.Fill : pG.Margin = New Padding(0, 8, 0, 8)
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 76)) : tbl.Controls.Add(pG)
            Dim lGT As New Label() : lGT.Text = "GRAND TOTAL" : lGT.Font = AppTheme.F8B : lGT.ForeColor = Color.White
            lGT.Location = New Point(12, 8) : lGT.AutoSize = True : pG.Controls.Add(lGT)
            lblTotal = New Label() : lblTotal.Text = "Rs. 0.00"
            lblTotal.Font = New System.Drawing.Font("Segoe UI", 15, FontStyle.Bold) : lblTotal.ForeColor = Color.FromArgb(255, 228, 100)
            lblTotal.Location = New Point(12, 26) : lblTotal.AutoSize = True : pG.Controls.Add(lblTotal)

            ' Discount + Payment row
            Dim pPay As New TableLayoutPanel()
            pPay.ColumnCount = 4 : pPay.RowCount = 1 : pPay.Dock = DockStyle.Fill
            pPay.Margin = New Padding(0, 0, 0, 4) : pPay.BackColor = Color.Transparent
            pPay.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 58))
            pPay.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 58))
            pPay.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 42))
            pPay.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 34)) : tbl.Controls.Add(pPay)
            AddPL(pPay, "Disc%:", 0) : txtDiscount = AddPTB(pPay, 1) : txtDiscount.Text = "0" : txtDiscount.TextAlign = HorizontalAlignment.Center
            AddHandler txtDiscount.TextChanged, Sub(s, e) Recalc()
            AddPL(pPay, "Pay:", 2)
            cmbPayment = New ComboBox() : cmbPayment.Dock = DockStyle.Fill : cmbPayment.Font = AppTheme.F9
            cmbPayment.DropDownStyle = ComboBoxStyle.DropDownList
            cmbPayment.Items.AddRange({"Cash", "Card", "UPI", "Wallet"}) : cmbPayment.SelectedIndex = 0
            pPay.Controls.Add(cmbPayment, 3, 0)

            ' Paid + Change row
            Dim pPaid As New TableLayoutPanel()
            pPaid.ColumnCount = 4 : pPaid.RowCount = 1 : pPaid.Dock = DockStyle.Fill
            pPaid.Margin = New Padding(0, 0, 0, 8) : pPaid.BackColor = Color.Transparent
            pPaid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 46))
            pPaid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 56))
            pPaid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 78))
            pPaid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 34)) : tbl.Controls.Add(pPaid)
            AddPL(pPaid, "Paid:", 0) : txtPaid = AddPTB(pPaid, 1) : txtPaid.Text = "0" : txtPaid.TextAlign = HorizontalAlignment.Center
            AddHandler txtPaid.TextChanged, Sub(s, e) CalcChange()
            AddPL(pPaid, "Change:", 2)
            lblChange = New Label() : lblChange.Text = "Rs. 0.00" : lblChange.Font = AppTheme.F9B
            lblChange.ForeColor = AppTheme.Green : lblChange.Dock = DockStyle.Fill : lblChange.TextAlign = ContentAlignment.MiddleLeft
            pPaid.Controls.Add(lblChange, 3, 0)

            ' Action buttons
            Dim btnDefs() As String = {"Add to Order", "Save Order", "Close & Pay", "KOT Print", "PDF Bill", "Clear"}
            Dim btnStys() As String = {"success", "info", "primary", "warning", "secondary", "danger"}
            For bi As Integer = 0 To 5
                Dim ab As New Button() : ab.Text = btnDefs(bi) : ab.Dock = DockStyle.Fill : ab.Margin = New Padding(0, 0, 0, 4)
                AppTheme.StyleBtn(ab, btnStys(bi))
                tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 40)) : Dim bname As String = btnDefs(bi)
                AddHandler ab.Click, Sub(s, e) HandleAction(bname)
                tbl.Controls.Add(ab)
            Next

            ' Open orders
            Dim lOO As New Label() : lOO.Text = "Open Orders  (double-click to load)"
            lOO.Font = AppTheme.F9B : lOO.ForeColor = AppTheme.Primary : lOO.Dock = DockStyle.Fill : lOO.Margin = New Padding(0, 6, 0, 2)
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 26)) : tbl.Controls.Add(lOO)

            Dim dgvOpen As New DataGridView()
            dgvOpen.Name = "dgvOpen" : dgvOpen.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvOpen)
            AddHandler dgvOpen.CellDoubleClick, Sub(s, e)
                                                    If e.RowIndex >= 0 Then
                                                        Dim oid As Object = dgvOpen.Rows(e.RowIndex).Cells("OrderID").Value
                                                        If oid IsNot Nothing Then LoadExistingOrder(CInt(oid))
                                                    End If
                                                End Sub
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100)) : tbl.Controls.Add(dgvOpen)

            pR.Controls.Add(tbl)
        End Sub

        Private Sub AddTRow(tbl As TableLayoutPanel, lbl As String, ByRef valLbl As Label)
            Dim pRow As New TableLayoutPanel()
            pRow.ColumnCount = 2 : pRow.RowCount = 1 : pRow.Dock = DockStyle.Fill
            pRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            pRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            pRow.BackColor = Color.Transparent : pRow.Margin = New Padding(0, 0, 0, 4)
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 26)) : tbl.Controls.Add(pRow)
            Dim ll As New Label() : ll.Text = lbl : ll.Font = AppTheme.F9 : ll.ForeColor = AppTheme.TextMed
            ll.Dock = DockStyle.Fill : ll.TextAlign = ContentAlignment.MiddleLeft : pRow.Controls.Add(ll, 0, 0)
            valLbl = New Label() : valLbl.Text = "Rs. 0.00" : valLbl.Font = AppTheme.F9B : valLbl.ForeColor = AppTheme.TextDark
            valLbl.Dock = DockStyle.Fill : valLbl.TextAlign = ContentAlignment.MiddleRight : pRow.Controls.Add(valLbl, 1, 0)
        End Sub

        Private Function AddTC(flow As FlowLayoutPanel, lbl As String, w As Integer) As ComboBox
            Dim pnl As New Panel() : pnl.Size = New Size(w + 52, 44) : pnl.BackColor = Color.Transparent
            Dim l As New Label() : l.Text = lbl : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, 4) : l.AutoSize = True : pnl.Controls.Add(l)
            Dim cb As New ComboBox() : cb.Location = New Point(0, 22) : cb.Size = New Size(w, 22) : cb.Font = AppTheme.F9 : cb.DropDownStyle = ComboBoxStyle.DropDownList
            pnl.Controls.Add(cb) : flow.Controls.Add(pnl) : Return cb
        End Function

        Private Function AddTT(flow As FlowLayoutPanel, lbl As String, w As Integer) As TextBox
            Dim pnl As New Panel() : pnl.Size = New Size(w + 52, 44) : pnl.BackColor = Color.Transparent
            Dim l As New Label() : l.Text = lbl : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, 4) : l.AutoSize = True : pnl.Controls.Add(l)
            Dim tb As New TextBox() : tb.Location = New Point(0, 22) : tb.Size = New Size(w, 22) : tb.Font = AppTheme.F9
            tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg
            pnl.Controls.Add(tb) : flow.Controls.Add(pnl) : Return tb
        End Function

        Private Sub AddPL(p As TableLayoutPanel, t As String, col As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed
            l.Dock = DockStyle.Fill : l.TextAlign = ContentAlignment.MiddleLeft : l.Margin = New Padding(0, 0, 2, 0)
            l.AutoEllipsis = True
            p.Controls.Add(l, col, 0)
        End Sub

        Private Function AddPTB(p As TableLayoutPanel, col As Integer) As TextBox
            Dim tb As New TextBox() : tb.Dock = DockStyle.Fill : tb.Font = AppTheme.F9
            tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg
            p.Controls.Add(tb, col, 0) : Return tb
        End Function

        Private Sub LoadTables()
            Try
                cmbTable.Items.Clear() : cmbTable.Items.Add("No Table")
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT TableNumber FROM RestaurantTables ORDER BY TableNumber")
                For Each r As DataRow In dt.Rows : cmbTable.Items.Add(r("TableNumber").ToString()) : Next
                cmbTable.SelectedIndex = 0
            Catch
            End Try
        End Sub

        Private Sub LoadCats()
            Try
                cmbCategory.Items.Clear() : cmbCategory.Items.Add("All Categories")
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT Name FROM MenuCategories WHERE IsActive=1 ORDER BY SortOrder")
                For Each r As DataRow In dt.Rows : cmbCategory.Items.Add(r("Name").ToString()) : Next
                cmbCategory.SelectedIndex = 0
            Catch
            End Try
        End Sub

        Private Sub LoadMenu(Optional search As String = "")
            Try
                Dim sql As String = "SELECT mi.ItemID, mi.Name AS [Item Name], mc.Name AS Category, mi.Price, mi.TaxPercent AS [Tax%], CASE WHEN mi.IsVeg=1 THEN 'Veg' ELSE 'Non-Veg' END AS Type FROM MenuItems mi LEFT JOIN MenuCategories mc ON mi.CategoryID=mc.CategoryID WHERE mi.IsAvailable=1"
                Dim prms As New List(Of SqlParameter)()
                If cmbCategory.SelectedIndex > 0 Then sql &= " AND mc.Name=@cat" : prms.Add(New SqlParameter("@cat", cmbCategory.Text))
                If Not String.IsNullOrEmpty(search) Then sql &= " AND mi.Name LIKE @s" : prms.Add(New SqlParameter("@s", "%" & search & "%"))
                sql &= " ORDER BY mc.SortOrder, mi.Name"
                menuData = DatabaseManager.GetDataTable(sql, prms.ToArray())
                dgvMenu.DataSource = menuData
                If dgvMenu.Columns.Contains("ItemID") Then dgvMenu.Columns("ItemID").Visible = False
            Catch ex As Exception : MessageBox.Show("Menu error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadOpenOrders()
            Try
                Dim fc() As Control = Me.Controls.Find("dgvOpen", True)
                If fc.Length > 0 Then
                    Dim dg As DataGridView = TryCast(fc(0), DataGridView)
                    If dg IsNot Nothing Then
                        dg.DataSource = DatabaseManager.GetDataTable("SELECT OrderID, CustomerName, TotalAmount, CONVERT(VARCHAR,OrderDate,108) AS Time FROM Orders WHERE Status='Open' ORDER BY OrderID DESC")
                    End If
                End If
            Catch
            End Try
        End Sub

        Private Sub AddItem(rowIndex As Integer)
            If rowIndex < 0 OrElse rowIndex >= menuData.Rows.Count Then Return
            Dim row As DataRow = menuData.Rows(rowIndex)
            Dim itemID As Integer = CInt(row("ItemID")) : Dim price As Decimal = CDec(row("Price")) : Dim tax As Decimal = CDec(row("Tax%")) : Dim name As String = row("Item Name").ToString()
            Dim existing As DataRow = Nothing
            For Each r As DataRow In orderItems.Rows
                If CInt(r("ItemID")) = itemID Then
                    existing = r
                    Exit For
                End If
            Next
            If existing IsNot Nothing Then
                existing("Qty") = CInt(existing("Qty")) + 1 : existing("Total") = CDec(existing("Price")) * CInt(existing("Qty"))
            Else
                orderItems.Rows.Add(itemID, name, price, tax, 1, price)
            End If
            Recalc()
        End Sub

        Private Sub Recalc()
            Dim s1 As Decimal = 0 : Dim tA As Decimal = 0
            For Each r As DataRow In orderItems.Rows
                Dim lt As Decimal = CDec(r("Price")) * CInt(r("Qty"))
                s1 += lt
                tA += lt * CDec(r("Tax%")) / 100
            Next
            Dim dp As Decimal = 0 : Decimal.TryParse(txtDiscount.Text, dp) : Dim da As Decimal = s1 * dp / 100 : Dim grand As Decimal = s1 + tA - da
            lblSub.Text = "Rs. " & s1.ToString("N2") : lblTax.Text = "Rs. " & tA.ToString("N2") : lblDisc.Text = "Rs. " & da.ToString("N2") : lblTotal.Text = "Rs. " & grand.ToString("N2") : CalcChange()
        End Sub

        Private Sub CalcChange()
            Dim tot As Decimal = 0 : Decimal.TryParse(lblTotal.Text.Replace("Rs. ", "").Replace(",", ""), tot)
            Dim paid As Decimal = 0 : Decimal.TryParse(txtPaid.Text, paid) : Dim chg As Decimal = paid - tot
            lblChange.Text = "Rs. " & chg.ToString("N2") : lblChange.ForeColor = If(chg >= 0, AppTheme.Green, AppTheme.Red)
        End Sub

        Private Sub HandleAction(action As String)
            Select Case action
                Case "Add to Order" : If dgvMenu.CurrentRow IsNot Nothing Then AddItem(dgvMenu.CurrentRow.Index)
                Case "Save Order" : SaveOrder(False)
                Case "Close & Pay" : SaveOrder(True)
                Case "KOT Print" : PrintKOT()
                Case "PDF Bill" : ExportPDF()
                Case "Clear" : ClearOrder()
            End Select
        End Sub

        Private Sub SaveOrder(closeIt As Boolean)
            If orderItems.Rows.Count = 0 Then
                MessageBox.Show("Add items first.", "Empty Order", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim s1 As Decimal = 0 : Dim tA As Decimal = 0
            For Each r As DataRow In orderItems.Rows
                Dim lt As Decimal = CDec(r("Price")) * CInt(r("Qty"))
                s1 += lt
                tA += lt * CDec(r("Tax%")) / 100
            Next
            Dim dp As Decimal = 0 : Decimal.TryParse(txtDiscount.Text, dp) : Dim da As Decimal = s1 * dp / 100 : Dim grand As Decimal = s1 + tA - da
            Dim paid As Decimal = 0 : Decimal.TryParse(txtPaid.Text, paid) : Dim chg As Decimal = Math.Max(0, paid - grand)
            Dim tableID As Object = DBNull.Value
            If cmbTable.SelectedIndex > 0 Then
                Dim td As DataTable = DatabaseManager.GetDataTable("SELECT TableID FROM RestaurantTables WHERE TableNumber=@t", New SqlParameter("@t", cmbTable.Text))
                If td.Rows.Count > 0 Then tableID = CInt(td.Rows(0)("TableID"))
            End If
            Try
                If currentOrderID = 0 Then
                    currentOrderID = CInt(DatabaseManager.ExecuteScalar(
                        "INSERT INTO Orders(TableID,CustomerName,OrderType,Status,SubTotal,TaxAmount,DiscountPercent,DiscountAmount,TotalAmount,PaymentMode,PaidAmount,ChangeAmount,Notes,CreatedBy) OUTPUT INSERTED.OrderID VALUES(@tid,@cn,@ot,@st,@sub,@tax,@dp,@da,@tot,@pm,@paid,@chg,@nt,@cb)",
                        New SqlParameter("@tid", tableID), New SqlParameter("@cn", txtCustomer.Text.Trim()), New SqlParameter("@ot", cmbType.Text), New SqlParameter("@st", If(closeIt, "Closed", "Open")),
                        New SqlParameter("@sub", s1), New SqlParameter("@tax", tA), New SqlParameter("@dp", dp), New SqlParameter("@da", da), New SqlParameter("@tot", grand),
                        New SqlParameter("@pm", cmbPayment.Text), New SqlParameter("@paid", paid), New SqlParameter("@chg", chg), New SqlParameter("@nt", txtNotes.Text), New SqlParameter("@cb", SessionManager.UserID)))
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE Orders SET Status=@st,SubTotal=@sub,TaxAmount=@tax,DiscountPercent=@dp,DiscountAmount=@da,TotalAmount=@tot,PaymentMode=@pm,PaidAmount=@paid,ChangeAmount=@chg,Notes=@nt" & If(closeIt, ",ClosedDate=GETDATE()", "") & " WHERE OrderID=@id",
                        New SqlParameter("@st", If(closeIt, "Closed", "Open")), New SqlParameter("@sub", s1), New SqlParameter("@tax", tA), New SqlParameter("@dp", dp), New SqlParameter("@da", da), New SqlParameter("@tot", grand), New SqlParameter("@pm", cmbPayment.Text), New SqlParameter("@paid", paid), New SqlParameter("@chg", chg), New SqlParameter("@nt", txtNotes.Text), New SqlParameter("@id", currentOrderID))
                    DatabaseManager.ExecuteNonQuery("DELETE FROM OrderItems WHERE OrderID=@id", New SqlParameter("@id", currentOrderID))
                End If
                For Each r As DataRow In orderItems.Rows
                    DatabaseManager.ExecuteNonQuery("INSERT INTO OrderItems(OrderID,ItemID,ItemName,Quantity,UnitPrice,TaxPercent,TotalPrice) VALUES(@oid,@iid,@in,@qty,@up,@tp,@tot)",
                        New SqlParameter("@oid", currentOrderID), New SqlParameter("@iid", CInt(r("ItemID"))), New SqlParameter("@in", r("Item").ToString()), New SqlParameter("@qty", CInt(r("Qty"))), New SqlParameter("@up", CDec(r("Price"))), New SqlParameter("@tp", CDec(r("Tax%"))), New SqlParameter("@tot", CDec(r("Price")) * CInt(r("Qty"))))
                Next
                If tableID IsNot DBNull.Value Then DatabaseManager.ExecuteNonQuery("UPDATE RestaurantTables SET Status=@s WHERE TableID=@id", New SqlParameter("@s", If(closeIt, "Available", "Occupied")), New SqlParameter("@id", CInt(tableID)))
                If closeIt Then
                    MessageBox.Show($"Bill #{currentOrderID} closed!{vbCrLf}Total: Rs.{grand:N2}   Change: Rs.{chg:N2}", "Bill Closed", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    If MessageBox.Show("Export PDF?", "PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then ExportPDF()
                    ClearOrder()
                Else
                    MessageBox.Show($"Order #{currentOrderID} saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                LoadOpenOrders()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub LoadExistingOrder(orderID As Integer)
            Try
                Dim od As DataTable = DatabaseManager.GetDataTable("SELECT * FROM Orders WHERE OrderID=@id", New SqlParameter("@id", orderID))
                If od.Rows.Count = 0 Then Return
                Dim r As DataRow = od.Rows(0) : currentOrderID = orderID : txtCustomer.Text = r("CustomerName").ToString()
                Dim ti As Integer = cmbType.Items.IndexOf(r("OrderType").ToString()) : If ti >= 0 Then cmbType.SelectedIndex = ti
                txtDiscount.Text = r("DiscountPercent").ToString() : txtNotes.Text = r("Notes").ToString()
                orderItems.Rows.Clear()
                Dim items As DataTable = DatabaseManager.GetDataTable("SELECT ItemID,ItemName,UnitPrice,TaxPercent,Quantity,TotalPrice FROM OrderItems WHERE OrderID=@id", New SqlParameter("@id", orderID))
                For Each ir As DataRow In items.Rows : orderItems.Rows.Add(CInt(ir("ItemID")), ir("ItemName").ToString(), CDec(ir("UnitPrice")), CDec(ir("TaxPercent")), CInt(ir("Quantity")), CDec(ir("TotalPrice"))) : Next
                Recalc() : MessageBox.Show($"Order #{orderID} loaded.", "Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub PrintKOT()
            If orderItems.Rows.Count = 0 Then Return
            Dim pd As New Printing.PrintDocument()
            AddHandler pd.PrintPage, Sub(s, e)
                                         Dim g As Graphics = e.Graphics : Dim y As Integer = 20
                                         g.DrawString("KITCHEN ORDER TICKET", New System.Drawing.Font("Courier New", 13, FontStyle.Bold), Brushes.Black, 20, y) : y += 26
                                         g.DrawString("Order : " & If(currentOrderID = 0, "NEW", CStr(currentOrderID)), New System.Drawing.Font("Courier New", 10), Brushes.Black, 20, y) : y += 20
                                         g.DrawString("Table : " & cmbTable.Text, New System.Drawing.Font("Courier New", 10), Brushes.Black, 20, y) : y += 20
                                         g.DrawString("Type  : " & cmbType.Text, New System.Drawing.Font("Courier New", 10), Brushes.Black, 20, y) : y += 20
                                         g.DrawString("Time  : " & DateTime.Now.ToString("HH:mm  dd/MM/yyyy"), New System.Drawing.Font("Courier New", 10), Brushes.Black, 20, y) : y += 20
                                         g.DrawString(New String("-"c, 42), New System.Drawing.Font("Courier New", 10), Brushes.Black, 20, y) : y += 16
                                         For Each r As DataRow In orderItems.Rows
                                             g.DrawString("  " & r("Qty").ToString() & " x  " & r("Item").ToString(), New System.Drawing.Font("Courier New", 11, FontStyle.Bold), Brushes.Black, 20, y) : y += 22
                                         Next
                                         g.DrawString(New String("-"c, 42), New System.Drawing.Font("Courier New", 10), Brushes.Black, 20, y)
                                     End Sub
            Dim ppd As New System.Windows.Forms.PrintPreviewDialog() : ppd.Document = pd : ppd.ShowDialog()
        End Sub

        Private Sub ExportPDF()
            If orderItems.Rows.Count = 0 Then
                MessageBox.Show("No items.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim sfd As New SaveFileDialog() : sfd.Filter = "PDF|*.pdf"
            sfd.FileName = "Bill_" & If(currentOrderID > 0, CStr(currentOrderID), "Draft") & "_" & DateTime.Now.ToString("yyyyMMdd_HHmm")
            If sfd.ShowDialog() <> DialogResult.OK Then Return
            Try
                Dim doc As New iTextSharp.text.Document(PageSize.A5, 28, 28, 38, 28)
                PdfWriter.GetInstance(doc, New FileStream(sfd.FileName, FileMode.Create))
                doc.Open()
                Dim hF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 17, New BaseColor(183, 55, 35))
                Dim sF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA, 8, New BaseColor(110, 88, 68))
                Dim nF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA, 9)
                Dim wF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)
                Dim tF As iTextSharp.text.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, New BaseColor(183, 55, 35))
                Dim p1 As New Paragraph("Restaurant POS", hF)
                p1.Alignment = Element.ALIGN_CENTER
                doc.Add(p1)
                Dim p2 As New Paragraph("Tax Invoice", sF)
                p2.Alignment = Element.ALIGN_CENTER
                doc.Add(p2)
                doc.Add(New Paragraph(" "))
                Dim inf As New PdfPTable(2) : inf.WidthPercentage = 100 : inf.SpacingBefore = 4 : inf.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER
                Dim custInfo As String = "Customer: " & txtCustomer.Text & vbLf & "Type: " & cmbType.Text & vbLf & "Table: " & cmbTable.Text
                Dim lc As New PdfPCell(New Phrase(custInfo, nF))
                lc.Border = iTextSharp.text.Rectangle.NO_BORDER
                inf.AddCell(lc)
                Dim billNo As String = If(currentOrderID > 0, CStr(currentOrderID), "DRAFT")
                Dim billInfo As String = "Bill#: " & billNo & vbLf & "Date: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm") & vbLf & "Pay: " & cmbPayment.Text
                Dim rc As New PdfPCell(New Phrase(billInfo, nF))
                rc.Border = iTextSharp.text.Rectangle.NO_BORDER
                rc.HorizontalAlignment = Element.ALIGN_RIGHT
                inf.AddCell(rc)
                doc.Add(inf)
                Dim tbl As New PdfPTable(4) : tbl.WidthPercentage = 100 : tbl.SpacingBefore = 8 : tbl.SetWidths(New Single() {3.5F, 0.8F, 1.3F, 1.3F})
                For Each h As String In {"ITEM", "QTY", "PRICE", "TOTAL"}
                    Dim hc As New PdfPCell(New Phrase(h, wF)) : hc.BackgroundColor = New BaseColor(183, 55, 35) : hc.Padding = 5 : hc.HorizontalAlignment = Element.ALIGN_CENTER : tbl.AddCell(hc)
                Next
                Dim ri As Integer = 0
                For Each r As DataRow In orderItems.Rows
                    Dim bg As BaseColor = If(ri Mod 2 = 0, BaseColor.WHITE, New BaseColor(252, 248, 244))
                    Dim cls() As String = {r("Item").ToString(), r("Qty").ToString(), "Rs." & CDec(r("Price")).ToString("N2"), "Rs." & CDec(r("Total")).ToString("N2")}
                    Dim als() As Integer = {Element.ALIGN_LEFT, Element.ALIGN_CENTER, Element.ALIGN_RIGHT, Element.ALIGN_RIGHT}
                    For ci As Integer = 0 To 3
                        Dim cell As New PdfPCell(New Phrase(cls(ci), nF))
                        cell.BackgroundColor = bg
                        cell.Padding = 4
                        cell.HorizontalAlignment = als(ci)
                        tbl.AddCell(cell)
                    Next
                    ri += 1
                Next
                doc.Add(tbl)
                Dim s1 As Decimal = 0 : Dim tA As Decimal = 0
                For Each r As DataRow In orderItems.Rows
                    Dim lt As Decimal = CDec(r("Price")) * CInt(r("Qty"))
                    s1 += lt
                    tA += lt * CDec(r("Tax%")) / 100
                Next
                Dim dp As Decimal = 0 : Decimal.TryParse(txtDiscount.Text, dp) : Dim da As Decimal = s1 * dp / 100 : Dim grand As Decimal = s1 + tA - da
                Dim tot2 As New PdfPTable(2) : tot2.WidthPercentage = 46 : tot2.HorizontalAlignment = Element.ALIGN_RIGHT : tot2.SpacingBefore = 6 : tot2.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER
                Dim ar As Action(Of String, String, iTextSharp.text.Font) = Sub(l2, v2, f2)
                                                                                Dim lc2 As New PdfPCell(New Phrase(l2, f2)) With {.Border = iTextSharp.text.Rectangle.NO_BORDER, .Padding = 2}
                                                                                Dim vc2 As New PdfPCell(New Phrase(v2, f2)) With {.Border = iTextSharp.text.Rectangle.NO_BORDER, .Padding = 2, .HorizontalAlignment = Element.ALIGN_RIGHT}
                                                                                tot2.AddCell(lc2) : tot2.AddCell(vc2)
                                                                            End Sub
                ar("Sub Total:", "Rs." & s1.ToString("N2"), nF) : ar("Tax:", "Rs." & tA.ToString("N2"), nF)
                If da > 0 Then ar("Discount:", "- Rs." & da.ToString("N2"), nF)
                Dim gtL As New PdfPCell(New Phrase("GRAND TOTAL:", tF)) With {.Border = iTextSharp.text.Rectangle.TOP_BORDER, .Padding = 5, .BorderColor = New BaseColor(183, 55, 35)}
                Dim gtV As New PdfPCell(New Phrase("Rs." & grand.ToString("N2"), tF)) With {.Border = iTextSharp.text.Rectangle.TOP_BORDER, .Padding = 5, .HorizontalAlignment = Element.ALIGN_RIGHT, .BorderColor = New BaseColor(183, 55, 35)}
                tot2.AddCell(gtL) : tot2.AddCell(gtV) : doc.Add(tot2)
                Dim pThanks As New Paragraph(vbCrLf & "Thank you for dining with us!", sF)
                pThanks.Alignment = Element.ALIGN_CENTER
                doc.Add(pThanks)
                doc.Close()
                MessageBox.Show("PDF exported!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Dim psi As New ProcessStartInfo(sfd.FileName)
                psi.UseShellExecute = True
                Process.Start(psi)
            Catch ex As Exception : MessageBox.Show("PDF error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub ClearOrder()
            orderItems.Rows.Clear() : txtCustomer.Text = "Walk-in Guest" : txtPhone.Text = "" : txtNotes.Text = ""
            txtDiscount.Text = "0" : txtPaid.Text = "0" : cmbType.SelectedIndex = 0 : cmbTable.SelectedIndex = 0 : cmbPayment.SelectedIndex = 0 : currentOrderID = 0
            lblSub.Text = "Rs. 0.00" : lblTax.Text = "Rs. 0.00" : lblDisc.Text = "Rs. 0.00" : lblTotal.Text = "Rs. 0.00" : lblChange.Text = "Rs. 0.00"
            LoadOpenOrders()
        End Sub
    End Class

End Namespace


