using ASP.NET_Core_MVC_Playground.Data;
using ASP.NET_Core_MVC_Playground.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.IO;
using System.Linq;

namespace ASP.NET_Core_MVC_Playground.Controllers
{
    public class SpreadsheetController : Controller
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;

        public SpreadsheetController(DataDbContext db, ILogger<SpreadsheetController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult UploadSellers()
        {
            ViewBag.Status = TempData["Message"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadSellers(OwnerSpreadsheetViewModel model)
        {
            using (var stream = new MemoryStream())
            {
                model.SpreadsheetFile.CopyTo(stream);
                stream.Position = 0;

                IExcelDataReader reader;
                ExcelDataSetConfiguration configuration = new()
                {
                    // Gets or sets a callback to obtain configuration options for a DataTable. 
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                    {
                        // Gets or sets a value indicating whether to use a row from the 
                        // data as column names.
                        UseHeaderRow = true,
                    }
                };

                if (model.SpreadsheetFile.FileName.Split(".")[1] != "csv")
                {
                    // Auto-detect format, supports:
                    //  - Binary Excel files (2.0-2003 format; *.xls)
                    //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                    reader = ExcelReaderFactory.CreateReader(stream);
                    using (reader)
                    {
                        var dataset = reader.AsDataSet(configuration);

                        // Using DataSet
                        processSellersSpreadsheetAsDataSet(dataset.Tables[0]);

                        // Using Reader
                        // processSellersSpreadsheet(reader);
                    }
                }
                else
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                    using (reader)
                    {
                        var dataset = reader.AsDataSet(configuration);
      
                        // Using DataSet
                        processSellersSpreadsheetAsDataSet(dataset.Tables[0]);

                        // Using Reader
                        //processSellersSpreadsheet(reader);
                    }
                }
            }
            return View();
        }

        private void processSellersSpreadsheetAsDataSet(DataTable table)
        {
            if (table.Columns.Count != 4)
            {
                TempData["Message"] = "Incorrect number of columns!";
                return;
            }
            else if (table.Rows.Count <= 1)
            {
                TempData["Message"] = "No owner information provided!";
                return;
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        string firstName = row["FirstName"].ToString();
                        string lastName = row["LastName"].ToString();
                        int phoneNumber = int.Parse(row["PhoneNumber"].ToString());
                        string email = row["Email"].ToString();

                        Seller seller = (from owners in _db.Sellers
                                       where owners.Email == email
                                       select owners).FirstOrDefault();
                        if (seller != null)
                        {
                            seller.FirstName = firstName;
                            seller.LastName = lastName;
                            seller.PhoneNumber = phoneNumber;
                            seller.Email = email;

                            _db.Sellers.Update(seller);
                        }
                        else
                        {
                            Seller newSeller = new()
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                PhoneNumber = phoneNumber,
                                Email = email,
                            };

                            _db.Sellers.Add(newSeller);
                        }
                    }
                    catch (System.Exception)
                    {
                        TempData["Message"] = "Incorrectly formatted columns. Fix them and try again!";
                        return;
                    }

                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (System.Exception)
                    {
                        TempData["Message"] = "Database error";
                        return;
                    }
                }
                TempData["Message"] = "Owner upload complete!";
            }
        }

        private void processSellersSpreadsheet(IExcelDataReader reader)
        {
            if (reader.FieldCount != 4)
            {
                TempData["Message"] = "Incorrect number of columns!";
                return;
            }
            else if (reader.RowCount <= 1)
            {
                TempData["Message"] = "No owner information provided!";
                return;
            }
            else
            {
                int currentRow = 0;
                while (reader.Read())
                {
                    if (currentRow == 0)
                    {
                        // Skip it. Column Names
                        currentRow++;
                    }
                    else
                    {
                        try
                        {
                            string firstName = reader.GetValue(0).ToString();
                            string lastName = reader.GetValue(1).ToString();
                            int phoneNumber = int.Parse(reader.GetValue(2).ToString());
                            string email = reader.GetValue(3).ToString();

                            Seller seller = (from owners in _db.Sellers
                                                      where owners.Email == email
                                                      select owners).FirstOrDefault();
                            if (seller != null)
                            {
                                seller.FirstName = firstName;
                                seller.LastName = lastName;
                                seller.PhoneNumber = phoneNumber;
                                seller.Email = email;

                                _db.Sellers.Update(seller);
                            }
                            else
                            {
                                Seller newSeller = new()
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    PhoneNumber = phoneNumber,
                                    Email = email,
                                };

                                _db.Sellers.Add(newSeller);
                            }
                        }
                        catch (System.Exception)
                        {
                            TempData["Message"] = "Incorrectly formatted columns. Fix them and try again!";
                            return;
                        }

                        try
                        {
                            _db.SaveChanges();
                        }
                        catch (System.Exception)
                        {
                            TempData["Message"] = "Database error";
                            return;
                        }
                        currentRow++;
                    }
                }
                TempData["Message"] = "Seller upload complete!";
            }
        }
    }
}
