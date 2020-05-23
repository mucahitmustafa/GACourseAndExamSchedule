using System;
using System.Configuration;
using System.Data.OleDb;
using System.IO;

namespace StatisticsRecorder
{
    public class StatisticsRecorderBase
    {
        private string PATH_EXCEL_OUTPUT_DATA;
        private string FOLDERNAME_STATISTICS = "Statistics";
        private string FILENAME_OUTPUT = "GAScheduleStatistics.xlsx";
        private string _fileName;

        public StatisticsRecorderBase()
        {
            if (!Directory.Exists(FOLDERNAME_STATISTICS)) Directory.CreateDirectory(FOLDERNAME_STATISTICS);

            PATH_EXCEL_OUTPUT_DATA = ConfigurationManager.AppSettings.Get("data.output.location");
            if (!PATH_EXCEL_OUTPUT_DATA.EndsWith("/") && !PATH_EXCEL_OUTPUT_DATA.EndsWith("\\")) PATH_EXCEL_OUTPUT_DATA = PATH_EXCEL_OUTPUT_DATA + "\\";

            DateTime _now = DateTime.Now;
            string _nowDT = _now.ToString().Replace(":", "").Replace(".", "").Replace("/", "");
            _fileName = $"{PATH_EXCEL_OUTPUT_DATA}{FOLDERNAME_STATISTICS}\\statistics-{_nowDT}.xlsx";
            File.Copy($"{PATH_EXCEL_OUTPUT_DATA}{FILENAME_OUTPUT}", _fileName);
        }

        public void InsertData(int time, int generation, float fitness)
        {
            string connection = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + _fileName + "';Extended Properties='Excel 12.0 Xml;HDR=YES;'";
            OleDbConnection con = new OleDbConnection(connection);
            con.Open();

            string Command = $"Insert into [Statistics$]([Time], Generation, Fitness) values('{time}', '{generation}', '{fitness}')";
            OleDbCommand cmd = new OleDbCommand(Command, con);

            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}
