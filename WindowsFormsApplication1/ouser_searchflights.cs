using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1.Global;
using WindowsFormsApplication1.Usercontrol;

namespace WindowsFormsApplication1
{
    public partial class ouser_searchflights : Form
    {
        string sql;
        DataTable dt;
        DataTable rst = new DataTable();
        List<FlightInfo> list = new List<FlightInfo>();
        string dep, arr, deptime, flttp;
        int cid;
        ArrayList routeID = new ArrayList();
        string[] depap, arrap;
        ArrayList scheduleID = new ArrayList();

        public ouser_searchflights()
        {
            InitializeComponent();
            //City
            sql = "select CityCode, CityName from City order by CityName";
            dt = SqlHelper.ExecuteDataTable(sql, null);
            cbDEPct.DataSource = dt;
            cbDEPct.DisplayMember = "CityName";
            cbDEPct.ValueMember = "CityCode";
            cbDEPct.SelectedIndex = 0;
            //dt.copy()与dt是不同的数据源
            cbARRct.DataSource = dt.Copy();
            cbARRct.DisplayMember = "CityName";
            cbARRct.ValueMember = "CityCode";
            cbARRct.SelectedIndex = 0;

            // CabinType
            sql = "select * from CabinType";
            dt = SqlHelper.ExecuteDataTable(sql, null);
            cbCabin.DataSource = dt;
            cbCabin.DisplayMember = "CabinTypeName";
            cbCabin.ValueMember = "CabinTypeId";
            cid = (int)cbCabin.SelectedValue;

            //FlightType
            cbFLT.SelectedIndex = 0;
            flttp = cbFLT.Text;

            //date
            dateDEP.MinDate = DateTime.Today;
            dateRTN.MinDate = dateDEP.MinDate;

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void ouser_searchflights_Load(object sender, EventArgs e)
        {

        }

        private void grpways_Enter(object sender, EventArgs e)
        {

        }

        private void radioOneWay_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //查询条件
            dep = cbDEPct.SelectedValue.ToString();
            arr = cbARRct.SelectedValue.ToString();
            deptime = dateDEP.Text; //arrtime = dateRTN.Text;

            //查询test
            deptime = "2019-10-1";

            //查询结果
            //通过city查机场
            depap = airport(dep, depap);
            arrap = airport(arr, arrap);

            //通过机场查航线
            foreach (string ap1 in depap) {
                foreach (string ap2 in arrap) {
                    route(ap1, ap2);
                }
            }

            //通过航线查计划
            string sql_rid = "";
            foreach (int rid in routeID) {
                sql_rid += " RouteId = " + rid.ToString() + " or";
                //schedule(rid);
            }
            sql_rid = sql_rid.TrimEnd("or".ToCharArray());
            schedule(sql_rid);

            //通过计划查飞机各舱位余量 
            //param:scheduleid
            rst.Columns.Add(new DataColumn("First", Type.GetType("System.Int32")));
            rst.Columns.Add(new DataColumn("Business", Type.GetType("System.Int32")));
            rst.Columns.Add(new DataColumn("Economy", Type.GetType("System.Int32")));
            foreach (int sid in scheduleID)
            {
                airplane(sid);
            }

            //rst中 为每条记录指定出发地到达地到达时间
            rst.Columns.Add(new DataColumn("Depap", Type.GetType("System.String")));
            rst.Columns.Add(new DataColumn("Arrap", Type.GetType("System.String")));
            rst.Columns.Add(new DataColumn("FLTtime", Type.GetType("System.Int32")));
            city();


            //遍历DataTable:rst 创建记录列表
            traverseDataTable();

        }

        private void flightOneWay1_Load(object sender, EventArgs e)
        {

        }

        public string[] airport(string city, string[] aprs)
        {
            //select IATACode from Airport where CityCode = '{0}' | fill:dep arr
            string apstr = "select IATACode from Airport where CityCode = '{0}'";
            sql = string.Format(apstr, city);
            dt = SqlHelper.ExecuteDataTable(sql, null);
            int apcount = dt.Rows.Count;
            aprs = new string[apcount];
            int i = 0;
            foreach (DataRow dataRow in dt.Rows)
            {
                aprs[i] = dataRow["IATACode"].ToString();
                i++;
            }
            return aprs;
        }

        public void route(string ap1,string ap2)
        {
            sql = string.Format("select RouteID from Route where DepartureAirportIATA = '{0}' and ArrivalAirportIATA='{1}'", ap1, ap2);
            dt = SqlHelper.ExecuteDataTable(sql, null);
            //routeID = Array.ConvertAll<DataRow, int>(dt.Rows[0], int.Parse);
            foreach (DataRow dataRow in dt.Rows)
            {
                routeID.Add(Convert.ToInt32(dataRow["RouteID"]));
            }
        }

        public void schedule(string rid)
        {
            //原参数:int rid
            //sql = "select * from Schedule where RouteId = {0} and Date > '{1}'";
            if (rid == "") rid = "1=1";
            sql = "select * from Schedule where ({0}) and Date = '{1}' order by Date";
            sql = string.Format(sql, rid, deptime);
            rst = SqlHelper.ExecuteDataTable(sql, null);
            foreach (DataRow dataRow in rst.Rows)
            {
                scheduleID.Add(Convert.ToInt32(dataRow["ScheduleId"]));
            }
        }

        public void airplane(int sid)
        {
            //string sql_rest= "select COUNT(SeatLayoutId) from FlightReservation where ScheduleId in (select ScheduleId from Schedule where RouteId = {0}) and SeatLayoutId between ";
            string sql_rest = "select COUNT(*) from FlightReservation where ScheduleId = {0} and SeatLayoutId between ";
            sql = string.Format("select distinct AircraftId from Schedule where ScheduleId = {0}", sid);
            int pid =(int) SqlHelper.ExecuteScalar(sql, null);
            
            int tickets = 0;
            DataRow dr = rst.Select("ScheduleId =" + sid.ToString())[0];
            if (pid == 1)
            {
                //cabin=1
                sql = string.Format(sql_rest + "1 and 8", sid);
                tickets = 8 - (int)SqlHelper.ExecuteScalar(sql, null);
                dr["First"] = tickets;
                //cabin=2
                sql = string.Format(sql_rest + "9 and 68", sid);
                tickets = 60 - (int)SqlHelper.ExecuteScalar(sql, null);
                dr["Business"] = tickets;
                //cabin=3
                sql = string.Format(sql_rest + "69 and 188", sid);
                tickets = 120 - (int)SqlHelper.ExecuteScalar(sql, null);
                dr["Economy"] = tickets;
            }
            else
            {
                sql = string.Format(sql_rest + "189 and 200", sid);
                tickets = 12 - (int)SqlHelper.ExecuteScalar(sql, null);
                dr["First"] = tickets;
                sql = string.Format(sql_rest + "201 and 260", sid);
                tickets = 60 - (int)SqlHelper.ExecuteScalar(sql, null);
                dr["Business"] = tickets;
                sql = string.Format(sql_rest + "261 and 350", sid);
                tickets = 90 - (int)SqlHelper.ExecuteScalar(sql, null);
                dr["Economy"] = tickets;
            }
            //sql = string.Format(sql_rest, sid);
            //tickets = (int)SqlHelper.ExecuteScalar(sql, null);
        }

        public void traverseDataTable() {
            foreach (DataRow row in rst.Rows) {
                FlightInfo info = new FlightInfo();
                info.ScheduleId = (row["ScheduleId"]==DBNull.Value)?0: Convert.ToInt32(row["ScheduleId"]);
                info.Date =Convert.ToDateTime(row["Date"]);
                //info.Time =TimeSpan.Parse(row["Time"].ToString());
                info.Time = TimeSpan.Parse(row["Time"].ToString());
                info.AircraftId = (row["AircraftId"] == DBNull.Value) ? 0 : Convert.ToInt32(row["AircraftId"]);
                info.RouteId = (row["RouteId"] == DBNull.Value) ? 0 : Convert.ToInt32(row["RouteId"]);
                info.EPrice = (row["EconomyPrice"] == DBNull.Value) ? 0 : Convert.ToDouble(row["EconomyPrice"]);
                info.FlightNum = (row["FlightNumber"] == DBNull.Value) ? 0 : Convert.ToInt32(row["FlightNumber"]);
                info.Gate= Convert.ToString(row["Gate"]);
                info.Status=Convert.ToString(row["Status"]);

                info.First = (row["First"] == DBNull.Value) ? 0 : Convert.ToInt32(row["First"]);
                info.Business = (row["Business"] == DBNull.Value) ? 0 : Convert.ToInt32(row["Business"]);
                info.Economy = (row["Economy"] == DBNull.Value) ? 0 : Convert.ToInt32(row["Economy"]);
                info.BPrice = 0;
                info.HPrice = 0;
                info.Deptime = DateTime.Today;

                info.Depcity = dep;
                info.Arrcity = arr;

                int m= (row["FLTtime"] == DBNull.Value) ? 0 : Convert.ToInt32(row["FLTtime"]);
                info.Arrtime = info.Deptime.AddMinutes(m);
                info.Totaltime = TimeSpan.FromMinutes(m);

                list.Add(info);
            }
        }
        public void city() {
            foreach (int rid in routeID) {
                sql = string.Format("select DepartureAirportIATA,ArrivalAirportIATA,FlightTime from Route where RouteID = '{0}'", rid);
                dt = SqlHelper.ExecuteDataTable(sql, null);
                DataRow[] rowarr = rst.Select("RouteId =" + rid.ToString());
                foreach (DataRow row in rowarr) {
                    row["Depap"] = dt.Rows[0]["DepartureAirportIATA"].ToString();
                    row["Arrap"] = dt.Rows[0]["ArrivalAirportIATA"].ToString();
                    row["FLTtime"] = dt.Rows[0]["FlightTime"];
                }
            }
        }
        public void loadUserControl() {
            for (int index = 0; index < list.Count; index++) {
                flightOneWay uc = new flightOneWay();
                pFLTrs.Controls.Add(uc);
                /*
                    private string price;
                    private string cabinType;
                    private string fltNum;
                    private string dep;
                    private string depTime;
                    private string arr;
                    private string arrTime;
                    private string fltType;
                    private string totalTime;
                    private string availableTickets;    
             */
                uc.Price = list[index].EPrice.ToString();
                uc.CabinType = cbCabin.Text;
                uc.FLTnum = list[index].FlightNum.ToString();
                uc.DEP = list[index].Depcity+"/"+ list[index].Depap;
                uc.DEPtime = list[index].Deptime.ToString();
                uc.ARR = list[index].Arrcity+"/"+list[index].Arrap;
                uc.ARRtime = list[index].Arrtime.ToString();
                uc.FLTtype = flttp;
                uc.TotalTime = list[index].Totaltime.ToString();
                uc.AvailableTickets = list[index].First.ToString();


            }
        }
    }
}
