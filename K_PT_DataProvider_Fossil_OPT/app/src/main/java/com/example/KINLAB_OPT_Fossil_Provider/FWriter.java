package com.example.KINLAB_OPT_Fossil_Provider;


import android.os.Environment;

import java.io.File;
import java.io.FileOutputStream;
import java.io.OutputStream;

class FWriter implements  Runnable
{
    //tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j], lng[i], lat[i]
    FWriter(String dirname_, String fn_, int szbuf_, long[] t0, long[] t1, float[] d0, float[] d1, float[] d2, float[] d3, float[] d4, float[] d5, float[] d6, float[] d7 )
    {
        mode = 0;
        fn1 = fn_;
        dirname = dirname_;
        szbuf = szbuf_;

        tacc = t0;
        tgyro = t1;

        accx = d0 ;
        accy = d1;
        accz = d2;

        px = d3;
        py = d4;
        pz = d5;

        lng = d6;
        lat = d7;
    }

    FWriter(String dirname_, String fn_, String msg_)
    {
        mode = 1;
        fn1 = fn_;
        dirname = dirname_;
        msg = msg_;
    }
    private int mode;
    private String msg;

    private int szbuf;
    private long[] tacc;
    private float[] px;
    private float[] py;
    private float[] pz;

    private long[] tgyro;

    private float[] accx;
    private float[] accy;
    private float[] accz;

    private float[] lng;
    private float[] lat;

    private String fn1;
    private String dirname;

    public void run() {
        String fn="";
        try {
            //fn = Environment.getExternalStorageDirectory() + File.separator + "data"+ File.separator + dirname;
            fn = Environment.getDataDirectory()+ File.separator + "data"+ File.separator + dirname;
            fn = "/storage/emulated/0" + File.separator + "data"+ File.separator + dirname;


            Dbg.out(fn);
            // /storage/emulated/0/data/Sensors/20200122_fossil_C00_

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


            //System.out.println(fn1);
            Dbg.out(fn1);
            File file = new File(tDir, fn1);
            file.createNewFile();

            //write the bytes in file
            if(file.exists())
            {
                OutputStream fo = new FileOutputStream(file);

                if(mode==0) {
                    for (int i = 0; i < szbuf; i++) { // TODO : sbuf? 일단 길면 길지 큰 차이는 안나긴하는데... 20ms 기준으로 한거라..
                        int j = szbuf - 1 - i;
                        //String str1 = String.format("%d\t%.3f\t%.3f\t%.3f\t%d\n", (tgyro[j] - tgyro[szbuf-1]), px[j], py[j], pz[j], btn_gyro[j]);
                        //String str1 = String.format("%d\t%d\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\n", tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j]);

                        if(tgyro[j]==0 && px[j] ==0 && py[j]==0 && pz[j] ==0) // 자이로가 1개 적은 경우. 가끔 발생. Imputation
                        {
                            tgyro[j] = tacc[j];

                            if(j-1>-1) {
                                px[j] = px[j - 1];
                                py[j] = py[j - 1];
                                pz[j] = pz[j - 1];
                            }
                        }

                        String str1 = String.format("%d\t%d\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.6f\t%.6f\n",
                                tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j], lng[j], lat[j]);

                        fo.write(str1.getBytes());
                    }
                }
                else //header file
                {
                    String str1 = msg;
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

    }
}