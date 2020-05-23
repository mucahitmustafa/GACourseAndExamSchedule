using System.Windows.Forms;

namespace GACourseAndExamSchedule.SpannedDataGrid
{
    interface ISpannedCell
    {
        int ColumnSpan { get; }
        int RowSpan { get; }
        DataGridViewCell OwnerCell { get; }
    }
}
