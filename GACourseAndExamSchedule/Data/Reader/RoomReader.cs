using ExcelDataReader;
using GACourseAndExamSchedule.Algorithm;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;

namespace GACourseAndExamSchedule.Data.Reader
{
    public class RoomReader
    {
        #region Constants and Fields

        private string PATH_EXCEL_DATA;
        private string FILENAME_ROOM = ConfigurationManager.AppSettings.Get("data.input.filename.rooms");

        private const string COLUMN_NAME_NAME = "İsim";
        private const string COLUMN_NAME_CAPACITY = "Kapasite";
        private const string COLUMN_NAME_TYPE = "Tür";

        private string _filePath;
        private List<Room> _rooms = new List<Room>();

        private int _nameColumnNumber = 0;
        private int _capacityColumnNumber = 0;
        private int _typeColumnNumber = 0;

        #endregion

        #region Constructors

        public RoomReader()
        {
            PATH_EXCEL_DATA = ConfigurationManager.AppSettings.Get("data.input.location");
           
            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = PATH_EXCEL_DATA + (PATH_EXCEL_DATA.EndsWith("\\") ? "" : "\\") + FILENAME_ROOM;
            }
        }

        #endregion

        #region Public Methods

        public void ResetData()
        {
            if (_rooms != null) _rooms.Clear();
        }

        public List<Room> GetRooms()
        {
            CollectRooms();

            return _rooms;
        }

        public List<Room> GetLabs()
        {
            CollectRooms();

            return _rooms.FindAll(_r => _r.IsLab);
        }

        public Room GetRoomByName(string name)
        {
            CollectRooms();

            return _rooms.Find(_r => _r.Name.Equals(name));
        }

        public Room GetRoomById(int id)
        {
            CollectRooms();

            return _rooms.Find(_r => _r.ID.Equals(id));
        }

        #endregion

        #region Private Methods

        private void CollectRooms()
        {
            if (_rooms != null && _rooms.Count != 0) return;

            using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read()) { }
                    } while (reader.NextResult());

                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = true,
                        FilterSheet = (tableReader, sheetIndex) => true,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                            FilterRow = (rowReader) => {
                                return true;
                            },
                            FilterColumn = (rowReader, columnIndex) => {
                                return true;
                            }
                        }
                    });

                    FindColumnIndexes(result.Tables[0].Columns);

                    int _roomId = 0;
                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        _rooms.Add(new Room
                        (
                            id: _roomId,
                            name: row.ItemArray[_nameColumnNumber].ToString(),
                            capacity: int.Parse(row.ItemArray[_capacityColumnNumber].ToString()),
                            isLab: row.ItemArray[_typeColumnNumber].ToString().Contains("Lab")
                        ));
                        _roomId++;
                    }
                }
            }
        }

        private void FindColumnIndexes(DataColumnCollection columnCollection)
        {
            if ((_nameColumnNumber == _capacityColumnNumber) ||
                (_nameColumnNumber == _typeColumnNumber) ||

                (_capacityColumnNumber == _typeColumnNumber))
            {
                for (int i = 0; i < columnCollection.Count; i++)
                {
                    DataColumn column = columnCollection[i];
                    switch (column.ColumnName)
                    {
                        case COLUMN_NAME_NAME:
                            _nameColumnNumber = i;
                            break;
                        case COLUMN_NAME_CAPACITY:
                            _capacityColumnNumber = i;
                            break;
                        case COLUMN_NAME_TYPE:
                            _typeColumnNumber = i;
                            break;
                    }
                }
            }
        }

        #endregion

    }
}
