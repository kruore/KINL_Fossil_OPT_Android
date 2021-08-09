package com.example.KINLAB_OPT_Fossil_Provider;

import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.util.Enumeration;

public class DDUtilNetwork {
    public static String getIpAddress(boolean useInteralIP) {
        String ip = "";
        try {
            Enumeration<NetworkInterface> enumNetworkInterfaces = NetworkInterface
                    .getNetworkInterfaces();
            while (enumNetworkInterfaces.hasMoreElements()) {
                NetworkInterface networkInterface = enumNetworkInterfaces
                        .nextElement();
                Enumeration<InetAddress> enumInetAddress = networkInterface
                        .getInetAddresses();
                while (enumInetAddress.hasMoreElements()) {
                    InetAddress inetAddress = enumInetAddress.nextElement();

                    if (inetAddress.isSiteLocalAddress()) {

                        if(useInteralIP) {
                           if (inetAddress.getHostAddress().substring(0, 8).compareTo("192.168.")==0 ||
                                inetAddress.getHostAddress().substring(0, 7).compareTo("172.30.")==0) {
                                ip +=  inetAddress.getHostAddress()+ "\n";
                            }
                        }else
                        {
                            ip +=  inetAddress.getHostAddress()+ "\n";
                        }

                    }
                }
            }

        } catch (SocketException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            ip += "Something Wrong! " + e.toString() + "\n";
        }

        return ip;
    }
}
