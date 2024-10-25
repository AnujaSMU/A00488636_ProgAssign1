using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using CsvHelper;
using CsvHelper.Configuration;
namespace ProgAssign1
{
    class DirWalker
    {
        static int validRowsCount = 0;
        static int skippedRowsCount = 0;

        static List<string> dataColumns = new List<string>
            {
                "First Name",
                "Last Name",
                "Street Number",
                "Street",
                "City",
                "Province",
                "Country",
                "Postal Code",
                "Phone Number",
                "email Address",
                "Date"
            };
        private static CsvHelper.CsvWriter _csvWriter;
        private static StreamWriter _logger;

        public static void Main(string[] args)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            string rootDirectory = @"C:\Users\anuja\Downloads\MCDA5510_Assignments-master\MCDA5510_Assignments-master\Sample Data\";
            if (Directory.Exists(rootDirectory))
            {
                
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string outputFilePath = Path.Combine(projectRoot, "Output", "Output.csv");
                string logFilePath = Path.Combine(projectRoot, "logs", "log.txt");

                _logger = new StreamWriter(logFilePath);

                var writer = new StreamWriter(outputFilePath);
                _csvWriter = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
                
                _csvWriter.WriteField(dataColumns); // Write the header
                dataColumns.Remove("Date"); // Remove Date to use dataColumns for checking missing data
                _csvWriter.NextRecord();

                Console.WriteLine("Walking directory!");
                DirWalker walker = new DirWalker();
                walker.Walk(rootDirectory);

                stopwatch.Stop();

                Console.WriteLine($"\nTotal execution time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine("Total Number of Valid Rows: " + validRowsCount);
                Console.WriteLine("Total number of Skipped Rows: " + skippedRowsCount);

                
                _logger.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds} ms");
                _logger.WriteLine("Total Number of Valid Rows: " + validRowsCount);
                _logger.WriteLine("Total number of Skipped Rows: " + skippedRowsCount);

                _logger.Close();
                _csvWriter.Dispose();

            }
            else
            {
                _logger.WriteLine($"Directory {rootDirectory} does not exist.");
            }
        }

        public void Walk(string dirPath)
        {
            try
            {
                string[] list = Directory.GetDirectories(dirPath);

                if (list == null) return;

                foreach (string dirpath in list)
                {
                    if (Directory.Exists(dirpath))
                    {
                        _logger.WriteLine("Dir:" + dirpath);
                        Walk(dirpath);
                        
                    }
                }
                string[] fileList = Directory.GetFiles(dirPath, "*.csv"); //Filtering only CSV files
                foreach (string filepath in fileList)
                {
                    _logger.WriteLine("File:" + filepath);
                    ProcessCsv(filepath,dirPath);
                }
            }
            catch (FileNotFoundException)
            {
                _logger.WriteLine("The file or directory cannot be found.");
            }
            catch (DirectoryNotFoundException)
            {
                _logger.WriteLine("The file or directory cannot be found.");
            }
            catch (DriveNotFoundException)
            {
                _logger.WriteLine("The drive specified in 'path' is invalid.");
            }
            catch (PathTooLongException)
            {
                _logger.WriteLine("'path' exceeds the maximum supported path length.");
            }
            catch (UnauthorizedAccessException)
            {
                _logger.WriteLine("You do not have permission to create this file.");
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
            {
                _logger.WriteLine("There is a sharing violation.");
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 80)
            {
                _logger.WriteLine("The file already exists.");
            }
            catch (IOException e)
            {
                _logger.WriteLine($"An exception occurred:\nError code: " +
                                  $"{e.HResult & 0x0000FFFF}\nMessage: {e.Message}");
            }
        }

        private static void ProcessCsv(string filePath, string dirPath)
        {
            //if (!filePath.EndsWith(".csv"))
            //{
            //    Console.WriteLine("File is not a CSV file: " + filePath);
            //    return;
            //}
            string[] yearDir = dirPath.Split(Path.DirectorySeparatorChar);
            string year = yearDir[yearDir.Length - 3];
            string month = yearDir[yearDir.Length - 2].PadLeft(2, '0'); 
            string day = yearDir[yearDir.Length - 1].PadLeft(2, '0');   

            // Format into yyyy/mm/dd
            string formattedDate = $"{year}/{month}/{day}";
            string fileName = Path.GetFileName(filePath);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // CSVHelper configuration
            }))

            {
                int Count = 0;
                var records = csv.GetRecords<dynamic>().ToList();

                foreach (var record in records)
                {
                    var recordDict = (IDictionary<string, object>)record;
                    bool hasMissingData = false;

                    Count++;
                    foreach (var column in dataColumns)
                    {
                        if (!recordDict.ContainsKey(column) || string.IsNullOrEmpty(recordDict[column]?.ToString()))
                        {

                            hasMissingData = true; 
                            break;
                        }
                    }
                    

                    if (hasMissingData)
                    {
                        _logger.WriteLine($"In file {formattedDate}/{fileName} Row {Count} has missing data. Skipping row.");
                        skippedRowsCount++;
                        continue; 
                    }
                    recordDict["Date"] = formattedDate;
                    _csvWriter.WriteRecord(recordDict);
                    _csvWriter.NextRecord();

                    validRowsCount++;
                }
            }

            _logger.WriteLine($"Processed {formattedDate}/{fileName}");
        }

     
    }
 
}