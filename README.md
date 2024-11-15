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

     checks................: 100.00% 825 out of 825
     data_received.........: 72 MB   220 kB/s
     data_sent.............: 532 kB  1.6 kB/s
     iteration_duration....: avg=30.08s min=45.17ms med=28.52s max=1m2s     p(90)=51.35s p(95)=57.17s
     iterations............: 825     2.537022/s
     vus...................: 1       min=1          max=100
     vus_max...............: 100     min=100        max=100
     ws_connecting.........: avg=2.23ms min=1.47ms  med=1.83ms max=108.05ms p(90)=2.47ms p(95)=3.03ms
     ws_msgs_received......: 638491  1963.473684/s
     ws_msgs_sent..........: 6956    21.39094/s
     ws_session_duration...: avg=30.08s min=45ms    med=28.52s max=1m2s     p(90)=51.35s p(95)=57.17s
     ws_sessions...........: 845     2.598526/s

                                                                                                                     
running (5m25.2s), 000/100 VUs, 825 complete and 20 interrupted iterations                                           
default ✓ [======================================] 000/100 VUs  5m0s  
```
