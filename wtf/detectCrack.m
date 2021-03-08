function [cracks,signalAfterFilter] = detectCrack(signal,fs,sampleHz,windowMulitPara,tolerance)
%UNTITLED4 此处显示有关此函数的摘要
%   此处显示详细说明
    me = mean(signal);
    signal = signal-mean(signal);
    [~,L] = size(signal);
    window = double(windowMulitPara*fs/sampleHz);
    cracks = zeros(1,floor(8*L/window)+1,1);
    j =1;
    signalAfterFilter = lowpass(signal,sampleHz,fs);
    maxValue = max(signalAfterFilter);
    for i=1:window/8:L-window
        signalWindow = signalAfterFilter(i:i+window);
        Y= fft(signalWindow);
        P2 = abs(Y)/window;
        P22 = P2(2:window/4);
        [~,s] = max(P22);
        if s ~= windowMulitPara
            cracks(j) = 1;
        else 
            m1 = P22(s);
            P22(s) = 0;
            if m1 <maxValue/50 || m1 / max(P22) < tolerance
               cracks(j) = 1;
            end
        end

        j=j+1;
    end
    signalAfterFilter = signalAfterFilter+me;
end

