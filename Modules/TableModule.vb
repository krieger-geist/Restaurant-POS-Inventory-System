Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient

Namespace RestaurantPOS
    Public Class TableModule
        Inherits Panel
        Private pnlVisual As Panel
        Private dgvTables As DataGridView
        Private lblFT As Label
        Private txtNum As TextBox
        Private cmbCap, cmbLoc, cmbStat As ComboBox
        Private currentID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadAll()
        End Sub

        Private Sub Build()
            ' ROOT: TableLayoutPanel with 2 rows - toolbar (fixed 52px) | body (fill)
            Dim rootTbl As New TableLayoutPanel()
            rootTbl.Dock = DockStyle.Fill : rootTbl.ColumnCount = 1 : rootTbl.RowCount = 2
            rootTbl.Padding = New Padding(0) : rootTbl.Margin = New Padding(0)
            rootTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rootTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rootTbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 52))
            rootTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Me.Controls.Add(rootTbl)

            Dim pBar As Panel = AppTheme.MakeToolbar()
            pBar.Dock = DockStyle.Fill
            rootTbl.Controls.Add(pBar, 0, 0)
            Dim btnAdd As New Button() : btnAdd.Text = "Add New Table" : btnAdd.Size = New Size(140, 32) : btnAdd.Location = New Point(10, 10)
            AppTheme.StyleBtn(btnAdd, "success") : AddHandler btnAdd.Click, AddressOf NewTable : pBar.Controls.Add(btnAdd)
            Dim btnRef As New Button() : btnRef.Text = "Refresh" : btnRef.Size = New Size(90, 32) : btnRef.Location = New Point(158, 10)
            AppTheme.StyleBtn(btnRef, "secondary")
            AddHandler btnRef.Click, Sub(s, e) LoadAll()
            pBar.Controls.Add(btnRef)
            Dim lx As Integer = 264
            Dim legendPairs As New List(Of Object())()
            legendPairs.Add(New Object() {"Available", AppTheme.Green})
            legendPairs.Add(New Object() {"Occupied", AppTheme.Red})
            legendPairs.Add(New Object() {"Reserved", AppTheme.Orange})
            For Each pair As Object() In legendPairs
                Dim dot As New Panel() : dot.Size = New Size(12, 12) : dot.Location = New Point(lx, 20) : dot.BackColor = CType(pair(1), Color) : pBar.Controls.Add(dot)
                Dim ll As New Label() : ll.Text = CStr(pair(0)) : ll.Font = AppTheme.F9 : ll.ForeColor = AppTheme.TextMed : ll.AutoSize = True : ll.Location = New Point(lx + 16, 17) : pBar.Controls.Add(ll) : lx += 106
            Next

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100, 358)
            rootTbl.Controls.Add(split, 0, 1)

            pnlVisual = New Panel() : pnlVisual.Dock = DockStyle.Fill : pnlVisual.BackColor = Color.White
            pnlVisual.AutoScroll = True : pnlVisual.Margin = New Padding(0, 0, 4, 0)
            split.Controls.Add(pnlVisual, 0, 0)

            Dim rightTbl As New TableLayoutPanel()
            rightTbl.Dock = DockStyle.Fill : rightTbl.ColumnCount = 1 : rightTbl.RowCount = 2
            rightTbl.Padding = New Padding(0) : rightTbl.Margin = New Padding(4, 0, 0, 0)
            rightTbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            rightTbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            rightTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 46))
            rightTbl.RowStyles.Add(New RowStyle(SizeType.Percent, 54))
            split.Controls.Add(rightTbl, 1, 0)

            dgvTables = New DataGridView() : dgvTables.Dock = DockStyle.Fill : dgvTables.Margin = New Padding(0, 0, 0, 4)
            AppTheme.MakeGrid(dgvTables)
            AddHandler dgvTables.SelectionChanged, Sub(s, e)
                                                       If dgvTables.CurrentRow IsNot Nothing AndAlso dgvTables.CurrentRow.Index >= 0 Then
                                                           Try : LoadForm(CInt(dgvTables.CurrentRow.Cells("TableID").Value)) : Catch : End Try
                                                       End If
                                                   End Sub
            rightTbl.Controls.Add(dgvTables, 0, 0)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(0, 4, 0, 0)
            rightTbl.Controls.Add(pForm, 0, 1)

            pForm.Controls.Add(AppTheme.AccentBar())
            lblFT = AppTheme.FormTitle("Table Details") : pForm.Controls.Add(lblFT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14, 6, 14, 10) : pF.BackColor = Color.White : pForm.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 50
            AddL(pF, "Table Number *", y) : txtNum = NTB(pF, y + 18) : y += sp
            AddL(pF, "Capacity", y) : cmbCap = NCmb(pF, y + 18) : cmbCap.Items.AddRange({"2", "4", "6", "8", "10", "12"}) : cmbCap.SelectedIndex = 1 : y += sp
            AddL(pF, "Location", y) : cmbLoc = NCmb(pF, y + 18) : cmbLoc.Items.AddRange({"Indoor", "Outdoor", "VIP", "Terrace", "Private"}) : cmbLoc.SelectedIndex = 0 : y += sp
            AddL(pF, "Status", y) : cmbStat = NCmb(pF, y + 18) : cmbStat.Items.AddRange({"Available", "Occupied", "Reserved", "Maintenance"}) : cmbStat.SelectedIndex = 0 : y += sp

            Dim btnS As New Button() : btnS.Text = "Save" : btnS.Size = New Size(94, 34) : btnS.Location = New Point(0, y)
            AppTheme.StyleBtn(btnS, "success") : AddHandler btnS.Click, AddressOf BtnSave : pF.Controls.Add(btnS)
            Dim btnD As New Button() : btnD.Text = "Delete" : btnD.Size = New Size(94, 34) : btnD.Location = New Point(102, y)
            AppTheme.StyleBtn(btnD, "danger") : AddHandler btnD.Click, AddressOf BtnDelete : pF.Controls.Add(btnD)
            Dim btnN As New Button() : btnN.Text = "New" : btnN.Size = New Size(94, 34) : btnN.Location = New Point(204, y)
            AppTheme.StyleBtn(btnN, "secondary") : AddHandler btnN.Click, AddressOf NewTable : pF.Controls.Add(btnN)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0, y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0, y) : tb.Size = New Size(328, 26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function
        Private Function NCmb(p As Panel, y As Integer) As ComboBox
            Dim cb As New ComboBox() : cb.Location = New Point(0, y) : cb.Size = New Size(328, 26) : cb.Font = AppTheme.F10 : cb.DropDownStyle = ComboBoxStyle.DropDownList : p.Controls.Add(cb) : Return cb
        End Function

        Private Sub LoadAll()
            LoadGrid()
            LoadVisual()
        End Sub

        Private Sub LoadGrid()
            Try
                dgvTables.DataSource = DatabaseManager.GetDataTable("SELECT TableID, TableNumber AS [Table No], Capacity, Location, Status FROM RestaurantTables ORDER BY TableNumber")
                If dgvTables.Columns.Contains("TableID") Then dgvTables.Columns("TableID").Visible = False
                For Each row As DataGridViewRow In dgvTables.Rows
                    Dim st As String = If(row.Cells("Status").Value IsNot Nothing, row.Cells("Status").Value.ToString(), "")
                    Select Case st
                        Case "Occupied" : row.DefaultCellStyle.BackColor = Color.FromArgb(255, 236, 236)
                        Case "Reserved" : row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 222)
                    End Select
                Next
            Catch ex As Exception : MessageBox.Show("Grid error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadVisual()
            pnlVisual.Controls.Clear()
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT TableID,TableNumber,Capacity,Location,Status FROM RestaurantTables ORDER BY TableNumber")
                Dim col As Integer = 0 : Dim row2 As Integer = 0
                Dim cw As Integer = 128 : Dim ch As Integer = 94 : Dim gx As Integer = 10 : Dim gy As Integer = 10
                For Each dr As DataRow In dt.Rows
                    Dim tid As Integer = CInt(dr("TableID")) : Dim status As String = dr("Status").ToString()
                    Dim bgC As Color = Color.White : Dim bc As Color = AppTheme.Green
                    Select Case status
                        Case "Occupied" : bgC = Color.FromArgb(255, 240, 240) : bc = AppTheme.Red
                        Case "Reserved" : bgC = Color.FromArgb(255, 250, 225) : bc = AppTheme.Orange
                        Case "Maintenance" : bgC = Color.FromArgb(245, 245, 245) : bc = Color.Gray
                    End Select
                    Dim card As New Panel() : card.Size = New Size(cw, ch) : card.Location = New Point(10 + col * (cw + gx), 10 + row2 * (ch + gy)) : card.BackColor = bgC : card.Cursor = Cursors.Hand
                    Dim capturedBc As Color = bc
                    AddHandler card.Paint, Sub(s, e)
                                               e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                                               Using pen As New Pen(capturedBc, 2) : e.Graphics.DrawRectangle(pen, 1, 1, DirectCast(s, Panel).Width - 3, DirectCast(s, Panel).Height - 3) : End Using
                                           End Sub
                    Dim lN As New Label() : lN.Text = dr("TableNumber").ToString() : lN.Font = New System.Drawing.Font("Segoe UI", 14, FontStyle.Bold) : lN.ForeColor = bc : lN.Location = New Point(8, 5) : lN.AutoSize = True : card.Controls.Add(lN)
                    Dim lC As New Label() : lC.Text = "Seats: " & dr("Capacity").ToString() : lC.Font = AppTheme.F8B : lC.ForeColor = AppTheme.TextMed : lC.Location = New Point(8, 42) : lC.AutoSize = True : card.Controls.Add(lC)
                    Dim lL As New Label() : lL.Text = dr("Location").ToString() : lL.Font = AppTheme.F9 : lL.ForeColor = AppTheme.TextLight : lL.Location = New Point(8, 57) : lL.AutoSize = True : card.Controls.Add(lL)
                    Dim lS As New Label() : lS.Text = status : lS.Font = AppTheme.F8B : lS.ForeColor = bc : lS.Location = New Point(8, 74) : lS.AutoSize = True : card.Controls.Add(lS)
                    Dim capturedID As Integer = tid
                    AddHandler card.Click, Sub(s, e) LoadForm(capturedID)
                    For Each ctrl As Control In card.Controls
                        AddHandler ctrl.Click, Sub(s, e) LoadForm(capturedID)
                    Next
                    pnlVisual.Controls.Add(card) : col += 1 : If col >= 5 Then col = 0 : row2 += 1
                Next
            Catch ex As Exception : MessageBox.Show("Visual error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadForm(tableID As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT * FROM RestaurantTables WHERE TableID=@id", New SqlParameter("@id", tableID))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentID = tableID : lblFT.Text = "Edit: " & row("TableNumber").ToString()
                txtNum.Text = row("TableNumber").ToString()
                Dim ci As Integer = cmbCap.Items.IndexOf(row("Capacity").ToString()) : If ci >= 0 Then cmbCap.SelectedIndex = ci
                Dim li As Integer = cmbLoc.Items.IndexOf(row("Location").ToString()) : If li >= 0 Then cmbLoc.SelectedIndex = li
                Dim si As Integer = cmbStat.Items.IndexOf(row("Status").ToString()) : If si >= 0 Then cmbStat.SelectedIndex = si
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub NewTable(sender As Object, e As EventArgs)
            currentID = 0 : lblFT.Text = "New Table" : txtNum.Text = "" : cmbCap.SelectedIndex = 1 : cmbLoc.SelectedIndex = 0 : cmbStat.SelectedIndex = 0 : txtNum.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtNum.Text.Trim()) Then
                MessageBox.Show("Table number required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Try
                If currentID = 0 Then
                    DatabaseManager.ExecuteNonQuery("INSERT INTO RestaurantTables(TableNumber,Capacity,Location,Status) VALUES(@n,@c,@l,@s)", New SqlParameter("@n", txtNum.Text.Trim()), New SqlParameter("@c", CInt(cmbCap.Text)), New SqlParameter("@l", cmbLoc.Text), New SqlParameter("@s", cmbStat.Text))
                    MessageBox.Show("Table added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE RestaurantTables SET TableNumber=@n,Capacity=@c,Location=@l,Status=@s WHERE TableID=@id", New SqlParameter("@n", txtNum.Text.Trim()), New SqlParameter("@c", CInt(cmbCap.Text)), New SqlParameter("@l", cmbLoc.Text), New SqlParameter("@s", cmbStat.Text), New SqlParameter("@id", currentID))
                    MessageBox.Show("Table updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                LoadAll()
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnDelete(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            If MessageBox.Show("Delete '" & txtNum.Text & "'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("DELETE FROM RestaurantTables WHERE TableID=@id", New SqlParameter("@id", currentID))
                MessageBox.Show("Deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information) : LoadAll() : NewTable(Nothing, Nothing)
            Catch ex As Exception : MessageBox.Show("Cannot delete: " & ex.Message) : End Try
        End Sub
    End Class
End Namespace