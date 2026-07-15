Imports System.Data
Imports System.Data.SqlClient
Imports System.Windows.Forms
Imports BCrypt.Net

Namespace RestaurantPOS

    Public Class DatabaseManager

        Private Shared _conn As String =
            "Server=(localdb)\MSSQLLocalDB;Database=RestaurantPOSDB;Integrated Security=True;MultipleActiveResultSets=True;Connection Timeout=60;"

        Public Shared Property ConnectionString As String
            Get
                Return _conn
            End Get
            Set(v As String)
                _conn = v
            End Set
        End Property

        Public Shared Function GetConnection() As SqlConnection
            Return New SqlConnection(_conn)
        End Function

        Public Shared Sub InitializeDatabase()
            ' LocalDB can be slow to start — retry up to 3 times with delay
            Dim maxRetries As Integer = 3
            Dim attempt As Integer = 0
            Dim lastError As String = ""

            While attempt < maxRetries
                attempt += 1
                Try
                    ' First wake up LocalDB instance
                    Dim masterConn As String =
                        "Server=(localdb)\MSSQLLocalDB;Integrated Security=True;Connection Timeout=60;"
                    Using conn As New SqlConnection(masterConn)
                        conn.Open()
                        Dim cmd As New SqlCommand(
                            "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name='RestaurantPOSDB') " &
                            "CREATE DATABASE RestaurantPOSDB", conn)
                        cmd.CommandTimeout = 60
                        cmd.ExecuteNonQuery()
                    End Using
                    ' Small pause to let DB fully initialize
                    System.Threading.Thread.Sleep(500)
                    Using conn As New SqlConnection(_conn)
                        conn.Open()
                        CreateTables(conn)
                        SeedData(conn)
                    End Using
                    Return  ' Success — exit
                Catch ex As Exception
                    lastError = ex.Message
                    If attempt < maxRetries Then
                        ' Wait before retry — LocalDB needs time to start
                        System.Threading.Thread.Sleep(2000)
                    End If
                End Try
            End While

            ' All retries failed
            Dim result As DialogResult = MessageBox.Show(
                $"Could not connect to database after {maxRetries} attempts.{vbCrLf}{vbCrLf}" &
                $"Error: {lastError}{vbCrLf}{vbCrLf}" &
                "The app will open but data won't load. Check that SQL Server LocalDB is installed." &
                $"{vbCrLf}{vbCrLf}Try running:  sqllocaldb start MSSQLLocalDB",
                "Database Connection Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Sub

        Private Shared Sub CreateTables(conn As SqlConnection)
            Dim sql As String = "
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
            CREATE TABLE Users (
                UserID INT IDENTITY(1,1) PRIMARY KEY,
                Username NVARCHAR(100) NOT NULL UNIQUE,
                PasswordHash NVARCHAR(256) NOT NULL,
                Role NVARCHAR(50) NOT NULL DEFAULT 'Staff',
                FullName NVARCHAR(200),
                Email NVARCHAR(200),
                Phone NVARCHAR(50),
                IsActive BIT DEFAULT 1,
                LastLogin DATETIME,
                CreatedDate DATETIME DEFAULT GETDATE()
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MenuCategories' AND xtype='U')
            CREATE TABLE MenuCategories (
                CategoryID INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL,
                Description NVARCHAR(300),
                SortOrder INT DEFAULT 0,
                IsActive BIT DEFAULT 1
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MenuItems' AND xtype='U')
            CREATE TABLE MenuItems (
                ItemID INT IDENTITY(1,1) PRIMARY KEY,
                CategoryID INT,
                Name NVARCHAR(200) NOT NULL,
                Description NVARCHAR(500),
                Price DECIMAL(10,2) NOT NULL DEFAULT 0,
                TaxPercent DECIMAL(5,2) DEFAULT 5,
                IsVeg BIT DEFAULT 1,
                IsAvailable BIT DEFAULT 1,
                CreatedDate DATETIME DEFAULT GETDATE(),
                CONSTRAINT FK_MI_Cat FOREIGN KEY (CategoryID)
                    REFERENCES MenuCategories(CategoryID) ON DELETE SET NULL
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RestaurantTables' AND xtype='U')
            CREATE TABLE RestaurantTables (
                TableID INT IDENTITY(1,1) PRIMARY KEY,
                TableNumber NVARCHAR(20) NOT NULL UNIQUE,
                Capacity INT DEFAULT 4,
                Location NVARCHAR(100),
                Status NVARCHAR(20) DEFAULT 'Available'
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Staff' AND xtype='U')
            CREATE TABLE Staff (
                StaffID INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(200) NOT NULL,
                Phone NVARCHAR(50),
                Email NVARCHAR(200),
                Role NVARCHAR(100),
                Department NVARCHAR(100),
                Salary DECIMAL(10,2) DEFAULT 0,
                JoinDate DATE,
                IsActive BIT DEFAULT 1,
                UserID INT,
                CreatedDate DATETIME DEFAULT GETDATE(),
                CONSTRAINT FK_Staff_Users FOREIGN KEY (UserID)
                    REFERENCES Users(UserID) ON DELETE SET NULL
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
            CREATE TABLE Customers (
                CustomerID INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(200) NOT NULL,
                Phone NVARCHAR(50),
                Email NVARCHAR(200),
                Address NVARCHAR(500),
                LoyaltyPoints INT DEFAULT 0,
                TotalVisits INT DEFAULT 0,
                TotalSpent DECIMAL(10,2) DEFAULT 0,
                CreatedDate DATETIME DEFAULT GETDATE()
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
            CREATE TABLE Orders (
                OrderID INT IDENTITY(1,1) PRIMARY KEY,
                TableID INT,
                CustomerID INT,
                CustomerName NVARCHAR(200),
                OrderType NVARCHAR(50) DEFAULT 'Dine-In',
                Status NVARCHAR(50) DEFAULT 'Open',
                SubTotal DECIMAL(10,2) DEFAULT 0,
                TaxAmount DECIMAL(10,2) DEFAULT 0,
                DiscountPercent DECIMAL(5,2) DEFAULT 0,
                DiscountAmount DECIMAL(10,2) DEFAULT 0,
                TotalAmount DECIMAL(10,2) DEFAULT 0,
                PaymentMode NVARCHAR(50),
                PaidAmount DECIMAL(10,2) DEFAULT 0,
                ChangeAmount DECIMAL(10,2) DEFAULT 0,
                Notes NVARCHAR(500),
                OrderDate DATETIME DEFAULT GETDATE(),
                ClosedDate DATETIME,
                CreatedBy INT,
                CONSTRAINT FK_Orders_Tables FOREIGN KEY (TableID)
                    REFERENCES RestaurantTables(TableID) ON DELETE SET NULL,
                CONSTRAINT FK_Orders_Cust FOREIGN KEY (CustomerID)
                    REFERENCES Customers(CustomerID) ON DELETE SET NULL,
                CONSTRAINT FK_Orders_Users FOREIGN KEY (CreatedBy)
                    REFERENCES Users(UserID) ON DELETE SET NULL
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
            CREATE TABLE OrderItems (
                OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
                OrderID INT NOT NULL,
                ItemID INT,
                ItemName NVARCHAR(200),
                Quantity INT NOT NULL DEFAULT 1,
                UnitPrice DECIMAL(10,2) NOT NULL,
                TaxPercent DECIMAL(5,2) DEFAULT 0,
                TotalPrice DECIMAL(10,2) NOT NULL,
                Notes NVARCHAR(200),
                KOTSent BIT DEFAULT 0,
                CONSTRAINT FK_OI_Orders FOREIGN KEY (OrderID)
                    REFERENCES Orders(OrderID) ON DELETE CASCADE,
                CONSTRAINT FK_OI_Items FOREIGN KEY (ItemID)
                    REFERENCES MenuItems(ItemID) ON DELETE SET NULL
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reservations' AND xtype='U')
            CREATE TABLE Reservations (
                ReservationID INT IDENTITY(1,1) PRIMARY KEY,
                CustomerName NVARCHAR(200) NOT NULL,
                Phone NVARCHAR(50),
                TableID INT,
                GuestCount INT DEFAULT 1,
                ReservationDate DATETIME NOT NULL,
                Status NVARCHAR(50) DEFAULT 'Confirmed',
                Notes NVARCHAR(500),
                CreatedDate DATETIME DEFAULT GETDATE(),
                CreatedBy INT,
                CONSTRAINT FK_Res_Tables FOREIGN KEY (TableID)
                    REFERENCES RestaurantTables(TableID) ON DELETE SET NULL,
                CONSTRAINT FK_Res_Users FOREIGN KEY (CreatedBy)
                    REFERENCES Users(UserID) ON DELETE SET NULL
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Inventory' AND xtype='U')
            CREATE TABLE Inventory (
                InventoryID INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(200) NOT NULL,
                Category NVARCHAR(100),
                Unit NVARCHAR(50),
                CurrentStock DECIMAL(10,2) DEFAULT 0,
                MinStock DECIMAL(10,2) DEFAULT 5,
                CostPerUnit DECIMAL(10,2) DEFAULT 0,
                Supplier NVARCHAR(200),
                LastUpdated DATETIME DEFAULT GETDATE()
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Expenses' AND xtype='U')
            CREATE TABLE Expenses (
                ExpenseID INT IDENTITY(1,1) PRIMARY KEY,
                Category NVARCHAR(100),
                Description NVARCHAR(500),
                Amount DECIMAL(10,2) NOT NULL,
                ExpenseDate DATE DEFAULT CAST(GETDATE() AS DATE),
                PaidTo NVARCHAR(200),
                CreatedBy INT,
                CreatedDate DATETIME DEFAULT GETDATE(),
                CONSTRAINT FK_Exp_Users FOREIGN KEY (CreatedBy)
                    REFERENCES Users(UserID) ON DELETE SET NULL
            );

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ActivityLog' AND xtype='U')
            CREATE TABLE ActivityLog (
                LogID INT IDENTITY(1,1) PRIMARY KEY,
                UserID INT,
                Username NVARCHAR(100),
                Action NVARCHAR(200),
                Details NVARCHAR(1000),
                LogDate DATETIME DEFAULT GETDATE(),
                CONSTRAINT FK_Log_Users FOREIGN KEY (UserID)
                    REFERENCES Users(UserID) ON DELETE SET NULL
            );
            "
            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandTimeout = 120
            cmd.ExecuteNonQuery()
        End Sub

        Private Shared Sub RunCmd(sql As String, conn As SqlConnection)
            Dim cmd As New SqlCommand(sql, conn)
            cmd.CommandTimeout = 60
            cmd.ExecuteNonQuery()
        End Sub

        Private Shared Sub SeedData(conn As SqlConnection)
            Dim chk As New SqlCommand("SELECT COUNT(*) FROM Users WHERE Username='admin'", conn)
            If CInt(chk.ExecuteScalar()) > 0 Then Return

            ' Default users — insert one at a time to avoid inline concat issues
            Dim h1 As String = BCrypt.Net.BCrypt.HashPassword("Admin@123")
            Dim h2 As String = BCrypt.Net.BCrypt.HashPassword("Manager@123")
            Dim h3 As String = BCrypt.Net.BCrypt.HashPassword("Staff@123")

            Dim cmdU As New SqlCommand("INSERT INTO Users(Username,PasswordHash,Role,FullName,Email) VALUES(@u,@h,@r,@fn,@e)", conn)
            cmdU.Parameters.AddWithValue("@u", "admin")
            cmdU.Parameters.AddWithValue("@h", h1)
            cmdU.Parameters.AddWithValue("@r", "Admin")
            cmdU.Parameters.AddWithValue("@fn", "Restaurant Admin")
            cmdU.Parameters.AddWithValue("@e", "admin@res.com")
            cmdU.ExecuteNonQuery()

            cmdU.Parameters("@u").Value = "manager"
            cmdU.Parameters("@h").Value = h2
            cmdU.Parameters("@r").Value = "Manager"
            cmdU.Parameters("@fn").Value = "Floor Manager"
            cmdU.Parameters("@e").Value = "manager@res.com"
            cmdU.ExecuteNonQuery()

            cmdU.Parameters("@u").Value = "staff"
            cmdU.Parameters("@h").Value = h3
            cmdU.Parameters("@r").Value = "Staff"
            cmdU.Parameters("@fn").Value = "Serving Staff"
            cmdU.Parameters("@e").Value = "staff@res.com"
            cmdU.ExecuteNonQuery()

            ' Menu categories
            Dim catSql As String =
                "INSERT INTO MenuCategories(Name,Description,SortOrder) VALUES" &
                "('Starters','Soups, salads and appetizers',1)," &
                "('Main Course','Signature main dishes',2)," &
                "('Breads','Fresh baked breads and rotis',3)," &
                "('Rice & Biryani','Rice preparations',4)," &
                "('Chinese','Indo-Chinese specialties',5)," &
                "('Beverages','Cold drinks and juices',6)," &
                "('Desserts','Sweet endings',7)," &
                "('Combos','Value meal combos',8)"
            RunCmd(catSql, conn)

            ' Menu items — split into smaller batches for VB string limits
            Dim miSql1 As String =
                "INSERT INTO MenuItems(CategoryID,Name,Description,Price,TaxPercent,IsVeg) VALUES" &
                "(1,'Veg Soup','Hot vegetable clear soup',80,5,1)," &
                "(1,'Paneer Tikka','Cottage cheese grilled in tandoor',220,5,1)," &
                "(1,'Chicken Tikka','Tender chicken marinated and grilled',260,5,0)," &
                "(1,'Veg Spring Rolls','Crispy rolls with veg filling',120,5,1)," &
                "(1,'Fish Fingers','Golden fried fish strips',200,5,0)," &
                "(2,'Dal Makhani','Slow cooked black lentils in cream',180,5,1)," &
                "(2,'Paneer Butter Masala','Paneer in rich tomato gravy',220,5,1)"
            RunCmd(miSql1, conn)

            Dim miSql2 As String =
                "INSERT INTO MenuItems(CategoryID,Name,Description,Price,TaxPercent,IsVeg) VALUES" &
                "(2,'Chicken Butter Masala','Chicken in creamy tomato gravy',280,5,0)," &
                "(2,'Mutton Rogan Josh','Tender mutton in Kashmiri spices',340,5,0)," &
                "(2,'Veg Kadai','Mixed vegetables in spiced gravy',170,5,1)," &
                "(3,'Butter Naan','Soft bread brushed with butter',50,5,1)," &
                "(3,'Tandoori Roti','Whole wheat bread from tandoor',40,5,1)," &
                "(3,'Garlic Naan','Naan topped with garlic and butter',60,5,1)," &
                "(4,'Steamed Rice','Plain steamed basmati rice',100,5,1)"
            RunCmd(miSql2, conn)

            Dim miSql3 As String =
                "INSERT INTO MenuItems(CategoryID,Name,Description,Price,TaxPercent,IsVeg) VALUES" &
                "(4,'Chicken Biryani','Aromatic chicken biryani',320,5,0)," &
                "(4,'Veg Biryani','Fragrant vegetable biryani',250,5,1)," &
                "(4,'Mutton Biryani','Royal mutton dum biryani',380,5,0)," &
                "(5,'Veg Fried Rice','Stir fried rice with vegetables',160,5,1)," &
                "(5,'Chicken Manchurian','Chicken in spicy manchurian sauce',220,5,0)," &
                "(5,'Hakka Noodles','Stir fried noodles with veggies',150,5,1)," &
                "(6,'Fresh Lime Soda','Cool lime carbonated drink',60,12,1)"
            RunCmd(miSql3, conn)

            Dim miSql4 As String =
                "INSERT INTO MenuItems(CategoryID,Name,Description,Price,TaxPercent,IsVeg) VALUES" &
                "(6,'Mango Lassi','Thick mango yogurt drink',90,12,1)," &
                "(6,'Cold Coffee','Blended iced coffee',110,12,1)," &
                "(6,'Fresh Juice','Seasonal fruit juice',80,12,1)," &
                "(7,'Gulab Jamun','Soft milk dumplings in syrup',80,5,1)," &
                "(7,'Ice Cream','Two scoops of ice cream',100,5,1)," &
                "(7,'Kheer','Creamy rice pudding',90,5,1)," &
                "(8,'Veg Combo','1 Main + 2 Roti + Rice + Dal',350,5,1)," &
                "(8,'Non-Veg Combo','1 Chicken Main + 2 Naan + Rice',420,5,0)"
            RunCmd(miSql4, conn)

            ' Restaurant tables
            Dim tblSql As String =
                "INSERT INTO RestaurantTables(TableNumber,Capacity,Location) VALUES" &
                "('T01',2,'Indoor'),('T02',2,'Indoor'),('T03',4,'Indoor')," &
                "('T04',4,'Indoor'),('T05',4,'Indoor'),('T06',6,'Indoor')," &
                "('T07',6,'Indoor'),('T08',8,'Indoor'),('T09',4,'Outdoor')," &
                "('T10',4,'Outdoor'),('T11',4,'Outdoor'),('T12',6,'VIP')"
            RunCmd(tblSql, conn)

            ' Customers
            Dim custSql As String =
                "INSERT INTO Customers(Name,Phone,Email,LoyaltyPoints) VALUES" &
                "('Walk-in Guest','','',0)," &
                "('Rahul Sharma','9876543210','rahul@email.com',150)," &
                "('Priya Patel','9812345678','priya@email.com',80)," &
                "('Amit Kumar','9900112233','amit@email.com',200)"
            RunCmd(custSql, conn)

            ' Inventory
            Dim invSql As String =
                "INSERT INTO Inventory(Name,Category,Unit,CurrentStock,MinStock,CostPerUnit) VALUES" &
                "('Basmati Rice','Grains','Kg',50,10,80)," &
                "('Chicken','Meat','Kg',20,5,200)," &
                "('Onion','Vegetables','Kg',30,8,30)," &
                "('Paneer','Dairy','Kg',10,3,300)," &
                "('Cooking Oil','Oils','Liter',15,4,120)," &
                "('Milk','Dairy','Liter',20,5,55)," &
                "('Butter','Dairy','Kg',5,2,400)," &
                "('Tomato','Vegetables','Kg',25,8,40)"
            RunCmd(invSql, conn)

            ' Sample staff
            Dim staffSql As String =
                "INSERT INTO Staff(Name,Phone,Role,Department,Salary,JoinDate,IsActive) VALUES" &
                "('Ravi Kumar','9811111111','Head Chef','Kitchen',35000,'2024-01-15',1)," &
                "('Meena Singh','9822222222','Waiter','Floor',18000,'2024-03-01',1)," &
                "('Suresh Patel','9833333333','Cashier','Billing',20000,'2024-02-10',1)," &
                "('Anjali Sharma','9844444444','Manager','Operations',40000,'2023-12-01',1)"
            RunCmd(staffSql, conn)
        End Sub

        ' Core helpers
        Public Shared Function ExecuteNonQuery(sql As String, ParamArray prms() As SqlParameter) As Integer
            Using conn As New SqlConnection(_conn)
                conn.Open()
                Using cmd As New SqlCommand(sql, conn)
                    If prms IsNot Nothing Then cmd.Parameters.AddRange(prms)
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        End Function

        Public Shared Function ExecuteScalar(sql As String, ParamArray prms() As SqlParameter) As Object
            Using conn As New SqlConnection(_conn)
                conn.Open()
                Using cmd As New SqlCommand(sql, conn)
                    If prms IsNot Nothing Then cmd.Parameters.AddRange(prms)
                    Return cmd.ExecuteScalar()
                End Using
            End Using
        End Function

        Public Shared Function GetDataTable(sql As String, ParamArray prms() As SqlParameter) As DataTable
            Using conn As New SqlConnection(_conn)
                conn.Open()
                Using cmd As New SqlCommand(sql, conn)
                    If prms IsNot Nothing Then cmd.Parameters.AddRange(prms)
                    Dim dt As New DataTable()
                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                    Return dt
                End Using
            End Using
        End Function

        Public Shared Sub Log(userID As Integer, username As String, action As String, details As String)
            Try
                ExecuteNonQuery(
                    "INSERT INTO ActivityLog(UserID,Username,Action,Details) VALUES(@u,@n,@a,@d)",
                    New SqlParameter("@u", userID),
                    New SqlParameter("@n", username),
                    New SqlParameter("@a", action),
                    New SqlParameter("@d", details))
            Catch
            End Try
        End Sub

    End Class

End Namespace
