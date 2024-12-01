const { spawn } = require('node:child_process');
const { readFile } = require('node:fs/promises');
const path = require('node:path');
const WebSocket = require('ws');
const { 
  resolveEnvironmentVariables,
  sleep
} = require('./utils.js');

const spawnAsync = async (...args) => new Promise((resolve, reject) => {
  const handle = spawn(...args);
  handle.stderr.on('data', reject);
  handle.on('close', code => {
    if (code === 0) resolve();
    else reject(code);
  });
});

module.exports.launchDiscordAndInjectScript = async () => {
  console.log('DiscordLauncher starting...');

  //console.trace('Shutting down existing Discord instances.');
  try {
    await spawnAsync(
      'taskkill',
      [
        '/F',
        '/IM',
        'Discord.exe',
        '/T'
      ],
      { 
        shell: true,
        detached: true,
        windowsHide: true
      }
    );
  }
  catch{}
  
  //console.trace('Staring new Discord instance with remote debugger.');
  await spawnAsync(
    path.resolve(resolveEnvironmentVariables('%LOCALAPPDATA%\\Discord'), 'Update.exe'),
    [
      '--processStart',
      'Discord.exe',
      '--process-start-args',
      '--remote-debugging-port=16661'
    ],
    { 
      shell: false,
      detached: true,
      windowsHide: true
   }
  );
  await sleep(3000);
  
  //console.trace('Building injection payload.');
  const scriptPath = path.resolve(__dirname, '../resources/injected-script.js');
  const script = await readFile(scriptPath, { encoding: 'utf-8' });
  const command = {
    id: 1234,
    method: 'Runtime.evaluate',
    params: {
        expression: script,
        objectGroup: 'contracts-watcher',
        silent: true,
        returnByValue: false,
        userGesture: true,
        // doNotPauseOnExceptionsAndMuteConsole: false,
        // generatePreview: false,
        // includeCommandLineAPI: true,
    }
  };
  
  //console.trace('Connecting to remote debugger & injecting payload');
  let ex;
  let retries = 0;
  let session;
  do {
    try {
      ex = null;
      const response = await fetch('http://localhost:16661/json');
      const sessions = await response.json();
      session = sessions.find(s => s.url === 'https://discord.com/channels/@me');
      if (!session) {
        throw new Error('Unable to find a session for "friends"');
      }
    }
    catch(innerEx) {
      ex = innerEx;
      await sleep(1000 * retries);
      retries++;
    }
  }
  while(ex || retries >= 10);
  ex = null;
  retries = 0;
  const sendAndWait = async (ws) => new Promise(async (resolve, reject) => {
    ws.on('open', () => {
      ws.send(JSON.stringify(command), (err) => {
        ws.close();
        if (err) throw new Error(err);
        resolve();
      });
    });
    await sleep(1000);
    ws.close();
    reject();
  });
  do {
    try {
      ex = null;
      const ws = new WebSocket(session.webSocketDebuggerUrl);
      await sendAndWait(ws);      
    }
    catch(innerEx) {
      ex = innerEx;
      await sleep(1000 * retries);
      retries++;
    }
  }
  while(ex || retries >= 10);

  console.log('DiscordLauncher completed.');
}