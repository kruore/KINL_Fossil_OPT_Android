package com.example.KINLAB_OPT_Fossil_Provider;

import android.util.Log;

public final class Dbg {
    private Dbg (){}

    public static void out (Object msg){
        Log.i ("info", msg.toString ());
    }
}

