﻿(async () => {
  /** The selector for server links */
  const serversSelector = '[data-list-id="guildsnav"] div[class^="scroller"] > div:nth-child(3) > div[class^="listItem"]';
  /** The selector for channel links */
  const channelsSelector = '#channels ul a[href^="/channels/"]';
  /** The selector for the chat container */
  const chatSelector = '[data-list-id="chat-messages"]';
  /** The selector for the message content */
  const messageContentSelector = '[id^="message-content"]:not([class^="repliedText"])';
  /** The selector for message accessories */
  const messageAccessoriesSelector = '[id^="message-accessories"]';

  const ws = new WebSocket('ws://127.0.0.1:8080');
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
    console.log(data);
  });

  /**
  * Parses messages from the provides nodes
  * @param {NodeList} nodes The nodes to parse the messages from
  */
  const parseMessages = async (nodes) => {
    const messages = Array.from(nodes).map(node => node.querySelector(messageContentSelector)).filter(node => node);
    const accessories = Array.from(nodes).map(node => node.querySelector(messageAccessoriesSelector)).filter(node => node);
    // regexp messages textContent
    //console.log(messages.map(msg => msg.textContent));
    //console.log(accessories.map(msg => msg.textContent));
    for(const msg of messages) {
      //debugger;
      ws.send(JSON.stringify({ type: 'new-contract', contractId: msg.textContent }));
    }
  };

  /**
  * The chat mutations callback
  * @param {MutationRecord[]} mutationsList The list of mutations
  * @param {MutationObserver} observer The mutation observer
  */
  const chatMutationCallback = async (mutationsList, observer) => {
    console.log('Processing mutations', mutationsList);
    for (const mutation of mutationsList) {
      if (mutation.type === "childList" && mutation.addedNodes.length) {
        await parseMessages(mutation.addedNodes);
      }
    }
  };

  /**
  * A function used to wait for an amount of milliseconds
  * @param {any} ms
  * @returns
  */
  const sleep = async (ms) => new Promise(resolve => setTimeout(resolve, ms));

  let observer;
  let channels = [];
  /**
  * Parses and observes the chat messages
  * @param {MouseEvent} e The mouse event
  */
  const refreshObserver = async (e) => {
    await sleep(500);
    channels.forEach(channel => channel.removeEventListener('click', refreshObserver));
    channels = document.querySelectorAll(channelsSelector);
    channels.forEach(channel => channel.addEventListener('click', refreshObserver));
    const chat = document.querySelector(chatSelector);
    if (chat) {
      await parseMessages(chat.querySelectorAll('li'));
      if (observer) {
        observer.disconnect();
        observer = null;
      }
      observer = new MutationObserver(chatMutationCallback);
      observer.observe(chat, { childList: true });
    }
  };

  const globalObserver = new MutationObserver(() => {
    const servers = document.querySelectorAll(serversSelector);
    if (servers.length) {
      servers.forEach(server => {
        server.addEventListener('click', async (e) => await refreshObserver(e));
      });
      globalObserver.disconnect();
    }
  });
  globalObserver.observe(document.body, { childList: true, subtree: true });
  await refreshObserver(null);
})();