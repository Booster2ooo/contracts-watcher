(() => {
	/** The selector for server links */
	const serversSelector = '[data-list-id="guildsnav"] > div[class^="scroller"] > div:nth-child(3) > div[class^="listItem"]';
	/** The selector for channel links */
	const channelsSelector = '#channels ul a[href^="/channels/"]';
	/** The selector for the chat container */
	const chatSelector = '[data-list-id="chat-messages"]';
	/** The selector for the message content */
	const messageContentSelector = '[id^="message-content"]:not([class^="repliedText"])';
	/** The selector for message accessories */
	const messageAccessoriesSelector = '[id^="message-accessories"]';

	/**
	 * Parses messages from the provides nodes
	 * @param {NodeList} nodes The nodes to parse the messages from
	 */
	async function parseMessages(nodes) {
		const messages = Array.from(nodes).map(node => node.querySelector(messageContentSelector)).filter(node => node);
		const accessories = Array.from(nodes).map(node => node.querySelector(messageAccessoriesSelector)).filter(node => node);
		// regexp messages textContent
		//console.log(messages.map(msg => msg.textContent));
		//console.log(accessories.map(msg => msg.textContent));
		for await (const msg of messages) {
			debugger;
			await fetch('%SERVER%/Contracts',
				{
					method: 'POST',
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify(msg.textContent)
				}
			);
		}
	};

	/**
	 * The chat mutations callback
	 * @param {MutationRecord[]} mutationsList The list of mutations
	 * @param {MutationObserver} observer The mutation observer
	 */
	async function chatMutationCallback(mutationsList, observer) {
		console.log('Processing mutations', mutationsList);
		for (const mutation of mutationsList) {
			if (mutation.type === "childList" && mutation.addedNodes.length) {
				await parseMessages(mutation.addedNodes);
			}
		}
	};

	let observer;
	let channels = [];
	/**
	 * Parses and observes the chat messages
	 * @param {MouseEvent} event The mouse event
	 */
	function refreshObserver(event) {
		setTimeout(async () => {
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
		}, 500);
	};

	const servers = document.querySelectorAll(serversSelector);
	servers.forEach(server => {
		server.addEventListener('click', refreshObserver);
	});
	refreshObserver(null);
})();
