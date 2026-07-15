Imports System.Data
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data.SqlClient
Imports BCrypt.Net

Namespace RestaurantPOS
    Public Class FrmLogin
        Inherits Form
        Private txtUser As TextBox
        Private txtPass As TextBox
        Private lblErr As Label
        Private btnGo As Button

        Public Sub New()
            Me.Text = "Restaurant POS — Login"
            Me.ClientSize = New Size(820, 520)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.BackColor = Color.White
            Build()
        End Sub

        Private Sub Build()
            Dim pL As New Panel()
            pL.Size = New Size(300, 520) : pL.Location = New Point(0, 0)
            AddHandler pL.Paint, Sub(s, e)
                                     Using br As New Drawing2D.LinearGradientBrush(New Rectangle(0, 0, 300, 520), Color.FromArgb(80, 10, 3), Color.FromArgb(183, 55, 35), 150)
                                         e.Graphics.FillRectangle(br, 0, 0, 300, 520)
                                     End Using
                                 End Sub
            Me.Controls.Add(pL)

            Dim logo As New Panel()
            logo.Size = New Size(76, 76) : logo.Location = New Point(112, 62) : logo.BackColor = Color.Transparent
            AddHandler logo.Paint, Sub(s, e)
                                       e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                                       e.Graphics.FillEllipse(New SolidBrush(Color.FromArgb(60, 255, 255, 255)), 0, 0, 75, 75)
                                       e.Graphics.DrawEllipse(New Pen(Color.FromArgb(255, 165, 0), 2), 1, 1, 73, 73)
                                       Using f As New System.Drawing.Font("Segoe UI", 25, FontStyle.Bold)
                                           Dim sz As SizeF = e.Graphics.MeasureString("R", f)
                                           e.Graphics.DrawString("R", f, Brushes.White, (76 - sz.Width) / 2, (76 - sz.Height) / 2)
                                       End Using
                                   End Sub
            pL.Controls.Add(logo)

            Dim lR As New Label() : lR.Text = "Restaurant" : lR.Font = New System.Drawing.Font("Segoe UI", 21, FontStyle.Bold)
            lR.ForeColor = Color.White : lR.BackColor = Color.Transparent : lR.AutoSize = True : lR.Location = New Point(60, 158) : pL.Controls.Add(lR)
            Dim lP As New Label() : lP.Text = "POS System" : lP.Font = New System.Drawing.Font("Segoe UI", 14)
            lP.ForeColor = Color.FromArgb(255, 200, 100) : lP.BackColor = Color.Transparent : lP.AutoSize = True : lP.Location = New Point(85, 196) : pL.Controls.Add(lP)
            Dim lM As New Label() : lM.Text = "Complete Management" : lM.Font = New System.Drawing.Font("Segoe UI", 9)
            lM.ForeColor = Color.FromArgb(200, 175, 160) : lM.BackColor = Color.Transparent : lM.AutoSize = True : lM.Location = New Point(58, 224) : pL.Controls.Add(lM)

            Dim sep As New Panel() : sep.Size = New Size(240, 1) : sep.Location = New Point(30, 252)
            sep.BackColor = Color.FromArgb(60, 255, 255, 255) : pL.Controls.Add(sep)

            Dim feats() As String = {"Table & Order Management", "Kitchen Order Tickets", "PDF Billing & Receipts", "Reservations & Reports", "Inventory Management"}
            Dim fy As Integer = 265
            For Each f As String In feats
                Dim dot As New Panel() : dot.Size = New Size(7, 7) : dot.Location = New Point(30, fy + 10) : dot.BackColor = Color.FromArgb(255, 200, 100) : pL.Controls.Add(dot)
                Dim lf As New Label() : lf.Text = f : lf.Font = New System.Drawing.Font("Segoe UI", 9)
                lf.ForeColor = Color.FromArgb(208, 193, 180) : lf.BackColor = Color.Transparent : lf.AutoSize = True : lf.Location = New Point(46, fy + 4) : pL.Controls.Add(lf)
                fy += 27
            Next

            Dim pR As New Panel()
            pR.Size = New Size(520, 520) : pR.Location = New Point(300, 0) : pR.BackColor = Color.White
            Me.Controls.Add(pR)

            Dim lW As New Label() : lW.Text = "Welcome Back"
            lW.Font = New System.Drawing.Font("Segoe UI", 24, FontStyle.Bold)
            lW.ForeColor = Color.FromArgb(28, 18, 8) : lW.AutoSize = True : lW.Location = New Point(50, 56) : pR.Controls.Add(lW)

            Dim lS As New Label() : lS.Text = "Sign in to your POS account"
            lS.Font = New System.Drawing.Font("Segoe UI", 11) : lS.ForeColor = Color.FromArgb(120, 98, 78)
            lS.AutoSize = True : lS.Location = New Point(50, 108) : pR.Controls.Add(lS)

            Dim sepH As New Panel() : sepH.Size = New Size(420, 1) : sepH.Location = New Point(50, 140)
            sepH.BackColor = Color.FromArgb(218, 208, 198) : pR.Controls.Add(sepH)

            Dim lU As New Label() : lU.Text = "USERNAME" : lU.Font = New System.Drawing.Font("Segoe UI", 8, FontStyle.Bold)
            lU.ForeColor = Color.FromArgb(120, 98, 78) : lU.Location = New Point(50, 160) : lU.AutoSize = True : pR.Controls.Add(lU)
            txtUser = New TextBox() : txtUser.Location = New Point(50, 180) : txtUser.Size = New Size(420, 32)
            txtUser.Font = New System.Drawing.Font("Segoe UI", 12) : txtUser.BorderStyle = BorderStyle.FixedSingle
            txtUser.BackColor = Color.FromArgb(250, 248, 246) : txtUser.Text = "admin" : pR.Controls.Add(txtUser)

            Dim lPL As New Label() : lPL.Text = "PASSWORD" : lPL.Font = New System.Drawing.Font("Segoe UI", 8, FontStyle.Bold)
            lPL.ForeColor = Color.FromArgb(120, 98, 78) : lPL.Location = New Point(50, 226) : lPL.AutoSize = True : pR.Controls.Add(lPL)
            txtPass = New TextBox() : txtPass.Location = New Point(50, 246) : txtPass.Size = New Size(420, 32)
            txtPass.Font = New System.Drawing.Font("Segoe UI", 12) : txtPass.BorderStyle = BorderStyle.FixedSingle
            txtPass.BackColor = Color.FromArgb(250, 248, 246) : txtPass.UseSystemPasswordChar = True
            AddHandler txtPass.KeyPress, Sub(s, e)
                                             If e.KeyChar = Chr(13) Then DoLogin()
                                         End Sub
            pR.Controls.Add(txtPass)

            Dim chk As New CheckBox() : chk.Text = "Show password" : chk.Font = New System.Drawing.Font("Segoe UI", 9)
            chk.ForeColor = Color.FromArgb(120, 98, 78) : chk.Location = New Point(50, 288) : chk.AutoSize = True
            AddHandler chk.CheckedChanged, Sub(s, e) txtPass.UseSystemPasswordChar = Not chk.Checked
            pR.Controls.Add(chk)

            lblErr = New Label() : lblErr.Text = "" : lblErr.Font = New System.Drawing.Font("Segoe UI", 9)
            lblErr.ForeColor = Color.FromArgb(192, 57, 43) : lblErr.Location = New Point(50, 316) : lblErr.Size = New Size(420, 22) : pR.Controls.Add(lblErr)

            btnGo = New Button() : btnGo.Text = "SIGN IN" : btnGo.Location = New Point(50, 346)
            btnGo.Size = New Size(420, 48) : btnGo.FlatStyle = FlatStyle.Flat : btnGo.FlatAppearance.BorderSize = 0
            btnGo.FlatAppearance.MouseOverBackColor = Color.FromArgb(140, 30, 12)
            btnGo.BackColor = Color.FromArgb(183, 55, 35) : btnGo.ForeColor = Color.White
            btnGo.Font = New System.Drawing.Font("Segoe UI", 13, FontStyle.Bold) : btnGo.Cursor = Cursors.Hand
            AddHandler btnGo.Click, Sub(s, e) DoLogin()
            pR.Controls.Add(btnGo)

            Dim lH As New Label() : lH.Text = "Default credentials:   admin / Admin@123"
            lH.Font = New System.Drawing.Font("Segoe UI", 9) : lH.ForeColor = Color.FromArgb(175, 155, 135)
            lH.Location = New Point(50, 408) : lH.Size = New Size(420, 24) : lH.TextAlign = ContentAlignment.MiddleCenter : pR.Controls.Add(lH)

            Dim lV As New Label() : lV.Text = "Restaurant POS v1.0   |   VB.NET   |   .NET 10   |   Visual Studio 2026"
            lV.Font = New System.Drawing.Font("Segoe UI", 8) : lV.ForeColor = Color.FromArgb(190, 180, 170)
            lV.Location = New Point(50, 482) : lV.Size = New Size(420, 22) : lV.TextAlign = ContentAlignment.MiddleCenter : pR.Controls.Add(lV)
        End Sub

        Private Sub DoLogin()
            lblErr.Text = ""
            Dim uname As String = txtUser.Text.Trim() : Dim pass As String = txtPass.Text
            If String.IsNullOrEmpty(uname) OrElse String.IsNullOrEmpty(pass) Then
                lblErr.Text = "Please enter username and password."
                Return
            End If
            btnGo.Enabled = False : btnGo.Text = "Signing in..."
            Try
                Dim dt As DataTable = DatabaseManager.GetDataTable(
                    "SELECT UserID,Username,PasswordHash,Role,FullName FROM Users WHERE Username=@u AND IsActive=1",
                    New SqlParameter("@u", uname))
                If dt.Rows.Count > 0 Then
                    Dim row As DataRow = dt.Rows(0)
                    If BCrypt.Net.BCrypt.Verify(pass, row("PasswordHash").ToString()) Then
                        DatabaseManager.ExecuteNonQuery("UPDATE Users SET LastLogin=GETDATE() WHERE UserID=@id", New SqlParameter("@id", CInt(row("UserID"))))
                        SessionManager.UserID = CInt(row("UserID"))
                        SessionManager.Username = row("Username").ToString()
                        SessionManager.FullName = row("FullName").ToString()
                        SessionManager.Role = row("Role").ToString()
                        SessionManager.LoginTime = DateTime.Now
                        DatabaseManager.Log(SessionManager.UserID, SessionManager.Username, "LOGIN", "Logged in")
                        Me.DialogResult = DialogResult.OK : Me.Close()
                    Else
                        lblErr.Text = "Incorrect username or password." : txtPass.Clear() : txtPass.Focus()
                    End If
                Else
                    lblErr.Text = "Account not found or inactive."
                End If
            Catch ex As Exception
                lblErr.Text = "Error: " & ex.Message.Substring(0, Math.Min(60, ex.Message.Length))
            Finally
                btnGo.Enabled = True : btnGo.Text = "SIGN IN"
            End Try
        End Sub
    End Class
End Namespace