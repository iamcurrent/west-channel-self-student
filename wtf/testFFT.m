xx = data(5,:);
xx = xx-mean(xx);
[a,L] = size(xx);
window = 80;
fs = 4000;
unit =window/8/fs;
r = zeros(floor(8*L/window)+1,1);
t =0:window/8/fs:L/fs;
j =1;
xx = lowpass(xx,100,4000);
maxx = max(xx);
for i=1:window/8:L-window
    signal = xx(i:i+window);
    %signal = signal - mean(signal);
    Y= fft(signal);
    P2 = abs(Y/window);
    figure(1);
    P22 = P2(2:window/4);
    
    [a,s] = max(P22);
    %r(j)= s;
    if s ~= 2
        r(j) = 1;
    else 
        m1 = P22(s);
        P22(s) = 0;
        if m1 <maxx/100 || m1 / max(P22) < 1.05
          %  r(j) = 1;
        end
    end
   
    j=j+1;
end
figure(1)

%r(r==0) = 100;
plot(t,r','-*')