Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient

Namespace RestaurantPOS

    '═══════════════════════════════════════════════
    ' RESERVATION MODULE
    '═══════════════════════════════════════════════
    Public Class ReservationModule
        Inherits Panel
        Private dgvRes As DataGridView
        Private lblFT As Label
        Private txtName, txtPhone, txtNotes As TextBox
        Private cmbTable, cmbStatus As ComboBox
        Private nudGuests As NumericUpDown
        Private dtpDate As DateTimePicker
        Private dtpTime As DateTimePicker
        Private currentID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : Load(False)
        End Sub

        Private Sub Build()
            Dim pBar As Panel = AppTheme.MakeToolbar()
            Me.Controls.Add(pBar)
            Dim btnNew As New Button() : btnNew.Text = "New Reservation" : btnNew.Size = New Size(148, 32) : btnNew.Location = New Point(10, 10)
            AppTheme.StyleBtn(btnNew, "success") : AddHandler btnNew.Click, AddressOf NewRes : pBar.Controls.Add(btnNew)
            Dim btnToday As New Button() : btnToday.Text = "Today Only" : btnToday.Size = New Size(110, 32) : btnToday.Location = New Point(166, 10)
            AppTheme.StyleBtn(btnToday, "info")
            AddHandler btnToday.Click, Sub(s, e) Load(True)
            pBar.Controls.Add(btnToday)
            Dim btnAll As New Button() : btnAll.Text = "All" : btnAll.Size = New Size(70, 32) : btnAll.Location = New Point(284, 10)
            AppTheme.StyleBtn(btnAll, "secondary")
            AddHandler btnAll.Click, Sub(s, e) Load(False)
            pBar.Controls.Add(btnAll)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 350)
            Me.Controls.Add(split)

            dgvRes = New DataGridView() : dgvRes.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvRes) : dgvRes.Margin = New Padding(0, 0, 4, 0)
            AddHandler dgvRes.SelectionChanged, Sub(s, e)
                                                    If dgvRes.CurrentRow IsNot Nothing AndAlso dgvRes.CurrentRow.Index >= 0 Then
                                                        Try : LoadForm(CInt(dgvRes.CurrentRow.Cells("ReservationID").Value)) : Catch : End Try
                                                    End If
                                                End Sub
            split.Controls.Add(dgvRes, 0, 0)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(4, 0, 0, 0)
            split.Controls.Add(pForm, 1, 0)
            pForm.Controls.Add(AppTheme.AccentBar())
            lblFT = AppTheme.FormTitle("Reservation Details") : pForm.Controls.Add(lblFT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 6, 14, 10) : pF.BackColor = Color.White : pForm.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 50
            AddL(pF, "Customer Name *", y) : txtName = NTB(pF, y + 18) : y += sp
            AddL(pF, "Phone Number", y) : txtPhone = NTB(pF, y + 18) : y += sp
            AddL(pF, "Table", y)
            cmbTable = New ComboBox() : cmbTable.Location = New Point(0, y + 18) : cmbTable.Size = New Size(318, 26) : cmbTable.Font = AppTheme.F10 : cmbTable.DropDownStyle = ComboBoxStyle.DropDownList
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT TableNumber FROM RestaurantTables ORDER BY TableNumber")
                For Each r As DataRow In dt.Rows : cmbTable.Items.Add(r("TableNumber").ToString()) : Next
                If cmbTable.Items.Count > 0 Then cmbTable.SelectedIndex = 0
            Catch
            End Try
            pF.Controls.Add(cmbTable) : y += sp
            AddL(pF, "Number of Guests", y)
            nudGuests = New NumericUpDown() : nudGuests.Location = New Point(0, y + 18) : nudGuests.Size = New Size(318, 26) : nudGuests.Font = AppTheme.F10 : nudGuests.Minimum = 1 : nudGuests.Maximum = 50 : nudGuests.Value = 2 : pF.Controls.Add(nudGuests) : y += sp

            ' Date and Time on same row
            Dim lD As New Label() : lD.Text = "Date" : lD.Font = AppTheme.F8B : lD.ForeColor = AppTheme.TextMed : lD.Location = New Point(0, y) : lD.AutoSize = True : pF.Controls.Add(lD)
            Dim lT As New Label() : lT.Text = "Time" : lT.Font = AppTheme.F8B : lT.ForeColor = AppTheme.TextMed : lT.Location = New Point(180, y) : lT.AutoSize = True : pF.Controls.Add(lT)
            dtpDate = New DateTimePicker() : dtpDate.Location = New Point(0, y + 18) : dtpDate.Size = New Size(170, 26) : dtpDate.Font = AppTheme.F10 : dtpDate.Format = DateTimePickerFormat.Short : dtpDate.Value = DateTime.Today : pF.Controls.Add(dtpDate)
            dtpTime = New DateTimePicker() : dtpTime.Location = New Point(180, y + 18) : dtpTime.Size = New Size(138, 26) : dtpTime.Font = AppTheme.F10 : dtpTime.Format = DateTimePickerFormat.Time : dtpTime.ShowUpDown = True : dtpTime.Value = DateTime.Today.AddHours(19) : pF.Controls.Add(dtpTime) : y += sp

            AddL(pF, "Status", y) : cmbStatus = New ComboBox() : cmbStatus.Location = New Point(0, y + 18) : cmbStatus.Size = New Size(318, 26) : cmbStatus.Font = AppTheme.F10 : cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList
            cmbStatus.Items.AddRange({"Confirmed", "Pending", "Completed", "Cancelled"}) : cmbStatus.SelectedIndex = 0 : pF.Controls.Add(cmbStatus) : y += sp
            AddL(pF, "Special Notes", y) : txtNotes = NTB(pF, y + 18) : y += sp

            Dim btnSaveRes As New Button() : btnSaveRes.Text = "Save" : btnSaveRes.Size = New Size(94, 34) : btnSaveRes.Location = New Point(0, y) : AppTheme.StyleBtn(btnSaveRes, "success") : AddHandler btnSaveRes.Click, AddressOf BtnSave : pF.Controls.Add(btnSaveRes)
            Dim btnDel As New Button() : btnDel.Text = "Delete" : btnDel.Size = New Size(94, 34) : btnDel.Location = New Point(102, y) : AppTheme.StyleBtn(btnDel, "danger") : AddHandler btnDel.Click, AddressOf BtnDelete : pF.Controls.Add(btnDel)
            Dim btnN As New Button() : btnN.Text = "New" : btnN.Size = New Size(94, 34) : btnN.Location = New Point(204, y) : AppTheme.StyleBtn(btnN, "secondary") : AddHandler btnN.Click, AddressOf NewRes : pF.Controls.Add(btnN)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(318, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function

        Private Sub Load(todayOnly As Boolean)
            Try
                Dim sql As String = "SELECT ReservationID,CustomerName,Phone,TableNumber,Guests,ReservationDate,Status FROM Reservations"
                If todayOnly Then sql &= " WHERE CAST(ReservationDate AS DATE)=CAST(GETDATE() AS DATE)"
                sql &= " ORDER BY ReservationDate DESC"
                dgvRes.DataSource = DatabaseManager.GetDataTable(sql)
                If dgvRes.Columns.Contains("ReservationID") Then dgvRes.Columns("ReservationID").Visible = False
                For Each row As DataGridViewRow In dgvRes.Rows
                    Dim st As String = If(row.Cells("Status").Value IsNot Nothing, row.Cells("Status").Value.ToString(), "")
                    Select Case st
                        Case "Confirmed" : row.DefaultCellStyle.BackColor = Color.FromArgb(235, 255, 240)
                        Case "Pending" : row.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 225)
                        Case "Cancelled" : row.DefaultCellStyle.BackColor = Color.FromArgb(255, 238, 238)
                    End Select
                Next
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadForm(id As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT * FROM Reservations WHERE ReservationID=@id", New SqlParameter("@id", id))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentID = id : lblFT.Text = "Edit: " & row("CustomerName").ToString()
                txtName.Text = row("CustomerName").ToString() : txtPhone.Text = row("Phone").ToString() : txtNotes.Text = row("Notes").ToString()
                Dim ti As Integer = cmbTable.Items.IndexOf(row("TableNumber").ToString()) : If ti >= 0 Then cmbTable.SelectedIndex = ti
                nudGuests.Value = Math.Min(Math.Max(CInt(row("Guests")), 1), 50)
                Dim rd As DateTime = CDate(row("ReservationDate")) : dtpDate.Value = rd.Date : dtpTime.Value = rd
                Dim si As Integer = cmbStatus.Items.IndexOf(row("Status").ToString()) : If si >= 0 Then cmbStatus.SelectedIndex = si
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub NewRes(sender As Object, e As EventArgs)
            currentID = 0 : lblFT.Text = "New Reservation" : txtName.Text = "" : txtPhone.Text = "" : txtNotes.Text = ""
            nudGuests.Value = 2 : dtpDate.Value = DateTime.Today : dtpTime.Value = DateTime.Today.AddHours(19) : cmbStatus.SelectedIndex = 0 : txtName.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtName.Text.Trim()) Then
                MessageBox.Show("Customer name required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim resDate As DateTime = dtpDate.Value.Date.Add(dtpTime.Value.TimeOfDay)
            Try
                If currentID = 0 Then
                    DatabaseManager.ExecuteNonQuery("INSERT INTO Reservations(CustomerName,Phone,TableNumber,Guests,ReservationDate,Status,Notes,CreatedBy) VALUES(@cn,@ph,@tn,@g,@rd,@st,@nt,@cb)",
                        New SqlParameter("@cn", txtName.Text.Trim()), New SqlParameter("@ph", txtPhone.Text.Trim()), New SqlParameter("@tn", cmbTable.Text), New SqlParameter("@g", CInt(nudGuests.Value)), New SqlParameter("@rd", resDate), New SqlParameter("@st", cmbStatus.Text), New SqlParameter("@nt", txtNotes.Text), New SqlParameter("@cb", SessionManager.UserID))
                    MessageBox.Show("Reservation added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE Reservations SET CustomerName=@cn,Phone=@ph,TableNumber=@tn,Guests=@g,ReservationDate=@rd,Status=@st,Notes=@nt WHERE ReservationID=@id",
                        New SqlParameter("@cn", txtName.Text.Trim()), New SqlParameter("@ph", txtPhone.Text.Trim()), New SqlParameter("@tn", cmbTable.Text), New SqlParameter("@g", CInt(nudGuests.Value)), New SqlParameter("@rd", resDate), New SqlParameter("@st", cmbStatus.Text), New SqlParameter("@nt", txtNotes.Text), New SqlParameter("@id", currentID))
                    MessageBox.Show("Reservation updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                Load(False)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnDelete(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            If MessageBox.Show("Delete reservation?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("DELETE FROM Reservations WHERE ReservationID=@id", New SqlParameter("@id", currentID))
                Load(False) : NewRes(Nothing, Nothing)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class

    '═══════════════════════════════════════════════
    ' CUSTOMER MODULE
    '═══════════════════════════════════════════════
    Public Class CustomerModule
        Inherits Panel
        Private dgvCust, dgvHistory As DataGridView
        Private lblFT, lblSpent, lblVisits As Label
        Private txtName, txtPhone, txtEmail, txtAddr, txtSearch As TextBox
        Private currentID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadCust()
        End Sub

        Private Sub Build()
            Dim pBar As Panel = AppTheme.MakeToolbar()
            Me.Controls.Add(pBar)
            txtSearch = New TextBox() : txtSearch.Location = New Point(10, 12) : txtSearch.Size = New Size(200, 28) : txtSearch.Font = AppTheme.F10 : txtSearch.BorderStyle = BorderStyle.FixedSingle : txtSearch.PlaceholderText = "Search customers..." : txtSearch.BackColor = AppTheme.InputBg
            AddHandler txtSearch.TextChanged, Sub(s, e) LoadCust(txtSearch.Text)
            pBar.Controls.Add(txtSearch)
            Dim btnNew As New Button() : btnNew.Text = "New Customer" : btnNew.Size = New Size(130, 32) : btnNew.Location = New Point(220, 10) : AppTheme.StyleBtn(btnNew, "success") : AddHandler btnNew.Click, AddressOf NewCust : pBar.Controls.Add(btnNew)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 368)
            Me.Controls.Add(split)

            ' Left: customers grid
            dgvCust = New DataGridView() : dgvCust.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvCust) : dgvCust.Margin = New Padding(0, 0, 4, 0)
            AddHandler dgvCust.SelectionChanged, Sub(s, e)
                                                     If dgvCust.CurrentRow IsNot Nothing AndAlso dgvCust.CurrentRow.Index >= 0 Then
                                                         Try : LoadForm(CInt(dgvCust.CurrentRow.Cells("CustomerID").Value)) : Catch : End Try
                                                     End If
                                                 End Sub
            split.Controls.Add(dgvCust, 0, 0)

            ' Right: form + history
            Dim rightTbl As New TableLayoutPanel()
            rightTbl.Dock = DockStyle.Fill : rightTbl.ColumnCount = 1 : rightTbl.RowCount = 2
            rightTbl.Padding = New Padding(0) : rightTbl.Margin = New Padding(4, 0, 0, 0)
            rightTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rightTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rightTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 54))
            rightTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 46))
            split.Controls.Add(rightTbl, 1, 0)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(0, 0, 0, 4)
            rightTbl.Controls.Add(pForm, 0, 0)

            Dim cfTbl As New TableLayoutPanel()
            cfTbl.Dock = DockStyle.Fill : cfTbl.ColumnCount = 1 : cfTbl.RowCount = 4
            cfTbl.Padding = New Padding(0) : cfTbl.Margin = New Padding(0)
            cfTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            cfTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            cfTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 4))
            cfTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
            cfTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
            cfTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            pForm.Controls.Add(cfTbl)

            Dim accBar As Panel = AppTheme.AccentBar() : accBar.Dock = DockStyle.Fill : cfTbl.Controls.Add(accBar, 0, 0)
            lblFT = AppTheme.FormTitle("Customer Details") : lblFT.Dock = DockStyle.Fill : cfTbl.Controls.Add(lblFT, 0, 1)

            ' Stats strip
            Dim pStats As New Panel() : pStats.Dock = DockStyle.Fill : pStats.BackColor = Color.FromArgb(248, 244, 240) : cfTbl.Controls.Add(pStats, 0, 2)
            lblSpent = New Label() : lblSpent.Text = "Rs. 0" : lblSpent.Font = New System.Drawing.Font("Segoe UI", 13, FontStyle.Bold) : lblSpent.ForeColor = AppTheme.Primary : lblSpent.Location = New Point(16, 6) : lblSpent.AutoSize = True : pStats.Controls.Add(lblSpent)
            Dim lST As New Label() : lST.Text = "Total Spent" : lST.Font = AppTheme.F8B : lST.ForeColor = AppTheme.TextLight : lST.Location = New Point(16, 30) : lST.AutoSize = True : pStats.Controls.Add(lST)
            lblVisits = New Label() : lblVisits.Text = "0" : lblVisits.Font = New System.Drawing.Font("Segoe UI", 13, FontStyle.Bold) : lblVisits.ForeColor = AppTheme.Blue : lblVisits.Location = New Point(170, 6) : lblVisits.AutoSize = True : pStats.Controls.Add(lblVisits)
            Dim lVT As New Label() : lVT.Text = "Visits" : lVT.Font = AppTheme.F8B : lVT.ForeColor = AppTheme.TextLight : lVT.Location = New Point(170, 30) : lVT.AutoSize = True : pStats.Controls.Add(lVT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 4, 14, 8) : pF.BackColor = Color.White : cfTbl.Controls.Add(pF, 0, 3)
            Dim y As Integer = 0 : Dim sp As Integer = 46
            AddL(pF, "Full Name *", y) : txtName = NTB(pF, y + 18) : y += sp
            AddL(pF, "Phone Number", y) : txtPhone = NTB(pF, y + 18) : y += sp
            AddL(pF, "Email Address", y) : txtEmail = NTB(pF, y + 18) : y += sp
            AddL(pF, "Address", y) : txtAddr = NTB(pF, y + 18) : y += sp
            Dim btnS As New Button() : btnS.Text = "Save" : btnS.Size = New Size(94, 34) : btnS.Location = New Point(0, y) : AppTheme.StyleBtn(btnS, "success") : AddHandler btnS.Click, AddressOf BtnSave : pF.Controls.Add(btnS)
            Dim btnD As New Button() : btnD.Text = "Delete" : btnD.Size = New Size(94, 34) : btnD.Location = New Point(102, y) : AppTheme.StyleBtn(btnD, "danger") : AddHandler btnD.Click, AddressOf BtnDelete : pF.Controls.Add(btnD)

            ' History
            Dim pHist As New Panel() : pHist.Dock = DockStyle.Fill : pHist.BackColor = Color.White : pHist.Margin = New Padding(0, 4, 0, 0)
            rightTbl.Controls.Add(pHist, 0, 1)
            Dim histTbl As New TableLayoutPanel()
            histTbl.Dock = DockStyle.Fill : histTbl.ColumnCount = 1 : histTbl.RowCount = 2
            histTbl.Padding = New Padding(0) : histTbl.Margin = New Padding(0)
            histTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            histTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            histTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
            histTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            pHist.Controls.Add(histTbl)
            Dim lHdr As New Label() : lHdr.Text = "Purchase History" : lHdr.Font = AppTheme.F9B : lHdr.ForeColor = AppTheme.Primary : lHdr.Dock = DockStyle.Fill : lHdr.Padding = New Padding(14, 6, 0, 0) : histTbl.Controls.Add(lHdr, 0, 0)
            dgvHistory = New DataGridView() : dgvHistory.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvHistory) : histTbl.Controls.Add(dgvHistory, 0, 1)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(332, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function

        Private Sub LoadCust(Optional search As String = "")
            Try
                Dim sql As String = "SELECT CustomerID,Name,Phone,Email,LoyaltyPoints,TotalSpent FROM Customers WHERE 1=1"
                Dim prms As New List(Of SqlParameter)()
                If Not String.IsNullOrEmpty(search) Then sql &= " AND Name LIKE @s" : prms.Add(New SqlParameter("@s", "%" & search & "%"))
                sql &= " ORDER BY Name"
                dgvCust.DataSource = DatabaseManager.GetDataTable(sql, prms.ToArray())
                If dgvCust.Columns.Contains("CustomerID") Then dgvCust.Columns("CustomerID").Visible = False
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadForm(id As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT * FROM Customers WHERE CustomerID=@id", New SqlParameter("@id", id))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentID = id : lblFT.Text = "Edit: " & row("Name").ToString()
                txtName.Text = row("Name").ToString() : txtPhone.Text = row("Phone").ToString() : txtEmail.Text = row("Email").ToString() : txtAddr.Text = row("Address").ToString()
                lblSpent.Text = "Rs. " & CDec(row("TotalSpent")).ToString("N0")
                Dim visits As Object = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM Orders WHERE CustomerName=@n AND Status='Closed'", New SqlParameter("@n", row("Name").ToString()))
                lblVisits.Text = visits.ToString()
                dgvHistory.DataSource = DatabaseManager.GetDataTable("SELECT TOP 20 OrderID,TotalAmount,PaymentMode,CONVERT(VARCHAR,OrderDate,105) AS Date FROM Orders WHERE CustomerName=@n ORDER BY OrderID DESC", New SqlParameter("@n", row("Name").ToString()))
                If dgvHistory.Columns.Contains("OrderID") Then dgvHistory.Columns("OrderID").Visible = False
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub NewCust(sender As Object, e As EventArgs)
            currentID = 0 : lblFT.Text = "New Customer" : txtName.Text = "" : txtPhone.Text = "" : txtEmail.Text = "" : txtAddr.Text = "" : lblSpent.Text = "Rs. 0" : lblVisits.Text = "0" : txtName.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtName.Text.Trim()) Then
                MessageBox.Show("Name required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Try
                If currentID = 0 Then
                    DatabaseManager.ExecuteNonQuery("INSERT INTO Customers(Name,Phone,Email,Address) VALUES(@n,@p,@e,@a)", New SqlParameter("@n", txtName.Text.Trim()), New SqlParameter("@p", txtPhone.Text.Trim()), New SqlParameter("@e", txtEmail.Text.Trim()), New SqlParameter("@a", txtAddr.Text.Trim()))
                    MessageBox.Show("Customer added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE Customers SET Name=@n,Phone=@p,Email=@e,Address=@a WHERE CustomerID=@id", New SqlParameter("@n", txtName.Text.Trim()), New SqlParameter("@p", txtPhone.Text.Trim()), New SqlParameter("@e", txtEmail.Text.Trim()), New SqlParameter("@a", txtAddr.Text.Trim()), New SqlParameter("@id", currentID))
                    MessageBox.Show("Customer updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                LoadCust(txtSearch.Text)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnDelete(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            If MessageBox.Show("Delete '" & txtName.Text & "'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@id", New SqlParameter("@id", currentID))
                LoadCust() : NewCust(Nothing, Nothing)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class

    '═══════════════════════════════════════════════
    ' STAFF MODULE
    '═══════════════════════════════════════════════
    Public Class StaffModule
        Inherits Panel
        Private dgvStaff As DataGridView
        Private lblFT As Label
        Private txtName, txtPhone, txtEmail, txtSalary, txtSearch As TextBox
        Private cmbRole, cmbDept As ComboBox
        Private dtpJoin As DateTimePicker
        Private chkActive As CheckBox
        Private currentID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadStaff()
        End Sub

        Private Sub Build()
            Dim pBar As Panel = AppTheme.MakeToolbar()
            Me.Controls.Add(pBar)
            txtSearch = New TextBox() : txtSearch.Location = New Point(10, 12) : txtSearch.Size = New Size(200, 28) : txtSearch.Font = AppTheme.F10 : txtSearch.BorderStyle = BorderStyle.FixedSingle : txtSearch.PlaceholderText = "Search staff..." : txtSearch.BackColor = AppTheme.InputBg
            AddHandler txtSearch.TextChanged, Sub(s, e) LoadStaff(txtSearch.Text)
            pBar.Controls.Add(txtSearch)
            Dim btnAdd As New Button() : btnAdd.Text = "Add Staff" : btnAdd.Size = New Size(110, 32) : btnAdd.Location = New Point(220, 10) : AppTheme.StyleBtn(btnAdd, "success") : AddHandler btnAdd.Click, AddressOf NewStaff : pBar.Controls.Add(btnAdd)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 350)
            Me.Controls.Add(split)

            dgvStaff = New DataGridView() : dgvStaff.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvStaff) : dgvStaff.Margin = New Padding(0, 0, 4, 0)
            AddHandler dgvStaff.SelectionChanged, Sub(s, e)
                                                      If dgvStaff.CurrentRow IsNot Nothing AndAlso dgvStaff.CurrentRow.Index >= 0 Then
                                                          Try : LoadForm(CInt(dgvStaff.CurrentRow.Cells("StaffID").Value)) : Catch : End Try
                                                      End If
                                                  End Sub
            split.Controls.Add(dgvStaff, 0, 0)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(4, 0, 0, 0)
            split.Controls.Add(pForm, 1, 0)
            pForm.Controls.Add(AppTheme.AccentBar())
            lblFT = AppTheme.FormTitle("Staff Details") : pForm.Controls.Add(lblFT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 6, 14, 10) : pF.BackColor = Color.White : pForm.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 48
            AddL(pF, "Full Name *", y) : txtName = NTB(pF, y + 18) : y += sp
            AddL(pF, "Phone Number", y) : txtPhone = NTB(pF, y + 18) : y += sp
            AddL(pF, "Email", y) : txtEmail = NTB(pF, y + 18) : y += sp
            AddL(pF, "Role", y) : cmbRole = NCmb(pF, y + 18) : cmbRole.Items.AddRange({"Head Chef", "Sous Chef", "Cook", "Waiter", "Cashier", "Manager", "Cleaner", "Security"}) : cmbRole.SelectedIndex = 3 : y += sp
            AddL(pF, "Department", y) : cmbDept = NCmb(pF, y + 18) : cmbDept.Items.AddRange({"Kitchen", "Floor", "Billing", "Management", "Housekeeping", "Security"}) : cmbDept.SelectedIndex = 1 : y += sp
            AddL(pF, "Monthly Salary (Rs.)", y) : txtSalary = NTB(pF, y + 18) : txtSalary.Text = "0" : y += sp
            AddL(pF, "Join Date", y) : dtpJoin = New DateTimePicker() : dtpJoin.Location = New Point(0, y + 18) : dtpJoin.Size = New Size(318, 26) : dtpJoin.Font = AppTheme.F10 : dtpJoin.Format = DateTimePickerFormat.Short : dtpJoin.Value = DateTime.Today : pF.Controls.Add(dtpJoin) : y += sp
            chkActive = New CheckBox() : chkActive.Text = "Active Staff Member" : chkActive.Font = AppTheme.F10 : chkActive.ForeColor = AppTheme.TextDark : chkActive.Checked = True : chkActive.Location = New Point(0, y) : chkActive.AutoSize = True : pF.Controls.Add(chkActive) : y += 34
            Dim btnS As New Button() : btnS.Text = "Save" : btnS.Size = New Size(94, 34) : btnS.Location = New Point(0, y) : AppTheme.StyleBtn(btnS, "success") : AddHandler btnS.Click, AddressOf BtnSave : pF.Controls.Add(btnS)
            Dim btnD As New Button() : btnD.Text = "Deactivate" : btnD.Size = New Size(110, 34) : btnD.Location = New Point(102, y) : AppTheme.StyleBtn(btnD, "danger") : AddHandler btnD.Click, AddressOf BtnDeactivate : pF.Controls.Add(btnD)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(318, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function
        Private Function NCmb(p As Panel, y As Integer) As ComboBox
            Dim cb As New ComboBox() : cb.Location = New Point(0, y) : cb.Size = New Size(318, 26) : cb.Font = AppTheme.F10 : cb.DropDownStyle = ComboBoxStyle.DropDownList : p.Controls.Add(cb) : Return cb
        End Function

        Private Sub LoadStaff(Optional search As String = "")
            Try
                Dim sql As String = "SELECT StaffID,Name,Phone,Role,Department,Salary,IsActive FROM Staff WHERE 1=1"
                Dim prms As New List(Of SqlParameter)()
                If Not String.IsNullOrEmpty(search) Then sql &= " AND Name LIKE @s" : prms.Add(New SqlParameter("@s", "%" & search & "%"))
                sql &= " ORDER BY Name"
                dgvStaff.DataSource = DatabaseManager.GetDataTable(sql, prms.ToArray())
                If dgvStaff.Columns.Contains("StaffID") Then dgvStaff.Columns("StaffID").Visible = False
                For Each row As DataGridViewRow In dgvStaff.Rows
                    If row.Cells("IsActive").Value IsNot Nothing AndAlso CBool(row.Cells("IsActive").Value) = False Then
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(170, 155, 140) : row.DefaultCellStyle.BackColor = Color.FromArgb(250, 248, 246)
                    End If
                Next
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadForm(id As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT * FROM Staff WHERE StaffID=@id", New SqlParameter("@id", id))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentID = id : lblFT.Text = "Edit: " & row("Name").ToString()
                txtName.Text = row("Name").ToString() : txtPhone.Text = row("Phone").ToString() : txtEmail.Text = row("Email").ToString() : txtSalary.Text = row("Salary").ToString()
                Dim ri As Integer = cmbRole.Items.IndexOf(row("Role").ToString()) : If ri >= 0 Then cmbRole.SelectedIndex = ri
                Dim di As Integer = cmbDept.Items.IndexOf(row("Department").ToString()) : If di >= 0 Then cmbDept.SelectedIndex = di
                dtpJoin.Value = CDate(row("JoinDate")) : chkActive.Checked = CBool(row("IsActive"))
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub NewStaff(sender As Object, e As EventArgs)
            currentID = 0 : lblFT.Text = "New Staff" : txtName.Text = "" : txtPhone.Text = "" : txtEmail.Text = "" : txtSalary.Text = "0"
            cmbRole.SelectedIndex = 3 : cmbDept.SelectedIndex = 1 : dtpJoin.Value = DateTime.Today : chkActive.Checked = True : txtName.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtName.Text.Trim()) Then
                MessageBox.Show("Name required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim sal As Decimal = 0 : Decimal.TryParse(txtSalary.Text, sal)
            Try
                If currentID = 0 Then
                    DatabaseManager.ExecuteNonQuery("INSERT INTO Staff(Name,Phone,Email,Role,Department,Salary,JoinDate,IsActive) VALUES(@n,@p,@e,@r,@d,@s,@j,@a)", New SqlParameter("@n", txtName.Text.Trim()), New SqlParameter("@p", txtPhone.Text.Trim()), New SqlParameter("@e", txtEmail.Text.Trim()), New SqlParameter("@r", cmbRole.Text), New SqlParameter("@d", cmbDept.Text), New SqlParameter("@s", sal), New SqlParameter("@j", dtpJoin.Value.Date), New SqlParameter("@a", If(chkActive.Checked, 1, 0)))
                    MessageBox.Show("Staff added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE Staff SET Name=@n,Phone=@p,Email=@e,Role=@r,Department=@d,Salary=@s,JoinDate=@j,IsActive=@a WHERE StaffID=@id", New SqlParameter("@n", txtName.Text.Trim()), New SqlParameter("@p", txtPhone.Text.Trim()), New SqlParameter("@e", txtEmail.Text.Trim()), New SqlParameter("@r", cmbRole.Text), New SqlParameter("@d", cmbDept.Text), New SqlParameter("@s", sal), New SqlParameter("@j", dtpJoin.Value.Date), New SqlParameter("@a", If(chkActive.Checked, 1, 0)), New SqlParameter("@id", currentID))
                    MessageBox.Show("Staff updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                LoadStaff(txtSearch.Text)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnDeactivate(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            If MessageBox.Show("Deactivate '" & txtName.Text & "'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("UPDATE Staff SET IsActive=0 WHERE StaffID=@id", New SqlParameter("@id", currentID))
                LoadStaff() : NewStaff(Nothing, Nothing)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub
    End Class
End Namespace