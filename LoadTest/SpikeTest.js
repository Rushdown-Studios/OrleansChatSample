import { randomString, randomIntBetween, uuidv4 } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import ws from 'k6/ws';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 1000 }, // ramp up to 100 VUs
    { duration: '3m', target: 1000 }, // stay at 100 VUs for 3 minutes
    { duration: '1m', target: 0 },   // ramp down to 0 VUs
  ],
};

const sessionDuration = randomIntBetween(10000, 60000);

export default function () {
    const guid = uuidv4();
    const url = `ws://localhost:5000/ws?userId=${guid}`;

  const res = ws.connect(url, function (socket) {
    socket.on('open', function open() {
      console.log(`VU ${__VU}: connected`);

      socket.send(JSON.stringify({ event: 'SET_NAME', new_name: `Croc ${__VU}` }));

      socket.setInterval(function timeout() {
        socket.send(JSON.stringify({ event: 'SAY', message: `I'm saying ${randomString(5)}` }));
      }, randomIntBetween(2000, 8000)); // say something every 2-8seconds
    });

    socket.on('ping', function () {
      console.log('PING!');
    });

    socket.on('pong', function () {
      console.log('PONG!');
    });

    socket.on('close', function () {
      console.log(`VU ${__VU}: disconnected`);
    });

    socket.on('message', function (message) {
      const msg = JSON.parse(message);
      console.log(`VU ${__VU} received message: ${msg.message}`);
    });

    socket.setTimeout(function () {
      console.log(`VU ${__VU}: ${sessionDuration}ms passed, leaving the chat`);
      socket.send(JSON.stringify({ event: 'LEAVE' }));
    }, sessionDuration);

    socket.setTimeout(function () {
      console.log(`Closing the socket forcefully 3s after graceful LEAVE`);
      socket.close();
    }, sessionDuration + 3000);
  });

  check(res, { 'Connected successfully': (r) => r && r.status === 101 });
}