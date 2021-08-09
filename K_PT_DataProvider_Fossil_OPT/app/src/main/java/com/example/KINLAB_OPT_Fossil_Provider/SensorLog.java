package com.example.KINLAB_OPT_Fossil_Provider;

import java.io.File;
import java.io.FileOutputStream;
import java.io.OutputStream;

import android.content.Context;

import android.content.SharedPreferences;
import android.os.Environment;


class SensorLog {
	private static final String PREFS_NAME = "Dalek";

	//gyro
	public float[] px;	
	public float[] py;
	public float[] pz;
	
	public long[] tgyro;
	
	public float[] accx;	
	public float[] accy;
	public float[] accz;

	public float[] lng;
	public float[] lat;


	public long[] tacc;

    private long[] tacc2;
    private float[] px2;
    private float[] py2;
    private float[] pz2;

    private long[] tgyro2;

    private float[] accx2;
    private float[] accy2;
    private float[] accz2;

    private float[] lng2;
    private float[] lat2;


	private long cnt1, cnt2;
	
	public int szbuf;
	
	
	public int nButton;
	public boolean bSavedGyro = false;
	public boolean bSavedAcc = false;

	public String fcommon  = null;
	public String mDevID  = null;
	public String mDevName = null;

	public int nSavedFile = 0;
	public int nAbnormal = 0;

	private int nclass;
    private int mCntFsave;


    private boolean bOkayDiffTime;
	private Context mContext=null;

	String dirname2save="";

	public void SetDeviceName(String devname)
	{
		mDevName = devname;

	}


	public void SetDeviceID(String devidx)
	{
		mDevID = devidx;

	}

	public String GetDeviceID()
	{
		return mDevID;
	}

	public String GetDeviceName()
	{
		return mDevName;
	}
	
    public int GetNumberOfSaved()
    {

        return nSavedFile;
    }
    public int IncreaseSaveCount()
    {
        return ++mCntFsave;

    }

	public int IncreaseNbAbnormal()
	{
		return ++nAbnormal;

	}

	public int SetNbAbnormal(int nb_)
	{
		nAbnormal = nb_;
		return nAbnormal;

	}

	public int GetNbAbnormal()
	{
		return nAbnormal;

	}


	public SensorLog(int nsz, Context ctx){
		mContext = ctx;

        px =  new float[nsz];
        py =  new float[nsz];
        pz =  new float[nsz];

        accx =  new float[nsz];
        accy =  new float[nsz];
        accz =  new float[nsz];

		lng = new float[nsz];
		lat = new float[nsz];

		tacc =  new long[nsz];
        tgyro =  new long[nsz];

        for(int i=0; i<nsz; i++)
        {
            px[i] = 0;
            py[i] = 0;
            pz[i] = 0;

            accx[i] = 0;
            accy[i] = 0;
            accz[i] = 0;

            tgyro[i] 	= 0;
            tacc[i] 	= 0;
        }

        cnt1 = -1;
        cnt2 = -1;
		szbuf = nsz;
		nclass = -1;

        mCntFsave  =0 ;


		bOkayDiffTime = false;

	}


	public void SetClass(int class_selected)
	{
		nclass = class_selected;

	}

	@Deprecated
	public int GetClass()
	{
		return GetMotionClass();

	}

	public int GetMotionClass()
	{
		return nclass;

	}

	public String getCurrentValuesGyroAsString()
	{
		String buf1 = String.format("%.3f/%.3f/%.3f", px[0], py[0], pz[0]);
		return buf1;
	}

    public String getCurrentValuesAccelAsString()
    {
        String buf1 = String.format("%.3f/%.3f/%.3f", accx[0], accy[0], accz[0]);
        return buf1;
    }

	
	public void SaveSensorData()
	{
		cnt1++;
		
		//SaveSensorDataGyro();
		//SaveSensorDataAcc();
		
		//nButton = 0;
	}
	
	public void MakeCommonFnameBasis()
	{
		//if(!bSavedAcc && !bSavedGyro) //&& !bSavedRot
		{
			fcommon = DDUtilFile.MakeFileNameWithDateTime();
			
		}
		
	}
    public String SaveSensorDataSync2(String dirname)
    {
        String fn="";
        try {
            fn = Environment.getExternalStorageDirectory() + File.separator + "data"+ File.separator + dirname;
            //fn = Environment.getDataDirectory() + File.separator + "data"+ File.separator + dirname;
            //String intStorageDirectory = getFilesDir().toString();

            File tDir = new File(fn);
            // have the object build the directory structure, if needed.
            try {
                tDir.mkdirs();
            }catch (Exception e) {
                e.printStackTrace();
                Dbg.out("Directory not created...");
            }

            if(tDir.exists())
            {
                System.out.println("DIR : "+tDir);
            }

            String tmp1 = String.format("%04d_C%d_",mCntFsave, nclass);
            tmp1 = mDevID + "_"+ mDevName+ "_" + tmp1;
            String fn1 = tmp1+fcommon+"_sync.txt"; //String.format("%d_%d_g.txt", (int)tstamp, cnt1);
            //System.out.println(fn1);

            File file = new File(tDir, fn1);
            file.createNewFile();

            //write the bytes in file
            if(file.exists())
            {
                OutputStream fo = new FileOutputStream(file);
                for(int i=0; i<szbuf; i++)
                {
                    int j = szbuf-1-i;
                    //String str1 = String.format("%d\t%.3f\t%.3f\t%.3f\t%d\n", (tgyro[j] -late tgyro[szbuf-1]), px[j], py[j], pz[j], btn_gyro[j]);
                    //String str1 = String.format("%d\t%d\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\n", tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j]);
                    String str1 = String.format("%d\t%d\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.5f\t%.5f\n",
                            tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j], lng[j], lat[j]);

                    fo.write(str1.getBytes());
                }

                fo.close();
                System.out.println("file created: "+file);

                //bSavedGyro = true;
                //nclass = 0;

            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        return fn;
    }

    public String GetLatestValues(boolean include_stamp)
	{
		int j = 0;
		String str1;
		if(include_stamp) {
			str1 = String.format("%d,%d,%.3f,%.3f,%.3f,%.3f,%.3f,%.3f",
					tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j]);
		}
		else
		{	str1 = String.format("%.3f,%.3f,%.3f,%.3f,%.3f,%.3f",
				accx[j], accy[j], accz[j], px[j], py[j], pz[j]);
		}
		return str1;
	}
	public int GetTimeElapsed(int steps)
	{
		if(steps<1)
			steps =1;
		return (int) (tacc[0] - tacc[steps]);
	}
	public String GetLatestAcc(boolean include_stamp)
	{
		int j = 0;
		String str1;
		if(include_stamp) {
			str1 = String.format("%d,%.3f,%.3f,%.3f",
					tacc[j], accx[j], accy[j], accz[j]);
		}
		else
		{	str1 = String.format("%.3f,%.3f,%.3f",
				accx[j], accy[j], accz[j]);
		}
		return str1;

	}
	public String GetLatestGyro(boolean include_stamp)
	{
		int j = 0;
		String str1;
		if(include_stamp) {
			str1 = String.format("%d,%.3f,%.3f,%.3f",
					tgyro[j], px[j], py[j], pz[j]);
		}
		else
		{	str1 = String.format("%.3f,%.3f,%.3f",
				px[j], py[j], pz[j]);
		}
		return str1;

	}
    public void Duplicate()
    {
        //int[] b = a.clone();

        //tacc, tgyro, accx, accy, accz, px, py, pz, lng, lat

        tacc2 = tacc.clone();
        px2  = px.clone();
        py2  = py.clone();
        pz2  = pz.clone();

        tgyro2 = tgyro.clone();

        accx2 = accx.clone();
        accy2 = accy.clone();
        accz2 = accz.clone();

        lng2 = lng.clone();
        lat2 = lat.clone();
    }
//sendMessage
    public String SaveSensorDataSync(String dirname, String fnBasis, String note1,  int to_be_delayed)
    {
		//SetDeviceName(name);
        String tmp0 = String.format("%03d_C%d_",mCntFsave, nclass);
		String tmp2 = mDevID + "_"+ mDevName;
		//String tmp1 = mDevID + "_"+ mDevName+ "_" + tmp0;
        fcommon = DDUtilFile.MakeFileNameWithDateTime();

        //String dirname2;


		//Dbg.out(String.format("%02d_", this.GetClass()));
		//Dbg.out(dirname2);
		//Dbg.out("---------------------------------");

		if(mCntFsave==0)
		{
			SharedPreferences sharedPref = mContext.getSharedPreferences( PREFS_NAME, Context.MODE_PRIVATE);
			int cntSession = sharedPref.getInt("nSaved", 0);

			if(nAbnormal==0) // otherwise, file count is reset due to an abnormal situation.
			{
				cntSession = cntSession+1;
				SharedPreferences prefs = mContext.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
				SharedPreferences.Editor editor = prefs.edit();
				editor.putInt("nSaved", cntSession);
				editor.commit();

			}
			if (note1 != null && !note1.isEmpty())
			{
				note1 = String.format("delay %d ms\n%s", to_be_delayed, note1);
				Dbg.out(note1);
				String fn2;
				if(fnBasis!=null)
					fn2 = fnBasis+"_"+ tmp2 + "_" + fcommon + "_" + tmp0 + "_readme.txt"; //String.format("%d_%d_g.txt", (int)tstamp, cnt1);
				else
					fn2 = tmp2 + "_" + fcommon + "_" + tmp0 + "_readme.txt";


				dirname2save = dirname+"/"+DDUtilFile.MakeStringWithDate()+String.format("_S%d_%s_C%02d_",cntSession, mDevName,  this.GetMotionClass());

				FWriter fwriter2 = new FWriter(dirname2save, fn2, note1);

				Thread t2 = new Thread(fwriter2);
				t2.start();
				//Dbg.out(fn2);
				//((MainActivity) mContext).sendMessage(dirname2save);

			}
		}


		String fn1;

		if(fnBasis!=null)
			fn1 = fnBasis+"_"+  tmp2 + "_" + fcommon + "_" + tmp0 + "_sync.txt"; //String.format("%d_%d_g.txt", (int)tstamp, cnt1);
		else
			fn1 = tmp2 + "_" + fcommon + "_" + tmp0 + "_sync.txt";

		//FWriter fwriter1 = new FWriter(dirname2, fn1, szbuf, tacc, tgyro, accx, accy, accz, px, py, pz, lng, lat);
        FWriter fwriter1 = new FWriter(dirname2save, fn1, szbuf, tacc2, tgyro2, accx2, accy2, accz2, px2, py2, pz2, lng2, lat2);
        Thread t1 = new Thread(fwriter1);
        t1.start();
		Dbg.out(fn1);
        return dirname2save+"/"+fn1;
    }

	public String SaveSensorDataGyro(String dirname)
	{
		Dbg.out("SaveSensorDataGyro()");
		MakeCommonFnameBasis();
		String fn="";
		try {
			fn = Environment.getExternalStorageDirectory() + File.separator + "data"+ File.separator + dirname;
			//fn = Environment.getDataDirectory() + File.separator + "data"+ File.separator + dirname;
			//String intStorageDirectory = getFilesDir().toString();

			File tDir = new File(fn);
			// have the object build the directory structure, if needed.
			try {
				tDir.mkdirs();
			}catch (Exception e) {
				e.printStackTrace();
				Dbg.out("Directory not created...");
			}

			if(tDir.exists())
			{
				System.out.println("DIR : "+tDir);
			}

            String tmp1 = String.format("%04d_C%d_",mCntFsave, nclass);
			tmp1 = mDevID + "_"+ mDevName+ "_" + tmp1;
			String fn1 = tmp1+fcommon+"_gyro.txt"; //String.format("%d_%d_g.txt", (int)tstamp, cnt1);
			//System.out.println(fn1);
			
			File file = new File(tDir, fn1);
			file.createNewFile();
			
			//write the bytes in file
			if(file.exists())
			{
			     OutputStream fo = new FileOutputStream(file);      
			     for(int i=0; i<szbuf; i++)
			     {
			    	 int j = szbuf-1-i;
			    	 //String str1 = String.format("%d\t%.3f\t%.3f\t%.3f\t%d\n", (tgyro[j] - tgyro[szbuf-1]), px[j], py[j], pz[j], btn_gyro[j]);
			    	 String str1 = String.format("%d\t%.3f\t%.3f\t%.3f\n", tgyro[j], px[j], py[j], pz[j]);
			    	 fo.write(str1.getBytes());
			     }
			     
				fo.close();
				System.out.println("file created: "+file);

				//bSavedGyro = true;
				//nclass = 0;
			     
			}               
		} catch (Exception e) {
			  e.printStackTrace();
		}
		return fn;
	}
	

	public String SaveSensorDataAcc(String dirname)
	{
		Dbg.out("SaveSensorDataAcc()");
		MakeCommonFnameBasis();
		String fn="";
		try {
			fn = Environment.getExternalStorageDirectory() + File.separator + "data"+ File.separator + dirname;
			//fn = Environment.getDataDirectory() + File.separator + "data"+ File.separator + dirname;
			//String intStorageDirectory = getFilesDir().toString();

			File tDir = new File(fn);
			// have the object build the directory structure, if needed.
			try {
				tDir.mkdirs();
			}catch (Exception e) {
				e.printStackTrace();
				Dbg.out("Directory not created...");
			}

			String tmp1 = String.format("%04d_C%d_",mCntFsave, nclass);
			tmp1 = mDevID + "_"+ mDevName+ "_" + tmp1;
			String fn1 = tmp1+fcommon+"_acc.txt"; //String.format("%d_%d_g.txt", (int)tstamp, cnt1);
			//System.out.println(fn1);

			File file = new File(tDir, fn1);
			file.createNewFile();

			//write the bytes in file
			if(file.exists())
			{
				OutputStream fo = new FileOutputStream(file);
				for(int i=0; i<szbuf; i++)
				{
					int j = szbuf-1-i;
					//String str1 = String.format("%d\t%.3f\t%.3f\t%.3f\t%d\n", (tgyro[j] - tgyro[szbuf-1]), px[j], py[j], pz[j], btn_gyro[j]);
					String str1 = String.format("%d\t%.3f\t%.3f\t%.3f\n", tacc[j], accx[j], accy[j], accz[j]);
					fo.write(str1.getBytes());
				}

				fo.close();
				System.out.println("file created: "+file);

				//bSavedAcc = true;

			}
		} catch (Exception e) {
			e.printStackTrace();
		}

		return fn;
	}


	public int UpdateSensorsGyro2(long tstamp, float x_, float y_, float z_)
	{


		DDMisc.ShiftWithNewl(tgyro, tstamp, szbuf);
		DDMisc.ShiftWithNewf(px, x_, szbuf);
		DDMisc.ShiftWithNewf(py, y_, szbuf);
		DDMisc.ShiftWithNewf(pz, z_, szbuf);

		return 1;

	}
	public int UpdateSensorsGPS(float lng_, float lat_) {
		DDMisc.ShiftWithNewf(lat, lat_, szbuf);
		DDMisc.ShiftWithNewf(lng, lng_, szbuf);
		return 1;
	}


	public int UpdateSensorsAccel(long tstamp, float x_, float y_, float z_)
	{
		//if(true)
		if(tgyro[0]>0 && ( Math.abs(tstamp - tgyro[0])) >100 ) // for some reasons 100ms difference
		{
			//throw away !
			return 0;
		}

		DDMisc.ShiftWithNewl(tacc, tstamp, szbuf);
		DDMisc.ShiftWithNewf(accx, x_, szbuf);
		DDMisc.ShiftWithNewf(accy, y_, szbuf);
		DDMisc.ShiftWithNewf(accz, z_, szbuf);

		return 1;
	}

	public int UpdateSensorsAccel2(long tstamp, float x_, float y_, float z_)
	{
		DDMisc.ShiftWithNewl(tacc, tstamp, szbuf);
		DDMisc.ShiftWithNewf(accx, x_, szbuf);
		DDMisc.ShiftWithNewf(accy, y_, szbuf);
		DDMisc.ShiftWithNewf(accz, z_, szbuf);

		return 1;
	}

	
}