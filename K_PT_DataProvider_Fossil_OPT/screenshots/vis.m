clc; clear; close all;

%A = load('data/c0_lift/out_00410_20171227_105105_875421.txt');
%A = load('data/c0_lift/out_00411_20171227_105115_798165.txt');

if ~exist('_outmatlab', 'dir')
	 mkdir('_outmatlab');
end

%A = load('c1_walk/bfde1_user_00021_C120191115_215804_sync.txt');

%A = load('c1_walk/bfde1_user_00022_C120191115_215443_sync.txt');
%A = load('c2_staircase_up/bfde1_user_00008_C220191115_220930_sync.txt');
%A = load('c3_jump/bfde1_user_00009_C320191115_215040_acc.txt');
%A = load('test/bfde1_user_00000_C0_20191117_154452_sync.txt');
A = load('test/bfde1_user_00001_C0_20191117_154507_sync.txt');


B = A;%A(1:100, :);
t = B(:,1) - B(1,1);
t = t / 1000.0;

%{
String str1 = String.format("%d\t%d\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.5f\t%.5f\n",
                            tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j], lng[i], lat[i]);

                    fo.write(str1.getBytes());
%}     
figure;
subplot(6,1,1)
plot(t, B(:,3))

subplot(6,1,2)
plot( t, B(:,4))

subplot(6,1,3)
plot(t, B(:,5))

subplot(6,1,4)
plot(t, B(:,6))

subplot(6,1,5)
plot( t, B(:,7))

subplot(6,1,6)
plot(t, B(:,8))

