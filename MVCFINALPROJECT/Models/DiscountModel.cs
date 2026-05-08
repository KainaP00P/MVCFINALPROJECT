using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the Discounts table
    public class DiscountModel
    {
        // Properties match the columns in the Discounts table
        public int DiscountID { get; set; }
        public string DiscountName { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }  // fixed peso amount
        public decimal DiscountPercent { get; set; } // or percentage based
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // -------------------------------------------------------
        // Get all discounts (active and inactive)
        // -------------------------------------------------------
        public static DataTable GetAllDiscounts()
        {
            string query = "SELECT * FROM Discounts ORDER BY DiscountName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get only active discounts (used in dropdowns)
        // -------------------------------------------------------
        public static DataTable GetActiveDiscounts()
        {
            string query = @"SELECT * FROM Discounts 
                             WHERE IsActive = 1 
                             ORDER BY DiscountName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get a single discount by ID
        // -------------------------------------------------------
        public static DataTable GetDiscountByID(int discountID)
        {
            string query = "SELECT * FROM Discounts WHERE DiscountID = @DiscountID";
            SqlParameter[] param = {
                new SqlParameter("@DiscountID", discountID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new discount
        // -------------------------------------------------------
        public static int InsertDiscount(DiscountModel d)
        {
            string query = @"INSERT INTO Discounts
                            (DiscountName, Description, DiscountAmount, 
                             DiscountPercent, IsActive)
                             VALUES
                            (@DiscountName, @Description, @DiscountAmount,
                             @DiscountPercent, @IsActive)";

            SqlParameter[] param = {
                new SqlParameter("@DiscountName", d.DiscountName),
                new SqlParameter("@Description", (object)d.Description ?? DBNull.Value),
                // If no fixed amount entered, store 0
                new SqlParameter("@DiscountAmount", d.DiscountAmount),
                // If no percent entered, store 0
                new SqlParameter("@DiscountPercent", d.DiscountPercent),
                new SqlParameter("@IsActive", d.IsActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Update an existing discount
        // -------------------------------------------------------
        public static int UpdateDiscount(DiscountModel d)
        {
            string query = @"UPDATE Discounts SET
                             DiscountName = @DiscountName,
                             Description = @Description,
                             DiscountAmount = @DiscountAmount,
                             DiscountPercent = @DiscountPercent,
                             IsActive = @IsActive
                             WHERE DiscountID = @DiscountID";

            SqlParameter[] param = {
                new SqlParameter("@DiscountID", d.DiscountID),
                new SqlParameter("@DiscountName", d.DiscountName),
                new SqlParameter("@Description", (object)d.Description ?? DBNull.Value),
                new SqlParameter("@DiscountAmount", d.DiscountAmount),
                new SqlParameter("@DiscountPercent", d.DiscountPercent),
                new SqlParameter("@IsActive", d.IsActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Delete a discount by ID
        // -------------------------------------------------------
        public static int DeleteDiscount(int discountID)
        {
            string query = "DELETE FROM Discounts WHERE DiscountID = @DiscountID";
            SqlParameter[] param = {
                new SqlParameter("@DiscountID", discountID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Check if discount name already exists (for validation)
        // -------------------------------------------------------
        public static bool DiscountNameExists(string name, int excludeID = 0)
        {
            // excludeID is used during UPDATE to ignore the current record
            string query = @"SELECT COUNT(*) FROM Discounts
                             WHERE DiscountName = @DiscountName
                             AND DiscountID != @ExcludeID";
            SqlParameter[] param = {
                new SqlParameter("@DiscountName", name),
                new SqlParameter("@ExcludeID", excludeID)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0;
        }

        // -------------------------------------------------------
        // Compute actual deduction amount based on type
        // Used during fee assessment computation
        // -------------------------------------------------------
        public static decimal ComputeDeduction(DiscountModel d, decimal totalFee)
        {
            if (d.DiscountPercent > 0)
            {
                // Percentage-based: compute from total fee
                return (d.DiscountPercent / 100) * totalFee;
            }
            else
            {
                // Fixed amount discount
                return d.DiscountAmount;
            }
        }
    }
}