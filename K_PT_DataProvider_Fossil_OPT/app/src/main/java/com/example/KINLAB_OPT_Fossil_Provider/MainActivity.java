package com.example.KINLAB_OPT_Fossil_Provider;

import android.Manifest;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Context;
//import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.Debug;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.os.Message;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.provider.Settings;
import android.support.wearable.activity.WearableActivity;
import android.support.wearable.input.RotaryEncoder;
import android.support.wearable.input.WearableButtons;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

/*
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.FragmentTransaction;
*/
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.LinkedList;
import java.util.List;
import java.util.Queue;

// C:\Users\seung\AppData\Local\Android\Sdk\platform-tools>adb connect 192.168.1.156
// C:\Users\seung\AppData\Local\Android\Sdk\platform-tools>adb connect 192.168.1.143 @211
// adb connect 192.168.0.30 @home

public class MainActivity extends WearableActivity implements SensorEventListener {

    //PTP




    public boolean Start = false;
    private int i_Current_timestamp_ms_ACC = 0;
    private int i_Current_timestamp_SS_ACC = 0;
    private int i_Current_timestamp_MM_ACC = 0;
    private int i_Current_timestamp_HH_ACC = 0;

    private int i_Current_timestamp_ms_GYRO = 0;
    private int i_Current_timestamp_SS_GYRO = 0;
    private int i_Current_timestamp_MM_GYRO = 0;
    private int i_Current_timestamp_HH_GYRO = 0;

    private long i_pre_timeCounter_ACC = 0;
    private long i_timeCounter_ACC = 0;
    private long i_pre_timeCounter_GYRO = 0;
    private long i_timeCounter_GYRO = 0;

    private String buf_PTP_revTime;
    private String buf_PTP_sendTime;
    private String buf_PTP_revMsg;

    private ImageView mConnectivityIcon;
    private TextView mConnectivityText;
    //connectivity_text
    private Button mButton1;

    private TextView mTextView1;
    private TextView mTextView2;
    private TextView mTextView3;
    private TextView mTextView5;
    private TextView mTextView4;
    private TextView mTextView6; //to be delayed

    private Sensor accelSensor;
    private Sensor gyroSensor;
    private LocationManager locationManager;
    //   private List<String> listProviders;

    private String GyroData;
    private String AccData;

    private SensorManager mSensorManager;
    private boolean samplingStarted = false;
    //   long startTime = System.currentTimeMillis();
    long cnt1 = -1;//-1;
    long cnt2 = -1;

    int to_be_delayed = -1;
    int cntdelay = 0;

    //  int elapsed = 0;
    SensorLog sl1;
    int sequence_len;
    int cnt_fsave;
    String myAndroidDeviceId;
    int class_selected = 0;
    float accum_rotation = 0;
    private static final String PREFS_NAME = "Dalek";

    private static String dev_username = "Fossil";
    private String fnBasis = null;
    private String noteReceived = null;

    boolean bBLEConnected = false;
    double mLat = 0.0;
    double mLng = 0.0;

    int nSaved = 0;

    private int saveMode = 0; // 0 for conventional count-based saving scheme.
    // 1 for time-based for sync-- need to be activated through BLE

    private long t0 = 0;


    private boolean isACCLoaded = false;

    private LinkedList<String> list_SW_Data;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        checkAndRequestPermissions(this);

        Log.d("ClientThread7777", "연결시도.");

        thread = new HandlerThread("HandlerThread");
        thread.start();
        mServiceLooper = thread.getLooper();
        mServiceHandler = new ServiceHandler(mServiceLooper);

        m_ClientThread = new ClientThread();
        m_ClientThread.start();

        list_SW_Data = new LinkedList<String>();

        dev_username = getString(R.string.device_user);

        locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        Dbg.out(locationManager.toString()); //android.location.LocationManager@

        mConnectivityIcon = findViewById(R.id.connectivity_icon);
        mConnectivityText = findViewById(R.id.connectivity_text);
        mTextView1 = (TextView) findViewById(R.id.text1);
        mTextView2 = (TextView) findViewById(R.id.text2);
        mTextView3 = (TextView) findViewById(R.id.text3);
        mTextView5 = (TextView) findViewById(R.id.text5);
        mTextView4 = (TextView) findViewById(R.id.text4);
        mTextView6 = (TextView) findViewById(R.id.text6);

        mConnectivityText.setText("Ready..");

        myAndroidDeviceId = getUniqueID();
        myAndroidDeviceId = myAndroidDeviceId.substring(myAndroidDeviceId.length() - 5, myAndroidDeviceId.length());

        // timerHandler.postDelayed(timerRunnable, 0);
        // Enables Always-on
        setAmbientEnabled();

        mButton1 = (Button) findViewById(R.id.button1);
        mButton1.setText("Start");
        mButton1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                //onButtonClicked(v);
            }
        });
        mButton1.setEnabled(false);

        sequence_len = 500;
        cnt_fsave = 0;
        mSensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        sl1 = new SensorLog(sequence_len, getApplicationContext());
        sl1.SetDeviceID(myAndroidDeviceId);
        sl1.SetDeviceName(dev_username);
        //sl1.SetClass(0); //TODO !

        SharedPreferences sharedPref = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
        int class_retieved = sharedPref.getInt("class", 0);
        nSaved = sharedPref.getInt("nSaved", 0);
        if (false)// only for development process
            UpdateClass(0, true);
        else
            UpdateClass(class_retieved, false);

        to_be_delayed = sharedPref.getInt("to_be_delayed2", 3000);

        accum_rotation = class_retieved * 5.0f;

        int count = WearableButtons.getButtonCount(this);
        Dbg.out(String.format("System inti with %d MF buttons", count));
        InitSensors();
        UpdateUI();

    }

    @Override
    public boolean onGenericMotionEvent(MotionEvent ev) {
        if (ev.getAction() == MotionEvent.ACTION_SCROLL
                && RotaryEncoder.isFromRotaryEncoder(ev)) {
            // Note that we negate the delta value here in order to get the right scroll direction.
            float delta = -RotaryEncoder.getRotaryAxisValue(ev);//.getScaledScrollFactor(getContext()
            Dbg.out(String.format("RotaryEncoder %.2f: ", delta));

            accum_rotation += delta;
            class_selected = Math.round(accum_rotation / 5.0f);
            /*
            if(delta>0)
            {
                class_selected = class_selected +1; // todo : Use delta *values*
            }
            else
            {
                class_selected = class_selected -1;
            }
            */
            if (class_selected != sl1.GetMotionClass())
                UpdateClass(class_selected, false); // true when pressing button


            //scrollBy(0, Math.round(delta));
            return true;
        }

        return super.onGenericMotionEvent(ev);
    }


    private void setUiState(int uiState) {
        switch (uiState) {
            case 0:
                if (mConnectivityIcon != null) {
                    ;//mConnectivityIcon.setImageResource(null);
                    //TODO: NULL Resource
                }
                //mConnectivityText.setText(R.string.network_connecting);
                break;
            case 1:
                if (mConnectivityIcon != null) {
                    mConnectivityIcon.setImageResource(R.drawable.bluetooth_line);
                }
                //mConnectivityText.setText(R.string.network_connecting);
                break;

        }
    }

    private void UpdateUI() {
        //String buf = sl1.GetLatestValues(false);
        String buf;
        buf = sl1.GetLatestAcc(false);
        mTextView1.setText(buf);
        //   Load_Message_on_Handler(Enum_IPC_Message.Fossill_Trasfer_Acc_SampleData,buf);
        buf = sl1.GetLatestGyro(false);
        mTextView2.setText(buf);
        //    Load_Message_on_Handler(Enum_IPC_Message.Fossill_Trasfer_Gyro_SampleData,buf);
        mTextView3.setText(String.format("%d", cnt_fsave));
        //mTextView.setText(String.format("%d:%02d", minutes, seconds));

        int dt = sl1.GetTimeElapsed(1);
        mTextView5.setText(String.format("%d     %d ms    S%d", cnt1, dt, nSaved + 1));
        mTextView6.setText(String.format("Delay %d ms AB%d", to_be_delayed, sl1.GetNbAbnormal()));
    }

    private void UpdateClass(int c, boolean commit) {
        if (c < 0) {
            c = 0;
            return;
        }

        sl1.SetClass(c);
        class_selected = c;
        //  mTextView4.setText(String.format("C%02d (%.4f, %.4f)", class_selected, mLat, mLng));

        if (commit) {
            SharedPreferences prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
            SharedPreferences.Editor editor = prefs.edit();
            editor.putInt("class", sl1.GetMotionClass());
            editor.commit();
            Dbg.out(String.format("Commited class info.. C%02d", c));
        }

    }


    //runs without a timer by reposting this handler at the end of the runnable
    Handler timerHandler = new Handler();
    Runnable timerRunnable = new Runnable() {

        @Override
        public void run() {
            if (cnt1 < 0) {
                cnt1 = 0;
                cnt2 = 0; //just for check

                t0 = System.currentTimeMillis();
            }
            /*
            long millis = System.currentTimeMillis() - startTime;
            int seconds = (int) (millis / 1000);
            int minutes = seconds / 60;
            seconds = seconds % 60;
            */
            UpdateUI();

            timerHandler.postDelayed(this, 200);
        }
    };

    private void StartIgnoringDelay() // called by external input
    {
        mButton1.setText("Stop");

        timerHandler.postDelayed(timerRunnable, 0);
    }

    private void Start() {
        // DisplayLocation();

        String devid = sl1.GetDeviceID();//mTextViewID.getText().toString().trim();
        String devname = sl1.GetDeviceName();
        SharedPreferences prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = prefs.edit();
        editor.putString("name", devname);
        editor.putString("devid", devid);
        editor.putInt("class", sl1.GetMotionClass());
        editor.putInt("to_be_delayed2", to_be_delayed);

        editor.commit();
        sl1.SetNbAbnormal(0); // just in case where the app is reused

        if (saveMode == 3) {
            list_SW_Data.clear();

            Vibrator vibrator = (Vibrator) getSystemService(VIBRATOR_SERVICE);
            vibrator.vibrate(VibrationEffect.createOneShot(100, 10));
        }

        if (to_be_delayed > 500) {
            Dbg.out("start.. zing...!");

            Vibrator vibrator = (Vibrator) getSystemService(VIBRATOR_SERVICE);
            vibrator.vibrate(VibrationEffect.createOneShot(100, 10));
        } else {
            Dbg.out("start..without zing!");
        }

        mButton1.setText("Stop");

        SharedPreferences sharedPref = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
        nSaved = sharedPref.getInt("nSaved", 0);

        if (saveMode != 3) {
            //UpdateUI();
        }


        timerHandler.postDelayed(timerRunnable, to_be_delayed);
        //
    }

    private void ResetLogger() {
        // Reset the logger class
        int class_keep = sl1.GetMotionClass();

        sl1 = null;
        sl1 = new SensorLog(sequence_len, getApplicationContext());

        sl1.SetDeviceID(myAndroidDeviceId);
        sl1.SetDeviceName(dev_username);

        sl1.SetDeviceID(myAndroidDeviceId);

        UpdateClass(class_keep, true);

    }

    private void Stop() {
        cnt1 = -1;
        cnt2 = -1; //just for check
        timerHandler.removeCallbacks(timerRunnable);
        if (saveMode == 3) {
            return;
        }
        //    ResetLogger();

        //String   myAndroidDeviceId = getUniqueID();
        //myAndroidDeviceId = myAndroidDeviceId.substring(myAndroidDeviceId.length()-5, myAndroidDeviceId.length());
        //sl1.SetDeviceID(myAndroidDeviceId);

        //       mTextView1.setText("-");
//        mTextView2.setText("-");
//       mTextView5.setText("-");


        mButton1.setText("Start");

        // vibration feedback
        Vibrator vibrator = (Vibrator) getSystemService(VIBRATOR_SERVICE);
        //long[] vibrationPattern = {0, 500, 50, 300};
        //-1 - don't repeat
        //final int indexInPatternToRepeat = -g
        //vibrator.vibrate(vibrationPattern, indexInPatternToRepeat);
        //vibrator.vibrate
        vibrator.vibrate(VibrationEffect.createOneShot(300, 10));
    }


    public void DbgToast(String message) {
        Toast toast = null;
        toast = Toast.makeText(this, message, Toast.LENGTH_SHORT);
        toast.show();
    }

    @Override
    // Activity
    // https://developer.android.com/training/wearables/ui/multi-function#java
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (event.getRepeatCount() == 0) {
            if (keyCode == KeyEvent.KEYCODE_STEM_1) {
                Dbg.out("KeyEvent.KEYCODE_STEM_1");
                UpdateClass(class_selected, true); // true when pressing button

                if (mButton1.getText().toString().compareTo("Start") == 0) {
                    //Dbg.out("Hello from watch ! Start");

                    //cnt1 = 0;
                    DbgToast("곧 시작합니다 !");
                    Start();
                } else {
                    //Dbg.out("Hello from watch ! Stop");
                    //cnt1 = -1;
                    //mButton1.setText("Start");

                    Stop();
                }
                return true;
            } else if (keyCode == KeyEvent.KEYCODE_STEM_2) {
                Dbg.out("KeyEvent.KEYCODE_STEM_2");

                //sendMessage("aa");
                /*
                if(cnt1>-1)
                {
                    Dbg.out("Frozen !");
                    return true;
                }
                if(to_be_delayed>5000)
                {
                    to_be_delayed = 0;
                }
                else
                {
                    to_be_delayed = to_be_delayed +500;
                }
                Dbg.out(String.format("delay adjusted to %d ms", to_be_delayed));
                UpdateUI();
*/
                return true;
            } else if (keyCode == KeyEvent.KEYCODE_STEM_3) {
                Dbg.out("KeyEvent.KEYCODE_STEM_3");

                // Do stuff
                return true;
            }
        } else if (event.getRepeatCount() == 1) {
            if (keyCode == KeyEvent.KEYCODE_STEM_1) {
                Dbg.out("KeyEvent.KEYCODE_STEM_1 DBL===");

                return true;
            } else if (keyCode == KeyEvent.KEYCODE_STEM_2) {
                Dbg.out("KeyEvent.KEYCODE_STEM_2 DBL===");
                // Do stuff
                return true;
            } else if (keyCode == KeyEvent.KEYCODE_STEM_3) {
                Dbg.out("KeyEvent.KEYCODE_STEM_3 DBL===");
                // Do stuff
                return true;
            }
        }

        return super.onKeyDown(keyCode, event);
    }

    private void InitSensors() {
        //http://developer.android.com/guide/topics/sensors/sensors_motion.html
        //Acceleration force along the x axis (excluding gravity).
        List<Sensor> sensors = mSensorManager.getSensorList(Sensor.TYPE_ACCELEROMETER);//TYPE_LINEAR_ACCELERATION
        accelSensor = sensors.size() == 0 ? null : sensors.get(0);


        if (accelSensor != null) {

            // 10,000 microseconds = 10 millisecond
            mSensorManager.registerListener(
                    this,
                    accelSensor,
                    10000);//// SensorManager.SENSOR_DELAY_FASTEST

            samplingStarted = true;
            if (samplingStarted) {
                Log.d("Sampling", "Start!");
            }

        } else {
            Dbg.out("Sensor(s) missing: accelSensor: " +
                    accelSensor +
                    "; gyroSensor: " +
                    gyroSensor);
        }

        sensors = mSensorManager.getSensorList(Sensor.TYPE_GYROSCOPE);
        gyroSensor = sensors.size() == 0 ? null : sensors.get(0);

        if (gyroSensor != null) {

            // 10,000 microseconds = 10 millisecond
            mSensorManager.registerListener(
                    this,
                    gyroSensor,
                    10000);//// SensorManager.SENSOR_DELAY_FASTEST

            samplingStarted = true;
        } else {
            Dbg.out("Sensor(s) missing: accelSensor: " +
                    accelSensor +
                    "; gyroSensor: " +
                    gyroSensor);
        }
    }
    public Queue<String> dataQueue_ACC = new LinkedList<>();
    public Queue<String> dataQueue_GYRO = new LinkedList<>();

    @Override
    public void onSensorChanged(SensorEvent event) {
        if (cnt1 < 0) return;
        if (Start) {
            ProcessSample(event);
        }

    }

    private long time_gyro_prev = 0;
    private long time_acc_prev = 0;

    private void SetCntStart() {
        cnt1 = 0;
        cnt2 = 0; //just for check
    }


    StringBuilder sb = new StringBuilder();

    private void ProcessSample(SensorEvent sensorEvent) {

        float values[] = sensorEvent.values;
        long tsLong = System.currentTimeMillis();
        if (values.length < 3)
            return;
        long timeInMillis = (new Date()).getTime() + (sensorEvent.timestamp - System.nanoTime()) / 1000000L;
        if (sensorEvent.sensor == accelSensor) {
            if (i_pre_timeCounter_ACC != 0) {
                i_timeCounter_ACC = timeInMillis - i_pre_timeCounter_ACC;
            }
            Log.d("i_TIMECOUNTER", String.format("%s", i_timeCounter_ACC));
            //  Log.d("i_TIMECOUNTER", String.format("%s",i_timeCounter_ACC));
            // i_pre_timeCounter_ACC = t_now;
            i_Current_timestamp_ms_ACC += i_timeCounter_ACC;
            Log.d("i_TIMECOUNTER2", String.format("%s", i_Current_timestamp_ms_ACC));
            if (i_Current_timestamp_ms_ACC > 999) {
                i_Current_timestamp_ms_ACC -= 1000;
                i_Current_timestamp_SS_ACC++;
                if (i_Current_timestamp_SS_ACC > 59) {
                    i_Current_timestamp_SS_ACC = 0;
                    i_Current_timestamp_MM_ACC++;
                    if (i_Current_timestamp_MM_ACC > 59) {
                        i_Current_timestamp_MM_ACC = 0;
                        i_Current_timestamp_HH_ACC++;
                        if (i_Current_timestamp_HH_ACC > 24) {
                            i_Current_timestamp_HH_ACC = 0;
                        }
                    }
                }
            }
            i_pre_timeCounter_ACC = timeInMillis;

            String currentDataTime = String.format("%02d", (i_Current_timestamp_HH_ACC)) + String.format("%02d", (i_Current_timestamp_MM_ACC)) + String.format("%02d", (i_Current_timestamp_SS_ACC)) + String.format("%03d", (i_Current_timestamp_ms_ACC));
            Log.d("시간은", currentDataTime);
            //String str_time = String.format("%02d%02d%02d%03d", tsLong);(
            String str_time = currentDataTime;
            // String str_time = (new SimpleDateFormat("HHmmssSSS")).format(new Date(tsLong));
            //String data = str_time + "," + values[0] + "," + values[1] + "," + values[2] + ";";
            String data = str_time + String.format("^%.3f^%.3f^%.3f", values[0], values[1], values[2]);
            Log.d("ACC DATA", data);
            AccData = data;
            dataQueue_ACC.offer(AccData);
          //  AccData = null;
        }
        if (sensorEvent.sensor == gyroSensor) {
            //UpdateGUI();
            if (timeInMillis != 0) {
                if (i_pre_timeCounter_GYRO != 0) {
                    i_timeCounter_GYRO = timeInMillis - i_pre_timeCounter_GYRO;
                }
                //  Log.d("i_TIMECOUNTER", String.format("%s",i_timeCounter_ACC));
                // i_pre_timeCounter_ACC = t_now;
                i_Current_timestamp_ms_GYRO += i_timeCounter_GYRO;

                if (i_Current_timestamp_ms_GYRO > 999) {
                    i_Current_timestamp_ms_GYRO -= 1000;
                    i_Current_timestamp_SS_GYRO++;

                    if (i_Current_timestamp_SS_GYRO > 59) {
                        i_Current_timestamp_SS_GYRO = 0;
                        i_Current_timestamp_MM_GYRO++;

                        if (i_Current_timestamp_MM_GYRO > 59) {
                            i_Current_timestamp_MM_GYRO = 0;
                            i_Current_timestamp_HH_GYRO++;

                            if (i_Current_timestamp_HH_GYRO > 24) {
                                i_Current_timestamp_HH_GYRO = 0;
                            }
                        }
                    }
                }
                i_pre_timeCounter_GYRO = timeInMillis;
            }

            sl1.UpdateSensorsGyro2(tsLong, values[0], values[1], values[2]);
            // String str_time2 = String.format("%02d%02d%02d%03d", tsLong);
            String currentDataTime2 = String.format("%02d", (i_Current_timestamp_HH_GYRO)) + String.format("%02d", (i_Current_timestamp_MM_GYRO)) + String.format("%02d", (i_Current_timestamp_SS_GYRO)) + String.format("%03d", (i_Current_timestamp_ms_GYRO));
            // String str_time2 = (new SimpleDateFormat("HHmmssSSS")).format(new Date(tsLong));
            String data2 = currentDataTime2 + String.format("^%.3f^%.3f^%.3f", values[0], values[1], values[2]);
            Log.d("GYRO DATA", data2);

            GyroData = data2;
            dataQueue_GYRO.offer(GyroData);
         //   GyroData = null;
            //Send_Message_to_srv(Enum_IPC_Message.Fossill_Trasfer_Gyro_SampleData, GyroData);
        }

        if (dataQueue_ACC.size() > 0) {
            String a = dataQueue_ACC.poll();
            Load_Message_on_Handler(Enum_IPC_Message.Fossill_Trasfer_Acc_SampleData, a);
            //  Send_Message_direct(Enum_IPC_Message.Fossill_Trasfer_Acc_SampleData, a);
        }
        if (dataQueue_GYRO.size() > 0) {
            String b = dataQueue_GYRO.poll();
            Load_Message_on_Handler(Enum_IPC_Message.Fossill_Trasfer_Gyro_SampleData, b);
            // Send_Message_direct(Enum_IPC_Message.Fossill_Trasfer_Gyro_SampleData,b );
        }

    }


    // TODO: FORCE SAVING FILE WITH A GIVEN CODE !!

    //#define EPSILON    (1.0E-8)
    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    // https://stackoverflow.com/a/35495855
    public static final int REQUEST_ID_MULTIPLE_PERMISSIONS = 1;

    private boolean checkAndRequestPermissions(Activity activity) {

        int locationPermission = ContextCompat.checkSelfPermission(activity, Manifest.permission.ACCESS_FINE_LOCATION);
        int locationPermission2 = ContextCompat.checkSelfPermission(activity, Manifest.permission.ACCESS_COARSE_LOCATION);

        int storagePermission = ContextCompat.checkSelfPermission(activity, Manifest.permission.WRITE_EXTERNAL_STORAGE);
        List<String> listPermissionsNeeded = new ArrayList<>();

        if (locationPermission != PackageManager.PERMISSION_GRANTED) {
            listPermissionsNeeded.add(Manifest.permission.ACCESS_FINE_LOCATION);
        }
        if (locationPermission2 != PackageManager.PERMISSION_GRANTED) {
            listPermissionsNeeded.add(Manifest.permission.ACCESS_COARSE_LOCATION);
        }
        if (storagePermission != PackageManager.PERMISSION_GRANTED) {
            listPermissionsNeeded.add(Manifest.permission.WRITE_EXTERNAL_STORAGE);
        }
        if (!listPermissionsNeeded.isEmpty()) {
            ActivityCompat.requestPermissions(this, listPermissionsNeeded.toArray(new String[listPermissionsNeeded.size()]), REQUEST_ID_MULTIPLE_PERMISSIONS);
            return false;
        }
        return true;
    }

    // http://stackoverflow.com/a/28368570
    public String getUniqueID() {
        String myAndroidDeviceId = Settings.Secure.getString(getApplicationContext().getContentResolver(), Settings.Secure.ANDROID_ID);
        return myAndroidDeviceId;
    }

//    private void DisplayLocation()
//    {
//        Location lastKnownLocation = locationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
//
//        if (lastKnownLocation != null) {
//            double lng = lastKnownLocation.getLongitude();
//            double lat = lastKnownLocation.getLatitude();
//
//            mLat = lat;
//            mLng = lng;
//            //Log.d("DBG", "longtitude=" + lng + ", latitude=" + lat);
//            //String msg  = String.format("(%.6f, %.6f)", lng, lat);//"longtitude=" + lng + ", latitude=" + lat;
//            String msg  = String.format("C%02d (%.6f, %.6f)", class_selected, mLat, mLng);
//            //mTextViewLocation.setText(msg);
//            mTextView4.setText(msg);
//            Dbg.out(msg);
//
//            //tvGpsLatitude.setText(":: " + Double.toString(lat ));
//            //tvGpsLongitude.setText((":: " + Double.toString(lng)));
//        }
//        if (lastKnownLocation == null) {
//            lastKnownLocation = locationManager.getLastKnownLocation(LocationManager.NETWORK_PROVIDER);
//            if (lastKnownLocation != null) {
//                double lng = lastKnownLocation.getLongitude();
//                double lat = lastKnownLocation.getLatitude();
//                //Log.d(TAG, "longtitude=" + lng + ", latitude=" + lat);
//                Dbg.out("longtitude=" + lng + ", latitude=" + lat);
//
//
//            }
//        }
//        if (lastKnownLocation == null) {
//            lastKnownLocation = locationManager.getLastKnownLocation(LocationManager.PASSIVE_PROVIDER);
//            if (lastKnownLocation != null) {
//                double lng = lastKnownLocation.getLongitude();
//                double lat = lastKnownLocation.getLatitude();
//                Dbg.out("longtitude=" + lng + ", latitude=" + lat);
//            }
//        }
//
//        listProviders = locationManager.getAllProviders();
//        boolean [] isEnabled = new boolean[3];
//        for(int i=0; i<listProviders.size();i++) {
//            if(listProviders.get(i).equals(LocationManager.GPS_PROVIDER)) {
//                isEnabled[0] = locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER);
//
//                locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, this);
//            } else if(listProviders.get(i).equals(LocationManager.NETWORK_PROVIDER)) {
//                isEnabled[1] = locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER);
//
//                locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 0, this);
//            } else if(listProviders.get(i).equals(LocationManager.PASSIVE_PROVIDER)) {
//                isEnabled[2] = locationManager.isProviderEnabled(LocationManager.PASSIVE_PROVIDER);
//
//                locationManager.requestLocationUpdates(LocationManager.PASSIVE_PROVIDER, 0, 0, this);
//            }
//
//        }
//
//        String buf= String.format("Location %b %b %b", isEnabled[0], isEnabled[1], isEnabled[2]);
//        Dbg.out(buf);

    //String msg  = String.format("C%02d (%.6f, %.6f)", class_selected, mLat, mLng);
    //mTextViewLocation.setText(msg);
    //mTextView4.setText(msg);

    //mTextViewLocation.setText(buf);

    //  }
//
//    @Override
//    public void onLocationChanged(Location location) {
//
//        double lat = 0.0;
//        double lng = 0.0;
//
//        if (location.getProvider().equals(LocationManager.GPS_PROVIDER)) {
//            lat = location.getLatitude();
//            lng = location.getLongitude();
//
//            mLat = lat;
//            mLng = lng;
//
//            //String msg = String.format("(%.6f, %.6f)", lng, lat);//"longtitude=" + lng + ", latitude=" + lat;
//            String msg = String.format("C%02d (%.6f, %.6f)", class_selected, mLat, mLng);
//            mTextView4.setText(msg);
//            Dbg.out(msg);
//
//            //Double.toString(latitude )
//            //tvGpsLatitude.setText(": " + Double.toString(latitude ));
//            //tvGpsLongitude.setText((": " + Double.toString(longitude)));
//            //Log.d(TAG + " GPS : ", Double.toString(latitude )+ '/' + Double.toString(longitude));
//        }
//        if (location.getProvider().equals(LocationManager.NETWORK_PROVIDER)) {
//            lat = location.getLatitude();
//            lng = location.getLongitude();
//
//            mLat = lat;
//            mLng = lng;
//
//            //String msg = String.format("(%.6f, %.6f)", lng, lat);//"longtitude=" + lng + ", latitude=" + lat;
//            String msg = String.format("C%02d (%.6f, %.6f)", class_selected, mLat, mLng);
//            mTextView4.setText(msg);
//
//
//        }
//        if (location.getProvider().equals(LocationManager.PASSIVE_PROVIDER)) {
//            lat = location.getLatitude();
//            lng = location.getLongitude();
//
//            mLat = lat;
//            mLng = lng;
//
//            //String msg = String.format("(%.6f, %.6f)", lng, lat);//"longtitude=" + lng + ", latitude=" + lat;
//            String msg = String.format("C%02d (%.6f, %.6f)", class_selected, mLat, mLng);
//            mTextView4.setText(msg);
//
//        }

//    }
//    @Override
//    public void onProviderEnabled(String provider) {
//        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
//            return;
//        }
//        locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, this);
//        locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 0, this);
//        locationManager.requestLocationUpdates(LocationManager.PASSIVE_PROVIDER, 0, 0, this);
//    }
//
//    @Override
//    public void onProviderDisabled(String provider) {
//
//    }
//
//    @Override
//    public void onStatusChanged(String provider, int status, Bundle extras) {
//
//    }


//
//    ///////// BLE
//
//    private static final String TAG = "BLE";
//
//    // Intent request codes
//    public static final int REQUEST_CONNECT_DEVICE_SECURE = 1;
//    public static final int REQUEST_CONNECT_DEVICE_INSECURE = 2;
//    public static final int REQUEST_ENABLE_BT = 3;
//
//    private String messageToSend="aa";
//
//    /**
//     * Name of the connected device
//     */
//    private String mConnectedDeviceName = null;
//    private String mConnectedDeviceAddress = null;
//
//    private StringBuffer mOutStringBuffer;
//
//    String lastReadMessage=null;
//
//    /**
//     * Local Bluetooth adapter
//     */
//    private BluetoothAdapter mBluetoothAdapter = null;
//
//    /**
//     * Member object for the chat services
//     */
//    private BluetoothChatService mChatService = null;
//
//    void InitBlueTooth()
//    {
//        mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
//
//        // If the adapter is null, then Bluetooth is not supported
//
//        if (mBluetoothAdapter == null ) {
//
//            DbgToast("BLE NOT Working");
//        }
//        else
//        {
//            mConnectivityText.setText("Bluetooth Ready");
//
//        }
//    }
//
//    @Override
//    public void onStart() {
//        super.onStart();
//        if (mBluetoothAdapter == null) {
//            return;
//        }
//        // If BT is not on, request that it be enabled.
//        // setupChat() will then be called during onActivityResult
//        if (!mBluetoothAdapter.isEnabled()) {
//            Intent enableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
//            startActivityForResult(enableIntent, REQUEST_ENABLE_BT);
//            // Otherwise, setup the chat session
//        } else if (mChatService == null) {
//           // setupChat();
//        }
//    }
//
//    @Override
//    public void onDestroy() {
//
//        if (mChatService != null) {
//            mChatService.stop();
//        }
//
//        if(m_ClientThread != null && m_ClientThread.isAlive()){
//            m_ClientThread.Stop_Thread();
//        }
//
//        super.onDestroy();
//    }
//
//    @Override
//    public void onResume() {
//        super.onResume();
//
//        // Performing this check in onResume() covers the case in which BT was
//        // not enabled during onStart(), so we were paused to enable it...
//        // onResume() will be called when ACTION_REQUEST_ENABLE activity returns.
//        if (mChatService != null) {
//            // Only if the state is STATE_NONE, do we know that we haven't started already
//            if (mChatService.getState() == BluetoothChatService.STATE_NONE) {
//                // Start the Bluetooth chat services
//                mChatService.start();
//            }
//        }
//    }
//
//    public void Connect()
//    {
//        Log.d(TAG, "-----------------------------");
//        Log.d(TAG, "connecting");
//
//
//        Intent serverIntent = new Intent(this, DeviceListActivity.class); //getActivity()
//        //startActivity(serverIntent);
//        startActivityForResult(serverIntent, REQUEST_CONNECT_DEVICE_SECURE);
//
//    }
//    /**
//     * Sends a message.
//     *
//     * @param message A string of text to send.
//     */
//    public void sendMessage(String message) {
//        // Check that we're actually connected before trying anything
//        if (mChatService.getState() != BluetoothChatService.STATE_CONNECTED) {
//            DbgToast("NOT CONNECTED");
//            //Toast.makeText(getActivity(), R.string.not_connected, Toast.LENGTH_SHORT).show();
//            return;
//        }
//
//        // Check that there's actually something to send
//        if (message.length() > 0) {
//
//            // Get the message bytes and tell the BluetoothChatService to write
//            byte[] send = message.getBytes();
//            mChatService.write(send);
//
//            //DbgToast("Sent..."+message);
//
//            // Reset out string buffer to zero and clear the edit text field
//            mOutStringBuffer.setLength(0);
//
//
//            //mOutEditText.setText(mOutStringBuffer);
//        }
//    }
//    private void setupChat() {
//        Log.d(TAG, "setupChat()");
//
//        // Initialize the array adapter for the conversation thread
//        //FragmentActivity activity = this;//getActivity();
//        //if (activity == null) {
//        //    return;
//        //}
//
//        // Initialize the BluetoothChatService to perform bluetooth connections
//        mChatService = new BluetoothChatService(this, mHandler);//activity
//
//        // Initialize the buffer for outgoing messages
//        mOutStringBuffer = new StringBuffer();
//    }
//    /**
//     * The Handler that gets information back from the BluetoothChatService
//     */
//    private void SetConnectionStatus(boolean bconnected, String devname)
//    {
//        mConnectivityText.setText(mConnectedDeviceName);
//
//        setUiState(1);
//        mConnectedDeviceName = devname;
//
//        bBLEConnected = bconnected;
//        DbgToast("Connected to "
//                + mConnectedDeviceName);
//
//    }
//
//    private final Handler mHandler = new Handler() {
//        @Override
//        public void handleMessage(Message msg) {
//            //FragmentActivity activity = getActivity();
//            //MainActivity activity2 = (MainActivity) getActivity();
//
//            switch (msg.what) {
//                case Constants.MESSAGE_STATE_CHANGE:
//                    switch (msg.arg1) {
//                        case BluetoothChatService.STATE_CONNECTED:
//                            //setStatus(getString(R.string.title_connected_to, mConnectedDeviceName));
//                            //activity2.SetDeviceInfo(mConnectedDeviceName, mConnectedDeviceAddress);
//                            //DbgToast("Connected to "+mConnectedDeviceName);
//
//                            ;
//                            //
//                            break;
//                        case BluetoothChatService.STATE_CONNECTING:
//                            //setStatus(R.string.title_connecting);
//                            break;
//                        case BluetoothChatService.STATE_LISTEN:
//                        case BluetoothChatService.STATE_NONE:
//                            //setStatus(R.string.title_not_connected);
//                            break;
//                    }
//                    break;
//                case Constants.MESSAGE_WRITE:
//                    byte[] writeBuf = (byte[]) msg.obj;
//                    // construct a string from the buffer
//                    String writeMessage = new String(writeBuf);
//                    //mTvSent.setText("Me:  " + writeMessage);
//                    break;
//                case Constants.MESSAGE_READ:
//                    byte[] readBuf = (byte[]) msg.obj;
//                    // construct a string from the valid bytes in the buffer
//                    String readMessage = new String(readBuf, 0, msg.arg1);
//                    CheckReceivedBLE(readMessage);
//                    //mTvReceived.setText(mConnectedDeviceName + ":  " + readMessage);
//                    //activity2.SetMessage(readMessage);
//                    //DbgToast(readMessage);
//
//                    //mConversationArrayAdapter.add(mConnectedDeviceName + ":  " + readMessage);
//                    break;
//                case Constants.MESSAGE_DEVICE_NAME:
//                    // save the connected device's name
//                    mConnectedDeviceName = msg.getData().getString(Constants.DEVICE_NAME);
//                    mConnectedDeviceAddress = msg.getData().getString(Constants.DEVICE_ADDRESS); //SC
//
//
//                    SetConnectionStatus(true, mConnectedDeviceName);
//
//                    //mTvStatus.setText(mConnectedDeviceName);
//
//                    break;
//                case Constants.MESSAGE_TOAST:
//                    DbgToast(msg.getData().getString(Constants.TOAST));
//
//                    break;
//            }
//        }
//    };
//    private void CheckReceivedBLE(final String readMessage)
//    {
//        lastReadMessage = readMessage;
//        if(readMessage.length()>4) {
//            if (readMessage.substring(0, 5).equals("START") || readMessage.substring(0, 5).equals("start")) {//"START"
//                Log.d(TAG, "Start");
//                if(readMessage.length()>5)
//                    fnBasis = readMessage.substring(5);
//                else
//                    fnBasis = null;
//
//                if(fnBasis!=null)
//                {
//                    Log.d(TAG, fnBasis);
//                    DbgToast("Started by BLE!\n"+fnBasis);
//                    ;//DbgToast(fnBasis); // okay
//                }
//                else
//                {
//                    DbgToast("Started by BLE!!");
//                }
//                // external triggering
//                //StartIgnoringDelay();
//                saveMode = 2; //sample-basis
//                Start();
//            }
//            else if  (readMessage.substring(0, 5).equals("SYNCH") || readMessage.substring(0, 5).equals("synch")) //"START"
//            {   // same as Start. except for ignoring delay.
//                if(readMessage.length()>5)
//                    fnBasis = readMessage.substring(5);
//                else
//                    fnBasis = null;
//
//                if(fnBasis!=null)
//                {
//                    Log.d(TAG, fnBasis);
//                    DbgToast("Started by BLE!\n"+fnBasis);
//                    ;//DbgToast(fnBasis); // okay
//                }
//                else
//                {
//                    DbgToast("Started by BLE!!");
//                }
//                // external triggering
//                saveMode = 1; //time-basis
//                StartIgnoringDelay();
//                //Start();
//
//            }
//            else if (readMessage.substring(0, 5).equals("SMODE") || readMessage.substring(0, 5).equals("smode")) { //saving_mode
//                if(readMessage.length()>5)
//                {
//                    saveMode = Integer.parseInt(readMessage.substring(5));
//                }
//                else
//                {
//                    saveMode = 0;
//                }
//                DbgToast(String.format("Saving mode...%d", saveMode));
//            }
//            else if (readMessage.substring(0, 5).equals("NOTE_") || readMessage.substring(0, 5).equals("note_")) { //saving_mode
//                if(readMessage.length()>5)
//                {
//                    //noteReceived = readMessage.substring(5, readMessage.length());
//                    noteReceived = readMessage.substring(5);
//                }
//                else
//                {
//                    noteReceived = null;
//                }
//                Dbg.out("Notes: "+ noteReceived);
//                DbgToast(String.format("Notes: "+ noteReceived));
//            }
//            else if (readMessage.substring(0, 5).equals("CLASS") || readMessage.substring(0, 5).equals("class")) {
//                String nclass_received = readMessage.substring(5);
//                int nclass = Integer.parseInt(nclass_received); // Todo : catch exeption !
//                UpdateClass(nclass, true);
//                DbgToast(String.format("Class : %d", nclass));
//            }
//        }
//        //TODO : MEMO 를 받아 처리하는...ok
//        //TODO: savemode ==1 (time-basis) 인경우, ab 처리 안하도록...ok
//
//        else { //short
//
//            if (readMessage.equals("STOP") || readMessage.equals("stop")) {
//
//                Log.d(TAG, "STOP");
//                Stop();
//                DbgToast("정지되었습니다.!");
//            } else if (readMessage.equals("SAVE") || readMessage.equals("save")) {
//          //      SaveData();
//            } else {
//                DbgToast(readMessage);
//            }
//        }
//
//    }
//
//
//    public void onActivityResult(int requestCode, int resultCode, Intent data) {
//        switch (requestCode) {
//            case REQUEST_CONNECT_DEVICE_SECURE:
//                // When DeviceListActivity returns with a device to connect
//                if (resultCode == Activity.RESULT_OK) {
//                    connectDevice(data, true);
//                }
//                break;
//            case REQUEST_CONNECT_DEVICE_INSECURE:
//                // When DeviceListActivity returns with a device to connect
//                if (resultCode == Activity.RESULT_OK) {
//                    connectDevice(data, false);
//                }
//                break;
//            case REQUEST_ENABLE_BT:
//                // When the request to enable Bluetooth returns
//                if (resultCode == Activity.RESULT_OK) {
//                    // Bluetooth is now enabled, so set up a chat session
//                    setupChat();
//                } else {
//                    // User did not enable Bluetooth or an error occurred
//                    Log.d(TAG, "BT not enabled");
//                    DbgToast("BT not enabled");
//
//                }
//        }
//    }
//
//    /**
//     * Establish connection with other device
//     *
//     * @param data   An {@link Intent} with {@link DeviceListActivity#EXTRA_DEVICE_ADDRESS} extra.
//     * @param secure Socket Security type - Secure (true) , Insecure (false)
//     */
//
//    public void connectDevice(Intent data, boolean secure) {
//        // Get the device MAC address
//        Bundle extras = data.getExtras();
//        if (extras == null) {
//            return;
//        }
//        String address = extras.getString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
//        // Get the BluetoothDevice object
//        BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(address);
//        // Attempt to connect to the device
//        mChatService.connect(device, secure);
//    }


    //-- to K-PT server
    public enum Enum_IPC_Message {

        Connect(1),
        ConnectFossil(3),
        DisConnectFossil(4),
        SyncMasterSlaveClockTime(13),
        StartFossilData(14),
        StopFossilData(15),
        Fossill_Trasfer_Acc_SampleData(33),
        Fossill_Trasfer_Gyro_SampleData(333);

        private final int value;

        Enum_IPC_Message(int value) {
            this.value = value;
        }

        public int GetValue() {
            return value;
        }
    }


    private ClientThread m_ClientThread = null;

    private HandlerThread thread;

    public InetAddress serverAddr;
    public Socket clientSocket;
    //서울대학교 위치
    private String serverIPAddr = "147.46.4.59";
    private int portNum = 4545;

    private BufferedReader networkReader;
    private BufferedWriter networkWriter;


    private Looper mServiceLooper;
    private ServiceHandler mServiceHandler;

    private final class ServiceHandler extends Handler {
        public ServiceHandler(Looper looper) {
            super(looper);
        }

        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case 3:
                    Send_Message_to_srv(Enum_IPC_Message.ConnectFossil, "");
                    break;
                case 4:
                    Send_Message_to_srv(Enum_IPC_Message.DisConnectFossil, "");
                    break;
                case 33:
                    Send_Message_to_srv(Enum_IPC_Message.Fossill_Trasfer_Acc_SampleData, msg.obj.toString());
                    break;
                case 333:
                    Send_Message_to_srv(Enum_IPC_Message.Fossill_Trasfer_Gyro_SampleData,msg.obj.toString());
                    break;
                case 13:
                    long tsLong = System.currentTimeMillis();
                    Log.d("ClientThread7777", "Srv : Command_PTP");
                    buf_PTP_sendTime = (new SimpleDateFormat("HHmmssSSS")).format(new Date(tsLong));
                    Send_Message_to_srv(Enum_IPC_Message.SyncMasterSlaveClockTime, buf_PTP_revTime + "," + buf_PTP_revMsg + "," + buf_PTP_sendTime);
                    //   InitSensors();
                case 14:
                    Log.d("ClientThread7777", "Srv : Command_Sensor_Start");
                    //   saveMode = 0;
                    Start();
                    Start = true;
                    break;
                case 15:
                    Log.d("ClientThread7777", "Srv : Command_Sensor_Stop");
                    Stop();
                    Start = false;
                    break;

            }
        }
    }


    class ClientThread extends Thread {

        private boolean stopped = false;
        private String line;

        @Override
        public void run() {


            try {
                InetSocketAddress socketAddress = new InetSocketAddress(InetAddress.getByName(serverIPAddr), portNum);
                clientSocket = new Socket();
                Log.d("ClientThread7777", "주소입력");
                clientSocket.connect(socketAddress, 5000);
                Log.d("Clinet", socketAddress.toString());

                //-----------------------------------------------------------------------------------------


                if (clientSocket.isConnected()) {
                    Log.d("ClientThread7777", "연결성공.");


                    Load_Message_on_Handler(Enum_IPC_Message.ConnectFossil, "");
                    InputStreamReader i = new InputStreamReader(clientSocket.getInputStream());
                    networkReader = new BufferedReader(i);


                    //   Send_Message_to_srv(Enum_IPC_Message.Fossill_Trasfer_Acc_SampleData, AccData);
                    //   Send_Message_to_srv(Enum_IPC_Message.Fossill_Trasfer_Gyro_SampleData, GyroData);

                } else {
                    Log.d("ClientThread7777", "연결실패.");
                }

            } catch (Exception e) {
                e.printStackTrace();
//                receivedText = e.toString();
                Log.d("ClientThread7777", "에러" + e.toString());
            }

            try {
                while (!stopped) {
                    Log.d("ClientThread7777", "222222");


                    line = networkReader.readLine();

                    if (line == null) {
                        break;
                    }

                    Log.d("ClientThread7777", "ㅎㅎㅎ " + line);

                    ///ex [202012071711.999] [Connect] [11] 3
                    String[] splittedData = line.split(",");
                    Log.d("ClientThread7777", splittedData[1]);
                    if (!Validate_Data(splittedData[1]))
                        continue;

                    int int_Command = Integer.parseInt(splittedData[1]);
                    if (int_Command == 13) {
                        buf_PTP_revTime = splittedData[4];
                        PTP_TimeSync();
                    }
                    Log.d("ClientThread7777", "11111");
                    Message msg = mServiceHandler.obtainMessage();
                    msg.what = int_Command;
                    mServiceHandler.sendMessage(msg);


                    Log.d("ClientThread7777", "33333333333333");


                }

                Log.d("ClientThread7777", "4444444444444444444");
            } catch (Exception e) {
                e.printStackTrace();
                Log.d("ClientThread7777", "에aa러" + e.toString());
            }

        }

        private void PTP_TimeSync() {
            i_Current_timestamp_HH_ACC = Integer.parseInt(buf_PTP_revTime.substring(0, 2));
            i_Current_timestamp_MM_ACC = Integer.parseInt(buf_PTP_revTime.substring(2, 4));
            i_Current_timestamp_SS_ACC = Integer.parseInt(buf_PTP_revTime.substring(4, 6));
            i_Current_timestamp_ms_ACC = Integer.parseInt(buf_PTP_revTime.substring(6, 9));

            i_Current_timestamp_HH_GYRO = Integer.parseInt(buf_PTP_revTime.substring(0, 2));
            i_Current_timestamp_MM_GYRO = Integer.parseInt(buf_PTP_revTime.substring(2, 4));
            i_Current_timestamp_SS_GYRO = Integer.parseInt(buf_PTP_revTime.substring(4, 6));
            i_Current_timestamp_ms_GYRO = Integer.parseInt(buf_PTP_revTime.substring(6, 9));

            long tsLong = System.currentTimeMillis();
            buf_PTP_revMsg = (new SimpleDateFormat("HHmmssSSS")).format(new Date(tsLong));
        }

        private boolean Validate_Data(String s) {
            try {
                Integer.parseInt(s);
                return true;
            } catch (NumberFormatException e) {
                return false;
            }
        }

        public void Stop_Thread() {

            Log.d("ClientThread7777", "클라가 종료됨.");

//            m_ClientThread.interrupt();

            Load_Message_on_Handler(Enum_IPC_Message.DisConnectFossil, "");
            //  Load_Message_on_Handler(Enum_IPC_Message.Connect, "");

            stopped = true;
        }
    }

    private void Load_Message_on_Handler(Enum_IPC_Message _enum_ipc_message, String _data) {
        Message msg = mServiceHandler.obtainMessage();
        msg.what = _enum_ipc_message.GetValue();
        Object obj = _data;
        msg.obj = obj;
        mServiceHandler.sendMessage(msg);
    }

    private void Send_Message_direct(Enum_IPC_Message _enum_ipc_message, String _message) {
        try {

            if (clientSocket.isConnected()) {
                String msg = "";
                networkWriter = new BufferedWriter(new OutputStreamWriter(clientSocket.getOutputStream()), 8096);
                msg = _enum_ipc_message.toString() + "," + _enum_ipc_message.GetValue() + "," + _message + ';';
                networkWriter.write(msg);
                networkWriter.flush();
                Log.d("ClientThread7777", "보내기? " + msg);
            }
        } catch (Exception e) {
            e.printStackTrace();
//                receivedText = e.toString();
            Log.d("ClientThread7777", "에러" + e.toString());
        }
    }

    private void Send_Message_Index(String _message, int _index, String _TS_data) {
        try {
            if (clientSocket.isConnected()) {
                String msg = "";

                long tsLong = System.currentTimeMillis();
                String str_time = (new SimpleDateFormat("HHmmssSSS")).format(new Date(tsLong));
                networkWriter = new BufferedWriter(new OutputStreamWriter(clientSocket.getOutputStream()), 8096);
                msg = _message + "," + "SW_Fossil,LSJ," + _TS_data + ';';
                networkWriter.write(msg);
                networkWriter.flush();
                Log.d("ClientThread7777", "보내기 " + msg);
            }
        } catch (Exception e) {
            e.printStackTrace();
//                receivedText = e.toString();
            Log.d("ClientThread7777", "에러" + e.toString());
        }
    }

    public void Send_Message_to_srv(Enum_IPC_Message _IPC_Message, String _TS_data) {
        //String msg = DateTime.Now.ToString("yyyyMMddHHmmss.fff") + "," + _IPC_Message.ToString();

        long tsLong = System.currentTimeMillis();
        String str_time = (new SimpleDateFormat("HHmmssSSS")).format(new Date(tsLong));
        String msg = _IPC_Message.toString() + "," + _IPC_Message.GetValue();
        int msgIndex = _IPC_Message.GetValue();

        Send_Message_Index(msg, msgIndex, _TS_data);
    }
}
