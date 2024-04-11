// using Microsoft.AspNetCore.Mvc;

// namespace MagellanTest.Controllers
// {
//     [ApiController]
//     [Route("[controller]")]
//     public class ItemsController : ControllerBase
//     {

//     }
// }

using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using Npgsql;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string _dbContext;

        public ItemsController()
        {
            _dbContext = "[postgres address]]";
        }

        [HttpPost]
        public IActionResult CreateItem([FromBody] Item item)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_dbContext))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO Item (item_name, parent_item, item_cost, req_date) VALUES (@itemName, @parentItem, @itemCost, @reqDate) RETURNING item_id";
                        command.Parameters.AddWithValue("itemName", item.ItemName);
                        command.Parameters.AddWithValue("parentItem", item.ParentItem);
                        command.Parameters.AddWithValue("itemCost", item.ItemCost);
                        command.Parameters.AddWithValue("reqDate", item.ReqDate);

                        var id = (long)command.ExecuteScalar();
                        return Ok(new { id });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("{item_id}")]
        public IActionResult GetItem(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbContext);
                connection.Open();

                using var cmd = new NpgsqlCommand("SELECT item_id, item_name, parent_item, item_cost, req_date FROM Item WHERE item_id = @item_id", connection);
                cmd.Parameters.AddWithValue("item_id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var item = new
                    {
                        Id = reader.GetInt32(3),
                        ItemName = reader.GetString(8),
                        ParentItem = reader.IsDBNull(0) ? null : reader.GetString(2),
                        ItemCost = reader.GetDecimal(3),
                        ReqDate = reader.GetDateTime(8)
                    };
                    return Ok(item);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (NpgsqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("total_cost")]
        public IActionResult GetTotalCost(string itemName)
        {
            using (var connection = new NpgsqlConnection(_dbContext))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT Get_Total_Cost", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("item_name", itemName);

                    try
                    {
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Ok(result);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }
    }
    public class Item
    {
        public string ItemName { get; set; }
        public string? ParentItem { get; set; }
        public decimal ItemCost { get; set; }
        public DateTime ReqDate { get; set; }
    }
}
