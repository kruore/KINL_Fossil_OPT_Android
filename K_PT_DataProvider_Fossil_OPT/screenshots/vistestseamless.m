clc; clear; close all;

flist = getAllFilesWithFormat('test01', '*_sync.txt');
data = [];
nfiles = length(flist);

for i=1:nfiles
    fn = flist{i};
    disp(fn)
    A = load(fn);
    data{i} = A;
end


%%
% if ~exist('_outmatlab', 'dir')
% 	 mkdir('_outmatlab');
% end

A = data{1};
B = data{2};
C = data{3};

t = A(:,1) - A(1,1);
t = t / 1000.0;

% B = A;%A(1:100, :);

x1 = linspace(1, size(A, 1), size(A, 1));
x2 = linspace(size(A, 1)+1, size(A, 1)+ size(A, 1), size(A, 1));


%{
String str1 = String.format("%d\t%d\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.5f\t%.5f\n",
                            tacc[j], tgyro[j], accx[j], accy[j], accz[j], px[j], py[j], pz[j], lng[i], lat[i]);

                    fo.write(str1.getBytes());
%}     
figure;
subplot(6,1,1)
plot(x1, A(:,3)); hold on;
plot(x2, B(:,3))

subplot(6,1,2)
plot(x1, A(:,4)); hold on;
plot(x2, B(:,4))

subplot(6,1,3)
plot(x1, A(:,5)); hold on;
plot(x2, B(:,5))

subplot(6,1,4)
plot(x1, A(:,6)); hold on;
plot(x2, B(:,6))

subplot(6,1,5)
plot( x1, A(:,7)); hold on;
plot(x2, B(:,7))

subplot(6,1,6)
plot(x1, A(:,8)); hold on;
plot(x2, B(:,8))
%%
% concat
B = [A; B; C];
disp(size(B))
figure;

subplot(6,1,1)
plot(B(:,3))
title('Concat')

subplot(6,1,2)
plot(B(:,4))

subplot(6,1,3)
plot(B(:,5))

subplot(6,1,4)
plot(B(:,6))

subplot(6,1,5)
plot(B(:,7))

subplot(6,1,6)
plot(B(:,8))