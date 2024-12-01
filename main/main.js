const { app, BrowserWindow } = require('electron');
const path = require('node:path');
const { launchDiscordAndInjectScript } = require('./discord-manager.js');
const { createWebSocketServer } = require('./websocket-server.js');

createWebSocketServer();
/*await*/ launchDiscordAndInjectScript(); // fire & forget

const createWindow = () => {
  const win = new BrowserWindow({
    width: 800,
    height: 600,
    transparent: true,
    alwaysOnTop: true,
    frame: false,
    autoHideMenuBar: true,
  });

  win.loadFile(`${path.resolve(__dirname, '../renderer/', 'index.html')}`);
  //win.webContents.openDevTools();
};

app.whenReady()
  .then(() => {
    createWindow();

    app.on('activate', () => {
      if (BrowserWindow.getAllWindows().length === 0) createWindow();
    })
  })
  .catch(err => console.error(err));

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});