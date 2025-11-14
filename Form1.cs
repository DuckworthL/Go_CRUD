using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace FoodOrderApp
{
    public class Form1 : Form
    {
        // Database and UI fields
        private DbConnections db;
        private TextBox txtCust, txtItem, txtQty, txtPrice, txtSearch;
        private Button btnAddOrder, btnUpdate, btnStartBulk, btnAddBulkItem, btnFinalizeBulk, btnDelete, btnSearch, btnClear;
        private DataGridView dgvItems, dgvOrders, dgvOrderDetail;
        private List<OrderItem> bulkOrderItems;
        private int updatingOrderId = -1; // Track selected order for update
        private bool isBulkMode = false;

        public Form1()
        {
            db = new DbConnections();
            InitializeUI();
            LoadOrders();
        }

        // ====== UI Setup ======
        private void InitializeUI()
        {
            this.Text = "Go Food Ordering System";
            this.Size = new Size(620, 650);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            Font labelFont = new Font("Segoe UI", 10);

            // --- Labels and input fields ---
            var lblHeader = new Label()
            {
                Text = "Food Orders",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 14)
            };
            var lblCust = new Label() { Text = "Customer", Location = new Point(20, 54), Size = new Size(70, 26), TextAlign = ContentAlignment.MiddleRight, Font = labelFont };
            var lblItem = new Label() { Text = "Item", Location = new Point(200, 54), Size = new Size(40, 26), TextAlign = ContentAlignment.MiddleRight, Font = labelFont };
            var lblQty = new Label() { Text = "Qty", Location = new Point(330, 54), Size = new Size(30, 26), TextAlign = ContentAlignment.MiddleRight, Font = labelFont };
            var lblPrice = new Label() { Text = "Price", Location = new Point(395, 54), Size = new Size(40, 26), TextAlign = ContentAlignment.MiddleRight, Font = labelFont };

            txtCust = new TextBox() { Location = new Point(90, 54), Size = new Size(100, 26) };
            txtItem = new TextBox() { Location = new Point(240, 54), Size = new Size(80, 26) };
            txtQty = new TextBox() { Location = new Point(365, 54), Size = new Size(30, 26) };
            txtPrice = new TextBox() { Location = new Point(440, 54), Size = new Size(55, 26) };

            // --- CRUD buttons ---
            btnAddOrder = new Button() { Text = "Add", Location = new Point(500, 53), Size = new Size(47, 28), BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button() { Text = "Update", Location = new Point(500, 89), Size = new Size(49, 28), BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnDelete = new Button() { Text = "Delete", Location = new Point(553, 89), Size = new Size(47, 28), BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAddOrder.Click += BtnAddOrder_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;

            // --- Bulk Order Buttons ---
            btnStartBulk = new Button() { Text = "Bulk", Location = new Point(553, 53), Size = new Size(47, 28), BackColor = Color.MediumPurple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddBulkItem = new Button() { Text = "+", Location = new Point(500, 124), Size = new Size(47, 28), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Visible = false };
            btnFinalizeBulk = new Button() { Text = "Done", Location = new Point(553, 124), Size = new Size(47, 28), BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Visible = false };
            btnStartBulk.Click += BtnStartBulk_Click;
            btnAddBulkItem.Click += BtnAddBulkItem_Click;
            btnFinalizeBulk.Click += BtnFinalizeBulk_Click;

            // --- Search controls ---
            txtSearch = new TextBox() { Location = new Point(20, 98), Width = 132 };
            txtSearch.Text = "Search...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Search...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Search..."; txtSearch.ForeColor = Color.Gray; } };
            btnSearch = new Button() { Text = "Search", Location = new Point(157, 98), Size = new Size(54, 27), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClear = new Button() { Text = "Clear", Location = new Point(217, 98), Size = new Size(54, 27), BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += (s, e) => { txtSearch.Text = "Search..."; LoadOrders(); ResetUI(); };

            // --- Data Grids ---
            dgvItems = new DataGridView()
            {
                Location = new Point(20, 135),
                Size = new Size(580, 75),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Visible = false
            };
            dgvOrders = new DataGridView()
            {
                Location = new Point(20, 220),
                Size = new Size(580, 140),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;
            dgvOrderDetail = new DataGridView()
            {
                Location = new Point(20, 370),
                Size = new Size(580, 120),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            this.Controls.Add(lblHeader); this.Controls.Add(lblCust); this.Controls.Add(lblItem); this.Controls.Add(lblQty); this.Controls.Add(lblPrice);
            this.Controls.Add(txtCust); this.Controls.Add(txtItem); this.Controls.Add(txtQty); this.Controls.Add(txtPrice);
            this.Controls.Add(btnAddOrder); this.Controls.Add(btnUpdate); this.Controls.Add(btnDelete);
            this.Controls.Add(btnStartBulk); this.Controls.Add(btnAddBulkItem); this.Controls.Add(btnFinalizeBulk);
            this.Controls.Add(txtSearch); this.Controls.Add(btnSearch); this.Controls.Add(btnClear);
            this.Controls.Add(dgvItems); this.Controls.Add(dgvOrders); this.Controls.Add(dgvOrderDetail);

            // Initial state
            ResetUI();
        }

        // === INSERT: Add new order ===
        private void BtnAddOrder_Click(object sender, EventArgs e)
        {
            if (!IsValidEntry() || isBulkMode) return;
            SqlCommand cmdOrder = new SqlCommand("INSERT INTO Orders (CustomerName, TotalAmount, OrderType, DateOrdered) OUTPUT INSERTED.OrderID VALUES (@c, @amt, 'Simple', @d)");
            decimal total = decimal.Parse(txtPrice.Text) * int.Parse(txtQty.Text);
            cmdOrder.Parameters.AddWithValue("@c", txtCust.Text.Trim());
            cmdOrder.Parameters.AddWithValue("@amt", total);
            cmdOrder.Parameters.AddWithValue("@d", DateTime.Now);
            int orderId = (int)db.ExecuteScalar(cmdOrder);
            SqlCommand cmdItem = new SqlCommand("INSERT INTO OrderItems (OrderID, ItemName, Quantity, Price) VALUES (@oid, @i, @q, @p)");
            cmdItem.Parameters.AddWithValue("@oid", orderId);
            cmdItem.Parameters.AddWithValue("@i", txtItem.Text.Trim());
            cmdItem.Parameters.AddWithValue("@q", int.Parse(txtQty.Text));
            cmdItem.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text));
            db.ExecuteQuery(cmdItem);
            LoadOrders(); ClearInputs(); ResetUI();
        }

        // === UPDATE: Update selected order ===
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (updatingOrderId < 0 || !IsValidEntry() || isBulkMode) return;
            using (SqlConnection conn = new SqlConnection(db.GetConnString()))
            {
                conn.Open();
                SqlCommand updOrder = new SqlCommand("UPDATE Orders SET CustomerName=@c, TotalAmount=@amt WHERE OrderID=@id", conn);
                updOrder.Parameters.AddWithValue("@c", txtCust.Text.Trim());
                updOrder.Parameters.AddWithValue("@amt", decimal.Parse(txtPrice.Text) * int.Parse(txtQty.Text));
                updOrder.Parameters.AddWithValue("@id", updatingOrderId);
                updOrder.ExecuteNonQuery();

                SqlCommand updItem = new SqlCommand("UPDATE OrderItems SET ItemName=@i, Quantity=@q, Price=@p WHERE OrderID=@id", conn);
                updItem.Parameters.AddWithValue("@i", txtItem.Text.Trim());
                updItem.Parameters.AddWithValue("@q", int.Parse(txtQty.Text));
                updItem.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text));
                updItem.Parameters.AddWithValue("@id", updatingOrderId);
                updItem.ExecuteNonQuery();
            }
            LoadOrders(); ClearInputs(); ResetUI();
        }

        // === DELETE: Remove selected ===
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0 || isBulkMode) return;
            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);
            using (SqlConnection conn = new SqlConnection(db.GetConnString()))
            {
                conn.Open();
                SqlCommand delItems = new SqlCommand("DELETE FROM OrderItems WHERE OrderID=@id", conn);
                delItems.Parameters.AddWithValue("@id", orderId);
                delItems.ExecuteNonQuery();
                SqlCommand delOrder = new SqlCommand("DELETE FROM Orders WHERE OrderID=@id", conn);
                delOrder.Parameters.AddWithValue("@id", orderId);
                delOrder.ExecuteNonQuery();
            }
            LoadOrders(); dgvOrderDetail.DataSource = null; ResetUI();
        }

        // === BULK MODE ===
        private void BtnStartBulk_Click(object sender, EventArgs e)
        {
            bulkOrderItems = new List<OrderItem>();
            isBulkMode = true;
            dgvItems.Visible = true;
            btnAddBulkItem.Visible = true;
            btnFinalizeBulk.Visible = true;
            btnAddOrder.Enabled = btnUpdate.Enabled = btnStartBulk.Enabled = false;
            btnDelete.Enabled = false;
            dgvItems.DataSource = null;
            dgvItems.Columns.Clear();
            dgvItems.DataSource = bulkOrderItems;
            dgvItems.Refresh();
        }

        private void BtnAddBulkItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItem.Text) || !int.TryParse(txtQty.Text, out int qty) || qty <= 0 || !decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0) return;
            bulkOrderItems.Add(new OrderItem { ItemName = txtItem.Text.Trim(), Quantity = qty, Price = price });
            dgvItems.DataSource = null;
            dgvItems.DataSource = bulkOrderItems;
            dgvItems.Refresh();
            txtItem.Clear(); txtQty.Clear(); txtPrice.Clear();
        }

        private void BtnFinalizeBulk_Click(object sender, EventArgs e)
        {
            if (bulkOrderItems.Count == 0 || string.IsNullOrWhiteSpace(txtCust.Text)) return;
            decimal total = 0;
            foreach (var it in bulkOrderItems) total += it.Quantity * it.Price;
            SqlCommand cmdOrder = new SqlCommand("INSERT INTO Orders (CustomerName, TotalAmount, OrderType, DateOrdered) OUTPUT INSERTED.OrderID VALUES (@c, @amt, 'Bulk', @d)");
            cmdOrder.Parameters.AddWithValue("@c", txtCust.Text.Trim());
            cmdOrder.Parameters.AddWithValue("@amt", total);
            cmdOrder.Parameters.AddWithValue("@d", DateTime.Now);
            int orderId = (int)db.ExecuteScalar(cmdOrder);
            foreach (var it in bulkOrderItems)
            {
                SqlCommand cmdItem = new SqlCommand("INSERT INTO OrderItems (OrderID, ItemName, Quantity, Price) VALUES (@oid, @i, @q, @p)");
                cmdItem.Parameters.AddWithValue("@oid", orderId);
                cmdItem.Parameters.AddWithValue("@i", it.ItemName);
                cmdItem.Parameters.AddWithValue("@q", it.Quantity);
                cmdItem.Parameters.AddWithValue("@p", it.Price);
                db.ExecuteQuery(cmdItem);
            }
            bulkOrderItems.Clear();
            dgvItems.Visible = btnAddBulkItem.Visible = btnFinalizeBulk.Visible = false;
            isBulkMode = false;
            ResetUI();
            LoadOrders(); ClearInputs();
        }

        // === SEARCH ===
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (isBulkMode) return;
            string search = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(search) || search == "Search...") { LoadOrders(); ResetUI(); return; }
            DataTable dt = new DataTable();
            string sql = "SELECT OrderID, CustomerName, TotalAmount, OrderType, DateOrdered FROM Orders WHERE CustomerName LIKE @q OR OrderType LIKE @q OR OrderID IN (SELECT OrderID FROM OrderItems WHERE ItemName LIKE @q)";
            using (SqlConnection conn = new SqlConnection(db.GetConnString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + search + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            dgvOrders.DataSource = dt;
            dgvOrderDetail.DataSource = null;
            ResetUI();
        }

        // === VIEW/SELECT for UPDATE ===
        private void DgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0 || isBulkMode)
            {
                dgvOrderDetail.DataSource = null;
                ResetUI();
                return;
            }
            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);
            DataTable dtItems = new DataTable();
            using (SqlConnection conn = new SqlConnection(db.GetConnString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ItemName, Quantity, Price FROM OrderItems WHERE OrderID=@id", conn);
                cmd.Parameters.AddWithValue("@id", orderId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dtItems);
            }
            dgvOrderDetail.DataSource = dtItems;
            // Fill update controls
            txtCust.Text = dgvOrders.SelectedRows[0].Cells["CustomerName"].Value?.ToString() ?? "";
            txtItem.Text = dtItems.Rows.Count > 0 ? dtItems.Rows[0]["ItemName"].ToString() : "";
            txtQty.Text = dtItems.Rows.Count > 0 ? dtItems.Rows[0]["Quantity"].ToString() : "";
            txtPrice.Text = dtItems.Rows.Count > 0 ? dtItems.Rows[0]["Price"].ToString() : "";

            updatingOrderId = orderId;
            btnAddOrder.Enabled = false;
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
        }

        // === UTILITIES ===
        private bool IsValidEntry()
        {
            if (string.IsNullOrWhiteSpace(txtCust.Text) || string.IsNullOrWhiteSpace(txtItem.Text) ||
                !int.TryParse(txtQty.Text, out int qty) || qty <= 0 ||
                !decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0) return false;
            return true;
        }
        private void LoadOrders()
        {
            DataTable dt = new DataTable();
            db.FillData("SELECT OrderID, CustomerName, TotalAmount, OrderType, DateOrdered FROM Orders", dt);
            dgvOrders.DataSource = dt;
            dgvOrderDetail.DataSource = null;
        }
        private void ClearInputs()
        {
            txtCust.Clear(); txtItem.Clear(); txtQty.Clear(); txtPrice.Clear();
        }
        private void ResetUI()
        {
            // Restore default state after any operation
            updatingOrderId = -1;
            isBulkMode = false;
            btnAddOrder.Enabled = true;
            btnUpdate.Enabled = false;
            btnStartBulk.Enabled = true;
            btnDelete.Enabled = true;
            dgvItems.Visible = btnAddBulkItem.Visible = btnFinalizeBulk.Visible = false;
        }

        // Helper class for order item
        public class OrderItem
        {
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
    }
}
