const contractsContainer = document.getElementById('contracts');
const ws = new WebSocket('ws://localhost:8080');

ws.addEventListener('open', () => {
  console.log('ws connection opened');
});
ws.addEventListener('close', (e) => {
  console.log('ws connection closed');
});
ws.addEventListener('error', (e) => {
  console.log('ws connection error', e);
});
ws.addEventListener('message', (e) => {
  const { data } = e;
  let message = data;
  try {
    message = JSON.parse(data);
  }
  catch {}
  console.log('ws message', message, e);
  if (message?.type === 'new-contract') {
    const li = document.createElement('li');
    li.textContent = message.contractId;
    contractsContainer.prepend(li);
  }
});