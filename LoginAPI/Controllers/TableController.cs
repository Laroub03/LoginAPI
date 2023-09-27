using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        // Static list to simulate in-memory storage of table data
        private static List<TableData> _tableData = new List<TableData>
        {
            // Assuming you have some initial data here
        };

        [HttpPost("edit")]
        [Authorize(Roles = "Editor1, Editor2")] // Restrict access to Editor1 and Editor2 roles
        public ActionResult<TableData> Edit(TableData editedData)
        {
            // Find the existing data entry by Id
            var existingData = _tableData.FirstOrDefault(data => data.Id == editedData.Id);
            if (existingData == null)
            {
                // If not found, return a NotFound response
                return NotFound("Data not found");
            }

            // Update the existing data entry with the edited values
            existingData.Model = editedData.Model;
            existingData.Amount = editedData.Amount;
            existingData.Change = editedData.Change;

            // Return the updated data entry
            return Ok(existingData);
        }
    }

    public class TableData
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public double Amount { get; set; }
        public string Change { get; set; }
    }
}
