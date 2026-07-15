Imports System.Windows.Forms

Module Program

    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        RestaurantPOS.DatabaseManager.InitializeDatabase()
        Dim login As New RestaurantPOS.FrmLogin()
        If login.ShowDialog() = DialogResult.OK Then
            Application.Run(New RestaurantPOS.FrmDashboard())
        End If
    End Sub

End Module
