% 
% L=80;
% start = 13500;
% m = xx-mean(xx);
% Y= fft(m(start:start+L));
% P2 = abs(Y/L);
% P1 = P2(1:L/2+1);
% P1(2:end-1) = 2*P1(2:end-1);
% f = 4000*(0:(L/2))/L;
% figure(1);
% plot(f,P1) 
% title('Single-Sided Amplitude Spectrum of X(t)')
% xlabel('f (Hz)')
% ylabel('|P1(f)|')
% 
% [r, d] = detectCrack(data(4,:),4000,100,4,1.05);
% plot(r,'-*')

x = data(9,:);
%x = abs(x- mean(x));

findpeaks(x, 'MinPeakDistance',20,'MinPeakHeight',0.015)

%[plocs,m,a6] = detectPeaks(data(14,:),5000,0.01);