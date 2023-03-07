using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            checkUserRow();
        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.AllowUserToAddRows != false)
            {
                double[] xy = getXYPos(e);
                int idxClick = dataGridView1.Rows.Add(new object[]{ Math.Round(xy[0],2), Math.Round(xy[1],2) });
                
                checkUserRow();
                checkCell(idxClick);
            }
        }

        RectangleF ChartAreaClientRectangle(Chart chart, ChartArea CA)
        {
            RectangleF CAR = CA.Position.ToRectangleF();
            float pw = chart.ClientSize.Width / 100f;
            float ph = chart.ClientSize.Height / 100f;
            return new RectangleF(pw * CAR.X, ph * CAR.Y, pw * CAR.Width, ph * CAR.Height);
        }

        RectangleF InnerPlotPositionClientRectangle(Chart chart, ChartArea CA)
        {
            RectangleF IPP = CA.InnerPlotPosition.ToRectangleF();
            RectangleF CArp = ChartAreaClientRectangle(chart, CA);

            float pw = CArp.Width / 100f;
            float ph = CArp.Height / 100f;

            return new RectangleF(CArp.X + pw * IPP.X, CArp.Y + ph * IPP.Y,
                                    pw * IPP.Width, ph * IPP.Height);
        }

        ToolTip tt = null;
        Point tl = Point.Empty;

        private double[] getXYPos(MouseEventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];

            Axis ax = ca.AxisX;
            Axis ay = ca.AxisY;
            double x = ax.PixelPositionToValue(e.X);
            double y = ay.PixelPositionToValue(e.Y);
            return new double[]{x,y};
        }
        private void ChartMoveCursor(object sender, MouseEventArgs e)
        {

            if (tt == null) tt = new ToolTip();

            ChartArea ca = chart1.ChartAreas[0];


            if (InnerPlotPositionClientRectangle(chart1, ca).Contains(e.Location))
            {
                double[] pos = getXYPos(e);
                if (e.Location != tl)
                    tt.SetToolTip(chart1, string.Format("X={0:0.000} ; Y={1:0.00}", pos[0], pos[1]));

                //tl = e.Location;
            }
            else tt.Hide(chart1);
        }

        private void bindData2Chart()
        {
            // Set the X and Y values for the chart
            chart1.Series["Series1"].Points.DataBindXY(dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => !r.Cells.Cast<DataGridViewCell>().All(c => c.Value == null)).Select(r => Convert.ToDouble(r.Cells[0].Value)).ToList(),
                                                         dataGridView1.Rows.Cast<DataGridViewRow>().Where(r => !r.Cells.Cast<DataGridViewCell>().All(c => c.Value == null)).Select(r => Convert.ToDouble(r.Cells[1].Value)).ToList());
            
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            bindData2Chart();
        }

        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            checkUserRow();
        }

        private void checkUserRow()
        {
            if (dataGridView1.Rows.Count > 5)
                dataGridView1.AllowUserToAddRows = false;
            else
                dataGridView1.AllowUserToAddRows = true;
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            checkUserRow();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Check if the changed cell belongs to a data row (not the header row)
            {
                // Get the values of the changed row
                bindData2Chart();
                checkCell(e.RowIndex);
            }
        }
        private void checkCell(int rowIndex)
        {
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            DataGridViewCell cell = row.Cells[0];

            // Get the values of the previous and next cells in the same column
            object prevValue = rowIndex > 0 ? dataGridView1.Rows[rowIndex - 1].Cells[0].Value : null;
            object nextValue = (rowIndex < dataGridView1.Rows.Count - 1) ? dataGridView1.Rows[rowIndex + 1].Cells[0].Value : null;
            double dprevVal = prevValue == null ? double.NaN : Convert.ToDouble(prevValue);
            double dnextVal = nextValue == null ? double.NaN : Convert.ToDouble(nextValue);

            if (dprevVal.Equals(double.NaN) && rowIndex != 0)
            {
                MessageBox.Show("Fill any blank X values");
                return;
            }

            double currVal;
            if (double.TryParse(cell.Value.ToString(), out currVal))
            {
                if (currVal <= dprevVal)
                {
                    MessageBox.Show("The Value must be higher then previous value");
                    dataGridView1.Rows.RemoveAt(rowIndex);
                    // The value is between the values of the previous and next cells, do something...
                }
                else if (currVal >= dnextVal || dnextVal.Equals(double.NaN) && nextValue != null)
                {
                    MessageBox.Show("The Value must be lower then next value");
                    dataGridView1.Rows.RemoveAt(rowIndex);

                }
            }
            bindData2Chart();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
