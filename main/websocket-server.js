const WebSocket = require('ws');
const { WebSocketServer } = require('ws');

module.exports.createWebSocketServer = () => {
  const wss = new WebSocketServer({ port: 8080 });

  wss.on('connection', (ws) => {
    ws.isAlive = true;
    ws.on('error', console.error);
    ws.on('pong', () => {
      ws.isAlive = true;
    });
    ws.on('message', (data, binary) => {
      wss.clients.forEach(client => {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
          client.send(data, { binary });
        }
      });
    });
  });
  
  const pingInterval = setInterval(() => {
    wss.clients.forEach(ws => {
      if (ws.isAlive === false) return ws.terminate();
      ws.isAlive = false;
      ws.ping();
    });
  }, 30000);
  
  wss.on('close', () => clearInterval(pingInterval));
  return wss;
}