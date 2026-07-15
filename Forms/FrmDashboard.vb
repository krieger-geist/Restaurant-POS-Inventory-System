Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient

Namespace RestaurantPOS
    Public Class FrmDashboard
        Inherits Form
        Private pnlContent As Panel
        Private lblPage As Label, lblClock As Label
        Private tmr As Timer
        Private activeBtn As Button
        Private btnDash, btnBilling, btnTables, btnOrders, btnReservations As Button
        Private btnMenu, btnCustomers, btnStaff, btnInventory, btnExpenses As Button
        Private btnReports, btnUsers, btnSettings As Button

        Public Sub New()
            Me.Text = "Restaurant POS"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.WindowState = FormWindowState.Maximized
            Me.MinimumSize = New Size(1100, 700)
            Me.BackColor = AppTheme.ContentBg
            Build() : LoadHome() : StartClock()
        End Sub

        Private Sub Build()
            Dim root As New TableLayoutPanel()
            root.Dock = DockStyle.Fill : root.ColumnCount = 2 : root.RowCount = 1
            root.Padding = New Padding(0) : root.Margin = New Padding(0)
            root.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190))
            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Me.Controls.Add(root)

            Dim sidebar As New Panel()
            sidebar.Dock = DockStyle.Fill : sidebar.BackColor = AppTheme.SidebarBg : sidebar.Margin = New Padding(0)
            root.Controls.Add(sidebar, 0, 0)

            Dim pLogo As New Panel()
            pLogo.Dock = DockStyle.Top : pLogo.Height = 62 : pLogo.BackColor = Color.FromArgb(110, 22, 8)
            sidebar.Controls.Add(pLogo)
            Dim lBrand As New Label()
            lBrand.Text = "Restaurant POS" : lBrand.Font = New System.Drawing.Font("Segoe UI", 11, FontStyle.Bold)
            lBrand.ForeColor = Color.White : lBrand.Dock = DockStyle.Fill : lBrand.TextAlign = ContentAlignment.MiddleCenter
            pLogo.Controls.Add(lBrand)

            Dim btnOut As New Button()
            btnOut.Text = "   Sign Out" : btnOut.Height = 40 : btnOut.Dock = DockStyle.Bottom
            btnOut.FlatStyle = FlatStyle.Flat : btnOut.FlatAppearance.BorderSize = 0
            btnOut.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 15, 5)
            btnOut.BackColor = Color.FromArgb(70, 8, 3) : btnOut.ForeColor = Color.FromArgb(255, 140, 115)
            btnOut.Font = AppTheme.F10 : btnOut.TextAlign = ContentAlignment.MiddleLeft
            btnOut.Padding = New Padding(16, 0, 0, 0) : btnOut.Cursor = Cursors.Hand
            AddHandler btnOut.Click, AddressOf DoLogout
            sidebar.Controls.Add(btnOut)

            Dim pUser As New Panel()
            pUser.Dock = DockStyle.Bottom : pUser.Height = 56 : pUser.BackColor = Color.FromArgb(36, 16, 8)
            sidebar.Controls.Add(pUser)
            Dim lAv As New Label()
            lAv.Text = If(SessionManager.FullName.Length > 0, SessionManager.FullName(0).ToString().ToUpper(), "U")
            lAv.Font = New System.Drawing.Font("Segoe UI", 11, FontStyle.Bold) : lAv.ForeColor = Color.White
            lAv.BackColor = AppTheme.Primary : lAv.Size = New Size(34, 34) : lAv.Location = New Point(10, 11)
            lAv.TextAlign = ContentAlignment.MiddleCenter : pUser.Controls.Add(lAv)
            Dim lUN As New Label()
            lUN.Text = SessionManager.FullName : lUN.Font = AppTheme.F9B : lUN.ForeColor = Color.White
            lUN.AutoSize = True : lUN.Location = New Point(52, 10) : pUser.Controls.Add(lUN)
            Dim lRl As New Label()
            lRl.Text = SessionManager.Role : lRl.Font = AppTheme.F9 : lRl.ForeColor = Color.FromArgb(255, 175, 75)
            lRl.AutoSize = True : lRl.Location = New Point(52, 29) : pUser.Controls.Add(lRl)

            Dim pNav As New Panel()
            pNav.Dock = DockStyle.Fill : pNav.BackColor = Color.Transparent
            sidebar.Controls.Add(pNav)

            Dim ny As Integer = 8
            AddSec(pNav, "OPERATIONS", ny) : ny += 22
            btnDash = NBtn(pNav, "Dashboard", ny) : ny += 37
            btnBilling = NBtn(pNav, "Billing & POS", ny) : ny += 37
            btnTables = NBtn(pNav, "Table Manager", ny) : ny += 37
            btnOrders = NBtn(pNav, "Orders", ny) : ny += 37
            btnReservations = NBtn(pNav, "Reservations", ny) : ny += 37
            ny += 4
            AddSec(pNav, "MANAGEMENT", ny) : ny += 22
            btnMenu = NBtn(pNav, "Menu Manager", ny) : ny += 37
            btnCustomers = NBtn(pNav, "Customers", ny) : ny += 37
            btnStaff = NBtn(pNav, "Staff", ny) : ny += 37
            btnInventory = NBtn(pNav, "Inventory", ny) : ny += 37
            btnExpenses = NBtn(pNav, "Expenses", ny) : ny += 37
            ny += 4
            AddSec(pNav, "ADMIN", ny) : ny += 22
            btnReports = NBtn(pNav, "Reports", ny) : ny += 37
            btnUsers = NBtn(pNav, "User Accounts", ny) : ny += 37
            btnSettings = NBtn(pNav, "Settings", ny)

            Dim rRight As New TableLayoutPanel()
            rRight.Dock = DockStyle.Fill : rRight.ColumnCount = 1 : rRight.RowCount = 2
            rRight.Padding = New Padding(0) : rRight.Margin = New Padding(0)
            rRight.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 52))
            rRight.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.Controls.Add(rRight, 1, 0)

            Dim pTop As New Panel()
            pTop.Dock = DockStyle.Fill : pTop.BackColor = Color.White : pTop.Margin = New Padding(0)
            AddHandler pTop.Paint, Sub(s, e) e.Graphics.DrawLine(New Pen(AppTheme.BorderColor), 0, DirectCast(s, Panel).Height - 1, DirectCast(s, Panel).Width, DirectCast(s, Panel).Height - 1)
            rRight.Controls.Add(pTop, 0, 0)

            lblPage = New Label() : lblPage.Text = "Dashboard"
            lblPage.Font = New System.Drawing.Font("Segoe UI", 14, FontStyle.Bold)
            lblPage.ForeColor = AppTheme.TextDark : lblPage.AutoSize = True : lblPage.Location = New Point(16, 14)
            pTop.Controls.Add(lblPage)

            lblClock = New Label() : lblClock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy   HH:mm:ss")
            lblClock.Font = AppTheme.F9 : lblClock.ForeColor = AppTheme.TextLight
            lblClock.AutoSize = True : lblClock.Location = New Point(330, 18)
            pTop.Controls.Add(lblClock)

            pnlContent = New Panel()
            pnlContent.Dock = DockStyle.Fill : pnlContent.BackColor = AppTheme.ContentBg
            pnlContent.Padding = New Padding(14, 12, 14, 12) : pnlContent.Margin = New Padding(0)
            rRight.Controls.Add(pnlContent, 0, 1)

            SetActive(btnDash)
        End Sub

        Private Sub AddSec(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = New System.Drawing.Font("Segoe UI", 7, FontStyle.Bold)
            l.ForeColor = Color.FromArgb(80, 105, 130) : l.AutoSize = True : l.Location = New Point(14, y) : p.Controls.Add(l)
        End Sub

        Private Function NBtn(p As Panel, t As String, y As Integer) As Button
            Dim b As New Button() : b.Text = "    " & t : b.Size = New Size(190, 35) : b.Location = New Point(0, y)
            b.FlatStyle = FlatStyle.Flat : b.FlatAppearance.BorderSize = 0
            b.BackColor = Color.Transparent : b.ForeColor = AppTheme.SidebarText
            b.Font = AppTheme.F10 : b.TextAlign = ContentAlignment.MiddleLeft : b.Cursor = Cursors.Hand
            AddHandler b.Click, AddressOf NavClick
            AddHandler b.MouseEnter, Sub(s, ev)
                                         Dim btn As Button = DirectCast(s, Button)
                                         If btn IsNot activeBtn Then btn.BackColor = Color.FromArgb(55, 28, 14)
                                     End Sub
            AddHandler b.MouseLeave, Sub(s, ev)
                                         Dim btn As Button = DirectCast(s, Button)
                                         If btn IsNot activeBtn Then btn.BackColor = Color.Transparent
                                     End Sub
            p.Controls.Add(b) : Return b
        End Function

        Public Sub SetActive(btn As Button)
            If activeBtn IsNot Nothing Then
                activeBtn.BackColor = Color.Transparent : activeBtn.ForeColor = AppTheme.SidebarText : activeBtn.Font = AppTheme.F10
            End If
            btn.BackColor = AppTheme.Primary : btn.ForeColor = Color.White : btn.Font = AppTheme.F10B : activeBtn = btn
        End Sub

        Private Sub NavClick(sender As Object, e As EventArgs)
            Dim btn As Button = DirectCast(sender, Button)
            SetActive(btn) : pnlContent.Controls.Clear()
            If btn Is btnDash Then
                lblPage.Text = "Dashboard" : LoadHome()
            ElseIf btn Is btnBilling Then
                lblPage.Text = "Billing & POS" : LoadMod(New BillingModule())
            ElseIf btn Is btnTables Then
                lblPage.Text = "Table Manager" : LoadMod(New TableModule())
            ElseIf btn Is btnOrders Then
                lblPage.Text = "Order History" : LoadMod(New OrdersModule())
            ElseIf btn Is btnReservations Then
                lblPage.Text = "Reservations" : LoadMod(New ReservationModule())
            ElseIf btn Is btnMenu Then
                lblPage.Text = "Menu Manager" : LoadMod(New MenuModule())
            ElseIf btn Is btnCustomers Then
                lblPage.Text = "Customers" : LoadMod(New CustomerModule())
            ElseIf btn Is btnStaff Then
                lblPage.Text = "Staff" : LoadMod(New StaffModule())
            ElseIf btn Is btnInventory Then
                lblPage.Text = "Inventory" : LoadMod(New InventoryModule())
            ElseIf btn Is btnExpenses Then
                lblPage.Text = "Expenses" : LoadMod(New ExpensesModule())
            ElseIf btn Is btnReports Then
                lblPage.Text = "Reports" : LoadMod(New ReportsModule())
            ElseIf btn Is btnUsers Then
                If Not SessionManager.IsAdmin() Then
                    MessageBox.Show("Admin access required.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                lblPage.Text = "User Accounts" : LoadMod(New UserModule())
            ElseIf btn Is btnSettings Then
                lblPage.Text = "Settings" : LoadMod(New SettingsModule())
            End If
        End Sub

        Public Sub LoadMod(pnl As Panel)
            pnl.Dock = DockStyle.Fill : pnlContent.Controls.Add(pnl)
        End Sub

        Private Sub LoadHome()
            Dim home As New Panel() : home.Dock = DockStyle.Fill : home.BackColor = Color.Transparent

            Dim todayRev = "Rs.0" : Dim openOrd = "0" : Dim freeTbl = "0" : Dim menuCnt = "0" : Dim resCnt = "0" : Dim lowStk = "0"
            Try
                Dim v As Object
                v = DatabaseManager.ExecuteScalar("SELECT ISNULL(SUM(TotalAmount),0) FROM Orders WHERE Status='Closed' AND CAST(ClosedDate AS DATE)=CAST(GETDATE() AS DATE)")
                todayRev = "Rs." & String.Format("{0:N0}", v)
                v = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Orders WHERE Status='Open'") : openOrd = v.ToString()
                v = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM RestaurantTables WHERE Status='Available'") : freeTbl = v.ToString()
                v = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM MenuItems WHERE IsAvailable=1") : menuCnt = v.ToString()
                v = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Reservations WHERE Status='Confirmed' AND CAST(ReservationDate AS DATE)>=CAST(GETDATE() AS DATE)") : resCnt = v.ToString()
                v = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Inventory WHERE CurrentStock<=MinStock") : lowStk = v.ToString()
            Catch
            End Try

            Dim titles() As String = {"Today Revenue", "Open Orders", "Free Tables", "Menu Items", "Reservations", "Low Stock"}
            Dim vals() As String = {todayRev, openOrd, freeTbl, menuCnt, resCnt, lowStk}
            Dim colors() As Color = {AppTheme.Green, AppTheme.Orange, AppTheme.Blue, AppTheme.Purple, AppTheme.Primary, AppTheme.Red}

            ' Root layout: TableLayoutPanel with 2 rows - cards (fixed 94px) | body (fill)
            ' Avoids WinForms Dock z-order overlap ambiguity entirely.
            Dim rootTbl As New TableLayoutPanel()
            rootTbl.Dock = DockStyle.Fill : rootTbl.ColumnCount = 1 : rootTbl.RowCount = 2
            rootTbl.Padding = New Padding(0) : rootTbl.Margin = New Padding(0)
            rootTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rootTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rootTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 94))
            rootTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            home.Controls.Add(rootTbl)

            Dim cardFlow As New FlowLayoutPanel()
            cardFlow.Dock = DockStyle.Fill
            cardFlow.FlowDirection = FlowDirection.LeftToRight : cardFlow.WrapContents = False
            cardFlow.BackColor = Color.Transparent : cardFlow.Margin = New Padding(0, 0, 0, 10)
            rootTbl.Controls.Add(cardFlow, 0, 0)

            For i As Integer = 0 To 5
                Dim card As New Panel() : card.Size = New Size(168, 86) : card.BackColor = Color.White : card.Margin = New Padding(0, 0, 10, 0)
                Dim topBar As New Panel() : topBar.Dock = DockStyle.Top : topBar.Height = 4 : topBar.BackColor = colors(i) : card.Controls.Add(topBar)
                Dim lv As New Label() : lv.Text = vals(i) : lv.Font = New System.Drawing.Font("Segoe UI", 12, FontStyle.Bold)
                lv.ForeColor = AppTheme.TextDark : lv.Location = New Point(10, 10) : lv.Size = New Size(146, 28) : lv.AutoEllipsis = True : card.Controls.Add(lv)
                Dim lt As New Label() : lt.Text = titles(i) : lt.Font = AppTheme.F8B : lt.ForeColor = AppTheme.TextLight
                lt.Location = New Point(10, 54) : lt.Size = New Size(146, 20) : card.Controls.Add(lt)
                cardFlow.Controls.Add(card)
            Next

            ' Body row: split left (recent orders) | right (quick actions)
            Dim splitRow As New TableLayoutPanel()
            splitRow.Dock = DockStyle.Fill : splitRow.ColumnCount = 2 : splitRow.RowCount = 1
            splitRow.Padding = New Padding(0) : splitRow.Margin = New Padding(0)
            splitRow.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            splitRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 62))
            splitRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38))
            splitRow.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            rootTbl.Controls.Add(splitRow, 0, 1)

            ' LEFT: header (fixed) + grid (fill) via nested TableLayoutPanel
            Dim leftTbl As New TableLayoutPanel()
            leftTbl.Dock = DockStyle.Fill : leftTbl.ColumnCount = 1 : leftTbl.RowCount = 2
            leftTbl.Margin = New Padding(0, 0, 6, 0) : leftTbl.Padding = New Padding(0)
            leftTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            leftTbl.BackColor = Color.White
            leftTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            leftTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
            leftTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            splitRow.Controls.Add(leftTbl, 0, 0)

            Dim lRec As New Label() : lRec.Text = "Recent Orders" : lRec.Font = AppTheme.F11B : lRec.ForeColor = AppTheme.TextDark
            lRec.Dock = DockStyle.Fill : lRec.Padding = New Padding(12, 10, 0, 0) : lRec.BackColor = Color.White
            leftTbl.Controls.Add(lRec, 0, 0)

            Dim dgv As New DataGridView() : dgv.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgv)
            Try
                dgv.DataSource = DatabaseManager.GetDataTable(
                    "SELECT TOP 15 OrderID, CustomerName, OrderType, Status, TotalAmount, PaymentMode, CONVERT(VARCHAR,OrderDate,120) AS [DateTime] FROM Orders ORDER BY OrderID DESC")
                If dgv.Columns.Contains("OrderID") Then dgv.Columns("OrderID").Visible = False
            Catch
            End Try
            leftTbl.Controls.Add(dgv, 0, 1)

            ' RIGHT: header (fixed) + quick action buttons (fill) via nested TableLayoutPanel
            Dim rightTbl As New TableLayoutPanel()
            rightTbl.Dock = DockStyle.Fill : rightTbl.ColumnCount = 1 : rightTbl.RowCount = 2
            rightTbl.Margin = New Padding(6, 0, 0, 0) : rightTbl.Padding = New Padding(0)
            rightTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rightTbl.BackColor = Color.White
            rightTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rightTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
            rightTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            splitRow.Controls.Add(rightTbl, 1, 0)

            Dim lQ As New Label() : lQ.Text = "Quick Actions" : lQ.Font = AppTheme.F11B : lQ.ForeColor = AppTheme.TextDark
            lQ.Dock = DockStyle.Fill : lQ.Padding = New Padding(12, 10, 0, 0) : lQ.BackColor = Color.White
            rightTbl.Controls.Add(lQ, 0, 0)

            Dim pBtns As New Panel() : pBtns.Dock = DockStyle.Fill : pBtns.BackColor = Color.White : pBtns.Padding = New Padding(12, 8, 12, 12)
            rightTbl.Controls.Add(pBtns, 0, 1)

            Dim qlabels() As String = {"New Bill / Order", "View Table Status", "New Reservation", "Today Reports", "Manage Menu", "Check Inventory"}
            Dim qstyles() As String = {"primary", "info", "success", "warning", "secondary", "danger"}
            Dim qy As Integer = 0
            For qi As Integer = 0 To 5
                Dim qb As New Button()
                qb.Text = "  " & qlabels(qi)
                qb.Location = New Point(0, qy)
                qb.Size = New Size(260, 38)
                qb.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
                AppTheme.StyleBtn(qb, qstyles(qi))
                qb.TextAlign = ContentAlignment.MiddleLeft
                Dim ql As String = qlabels(qi)
                AddHandler qb.Click, Sub(s, e) QA(ql)
                pBtns.Controls.Add(qb)
                qy += 46
            Next

            pnlContent.Controls.Clear() : pnlContent.Controls.Add(home)
        End Sub

        Private Sub QA(t As String)
            pnlContent.Controls.Clear()
            If t.Contains("Bill") Then : SetActive(btnBilling) : lblPage.Text = "Billing & POS" : LoadMod(New BillingModule())
            ElseIf t.Contains("Table") Then : SetActive(btnTables) : lblPage.Text = "Table Manager" : LoadMod(New TableModule())
            ElseIf t.Contains("Reservation") Then : SetActive(btnReservations) : lblPage.Text = "Reservations" : LoadMod(New ReservationModule())
            ElseIf t.Contains("Report") Then : SetActive(btnReports) : lblPage.Text = "Reports" : LoadMod(New ReportsModule())
            ElseIf t.Contains("Menu") Then : SetActive(btnMenu) : lblPage.Text = "Menu Manager" : LoadMod(New MenuModule())
            ElseIf t.Contains("Inventory") Then : SetActive(btnInventory) : lblPage.Text = "Inventory" : LoadMod(New InventoryModule())
            End If
        End Sub

        Private Sub StartClock()
            tmr = New Timer() With {.Interval = 1000}
            AddHandler tmr.Tick, Sub(s, e) lblClock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy   HH:mm:ss")
            tmr.Start()
        End Sub

        Private Sub DoLogout(sender As Object, e As EventArgs)
            If MessageBox.Show("Sign out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                DatabaseManager.Log(SessionManager.UserID, SessionManager.Username, "LOGOUT", "Signed out")
                SessionManager.ClearSession()
                If tmr IsNot Nothing Then tmr.Stop()
                Me.Close()
                Dim login As New FrmLogin()
                If login.ShowDialog() = DialogResult.OK Then Dim dash As New FrmDashboard() : dash.Show()
            End If
        End Sub

        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
            If tmr IsNot Nothing Then tmr.Stop() : MyBase.OnFormClosed(e)
        End Sub
    End Class
End Namespace