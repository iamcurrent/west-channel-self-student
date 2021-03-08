function [peaks,m,a6] = detectPeaks(x,minDistance,MinHeight) 

    [c,l] = wavedec(x,6,'db8');
    a5 = wrcoef('a',c,l,'db8');
    m = mean(a5);
    a6 = a5-m;
    L = length(x);
    if minDistance > L/2
        minDistance = L/2;
    end
    [~,plocsa,~,~]=findpeaks(a6, 'MinPeakDistance',minDistance,'MinPeakHeight',MinHeight);
    [~,nlocsa,~,~]=findpeaks(-a6, 'MinPeakDistance',minDistance,'MinPeakHeight',MinHeight);
    
    plocs = double(plocsa);
    nlocs = double(nlocsa);
    peaks =sort( [plocs,nlocs]);
    if(length(peaks) == 1)
        peaks(2) = 0;
    end
   
    %signalAfterFilter = lowpass(x,sampleHz,fs);
    %plot(a6)
end