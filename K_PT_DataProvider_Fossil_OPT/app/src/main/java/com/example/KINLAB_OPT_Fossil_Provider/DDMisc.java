package com.example.KINLAB_OPT_Fossil_Provider;

// TODO : http://stackoverflow.com/questions/462297/how-to-use-classt-in-java


public class DDMisc {

	
	public static void ShiftWithNewf(float[] arr, float n_val, int sz)
	{
		int i;
		for(i=0; i< sz-1; i++)
		{
			arr[sz-1-i] = arr[sz-2-i];
			
		}
	
		arr[0] = n_val;
	
	}
	
	

	public static void ShiftWithNewd(int[] arr, int n_val, int sz)
	{
		int i;
		for(i=0; i< sz-1; i++)
		{
			arr[sz-1-i] = arr[sz-2-i];
			
		}
	
		arr[0] = n_val;
	
	}
	

	public static void ShiftWithNewl(long[] arr, long n_val, int sz)
	{
		int i;
		for(i=0; i< sz-1; i++)
		{
			arr[sz-1-i] = arr[sz-2-i];
			
		}
	
		arr[0] = n_val;
	
	}
	
	
	
	
}
