Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient

Namespace RestaurantPOS
    Public Class MenuModule
        Inherits Panel
        Private dgvMenu As DataGridView
        Private lblFT As Label
        Private txtName,txtDesc,txtPrice,txtTax,txtSearch As TextBox
        Private cmbCat,cmbFilter As ComboBox
        Private chkVeg,chkAvail As CheckBox
        Private currentID As Integer = 0

        Public Sub New()
            Me.Dock = DockStyle.Fill : Me.BackColor = AppTheme.ContentBg
            Build() : LoadCats() : LoadMenu()
        End Sub

        Private Sub Build()
            Dim pBar As Panel = AppTheme.MakeToolbar()
            Me.Controls.Add(pBar)
            txtSearch = New TextBox() : txtSearch.Location = New Point(10,11) : txtSearch.Size = New Size(195,28)
            txtSearch.Font = AppTheme.F10 : txtSearch.BorderStyle = BorderStyle.FixedSingle : txtSearch.PlaceholderText = "Search items..."
            txtSearch.BackColor = AppTheme.InputBg
            AddHandler txtSearch.TextChanged, Sub(s,e) LoadMenu(txtSearch.Text,cmbFilter.Text)
            pBar.Controls.Add(txtSearch)
            Dim lc As New Label() : lc.Text = "Category:" : lc.Font = AppTheme.F8B : lc.ForeColor = AppTheme.TextMed : lc.Location = New Point(216,16) : lc.AutoSize = True : pBar.Controls.Add(lc)
            cmbFilter = New ComboBox() : cmbFilter.Location = New Point(278,12) : cmbFilter.Size = New Size(158,28) : cmbFilter.Font = AppTheme.F10 : cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList
            AddHandler cmbFilter.SelectedIndexChanged, Sub(s,e) LoadMenu(txtSearch.Text,cmbFilter.Text)
            pBar.Controls.Add(cmbFilter)
            Dim btnAddItem As New Button() : btnAddItem.Text = "Add New Item" : btnAddItem.Size = New Size(130,32) : btnAddItem.Location = New Point(448,10)
            AppTheme.StyleBtn(btnAddItem,"success") : AddHandler btnAddItem.Click, AddressOf BtnNew : pBar.Controls.Add(btnAddItem)
            Dim btnCat As New Button() : btnCat.Text = "Categories" : btnCat.Size = New Size(108,32) : btnCat.Location = New Point(586,10)
            AppTheme.StyleBtn(btnCat,"info") : AddHandler btnCat.Click, AddressOf ManageCats : pBar.Controls.Add(btnCat)

            Dim split As TableLayoutPanel = AppTheme.MakeSplit(100,348)
            Me.Controls.Add(split)

            dgvMenu = New DataGridView() : dgvMenu.Dock = DockStyle.Fill : AppTheme.MakeGrid(dgvMenu) : dgvMenu.Margin = New Padding(0,0,4,0)
            AddHandler dgvMenu.SelectionChanged, Sub(s,e)
                If dgvMenu.CurrentRow IsNot Nothing AndAlso dgvMenu.CurrentRow.Index >= 0 Then
                    Try : LoadToForm(CInt(dgvMenu.CurrentRow.Cells("ItemID").Value)) : Catch : End Try
                End If
            End Sub
            split.Controls.Add(dgvMenu,0,0)

            Dim pForm As New Panel() : pForm.Dock = DockStyle.Fill : pForm.BackColor = Color.White : pForm.Margin = New Padding(4,0,0,0)
            split.Controls.Add(pForm,1,0)
            pForm.Controls.Add(AppTheme.AccentBar())
            lblFT = AppTheme.FormTitle("Item Details") : pForm.Controls.Add(lblFT)

            Dim pF As New Panel() : pF.Dock = DockStyle.Fill : pF.Padding = New Padding(14,6,14,10) : pF.BackColor = Color.White : pForm.Controls.Add(pF)
            Dim y As Integer = 0 : Dim sp As Integer = 50
            AddL(pF,"Item Name *",y) : txtName = NTB(pF,y+18) : y += sp
            AddL(pF,"Category *",y)
            cmbCat = New ComboBox() : cmbCat.Location = New Point(0,y+18) : cmbCat.Size = New Size(318,26) : cmbCat.Font = AppTheme.F10 : cmbCat.DropDownStyle = ComboBoxStyle.DropDownList : pF.Controls.Add(cmbCat) : y += sp
            AddL(pF,"Description",y) : txtDesc = NTB(pF,y+18) : y += sp
            AddL(pF,"Price (Rs.) *",y) : txtPrice = NTB(pF,y+18) : y += sp
            AddL(pF,"Tax % (GST)",y) : txtTax = NTB(pF,y+18) : txtTax.Text = "5" : y += sp
            chkVeg = New CheckBox() : chkVeg.Text = "Vegetarian Item" : chkVeg.Font = AppTheme.F10 : chkVeg.ForeColor = AppTheme.Green : chkVeg.Checked = True : chkVeg.Location = New Point(0,y) : chkVeg.AutoSize = True : pF.Controls.Add(chkVeg)
            chkAvail = New CheckBox() : chkAvail.Text = "Available on Menu" : chkAvail.Font = AppTheme.F10 : chkAvail.ForeColor = AppTheme.TextDark : chkAvail.Checked = True : chkAvail.Location = New Point(0,y+26) : chkAvail.AutoSize = True : pF.Controls.Add(chkAvail) : y += 60
            Dim btnSaveItem As New Button() : btnSaveItem.Text = "Save" : btnSaveItem.Size = New Size(94,34) : btnSaveItem.Location = New Point(0,y) : AppTheme.StyleBtn(btnSaveItem,"success") : AddHandler btnSaveItem.Click, AddressOf BtnSave : pF.Controls.Add(btnSaveItem)
            Dim btnTog As New Button() : btnTog.Text = "Toggle" : btnTog.Size = New Size(94,34) : btnTog.Location = New Point(102,y) : AppTheme.StyleBtn(btnTog,"warning") : AddHandler btnTog.Click, AddressOf BtnToggle : pF.Controls.Add(btnTog)
            Dim btnDelItem As New Button() : btnDelItem.Text = "Delete" : btnDelItem.Size = New Size(94,34) : btnDelItem.Location = New Point(204,y) : AppTheme.StyleBtn(btnDelItem,"danger") : AddHandler btnDelItem.Click, AddressOf BtnDelete : pF.Controls.Add(btnDelItem)
        End Sub

        Private Sub AddL(p As Panel, t As String, y As Integer)
            Dim l As New Label() : l.Text = t : l.Font = AppTheme.F8B : l.ForeColor = AppTheme.TextMed : l.Location = New Point(0,y) : l.AutoSize = True : p.Controls.Add(l)
        End Sub
        Private Function NTB(p As Panel, y As Integer) As TextBox
            Dim tb As New TextBox() : tb.Location = New Point(0,y) : tb.Size = New Size(318,26) : tb.Font = AppTheme.F10 : tb.BorderStyle = BorderStyle.FixedSingle : tb.BackColor = AppTheme.InputBg : p.Controls.Add(tb) : Return tb
        End Function

        Private Sub LoadCats()
            Try
                cmbCat.Items.Clear() : cmbFilter.Items.Clear() : cmbFilter.Items.Add("All Categories")
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT CategoryID,Name FROM MenuCategories WHERE IsActive=1 ORDER BY SortOrder")
                For Each row As DataRow In dt.Rows : cmbCat.Items.Add(row("Name").ToString()) : cmbFilter.Items.Add(row("Name").ToString()) : Next
                If cmbCat.Items.Count > 0 Then cmbCat.SelectedIndex = 0 : cmbFilter.SelectedIndex = 0
            Catch
            End Try
        End Sub

        Private Sub LoadMenu(Optional search As String = "", Optional cat As String = "")
            Try
                Dim sql As String = "SELECT mi.ItemID, mi.Name AS [Item Name], mc.Name AS Category, mi.Price, mi.TaxPercent AS [Tax%], CASE WHEN mi.IsVeg=1 THEN 'Veg' ELSE 'Non-Veg' END AS Type, CASE WHEN mi.IsAvailable=1 THEN 'Yes' ELSE 'No' END AS Available FROM MenuItems mi LEFT JOIN MenuCategories mc ON mi.CategoryID=mc.CategoryID WHERE 1=1"
                Dim prms As New List(Of SqlParameter)()
                If Not String.IsNullOrEmpty(search) Then sql &= " AND mi.Name LIKE @s" : prms.Add(New SqlParameter("@s","%" & search & "%"))
                If Not String.IsNullOrEmpty(cat) AndAlso cat <> "All Categories" Then sql &= " AND mc.Name=@cat" : prms.Add(New SqlParameter("@cat",cat))
                sql &= " ORDER BY mc.SortOrder, mi.Name"
                dgvMenu.DataSource = DatabaseManager.GetDataTable(sql,prms.ToArray())
                If dgvMenu.Columns.Contains("ItemID") Then dgvMenu.Columns("ItemID").Visible = False
                For Each row As DataGridViewRow In dgvMenu.Rows
                    If row.Cells("Available").Value IsNot Nothing AndAlso row.Cells("Available").Value.ToString() = "No" Then
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(165,148,130) : row.DefaultCellStyle.BackColor = Color.FromArgb(250,247,244)
                    End If
                Next
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub LoadToForm(itemID As Integer)
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable("SELECT mi.*, mc.Name AS CatName FROM MenuItems mi LEFT JOIN MenuCategories mc ON mi.CategoryID=mc.CategoryID WHERE mi.ItemID=@id",New SqlParameter("@id",itemID))
                If dt.Rows.Count = 0 Then Return
                Dim row As DataRow = dt.Rows(0) : currentID = itemID : lblFT.Text = "Edit: " & row("Name").ToString()
                txtName.Text = row("Name").ToString() : txtDesc.Text = row("Description").ToString() : txtPrice.Text = row("Price").ToString() : txtTax.Text = row("TaxPercent").ToString()
                chkVeg.Checked = CBool(row("IsVeg")) : chkAvail.Checked = CBool(row("IsAvailable"))
                Dim ci As Integer = cmbCat.Items.IndexOf(row("CatName").ToString()) : If ci >= 0 Then cmbCat.SelectedIndex = ci
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub BtnNew(sender As Object, e As EventArgs)
            currentID = 0 : lblFT.Text = "New Menu Item" : txtName.Text = "" : txtDesc.Text = "" : txtPrice.Text = "0" : txtTax.Text = "5"
            chkVeg.Checked = True : chkAvail.Checked = True : If cmbCat.Items.Count > 0 Then cmbCat.SelectedIndex = 0 : txtName.Focus()
        End Sub

        Private Sub BtnSave(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(txtName.Text.Trim()) Then
                MessageBox.Show("Item name required.","Validation",MessageBoxButtons.OK,MessageBoxIcon.Warning)
                Return
            End If
            Dim price As Decimal = 0 : Decimal.TryParse(txtPrice.Text,price) : Dim tax As Decimal = 5 : Decimal.TryParse(txtTax.Text,tax)
            Try
                Dim catID As Object = DBNull.Value
                Dim cd As DataTable = DatabaseManager.GetDataTable("SELECT CategoryID FROM MenuCategories WHERE Name=@n",New SqlParameter("@n",cmbCat.Text))
                If cd.Rows.Count > 0 Then catID = CInt(cd.Rows(0)("CategoryID"))
                If currentID = 0 Then
                    DatabaseManager.ExecuteNonQuery("INSERT INTO MenuItems(CategoryID,Name,Description,Price,TaxPercent,IsVeg,IsAvailable) VALUES(@c,@n,@d,@p,@t,@v,@a)",New SqlParameter("@c",catID),New SqlParameter("@n",txtName.Text.Trim()),New SqlParameter("@d",txtDesc.Text.Trim()),New SqlParameter("@p",price),New SqlParameter("@t",tax),New SqlParameter("@v",If(chkVeg.Checked,1,0)),New SqlParameter("@a",If(chkAvail.Checked,1,0)))
                    MessageBox.Show("Item added!","Success",MessageBoxButtons.OK,MessageBoxIcon.Information)
                Else
                    DatabaseManager.ExecuteNonQuery("UPDATE MenuItems SET CategoryID=@c,Name=@n,Description=@d,Price=@p,TaxPercent=@t,IsVeg=@v,IsAvailable=@a WHERE ItemID=@id",New SqlParameter("@c",catID),New SqlParameter("@n",txtName.Text.Trim()),New SqlParameter("@d",txtDesc.Text.Trim()),New SqlParameter("@p",price),New SqlParameter("@t",tax),New SqlParameter("@v",If(chkVeg.Checked,1,0)),New SqlParameter("@a",If(chkAvail.Checked,1,0)),New SqlParameter("@id",currentID))
                    MessageBox.Show("Item updated!","Success",MessageBoxButtons.OK,MessageBoxIcon.Information)
                End If
                LoadMenu(txtSearch.Text,cmbFilter.Text)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
        End Sub

        Private Sub BtnToggle(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            Try
                DatabaseManager.ExecuteNonQuery("UPDATE MenuItems SET IsAvailable=CASE WHEN IsAvailable=1 THEN 0 ELSE 1 END WHERE ItemID=@id",New SqlParameter("@id",currentID))
                LoadMenu(txtSearch.Text,cmbFilter.Text)
            Catch ex As Exception : MessageBox.Show("Error: " & ex.Message) : End Try
        End Sub

        Private Sub BtnDelete(sender As Object, e As EventArgs)
            If currentID = 0 Then Return
            If MessageBox.Show("Delete '" & txtName.Text & "'?","Confirm",MessageBoxButtons.YesNo,MessageBoxIcon.Warning) = DialogResult.No Then Return
            Try
                DatabaseManager.ExecuteNonQuery("DELETE FROM MenuItems WHERE ItemID=@id",New SqlParameter("@id",currentID))
                LoadMenu() : BtnNew(Nothing,Nothing)
            Catch ex As Exception : MessageBox.Show("Cannot delete: " & ex.Message) : End Try
        End Sub

        Private Sub ManageCats(sender As Object, e As EventArgs)
            Dim frm As New Form() : frm.Text = "Manage Categories" : frm.Size = New Size(480,400) : frm.StartPosition = FormStartPosition.CenterParent : frm.BackColor = Color.White
            Dim dgvC As New DataGridView() : dgvC.Location = New Point(10,10) : dgvC.Size = New Size(330,340) : AppTheme.MakeGrid(dgvC) : frm.Controls.Add(dgvC)
            Dim loadC As Action = Sub()
                dgvC.DataSource = DatabaseManager.GetDataTable("SELECT CategoryID,Name,SortOrder FROM MenuCategories WHERE IsActive=1 ORDER BY SortOrder")
                If dgvC.Columns.Contains("CategoryID") Then dgvC.Columns("CategoryID").Visible = False
            End Sub
            loadC()
            Dim y2 As Integer = 10
            Dim AddL2 As Action(Of String,Integer) = Sub(t,yy)
                Dim l As New Label()
                l.Text = t
                l.Font = AppTheme.F8B
                l.ForeColor = AppTheme.TextMed
                l.Location = New Point(350,yy)
                l.AutoSize = True
                frm.Controls.Add(l)
            End Sub
            AddL2("Name:",y2) : Dim tN As New TextBox() : tN.Location = New Point(350,y2+18) : tN.Size = New Size(112,26) : tN.Font = AppTheme.F10 : tN.BorderStyle = BorderStyle.FixedSingle : frm.Controls.Add(tN) : y2 += 52
            AddL2("Sort Order:",y2) : Dim tS As New TextBox() : tS.Location = New Point(350,y2+18) : tS.Size = New Size(112,26) : tS.Font = AppTheme.F10 : tS.BorderStyle = BorderStyle.FixedSingle : tS.Text = "1" : frm.Controls.Add(tS) : y2 += 52
            Dim bA As New Button() : bA.Text = "Add" : bA.Size = New Size(112,32) : bA.Location = New Point(350,y2) : AppTheme.StyleBtn(bA,"success") : frm.Controls.Add(bA)
            AddHandler bA.Click, Sub(s,ev)
                If String.IsNullOrEmpty(tN.Text.Trim()) Then Return
                Dim so As Integer = 0 : Integer.TryParse(tS.Text,so)
                DatabaseManager.ExecuteNonQuery("INSERT INTO MenuCategories(Name,SortOrder) VALUES(@n,@s)",New SqlParameter("@n",tN.Text.Trim()),New SqlParameter("@s",so))
                loadC() : tN.Text = ""
            End Sub
            y2 += 42
            Dim bR As New Button() : bR.Text = "Remove" : bR.Size = New Size(112,32) : bR.Location = New Point(350,y2) : AppTheme.StyleBtn(bR,"danger") : frm.Controls.Add(bR)
            AddHandler bR.Click, Sub(s,ev)
                If dgvC.CurrentRow Is Nothing Then Return
                If MessageBox.Show("Remove?","Confirm",MessageBoxButtons.YesNo,MessageBoxIcon.Warning) = DialogResult.No Then Return
                DatabaseManager.ExecuteNonQuery("UPDATE MenuCategories SET IsActive=0 WHERE CategoryID=@id",New SqlParameter("@id",CInt(dgvC.CurrentRow.Cells("CategoryID").Value)))
                loadC()
            End Sub
            frm.ShowDialog(Me.FindForm()) : LoadCats() : LoadMenu()
        End Sub
    End Class
End Namespace
