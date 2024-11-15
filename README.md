# Rushdown Rpc

## Run and deploy on local environment

### Create / Start

```sh
docker-compose up -d
```

### down

```sh
docker-compose down
```

## Spike Test

```sh
k6 run LoadTest/SpikeTest.js
```

```txt
     ✓ Connected successfully

     checks................: 100.00% 24258 out of 24258
     data_received.........: 269 MB  898 kB/s
     data_sent.............: 9.8 MB  33 kB/s
     iteration_duration....: avg=10.06s min=3.32ms med=380.17ms max=1m3s    p(90)=37.46s  p(95)=48.01s
     iterations............: 24258   80.862527/s
     vus...................: 2       min=2              max=1000
     vus_max...............: 1000    min=1000           max=1000
     ws_connecting.........: avg=4.63ms min=1.19ms med=2.42ms   max=94.05ms p(90)=10.53ms p(95)=15.51ms
     ws_msgs_received......: 2229850 7433.065664/s
     ws_msgs_sent..........: 81912   273.048534/s
     ws_session_duration...: avg=10.06s min=3.24ms med=380.07ms max=1m3s    p(90)=37.46s  p(95)=48.01s
     ws_sessions...........: 24306   81.022532/s
                                                                                                                   
running (5m25.2s), 000/1000 VUs, 24306 complete and 20 interrupted iterations                                           
default ✓ [======================================] 000/1000 VUs  5m0s  
```
