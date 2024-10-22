using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
                "email Address"
            };
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string rootDirectory = @"C:\Users\anuja\Downloads\MCDA5510_Assignments-master\MCDA5510_Assignments-master\Sample Data\";
            if (Directory.Exists(rootDirectory))
            {
                 
                Console.WriteLine("Walking directory!");
                DirWalker walker = new DirWalker();
                walker.Walk(rootDirectory); 
                
                stopwatch.Stop();

                // Print the total execution time
                Console.WriteLine($"\nTotal execution time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine("Total Number of Valid Rows: " + validRowsCount);
                Console.WriteLine("Total number of Skipped Rows: " + skippedRowsCount);
            }
            else
            {
                Console.WriteLine($"Directory {rootDirectory} does not exist.");
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
                        Console.WriteLine("Dir:" + dirpath);
                        Walk(dirpath);
                        
                    }
                }
                string[] fileList = Directory.GetFiles(dirPath, "*.csv"); //Filtering only CSV files
                foreach (string filepath in fileList)
                {
                    Console.WriteLine("File:" + filepath);
                    ProcessCsv(filepath,dirPath);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The file or directory cannot be found.");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("The file or directory cannot be found.");
            }
            catch (DriveNotFoundException)
            {
                Console.WriteLine("The drive specified in 'path' is invalid.");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine("'path' exceeds the maximum supported path length.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("You do not have permission to create this file.");
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
            {
                Console.WriteLine("There is a sharing violation.");
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 80)
            {
                Console.WriteLine("The file already exists.");
            }
            catch (IOException e)
            {
                Console.WriteLine($"An exception occurred:\nError code: " +
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
            
            try
            {
                string[] yearDir = dirPath.Split(Path.DirectorySeparatorChar);

                string year = yearDir[yearDir.Length - 3];
                string month = yearDir[yearDir.Length - 2].PadLeft(2, '0'); 
                string day = yearDir[yearDir.Length - 1].PadLeft(2, '0');   

                // Format into yyyy/mm/dd
                string formattedDate = $"{year}/{month}/{day}";

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true, // CSVHelper configuration
                })) 
                {
                    var records = csv.GetRecords<dynamic>().ToList();
                    foreach (var record in records)
                    {
                        var recordDict = (IDictionary<string, object>)record;
                        bool hasMissingData = false;

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
                            skippedRowsCount++;
                            continue; 
                        }


                        Console.Write($"{recordDict["First Name"]}, {recordDict["Last Name"]}, {recordDict["Street Number"]} {recordDict["Street"]}, {recordDict["City"]}, {recordDict["Province"]}, {recordDict["Country"]}, {recordDict["Postal Code"]}, {recordDict["Phone Number"]}, {recordDict["email Address"]}");
                        Console.WriteLine($", {formattedDate}");
                        validRowsCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while processing the CSV file: {e.Message}");
            }
        }


     
    }
 
}