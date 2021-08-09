using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTimeCode : MonoBehaviour
{

    void Start()
    {
        //string t1 = "175959111";
        //string t2 = "175959211";
        //string t3 = "175959211";
        //string t4 = "175959311";

        //string t1 = "175959111";
        //string t2 = "175959221";
        //string t3 = "175959221";
        //string t4 = "175959311";

        string t1 = "175951000";
        string t2 = "175959000";
        string t3 = "175959000";
        string t4 = "175951500";

        Tuple<long, long> aaa = Calculate_PTP_OffsetDelay(t1, t2, t3, t4);
        Tuple<TimeSpan, TimeSpan> bbb = Calculate_PTP_OffsetDelay1(t1, t2, t3, t4);
    }   

    public Tuple<long, long> Calculate_PTP_OffsetDelay(string t1, string t2, string t3, string t4)
    {
        //ex 175959111 -> 17 59 59 111

        // adding the 3000 Milliseconds 
        // using AddMilliseconds() method; 
        //DateTime date2 = date1.AddMilliseconds(3000);


        // creating object of DateTime 
        DateTime date1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t1.Substring(0, 2)), int.Parse(t1.Substring(2, 2))
            , int.Parse(t1.Substring(4, 2)), int.Parse(t1.Substring(6, 3)));

        //Debug.Log("date1 : " + date1.ToString());

        DateTime date2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t2.Substring(0, 2)), int.Parse(t2.Substring(2, 2))
    , int.Parse(t2.Substring(4, 2)), int.Parse(t2.Substring(6, 3)));

        //Debug.Log("date2 : " + date2.ToString());

        TimeSpan date3 = date2 - date1;

        Debug.Log("date3 : " + date3.ToString());

        long t2minust1 = long.Parse(t2) - long.Parse(t1);
        //Debug.LogError("t1 " + t1);
        long t4minust3 = long.Parse(t4) - long.Parse(t3);
        //Debug.LogError("t2 " + t2);
        long offset = (t2minust1 - t4minust3) / 2;
        long delay = (t2minust1 + t4minust3) / 2;

        return Tuple.Create(offset, delay);
    }

    public Tuple<TimeSpan, TimeSpan> Calculate_PTP_OffsetDelay1(string t1, string t2, string t3, string t4)
    {
        //ex 175959111 -> 17 59 59 111
        TimeSpan offset, delay;

        try
        {
            Debug.Log("aaaa");

            List<DateTime> list_date = new List<DateTime>();

            list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t1.Substring(0, 2)), int.Parse(t1.Substring(2, 2))
                , int.Parse(t1.Substring(4, 2)), int.Parse(t1.Substring(6, 3))));

            list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t2.Substring(0, 2)), int.Parse(t2.Substring(2, 2))
                , int.Parse(t2.Substring(4, 2)), int.Parse(t2.Substring(6, 3))));

            list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t3.Substring(0, 2)), int.Parse(t3.Substring(2, 2))
                , int.Parse(t3.Substring(4, 2)), int.Parse(t3.Substring(6, 3))));

            list_date.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(t4.Substring(0, 2)), int.Parse(t4.Substring(2, 2))
                , int.Parse(t4.Substring(4, 2)), int.Parse(t4.Substring(6, 3))));


            TimeSpan timeSpan_t2t1 = list_date[1] - list_date[0];
            TimeSpan timeSpan_t4t3 = list_date[3] - list_date[2];

            Debug.Log(timeSpan_t2t1.ToString());

            offset = new TimeSpan((timeSpan_t2t1.Ticks - timeSpan_t4t3.Ticks) / 2);

            delay = new TimeSpan((timeSpan_t2t1.Ticks + timeSpan_t4t3.Ticks) / 2);

            Debug.Log("offset : " + offset.TotalSeconds.ToString());
            Debug.Log("delay : " + delay.TotalSeconds.ToString());

        }
        catch (Exception)
        {

            throw;
        }

        return Tuple.Create(offset, delay);
    }
}
