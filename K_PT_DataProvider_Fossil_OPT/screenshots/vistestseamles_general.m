clc; clear; close all;

%dirn = 'test02'
%dirn = '20200127_fossil_C18_2';
%dirn = '20200127_fossil_C33_';
%dirn = '20200127_fossil_C53_';
dirn = '20200219_fossil_C21_freestyle';
%dirn = '20200303_fossil_C01_walking_flat';

flist = getAllFilesWithFormat(dirn, '*_sync.txt');

data = [];
nfiles = length(flist);

for i=1:nfiles
    fn = flist{i};
    %disp(fn)
    A = load(fn);
    data{i} = A;
end
%% 
disp(sprintf('Loaded %d files', nfiles));

%% TODO
% n개씩 1줄 그리기
ndisp_per_row = 3;

nrows = nfiles / ndisp_per_row;

%%

for k=1:nrows
    figure(100+k)
    set(gcf,'Position',[10 + 20* k, 50, 1000, 900]);
    subplot(6, 1, 1);    
    
    fn1 = flist{1+ (k-1)*ndisp_per_row};
    fn2 = flist{2+ (k-1)*ndisp_per_row};
    fn3 = flist{3+ (k-1)*ndisp_per_row};
    
    [aa,matches] =strsplit(fn1 ,'\\');
    [aa2,matches] =strsplit(aa{2} ,'_');
    fn1a = strcat(aa2{3},'_',aa2{4},'_',aa2{5}); %num2str(str2num(aa2{5}))

    [aa,matches] =strsplit(fn2 ,'\\');
    [aa2,matches] =strsplit(aa{2} ,'_');
    fn2a = strcat(aa2{3},'_',aa2{4},'_',aa2{5}); %num2str(str2num(aa2{5}))

    [aa,matches] =strsplit(fn3 ,'\\');
    [aa2,matches] =strsplit(aa{2} ,'_');
    fn3a = strcat(aa2{3},'_',aa2{4},'_',aa2{5}); %num2str(str2num(aa2{5}))
    
    disp(fn1a);
    A = data{1+ (k-1)*ndisp_per_row};
    B = data{2+ (k-1)*ndisp_per_row};
    C = data{3+ (k-1)*ndisp_per_row};
    
    dt1 = mean(diff(A(:,1)));
    dt2 = mean(diff(B(:,1)));
    dt3 = mean(diff(C(:,1)));
    
    t1 = A(:,1) - A(1,1);
    %t1 = t1 / 1000.0;

    t2x = B(:,1) - A(1,1);
    t2 = B(:,1) - B(1,1)+t1(length(t1));
    %t2 = t2 / 1000.0;

    t3x = C(:,1) - A(1,1);
    t3 = C(:,1) - C(1,1)+t2(length(t2));
    %t3 = t3 / 1000.0;
    T = [t1; t2; t3];
    Tx =[ t1; t2x; t3x];
    % B = A;%A(1:100, :);

    %x1 = linspace(1, size(A, 1), size(A, 1));
    %x2 = linspace(size(A, 1)+1, size(A, 1)+ size(A, 1), size(A, 1));

    D = [A; B; C];
    %disp(fn1);
    %sprintf('%s ----- %s', fn1, fn1a)
    %disp(size(D))
    
    for i=1:6
        subplot(6,1,i)
        plot(T, D(:,3+i-1))
        hold on;
        plot(t2, B(:,3+i-1));
        
        if i==1
            %ttl1 = strcat(fn1a, '   ', fn2a, '   ', fn3a)
            ttl1 = sprintf('%s    %s    %s', fn1a, fn2a, fn3a);
            %ttl1 = regexprep(ttl1, '\_', '\\\_');
            title(ttl1, 'Interpreter', 'none');
        end
    end
    pause
end

figure; plot(Tx)
